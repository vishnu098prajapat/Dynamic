using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for mswiftRequest
/// </summary>
public class mswiftRequest
{
    public mswiftRequest()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}


public class BillPayResult
{
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public string TransactionId { get; set; }
    public string AvailableBalance { get; set; }
    public string ClientRefId { get; set; }
    public object OperatorTransactionId { get; set; }
}


public class PostGetCallMSWIFT
{
    public string PostCall(string Data)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(SONITechnoCredentrial.BBPS)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(SONITechnoCredentrial.BBPS,Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("JWTKey", "");
        request.AddHeader("APIID", "");
        //request.AddHeader("ClientID", "1000000031");
        //request.AddHeader("SecretKey", "ERTERTG!@#$242");
        //request.AddHeader("TokenID", "IDSH#124%342");
        request.AddParameter("application/json", Data, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }
    public string PostCallPay(string Data)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

       
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var options = new RestClientOptions(SONITechnoCredentrial.BBPS)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(SONITechnoCredentrial.BBPS, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("JWTKey", "65547E7A5D6471042978830DC59C1DC25A261A65D721CEF9FCDE40303890C574");
        request.AddHeader("APIID", "API70001");
        //request.AddHeader("ClientID", "1000000031");
        //request.AddHeader("SecretKey", "ERTERTG!@#$242");
        //request.AddHeader("TokenID", "IDSH#124%342");
        request.AddParameter("application/json",Data, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }
}