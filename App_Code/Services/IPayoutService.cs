using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IPayoutService
/// </summary>
public interface IPayoutService
{
    Task<string> FundTransfer(PayoutCommon payoutCommon, string MemberID);
    //Task<string> FundTransfer(PayoutCommon payoutCommon, string MemberID,String APIID);
    Task<PayoutCommonResponse> FundTransferPaysPrint(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferIserve(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferSafePax(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferezypay(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferInstantWithAccount(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferInstantWithSoniTechno(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferInstantWithEzypay(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferCCPaySoniTechno(CreditPayST creditCardRequest);
    Task<PayoutCommonResponse> FundTransferCCPayInstantPay(CreditPayST creditCardRequest);
    Task<PayoutCommonResponse> FundTransferAeronpay(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferAeronpayUPI(UPITransferRequestST payoutCommon);
    Task<PayoutCommonResponse> FundTransferSoniTechnoUPI(UPITransferRequestST payoutCommon);
    Task<PayoutCommonResponse> FundTransferfinzep(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferInstantWithPromptPe(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferInstantWithPayDMR(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferBetaUPI(UPITransferRequestST payoutCommon);
    Task<PayoutCommonResponse> FundTransferDMTSoniTechno(PayoutCommon payoutCommon);
	 Task<PayoutCommonResponse> FundTransferInstantWithBetaSource(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferrechargkit(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> FundTransferclassypay(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> SoniVerification(PayoutCommon payoutCommon);
    Task<PayoutCommonResponse> ClassypayVerification(PayoutCommon payoutCommon);


}


public class PayoutCommon
{
    public string AccountNO { get; set; }
    public string IFSC { get; set; }
    public string Name { get; set; }
    public string OrderID { get; set; }
    public string BankName { get; set; }
    public string Mobile { get; set; }
    public string AgentMobile { get; set; }
    public string Mode { get; set; }
    public string MailID { get; set; }
    public decimal Amount { get; set; }
    public string AccountID { get; set; }
    public string identifier { get; set; }
    public string otp { get; set; }
    public string BeneID { get; set; }
}




public class PayoutCommonResponse
{
    public string OrderID { get; set; }
    public string RRN { get; set; }
    public string AccountNo { get; set; }
    public string IFSC { get; set; }
    public string Name { get; set; }
    public string VendorID { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public string TXNCode { get; set; }
    public string Result { get; set; }
    public string Request { get; set; }
    public string GUID { get; set; }
    public string TXNO { get; set; }
    public bool IsOTP { get; set; }
    public string stateresp { get; set; }
    public bool IsTPIN { get; set; }
}

