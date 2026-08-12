using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
/// <summary>
/// Summary description for ICommonDMT
/// </summary>
public interface ICommonDMTService
{
    Task<string> GetCusResult(CommonSearch commonSearch);
    Task<string> GetCusResultWithOTP(CommonSearchWithOTP commonSearch);
    Task<string> EKYCPRrocess(CommonDMTEkycProcess commonEkycProcess);
    Task<string> Registration(CommonDMTRegistration commonDMTRegistration);
    Task<string> RegisterBeneficiary(CommonDMTRegisterBeneficiary commonDMTRegisterBeneficiary);
    Task<string> verification(CommonDMTBeneficiaryVerification commonDMTBeneficiaryVerification);
    Task<string> verificationWithOTP(CommonDMTBeneficiaryVerificationWithOTP commonDMTBeneficiaryVerification);
    Task<string> DeleteBeneficiary(CommonDMTDeleteBeneficiary commonDMTDeleteBeneficiary);
    Task<string> FetchBeneficiary(CommonDMTFetchListBeneficiary commonDMTFetchListBeneficiary);
    Task<string> sendmoneyspecialsendotp(CommonSendMoneyOTP commonDMTFetchListBeneficiary);
    Task<string> sendmoneyspecial(CommonSendMoney commonSendMoney);
    Task<string> PayoutCustomerRegistrationRequest(PayoutCustomerRegistrationRequest commonSearch);
    Task<string> RegisterBeneficiaryWithOTP(CommonDMTRegisterBeneficiaryWithOTP commonDMTRegisterBeneficiary);

    Task<DataTable> GetBankList(string Pipe);
}

//Search Member//
public class CommonSearch
{
    public string Mobile { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
    public string Source { get; set; }
}


//Search Member//
public class CommonSearchWithOTP
{
    public string Mobile { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
    public string Source { get; set; }
    public string OTP { get; set; }
    public string stateresp { get; set; }
}


public class CommonSearchResult
{
    public string RCode { get; set; }
    public string Name { get; set; }
    public string Limit { get; set; }
    public string Mobile { get; set; }
    public string UseLimit { get; set; } = string.Empty;
    public string Pipe { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string stateresp { get; set; } = string.Empty;
    public string ekyc_id { get; set; } = string.Empty;
    public bool IsAadhar { get; set; } = false;
    public bool IsCapture { get; set; } = false;
    public bool IsOTP { get; set; } = false;
    public bool IsTPIN { get; set; } = false;
    public string Finger { get; set; } = string.Empty;
    public string Face { get; set; } = string.Empty;
    public string Iris { get; set; } = string.Empty;
}

//End Customer//


public class CommonDMTEkycProcess
{
    public string Mobile { get; set; }
    public string AadharNo { get; set; }
    public string Data { get; set; }
    public string DeviceName { get; set; }
    public string DeviceID { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
}


public class CommonDMTRegistration
{
    public string Mobile { get; set; }
    public string AadharNo { get; set; }
    public string OTP { get; set; }
    public string stateresp { get; set; }
    public string ekyc_id { get; set; }
    public string Data { get; set; }
    public string DeviceName { get; set; }
    public string DeviceID { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
}


//Register Beneficiary Request
public class CommonDMTRegisterBeneficiary
{
    public string Mobile { get; set; }
    public string BankName { get; set; }
    public string BeniName { get; set; }
    public string BankID { get; set; }
    public string AccountNo { get; set; }
    public string IFSCCODE { get; set; }
    public bool Verified { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
}


//Register Beneficiary Request With OTP
public class CommonDMTRegisterBeneficiaryWithOTP
{
    public string Mobile { get; set; }
    public string BankName { get; set; }
    public string BeniName { get; set; }
    public string BankID { get; set; }
    public string AccountNo { get; set; }
    public string IFSCCODE { get; set; }
    public string OTP { get; set; }
    public string stateresp { get; set; }
    public bool Verified { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
}


//Verification 
public class CommonDMTBeneficiaryVerification
{
    public string BankName { get; set; }
    public string BankID { get; set; }
    public string AccountNo { get; set; }
    public string IFSCCODE { get; set; }
    public string CustomerMobile { get; set; }
    public string CusName { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string OrderID { get; set; }
    public string MemberID { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string IP { get; set; }
}


public class CommonDMTBeneficiaryVerificationWithOTP
{
    public string BankName { get; set; }
    public string BankID { get; set; }
    public string AccountNo { get; set; }
    public string IFSCCODE { get; set; }
    public string CustomerMobile { get; set; }
    public string CusName { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string OrderID { get; set; }
    public string MemberID { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string IP { get; set; }
    public string OTP { get; set; }
    public string stateresp { get; set; }
}

public class CommonDMTRegisterBeneficiaryResponse
{
    public string RCode { get; set; }
    public string Message { get; set; }
    public string stateresp { get; set; }
    public bool IsOTP { get; set; } = false;
    public bool IsTpin { get; set; } = false;

}

//End


public class CommonDMTDeleteBeneficiary
{
    public string Mobile { get; set; }
    public string BaniID { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
}


//Register Beneficiary Request
public class CommonDMTFetchListBeneficiary
{
    public string Mobile { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
    public string Source { get; set; }

}

public class CommonDMTFetchListBeneficiaryResponse
{
    public string Mobile { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string bene_id { get; set; }
    public string bankid { get; set; }
    public string bankname { get; set; }
    public string name { get; set; }
    public string accno { get; set; }
    public string ifsc { get; set; }
    public string verified { get; set; }
    public string banktype { get; set; }
    public string MemberID { get; set; }
    public string Source { get; set; }
    public decimal SendMaxAmt { get; set; }
    public decimal SendminAmt { get; set; }
    public decimal SplitAmt { get; set; }
    public bool IsDown { get; set; } = false;
    public bool IsOTP { get; set; } = false;
}

//End


// Send Money OTP
public class CommonSendMoneyOTP
{
    public string BeneName { get; set; }
    public string AccNumber { get; set; }
    public string Isfc { get; set; }
    public string BankName { get; set; }
    public string CusName { get; set; }
    public string CustomerMobile { get; set; }
    public string amount { get; set; }
    public string BeniID { get; set; }
    public string OrderID { get; set; }
    public string txntype { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string Source { get; set; }


}


// Send Money
public class CommonSendMoney
{
    [Required(ErrorMessage = "Amount is required.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Beneficiary Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Beneficiary Name must be between 2 and 100 characters.")]
    public string BeneName { get; set; }

    [Required(ErrorMessage = "Account Number is required.")]
    [RegularExpression(@"^[0-9]{9,18}$", ErrorMessage = "Account Number must be 9–18 digits.")]
    public string AccNumber { get; set; }

    [Required(ErrorMessage = "IFSC is required.")]
    [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC code format.")]
    public string Isfc { get; set; }

    [Required(ErrorMessage = "Bank Name is required.")]
    [StringLength(100, ErrorMessage = "Bank Name cannot exceed 100 characters.")]
    public string BankName { get; set; }

    [StringLength(100, ErrorMessage = "Customer Name cannot exceed 100 characters.")]
    public string CusName { get; set; }

    [Required(ErrorMessage = "Customer Mobile is required.")]
    [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Invalid Mobile Number.")]
    public string CustomerMobile { get; set; }

    [Required(ErrorMessage = "Beneficiary ID is required.")]
    public string BeniID { get; set; }

    public string OrderID { get; set; }

    public string stateresp { get; set; }

    public string OTP { get; set; }

    public string TXNO { get; set; }

    [Required(ErrorMessage = "Transaction type is required.")]
    [RegularExpression(@"^(IMPS|NEFT|RTGS|UPI)$", ErrorMessage = "Invalid Transaction Type.")]
    public string txntype { get; set; }

    [Required(ErrorMessage = "Pipe is required.")]
    public string Pipe { get; set; }

    [Required(ErrorMessage = "Service ID is required.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Service ID must be numeric.")]
    public string ServiceID { get; set; }

    [Required(ErrorMessage = "Member ID is required.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Member ID must be numeric.")]
    public string MemberID { get; set; }

    public string latitude { get; set; }

    public string longitude { get; set; }

    public string GUID { get; set; }

    [Required(ErrorMessage = "Source is required.")]
    [StringLength(50, ErrorMessage = "Source cannot exceed 50 characters.")]
    public string Source { get; set; }
    public string IP { get; set; }
}



public class PDMTResponseData
{
    public string Status { get; set; }
    public string Message { get; set; }
    public string AccountNO { get; set; }
    public string IFSC { get; set; }
    public string Name { get; set; }
    public string Amount { get; set; }
    public string Mode { get; set; }
    public string Date { get; set; }

    [JsonProperty("Customer no")]
    public string Customerno { get; set; }

    [JsonProperty("Customer Name")]
    public string CustomerName { get; set; }
    public string BankName { get; set; }
    public List<OrderList> OrderList { get; set; }
}

public class OrderList
{
    public string OrderID { get; set; }
    public string RRN { get; set; }
    public string Amount { get; set; }
    public string Status { get; set; }
}


//Payout Customer Registration Request
public class PayoutCustomerRegistrationRequest
{
    public string Name { get; set; }
    public string Mobile { get; set; }
    public string Pan { get; set; }
    public string Aadhar { get; set; }
    public string Address { get; set; }
    public string OTP { get; set; }
    public string stateresp { get; set; }
    public string Source { get; set; }
    public string Pipe { get; set; }
    public string ServiceID { get; set; }
    public string MemberID { get; set; }
}
