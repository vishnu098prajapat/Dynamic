using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for bankopen
/// </summary>
public class bankopen
{
    public static string Authorization = "";
    public static string URL = "";
    public static string debit_account_number = "";

}

public class BankOpenPostCall
{
    public static string PostCall(string Mstr, NameValueCollection Data)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(bankopen.URL + Mstr)
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();
       
        var request = new RestRequest(bankopen.URL + Mstr,Method.Post);
        request.AddHeader("Authorization", bankopen.Authorization);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Accept", "application/json");
        foreach (string key in Data)
        {
            request.AddParameter(key, Data[key]);
        }
        RestResponse response = client.Execute(request);
        return response.Content;
    }
}