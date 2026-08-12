using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for rechargkit
/// </summary>
public class rechargkit
{
    public static readonly string Authorization = "Bearer ";
    public static readonly string PayoutURL = "https://v2bapi.rechargkit.biz/rkitpayout/payoutTransfer";
    public static readonly string BalanceCheck = "https://v2bapi.rechargkit.biz/recharge/balanceCheck";
    public static readonly string CheckStatus = "https://v2bapi.rechargkit.biz/recharge/statusCheck";

    public rechargkit()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public async static Task<string> GetBalanceAsync()
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(rechargkit.BalanceCheck)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(rechargkit.BalanceCheck, Method.Get);
        request.AddHeader("Authorization", rechargkit.Authorization);
        //request.AddHeader("Content-Type", "application/json");
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;



    }

    public static async Task<string> PostCall(string BodyData)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(rechargkit.PayoutURL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(rechargkit.PayoutURL, Method.Post);
        request.AddHeader("Authorization", rechargkit.Authorization);
        request.AddHeader("Content-Type", "application/json");

        request.AddStringBody(BodyData, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static async Task<string> PostCallCheckStatus(string OrderID)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(rechargkit.CheckStatus)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(rechargkit.CheckStatus, Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Authorization", rechargkit.Authorization);
        request.AddParameter("partner_request_id", OrderID);
        RestResponse response = await client.ExecuteAsync(request);
        Console.WriteLine(response.Content);
        return response.Content;
    }
}