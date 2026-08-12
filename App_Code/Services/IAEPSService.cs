using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IAEPSService
/// </summary>
public interface IAEPSService
{
    Task<string> BE(AEPSRequest aEPSRequest);
    Task<string> SENDOTPAEPS(AEPSRequest aEPSRequest);
    Task<string> Authentication(AuthencationRequest aEPSRequest);
    Task<string> Registration(AuthencationRequest aEPSRequest);
    Task<string> EKYC(AuthencationRequest aEPSRequest);
    Task<string> SENDOTPEKYC(AuthencationRequest aEPSRequest);
    Task<string> VerifyOTPEKYC(AuthencationRequest aEPSRequest);
    Task<string> GetWadhDetails(string Pipe,string Module);



    Task<DataTable> GetAEPSStatus(string BankName,string Msrno);
    Task<Int32> UpdateAuthentication(string BankName,string Msrno);
    Task<string> GetBankList(string OutLetID);
}


public class InstantAEPSRequest
{
    public string AadharNo { get; set; }
    public string Mobile { get; set; }
    public string BankID { get; set; }
    public string BankName { get; set; }
    public string Amount { get; set; }
    public string Source { get; set; }
    public string Data { get; set; }
    public string DeviceName { get; set; }
    public string Type { get; set; }
    public string IP { get; set; }
}



// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class InstantAEPSCommonData
{
    public string OrderID { get; set; }
    public string bankName { get; set; }
    public string AadharNo { get; set; }
    public string VenndorID { get; set; }
    public string Amount { get; set; }
    public string Balance { get; set; }
    public string Status { get; set; }
    public string RRN { get; set; }
    public string Message { get; set; }
    public List<InstantAEPSCommonMiniStatement> miniStatement { get; set; }
}

public class InstantAEPSCommonMiniStatement
{
    public string date { get; set; }
    public string txnType { get; set; }
    public string amount { get; set; }
    public string narration { get; set; }
}


