using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IKYC
/// </summary>
public interface IKYCService
{
    Task<String> SendOTP(PaysAadharSendOTP paysAadharSendOTP, string MemberID);
    Task<String> VerifyOTP(OTPVerifyAadhar oTPVerifyAadhar, string MemberID);
}


public class OTPVerifyAadhar
{
    public string OrderID { get; set; }
    public string StepID { get; set; }
    public string AadharNo { get; set; }
    public string OTP { get; set; }
    public string CustomerNo { get; set; }
}