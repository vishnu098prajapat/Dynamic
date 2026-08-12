using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for AccountVerificationService
/// </summary>
public class AccountVerificationService : IAccountVerificationService
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    public static String ServiceID = "1062";
    public AccountVerificationService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
    }

    public async Task<string> AccountVerification(CommonAccountVerificationRequest accountVerifyST, string MemberID)
    {

        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(AccountVerificationService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        // Get member details
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Not Allow. Contact Admin.");
        }

        //// Step 5: Get surcharge and commission details
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
            _memberAllowDetails.PackgeID.ToString(),
            0,
            serviceData.APIID.ToString(),
            AccountVerificationService.ServiceID, "DMTV"
        );

        if (item.surcharge == 0)
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


        string hddGroupTransID = await wallet.GetOrderID();
        decimal MyBalance = 0;
        decimal charge = 0M;
        decimal mySurcharge = item.surcharge;
        MyBalance = _memberAllowDetails.MainBalance;
        if (MyBalance >= (charge + _memberAllowDetails.HoldAMT + mySurcharge))
        {

            Decimal GSTCalculation = (mySurcharge * 18 / 100);
            Decimal GSTCalculationC = GSTCalculation;// mySurcharge - GSTCalculation;

            string agentID = await wallet.GetOrderID();
            accountVerifyST.OrderID = agentID;

            string Dec = $"{accountVerifyST.AccountNO}/{accountVerifyST.IFSC}/{agentID}";
            string narration = $"{accountVerifyST.AccountNO}/{accountVerifyST.IFSC}/{agentID}";

            //Iserver
            AccountVerificationIserve request = new AccountVerificationIserve();
            request.beneName = "Soni";
            request.beneAccountNo = accountVerifyST.AccountNO;
            request.beneifsc = accountVerifyST.IFSC;
            request.benePhoneNo = Convert.ToInt64(_memberAllowDetails.Mobile);
            request.clientReferenceNo = agentID;
           
            request.latlong = $"";
            request.pincode = 302020;
            request.custMobNo = Convert.ToInt64(accountVerifyST.CusMobile);
            request.custName = "Vipin";
            request.custIpAddress = accountVerifyST.IP;
            request.beneBankName = accountVerifyST.BankName;
            request.paramA = "";
            request.paramB = "";
            PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
            if (serviceData.APIID == 1)
                payoutCommonResponse = await VSoniTechno(request);


            if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
            {
                int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + charge + "," + mySurcharge + ",0,'Success','Verification','" + payoutCommonResponse.Message + "','" + agentID + "','" + payoutCommonResponse.VendorID + "','" + payoutCommonResponse.RRN + "','" + payoutCommonResponse.Result + "',0,'" + accountVerifyST.BankName + "','" + accountVerifyST.AccountNO + "','" + accountVerifyST.IFSC + "','ALL','" + payoutCommonResponse.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + accountVerifyST.CusName + "','" + accountVerifyST.CusMobile + "','" + payoutCommonResponse.Name + "','" + GSTCalculationC + "','" + accountVerifyST.Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + accountVerifyST.IP + "'");
                if (Val > 0)
                {
                    DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + charge + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SAccountVerification + ",'" + agentID + "',0,0,0,'" + mySurcharge + "','" + GSTCalculationC + "'");
                    string jsonString = JsonSerializer.Serialize(payoutCommonResponse);
                    return ErrorShow.SuccessMessageAPI(payoutCommonResponse.Name);
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI("Try After Sometime");
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(payoutCommonResponse.Message);
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Insufficient Balance");
        }



    }


    public async Task<PayoutCommonResponse> VSoniTechno(AccountVerificationIserve request)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        AccountVerifyST accountVerifySTS = new AccountVerifyST();
        accountVerifySTS.APIID = SONITechnoCredentrial.APIID;
        accountVerifySTS.Token = SONITechnoCredentrial.Token;
        accountVerifySTS.MethodName = "accountverify";
        accountVerifySTS.OrderID = request.clientReferenceNo;
        accountVerifySTS.number = request.beneAccountNo;
        accountVerifySTS.ifsc = request.beneifsc;

        string Result = await ApiPostCall.PostCallAsync(accountVerifySTS.GetJson(), SONITechnoCredentrial.MoneyURL);
        JObject TransferData = JObject.Parse(Result);
        if (TransferData["code"].ToString().ToUpper() == "TXN")
        {
            if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS")
            {
                string Status = TransferData["status"].ToString();
                Response.Status = TransferData["data"]["Status"].ToString();
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["STID"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = request.beneAccountNo;
                Response.IFSC = request.beneifsc;
                Response.RRN = TransferData["data"]["RRN"].ToString();
                Response.Result = Result;
                Response.Name = TransferData["data"]["Name"].ToString();
                Response.OrderID = TransferData["data"]["OrderID"].ToString();
            }

        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = TransferData["mess"].ToString();
            Response.TXNCode = "";
            Response.AccountNo = request.beneAccountNo;
            Response.IFSC = request.beneifsc;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = "";
            Response.OrderID = request.clientReferenceNo;
        }
        return Response;
    }


 
}