using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for verifya2z
/// </summary>
public class verifya2z
{
    public static string JWTencryptionKey = "";
    public static string AuthKey = "";
    public static string AESKEY = "";
    public static string AESIV = "";
    public static string PARTNERID = "";
    public static string Prefix = "";
    public static string Authorisedsign = "";
    public static string URL = "https://api.verifya2z.com/api/v1/verification/";
    public static string OnBoradCall = "";
    public verifya2z()
    {
        //
        // TODO: Add constructor logic here
        //
    }


    public static async Task<string> PostCallWithBody(string url, string Token, string body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        string baseurl = verifya2z.URL + url;

        var options = new RestClientOptions(baseurl)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(baseurl, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Token", Token);
        request.AddHeader("authorisedkey", verifya2z.AuthKey);
        request.AddHeader("User-Agent", verifya2z.PARTNERID);
        request.AddStringBody(body, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return (response.Content);
    }
}