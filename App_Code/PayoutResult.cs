using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;
using System.Threading.Tasks;
/// <summary>
/// Summary description for PayoutResult
/// </summary>
public class PayoutResult
{
    public bool success { get; set; }
    public string message { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public int id { get; set; }
    public string txn_time { get; set; }
    public string txn_type { get; set; }
    public string status { get; set; }
    public double amount { get; set; }
    public double charges_gst { get; set; }
    public double settled_amount { get; set; }
    public string bank_ref_num { get; set; }
    public string closing_balance { get; set; }
    public string payment_type { get; set; }
    public string reference_number { get; set; }
    public string transfer_beneficiary_name { get; set; }
    public string transfer_account_number { get; set; }
    public string transfer_upi_id { get; set; }
    public string transfer_account_ifsc { get; set; }
    public string transfer_note { get; set; }
    public string transfer_udf1 { get; set; }
    public string transfer_udf2 { get; set; }
    public string transfer_udf3 { get; set; }
    public string transfer_account_holder { get; set; }
    public string txn_id { get; set; }
}

public class PayoutRequest
{
    public int amount { get; set; }
    public string payment_type { get; set; }
    public string ifsc { get; set; }
    public string number { get; set; }
    public string note { get; set; }
    public string udf1 { get; set; }
    public string udf2 { get; set; }
    public string udf3 { get; set; }
    public string reference_number { get; set; }

    public string GetJson()
    {
        PayoutRequest ObjData = new PayoutRequest();
        ObjData.amount = this.amount;
        ObjData.payment_type = this.payment_type;
        ObjData.ifsc = this.ifsc;
        ObjData.number = this.number;
        ObjData.note = this.note;
        ObjData.udf1 = this.udf1;
        ObjData.udf2 = this.udf2;
        ObjData.udf3 = this.udf3;
        ObjData.reference_number = this.reference_number;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class ApiPostCall
{
    
    public static string PostCall(string Json,string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(baseAddress, Method.Post);
        request.Method = Method.Post;
        request.AddHeader("Accept", "application/json");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        var response = client.Execute(request);
        return response.Content.ToString();
    }

 public static async Task<string> PostcallUpload(ezypayAgentKYCUpload ezypayAgentKYCUpload, string MethodN)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions("https://app.ezypay.co.in/ws/v1/prod/Action/uploadKycDoc")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("https://app.ezypay.co.in/ws/v1/prod/Action/uploadKycDoc", Method.Post);
        request.AddHeader("Cookie", "ci_session=060vl25oeq0bcl8jaa40lf1uuj6els0f");
        request.AlwaysMultipartFormData = true;
        request.AddParameter("requestid", ezypayAgentKYCUpload.requestid);
        request.AddParameter("token", ezypayAgentKYCUpload.token);
        request.AddParameter("agent_id", ezypayAgentKYCUpload.agent_id);
        request.AddParameter("doc_id", ezypayAgentKYCUpload.doc_id);
        request.AddParameter("doc_input", ezypayAgentKYCUpload.doc_input);
        request.AddFile("file", ezypayAgentKYCUpload.file);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }


    public static async Task<string> PostCallAsync(string Json, string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Accept", "application/json");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response =await client.ExecuteAsync(request);
        return response.Content.ToString();
    }
    public static string GetCall(string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        using (WebClient webClient = new WebClient())
        {
            webClient.Headers.Add("Content-Type", "application/json");
            webClient.Headers["Authorization"] = "7794bda5-8d4e-4782-84a0-2af4ef05bb6f";
            var json = webClient.DownloadString("https://partners.hypto.in/api/transfers/status/3432422222");
            return json.ToString();
        }
    }
    public static string GetHytpo(string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        using (WebClient webClient = new WebClient())
        {
            webClient.Headers.Add("Content-Type", "application/json");
            webClient.Headers["Authorization"] = "1c327b26-bf47-4bba-bde9-e45f8e427141";
            var json = webClient.DownloadString(url);
            return json.ToString();
        }
    }
}