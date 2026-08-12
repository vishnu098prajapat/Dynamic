using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for classypay
/// </summary>
public class classypay
{
    private readonly static string Authorization = "Bearer eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiI3MCIsInR5cGUiOiJwYXJ0bmVyX2FwaSIsImlhdCI6MTc4NDY5NzA3NywiZXhwIjoxOTQyNDYzNDc3LCJpcCI6IjE1Ny4xMTkuNDMuMjIwIn0.epf3dogC-8Hl45fKY5ZwVC8_Y7r8hmHcCyYmp8aSt0sudF3w3igxzyyJzW1VgcW0y5akbteurpi1tNnWQwnMnA";
    private readonly static string URL = "https://api.classypay.co.in/api/partner/";
    public classypay()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static async Task<string> PostCall(string BodyData,string MethodN)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions($"{classypay.URL}{MethodN}")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest($"{classypay.URL}{MethodN}", Method.Post);
        request.AddHeader("Authorization", classypay.Authorization);
        request.AddHeader("Content-Type", "application/json");

        request.AddStringBody(BodyData, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static async Task<string> GetCall(string BodyData, string MethodN)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions($"{classypay.URL}")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest($"{classypay.URL}{MethodN}", Method.Get);
        request.AddHeader("Authorization", classypay.Authorization);
     

        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }


}
