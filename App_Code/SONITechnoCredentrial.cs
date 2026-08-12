using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SONITechnoCredentrial
/// </summary>
public class SONITechnoCredentrial
{
    public const string APIID = "AP90388";
    //public const string Token = "ad931774-aed6-410f-a905-fba354667fdd";
    public const string Token = "07520f30-2a37-4659-b96b-54f108b3adcf";
    public const string MoneyURL = "https://sonitechno.in/api/MoneyTransfer.aspx";
    public const string DMT = "https://sonitechno.in/api/DMT.aspx";
    public const string UPICashOut = "https://sonitechno.in/api/UPICashOut.aspx";
    public const string MATMUTL = "https://sonitechno.in/api/Matm.aspx";
    public const string AEPSURL = "https://sonitechno.in/api/NAEPS.aspx";
    public const string UPIURL = "https://sonitechno.in/api/UPI.aspx";
    public const string PayOutURL = "https://sonitechno.in/api/Payout.aspx";
    public const string PanURL = "https://sonitechno.in/api/Pan.aspx";
    public const string NSDLPan = "https://sonitechno.in/api/NSDLSoft.aspx";
    public const string ProfitURL = "https://sonitechno.in/api/ProfitReportAPI.aspx";
    public const string UPI = "https://sonitechno.in/api/UPITransfer.aspx";
    public const string BBPS = "https://sonitechno.in/api/BBPS.aspx";
    public const string CCpay = "https://sonitechno.in/api/CreditPay.aspx";
    public const string BUS = "";
    public const string Prefix = "Janta";
    public const string AEPSCore = "https://sonitechno.in/api/AEPSCore.aspx";
    public const string WebsiteURL = "https://jharkhandpay.vip/";
    public const string PinCode = "https://api.postalpincode.in/pincode/";
    public const string UPIVerification = "https://sonitechno.in/api/UPIVerification.aspx";
    public const string Plan = "https://sonitechno.in/api/PlanAPI.aspx";
    public const string VanURL = "https://sonitechno.in/api/van.aspx";
    public const string KYCURL = "https://sonitechno.in/api/LiveKYC.aspx";
    public const string AccountOpening = "https://sonitechno.in/api/AccountOpening.aspx";
    public const string AEPSCorePaytm = "https://sonitechno.in/API/AEPSFPay.aspx";
    public const string DMTPPI = "https://sonitechno.in/api/DMTPPI.aspx";
}


public class CustomerRegistration : IValidatableObject
{

    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required]
    public string Name { get; set; }
    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string Mobile { get; set; }
    [Required]
    public string ADDRESS { get; set; }
    //[Required(ErrorMessage = "PAN Number is required")]
    //[RegularExpression("^([A-Za-z]){5}([0-9]){4}([A-Za-z]){1}$", ErrorMessage = "Invalid PAN Number")]
    //[MinLength(10, ErrorMessage = "Please Enter Valid Pan No")]
    //[MaxLength(10, ErrorMessage = "Please Enter Valid Pan No")]
    public string Pan { get; set; }
    //[Required(ErrorMessage = "Aadhar Number is required")]
    //[MinLength(12, ErrorMessage = "Please Enter Valid Aadhar No")]
    //[MaxLength(12, ErrorMessage = "Please Enter Valid Aadhar No")]
    public string AADHAR { get; set; }
    [Required]
    public bool IsVerify { get; set; }

    public string GetJson()
    {
        CustomerRegistration ObjData = new CustomerRegistration();
        ObjData.Name = this.Name;
        ObjData.Mobile = this.Mobile;
        ObjData.ADDRESS = this.ADDRESS;
        ObjData.Pan = this.Pan;
        ObjData.AADHAR = this.AADHAR;
        ObjData.IsVerify = this.IsVerify;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}


public class CustomerDetails : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string Mobile { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }

    public string GetJson()
    {
        CustomerDetails ObjData = new CustomerDetails();
        ObjData.Mobile = this.Mobile;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}


public class AddAccount : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required]
    public string Name { get; set; }
    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string Mobile { get; set; }

    [Required(ErrorMessage = "Customer Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string CustomerMobile { get; set; }
    [Required(ErrorMessage = "Account Number is required")]
    [MinLength(8, ErrorMessage = "Please Enter Valid Account Number")]
    [MaxLength(20, ErrorMessage = "Please Enter Valid Account Number")]
    public string accountNumber { get; set; }
    [Required(ErrorMessage = "IFSC is required")]
    [MinLength(5, ErrorMessage = "Please Enter Valid IFSC")]
    [MaxLength(15, ErrorMessage = "Please Enter Valid IFSC")]
    public string IFSC { get; set; }
    [Required]
    public string accountType { get; set; }

    public string GetJson()
    {
        AddAccount ObjData = new AddAccount();
        ObjData.Name = this.Name;
        ObjData.Mobile = this.Mobile;
        ObjData.accountNumber = this.accountNumber;
        ObjData.CustomerMobile = this.CustomerMobile;
        ObjData.IFSC = this.IFSC;
        ObjData.accountType = this.accountType;
        ObjData.IFSC = this.IFSC;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}


public class AccountVerifyST : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "IFSC is required")]
    [MinLength(5, ErrorMessage = "Please Enter Valid IFSC")]
    [MaxLength(15, ErrorMessage = "Please Enter Valid IFSC")]
    public string ifsc { get; set; }
    [Required(ErrorMessage = "Account Number is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Account Number")]
    [MaxLength(20, ErrorMessage = "Please Enter Valid Account Number")]
    public string number { get; set; }
    [Required(ErrorMessage = "Account Number is required")]
    [MinLength(10, ErrorMessage = "Please Enter Order ID")]
    [MaxLength(20, ErrorMessage = "Please Enter Order ID")]
    public string OrderID { get; set; }

    public string GetJson()
    {
        AccountVerifyST ObjData = new AccountVerifyST();
        ObjData.ifsc = this.ifsc;
        ObjData.number = this.number;
        ObjData.OrderID = this.OrderID;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        throw new NotImplementedException();
    }
}





public class PayoutTransferRequestST : IValidatableObject
{

    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "Amount is required")]

    public decimal Amount { get; set; }
    [Required(ErrorMessage = "PaymentType is required")]
    public string PaymentType { get; set; }

    [Required(ErrorMessage = "IFSC is required")]
    [MinLength(5, ErrorMessage = "Please Enter Valid IFSC")]
    [MaxLength(15, ErrorMessage = "Please Enter Valid IFSC")]
    public string ifsc { get; set; }
    [Required(ErrorMessage = "Account Number is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Account Number")]
    [MaxLength(20, ErrorMessage = "Please Enter Valid Account Number")]
    public string number { get; set; }
    [Required(ErrorMessage = "OrderID is required")]
    public string OrderID { get; set; }
    [Required(ErrorMessage = "CustomerMobileNo is required")]
    public string CustomerMobileNo { get; set; }

    public string GetJson()
    {
        PayoutTransferRequestST ObjData = new PayoutTransferRequestST();
        ObjData.ifsc = this.ifsc;
        ObjData.number = this.number;
        ObjData.OrderID = this.OrderID;
        ObjData.Amount = this.Amount;
        ObjData.CustomerMobileNo = this.CustomerMobileNo;
        ObjData.PaymentType = this.PaymentType;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(number))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}



public class MoneyTransferRequestST : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "Amount is required")]

    public decimal Amount { get; set; }
    [Required(ErrorMessage = "PaymentType is required")]
    public string PaymentType { get; set; }

    [Required(ErrorMessage = "IFSC is required")]

    public string ifsc { get; set; }
    [Required(ErrorMessage = "Account Number is required")]

    public string number { get; set; }
    [Required(ErrorMessage = "OrderID is required")]
    public string OrderID { get; set; }
    [Required(ErrorMessage = "CustomerMobileNo is required")]
    public string CustomerMobileNo { get; set; }

    public string GetJson()
    {
        MoneyTransferRequestST ObjData = new MoneyTransferRequestST();
        ObjData.ifsc = this.ifsc;
        ObjData.number = this.number;
        ObjData.OrderID = this.OrderID;
        ObjData.Amount = this.Amount;
        ObjData.CustomerMobileNo = this.CustomerMobileNo;
        ObjData.PaymentType = this.PaymentType;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(number))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}
public class MoneyTransferCheckStatusST : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "OrderID is required")]
    public string OrderID { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(OrderID))
        {
            yield return new ValidationResult("Can't Null");
        }
    }

    public string GetJson()
    {
        MoneyTransferCheckStatusST ObjData = new MoneyTransferCheckStatusST();
        ObjData.OrderID = this.OrderID;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}





public class MATM_RequestID : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required]
    public string BCID { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(BCID))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
    public string GetJson()
    {
        MATM_RequestID ObjData = new MATM_RequestID();
        ObjData.BCID = this.BCID;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class MATM_RequestST : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required]
    public string RefID { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(RefID))
        {
            yield return new ValidationResult("Can't Null");
        }
    }

    public string GetJson()
    {
        MATM_RequestST ObjData = new MATM_RequestST();
        ObjData.RefID = this.RefID;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




public class STProfitReportRequest
{
    [Required]
    public string APIID { get; set; }
    [Required]
    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    [Required(ErrorMessage = "ToDayDate is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid ToDayDate")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid ToDayDate")]
    public string ToDayDate { get; set; }


    public string GetJson()
    {
        STProfitReportRequest ObjData = new STProfitReportRequest();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.ToDayDate = this.ToDayDate;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }


}



public class ServiceTotal
{
    public string OpeningBalance { set; get; }
    public string FundAdd { set; get; }
    public string AEPSToMainWallet { set; get; }
    public string Recharge { set; get; }
    public string RechargeCommission { set; get; }
    public string DMT { set; get; }
    public string DMTSurcharge { set; get; }
    public string Payout { set; get; }
    public string PayoutSurcharge { set; get; }
    public string UPITransfer { set; get; }
    public string UPITransferSurcharge { set; get; }
    public string AEPS { set; get; }
    public string AEPSCommission { set; get; }
    public string MATM { set; get; }
    public string MATMCommission { set; get; }
    public string Pan { set; get; }
    public string PanCommission { set; get; }
    public string AadharPay { set; get; }
    public string AadharPaySurcharge { set; get; }
    public string UPI { set; get; }
    public string UPISurcharge { set; get; }

    public ServiceTotal()
    {
        OpeningBalance = "0";
        FundAdd = "0";
        AEPSToMainWallet = "0";
        Recharge = "0";
        RechargeCommission = "0";
        DMT = "0";
        DMTSurcharge = "0";
        Payout = "0";
        PayoutSurcharge = "0";
        UPITransfer = "0";
        UPITransferSurcharge = "0";
        AEPS = "0";
        AEPSCommission = "0";
        MATM = "0";
        MATMCommission = "0";
        Pan = "0";
        PanCommission = "0";
        AadharPay = "0";
        AadharPaySurcharge = "0";
        UPI = "0";
        UPISurcharge = "0";

    }
    public static ServiceTotal GetObject()
    {
        ServiceTotal serviceTotal = new ServiceTotal();
        serviceTotal.OpeningBalance = "0";
        serviceTotal.FundAdd = "0";
        serviceTotal.AEPSToMainWallet = "0";
        serviceTotal.Recharge = "0";
        serviceTotal.RechargeCommission = "0";
        serviceTotal.DMT = "0";
        serviceTotal.DMTSurcharge = "0";
        serviceTotal.Payout = "0";
        serviceTotal.UPITransfer = "0";
        serviceTotal.UPITransferSurcharge = "0";
        serviceTotal.PayoutSurcharge = "0";
        serviceTotal.AEPS = "0";
        serviceTotal.AEPSCommission = "0";
        serviceTotal.MATM = "0";
        serviceTotal.MATMCommission = "0";
        serviceTotal.Pan = "0";
        serviceTotal.PanCommission = "0";
        serviceTotal.AadharPay = "0";
        serviceTotal.AadharPaySurcharge = "0";
        serviceTotal.UPI = "0";
        serviceTotal.UPISurcharge = "0";
        return serviceTotal;
    }


}



public class ServiceTotalSelf
{
    public string Recharge { set; get; } = "0";
    public string RechargeCommission { set; get; } = "0";
    public string DMTPPI { set; get; } = "0";
    public string DMTPPISurcharge { set; get; } = "0";

    public string DMT { set; get; } = "0";
    public string DMTSurcharge { set; get; } = "0";

    public string DMTiserve { set; get; } = "0";
    public string DMTSurchargeiserve { set; get; } = "0";

    public string DMTMavyExpress { set; get; } = "0";
    public string DMTSurchargeMavyExpress { set; get; } = "0";

    public string Payout { set; get; } = "0";
    public string PayoutSurcharge { set; get; } = "0";

    public string AEPSKing { set; get; } = "0";
    public string AEPSKingCommission { set; get; } = "0";

    public string CashDeposit { set; get; } = "0";
    public string CashDepositCommission { set; get; } = "0";

    public string AEPSQueen { set; get; } = "0";
    public string AEPSQueenCommission { set; get; } = "0";

    public string AEPSJack { set; get; } = "0";
    public string AEPSJackCommission { set; get; } = "0";

    public string MATM { set; get; } = "0";
    public string MATMCommission { set; get; } = "0";

    public string MPOS { set; get; } = "0";
    public string MPOSCommission { set; get; } = "0";


    public string Pan { set; get; } = "0";
    public string PanCommission { set; get; } = "0";
    public string AadharPay { set; get; } = "0";
    public string AadharPaySurcharge { set; get; } = "0";
    public string UPI { set; get; } = "0";
    public string UPISurcharge { set; get; } = "0";
    public string MainBalance { set; get; } = "0";
    public string AEPSBalance { set; get; } = "0";

    public string UPITransfer { set; get; } = "0";
    public string UPITransferSurcharge { set; get; } = "0";
    public string LapuCommission { set; get; } = "0";
    public string FundRequest { set; get; } = "0";
    public string UPICashout { set; get; } = "0";
    public string UPICashoutSurchage { set; get; } = "0";

    public string CreditCardBillPay { set; get; } = "0";
    public string CreditCardBillPaySurchage { set; get; } = "0";



    public static ServiceTotalSelf GetServiceTotalInfo(DataTable dt)
    {
        ServiceTotalSelf serviceTotal = new ServiceTotalSelf();
        serviceTotal.Recharge = dt.Rows[0]["Recharge"].ToString();
        serviceTotal.RechargeCommission = dt.Rows[0]["RechargeCommission"].ToString();

        serviceTotal.DMTPPI = dt.Rows[0]["DMTPPI"].ToString();
        serviceTotal.DMTPPISurcharge = dt.Rows[0]["DMTPPISurcharge"].ToString();

        serviceTotal.DMT = dt.Rows[0]["DMT"].ToString();
        serviceTotal.DMTSurcharge = dt.Rows[0]["DMTSurcharge"].ToString();

        serviceTotal.DMTiserve = dt.Rows[0]["DMTiserve"].ToString();
        serviceTotal.DMTSurchargeiserve = dt.Rows[0]["DMTSurchargeiserve"].ToString();

        serviceTotal.DMTMavyExpress = dt.Rows[0]["DMTMavyExpress"].ToString();
        serviceTotal.DMTSurchargeMavyExpress = dt.Rows[0]["DMTSurchargeMavyExpress"].ToString();


        serviceTotal.Payout = dt.Rows[0]["Payout"].ToString();
        serviceTotal.PayoutSurcharge = dt.Rows[0]["PayoutSurcharge"].ToString();

        serviceTotal.AEPSKing = dt.Rows[0]["AEPSKing"].ToString();
        serviceTotal.AEPSKingCommission = dt.Rows[0]["AEPSKingCommission"].ToString();

        serviceTotal.CashDeposit = dt.Rows[0]["CashDeposit"].ToString();
        serviceTotal.CashDepositCommission = dt.Rows[0]["CashDepositCommission"].ToString();

        serviceTotal.AEPSQueen = dt.Rows[0]["AEPSQueen"].ToString();
        serviceTotal.AEPSQueenCommission = dt.Rows[0]["AEPSQueenCommission"].ToString();

        serviceTotal.AEPSJack = dt.Rows[0]["AEPSJack"].ToString();
        serviceTotal.AEPSJackCommission = dt.Rows[0]["AEPSJackCommission"].ToString();



        serviceTotal.MATM = dt.Rows[0]["MATM"].ToString();
        serviceTotal.MATMCommission = dt.Rows[0]["MATMCommission"].ToString();

        serviceTotal.MPOS = dt.Rows[0]["MPOS"].ToString();
        serviceTotal.MPOSCommission = dt.Rows[0]["MPOSCommission"].ToString();


        serviceTotal.Pan = dt.Rows[0]["Pan"].ToString();
        serviceTotal.PanCommission = dt.Rows[0]["PanCommission"].ToString();
        serviceTotal.AadharPay = dt.Rows[0]["AadharPay"].ToString();
        serviceTotal.AadharPaySurcharge = dt.Rows[0]["AadharPaySurcharge"].ToString();
        serviceTotal.UPI = dt.Rows[0]["UPI"].ToString();
        serviceTotal.UPISurcharge = dt.Rows[0]["UPISurcharge"].ToString();
        serviceTotal.MainBalance = dt.Rows[0]["MainBalance"].ToString();
        serviceTotal.AEPSBalance = dt.Rows[0]["AEPSBalance"].ToString();
        serviceTotal.UPITransfer = dt.Rows[0]["UPITransfer"].ToString();
        serviceTotal.LapuCommission = dt.Rows[0]["RechargeCommissionapu"].ToString();
        serviceTotal.UPITransferSurcharge = dt.Rows[0]["UPITransferSurchage"].ToString();
        serviceTotal.FundRequest = dt.Rows[0]["FundRequest"].ToString();
        serviceTotal.UPICashout = dt.Rows[0]["UPICashout"].ToString();
        serviceTotal.UPICashoutSurchage = dt.Rows[0]["UPICashoutSurchage"].ToString();
        serviceTotal.CreditCardBillPay = dt.Rows[0]["CreditCardBillPay"].ToString();
        serviceTotal.CreditCardBillPaySurchage = dt.Rows[0]["CreditCardBillPaySurchage"].ToString();
        return serviceTotal;
    }

    public string GetJson(DataTable dt)
    {
        ServiceTotalSelf serviceTotal = new ServiceTotalSelf();
        serviceTotal.Recharge = dt.Rows[0]["Recharge"].ToString();
        serviceTotal.RechargeCommission = dt.Rows[0]["RechargeCommission"].ToString();
        serviceTotal.DMT = dt.Rows[0]["DMT"].ToString();
        serviceTotal.DMTSurcharge = dt.Rows[0]["DMTSurcharge"].ToString();
        serviceTotal.Payout = dt.Rows[0]["Payout"].ToString();
        serviceTotal.PayoutSurcharge = dt.Rows[0]["PayoutSurcharge"].ToString();
        serviceTotal.AEPSKing = dt.Rows[0]["AEPSKing"].ToString();
        serviceTotal.AEPSKingCommission = dt.Rows[0]["AEPSKingCommission"].ToString();
        serviceTotal.CashDeposit = dt.Rows[0]["CashDeposit"].ToString();
        serviceTotal.CashDepositCommission = dt.Rows[0]["CashDepositCommission"].ToString();

        serviceTotal.AEPSQueen = dt.Rows[0]["AEPSQueen"].ToString();
        serviceTotal.AEPSQueenCommission = dt.Rows[0]["AEPSQueenCommission"].ToString();
        serviceTotal.AEPSJack = dt.Rows[0]["AEPSJack"].ToString();
        serviceTotal.AEPSJackCommission = dt.Rows[0]["AEPSJackCommission"].ToString();
        serviceTotal.MATM = dt.Rows[0]["MATM"].ToString();
        serviceTotal.MATMCommission = dt.Rows[0]["MATMCommission"].ToString();
        serviceTotal.Pan = dt.Rows[0]["Pan"].ToString();
        serviceTotal.PanCommission = dt.Rows[0]["PanCommission"].ToString();
        serviceTotal.AadharPay = dt.Rows[0]["AadharPay"].ToString();
        serviceTotal.AadharPaySurcharge = dt.Rows[0]["AadharPaySurcharge"].ToString();
        serviceTotal.UPI = dt.Rows[0]["UPI"].ToString();
        serviceTotal.UPISurcharge = dt.Rows[0]["UPISurcharge"].ToString();
        serviceTotal.MainBalance = dt.Rows[0]["MainBalance"].ToString();
        serviceTotal.AEPSBalance = dt.Rows[0]["AEPSBalance"].ToString();
        serviceTotal.UPITransfer = dt.Rows[0]["UPITransfer"].ToString();
        serviceTotal.UPITransferSurcharge = dt.Rows[0]["UPITransferSurchage"].ToString();
        string jsonString;
        jsonString = JsonConvert.SerializeObject(serviceTotal);
        return jsonString;
    }
}


public class UPITransferRequestST : IValidatableObject
{

    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    public decimal Amount { get; set; }


    [Required(ErrorMessage = "UPIID is required")]
    public string UPIID { get; set; }
    [Required(ErrorMessage = "OrderID is required")]
    public string OrderID { get; set; }
    [Required(ErrorMessage = "CustomerMobileNo is required")]
    public string CustomerMobileNo { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "email is required")]
    public string email { get; set; }
    public decimal latitude { get; set; }
    public decimal longitude { get; set; }
    public string Address { get; set; }
    public string Source { get; set; }
    public string IP { get; set; }
    public Int32 ServiceID { get; set; }

    public string GetJson()
    {
        UPITransferRequestST ObjData = new UPITransferRequestST();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.Amount = this.Amount;
        ObjData.UPIID = this.UPIID;
        ObjData.OrderID = this.OrderID;
        ObjData.Name = this.Name;
        ObjData.email = this.email;
        ObjData.CustomerMobileNo = this.CustomerMobileNo;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(UPIID))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}


public class UPIVerfication : IValidatableObject
{

    public string APIID { get; set; }
    public string Token { get; set; }

    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "OrderID is required")]
    public string OrderID { get; set; }


    [Required(ErrorMessage = "UPIID is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid UPIID")]
    [MaxLength(100, ErrorMessage = "Please Enter Valid UPIID")]
    public string Number { get; set; }


    public string GetJson()
    {
        UPIVerfication ObjData = new UPIVerfication();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.Number = this.Number;
        ObjData.OrderID = this.OrderID;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Number))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}



public class AxisAountOpning
{
    public string APIID { get; set; }
    public string Token { get; set; }

    [Required]
    public string MethodName { get; set; }
    public string merchantcode { get; set; }
    public string type { get; set; }
    public string GetJson()
    {
        AxisAountOpning ObjData = new AxisAountOpning();
        ObjData.merchantcode = this.merchantcode;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.type = this.type;


        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class UTIPanPurchase
{
    public string psaid { get; set; }
    public string OrderID { get; set; }
    public string CouponType { get; set; }
    public string transactiondate { get; set; }
    public string totalcoupon_digital { get; set; }
    public string totalcoupon_physical { get; set; }
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }

    public string GetJson()
    {
        UTIPanPurchase ObjData = new UTIPanPurchase();
        ObjData.psaid = this.psaid;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.OrderID = this.OrderID;
        ObjData.CouponType = this.CouponType;
        ObjData.transactiondate = this.transactiondate;
        ObjData.totalcoupon_digital = this.totalcoupon_digital;
        ObjData.totalcoupon_physical = this.totalcoupon_physical;
        ObjData.OrderID = this.OrderID;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class UTIPanCheckStatus
{
    public string requestid { get; set; }
    public string OrderID { get; set; }
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }

    public string GetJson()
    {
        UTIPanCheckStatus ObjData = new UTIPanCheckStatus();
        ObjData.OrderID = this.OrderID;
        ObjData.requestid = this.requestid;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class AEPSTWORegistration
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string accessmodetype { get; set; }
    public string adhaarnumber { get; set; }
    public string mobilenumber { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string referenceno { get; set; }
    public string submerchantid { get; set; }
    public string timestamp { get; set; }
    public string data { get; set; }
    public string ipaddress { get; set; }

    public string GetJson()
    {
        AEPSTWORegistration ObjData = new AEPSTWORegistration();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.referenceno = this.referenceno;
        ObjData.submerchantid = this.submerchantid;
        ObjData.timestamp = this.timestamp;
        ObjData.data = this.data;
        ObjData.ipaddress = this.ipaddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}


public class BillPayRequest
{
    public string APIID { get; set; }

    public string Token { get; set; }
    public string MethodName { get; set; }
    public string ClientRefId { get; set; }
    public string Number { get; set; }
    public string SPKey { get; set; }
    public string Amount { get; set; }
    public string TelecomCircleID { get; set; }
    public string Optional1 { get; set; }
    public string Optional2 { get; set; }
    public string Optional3 { get; set; }
    public string Optional4 { get; set; }
    public string Optional5 { get; set; }
    public string Optional6 { get; set; }
    public string Optional7 { get; set; }
    public string Optional8 { get; set; }
    public string Optional9 { get; set; }

    public BillPayRequest()
    {

        APIID = SONITechnoCredentrial.APIID;
        Token = SONITechnoCredentrial.Token;
        MethodName = "billpay";
        Optional1 = "";
        Optional2 = "";
        Optional3 = "";
        Optional4 = "";
        Optional5 = "";
        Optional6 = "";
        Optional7 = "";
        Optional8 = "";
        Optional9 = "";
    }

    public string GetJson()
    {
        BillPayRequest ObjData = new BillPayRequest();
        ObjData.ClientRefId = this.ClientRefId;
        ObjData.Number = this.Number;
        ObjData.SPKey = this.SPKey;
        ObjData.Amount = this.Amount;
        ObjData.TelecomCircleID = this.TelecomCircleID;
        ObjData.Optional1 = this.Optional1;
        ObjData.Optional2 = this.Optional2;
        ObjData.Optional3 = this.Optional3;
        ObjData.Optional4 = this.Optional4;
        ObjData.Optional5 = this.Optional5;
        ObjData.Optional6 = this.Optional6;
        ObjData.Optional7 = this.Optional7;
        ObjData.Optional8 = this.Optional8;
        ObjData.Optional9 = this.Optional9;
        ObjData.APIID = this.APIID;
        ObjData.Token = this.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}


public class BillFetchRequest
{
    public string APIID { get; set; }

    public string Token { get; set; }
    public string MethodName { get; set; }
    public string ClientRefId { get; set; }
    public string Number { get; set; }
    public string SPKey { get; set; }
    public string TelecomCircleID { get; set; }
    public string Optional1 { get; set; }
    public string Optional2 { get; set; }
    public string Optional3 { get; set; }
    public string Optional4 { get; set; }
    public string Optional5 { get; set; }
    public string Optional6 { get; set; }
    public string Optional7 { get; set; }
    public string Optional8 { get; set; }
    public string Optional9 { get; set; }

    public BillFetchRequest()
    {
        APIID = SONITechnoCredentrial.APIID;
        Token = SONITechnoCredentrial.Token;
        MethodName = "billfetch";
    }

    public string GetJson()
    {
        BillFetchRequest ObjData = new BillFetchRequest();
        ObjData.ClientRefId = this.ClientRefId;
        ObjData.Number = this.Number;
        ObjData.SPKey = this.SPKey;
        ObjData.TelecomCircleID = this.TelecomCircleID;
        ObjData.Optional1 = this.Optional1;
        ObjData.Optional2 = this.Optional2;
        ObjData.Optional3 = this.Optional3;
        ObjData.Optional4 = this.Optional4;
        ObjData.Optional5 = this.Optional5;
        ObjData.Optional6 = this.Optional6;
        ObjData.Optional7 = this.Optional7;
        ObjData.Optional8 = this.Optional8;
        ObjData.Optional9 = this.Optional9;
        ObjData.APIID = this.APIID;
        ObjData.Token = this.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}



public class BillFetchResult
{
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public double dueamount { get; set; }
    public string duedate { get; set; }
    public string customername { get; set; }
    public string billnumber { get; set; }
    public string billdate { get; set; }
    public string acceptPartPay { get; set; }
    public string BBPSCharges { get; set; }
    public string BillUpdate { get; set; }
    public string RequestID { get; set; }
    public string ClientRefId { get; set; }
}


public class AgentRegistrationPPI
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string merchant_code { get; set; }
    public string name { get; set; }
    public string mobile { get; set; }
    public string email { get; set; }
    public string pan_number { get; set; }
    public string firmname { get; set; }
    public string address { get; set; }
    public string pincode { get; set; }
    public string state { get; set; }

    public AgentRegistrationPPI()
    {

        APIID = SONITechnoCredentrial.APIID;
        Token = SONITechnoCredentrial.Token;
        MethodName = "agentregister";
    }
}



public class PPIIni
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string merchant_code { get; set; }
    public string refid { get; set; }
    public string redirect_url { get; set; }

    public PPIIni()
    {
        APIID = SONITechnoCredentrial.APIID;
        Token = SONITechnoCredentrial.Token;
        MethodName = "ppiin";
        redirect_url = this.redirect_url;
    }

}

