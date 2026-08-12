using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for PaywordAEPS
/// </summary>
public class PaywordAEPS
{
    public static string BodyKey = "=";
    public static string HeaderKey = "1Vx1IGJMp/=";

}


public class PaywordAEPSRequestHeader
{
    public string merchantId { get; set; }
    public string Timestamp { get; set; }
    public string merchantKey { get; set; }

    public PaywordAEPSRequestHeader()
    {
        merchantId = "623337251";
        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        merchantKey = "kCPoVGZ3hBpH3yqJRE6xTtqoPcP8GKWk3h3jNHc4s2I=";
    }

    public string GetJson()
    {
        PaywordAEPSRequestHeader ObjData = new PaywordAEPSRequestHeader();
        ObjData.merchantId = this.merchantId;
        ObjData.Timestamp = this.Timestamp;
        ObjData.merchantKey = this.merchantKey;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class PaywordAEPSRequestBody
{
    public string AgentId { get; set; }
    public string merchantService { get; set; }
    public string Version { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }

    public PaywordAEPSRequestBody()
    {       
        merchantService = "AEPS";
        Version = "1.0";
        Mobile = "";
        Email = "";
    }
    public string GetJson()
    {
        PaywordAEPSRequestBody ObjData = new PaywordAEPSRequestBody();
        ObjData.AgentId = this.AgentId;
        ObjData.merchantService = this.merchantService;
        ObjData.Version = this.Version;
        ObjData.Mobile = this.Mobile;
        ObjData.Email = this.Email;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class PaywordAEPSSendRequest
{
    public string enc_parameters { get; set; }
    public string enc_header { get; set; }
    public string GetJson()
    {
        PaywordAEPSSendRequest ObjData = new PaywordAEPSSendRequest();
        ObjData.enc_parameters = this.enc_parameters;

        ObjData.enc_header = this.enc_header;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class CheckStatusCardnumberORUIDForCW
{
    public string adhaarNumber { get; set; }
    public string nationalBankIdentificationNumber { get; set; }
}

public class CheckStatusForCWRoot
{
    public CheckStatusCardnumberORUIDForCW cardnumberORUID { get; set; }
    public string mobileNumber { get; set; }
    public string timestamp { get; set; }
    public string transactionType { get; set; }
    public string agentId { get; set; }
    public string transactionAmount { get; set; }
    public string merchantTransactionId { get; set; }
    public string deviceName { get; set; }
}

public class ReturnVerifyDataAfterFirstCallBack
{
    public string mobileNumber { get; set; }
    public string timestamp { get; set; }
    public string transactionType { get; set; }
    public string agentId { get; set; }
    public string merchantTransactionId { get; set; }
    public string transactionId { get; set; }
    public string status { get; set; }
    public string failure_desc { get; set; }
    public string description { get; set; }

    public string GetJson()
    {
        ReturnVerifyDataAfterFirstCallBack ObjData = new ReturnVerifyDataAfterFirstCallBack();
        ObjData.mobileNumber = this.mobileNumber;
        ObjData.timestamp = this.timestamp;
        ObjData.transactionType = this.transactionType;
        ObjData.agentId = this.agentId;
        ObjData.merchantTransactionId = this.merchantTransactionId;
        ObjData.transactionId = this.transactionId;
        ObjData.status = this.status;
        ObjData.failure_desc = this.failure_desc;
        ObjData.description = this.description;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class ReturnVerifyDataAfterFirstCallBackRequest
{
    public string verify_data { get; set; }
    public string GetJson()
    {
        ReturnVerifyDataAfterFirstCallBackRequest ObjData = new ReturnVerifyDataAfterFirstCallBackRequest();
        ObjData.verify_data = this.verify_data;


        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;

    }
}


public class LastCallBAckAEPS
{
    public string merchantTransactionId { get; set; }
    public string transactionId { get; set; }
    public string aepsmode { get; set; }
    public string timestamp { get; set; }
    public string terminalid { get; set; }
    public string agentid { get; set; }
    public string bcname { get; set; }
    public string bclocation { get; set; }
    public string aadhar { get; set; }
    public string rrn { get; set; }
    public string stan { get; set; }
    public string mATMReqId { get; set; }
    public string uidaiauthcode { get; set; }
    public string TxnStatus { get; set; }
    public string txnamt { get; set; }
    public string accountbal { get; set; }
    public string responsemsg { get; set; }
    public string statusCode { get; set; }
}

public class CheckStatusRequestAEPS
{
    public string merchantTransactionId { get; set; }
    public string merchantService { get; set; }
    public string Version { get; set; }

    public string GetJson()
    {
        CheckStatusRequestAEPS ObjData = new CheckStatusRequestAEPS();
        ObjData.merchantTransactionId = this.merchantTransactionId;
        ObjData.merchantService = "AEPS";
        ObjData.Version = "1.0";


        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;

    }
}

public class StatusResultAEPS
{
    public string date { get; set; }
    public string retailerId { get; set; }
    public string providerCode { get; set; }
    public string providerMessage { get; set; }
    public int amount { get; set; }
    public string aadhar { get; set; }
    public string merchantTransactionId { get; set; }
    public string transactionId { get; set; }
    public string status { get; set; }
}


public class ApiPostCallPayWord
{

    public static string PostCall(string Json, string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(url, Method.Post);

        request.AddHeader("Accept", "application/json");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);

        var response = client.Execute(request);




        return response.Content.ToString();
    }
    public static string GetCall(string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        using (WebClient webClient = new WebClient())
        {
            webClient.Headers.Add("Content-Type", "application/json");
            webClient.Headers["Authorization"] = "7794bda5-8d4e-4782-84a0-2af4ef05bb6f";
            var json = webClient.DownloadString("https://partners.hypto.in/api/transfers/status/3432422222");
            return json.ToString();
        }
    }
}