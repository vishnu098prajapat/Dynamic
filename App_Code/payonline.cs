using NewSoniTechno.App_Code.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for payonline
/// </summary>
public class payonline
{
    public static readonly string token = "";
    public static readonly string APIUserId = "";
    public static readonly string Mobile = "";
    public static async Task<string> PostCall(string baseurl)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        
        var options = new RestClientOptions(baseurl)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(baseurl, Method.Get);
        RestResponse response = await client.ExecuteAsync(request);
        return (response.Content);
    }
}