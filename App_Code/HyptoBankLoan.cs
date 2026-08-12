using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for HyptoBankLoan
/// </summary>
public class HyptoBankLoan
{    
    public bool success { get; set; }
    public string message { get; set; }
    public DataLoan data { get; set; }
}


public class DataLoan
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

public class PayoutRequestLoan
{
    public double amount { get; set; }
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
        PayoutRequestLoan ObjData = new PayoutRequestLoan();
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


public class AccountVerify
{
    public string ifsc { get; set; }
    public string number { get; set; }
    public string reference_number { get; set; }

    public string GetJson()
    {
        AccountVerify ObjData = new AccountVerify();
        ObjData.ifsc = this.ifsc;
        ObjData.number = this.number;
        ObjData.reference_number = this.reference_number;        
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




public class VerifyStataResultData
{
    public int id { get; set; }
    public string txn_time { get; set; }
    public string created_at { get; set; }
    public string txn_type { get; set; }
    public string status { get; set; }
    public double amount { get; set; }
    public double charges_gst { get; set; }
    public double settled_amount { get; set; }
    public string bank_ref_num { get; set; }
    public double closing_balance { get; set; }
    public string payment_type { get; set; }
    public string reference_number { get; set; }
    public string verify_reason { get; set; }
    public string verify_account_number { get; set; }
    public string verify_account_ifsc { get; set; }
    public string verify_account_holder { get; set; }
}

public class VerifyStataResultRoot
{
    public bool success { get; set; }
    public string message { get; set; }
    public VerifyStataResultData data { get; set; }
}

public class ApiPostCallLoan
{
    public static string URL = "https://partners.hypto.in/api/transfers/initiate";
    public static string PostCall(string Json)
    {
        var baseAddress = URL;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);

        var response = client.Execute(request);




        return response.Content.ToString();
    }

    public static string VerifyAccount(string Json)
    {
        var baseAddress = "https://partners.hypto.in/api/verify/bank_account";
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);

        var response = client.Execute(request);




        return response.Content.ToString();
    }

    public static string GetCall(string Val)
    {
        var baseAddress = URL;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        using (WebClient webClient = new WebClient())
        {
            webClient.Headers.Add("Content-Type", "application/json");
            webClient.Headers["Authorization"] = "1c327b26-bf47-4bba-bde9-e45f8e427141";
            var json = webClient.DownloadString("https://partners.hypto.in/api/transfers/status/"+ Val + "");
            return json.ToString();
        }
    }
}