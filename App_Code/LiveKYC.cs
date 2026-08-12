using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for LiveKYC
/// </summary>
public class LiveKYC
{
    public LiveKYC()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}

public class gstinVerification
{
    public string refid { get; set; }
    public string gst_number { get; set; }
    public bool filing_status_get { get; set; }
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    public string GetJson()
    {
        gstinVerification ObjData = new gstinVerification();
        ObjData.refid = this.refid;
        ObjData.gst_number = this.gst_number;
        ObjData.filing_status_get = this.filing_status_get;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class rcVerification
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    public string refid { get; set; }
    public string rc_number { get; set; }

    public string GetJson()
    {
        rcVerification ObjData = new rcVerification();
        ObjData.refid = this.refid;
        ObjData.rc_number = this.rc_number;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class PaysAadharSendOTP : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required]
    public string aadhar_number { get; set; }
    public string refid { get; set; }

    public string GetJson()
    {
        PaysAadharSendOTP ObjData = new PaysAadharSendOTP();
        ObjData.refid = this.refid;
        ObjData.aadhar_number = this.aadhar_number;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;


        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(aadhar_number))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}



public class PaysAadharSendOTPVerify : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    public string otp { get; set; }
    public string refid { get; set; }

    public string GetJson()
    {
        PaysAadharSendOTPVerify ObjData = new PaysAadharSendOTPVerify();
        ObjData.refid = this.refid;
        ObjData.otp = this.otp;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;


        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(otp))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}


public class PaysPanVerification : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    public string refid { get; set; }
    public string id_number { get; set; }
    public string consent_purpose { get; set; }

    public string GetJson()
    {
        PaysPanVerification ObjData = new PaysPanVerification();
        ObjData.id_number = this.id_number;
        ObjData.refid = this.refid;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.consent_purpose = "For KYC Retailer KYC";
        ObjData.MethodName = this.MethodName;


        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(id_number))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}