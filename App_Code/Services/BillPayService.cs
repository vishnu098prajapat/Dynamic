using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using NewSoniTechno.App_Code.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
/// <summary>
/// Summary description for BillPayService
/// </summary>
public class BillPayService : IBillPayService
{
    cls_connection cls;
    IServiceList serviceList;
    IWallet wallet;
    ICommonService commonService;
    ICommissionService commission;

    public BillPayService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commonService = new CommonService();
        commission = new CommissionService();
    }

    public async Task<string> BillFetch(BillFtech billFtech)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(billFtech.ServiceID);
        if (serviceData.Status != "Success") return ErrorShow.ErrorMessageAPI(serviceData.Reason);

        String fetchapiid = await cls.select_data_scalar_stringAsync("SELECT BillFetch FROM RechargeSwitchingNewConcept  WHERE SwitchType='opeator' AND OPID=(SELECT TOP 1  ID FROM Operator WHERE OperatorCODE='" + billFtech.SPKey + "')");

        if (!String.IsNullOrWhiteSpace(fetchapiid))
        {
            serviceData.APIID = Convert.ToInt32(fetchapiid);
        }

        billFtech.ClientRefId = System.Guid.NewGuid().ToString().Replace("-", "");

        BillFtechReturnResponse billFtechReturnResponse = new BillFtechReturnResponse();
        if (serviceData.APIID == 1 || serviceData.APIID == 6)
        {
            billFtechReturnResponse = await BillFetchSoni(billFtech, "1");
        }
        else if (serviceData.APIID == 1013)// For Mobiwik
        {
            billFtechReturnResponse = await BillFetchMobikwik(billFtech, serviceData.APIID.ToString());
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("API Not Switch With Service");
        }

        try
        {
            await commonService.SaveLogs(new LogsRequestModel() { ServiceID = billFtech.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = billFtech.ClientRefId, RequestData = billFtechReturnResponse.Request, ResponseData = billFtechReturnResponse.Result, CallBackData = "" });
        }
        catch { };

        return billFtechReturnResponse?.status == true
            ? ErrorShow.SuccessArrayAPI(Newtonsoft.Json.JsonConvert.SerializeObject(billFtechReturnResponse))
            : ErrorShow.ErrorMessageAPI(billFtechReturnResponse?.message);

    }

    public async Task<string> BillPay(BillFtech billFtech, string MemberID)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(billFtech.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }


        //String fetchapiid = await cls.select_data_scalar_stringAsync("SELECT MULTIPLE_SWITCH FROM RechargeSwitchingNewConcept  WHERE SwitchType='opeator' AND OPID=(SELECT TOP 1  ID FROM Operator WHERE OperatorCODE='" + billFtech.SPKey + "')");

        //if (!String.IsNullOrWhiteSpace(fetchapiid))
        //{
        //    serviceData.APIID = Convert.ToInt32(fetchapiid);
        //}



        // Get member details
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (memberDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Not Allow. Contact Admin.");
        }

        string OpID = await cls.select_data_scalar_stringAsync("SELECT TOP 1  ID FROM Operator WHERE OperatorCODE='" + billFtech.SPKey + "'");



        string activeApi = "0", authApi = "0";
        var service = new RechargeSwitchingService();

        service.ProcessRecharge(memberDetails.Msrno, Convert.ToInt32(OpID), memberDetails.PackgeID, Convert.ToDecimal(billFtech.Amount), out activeApi, out authApi);
        if (!string.IsNullOrEmpty(activeApi))
        {
            serviceData.APIID = Convert.ToInt32(activeApi);
        }


        // Initialize variables
        decimal myBalance = memberDetails.MainBalance;
        string agentID = await wallet.GetOrderID();
        billFtech.ClientRefId = agentID;

        // Step 5: Get surcharge and commission details
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
            memberDetails.PackgeID.ToString(),
           Convert.ToDecimal(billFtech.Amount),
            serviceData.APIID.ToString(),
            billFtech.ServiceID, billFtech.SPKey
        );


        //if (item.surcharge == 0)
        //return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


        // Check if the balance is sufficient
        if (myBalance < (Convert.ToDecimal(billFtech.Amount) + memberDetails.HoldAMT)) // Add hold amount
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }




        // Create narration and description
        string Narration = $"{billFtech.Number}/{billFtech.Name}/{billFtech.Amount}/{billFtech.SPKey}/{agentID}";
        string Dec = "";


        // Execute wallet transaction
        DataTable dtval = await ExecuteWalletTransaction(memberDetails.LoginID, Convert.ToDecimal(billFtech.Amount), Narration, Dec, agentID, 0, 0, billFtech.ServiceID);
        if (dtval.Rows.Count == 0 || dtval.Rows[0]["msrno"].ToString() != "1")
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }

        var valdt = await cls.select_data_dtAsync($"AddEditBBpsRecharge '{billFtech.Number}', {memberDetails.Msrno}, '{billFtech.ServiceID}', '{billFtech.TelecomCircleID}', '{agentID}', '{billFtech.Amount}', '{billFtech.SPKey}', '', '{serviceData.APIID}', '{billFtech.IPAddress}', '{billFtech.Source}', '', {(serviceData.APIID == 1016 ? 1 : 0)},{memberDetails.MainBalance},'{billFtech.Name}','{billFtech.DueDate}','{billFtech.Optional1}','{billFtech.Optional2}','{billFtech.Optional3}'");
        if (valdt.Rows.Count == 0)
        {
            await ProcessRefund(memberDetails, agentID, Narration, Convert.ToDecimal(billFtech.Amount), 0, 0, "Unable to Pay bill", billFtech.ServiceID);
        }

        BillPayReturnResponse billFtechReturnResponse = new BillPayReturnResponse();
        if (serviceData.APIID == 1)
        {
            billFtechReturnResponse = await BillPaySoni(billFtech, serviceData.APIID.ToString());
        }
        else if (serviceData.APIID == 6)
        {
            billFtechReturnResponse = await BillPayclassypay(billFtech, serviceData.APIID.ToString());
        }


        else
        {
            await ProcessRefund(memberDetails, agentID, Narration, Convert.ToDecimal(billFtech.Amount), 0, 0, "API Not Switch With Service", billFtech.ServiceID);
        }


        try
        {
            await commonService.SaveLogs(new LogsRequestModel() { ServiceID = billFtech.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = billFtech.ClientRefId, RequestData = billFtechReturnResponse.Request, ResponseData = billFtechReturnResponse.Result, CallBackData = "" });
        }
        catch { };

        if (billFtechReturnResponse.status)
        {
            Int32 val = await cls.update_dataAsync("update tblRecharge_History set status='Success',APIErrorCode='" + billFtechReturnResponse.OefID + "',APITransID='" + billFtechReturnResponse.RequestID + "' where TransID='" + agentID + "'");
            await commission.GiveBBPSCommission(agentID);
            string payRequest = Newtonsoft.Json.JsonConvert.SerializeObject(billFtechReturnResponse);
            return ErrorShow.SuccessArrayAPI(payRequest);
        }
        else
        {
            Int32 val = await cls.update_dataAsync("update tblRecharge_History set status='Failed',APIErrorCode='" + billFtechReturnResponse.message + "' where TransID='" + agentID + "'");
            return await ProcessRefund(memberDetails, agentID, Narration, Convert.ToDecimal(billFtech.Amount), 0, 0, billFtechReturnResponse.message, billFtech.ServiceID);
            //return ErrorShow.ErrorMessageAPI(billFtechReturnResponse.message);
        }

    }

    private async Task<string> ProcessRefund(MemberAllowData _memberAllowDetails, string agentID, string narration, decimal amount, decimal surcharge, decimal gst, string errorMessage, string ServiceID)
    {
        var dtval = await cls.select_data_dtAsync($"Exec [PROC_EWALLETTRANSACTION] '{_memberAllowDetails.LoginID}',{amount}, 'CR','Refund - {narration}','','{ServiceID}','{agentID}',0,0,0,'{surcharge}','{gst}'");

        if (dtval.Rows.Count > 0)
        {
            return ErrorShow.ErrorMessageAPI(errorMessage);
        }

        return ErrorShow.ErrorMessageAPI($"Transaction Failed due to {errorMessage}, But Amount Not Refunded in Wallet, Contact Admin");
    }

    private async Task<DataTable> ExecuteWalletTransaction(string loginID, decimal amount, string narration, string description, string agentID, decimal surcharge, decimal gst, string ServiceID)
    {
        return await cls.select_data_dtAsync($"exec PROC_EWALLETTRANSACTION '{loginID}','{amount}','DR','{narration}','{description}',{ServiceID},'{agentID}',0,0,0,'{surcharge}','{gst}'");
    }

    public async Task<OpDetails> FetchOPDetailsWithAPI(string OPCode, string APIID)
    {
        var opData = await cls.select_data_dtAsync("select *,(select name from service where id=Operator.ServiceID)ServiceName from Operator where Operatorcode='" + OPCode + "'");
        if (opData.Rows.Count > 0)
        {
            var opDataAPI = await cls.select_data_dtAsync("select * from tblRecharge_OperatorCode where OperatorID='" + opData.Rows[0]["ID"].ToString() + "' and APIID='" + APIID + "'");
            if (opDataAPI.Rows.Count > 0)
            {
                return new OpDetails { Max = Convert.ToDecimal(opData.Rows[0]["MaxVal"].ToString()), Min = Convert.ToDecimal(opData.Rows[0]["MinVal"].ToString()), Status = true, Message = "Fine", Optional1 = opDataAPI.Rows[0]["Optional1"].ToString(), Optional2 = opDataAPI.Rows[0]["Optional2"].ToString(), APIOPCode = opDataAPI.Rows[0]["OperatorCode"].ToString(), OPID = opData.Rows[0]["ID"].ToString().ToInt32(), IsPending = opData.Rows[0]["IsPending"].ToString().ToBoolen(), OpName = opData.Rows[0]["Name"].ToString(), ServiceName = opData.Rows[0]["ServiceName"].ToString() };
            }
            else
                return new OpDetails { Status = false, Message = "Operator Code Not Set" };

        }
        else
            return new OpDetails { Status = false, Message = "Operator Not Found" };
    }

    public async Task<OpDetails> FetchOPDetails(string OPCode)
    {
        var opData = await cls.select_data_dtAsync("select * from Operator where Operatorcode='" + OPCode + "'");
        if (opData.Rows.Count > 0)
        {
            return new OpDetails { Status = true, Message = "Fine", APIOPCode = "", OPID = opData.Rows[0]["ID"].ToString().ToInt32(), ServiceID = opData.Rows[0]["ServiceID"].ToString().ToInt32() };
        }
        else
            return new OpDetails { Status = false, Message = "Operator Not Found" };
    }


    public async Task<BillFtechReturnResponse> BillFetchSoni(BillFtech billFtech, string APIID)
    {
        BillFtechReturnResponse billFtechReturnResponse = new BillFtechReturnResponse();

        var opdetilas = await FetchOPDetailsWithAPI(billFtech.SPKey, APIID);
        if (!opdetilas.Status)
        {
            billFtechReturnResponse.status = false;
            billFtechReturnResponse.message = opdetilas.Message;
            return billFtechReturnResponse;
        }

        BillFetchRequest billFetchRequest = new BillFetchRequest
        {
            TelecomCircleID = "0",
            ClientRefId = billFtech.ClientRefId,
            Number = billFtech.Number,
            SPKey = opdetilas.APIOPCode,
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token,
            MethodName = "billfetch",
            Optional1 = billFtech.Optional1,
            Optional2 = billFtech.Optional2,
            Optional3 = billFtech.Optional3,
            Optional4 = billFtech.Optional4,
            Optional5 = billFtech.Optional5,
            Optional6 = billFtech.Optional6,
            Optional7 = billFtech.Optional7,
            Optional8 = billFtech.Optional8,
            Optional9 = billFtech.Optional9
        };

        // Serialize the BillFetchRequest object to JSON
        string payRequest = Newtonsoft.Json.JsonConvert.SerializeObject(billFetchRequest);
        billFtechReturnResponse.Request = payRequest;
        // Make the API call asynchronously
        string result = await ApiPostCall.PostCallAsync(payRequest, SONITechnoCredentrial.BBPS);
        JObject dd = JObject.Parse(result);
        //{"code":"ERR","status":"failed","mess":"Payment received for the billing period - no bill due"}
        // Deserialize the API response

        if (dd["code"].ToString() != "ERR")
        {
            BillFetchResult bsBenBindData = JsonConvert.DeserializeObject<BillFetchResult>(dd["data"].ToString());

            if (bsBenBindData?.ResponseCode == "000")
            {
                // Populate the success response
                billFtechReturnResponse = new BillFtechReturnResponse
                {
                    status = true,
                    message = bsBenBindData.ResponseMessage,
                    dueamount = bsBenBindData.dueamount.ToString() ?? "0",
                    duedate = bsBenBindData.duedate,
                    customername = bsBenBindData.customername,
                    billnumber = bsBenBindData.billnumber,
                    minamount = opdetilas.Min.ToString(),
                    maxamount = opdetilas.Max.ToString(),
                    billdate = bsBenBindData.billdate,
                    acceptPartPay = bsBenBindData.acceptPartPay,
                    BBPSCharges = bsBenBindData.BBPSCharges,
                    BillUpdate = bsBenBindData.BillUpdate,
                    Result = result,
                    RequestID = bsBenBindData.RequestID,
                    ClientRefId = bsBenBindData.ClientRefId,
                    BBPSComm = "0",
                    billValidationId = ""
                };
            }
            else
            {
                // Populate the failure response
                billFtechReturnResponse.status = false;
                billFtechReturnResponse.message = bsBenBindData?.ResponseMessage ?? "Unknown error occurred.";
            }
        }
        else
        {
            // Populate the failure response
            billFtechReturnResponse.status = false;
            billFtechReturnResponse.message = dd["mess"].ToString();
        }

        return billFtechReturnResponse;
    }
    public async Task<BillPayReturnResponse> BillPaySoni(BillFtech billFtech, string APIID)
    {
        BillPayReturnResponse billFtechReturnResponse = new BillPayReturnResponse();

        var opdetilas = await FetchOPDetailsWithAPI(billFtech.SPKey, APIID);
        if (!opdetilas.Status)
        {
            return new BillPayReturnResponse
            {
                status = false,
                message = opdetilas.Message
            };
        }


        if (opdetilas.IsPending)
        {
            return new BillPayReturnResponse
            {
                status = true,
                Rstatus = "Success",
                message = "Bill Pay Successfully",
                customername = billFtech.Name,
                billnumber = billFtech.Number,
                billdate = billFtech.BillDate,
                BBPSCharges = billFtech.ServiceCharge,
                Result = "",
                RequestID = "",
                OpName = opdetilas.OpName,
                ServiceName = opdetilas.ServiceName,
                ClientRefId = billFtech.ClientRefId
            };
        }


        var billFetchRequest = new BillPayRequest
        {
            TelecomCircleID = billFtech.TelecomCircleID,
            ClientRefId = billFtech.ClientRefId,
            Number = billFtech.Number,
            Amount = billFtech.Amount,
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token,
            MethodName = "billpay",
            SPKey = opdetilas.APIOPCode,
            Optional1 = billFtech.Optional1,
            Optional2 = billFtech.Optional2,
            Optional3 = billFtech.Optional3,
            Optional4 = billFtech.Optional4,
            Optional5 = billFtech.Optional5,
            Optional6 = billFtech.Optional6,
            Optional7 = billFtech.Optional7,
            Optional8 = billFtech.Optional8,
            Optional9 = billFtech.Optional9
        };

        // Serialize the BillFetchRequest object to JSON
        string payRequest = Newtonsoft.Json.JsonConvert.SerializeObject(billFetchRequest);

        // Make the API call asynchronously
        string result = await ApiPostCall.PostCallAsync(payRequest, SONITechnoCredentrial.BBPS);
        JObject dd = JObject.Parse(result);
        //{"code":"ERR","status":"failed","mess":"Payment received for the billing period - no bill due"}
        // Deserialize the API response
        string code = dd["code"]?.ToString();
        string status = dd["data"]?["status"]?.ToString().ToUpper();
        string message = (dd["data"] != null ? dd["data"]["mess"].ToString() : dd["mess"]?.ToString());

        if (code != "ERR" && (status == "SUCCESS" || status == "PENDING"))
        {
            // Populate the success response
            billFtechReturnResponse = new BillPayReturnResponse
            {
                status = true,
                Rstatus = status,
                message = message,
                customername = billFtech.Name,
                billnumber = billFtech.Number,
                billdate = billFtech.BillDate,
                BBPSCharges = billFtech.ServiceCharge,
                Result = result,
                Request = payRequest,
                OpName = opdetilas.OpName,
                ServiceName = opdetilas.ServiceName,
                OefID = dd["data"]?["OperatorTransactionId"]?.ToString(),
                RequestID = dd["data"]?["TransactionID"]?.ToString(),
                ClientRefId = billFtech.ClientRefId,
            };

        }
        else
        {
            billFtechReturnResponse.Result = result;
            billFtechReturnResponse.Request = payRequest;
            // Populate the failure response
            billFtechReturnResponse.status = false;
            billFtechReturnResponse.message = message;
        }

        return billFtechReturnResponse;
    }

    public async Task<BillPayReturnResponse> BillPayclassypay(BillFtech billFtech, string APIID)
    {
        BillPayReturnResponse billFtechReturnResponse = new BillPayReturnResponse();

        var opdetilas = await FetchOPDetailsWithAPI(billFtech.SPKey, APIID);
        if (!opdetilas.Status)
        {
            return new BillPayReturnResponse
            {
                status = false,
                message = opdetilas.Message
            };
        }


        if (opdetilas.IsPending)
        {
            return new BillPayReturnResponse
            {
                status = true,
                Rstatus = "Success",
                message = "Bill Pay Successfully",
                customername = billFtech.Name,
                billnumber = billFtech.Number,
                billdate = billFtech.BillDate,
                BBPSCharges = billFtech.ServiceCharge,
                Result = "",
                RequestID = "",
                OpName = opdetilas.OpName,
                ServiceName = opdetilas.ServiceName,
                ClientRefId = billFtech.ClientRefId
            };
        }


        var billFetchRequest = new
        {
            @event = "BBPS",
            customerNumber = billFtech.Number,
            operatorCode = opdetilas.APIOPCode,
            amount = billFtech.Amount,
            apiPartnerRequestId = billFtech.ClientRefId,
            v1 = billFtech.Optional1,
            v2 = billFtech.Optional2,
            v3 = billFtech.Optional3,
            v4 = billFtech.Optional4
        };

        // Serialize the BillFetchRequest object to JSON
        string payRequest = Newtonsoft.Json.JsonConvert.SerializeObject(billFetchRequest);

    // Make the API call asynchronously
    string result = await classypay.PostCall(payRequest, "payment/v3/recharge");
    JObject dd = JObject.Parse(result);
    //{"code":"ERR","status":"failed","mess":"Payment received for the billing period - no bill due"}
    // Deserialize the API response
    string code = dd["code"]?.ToString();
    string status = dd["status"]?.ToString().ToUpper();
    string message = (dd["message"]?.ToString());

        if ((status == "SUCCESS" || status == "PENDING"))
        {
            // Populate the success response
            billFtechReturnResponse = new BillPayReturnResponse
            {
                status = true,
                Rstatus = status,
                message = message,
                customername = billFtech.Name,
                billnumber = billFtech.Number,
                billdate = billFtech.BillDate,
                BBPSCharges = billFtech.ServiceCharge,
                Result = result,
                Request = payRequest,
                OpName = opdetilas.OpName,
                ServiceName = opdetilas.ServiceName,
                OefID = dd["data"]?["apiPartnerRequestId"]?.ToString(),
                RequestID = dd["data"]?["apiPartnerRequestId"]?.ToString(),
                ClientRefId = billFtech.ClientRefId,
            };

        }
        else
        {
            billFtechReturnResponse.Result = result;
            billFtechReturnResponse.Request = payRequest;
            // Populate the failure response
            billFtechReturnResponse.status = false;
            billFtechReturnResponse.message = message;
        }

        return billFtechReturnResponse;
    }


    public async Task<BillPayReturnResponse> BillPayMobiwik(BillFtech billFtech, string APIID)
{
    BillPayReturnResponse billFtechReturnResponse = new BillPayReturnResponse();

    var opdetilas = await FetchOPDetailsWithAPI(billFtech.SPKey, APIID);
    if (!opdetilas.Status)
    {
        return new BillPayReturnResponse
        {
            status = false,
            message = opdetilas.Message
        };
    }


    if (opdetilas.IsPending)
    {
        return new BillPayReturnResponse
        {
            status = true,
            Rstatus = "Success",
            message = "Bill Pay Successfully",
            customername = billFtech.Name,
            billnumber = billFtech.Number,
            billdate = billFtech.BillDate,
            BBPSCharges = billFtech.ServiceCharge,
            Result = "",
            RequestID = "",
            OpName = opdetilas.OpName,
            ServiceName = opdetilas.ServiceName,
            ClientRefId = billFtech.ClientRefId
        };
    }

    dynamic adParams = new ExpandoObject();
    var adParamsDict = (IDictionary<string, object>)adParams;

    if (!string.IsNullOrWhiteSpace(opdetilas.Optional1) && opdetilas.APIOPCode == "294" || opdetilas.APIOPCode == "978")
    {
        billFtech.Optional1 = await cls.select_data_scalar_stringAsync("select top 1 [values] from bbpsdatadown where spkey='" + billFtech.SPKey + "'");
    }


    string urldd = "https://rapi.mobikwik.com/recharge.do?uid=" + mobikwik.userid + "&pwd=" + mobikwik.pass + "&cn=" + billFtech.Number + "&op=" + opdetilas.APIOPCode + "&cir=" + billFtech.TelecomCircleID + "&amt=" + billFtech.Amount + "&reqid=" + billFtech.ClientRefId + "&ad1=" + billFtech.Optional1 + "";

    // Make the API call asynchronously
    string result = await mobikwik.GetGetCall(urldd);

    XmlDocument doc = new XmlDocument();
    doc.LoadXml(result);

    string jsonText = JsonConvert.SerializeXmlNode(doc);

    JObject dd = JObject.Parse(jsonText);
    //{"code":"ERR","status":"failed","mess":"Payment received for the billing period - no bill due"}
    // Deserialize the API response
    string code = "";
    string status = "PENDING";
    string message = "";

    if (dd["txStatus"] == null)
    {
        status = dd["recharge"]["status"].ToString().ToUpper();
        message = status;

        if (status == "")
        {
            message = dd["recharge"]["errorMsg"].ToString();
            code = "ERR";
            status = "FAILED";
        }
        else
        {
            if (dd["recharge"]["errorMsg"] != null)
            {
                message = dd["recharge"]["errorMsg"].ToString();
            }
        }
    }
    else
    {
        code = "ERR";
        status = "FAILED";
        message = dd["txStatus"]["errorMsg"].ToString();
    }

    if (code != "ERR" && (status == "SUCCESS" || status == "PENDING" || status == "SUCCESSPENDING"))
    {
        // Populate the success response
        billFtechReturnResponse = new BillPayReturnResponse
        {
            status = true,
            Rstatus = status,
            message = message,
            customername = billFtech.Name,
            billnumber = billFtech.Number,
            billdate = billFtech.BillDate,
            BBPSCharges = billFtech.ServiceCharge,
            Result = result,
            Request = urldd,
            OpName = opdetilas.OpName,
            ServiceName = opdetilas.ServiceName,
            OefID = dd["recharge"]?["opRefNo"]?.ToString(),
            RequestID = dd["recharge"]?["txId"]?.ToString(),
            ClientRefId = billFtech.ClientRefId,
        };

    }
    else
    {
        billFtechReturnResponse.Result = result;
        billFtechReturnResponse.Request = urldd;
        // Populate the failure response
        billFtechReturnResponse.status = false;
        billFtechReturnResponse.message = message;
    }

    return billFtechReturnResponse;
}

public async Task<BillFtechReturnResponse> BillFetchMobikwik(BillFtech billFtech, string APIID)
{
    BillFtechReturnResponse billFtechReturnResponse = new BillFtechReturnResponse();

    var opdetilas = await FetchOPDetailsWithAPI(billFtech.SPKey, APIID);

    if (!opdetilas.Status)
    {
        billFtechReturnResponse.status = false;
        billFtechReturnResponse.message = opdetilas.Message;
        return billFtechReturnResponse;
    }

    dynamic adParams = new ExpandoObject();
    var adParamsDict = (IDictionary<string, object>)adParams;

    if (!string.IsNullOrWhiteSpace(opdetilas.Optional1))
    {
        adParamsDict[opdetilas.Optional1] = billFtech.Optional1;
        if (string.IsNullOrWhiteSpace(billFtech.Optional1))
        {
            adParamsDict[opdetilas.Optional1] = await cls.select_data_scalar_stringAsync("select top 1 [values] from bbpsdatadown where spkey='" + billFtech.SPKey + "'");
        }
    }



    if (!string.IsNullOrWhiteSpace(opdetilas.Optional2))
    {
        adParamsDict[opdetilas.Optional2] = billFtech.Optional2;
    }



    object Requst = new
    {
        adParams = adParamsDict.Count > 0 ? adParams : new ExpandoObject(),
        uid = mobikwik.userid,
        pswd = mobikwik.pass,
        cir = billFtech.TelecomCircleID,
        cn = billFtech.Number,
        op = opdetilas.APIOPCode
    };

    // Serialize the BillFetchRequest object to JSON
    string payRequest = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
    billFtechReturnResponse.Request = payRequest;

    // Make the API call asynchronously
    string result = await mobikwik.GetPostCall(payRequest, mobikwik.BillFetch);

    JObject dd = JObject.Parse(result);
    if (dd["success"].ToString().ToUpper() == "TRUE")
    {
        string Json = "{\"ResponseCode\":\"000\",\"ResponseMessage\":\"Transaction Successful\",\"dueamount\": \"" + dd["data"][0]["billnetamount"].ToString() + "\",\"duedate\":\"" + (dd["data"][0]["dueDate"] != null ? dd["data"][0]["dueDate"].ToString() : "") + "\",\"customername\": \"" + dd["data"][0]["userName"].ToString() + "\",\"billnumber\": \"" + billFtech.Number + "\",\"billdate\": \"\",\"acceptPartPay\": \"N\",\"BBPSCharges\": \"0.00\",\"BillUpdate\": \"T + 1\",\"RequestID\":\"4000000508814032\",\"ClientRefId\": \"555558565688\",\"BBPSComm\": \"0.00\"}";
        // Populate the success response
        billFtechReturnResponse = new BillFtechReturnResponse
        {
            status = true,
            message = "Success",
            dueamount = dd["data"][0]["billnetamount"].ToString(),
            duedate = (dd["data"][0]["dueDate"] != null ? dd["data"][0]["dueDate"].ToString() : ""),
            customername = dd["data"][0]["userName"].ToString(),
            billnumber = billFtech.Number,
            minamount = opdetilas.Min.ToString(),
            maxamount = opdetilas.Max.ToString(),
            billdate = (dd["data"][0]["billdate"] != null ? dd["data"][0]["billdate"].ToString() : ""),
            acceptPartPay = (dd["data"][0]["acceptPartPay"] != null ? dd["data"][0]["acceptPartPay"].ToString() : ""),
            BBPSCharges = "0",
            BillUpdate = "",
            Result = result,
            RequestID = billFtech.ClientRefId,
            ClientRefId = billFtech.ClientRefId,
            BBPSComm = "0",
            billValidationId = ""
        };
    }
    else
    {
        // Populate the failure response
        billFtechReturnResponse.status = false;
        billFtechReturnResponse.message = dd["message"]["text"].ToString();
    }


    return billFtechReturnResponse;
}


public async Task<BillPayReturnResponse> BillPaypayonline(BillFtech billFtech, string APIID)
{
    BillPayReturnResponse billFtechReturnResponse = new BillPayReturnResponse();

    var opdetilas = await FetchOPDetailsWithAPI(billFtech.SPKey, APIID);
    if (!opdetilas.Status)
    {
        return new BillPayReturnResponse
        {
            status = false,
            message = opdetilas.Message
        };
    }


    if (opdetilas.IsPending)
    {
        return new BillPayReturnResponse
        {
            status = true,
            Rstatus = "Success",
            message = "Bill Pay Successfully",
            customername = billFtech.Name,
            billnumber = billFtech.Number,
            billdate = billFtech.BillDate,
            BBPSCharges = billFtech.ServiceCharge,
            Result = "",
            RequestID = "",
            OpName = opdetilas.OpName,
            ServiceName = opdetilas.ServiceName,
            ClientRefId = billFtech.ClientRefId
        };
    }

    var billFetchRequest = new BillPayRequest
    {
        TelecomCircleID = billFtech.TelecomCircleID,
        ClientRefId = billFtech.ClientRefId,
        Number = billFtech.Number,
        Amount = billFtech.Amount,
        SPKey = opdetilas.APIOPCode,
        Optional1 = billFtech.Optional1,
        Optional2 = billFtech.Optional2,
        Optional3 = billFtech.Optional3,
        Optional4 = billFtech.Optional4,
        Optional5 = billFtech.Optional5,
        Optional6 = billFtech.Optional6,
        Optional7 = billFtech.Optional7,
        Optional8 = billFtech.Optional8,
        Optional9 = billFtech.Optional9
    };

    string baseurl = "http://e-payonline.in/Controller/ApiServices.asmx/APIRechargeProcessGet?amount=" + billFetchRequest.Amount + "&circle=" + billFtech.Optional1 + "&token=" + payonline.token + "&clientTransactionId=" + billFtech.ClientRefId + "&serviceCode=" + billFetchRequest.SPKey + "&mobileNo=" + billFtech.Number + "&userRegisterMobileNo=" + payonline.Mobile + "&otherParameter=" + billFtech.Optional1 + "&otherParameter1=" + billFtech.Optional2 + "";
    // Make the API call asynchronously
    string result = await payonline.PostCall(baseurl);

    JObject dd = JObject.Parse(result);
    //{"MessageStatus": true, "Message": "Recharge Process Successfully", "JsonResult": { "TransactionStatus": "2/1/0", "TransactionStatusMessage": "SUCCESS/FAIL/PENDING", "SystemTransactionId": "150","ClientTransactionId": "12345","OperaterId": "BR0004DV8CDY", "Balance": 19631.28,"RechargeAmount": 20,"RechargeMobileNo": "XXXXXXXXXX", "flag": 2},"Status": 200}
    // Deserialize the API response
    string code = "";
    string status = "PENDING";
    string message = "";

    if (Convert.ToBoolean(dd["MessageStatus"]))
    {
        status = dd["JsonResult"]["TransactionStatus"].ToString().ToUpper();
        message = dd["JsonResult"]["TransactionStatusMessage"].ToString().ToUpper();

        if (status == "1")
        {
            code = "ERR";
            status = "FAILED";
        }
        if (status == "0")
        {
            code = "TXN";
            status = "PENDING";
        }
        if (status == "2")
        {
            code = "TXN";
            status = "SUCCESS";
        }
    }
    else
    {
        code = "ERR";
        status = "FAILED";
        message = dd["Message"].ToString();
    }
    status = status.ToUpper();
    if (code != "ERR" && (status == "SUCCESS" || status == "PENDING" || status == "SUCCESSPENDING"))
    {
        // Populate the success response
        billFtechReturnResponse = new BillPayReturnResponse
        {
            status = true,
            Rstatus = status,
            message = message,
            customername = billFtech.Name,
            billnumber = billFtech.Number,
            billdate = billFtech.BillDate,
            BBPSCharges = billFtech.ServiceCharge,
            Result = result,
            Request = baseurl,
            OpName = opdetilas.OpName,
            ServiceName = opdetilas.ServiceName,
            OefID = dd["JsonResult"]?["OperaterId"]?.ToString(),
            RequestID = dd["JsonResult"]?["OperaterId"]?.ToString(),
            ClientRefId = billFtech.ClientRefId,
        };

    }
    else
    {
        billFtechReturnResponse.Result = result;
        billFtechReturnResponse.Request = baseurl;
        // Populate the failure response
        billFtechReturnResponse.status = false;
        billFtechReturnResponse.message = message;
    }

    return billFtechReturnResponse;
}

public async Task<BillPayReturnResponse> BillPayOffline(BillFtech billFtech, string APIID)
{
    BillPayReturnResponse billFtechReturnResponse = new BillPayReturnResponse();

    var opdetilas = await FetchOPDetailsWithAPI(billFtech.SPKey, APIID);
    if (!opdetilas.Status)
    {
        return new BillPayReturnResponse
        {
            status = false,
            message = opdetilas.Message
        };
    }



    return new BillPayReturnResponse
    {
        status = true,
        Rstatus = "Success",
        message = "Bill Pay Successfully",
        customername = billFtech.Name,
        billnumber = billFtech.Number,
        billdate = billFtech.BillDate,
        BBPSCharges = billFtech.ServiceCharge,
        Result = "",
        RequestID = billFtech.ClientRefId,
        OpName = opdetilas.OpName,
        ServiceName = opdetilas.ServiceName,
        ClientRefId = billFtech.ClientRefId
    };

}
}