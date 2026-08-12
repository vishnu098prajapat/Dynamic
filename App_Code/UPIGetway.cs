using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for UPIGetway
/// </summary>
public class UPIGetway
{
    public static readonly string token = "";
    public static readonly string Redirect = "https://masabi.zendesk.com/hc/article_attachments/360008226974/360006195154.png";
    public static readonly string Url = "https://merchant.upigateway.com/api/";

    public UPIGetway()
    {
        //
        // TODO: Add constructor logic here
        //
    }


    public static async Task<string> PostCall(string Request,string MName)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        string BaseURL = UPIGetway.Url + MName;
        var options = new RestClientOptions(BaseURL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(BaseURL, Method.Post);
        request.AddParameter("application/json", Request, ParameterType.RequestBody);
        RestResponse response =await client.ExecuteAsync(request);
        return response.Content;
    }
}


