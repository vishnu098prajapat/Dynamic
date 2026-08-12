using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading.Tasks;

public interface IPDMTService
{
    Task<DataTable> GetBankListAsync();
    Task<string> GetCusResult(string No);
    Task<string> GetBeniList(string ID);
    Task<string> GetBeniDetails(string ID, string CusNo);
    Task<string> Registration(PaysPrintDMTCustomerRegister paysPrintDMTCustomerRegister, string MemberID);
    Task<string> AddBeni(PaysPrintDMTBeniRegister paysPrintDMTBeniRegister);
    Task<string> DeleteBeni(string No, string Number);
    Task<Decimal> GetSurchargeValue(String Msrno, decimal Amount);
    Task<string> EKYC(DMTRequestekyc dMTRequestekyc, string MemberID);
    Task<string> FundTransfer(DMTRequestPaysprint addAccount, string MemberID, string Source, string CusNo, string CusName);
    Task<string> FundTransfer(DMTRequestPaysprint addAccount, string MemberID, string Source, string CusNo, string CusName, string TXNO, string GUID);
    Task<string> FundTransferSendOTP(DMTRequestPaysprint addAccount, string MemberID, string Source, string CusNo, string CusName);
    Task<string> AccountVerificationNew(AccountVerificationNew accountVerifyST, string MemberID);
}
public class AccountVerificationNew
{
    public string ifsc { get; set; }
    public string number { get; set; }
    public string BankName { get; set; }
    public string BankID { get; set; }
    public string CNumber { get; set; }
    public string CName { get; set; }
    public string OrderID { get; set; }
    public string Source { get; set; }
    public string IP { get; set; }

    public string GetJson()
    {
        AccountVerificationNew ObjData = new AccountVerificationNew();
        ObjData.ifsc = this.ifsc;
        ObjData.number = this.number;
        ObjData.BankName = this.BankName;
        ObjData.CNumber = this.CNumber;
        ObjData.CName = this.CName;
        ObjData.OrderID = this.OrderID;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}