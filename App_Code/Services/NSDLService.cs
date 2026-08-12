using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for NSDLService
/// </summary>
public class NSDLService : INSDLService
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    ICommissionService commissionService;
    private static string ServiceID = "21";
    ICommonService commonService;

    public NSDLService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commonService = new CommonService();

        commissionService = new CommissionService();
    }

    public async Task<int> CheckAgentRegisterOrNot(string MemberID)
    {
        string PsaIdCount = await cls.select_data_scalar_stringAsync("Select PsaIdPan from member where Msrno='" + MemberID + "' and IsActive=1");
        if (!string.IsNullOrEmpty(PsaIdCount))
            return 1;
        else
            return 0;
    }


    public async Task<string> MERCHANTREGISTRATION(NSDLMerchantRegistration nSDLMerchantRegistration, string MemberID)
    {
        // Check service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(NSDLService.ServiceID);
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

        // Retrieve member balance
        decimal MyBalance = _memberAllowDetails.MainBalance;
        string agentID = await wallet.GetOrderID();
        var item =await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), 0, serviceData.APIID.ToString(), NSDLService.ServiceID, "NSDLOnbord");

        if (item.surcharge == 0)
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");

        decimal surcharge = item.surcharge;
        decimal GSTCalculation = 0;
        decimal TotalAmount = surcharge + GSTCalculation;

        if (MyBalance < TotalAmount)
        {
            return ErrorShow.ErrorMessageAPI("Insufficient Balance");
        }

        // Create narration and description
        string Narration = $"Pan Onbording/{agentID}";
        string Dec = "";

        // Send API request using HTTPS
        string Result = await ApiPostCall.PostCallAsync(nSDLMerchantRegistration.GetJson(), SONITechnoCredentrial.NSDLPan);

        try
        {
            await commonService.SaveLogs(new LogsRequestModel() { ServiceID = NSDLService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = nSDLMerchantRegistration.GetJson(), ResponseData = Result, CallBackData = "" });
        }
        catch { };

        JObject ObjData = JObject.Parse(Result);

        if (ObjData["code"].ToString().ToUpper() != "ERR")
        {
            // Parameterized query to update the member's PSA ID
            string agentId = ObjData["data"]["DATA"]["AgentId"].ToString();
            int UpdatVal = cls.update_data("update member set panstatus='Success', PsaIdPan='" + ObjData["data"]["DATA"]["AgentId"].ToString() + "' where loginid='" + _memberAllowDetails.LoginID + "'");

            // Execute wallet transaction
            DataTable dtval = await ExecuteWalletTransaction(_memberAllowDetails.LoginID, surcharge, Narration, Dec, agentID, 0, 0);
            if (dtval.Rows.Count == 0 || dtval.Rows[0]["msrno"].ToString() != "1")
            {
                return ErrorShow.ErrorMessageAPI("Transaction Failed");
            }

            // Show success message with redirection
            return ErrorShow.SuccessMessageAPI("Agent Registration Successful. Your PSA ID is: " + agentId);
        }
        else
        {
            // Show error message from the API response
           return ErrorShow.ErrorMessageAPI(ObjData["mess"].ToString());
        }

    }

    public async Task<string> ApplyNewPanCard(newPanApplySoft newPanApplySoft, string MemberID, string Source)
    {
        // Check service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(NSDLService.ServiceID);
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

        // Retrieve member balance
        decimal MyBalance = _memberAllowDetails.MainBalance;
        string agentID = await wallet.GetOrderID();
        var item =await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), 0, serviceData.APIID.ToString(), NSDLService.ServiceID, "NSDLP");
        decimal surcharge = item.surcharge;
        decimal GSTCalculation = 0;
        decimal Commission = 0;
        decimal TotalAmount = surcharge + GSTCalculation;

        if (item.surcharge == 0)
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


        if (MyBalance < TotalAmount)
        {
            return ErrorShow.ErrorMessageAPI("Insufficient Balance");
        }

        // Create narration and description
        string Narration = $"Pan TransID/{agentID}";
        string Dec = "";

        // Execute wallet transaction
        DataTable dtval = await ExecuteWalletTransaction(_memberAllowDetails.LoginID, surcharge, Narration, Dec, agentID, 0, 0);
        if (dtval.Rows.Count == 0 || dtval.Rows[0]["msrno"].ToString() != "1")
        {
            return ErrorShow.ErrorMessageAPI("Transaction Failed");
        }

        String Requet = Newtonsoft.Json.JsonConvert.SerializeObject(newPanApplySoft);

        int val = await cls.update_dataAsync($"exec AddEditNSDLPan '" + _memberAllowDetails.Msrno + "','P','" + agentID + "','','','" + surcharge + "','" + surcharge + "',0,'Pending','" + Requet + "',''");
        if (val <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Try After Sometime");
        }

        newPanApplySoft.txnid = agentID;

        if (serviceData.APIID == 1) // Assuming API ID 1 is for a specific service
        {
            string result = await ApiPostCall.PostCallAsync(newPanApplySoft.GetJson(), SONITechnoCredentrial.NSDLPan);

            try
            {
                await commonService.SaveLogs(new LogsRequestModel() { ServiceID = NSDLService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = newPanApplySoft.GetJson(), ResponseData = result, CallBackData = "" });
            }
            catch { };

            return await HandleTransferResponse(_memberAllowDetails, result, agentID, Narration, surcharge, 0, 0);
        }
        return ErrorShow.ErrorMessageAPI("API Not Allow For UPITransfer");
    }


    private async Task<string> ProcessRefund(MemberAllowData _memberAllowDetails, string agentID, string narration, decimal amount, decimal surcharge, decimal gst, string errorMessage)
    {
        var dtval = await cls.select_data_dtAsync($"Exec [PROC_EWALLETTRANSACTION] '{_memberAllowDetails.LoginID}',{amount}, 'CR','Refund - {narration}','','{ConstantsData.SPanCard}','{agentID}',0,0,0,'{surcharge}','{gst}'");

        if (dtval.Rows.Count > 0)
        {
            return ErrorShow.ErrorMessageAPI("Transaction Failed");
        }

        return ErrorShow.ErrorMessageAPI($"Transaction Failed due to {errorMessage}, But Amount Not Refunded in Wallet, Contact Admin");
    }

    private async Task<string> HandleTransferResponse(MemberAllowData _memberAllowDetails, string result, string agentID, string narration, decimal amount, decimal surcharge, decimal gst)
    {
        JObject transferData = JObject.Parse(result);
        if (transferData["code"].ToString().ToUpper() == "ERR")
        {
            await cls.update_dataAsync($"Update NSDLPan set Status='Refund',Reason='{transferData["mess"].ToString()}',Result='{result}' where OrderID='{agentID}'");
            return await ProcessRefund(_memberAllowDetails, agentID, narration, amount, surcharge, gst, transferData["mess"].ToString());
        }

        if (transferData["data"]["status"].ToString().ToUpper() != "1")
        {
            await cls.update_dataAsync($"Update NSDLPan set Status='Refund',Reason='{transferData["data"]["message"].ToString()}',Result='{result}' where OrderID='{agentID}'");
            return await ProcessRefund(_memberAllowDetails, agentID, narration, amount, surcharge, gst, transferData["mess"].ToString());
        }

        string status = transferData["data"]["status"].ToString().ToUpper();
        await cls.update_dataAsync($"Update NSDLPan set Status='Pending',Result='{result}' where OrderID='{agentID}'");
        var returnData = new
        {
            agentID,
            Message = transferData["mess"].ToString(),
            status,
            Auth = transferData["data"]["response"].ToString()
        };
        return ErrorShow.SuccessArrayAPI(Newtonsoft.Json.JsonConvert.SerializeObject(returnData));
    }

    private async Task<DataTable> ExecuteWalletTransaction(string loginID, decimal amount, string narration, string description, string agentID, decimal surcharge, decimal gst)
    {
        return await cls.select_data_dtAsync($"exec PROC_EWALLETTRANSACTION '{loginID}','{amount}','DR','{narration}','{description}',{ConstantsData.SPanCard},'{agentID}',0,0,0,'{surcharge}','{gst}'");
    }
}