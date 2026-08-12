using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ILoginService
/// </summary>
public interface ILoginService
{
    Task<ValidateReturn> Validate(Login login);
    Task<ValidateReturn> ValidateWithOTP(Login login, string Val);

    Task<string> ValidateMobile(Login login);
    Task<Int32> IncrementSuccessAttempts(string LoginID);
    Task<Tuple<Int32, string>> RsetPassword(string Msrno,string Pass);
    Task<Tuple<Int32, string>> RsetTpin(string Msrno,string Pass);

    
    Task<Int32> IncrementFailedAttempts(string LoginID);
    Task<Int32> LockAccount(string LoginID);
    Task<Int32> DeActiveAccount(string LoginID);
    Task<string> GetID(string LoginID);
    Task<string> LoginPin(string LoginID);
    Task<Int32> SaveLoginDetails(Login login);
    Task<Int32> SendLoginOTP(string Mobile, string CompanyID);
    Task<string> SendOTP(string SType,string Mobile,string token);
    Task<MemberPermission> GetPermission(string Msrno);
    
}


public class ValidateReturn
{
    public bool IsValid { get; set; }
    public bool ReVerify { get; set; }
    public string Reason { get; set; }
    public string Msrno { get; set; }
    public string NAME { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public string RoleRedirectLink { get; set; }
    public string Statename { get; set; }
    public string macaddress { get; set; }
    public string LoginID { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public bool KYC { get; set; }
    public bool FirstTimeLogin { get; set; }

}


public class OTPToken
{
    public string UserID { get; set; }
    public string OTP { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryTime { get; set; }
    public bool IsUsed { get; set; }
    public int Attempts { get; set; }
}


