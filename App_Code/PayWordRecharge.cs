using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for PayWordRecharge
/// </summary>
public class PayWordRecharge
{
    public static string MerChantID = "4";
    public static string merchantUserId = "1017";
    public static string JWTsecretkey = "";
    public PayWordRecharge()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}


public class GetBalanceRecharge
{
    public string merchantUserId { get; set; }
    public GetBalanceRecharge()
    {
        merchantUserId = PayWordRecharge.merchantUserId;
    }
}

public class GetBalanceRechargeRoot
{
    public string actionName { get; set; }
    public IList<GetBalanceRecharge> parameters { get; set; }
}


public class ApiPostCallPayWordRecharge
{

    public static string PostCall(string Json, string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);

        var request = new RestRequest(url,Method.Post);


        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("merchantId", "4");
        request.AddHeader("version", "v1.0");
        //request.AddHeader("merchantUserId", "1028");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);

        var response = client.Execute(request);
        return response.Content.ToString();



    }



}