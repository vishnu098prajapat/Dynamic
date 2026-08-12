using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for PayoutService
/// </summary>
public class PayoutService : IPayoutService
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    private static string ServiceID = "23";

    public PayoutService()
    {
        wallet = new Wallet();
        serviceList = new ServiceList();
        cls = new cls_connection();
    }

    public async Task<string> FundTransfer(PayoutCommon payoutCommon, string MemberID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PayoutService.ServiceID);
        if (serviceData.Status == "Success")
        {
            decimal MyBalance = 0;
            string agentID = await wallet.GetOrderID();
            string hddGroupTransID = await wallet.GetOrderID();

            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
            if (!_memberAllowDetails.HoldUnHold)
            {

                string Dec = $"{payoutCommon.AccountNO}/{payoutCommon.IFSC}/{agentID}";
                string narration = $"{payoutCommon.AccountNO}/{payoutCommon.IFSC}/{agentID}";
                PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
                //if (serviceData.APIID == 5)//Paysprint
                //{
                //    payoutCommonResponse = await FundTransferPaysPrint(payoutCommon);
                //}

                //else if (serviceData.APIID == 6)//Iserve
                //{
                //    payoutCommonResponse = await FundTransferIserve(payoutCommon);
                //}
                //else if (serviceData.APIID == 7)//Safepax
                //{
                //    payoutCommonResponse = await FundTransferSafePax(payoutCommon);
                //}
                if (serviceData.APIID == 1)//Iserve
                {
                    payoutCommonResponse = await FundTransferInstantWithSoniTechno(payoutCommon);
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI("API Not Allow For Payout");
                }
                string jsonString = JsonSerializer.Serialize(payoutCommonResponse);
                return ErrorShow.SuccessArrayAPI(jsonString);

            }
            else
                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }

    public async Task<PayoutCommonResponse> FundTransferPaysPrint(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        PayoutSettlement payoutSettlement = new PayoutSettlement();
        payoutSettlement.amount = payoutCommon.Amount.ToString();
        payoutSettlement.refid = payoutCommon.OrderID;
        payoutSettlement.mode = payoutCommon.Mode;
        payoutSettlement.bene_id = payoutCommon.AccountID;
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("payout/payout/dotransaction", jj.GetToken(), payoutSettlement.GetJson());
        JObject TransferData = JObject.Parse(Result);
        if (Convert.ToBoolean(TransferData["status"]))
        {
            Response.Status = "Pending";
            Response.Message = TransferData["message"].ToString();
            Response.VendorID = TransferData["ackno"].ToString();
            Response.TXNCode = TransferData["response_code"].ToString();
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["message"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }


    public async Task<PayoutCommonResponse> FundTransferInstantWithSoniTechno(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        MoneyTransferRequestST moneyTransferRequestST = new MoneyTransferRequestST();
        moneyTransferRequestST.Amount = Convert.ToDecimal(payoutCommon.Amount);
        moneyTransferRequestST.ifsc = payoutCommon.IFSC;
        moneyTransferRequestST.number = payoutCommon.AccountNO;
        moneyTransferRequestST.PaymentType = payoutCommon.Mode;
        moneyTransferRequestST.OrderID = payoutCommon.OrderID;
        moneyTransferRequestST.CustomerMobileNo = payoutCommon.Mobile;
        moneyTransferRequestST.MethodName = "sendamount";

        string Result = await ApiPostCall.PostCallAsync(moneyTransferRequestST.GetJson(), SONITechnoCredentrial.PayOutURL);
        JObject TransferData = JObject.Parse(Result);
        if (TransferData["code"].ToString().ToUpper() != "ERR")
        {
            if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS" || TransferData["data"]["Status"].ToString().ToUpper() == "PENDING" || TransferData["data"]["Status"].ToString().ToUpper() == "INPROGRESS")
            {
                string Status = TransferData["data"]["Status"].ToString();
                if (Status.ToUpper() == "PENDING" || Status.ToUpper() == "INPROGRESS")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["STID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["RRN"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = "Failed";
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["STID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["RRN"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferIserve(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        Location location = new Location();
        PayoutRequestIserve payoutRequestIserve = new PayoutRequestIserve();
        payoutRequestIserve.beneName = payoutCommon.Name;
        payoutRequestIserve.beneAccountNo = payoutCommon.AccountNO;
        payoutRequestIserve.beneifsc = payoutCommon.IFSC;
        payoutRequestIserve.clientReferenceNo = payoutCommon.OrderID;
        payoutRequestIserve.amount = payoutCommon.Amount.ToString().ToInt32();
        payoutRequestIserve.fundTransferType = payoutCommon.Mode;
        payoutRequestIserve.pincode = 302020;
        payoutRequestIserve.custName = payoutCommon.Name;
        payoutRequestIserve.custMobNo = payoutCommon.Mobile;
        payoutRequestIserve.latlong = $"{location.Latitude},{location.Longitude}";
        payoutRequestIserve.benePhoneNo = Convert.ToUInt64(payoutCommon.Mobile);
        payoutRequestIserve.beneBankName = payoutCommon.BankName;
        payoutRequestIserve.custIpAddress = "122.176.125.90";
        payoutRequestIserve.paramA = "";
        payoutRequestIserve.paramB = "";

        cls.update_data("insert into logdata(data)values('" + payoutRequestIserve.GetJSON() + "')");

        string Result = await iserve.PostCallWithBodyAsync(payoutRequestIserve.GetJSON(), iserve.TXNAPIPayout);
        JObject TransferData = JObject.Parse(Result);
        if (TransferData["txnStatus"] == null)
        {
            if (TransferData["status"].ToString().ToUpper() == "SUCCESS" || TransferData["status"].ToString().ToUpper().ToUpper() == "PENDING" || TransferData["status"].ToString().ToUpper().ToUpper() == "INPROGRESS")
            {
                string Status = TransferData["status"].ToString();
                if (TransferData["status"].ToString().ToUpper().ToUpper() == "PENDING" || TransferData["status"].ToString().ToUpper().ToUpper() == "INPROGRESS")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = TransferData["statusDesc"].ToString();
                Response.VendorID = TransferData["transactionId"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["rrn"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = "Failed";
                Response.Message = TransferData["statusDesc"].ToString();
                Response.VendorID = TransferData["transactionId"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["message"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferSafePax(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        SafePayout safePayout = new SafePayout();
        Header header = new Header();
        safePayout.header = header;
        UserInfo userInfo = new UserInfo();
        safePayout.userInfo = userInfo;
        Transaction transaction = new Transaction();
        transaction.txnAmt = 0;
        safePayout.transaction = transaction;
        PayOutBean payOutBean = new PayOutBean();
        payOutBean.mobileNo = payoutCommon.Mobile;
        payOutBean.accountNo = payoutCommon.AccountNO;
        payOutBean.ifscCode = payoutCommon.IFSC;
        payOutBean.bankName = payoutCommon.BankName;
        payOutBean.accountHolderName = payoutCommon.Name;
        payOutBean.txnType = payoutCommon.Mode;
        payOutBean.accountType = "Saving";
        payOutBean.emailId = payoutCommon.MailID;
        payOutBean.orderRefNo = payoutCommon.OrderID;
        payOutBean.count = 0;
        safePayout.payOutBean = payOutBean;
        string hh = "{\"header\": {\"operatingSystem\": \"android\",\"sessionId\": \"" + safepay.MerchantId + "\",\"version\": \"1.0.0\"},\"userInfo\": {           },\"transaction\": {\"requestType\": \"WTW\",\"requestSubType\": \"PWTB\",\"tranCode\": 0,\"txnAmt\": 0.0,\"id\": \"" + safepay.MerchantId + "\",\"surChargeAmount\": 0.0,\"txnCode\": 0,\"userType\": 0},\"payOutBean\": {               \"mobileNo\": \"" + payoutCommon.Mobile + "\",\"txnAmount\": \"" + payoutCommon.Amount + "\",\"accountNo\": \"" + payoutCommon.AccountNO + "\",\"ifscCode\": \"" + payoutCommon.IFSC + "\",\"bankName\": \"" + payoutCommon.BankName + "\",\"accountHolderName\": \"" + payoutCommon.Name + "\",\"txnType\": \"IMPS\",\"accountType\": \"Saving\",\"emailId\": \"payout@gmail.com\",\"orderRefNo\": \"" + payoutCommon.OrderID + "\",\"count\": 0}}";

        string Request = Common.encrypt(hh);
        string ResultEncrpted = await safepay.PostCallWithBodyAsync(Request);
        JObject jResponse = JObject.Parse(ResultEncrpted);
        if (jResponse["status"] == null)
        {
            string Result = Common.decrypt(jResponse["payload"].ToString());
            JObject TransferData = JObject.Parse(Result);
            if (TransferData["response"]["code"].ToString() == "0001" || TransferData["response"]["code"].ToString() == "0000" || TransferData["response"]["code"].ToString() == "0002" || TransferData["response"]["code"].ToString() == "0003")
            {
                string Status = "";
                if (TransferData["response"]["code"].ToString() == "0001")
                    Status = "Pending";
                else if (TransferData["response"]["code"].ToString() == "0002" || TransferData["response"]["code"].ToString() == "0003")
                    Status = "Failed";
                else if (TransferData["response"]["code"].ToString() == "0000")
                    Status = "Success";

                Response.Status = Status;
                Response.Message = TransferData["payOutBean"]["statusDesc"].ToString();
                Response.VendorID = TransferData["payOutBean"]["payoutId"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                string Status = TransferData["payOutBean"]["txnStatus"].ToString();
                if (TransferData["payOutBean"]["txnStatus"].ToString() == "REJECTED")
                    Status = "Failed";

                Response.Status = Status;
                Response.Message = TransferData["payOutBean"]["statusDesc"].ToString();
                Response.VendorID = TransferData["payOutBean"]["payoutId"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = jResponse["userMessage"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = ResultEncrpted;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }


    AesManaged CreateAes(string key)
    {
        var aes = new AesManaged();
        aes.Key = Convert.FromBase64String(key); //UTF8-Encoding
        aes.IV = System.Text.Encoding.UTF8.GetBytes("0123456789abcdef");//UT8-Encoding
        return aes;
    }


    public async Task<PayoutCommonResponse> FundTransferInstantWithAccount(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        RootInstantPayoutWithAccount rootInstantPayout = new RootInstantPayoutWithAccount();
        Payee payee = new Payee();
        payee.name = payoutCommon.Name;
        payee.bankIfsc = payoutCommon.IFSC;
        payee.accountNumber = payoutCommon.AccountNO;
        rootInstantPayout.payee = payee;
        PayerWithAccount payer = new PayerWithAccount();
        rootInstantPayout.payer = payer;
        Location objLocation = new Location();
        objLocation = objLocation.GetLocationDataCurrent("103.73.190.82");
        rootInstantPayout.externalRef = payoutCommon.OrderID;
        rootInstantPayout.latitude = objLocation.Latitude;
        rootInstantPayout.longitude = objLocation.Longitude;
        rootInstantPayout.transferMode = "IMPS";
        rootInstantPayout.transferAmount = payoutCommon.Amount.ToString();
        rootInstantPayout.remarks = "AEPS Settlement";
        rootInstantPayout.alertEmail = payoutCommon.MailID;

        string Result = await ApiPostCallInstantPay.PostCallWithFullURL(rootInstantPayout.GetJson(), "https://api.instantpay.in/payments/payout", "");
        JObject TransferData = JObject.Parse(Result);
        if (InstantPayout.arrTemplate.Contains(TransferData["statuscode"].ToString()))
        {
            if (TransferData["statuscode"].ToString().ToUpper() == "TXN" || TransferData["statuscode"].ToString().ToUpper() == "TUP")
            {
                string Status = "";
                if (TransferData["statuscode"].ToString().ToUpper() == "TXN")
                    Status = "Success";
                else if (TransferData["statuscode"].ToString().ToUpper() == "TUP")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = TransferData["status"].ToString();
                Response.VendorID = TransferData["ipay_uuid"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["txnReferenceId"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                string Status = "Failed";
                Response.Status = Status;
                Response.Message = TransferData["status"].ToString();
                Response.VendorID = "";
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["status"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }


    public async Task<PayoutCommonResponse> FundTransferezypay(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        object Requst = new
        {
            requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
            token = ezypay.token,
            service = payoutCommon.AccountID,
            sender = payoutCommon.Mobile,
            beneid = payoutCommon.BeneID,
            amount = payoutCommon.Amount,
            mode = payoutCommon.Mode,
            agent_mobile = payoutCommon.AgentMobile,
            identifier = payoutCommon.identifier,
            otp = payoutCommon.otp,
        };


        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);

        Response.Request = RequestJson;

        string Result = await ezypay.Postcall(RequestJson, "Dmt/remittanceTransactionAFA");
        JObject TransferData = JObject.Parse(Result);




        if (TransferData["Resp_code"].ToString().ToUpper() == "RCS")
        {
            string Status = "Pending";
            if (TransferData["data"]["txn_status"].ToString().ToUpper() == "SUCCESS")
                Status = "Success";
            else if (TransferData["data"]["txn_status"].ToString().ToUpper() == "PENDING")
                Status = "Pending";
            else if (TransferData["data"]["txn_status"].ToString().ToUpper() == "FAILED")
                Status = "FAILED";

            Response.Status = Status;
            Response.Message = TransferData["data"]["txn_desc"].ToString();
            Response.VendorID = TransferData["data"]["txnid"].ToString();
            Response.TXNCode = TransferData["Resp_code"].ToString();
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = TransferData["data"]["opid"].ToString();
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        else
        {
            string Status = "Failed";
            Response.Status = Status;
            Response.Message = TransferData["Resp_desc"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }


    public async Task<PayoutCommonResponse> FundTransferInstantWithEzypay(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        var Requst = new ezypayinitiateAgentSettlement()
        {
            requestid = payoutCommon.OrderID,
            token = ezypay.token,
            agent_id = payoutCommon.identifier,
            beneficiary_id = payoutCommon.BeneID,
            amount = payoutCommon.Amount.ToString(),
            request_type = "MT" + payoutCommon.Mode,
        };
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        Response.Request = RequestJson;

        string Result = await ezypay.Postcall(RequestJson, "Action/initiateAgentSettlement");

        JObject TransferData = JObject.Parse(Result);

        if (TransferData["Resp_code"].ToString().ToUpper() == "RCS")
        {
            string Status = "Pending";
            if (TransferData["data"]["txn_status"].ToString().ToUpper() == "SUCCESS")
                Status = "Success";
            else if (TransferData["data"]["txn_status"].ToString().ToUpper() == "PENDING")
                Status = "Pending";
            else if (TransferData["data"]["txn_status"].ToString().ToUpper() == "FAILED")
                Status = "FAILED";

            Response.Status = Status;
            Response.Message = TransferData["data"]["txn_desc"].ToString();
            Response.VendorID = TransferData["data"]["txnid"].ToString();
            Response.TXNCode = TransferData["Resp_code"].ToString();
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = TransferData["data"]["opid"].ToString();
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        else
        {
            string Status = "Failed";
            Response.Status = Status;
            Response.Message = TransferData["Resp_desc"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }


    public async Task<PayoutCommonResponse> FundTransferCCPaySoniTechno(CreditPayST creditPayST)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        string jsonString = JsonSerializer.Serialize(creditPayST);
        string Result = await ApiPostCall.PostCallAsync(jsonString, SONITechnoCredentrial.CCpay);
        Response.Request = jsonString;

        JObject TransferData = JObject.Parse(Result);

        if (TransferData["code"].ToString().ToUpper() != "ERR")
        {
            string Status = TransferData["data"]["Status"].ToString().ToUpper();

            if (Status == "SUCCESS")
                Status = "Success";
            else if (Status == "ERROR" || Status == "FAILED")
                Status = "Failed";

            if (Status == "Success" || Status == "Pending")
            {
                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["STID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = creditPayST.CardNo;
                Response.IFSC = creditPayST.MobileNO;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = creditPayST.Name;
                Response.OrderID = creditPayST.OrderID;
            }
            else
            {
                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = "";
                Response.TXNCode = "";
                Response.AccountNo = creditPayST.CardNo;
                Response.IFSC = creditPayST.MobileNO;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = creditPayST.Name;
                Response.OrderID = creditPayST.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = creditPayST.CardNo;
            Response.IFSC = "";
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = creditPayST.Name;
            Response.OrderID = creditPayST.OrderID;
        }

        return Response;
    }


    public async Task<PayoutCommonResponse> FundTransferCCPayInstantPay(CreditPayST creditPayST)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        RootCCPay rootInstantPayout = new RootCCPay();
        Payee payee = new Payee();
        payee.name = creditPayST.Name;
        payee.accountNumber = InstantPayout.GetEncryptAadhar(creditPayST.CardNo);
        rootInstantPayout.payee = payee;
        PayerCCPay payer = new PayerCCPay();
        rootInstantPayout.payer = payer;

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationDataCurrent("103.76.249.170");
        rootInstantPayout.externalRef = creditPayST.OrderID;
        rootInstantPayout.latitude = objLocation.Latitude;
        rootInstantPayout.longitude = objLocation.Longitude;
        rootInstantPayout.transferMode = "CREDITCARD";
        rootInstantPayout.transferAmount = creditPayST.Amount;
        rootInstantPayout.remarks = "CC Pay";
        string Result = await ApiPostCallInstantPay.PostCallWithFullURL(rootInstantPayout.GetJson(), "https://api.instantpay.in/payments/payout", "");
        JObject TransferData = JObject.Parse(Result);
        if (InstantPayout.arrTemplate.Contains(TransferData["statuscode"].ToString()))
        {
            if (TransferData["statuscode"].ToString().ToUpper() == "TXN" || TransferData["statuscode"].ToString().ToUpper() == "TUP")
            {
                string Status = "";
                if (TransferData["statuscode"].ToString().ToUpper() == "TXN")
                    Status = "Success";
                else if (TransferData["statuscode"].ToString().ToUpper() == "TUP")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = TransferData["status"].ToString();
                Response.VendorID = TransferData["ipay_uuid"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = creditPayST.CardNo;
                Response.IFSC = creditPayST.MobileNO;
                Response.RRN = TransferData["data"]["txnReferenceId"].ToString();
                Response.Result = Result;
                Response.Name = creditPayST.Name;
                Response.OrderID = creditPayST.OrderID;
            }
            else
            {
                string Status = "Failed";
                Response.Status = Status;
                Response.Message = TransferData["status"].ToString();
                Response.VendorID = "";
                Response.TXNCode = "";
                Response.AccountNo = creditPayST.CardNo;
                Response.IFSC = creditPayST.MobileNO;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = creditPayST.Name;
                Response.OrderID = creditPayST.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["status"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = creditPayST.CardNo;
            Response.IFSC = creditPayST.MobileNO;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = creditPayST.Name;
            Response.OrderID = creditPayST.OrderID;
        }

        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferAeronpay(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        AeronpayRequest payoutRequestPaymonk = new AeronpayRequest();
        payoutRequestPaymonk.amount = payoutCommon.Amount.ToString();
        payoutRequestPaymonk.bankid = await cls.select_data_scalar_stringAsync("select bank_id from bankaerolist where bank_name='" + payoutCommon.BankName + "'");
        payoutRequestPaymonk.client_referenceId = payoutCommon.OrderID;
        payoutRequestPaymonk.transferMode = payoutCommon.Mode;
        payoutRequestPaymonk.latitude = "26.78";
        payoutRequestPaymonk.longitude = "78.22";
        payoutRequestPaymonk.remarks = "AEPS Settlement";

        BeneDetails beneDetails = new BeneDetails();

        beneDetails.bankAccount = payoutCommon.AccountNO;
        beneDetails.ifsc = payoutCommon.IFSC;
        beneDetails.name = payoutCommon.Name;
        beneDetails.email = "caspay@gmail.com";
        beneDetails.phone = payoutCommon.Mobile;
        beneDetails.address1 = "112/8 delhi";
        payoutRequestPaymonk.beneDetails = beneDetails;

        string jsonString = JsonSerializer.Serialize(payoutRequestPaymonk);

        string Result = await Aeronpay.GetPostcall(jsonString, "v2/payout/imps_payment");
        JObject TransferData = JObject.Parse(Result);

        string Status = "Pending";

        if (TransferData["status"].ToString().ToUpper() == "SUCCESS")
            Status = "Success";
        else if (TransferData["status"].ToString().ToUpper() == "FAILED" || TransferData["status"].ToString().ToUpper() == "ERROR" || TransferData["status"].ToString().ToUpper() == "BAD_REQUEST_ERROR")
            Status = "Failed";

        if (Status == "Success" || Status == "Pending")
        {
            Response.Status = Status;
            Response.Message = TransferData["message"].ToString();
            Response.VendorID = TransferData["data"]["transactionId"].ToString();
            Response.TXNCode = TransferData["statusCode"].ToString();
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        else
        {
            string mess = "";
            if (TransferData["errors"] == null)
            {
                if (TransferData["description"] != null)
                    mess = TransferData["description"].ToString();
                else
                {
                    mess = TransferData["message"].ToString();
                }
            }
            else
            {
                mess = TransferData["errors"][0].ToString();
            }
            Response.Status = Status;
            Response.Message = mess;
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferInstantWithPromptPe(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        PromptPeMoneyTransferRequestST moneyTransferRequestST = new PromptPeMoneyTransferRequestST();
        moneyTransferRequestST.Amount = Convert.ToDecimal(payoutCommon.Amount).ToString();
        moneyTransferRequestST.IFSC = payoutCommon.IFSC;
        moneyTransferRequestST.Accountno = payoutCommon.AccountNO;
        moneyTransferRequestST.Mode = payoutCommon.Mode;
        moneyTransferRequestST.OrderID = payoutCommon.OrderID;
        moneyTransferRequestST.Mobile = payoutCommon.Mobile;
        moneyTransferRequestST.MethodName = "payout";
        moneyTransferRequestST.Name = payoutCommon.Name;
        // moneyTransferRequestST.IP = "";
        moneyTransferRequestST.BankName = "ICIC";
        // moneyTransferRequestST.Latitude = "";
        // moneyTransferRequestST.Longitude = "";


        string Result = await PromptPe.PostCall(moneyTransferRequestST.GetJson());
        JObject TransferData = JObject.Parse(Result);
        if (TransferData["code"].ToString().ToUpper() != "ERR")
        {
            if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS" || TransferData["data"]["Status"].ToString().ToUpper() == "PENDING" || TransferData["data"]["Status"].ToString().ToUpper() == "INPROGRESS")
            {
                string Status = TransferData["data"]["Status"].ToString();
                if (Status.ToUpper() == "PENDING" || Status.ToUpper() == "INPROGRESS")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["VendorID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["VendorID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = "Failed";
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["VendorID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["VendorID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferBetaUPI(UPITransferRequestST payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        PromptPeMoneyTransferRequestST moneyTransferRequestST = new PromptPeMoneyTransferRequestST();
        moneyTransferRequestST.Amount = payoutCommon.Amount.ToString();
        moneyTransferRequestST.IFSC = "UPI";
        moneyTransferRequestST.Accountno = payoutCommon.UPIID;
        moneyTransferRequestST.Mode = "UPI";
        moneyTransferRequestST.OrderID = payoutCommon.OrderID;
        moneyTransferRequestST.Mobile = payoutCommon.CustomerMobileNo;
        moneyTransferRequestST.MethodName = "payout";
        moneyTransferRequestST.Name = payoutCommon.Name;
        moneyTransferRequestST.BankName = "UPI";


        string Result = await PromptPe.PostCall(moneyTransferRequestST.GetJson());
        string jsonString = JsonSerializer.Serialize(moneyTransferRequestST);

        JObject TransferData = JObject.Parse(Result);



        if (TransferData["code"].ToString().ToUpper() != "ERR")
        {
            if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS" || TransferData["data"]["Status"].ToString().ToUpper() == "PENDING" || TransferData["data"]["Status"].ToString().ToUpper() == "INPROGRESS")
            {
                string Status = TransferData["data"]["Status"].ToString();
                if (Status.ToUpper() == "PENDING" || Status.ToUpper() == "INPROGRESS")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["VendorID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.UPIID;
                Response.IFSC = payoutCommon.UPIID;
                Response.RRN = TransferData["data"]["VendorID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = "Failed";
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["VendorID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.UPIID;
                Response.IFSC = payoutCommon.UPIID;
                Response.RRN = TransferData["data"]["VendorID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.UPIID;
            Response.IFSC = payoutCommon.UPIID;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;

    }

    public async Task<PayoutCommonResponse> FundTransferAeronpayUPI(UPITransferRequestST payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        AeronpayRequestAUPI payoutRequestPaymonk = new AeronpayRequestAUPI();
        payoutRequestPaymonk.amount = payoutCommon.Amount.ToString();
        payoutRequestPaymonk.client_referenceId = payoutCommon.OrderID;
        payoutRequestPaymonk.transferMode = "upi";
        payoutRequestPaymonk.latitude = "26.78";
        payoutRequestPaymonk.longitude = "78.22";
        payoutRequestPaymonk.remarks = "AEPS Settlement";

        BeneDetailsUPI beneDetails = new BeneDetailsUPI();

        beneDetails.vpa = payoutCommon.UPIID;
        beneDetails.name = payoutCommon.Name;
        beneDetails.email = "k1pau@gmail.com";
        beneDetails.phone = "9925434563";
        beneDetails.address1 = "112/8 delhi";
        payoutRequestPaymonk.beneDetails = beneDetails;

        string jsonString = JsonSerializer.Serialize(payoutRequestPaymonk);

        string Result = await Aeronpay.GetPostcall(jsonString, "v2/payout/upi_payout_txn");
        JObject TransferData = JObject.Parse(Result);

        string Status = "Pending";

        if (TransferData["status"].ToString().ToUpper() == "SUCCESS")
            Status = "Success";
        else if (TransferData["status"].ToString().ToUpper() == "FAILED" || TransferData["status"].ToString().ToUpper() == "ERROR" || TransferData["status"].ToString().ToUpper() == "BAD_REQUEST_ERROR")
            Status = "Failed";

        if (Status == "Success" || Status == "Pending")
        {
            Response.Status = Status;
            Response.Message = TransferData["message"].ToString();
            Response.VendorID = TransferData["data"]["transactionId"].ToString();
            Response.TXNCode = TransferData["statusCode"].ToString();
            Response.AccountNo = payoutCommon.UPIID;
            Response.IFSC = "";
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        else
        {
            string mess = "";
            if (TransferData["errors"] == null)
            {
                if (TransferData["description"] != null)
                    mess = TransferData["description"].ToString();
                else
                {
                    mess = TransferData["message"].ToString();
                }
            }
            else
            {
                mess = TransferData["errors"][0].ToString();
            }
            Response.Status = Status;
            Response.Message = mess;
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.UPIID;
            Response.IFSC = "";
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferSoniTechnoUPI(UPITransferRequestST payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        UPITransferRequestST ObjData = new UPITransferRequestST();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "sendamount";
        ObjData.Amount = payoutCommon.Amount;
        ObjData.UPIID = payoutCommon.UPIID;
        ObjData.OrderID = payoutCommon.OrderID;
        ObjData.Name = payoutCommon.Name;
        ObjData.email = "k1@gmail.com";
        ObjData.CustomerMobileNo = "9328718748";


        string jsonString = JsonSerializer.Serialize(ObjData);
        string Result = await ApiPostCall.PostCallAsync(jsonString, SONITechnoCredentrial.UPI);

        JObject TransferData = JObject.Parse(Result);

        if (TransferData["code"].ToString().ToUpper() != "ERR")
        {
            string Status = TransferData["data"]["Status"].ToString().ToUpper();

            if (Status == "SUCCESS")
                Status = "Success";
            else if (Status == "ERROR" || Status == "FAILED")
                Status = "Failed";

            if (Status == "Success" || Status == "Pending")
            {
                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["STID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.UPIID;
                Response.IFSC = "";
                Response.RRN = TransferData["data"]["RRN"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = "";
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.UPIID;
                Response.IFSC = "";
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.UPIID;
            Response.IFSC = "";
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferfinzep(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        finzeppayoutRoot finzeppayoutRoot = new finzeppayoutRoot();
        finzeppayout finzeppayout = new finzeppayout();
        finzeppayout.AccountNo = payoutCommon.AccountNO;
        finzeppayout.AmountR = (int)payoutCommon.Amount;
        finzeppayout.BankID = await cls.select_data_scalar_intAsny("select id from BankFiz where bankname='" + payoutCommon.BankName + "'");
        finzeppayout.IFSC = payoutCommon.IFSC;
        finzeppayout.SenderMobile = payoutCommon.Mobile;
        finzeppayout.SenderName = payoutCommon.Name;
        finzeppayout.SenderEmail = "payouon@gmail.com";
        finzeppayout.BeneName = payoutCommon.Name;
        finzeppayout.BeneMobile = payoutCommon.Mobile;
        finzeppayout.APIRequestID = payoutCommon.OrderID;
        finzeppayout.SPKey = payoutCommon.Mode;
        finzeppayout.WebHook = payoutCommon.AccountNO;



        finzeppayoutRoot.PayoutRequest = finzeppayout;

        string jsonString = JsonSerializer.Serialize(finzeppayoutRoot);
        Response.Request = jsonString;

        string Result = await finzep.PostCall(jsonString);
        JObject TransferData = JObject.Parse(Result);

        string Status = "Pending";

        if (TransferData["status"].ToString() != "0")
        {
            if (TransferData["statuscode"].ToString().ToUpper() == "1")
                Status = "Success";
            else if (TransferData["statuscode"].ToString().ToUpper() == "-1")
                Status = "Failed";

            if (Status == "Success" || Status == "Pending")
            {
                Response.Status = Status;
                Response.Message = TransferData["message"].ToString();
                Response.VendorID = TransferData["rpid"].ToString();
                Response.TXNCode = TransferData["errorCode"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["liveID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = Status;
                Response.Message = TransferData["message"].ToString();
                Response.VendorID = TransferData["rpid"].ToString();
                Response.TXNCode = TransferData["errorCode"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["liveID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Status = "Failed";
            Response.Status = Status;
            Response.Message = TransferData["message"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferDMTSoniTechno(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        var requestPayload = new
        {
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token,
            MethodName = "sendamount",
            Amount = payoutCommon.Amount,
            PaymentType = payoutCommon.Mode,
            ifsc = payoutCommon.IFSC,
            number = payoutCommon.AccountNO,
            OrderID = payoutCommon.OrderID,
            BeniID = payoutCommon.BeneID,
            CustomerMobileNo = payoutCommon.Mobile,
        };

        string jsonString = JsonSerializer.Serialize(requestPayload);
        Response.Request = jsonString;
        string Result = await ApiPostCall.PostCallAsync(jsonString, SONITechnoCredentrial.MoneyURL);
        JObject TransferData = JObject.Parse(Result);
        Response.Result = Result;

        if (Convert.ToString(TransferData["code"]) != "ERR")
        {
            string Status = TransferData["data"]["Status"].ToString();
            string mess = TransferData["mess"].ToString();

            if (Status == "Success" || Status == "Pending")
            {
                Response.Status = Status;
                Response.Message = mess;
                Response.VendorID = TransferData["data"]["STID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["RRN"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = Status;
                Response.Message = mess;
                Response.VendorID = "";
                Response.TXNCode = "";
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = "";
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            string mess = "";
            mess = TransferData["mess"].ToString();
            Response.Status = "Failed";
            Response.Message = mess;
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferInstantWithPayDMR(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        PayDMRTransferRequest moneyTransferRequestSTPayDMR = new PayDMRTransferRequest();
        moneyTransferRequestSTPayDMR.api_token = PayDMR.ApiToken;
        moneyTransferRequestSTPayDMR.amount = payoutCommon.Amount;
        moneyTransferRequestSTPayDMR.ifsc_code = payoutCommon.IFSC;
        moneyTransferRequestSTPayDMR.account_number = payoutCommon.AccountNO;
        moneyTransferRequestSTPayDMR.channel_id = payoutCommon.Mode == "IMPS" ? "2" : "1";
        moneyTransferRequestSTPayDMR.client_id = payoutCommon.OrderID;
        moneyTransferRequestSTPayDMR.mobile_number = payoutCommon.Mobile;
        moneyTransferRequestSTPayDMR.beneficiary_name = payoutCommon.Name;
        moneyTransferRequestSTPayDMR.email = payoutCommon.MailID;

        string Result = await PayDMR.PostCall(moneyTransferRequestSTPayDMR.GetJson());
        JObject TransferData = JObject.Parse(Result);
        string status = TransferData["status"]?.ToString().ToUpper() ?? "";
        string message = TransferData["message"]?.ToString() ?? "";
        string utr = TransferData["utr"]?.ToString() ?? "";
        string payid = TransferData["payid"]?.ToString() ?? "";
        if (status.ToUpper() != "FAILURE")
        {
            if (status.ToUpper() == "SUCCESS" || status.ToUpper() == "PENDING")
            {
                string Status = status;
                if (Status.ToUpper() == "PENDING")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = message;
                Response.VendorID = payid;
                Response.TXNCode = status.ToUpper();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = utr;
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = "Failed";
                Response.Message = message;
                Response.VendorID = payid;
                Response.TXNCode = status.ToUpper();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = utr;
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = message;
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferInstantWithBetaSource(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();
        BetaMoneyTransferRequestST moneyTransferRequestST = new BetaMoneyTransferRequestST();
        moneyTransferRequestST.Amount = Convert.ToDecimal(payoutCommon.Amount).ToString();
        moneyTransferRequestST.IFSC = payoutCommon.IFSC;
        moneyTransferRequestST.Accountno = payoutCommon.AccountNO;
        moneyTransferRequestST.Mode = payoutCommon.Mode;
        moneyTransferRequestST.OrderID = payoutCommon.OrderID;
        moneyTransferRequestST.Mobile = payoutCommon.Mobile;
        moneyTransferRequestST.MethodName = "payout";
        moneyTransferRequestST.Name = payoutCommon.Name;
        // moneyTransferRequestST.IP = "";
        moneyTransferRequestST.BankName = "ICIC";
        // moneyTransferRequestST.Latitude = "";
        // moneyTransferRequestST.Longitude = "";


        string Result = await BetaSource.PostCall(moneyTransferRequestST.GetJson());
        JObject TransferData = JObject.Parse(Result);
        if (TransferData["code"].ToString().ToUpper() != "ERR")
        {
            if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS" || TransferData["data"]["Status"].ToString().ToUpper() == "PENDING" || TransferData["data"]["Status"].ToString().ToUpper() == "INPROGRESS")
            {
                string Status = TransferData["data"]["Status"].ToString();
                if (Status.ToUpper() == "PENDING" || Status.ToUpper() == "INPROGRESS")
                    Status = "Pending";

                Response.Status = Status;
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["VendorID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["VendorID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
            else
            {
                Response.Status = "Failed";
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["VendorID"].ToString();
                Response.TXNCode = TransferData["code"].ToString();
                Response.AccountNo = payoutCommon.AccountNO;
                Response.IFSC = payoutCommon.IFSC;
                Response.RRN = TransferData["data"]["VendorID"].ToString();
                Response.Result = Result;
                Response.Name = payoutCommon.Name;
                Response.OrderID = payoutCommon.OrderID;
            }
        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        return Response;
    }

    public async Task<PayoutCommonResponse> FundTransferrechargkit(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        var cleanedBankName = Regex.Replace(
    payoutCommon.BankName
        .Replace("Bob", "")
        .Replace("Boi", "")
        .Trim(),
    @"[^a-zA-Z0-9 ]",
    string.Empty
);


        object Requst = new
        {
            mobile_no = payoutCommon.Mobile,
            account_no = payoutCommon.AccountNO,
            ifsc = payoutCommon.IFSC,
            bank_name = cleanedBankName,
            beneficiary_name = payoutCommon.Name,
            amount = payoutCommon.Amount,
            transfer_type = "5",
            partner_request_id = payoutCommon.OrderID
        };
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);


        Response.Request = RequestJson;

        string Result = await rechargkit.PostCall(RequestJson);


        JObject TransferData = JObject.Parse(Result);

        if (Convert.ToString(TransferData["error"]) == "0")
        {
            string Status = TransferData["status"].ToString();
            if (Status == "1")
            {
                Status = "SUCCESS";
            }
            else if (Status == "2")
            {
                Status = "PENDING";
            }
            else if (Status == "3")
            {
                Status = "Failed";
            }
            else
            {
                Status = "PENDING";
            }
            Response.Status = Status;
            Response.Message = TransferData["msg"].ToString();
            Response.VendorID = TransferData["partnerreqid"].ToString();
            Response.TXNCode = TransferData["error"].ToString();
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.Name;
            Response.RRN = TransferData["optransid"].ToString();
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        else
        {

            string Status = "Failed";
            Response.Status = Status;
            Response.Message = TransferData["msg"].ToString();
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }


    public async Task<PayoutCommonResponse> FundTransferclassypay(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();


        object Requst = new
        {
            operatorCode = "Payout",
            accountNo = payoutCommon.AccountNO,
            ifsc = payoutCommon.IFSC,
            bankName = payoutCommon.BankName,
            beneficiaryName = payoutCommon.Name,
            amount = payoutCommon.Amount,
            mobile = payoutCommon.Mobile,
            email = payoutCommon.MailID,
            apiPartnerRequestId = payoutCommon.OrderID,
            mode = payoutCommon.Mode
        };
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);


        Response.Request = RequestJson;

        string Result = await classypay.PostCall(RequestJson, "payment/v3/payout/transfer");


        JObject TransferData = JObject.Parse(Result);

        if (Convert.ToString(TransferData["status"]).ToUpper() != "FALSE")
        {
            string Status = TransferData["status"].ToString().ToUpper();
            if (Status == "SUCCESS")
            {
                Status = "SUCCESS";
            }
            else if (Status == "PENDING")
            {
                Status = "PENDING";
            }
            else if (Status == "FAILURE")
            {
                Status = "Failed";
            }
            else
            {
                Status = "PENDING";
            }
            Response.Status = Status;
            Response.Message = Convert.ToString(TransferData["message"]);
            Response.VendorID = Convert.ToString(TransferData["data"]?["txnId"].ToString());
            Response.TXNCode = Convert.ToString(TransferData["code"]);
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.Name;
            Response.RRN = Convert.ToString(TransferData["data"]?["rrn"].ToString());
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }
        else
        {

            string Status = "Failed";
            Response.Status = Status;
            Response.Message = Convert.ToString(TransferData["message"]);
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }


    public async Task<PayoutCommonResponse> SoniVerification(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();


        AccountVerifyST accountVerifySTS = new AccountVerifyST();
        accountVerifySTS.APIID = SONITechnoCredentrial.APIID;
        accountVerifySTS.Token = SONITechnoCredentrial.Token;
        accountVerifySTS.MethodName = "accountverify";
        accountVerifySTS.OrderID = payoutCommon.OrderID;
        accountVerifySTS.number = payoutCommon.AccountNO;
        accountVerifySTS.ifsc = payoutCommon.IFSC;

        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(accountVerifySTS);

        Response.Request = RequestJson;

        string Result = await ApiPostCall.PostCallAsync(Response.Request, SONITechnoCredentrial.MoneyURL);


        JObject TransferData = JObject.Parse(Result);

        if (TransferData["code"].ToString().ToUpper() == "TXN")
        {
            string Status = TransferData["data"]["Status"].ToString().ToUpper();


            Response.Status = Status;
            Response.Message = Convert.ToString(TransferData["mess"]);
            Response.VendorID = Convert.ToString(TransferData["data"]?["STID"]);
            Response.TXNCode = Convert.ToString(TransferData["code"]);
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.Name;
            Response.RRN = Convert.ToString(TransferData["data"]["RRN"]);
            Response.Result = Result;
            Response.Name =Convert.ToString(TransferData["data"]["Name"]);
            Response.OrderID = payoutCommon.OrderID;

        }
        else
        {

            string Status = "Failed";
            Response.Status = Status;
            Response.Message = Convert.ToString(TransferData["mess"]);
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }

    public async Task<PayoutCommonResponse> ClassypayVerification(PayoutCommon payoutCommon)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();


        object Requst = new
        {
            operatorCode = "BANK_VERIFY",
            accountNo = payoutCommon.AccountNO,
            ifsc = payoutCommon.IFSC,
            name = "Ram Soni",
            apiPartnerRequestId = payoutCommon.OrderID,
        };
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);


        Response.Request = RequestJson;

        string Result = await classypay.PostCall(RequestJson, "payment/v3/verify/bank-account");


        JObject TransferData = JObject.Parse(Result);

        if (Convert.ToString(TransferData["status"]).ToUpper() == "SUCCESS" && TransferData["success"]==null)
        {
            string Status = TransferData["status"].ToString().ToUpper();


            Response.Status = Status;
            Response.Message = Convert.ToString(TransferData["message"]);
            Response.VendorID = payoutCommon.OrderID;
            Response.TXNCode = Convert.ToString(TransferData["code"]);
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.Name;
            Response.RRN = Convert.ToString(TransferData["data"]["matchingScore"]);
            Response.Result = Result;
            Response.Name = Convert.ToString(TransferData["data"]["beneficiaryName"]);
            Response.OrderID = payoutCommon.OrderID;

        }
        else
        {

            string Status = "Failed";
            Response.Status = Status;
            Response.Message = Convert.ToString(TransferData["message"]);
            Response.VendorID = "";
            Response.TXNCode = "";
            Response.AccountNo = payoutCommon.AccountNO;
            Response.IFSC = payoutCommon.IFSC;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = payoutCommon.Name;
            Response.OrderID = payoutCommon.OrderID;
        }

        return Response;
    }
}