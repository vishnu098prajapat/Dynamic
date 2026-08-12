using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for tezindia
/// </summary>
public class tezindia
{
    public static readonly string token = "";
    public static readonly string Redirect = "https://masabi.zendesk.com/hc/article_attachments/360008226974/360006195154.png";
    public static readonly string Url = "https://upi.tezindia.in/api/";

    public tezindia()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static async Task<string> PostCall(string Amount, string MName)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions("https://upi.tezindia.in")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/api/create-order", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Cookie", "ASP.NET_SessionId=adbsf1zofudbagdz4hpjli5q");
        request.AddParameter("customer_mobile", "8005575599");
        request.AddParameter("user_token", tezindia.token);
        request.AddParameter("amount", Amount);
        request.AddParameter("order_id", MName);
        request.AddParameter("redirect_url", tezindia.Redirect);
        request.AddParameter("remark1", "testremark,");
        request.AddParameter("remark2", "");
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static async Task<string> PostCallCheckStatus(string order_id, string MName)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions("https://upi.tezindia.in")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/api/check-order-status", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Cookie", "ASP.NET_SessionId=adbsf1zofudbagdz4hpjli5q");
        request.AddParameter("user_token", tezindia.token);
        request.AddParameter("order_id", order_id);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }
}