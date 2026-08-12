using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for Aeronpay
/// </summary>
public class Aeronpay
{
    public readonly static string accountNumber = "";
    public readonly static string merchant_id = "";
    public readonly static string ClientID = "";
    public readonly static string Secret = "";
    public readonly static string URL = "https://superprodapi.aeronpay.in/api/core-services/serviceapi-prod/finance/securepay/";
    public Aeronpay()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static async Task<String> GetPostcall(string Body,string Methods)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        string BaseURL = $"{Aeronpay.URL}{Methods}";
        var options = new RestClientOptions(BaseURL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(BaseURL, Method.Post);
        request.AddHeader("accept", "application/json");
        request.AddHeader("client-id", Aeronpay.ClientID);
        request.AddHeader("client-secret", Aeronpay.Secret);
        request.AddHeader("content-Type", "application/json");
       
        request.AddStringBody(Body, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }
}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class BeneDetails
{
    public string bankAccount { get; set; }
    public string ifsc { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string address1 { get; set; }
}


public class BeneDetailsUPI
{
    public string vpa { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string address1 { get; set; }
}

public class AeronpayRequest
{
    public string amount { get; set; }
    public string client_referenceId { get; set; }
    public string transferMode { get; set; }
    public string bankid { get; set; }
    public BeneDetails beneDetails { get; set; }
    public string bankProfileId { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string remarks { get; set; }
    public string accountNumber { get; set; }
    public AeronpayRequest()
    {
        bankProfileId = "1";
        accountNumber = Aeronpay.merchant_id;
    }
}



public class AeronpayRequestAUPI
{
    public string amount { get; set; }
    public string client_referenceId { get; set; }
    public string transferMode { get; set; }
    public BeneDetailsUPI beneDetails { get; set; }
    public string bankProfileId { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string remarks { get; set; }
    public string accountNumber { get; set; }
    public AeronpayRequestAUPI()
    {
        bankProfileId = "1";
        accountNumber = Aeronpay.merchant_id;
    }
}