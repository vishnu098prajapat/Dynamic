using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for Razorpayx
/// </summary>
public class Razorpayx
{
    public static string YOUR_KEY = "";//"rzp_test_1vUpJUNccalKFG";
    public static string YOUR_SECRET = "";// "clVoodtdMBChCUIcGmCrciCY";
    public static string Url = "";
}

public class CreateContactNotes
{
    public string note_key { get; set; }
}

public class CreateContact
{
    public string name { get; set; }
    public string email { get; set; }
    public string contact { get; set; }
    public string type { get; set; }
    public string reference_id { get; set; }
    public CreateContactNotes notes { get; set; }

    public string GetJson()
    {
        CreateContact ObjData = new CreateContact();
        ObjData.name = this.name;
        ObjData.email = this.email;
        ObjData.contact = this.contact;
        ObjData.type = this.type;
        ObjData.reference_id = this.reference_id;
        ObjData.notes = this.notes;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class CreateContactResultNotes
{
    public string note_key { get; set; }
}

public class CreateContactResultRoot
{
    public string id { get; set; }
    public string entity { get; set; }
    public string name { get; set; }
    public string contact { get; set; }
    public string email { get; set; }
    public string reference_id { get; set; }
    public string type { get; set; }
    public bool active { get; set; }
    public CreateContactResultNotes notes { get; set; }
    public int created_at { get; set; }
}






public class FundAccountBankAccount
{
    public string name { get; set; }
    public string ifsc { get; set; }
    public string account_number { get; set; }
}

public class FundAccount
{
    public string contact_id { get; set; }
    public string account_type { get; set; }
    public FundAccountBankAccount bank_account { get; set; }
    public string GetJson()
    {
        FundAccount ObjData = new FundAccount();
        ObjData.contact_id = this.contact_id;
        ObjData.account_type = this.account_type;
        ObjData.bank_account = this.bank_account;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class Vpa
{
    public string address { get; set; }
}

public class VpaRequest
{
    public string account_type { get; set; }
    public string contact_id { get; set; }
    public Vpa vpa { get; set; }
    public string GetJson()
    {
        VpaRequest ObjData = new VpaRequest();
        ObjData.account_type = this.account_type;
        ObjData.contact_id = this.contact_id;
        ObjData.vpa = this.vpa;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class VpaResult
{
    public string username { get; set; }
    public string handle { get; set; }
    public string address { get; set; }
}

public class VPAResultRoot
{
    public string id { get; set; }
    public string entity { get; set; }
    public string contact_id { get; set; }
    public string account_type { get; set; }
    public VpaResult vpa { get; set; }
    public bool active { get; set; }
    public object batch_id { get; set; }
    public int created_at { get; set; }
}


public class FundAccountResultBankAccount
{
    public string ifsc { get; set; }
    public string bank_name { get; set; }
    public string name { get; set; }
    public string account_number { get; set; }
}

public class FundAccountResultRoot
{
    public string id { get; set; }
    public string entity { get; set; }
    public string contact_id { get; set; }
    public string account_type { get; set; }
    public bool active { get; set; }
    public FundAccountResultBankAccount bank_account { get; set; }
    public int created_at { get; set; }
}






public class CreatePayoutNotes
{
    public string notes_key_1 { get; set; }
    public string notes_key_2 { get; set; }
    public CreatePayoutNotes()
    {
        notes_key_1 = "Request";
        notes_key_2 = "Request";
    }
}

public class CreatePayout
{
    public string account_number { get; set; }
    public string fund_account_id { get; set; }
    public int amount { get; set; }
    public string currency { get; set; }
    public string mode { get; set; }
    public string purpose { get; set; }
    public bool queue_if_low_balance { get; set; }
    public string reference_id { get; set; }
    public string narration { get; set; }
    public CreatePayoutNotes notes { get; set; }


    public string GetJson()
    {
        CreatePayout ObjData = new CreatePayout();
        ObjData.account_number = this.account_number;
        ObjData.fund_account_id = this.fund_account_id;
        ObjData.amount = this.amount;
        ObjData.currency = this.currency;
        ObjData.mode = this.mode;
        ObjData.purpose = this.purpose;
        ObjData.queue_if_low_balance = this.queue_if_low_balance;
        ObjData.reference_id = this.reference_id;
        ObjData.narration = this.narration;
        ObjData.notes = this.notes;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class CreatePayoutResultNotes
{
    public string notes_key_1 { get; set; }
    public string notes_key_2 { get; set; }
}

public class CreatePayoutResult
{
    public string id { get; set; }
    public string entity { get; set; }
    public string fund_account_id { get; set; }
    public int amount { get; set; }
    public string currency { get; set; }
    public CreatePayoutResultNotes notes { get; set; }
    public int fees { get; set; }
    public int tax { get; set; }
    public string status { get; set; }
    public object utr { get; set; }
    public string mode { get; set; }
    public string purpose { get; set; }
    public string reference_id { get; set; }
    public string narration { get; set; }
    public object batch_id { get; set; }
    public object failure_reason { get; set; }
    public int created_at { get; set; }
}



public class ApiPostCallRazorpayx
{
    public static string PostCall(string Json, string url)
    {
        var baseAddress = Razorpayx.Url + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }
    public static string GetCheckStatus(string ID)
    {
        var options = new RestClientOptions("https://api.razorpay.com/v1/payouts/" + ID)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest("https://api.razorpay.com/v1/payouts/" + ID, Method.Get);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        RestResponse response = client.Execute(request);
        return response.Content;
    }
}