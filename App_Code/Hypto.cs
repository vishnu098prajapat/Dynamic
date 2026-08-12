using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for Hypto
/// </summary>
public class Hypto
{
    public const string APIID = "";
    public const string Token = "";
}

public class VirtualAccount
{
    public string reference_number { get; set; }
    public string udf1 { get; set; }
    public string udf2 { get; set; }
    public string udf3 { get; set; }

    public string GetJson()
    {
        VirtualAccount ObjData = new VirtualAccount();
        ObjData.reference_number = this.reference_number;
        ObjData.udf1 = this.udf1;
        ObjData.udf2 = this.udf2;
        ObjData.udf3 = this.udf3;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class VirtualDetailsItem
{

    /// <summary>
    /// 
    /// </summary>
    public string account_number { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string account_ifsc { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<string> payment_modes { get; set; }
}

public class Virtual_accountResult
{

    /// <summary>
    /// 
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string reference_number { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string account_number { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string udf1 { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string udf2 { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string udf3 { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<VirtualDetailsItem> details { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string status { get; set; }
}

public class VirtualData
{

    /// <summary>
    /// 
    /// </summary>
    public Virtual_accountResult virtual_account { get; set; }
}

public class VirtualResultRoot
{

    /// <summary>
    /// 
    /// </summary>
    public string success { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string message { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public VirtualData data { get; set; }
}




public class UPI
{
    public string upi_id { get; set; }
    public string name { get; set; }
    public string pan { get; set; }
    public string category { get; set; }
    public string address { get; set; }
    public string lat { get; set; }
    public string lon { get; set; }
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }

    public string GetJson()
    {
        UPI ObjData = new UPI();
        ObjData.upi_id = this.upi_id;
        ObjData.name = this.name;
        ObjData.pan = this.pan;
        ObjData.category = this.category;
        ObjData.address = this.address;
        ObjData.lat = this.lat;
        ObjData.lon = this.lon;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



//Bank Verification Status Data
public class BankVerificationData
{
    public int id { get; set; }
    public string txn_time { get; set; }
    public string txn_type { get; set; }
    public string status { get; set; }
    public int amount { get; set; }
    public double charges_gst { get; set; }
    public double settled_amount { get; set; }
    public string bank_ref_num { get; set; }
    public string closing_balance { get; set; }
    public string payment_type { get; set; }
    public string reference_number { get; set; }
    public string verify_account_number { get; set; }
    public string verify_account_ifsc { get; set; }
    public string verify_account_holder { get; set; }
}

public class BankVerificationStatus
{
    public bool success { get; set; }
    public string message { get; set; }
    public BankVerificationData data { get; set; }
}


//BankVerification CAll Back
public class BankVerificationCallBack
{
    public string account_number { get; set; }
    public string account_ifsc { get; set; }
    public string account_holder { get; set; }
    public string id { get; set; }
    public string amount { get; set; }
    public string charges_gst { get; set; }
    public string settled_amount { get; set; }
    public string txn_time { get; set; }
    public string bank_ref_num { get; set; }
    public string reference_number { get; set; }
    public string status { get; set; }
    public string txn_type { get; set; }
}


public class CreatePrefix
{
    public string prefix { get; set; }
    public string GetJson()
    {
        CreatePrefix ObjData = new CreatePrefix();
        ObjData.prefix = this.prefix;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}