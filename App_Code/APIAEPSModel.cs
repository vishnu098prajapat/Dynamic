using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for APIAEPSModel
/// </summary>
public class APIAEPSModel
{
    public APIAEPSModel()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}

public class AEPSONB
{
    public String APIID;
    public String Token;
    public String MethodName;
    public DateTime timestamp;
    public double latitude;
    public double longitude;
    public string ipAddress;
    public string merchantLoginId;
    public string merchantLoginPin;
    public string merchantPhoneNumber;
    public string firstName;
    public string lastName;
    public string middleName;
    public string merchantAddress1;
    public string merchantState;
    public string merchantCityName;
    public string merchantDistrictName;
    public string merchantPinCode;
    public string emailId;
    public string userPan;
    public string aadhaarNumber;
    public string gstinNumber;
    public string companyOrShopPan;
    public String companyBankAccountNumber;
    public String bankIfscCode;
    public String companyBankName;
    public String bankBranchName;
    public String bankAccountName;
    public String shopAddress;
    public String shopCity;
    public String shopDistrict;
    public Int32 shopState;
    public String shopPincode;
    public Double shopLatitude;
    public Double shopLongitude;
}




public class AEPSSendOTP
{
    public String APIID;
    public String Token;
    public String MethodName;
    public string merchantLoginId { get; set; }
    public string mobileNumber { get; set; }
    public string aadharNumber { get; set; }
    public string panNumber { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
}


public class AEPSverifyOTP
{
    public String APIID;
    public String Token;
    public String MethodName;
    public string merchantLoginId { get; set; }
    public string otp { get; set; }
    public string primaryKeyId { get; set; }
    public string encodeFPTxnId { get; set; }
}



public class AEPSKYC
{
    public String APIID;
    public String Token;
    public String MethodName;
    public string merchantLoginId { get; set; }
    public int primaryKeyId { get; set; }
    public string encodeFPTxnId { get; set; }
    public string requestRemarks { get; set; }
    public string nationalBankIdentificationNumber { get; set; }
    public string indicatorforUID { get; set; }
    public string adhaarNumber { get; set; }
    public string devicename { get; set; }
    public string capture { get; set; }
}



public class AEPSAuthentication
{
    public String APIID;
    public String Token;
    public String MethodName;
    public bool isFacialTan { get; set; }
    public string OrderID { get; set; }
    public string merchantLoginId { get; set; }
    public string mobileNumber { get; set; }
    public string nationalBankIdentificationNumber { get; set; }
    public string indicatorforUID { get; set; }
    public string adhaarNumber { get; set; }
    public string Source { get; set; }

    public string devicename { get; set; }
    public string capture { get; set; }
    public string IP { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
}


public class AEPSFingpayBE
{
    public String APIID;
    public String Token;
    public String MethodName;
    public bool isFacialTan { get; set; }
    public string OrderID { get; set; }
    public string merchantLoginId { get; set; }
    public string mobileNumber { get; set; }
    public string CustomerNO { get; set; }
    public string Amount { get; set; }
    public string BankName { get; set; }
    public string Source { get; set; }
    public string nationalBankIdentificationNumber { get; set; }
    public string indicatorforUID { get; set; }
    public string adhaarNumber { get; set; }
    public string devicename { get; set; }
    public string capture { get; set; }
    public string IP { get; set; }
    public string TType { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }

    public string TransactionId { get; set; }
    public string UidaiDataTxn { get; set; }
    public string txnReferenceNo { get; set; }
    public string authRequestId { get; set; }
    public string requestid { get; set; }
}
