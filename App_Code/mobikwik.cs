using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for mobikwik
/// </summary>
public class mobikwik
{
    public readonly static string userid = "";
    public readonly static string pass = "";
    public readonly static string MClient = "";
    public readonly static string BalanceFetch = "https://rapi.mobikwik.com/recharge/v1/retailerBalance";
    public readonly static string BillFetch = "https://rapi.mobikwik.com/retailer/v2/retailerViewbill";

    public mobikwik()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public async static Task<string> GetPostCall(string RequestJ,string Url)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(Url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(Url, Method.Post);
        request.AddHeader("X-MClient", mobikwik.MClient);
        request.AddHeader("Content-Type", "application/json");
        
        request.AddStringBody(RequestJ, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return (response.Content);
    }


    public async static Task<string> GetGetCall(string Url)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(Url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(Url, Method.Get);
        RestResponse response = await client.ExecuteAsync(request);
        return (response.Content);
    }
}