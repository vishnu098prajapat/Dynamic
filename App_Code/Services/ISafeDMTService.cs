using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Summary description for ISafeDMTService
/// </summary>
public interface ISafeDMTService
{
    Task<DataTable> GetCustomerAsync(string Number);
    Task<DataTable> GetBeniList(string ID);
    Task<DataTable> GetBeniListWithAccount(string ID, string AccountNo);
    Task<DataTable> GetDataByAccountNO(string AccountNO);
    Task<DataTable> GetBankList();
    Task<string> GetBankListAsync(string OurLetID);
    Task<string> HandleCallBack(string Data);
    Task<string> HandleCallBackIserverPayout(string Data);
    Task<string> FundTransfer(DMTRequestCommon addAccount, string MemberID, string Source, string OutLetID);
    Task<string> GetCusResult(string No, string OurLetID, string MemberID);
    Task<Int32> DeleteBeni(string No);
    Task<Int32> GiveCommission(string TransID);
    Task<Int32> AddBeniINDataBase(JArray Data, string CusID);
    Task<string> AddBeni(AddAccount addAccount, string MemberID, string CusID, string VerifyOTPAccount, string BankName);
    Task<string> SendOTPOnRegistration(RemitterRegistration customerRegistration, string MemberID, string OutLetID);
    Task<string> OTPValiDate(OtpVerification otpVerification, string MemberID, string OutLetID);
    Task<DataTable> GetCusResultList(string No);
    Task<string> AccountVerification(CommonAccountVerificationRequest accountVerifyST, string MemberID);

}