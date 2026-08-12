using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for UPITransferService
/// </summary>
public class UPITransferService : IUPITransfer
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    ICommissionService commissionService;
    ICommonService commonService;
    private static string ServiceID = "20";
    IPayoutService payoutService;

    public UPITransferService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commissionService = new CommissionService();
        commonService = new CommonService();
        payoutService = new PayoutService();
    }

    public async Task<string> GETUPIIDName(string UPIID)
    {
        return await cls.select_data_scalar_stringAsync("select top 1 name from UPITransfer where UPIID='" + UPIID + "' and Name<>''");
    }

    public async Task<string> UPITransfer(UPITransferRequestST uPITransferRequestST, string MemberID, string Source)
    {
        // Check service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(uPITransferRequestST.ServiceID.ToString());
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        // Retrieve member permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        }

        // Validate amount
        if (uPITransferRequestST.Amount <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Enter a valid amount");
        }

        // Retrieve member balance
        decimal MyBalance = 0;

        MyBalance = _memberAllowDetails.AEPSBalance;

        string agentID = await wallet.GetOrderID();


        // Step 5: Get surcharge and commission details
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
            _memberAllowDetails.PackgeID.ToString(),
            uPITransferRequestST.Amount,
            serviceData.APIID.ToString(),
            uPITransferRequestST.ServiceID.ToString(), "UPIT"
        );

        if (!IsValidSlab(item, uPITransferRequestST.Amount))
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");



        decimal surcharge = item.surcharge;
        decimal GSTCalculation = 0;
        GSTCalculation = (item.IsGst == false ? 0 : (surcharge * item.Gst) / 100);

        decimal TotalAmount = surcharge + uPITransferRequestST.Amount + GSTCalculation;

        if (MyBalance < TotalAmount)
        {
            return ErrorShow.ErrorMessageAPI("Insufficient Balance");
        }

        // Create narration and description
        string Narration = $"{uPITransferRequestST.UPIID}/{agentID}/{uPITransferRequestST.Name}";
        string Dec = Narration;

        // Execute wallet transaction
        DataTable dtval;

        dtval = await ExecuteAEPSWalletTransaction(_memberAllowDetails.LoginID, uPITransferRequestST.Amount, Narration, Dec, agentID, surcharge, GSTCalculation);

        if (dtval.Rows.Count == 0 || dtval.Rows[0]["msrno"].ToString() != "1")
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }

        int val = await cls.update_dataAsync($"AddEditUPITransfer {_memberAllowDetails.Msrno}, {uPITransferRequestST.Amount}, {surcharge}, '{uPITransferRequestST.UPIID}', '{uPITransferRequestST.Name}', 'Pending', 'UPI', '', '{agentID}', '', '', '', {GSTCalculation}, {serviceData.APIID}, '{Source}', '{uPITransferRequestST.ServiceID.ToString()}', '{item.OpID}','{MyBalance}',{(uPITransferRequestST.ServiceID == 1076 ? 1 : 0)}");
        if (val <= 0)
        {
            await ProcessRefundMain(_memberAllowDetails, agentID, Narration, uPITransferRequestST.Amount, surcharge, GSTCalculation, "", uPITransferRequestST.ServiceID);
            return ErrorShow.ErrorMessageAPI("Try After Sometime");
        }

        // Handle UPI transfer
        uPITransferRequestST.OrderID = agentID;
        PayoutCommonResponse payoutCommonResponse;

        if (serviceData.APIID == 1)
        {
            payoutCommonResponse = await payoutService.FundTransferSoniTechnoUPI(uPITransferRequestST);
        }
        //else if (serviceData.APIID == 1022)
        //{
        //    payoutCommonResponse = await payoutService.FundTransferAeronpayUPI(uPITransferRequestST);
        //}
        //else if (serviceData.APIID == 1033)
        //{
        //    payoutCommonResponse = await payoutService.FundTransferBetaUPI(uPITransferRequestST);
        //}
        else
        {
            return ErrorShow.ErrorMessageAPI("API Not Allow For UPITransfer");
        }

        try
        {
            await commonService.SaveLogs(new LogsRequestModel() { ServiceID = uPITransferRequestST.ServiceID, APIID = serviceData.APIID, TransID = agentID, RequestData = payoutCommonResponse.Request, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
        }
        catch { };
        return await HandleTransferResponseAEPSWallet(_memberAllowDetails, payoutCommonResponse, agentID, Narration, uPITransferRequestST.Amount, surcharge, GSTCalculation);

    }

    // Helper methods for modularization and readability


    private bool IsValidSlab(SlabModel slabDetails, decimal amount)
    {
        return slabDetails.startVal <= amount &&
               slabDetails.endVal >= amount &&
               slabDetails.startVal > 0 &&
               slabDetails.endVal > 0;
    }

    private async Task<DataTable> ExecuteWalletTransaction(string loginID, decimal amount, string narration, string description, string agentID, decimal surcharge, decimal gst)
    {
        return await cls.select_data_dtAsync($"exec PROC_EWALLETTRANSACTION '{loginID}','{amount}','DR','{narration}','{description}',{UPITransferService.ServiceID},'{agentID}',0,0,0,'{surcharge}','{gst}'");
    }

    private async Task<DataTable> ExecuteAEPSWalletTransaction(string loginID, decimal amount, string narration, string description, string agentID, decimal surcharge, decimal gst)
    {
        //return await cls.select_data_dtAsync($"exec PROC_EWALLETTRANSACTION '{loginID}','{amount}','DR','{narration}','{description}',{UPITransferService.ServiceID},'{agentID}',0,0,0,'{surcharge}','{gst}'");
        return await cls.select_data_dtAsync($"exec PROC_AEPSWALLETTRANSACTION '{loginID}','{amount}','DR','{narration}','{description}',{UPITransferService.ServiceID},'{agentID}',0,0,0,'{surcharge}','{gst}'");
    }

    private async Task<string> HandleTransferResponse(MemberAllowData _memberAllowDetails, PayoutCommonResponse payoutCommonResponse, string agentID, string narration, decimal amount, decimal surcharge, decimal gst)
    {
        if (payoutCommonResponse.Status.ToUpper() == "FAILED")
        {
            await cls.update_dataAsync($"Update UPITransfer set Status='Refund',Message='{payoutCommonResponse.Message}',Response='Error during transfer' where OrderID='{agentID}'");
            return await ProcessRefund(_memberAllowDetails, agentID, narration, amount, surcharge, gst, payoutCommonResponse.Message);
        }

        string status = payoutCommonResponse.Status;
        await cls.update_dataAsync($"Update UPITransfer set Status='{status}',VendorID='{payoutCommonResponse.VendorID}',Response='',Message='{payoutCommonResponse.Message}',RRN='{payoutCommonResponse.RRN}' where OrderID='{agentID}'");

        if (status.ToUpper() == "SUCCESS")
        {
            await commissionService.GiveUPITransferCommission(agentID);
        }

        var returnData = new
        {
            agentID,
            Message = "Transaction Process",
            status
        };
        return ErrorShow.SuccessArrayAPI(Newtonsoft.Json.JsonConvert.SerializeObject(returnData));
    }

    private async Task<string> HandleTransferResponseAEPSWallet(MemberAllowData _memberAllowDetails, PayoutCommonResponse payoutCommonResponse, string agentID, string narration, decimal amount, decimal surcharge, decimal gst)
    {
        if (payoutCommonResponse.Status.ToUpper() == "FAILED")
        {
            await cls.update_dataAsync($"Update UPITransfer set Status='Refund',Message='{payoutCommonResponse.Message}',Response='Error during transfer' where OrderID='{agentID}'");
            return await ProcessRefundMain(_memberAllowDetails, agentID, narration, amount, surcharge, gst, payoutCommonResponse.Message, 1076);
        }

        string status = payoutCommonResponse.Status;
        await cls.update_dataAsync($"Update UPITransfer set Status='{status}',VendorID='{payoutCommonResponse.VendorID}',Response='',Message='{payoutCommonResponse.Message}',RRN='{payoutCommonResponse.RRN}' where OrderID='{agentID}'");


        if (status.ToUpper() == "SUCCESS")
        {
            await commissionService.GiveUPITransferCommission(agentID);
        }

        var returnData = new
        {
            agentID,
            Message = "Transaction Process",
            status
        };
        return ErrorShow.SuccessArrayAPI(Newtonsoft.Json.JsonConvert.SerializeObject(returnData));
    }


    private async Task<string> ProcessRefund(MemberAllowData _memberAllowDetails, string agentID, string narration, decimal amount, decimal surcharge, decimal gst, string errorMessage)
    {
        var dtval = await cls.select_data_dtAsync($"Exec [PROC_EWALLETTRANSACTION] '{_memberAllowDetails.LoginID}',{amount}, 'CR','Refund - {narration}','','{UPITransferService.ServiceID}','{agentID}',0,0,0,'{surcharge}','{gst}'");

        if (dtval.Rows.Count > 0)
        {
            return ErrorShow.ErrorMessageAPI(errorMessage);
        }

        return ErrorShow.ErrorMessageAPI($"Transaction Failed due to {errorMessage}, But Amount Not Refunded in Wallet, Contact Admin");
    }

    private async Task<string> ProcessRefundMain(MemberAllowData _memberAllowDetails, string agentID, string narration, decimal amount, decimal surcharge, decimal gst, string errorMessage, Int32 ServiceID)
    {
        //var dtval = await cls.select_data_dtAsync($"Exec [PROC_EWALLETTRANSACTION] '{_memberAllowDetails.LoginID}',{amount}, 'CR','Refund - {narration}','','{ServiceID}','{agentID}',0,0,0,'{surcharge}','{gst}'");
        var dtval = await cls.select_data_dtAsync($"Exec [PROC_AEPSWALLETTRANSACTION] '{_memberAllowDetails.LoginID}',{amount}, 'CR','Refund - {narration}','','{ServiceID}','{agentID}',0,0,0,'{surcharge}','{gst}'");

        if (dtval.Rows.Count > 0)
        {
            return ErrorShow.ErrorMessageAPI(errorMessage);
        }

        return ErrorShow.ErrorMessageAPI($"Transaction Failed due to {errorMessage}, But Amount Not Refunded in Wallet, Contact Admin");
    }

    public async Task<string> UPIVerification(UPIVerfication uPIVerfication, string MemberID)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(UPITransferService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        // Get member details
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (memberDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Not Allow. Contact Admin.");
        }

        // Initialize variables
        decimal myBalance = memberDetails.AEPSBalance;
        string agentID = await wallet.GetOrderID();
        // Step 5: Get surcharge and commission details
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
            memberDetails.PackgeID.ToString(),
            0,
            serviceData.APIID.ToString(),
            UPITransferService.ServiceID, "UPIV"
        );

        if (item.surcharge == 0)
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");

        decimal verificationFee = item.surcharge;

        // Check if the balance is sufficient
        if (myBalance < (verificationFee + memberDetails.HoldAMT)) // Add hold amount
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }

        // Set up transaction details
        string description = $"{uPIVerfication.Number} UPI Verification With TransID-{agentID}";
        string narration = $"Account Verification TransID-{agentID}";
        uPIVerfication.OrderID = agentID;

        // Check if UPI ID is already verified
        string existingName = await cls.select_data_scalar_stringAsync("select top 1 name from UPITransfer where UPIID='" + uPIVerfication.Number + "' and Name<>''");

        if (!string.IsNullOrEmpty(existingName))
        {
            return ErrorShow.SuccessMessageAPI(existingName);
        }

        // Process UPI verification
        //if (serviceData.APIID != 1)
        // {
        //return ErrorShow.ErrorMessageAPI("API Not Allow For Verification");
        // }
        string Request = uPIVerfication.GetJson();

        string apiResult = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.UPIVerification);


        try
        {
            await commonService.SaveLogs(new LogsRequestModel() { ServiceID = ConstantsData.SUPIVerification, APIID = serviceData.APIID, TransID = agentID, RequestData = Request, ResponseData = apiResult, CallBackData = "" });
        }
        catch { };

        JObject responseJson = JObject.Parse(apiResult);
        decimal gstCalculation = verificationFee * 0.18m; // 18% GST

        if (responseJson["code"].ToString().ToUpper() != "TXN")
        {
            return ErrorShow.ErrorMessageAPI(responseJson["mess"].ToString());
        }

        // Update UPI Transfer record
        int updateResult = await cls.update_dataAsync("AddEditUPITransfer " + memberDetails.Msrno + ",0,'" + verificationFee + "','" + uPIVerfication.Number + "','" + responseJson["data"]["Name"].ToString() + "','Success','UPI','','" + agentID + "','" + responseJson["data"]["STID"].ToString() + "','" + responseJson["data"]["RRN"].ToString() + "','" + apiResult + "'," + gstCalculation + "," + serviceData.APIID + ",'Web', '" + UPITransferService.ServiceID + "', '" + item.OpID + "','" + myBalance + "'");
        if (updateResult <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Try After Sometime");
        }

        // Execute wallet transaction
        //await cls.select_data_scalar_stringAsync("exec [PROC_EWALLETTRANSACTION]  '" + memberDetails.LoginID + "', 0, 'Dr','" + narration + "','" + description + "'," + ConstantsData.SUPIVerification + ",'" + agentID + "',0,0,0," + verificationFee + "," + gstCalculation + "");
        await cls.select_data_scalar_stringAsync("exec [PROC_AEPSWALLETTRANSACTION] '" + memberDetails.LoginID + "', 0, 'Dr','" + narration + "','" + description + "'," + ConstantsData.SUPIVerification + ",'" + agentID + "',0,0,0," + verificationFee + "," + gstCalculation + "");

        return ErrorShow.SuccessMessageAPI(responseJson["data"]["Name"].ToString());


    }

}