using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for CashDeposit
/// </summary>


public class AEPSBankCredentials
{
    public static string JWTencryptionKey = "MmVmNDQ1ZmNmM2QyMGUyOTA2Y2U0YWFlNTE3ODAxYTE=";
    public static string AuthKey = "NTg4NDVhM2RhMTY1ZjkyNzM0NTQwNjY0NzY3MmIwMTU=";
    public static string AESKEY  = "a8d0fdd6c300e862";
    public static string AESIV  = "f0e85e077f231d24";
    public static string PARTNERID = "UFR008081";
    public static string Authorisedsign = "OWE5NDk4MzVmMDkzZDlkYmRhOWYxMWEyZWY2MWRmYTU=";
    public static string URL = "https://paysprint.in/service-api/api/v1/service/";
    public static string JWTencryptionKeyLive = "MmVmNDQ1ZmNmM2QyMGUyOTA2Y2U0YWFlNTE3ODAxYTE=";
    public static string AESKEYLive = "a8d0fdd6c300e862";
    public static string AESIVLive = "f0e85e077f231d24";
    public static string PARTNERIDLive = "UFR008081";
    
    public static string URLLive = "https://api.rnficms.com/api/aepsapi/v2/";
}


public class CashDeposit
{
    public long timestamp { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public string accessmodetype { get; set; }
    public string requestremarks { get; set; }
    public string mobilenumber { get; set; }
    public string accountnumber { get; set; }
    public string referenceno { get; set; }
    public string submerchantid { get; set; }
    public int amount { get; set; }
    public string ipaddress { get; set; }

    public CashDeposit()
    {
        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        longitude = objLocation.Longitude;
        latitude = objLocation.Latitude;
        accessmodetype = "SITE";
        requestremarks = "Successful";
        ipaddress = objLocation.IPAddress;

    }
    public string GetJson()
    {
        CashDeposit ObjData = new CashDeposit();
        ObjData.timestamp = this.timestamp;
        ObjData.longitude = this.longitude;
        ObjData.latitude = this.latitude;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.requestremarks = this.requestremarks;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.accountnumber = this.accountnumber;
        ObjData.referenceno = this.referenceno;
        ObjData.submerchantid = this.submerchantid;
        ObjData.amount = this.amount;
        ObjData.ipaddress = this.ipaddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}




public class ConfirmDeposit
{
    public string merchanttxnid { get; set; }
    public string txntoken { get; set; }
    public string otp { get; set; }
    public string txnreferenceno { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }

    public string GetJson()
    {
        ConfirmDeposit ObjData = new ConfirmDeposit();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.merchanttxnid = this.merchanttxnid;
        ObjData.txntoken = this.txntoken;
        ObjData.otp = this.otp;
        ObjData.txnreferenceno = this.txnreferenceno;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = objLocation.IPAddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class AEPSBankCall
{
    cls_connection Objconnection = new cls_connection();
    public string PostCall(string url,string Token,string Body)
    {

        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(AEPSBankCredentials.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(AEPSBankCredentials.URL + url, Method.Post);

        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Token", Token);
        request.AddHeader("PARTNERID", AEPSBankCredentials.PARTNERID);
        request.AddHeader("Authorisedsign", "OWE5NDk4MzVmMDkzZDlkYmRhOWYxMWEyZWY2MWRmYTU=");

        request.AddParameter("application/x-www-form-urlencoded", "{\"body\":\"" + Body + "\"}", ParameterType.RequestBody);
        //request.AddParameter("body", Data);
        RestResponse response = client.Execute(request);   
        //Objconnection.update_data("insert into tbl_Apeslog(url)values('"+ response.Content.ToString() + "')");

        return response.Content.ToString();
    }
    public string PostCallLive(string url, string Token, string Body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(AEPSBankCredentials.URLLive + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(AEPSBankCredentials.URLLive + url, Method.Post);

        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Token", Token);
        request.AddHeader("PARTNERID", AEPSBankCredentials.PARTNERIDLive);
        request.AddHeader("Authorisedsign", "OWE5NDk4MzVmMDkzZDlkYmRhOWYxMWEyZWY2MWRmYTU=");

        request.AddParameter("application/x-www-form-urlencoded", "{\"body\":\"" + Body + "\"}", ParameterType.RequestBody);
        //request.AddParameter("body", Data);
        RestResponse response = client.Execute(request);
       

        return response.Content.ToString();
    }

    public string GetCall(string url, string Token)
    {

        var options = new RestClientOptions(AEPSBankCredentials.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(AEPSBankCredentials.URL + url, Method.Get);

       
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Token", Token);
        request.AddHeader("Authorisedkey", AEPSBankCredentials.AuthKey);

        RestResponse response = client.Execute(request);
       
        return response.Content;
    }
    public string GetCallLive(string url, string Token)
    {
        var options = new RestClientOptions(AEPSBankCredentials.URLLive + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(AEPSBankCredentials.URLLive + url, Method.Get);

        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Token", Token);
        request.AddHeader("PARTNERID", AEPSBankCredentials.PARTNERIDLive);
        request.AddHeader("Authorisedsign", "OWE5NDk4MzVmMDkzZDlkYmRhOWYxMWEyZWY2MWRmYTU=");

        RestResponse response = client.Execute(request);
        return response.Content;
    }
}