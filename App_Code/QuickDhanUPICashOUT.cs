using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
/// <summary>


//Quick Dhan UPI CashOUT
public class QuickDhanUPICashOUTSendOTP
{
    public string refid { get; set; }
    public string mobile { get; set; }
    public string merchant_code { get; set; }
    public string merchant_name { get; set; }
    public string customer_name { get; set; }
    public string amount { get; set; }
    public string address { get; set; }
    public string pincode { get; set; }
    public string purpose { get; set; }
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    public string GetJson()
    {
        QuickDhanUPICashOUTSendOTP ObjData = new QuickDhanUPICashOUTSendOTP();
        ObjData.refid = this.refid;
        ObjData.mobile = this.mobile;
        ObjData.merchant_code = this.merchant_code;
        ObjData.merchant_name = this.merchant_name;
        ObjData.customer_name = this.customer_name;
        ObjData.amount = this.amount;
        ObjData.address = this.address;
        ObjData.pincode = this.pincode;
        ObjData.purpose = this.purpose;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class QuickDhanUPICashOUTOTPVerify
{
    public string refid { get; set; }
    public string mobile { get; set; }
    public string merchant_code { get; set; }
    public string merchant_name { get; set; }
    public string customer_name { get; set; }
    public string amount { get; set; }
    public string address { get; set; }
    public string purpose { get; set; }
    public string pincode { get; set; }
    public string otp_refid { get; set; }
    public string otp { get; set; }
    public string APIID { get; set; }
    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    public string GetJson()
    {
        QuickDhanUPICashOUTOTPVerify ObjData = new QuickDhanUPICashOUTOTPVerify();
        ObjData.refid = this.refid;
        ObjData.mobile = this.mobile;
        ObjData.merchant_code = this.merchant_code;
        ObjData.merchant_name = this.merchant_name;
        ObjData.customer_name = this.customer_name;
        ObjData.amount = this.amount;
        ObjData.address = this.address;
        ObjData.pincode = this.pincode;
        ObjData.purpose = this.purpose;
        ObjData.otp_refid = this.otp_refid;
        ObjData.otp = this.otp;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class QuickDhanUPICashOUTCheckSTatus
{
    public string refid { get; set; }


    public string GetJson()
    {
        QuickDhanUPICashOUTCheckSTatus ObjData = new QuickDhanUPICashOUTCheckSTatus();
        ObjData.refid = this.refid;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}