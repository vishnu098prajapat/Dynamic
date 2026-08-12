using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Summary description for IDMTService
/// </summary>
public interface IDMTService
{
    Task<DataTable> GetCustomerAsync(string Number);
    Task<DataTable> GetBeniList(string ID);
    Task<DataTable> GetBankListAsync();
    Task<string> FundTransfer(JObject addAccount,string MemberID,string Source);
    Task<string> GetCusResult(string No);
    Task<Int32> DeleteBeni(string No);
    Task<Int32> GiveCommission(string TransID);
    Task<string> AddBeni(AddAccount addAccount, string MemberID, string CusID, string VerifyOTPAccount, string BankName);
    Task<string> AccountVerification(AccountVerifyST accountVerifyST,string MemberID);
    Task<string> Registration(CustomerRegistration customerRegistration, string MemberID);
    Task<DataTable> GetCusResultList(string No);
    Task<Decimal> GetSurchargeValue(String Msrno, decimal Amount);



}