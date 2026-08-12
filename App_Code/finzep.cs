using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for finzep
/// </summary>
public class finzep
{
    public static readonly int UserID = 123;
    public static readonly string Token = "";
    public static readonly int OutletID = 123;
    public finzep()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static async Task<string> PostCall(string body)
    {
        var options = new RestClientOptions("https://api.finzep.com/API/Payout")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("https://api.finzep.com/API/Payout", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Cookie", ".roundpay.fintech=CfDJ8Kyz9HlFGSFGp6lqKDZtATcQ7Qky3aw8w3iXHHFBXf%2BBE6CXkBv0IpsJpZrcWPKEIeWCf7SCtIHJYeK7aHHiFAzv0tJNDqiVNsluzhEpihQ078Alk07m8Lbr%2BETqW480AwjJYCEYS1wZKcmCfOleL6hsOJVqMcDX8Bg6pQG4ZZCT");

        request.AddStringBody(body, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return (response.Content);
    }
}


public class finzeppayout
{
    public string AccountNo { get; set; }
    public int AmountR { get; set; }
    public int BankID { get; set; }
    public string IFSC { get; set; }
    public string SenderMobile { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string BeneName { get; set; }
    public string BeneMobile { get; set; }
    public string APIRequestID { get; set; }
    public string SPKey { get; set; }
    public string WebHook { get; set; }
}

public class finzeppayoutRoot
{
    public int UserID { get; set; }
    public string Token { get; set; }
    public int OutletID { get; set; }
    public finzeppayout PayoutRequest { get; set; }

    public finzeppayoutRoot()
    {
        UserID = finzep.UserID;
        Token = finzep.Token;
        OutletID = finzep.OutletID;
    }
}
