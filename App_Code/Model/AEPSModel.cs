using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for AEPSModel
/// </summary>
public class AEPSModel
{
    public AEPSModel()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}



public class AEPSResponse
{
    public string Img { get; set; }
    public string Message { get; set; }
    public string Amount { get; set; }
    public string AccountBalance { get; set; }
    public string BankName { get; set; }
    public string MobileNo { get; set; }
    public string BCCode { get; set; }
    public string BCName { get; set; }
    public string TransID { get; set; }
    public string AadharNo { get; set; }
    public string Reason { get; set; }
    public string BankNumber { get; set; }
    public string Date { get; set; }
    public IList<Ministatement> ministatement { get; set; }
}


public class Ministatement
{
    public string date { get; set; }
    public string amount { get; set; }
    public string txnType { get; set; }
    public string narration { get; set; }
}


public class AEPS
{
    public string Mobile { get; set; }
    public Int32 Amount { get; set; }
    public BankID BankID { get; set; }
    public string AadharNo { get; set; }
    public string Capture { get; set; }
    public string DeviceName { get; set; }
    public string Type { get; set; }
    public string TransactionId { get; set; }
    public string UidaiDataTxn { get; set; }
    public string txnReferenceNo { get; set; }
    public string authRequestId { get; set; }
    public string requestid { get; set; }
}

public class BankID
{
    public int id { get; set; }
    public string text { get; set; }
}





public class AEPSRequest
{
    public string UserID { get; set; }
    public string BCCode { get; set; }
    public string BCName { get; set; }
    public string AEPSID { get; set; }
    public string TType { get; set; }
    public decimal Amount { get; set; }
    public string MobileNO { get; set; }
    public string ServiceID { get; set; }
    public string CusMobileNO { get; set; }
    public string Adhaarnumber { get; set; }
    public string OrderID { get; set; }
    public string AEPSPin { get; set; }
    
    public string Data { get; set; }
    public string BankNo { get; set; }
    public string BankName { get; set; }
    public string BankType { get; set; }
    public string Source { get; set; }
    public string DeviceId { get; set; }
    public string DeviceName { get; set; }
    public string IP { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TransactionId { get; set; }
    public string UidaiDataTxn { get; set; }
    public string txnReferenceNo { get; set; }
    public string authRequestId { get; set; }
    public string requestid { get; set; }
}


public class AEPSResponseAPI
{
    public bool Status { get; set; }
    public string StatusM { get; set; }
    public string Message { get; set; }
    public string BCCode { get; set; }
    public string BCName { get; set; }
    public string MobileNO { get; set; }
    public string Adhaarnumber { get; set; }
    public string BankName { get; set; }
    public string DeviceName { get; set; }
    public string TransDate { get; set; }
    public string BalanceAmount { get; set; }
    public string BankRRN { get; set; }
    public string OrderID { get; set; }
    public string VendorID { get; set; }
    public string IP { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Amount { get; set; }
    public string Title { get; set; }
    public string TType { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
    public string TransactionId { get; set; }
    public string UidaiDataTxn { get; set; }
    public string txnReferenceNo { get; set; }
    public string authRequestId { get; set; }
    public string requestid { get; set; }
    public IList<Ministatement> ministatement { get; set; }
}




public class AuthencationRequest
{
    public string Source { get; set; }
    public string UserID { get; set; }
    public string adhaarnumber { get; set; }
    public string MobileNO { get; set; }
    public string pipe { get; set; }
    public string ServiceID { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string OrderID { get; set; }
    public string AEPSID { get; set; }
    public string AEPSPin { get; set; }
    public string timestamp { get; set; }
    public string DeviceName { get; set; }
    public string Data { get; set; }
    public string ipaddress { get; set; }
    public string TType { get; set; }
    public string ServiceType { get; set; }
    public string DOB { get; set; }
    public string Optional1 { get; set; }
    public string Optional2 { get; set; }
    public string OTP { get; set; }
}



public class AuthencationAEPS
{
    public string Source { get; set; }
    public string pipe { get; set; }
    public string DeviceName { get; set; }
    public string data { get; set; }
    public string Optional1 { get; set; }
    public string Optional2 { get; set; }
    public string OTP { get; set; }
}




public class AuthencationResponse
{
    public bool Status { get; set; }
    public string StatusM { get; set; }
    public string Message { get; set; }
    public string OrderID { get; set; }
    public string Optional1 { get; set; }
    public string Optional2 { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
}
