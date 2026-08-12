using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ezypay
/// </summary>
public class ezypay
{
    public static readonly string token = "";
    public static readonly string service = "";
    public static readonly string URL = "https://app.ezypay.co.in/ws/v1/prod/";
    public ezypay()
    {
        //
        // TODO: Add constructor logic here
        //
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
        //request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Cookie", "ci_session=060vl25oeq0bcl8jaa40lf1uuj6els0f");
        request.AlwaysMultipartFormData = true;
        request.AddParameter("requestid", ezypayAgentKYCUpload.requestid);
        request.AddParameter("token", ezypay.token);
        request.AddParameter("agent_id", ezypayAgentKYCUpload.agent_id);
        request.AddParameter("doc_id", ezypayAgentKYCUpload.doc_id);
        request.AddParameter("doc_input", ezypayAgentKYCUpload.doc_input);
        request.AddFile("file", ezypayAgentKYCUpload.file);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static async Task<string> PostcallUploadBankAdd(ezypayaddAgentBankDetails ezypayAgentKYCUpload, string MethodN)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions("https://app.ezypay.co.in/ws/v1/prod/Action/addAgentBankDetails")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("https://app.ezypay.co.in/ws/v1/prod/Action/addAgentBankDetails", Method.Post);
        //request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Cookie", "ci_session=060vl25oeq0bcl8jaa40lf1uuj6els0f");
        request.AlwaysMultipartFormData = true;
        request.AddParameter("requestid", ezypayAgentKYCUpload.requestid);
        request.AddParameter("token", ezypay.token);
        request.AddParameter("agent_id", ezypayAgentKYCUpload.agent_id);
        request.AddParameter("bank_name", ezypayAgentKYCUpload.bank_name);
        request.AddParameter("bank_nick_name", ezypayAgentKYCUpload.bank_nick_name);
        request.AddParameter("account_number", ezypayAgentKYCUpload.account_number);
        request.AddParameter("ifsc_code", ezypayAgentKYCUpload.ifsc_code);
        request.AddParameter("account_holder_name", ezypayAgentKYCUpload.account_holder_name);
        request.AddFile("cancelled_cheque", ezypayAgentKYCUpload.cancelled_cheque);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }



    public static async Task<string> Postcall(string Request, string MethodN)
    {
        cls_connection cls = new cls_connection();
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        string BaseURL = ezypay.URL + MethodN;
        var options = new RestClientOptions(BaseURL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(BaseURL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddStringBody(Request, DataFormat.Json);
        RestResponse response = await client.ExecuteAsync(request);

        return response.Content;
    }
}




public class ezypayAEPSAgentOnboardingStatus
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string agent_id { get; set; }

    public ezypayAEPSAgentOnboardingStatus()
    {
        token = ezypay.token;
    }
}


public class ezypayAEPSAgentOnboarding
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string agent_id { get; set; }
    public string lat { get; set; } = "0";
    public string @long { get; set; } = "0";
    public string aadhaar { get; set; }

    public ezypayAEPSAgentOnboarding()
    {
        token = ezypay.token;
    }
}


public class ezypayAEPSBioAuthentication
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string aadhaar { get; set; }
    public string device { get; set; }
    public string lat { get; set; }
    public string @long { get; set; }
    public string agent_id { get; set; }
    public string encdata { get; set; }

    public ezypayAEPSBioAuthentication()
    {
        token = ezypay.token;
    }
}


public class ezypayAEPSBE
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string aadhaar { get; set; }
    public string device { get; set; }
    public string lat { get; set; } = "0";
    public string @long { get; set; } = "0";
    public string cust_mob { get; set; }
    public string bank { get; set; }
    public string agent_id { get; set; }
    public string encdata { get; set; }

    public ezypayAEPSBE()
    {
        token = ezypay.token;
    }
}




public class ezypayAEPSMS
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string aadhaar { get; set; }
    public string device { get; set; }
    public decimal lat { get; set; } = 0;
    public decimal @long { get; set; } = 0;
    public string cust_mob { get; set; }
    public string bank { get; set; }
    public string agent_id { get; set; }
    public string encdata { get; set; }

    public ezypayAEPSMS()
    {
        token = ezypay.token;
    }
}





public class ezypayAEPSCW
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string aadhaar { get; set; }
    public string device { get; set; }
    public string lat { get; set; }
    public string @long { get; set; }
    public string cust_mob { get; set; }
    public string bank { get; set; }
    public string agent_id { get; set; }
    public string encdata { get; set; }
    public string amount { get; set; } = "0";

    public ezypayAEPSCW()
    {
        token = ezypay.token;
    }
}




public class ezypayAddAgent
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string contact_person { get; set; }
    public string mobile { get; set; }
    public string shop_address { get; set; }
    public string shop_name { get; set; }
    public string pan_card { get; set; }
    public string aadhar_card { get; set; }
    public string dob { get; set; }
    public string agent_type { get; set; } = "ONLINE";

    public ezypayAddAgent()
    {
        token = ezypay.token;
    }
}




public class ezypayAgentKYCList
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string agent_id { get; set; }


    public ezypayAgentKYCList()
    {
        token = ezypay.token;
    }
}


public class ezypayAgentKYCUpload
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string agent_id { get; set; }
    public string doc_id { get; set; }
    public string doc_input { get; set; }
    public string file { get; set; }


    public ezypayAgentKYCUpload()
    {
        token = ezypay.token;
    }
}



public class ezypayaddAgentBankDetails
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string agent_id { get; set; }
    public string bank_name { get; set; }
    public string bank_nick_name { get; set; }
    public string account_number { get; set; }
    public string ifsc_code { get; set; }
    public string account_holder_name { get; set; }
    public string cancelled_cheque { get; set; }


    public ezypayaddAgentBankDetails()
    {
        token = ezypay.token;
    }
}




public class ezypayinitiateAgentSettlement
{
    public string requestid { get; set; }
    public string token { get; set; }
    public string agent_id { get; set; }
    public string beneficiary_id { get; set; }
    public string request_type { get; set; }
    public string amount { get; set; }

    public ezypayinitiateAgentSettlement()
    {
        token = ezypay.token;
    }
}