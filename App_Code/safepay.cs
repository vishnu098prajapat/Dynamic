using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for safepay
/// </summary>
public class safepay
{
    public static string MerchantId = "";
    public static string MerchantKey = "";
    public static string URL = "";
    public safepay()
    {
        //
        // TODO: Add constructor logic here
        //
    }


    public static async Task<string> PostCallWithBodyAsync(string body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("https://remittance.safexpay.com/agWalletAPI/v2/agg")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("https://remittance.safexpay.com/agWalletAPI/v2/agg", Method.Post);
        request.AddHeader("X-MClient", "14");
        request.AddHeader("Content-Type", "application/json");
        var Request = "{\"payload\": \""+body+"\", \"uId\": \""+ safepay.MerchantId + "\"}";
        request.AddStringBody(Request, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
       return response.Content.Replace("\r\n", "");
    }

}



// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Header
{
    public string operatingSystem { get; set; }
    public string sessionId { get; set; }
    public string version { get; set; }

    public Header()
    {
        this.operatingSystem = "android";
        this.sessionId = safepay.MerchantId;
        this.version = "1.0.0";
    }
}

public class PayOutBean
{
    public string mobileNo { get; set; }
    public decimal txnAmount { get; set; }
    public string accountNo { get; set; }
    public string ifscCode { get; set; }
    public string bankName { get; set; }
    public string accountHolderName { get; set; }
    public string txnType { get; set; }
    public string accountType { get; set; }
    public string emailId { get; set; }
    public string orderRefNo { get; set; }
    public int count { get; set; }
}

public class SafePayout
{
    public Header header { get; set; }
    public UserInfo userInfo { get; set; }
    public Transaction transaction { get; set; }
    public PayOutBean payOutBean { get; set; }

    public string GetJSON()
    {
        SafePayout signupOTPVerify = new SafePayout();
        signupOTPVerify.header = this.header;
        signupOTPVerify.userInfo = this.userInfo;
        signupOTPVerify.transaction = this.transaction;
        signupOTPVerify.payOutBean = this.payOutBean;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}

public class Transaction
{
    public string requestType { get; set; }
    public string requestSubType { get; set; }
    public int tranCode { get; set; }
    public decimal txnAmt { get; set; }
    public string id { get; set; }
    public decimal surChargeAmount { get; set; }
    public int txnCode { get; set; }
    public int userType { get; set; }

    public Transaction()
    {
        requestType = "WTW";
        requestSubType = "PWTB";
        tranCode = 0;
        id = safepay.MerchantId;
        surChargeAmount = 0;
        txnCode = 0;
        userType = 0;
    }
}

public class UserInfo
{
}

