using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IAccountVerificationService
/// </summary>
public interface IAccountVerificationService
{
    Task<string> AccountVerification(CommonAccountVerificationRequest accountVerifyST,string MemberID);
}


public class CommonAccountVerificationRequest
{
    public string AccountNO { get; set; }
    public string IFSC { get; set; }
    public string OrderID { get; set; }
    public string BankName { get; set; }
    public string BankID { get; set; }
    public string CusName { get; set; }
    public string CusMobile { get; set; }
    public string IP { get; set; }
    public string Source { get; set; }
}