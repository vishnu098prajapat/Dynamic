using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IEDMTService
/// </summary>
public interface IEDMTService
{
    Task<string> CheckRemitterRegistration(string No,string ServiceID);
    Task<string> VerifyRemitterAadhaar(string No,string AadharNo, string MerchantID, string ServiceID);
    Task<string> VerifyRemitterOtp(object Request, string MerchantID, string ServiceID);
    Task<string> GetBeneficiaryList(string No, string ServiceID);
    Task<string> GetBankList();
    Task<string> AddBeneficiary(object Request, string ServiceID);
    Task<Decimal> GetSurchargeValue(String Msrno, decimal Amount, string ServiceID);
    Task<string> DeleteBeneficiaryRequest(string No, string beneficiaryid, string ServiceID);
    Task<string> EKYC(object dMTRequestekyc,string MerchantID, string ServiceID);
    Task<string> InititateAfaRequest(string No, string ServiceID);
    Task<string> FundTransfer(DMTRequestCommon addAccount, string MemberID, string Source, string OutLetID, string ServiceID);
    Task<string> FundTransferWithStep(DMTRequestCommon addAccount, string MemberID, string Source, string OutLetID, string ServiceID,string TXNNO,string GID);

    Task<string> AccountVerification(CommonAccountVerificationRequest accountVerifyST, string MemberID);
}



public class DMTRequestCommon
{
    public string RemitterMobile { get; set; }
    public string Accountno { get; set; }
    public string Name { get; set; }
    public string CusName { get; set; }
    public string CusMobile { get; set; }
    public string BankName { get; set; }
    public string IFSC { get; set; }
    public string BeneID { get; set; }
    public string Mode { get; set; }
    public int Amount { get; set; }
    public string OrderID { get; set; }
    public string OTP { get; set; }
    public string Pipe { get; set; }
    public string identifier { get; set; }
    public string IP { get; set; }
    public string UserID { get; set; }
    public string ServiceID { get; set; }
	public string Email { get; set; }

}