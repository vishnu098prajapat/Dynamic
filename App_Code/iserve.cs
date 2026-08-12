using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for iserve
/// </summary>
public class iserve
{
    public static string username = "mavyapi2";


    public static string client_idPayout = "";
    public static string client_secretPayout = "";
    public static string TXNAPIPayout = "";
    public static string BeneverificationPayout = "";
    public static string AccountVerify = "";
    public static string TXNSTATUSPayout = "";

    //DMT
    public static string client_idDMT = "";
    public static string client_secretDMT = "";
    public static string TXNAPIDMT = "";
    public static string BeneverificationDMT = "";
    public static string TXNSTATUSDMT = "";
    public iserve()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static async Task<string> PostCallWithBodyAsync(string body, string URL)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("client_id", iserve.client_idPayout);
        request.AddHeader("client_secret", iserve.client_secretPayout);
        request.AddStringBody(body, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content.Replace("\r\n", "");
    }


    public static async Task<string> PostCallWithBodyDMTAsync(string body, string URL)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("client_id", iserve.client_idDMT);
        request.AddHeader("client_secret", iserve.client_secretDMT);
        request.AddStringBody(body, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content.Replace("\r\n", "");
    }

}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class PayoutRequestIserve
{
    public string beneName { get; set; }
    public string beneAccountNo { get; set; }
    public string beneifsc { get; set; }
    public string clientReferenceNo { get; set; }
    public Int32 amount { get; set; }
    public string fundTransferType { get; set; }
    public Int32 pincode { get; set; }
    public string custName { get; set; }
    public string custMobNo { get; set; }
    public string latlong { get; set; }
    public ulong benePhoneNo { get; set; }
    public string beneBankName { get; set; }
    public string custIpAddress { get; set; }
    public string paramA { get; set; }
    public string paramB { get; set; }

    public string GetJSON()
    {
        PayoutRequestIserve signupOTPVerify = new PayoutRequestIserve();
        signupOTPVerify.beneName = this.beneName;
        signupOTPVerify.beneAccountNo = this.beneAccountNo;
        signupOTPVerify.beneifsc = this.beneifsc;
        signupOTPVerify.clientReferenceNo = this.clientReferenceNo;
        signupOTPVerify.amount = this.amount;
        signupOTPVerify.fundTransferType = this.fundTransferType;
        signupOTPVerify.pincode = this.pincode;
        signupOTPVerify.custName = this.custName;
        signupOTPVerify.custMobNo = this.custMobNo;
        signupOTPVerify.latlong = this.latlong;
        signupOTPVerify.benePhoneNo = this.benePhoneNo;
        signupOTPVerify.beneBankName = this.beneBankName;
        signupOTPVerify.custIpAddress = this.custIpAddress;
        signupOTPVerify.paramA = this.paramA;
        signupOTPVerify.paramB = this.paramB;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}




// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class iServeDMT
{
    public string externalRefNumber { get; set; }
    public string requestedAmount { get; set; }
    public string accountNumber { get; set; }
    public string beneBankName { get; set; }
    public string beneMobileNumber { get; set; }
    public string beneName { get; set; }
    public string bankCode { get; set; }
    public string beneIfscCode { get; set; }
    public string transactionMode { get; set; }
    public string customerName { get; set; }
    public string customerMobileNumber { get; set; }
    public string pincode { get; set; }
    public string address { get; set; }
    public bool isHoldTransaction { get; set; }
    public string username { get; set; }
    public string customerIP { get; set; }
    public string latLong { get; set; }

    public string GetJSON()
    {
        iServeDMT signupOTPVerify = new iServeDMT();
        signupOTPVerify.externalRefNumber = this.externalRefNumber;
        signupOTPVerify.requestedAmount = this.requestedAmount;
        signupOTPVerify.accountNumber = this.accountNumber;
        signupOTPVerify.beneBankName = this.beneBankName;
        signupOTPVerify.beneMobileNumber = this.beneMobileNumber;
        signupOTPVerify.beneName = this.beneName;
        signupOTPVerify.bankCode = this.bankCode;
        signupOTPVerify.beneIfscCode = this.beneIfscCode;
        signupOTPVerify.transactionMode = this.transactionMode;
        signupOTPVerify.customerName = this.customerName;
        signupOTPVerify.customerMobileNumber = this.customerMobileNumber;
        signupOTPVerify.pincode = this.pincode;
        signupOTPVerify.address = this.address;
        signupOTPVerify.isHoldTransaction = this.isHoldTransaction;
        signupOTPVerify.username = this.username;
        signupOTPVerify.customerIP = this.customerIP;
        signupOTPVerify.latLong = this.latLong;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}




// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class AccountVerificationIserve
{
    public string beneName { get; set; }
    public string beneAccountNo { get; set; }
    public string beneifsc { get; set; }
    public long benePhoneNo { get; set; }
    public string clientReferenceNo { get; set; }
    public string latlong { get; set; }
    public int pincode { get; set; }
    public string custName { get; set; }
    public long custMobNo { get; set; }
    public string custIpAddress { get; set; }
    public string beneBankName { get; set; }
    public string paramA { get; set; }
    public string paramB { get; set; }
}

