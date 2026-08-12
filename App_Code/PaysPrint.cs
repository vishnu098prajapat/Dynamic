using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for PaysPrint
/// </summary>
public class PaysPrint
{
    public static string JWTencryptionKey = "";
    public static string AuthKey = "";
    public static string AESKEY = "";
    public static string AESIV = "";
    public static string PARTNERID = "";
    public static string Prefix = "";
    public static string Authorisedsign = "=";
    public static string URL = "";
    public static string OnBoradCall = "";
    public static string RedirectionPan = "";

    public PaysPrint()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}
public class PaysPrintPostCall
{
    public static string PostCall(string url, string Token)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(PaysPrint.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(PaysPrint.URL + url, Method.Post);

        request.AddHeader("Token", Token);
        request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

 public async static Task<string> PostCallAsync(string url, string Token)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(PaysPrint.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(PaysPrint.URL + url, Method.Post);

        request.AddHeader("Token", Token);
        request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
        RestResponse response =await client.ExecuteAsync(request);
        return response.Content;
    }

    public static string PostCallBodyEncrption(string url, string Token, string Body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(PaysPrint.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(PaysPrint.URL + url, Method.Post);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Token", Token);
        request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
        request.AddStringBody("{\"body\":\"" + Body + "\"}", DataFormat.Json);
        // request.AddParameter("application/json", "{\"body\":\"" + Body + "\"}", ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content.ToString();
    }

    public static async Task<string> PostCallBodyEncrptionAsync(string url, string Token, string Body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(PaysPrint.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(PaysPrint.URL + url, Method.Post);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Token", Token);
        request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
        request.AddStringBody("{\"body\":\"" + Body + "\"}", DataFormat.Json);
        // request.AddParameter("application/json", "{\"body\":\"" + Body + "\"}", ParameterType.RequestBody);
        RestResponse response =await client.ExecuteAsync(request);
        return response.Content.ToString();
    }

    public static async Task<string> PostCallWithBodyAsync(string url, string Token, string body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(PaysPrint.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(PaysPrint.URL + url, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Token", Token);
        request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
        request.AddStringBody(body, DataFormat.Json);
        //request.AddParameter("application/json", body, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content.Replace("\r\n", "");
    }

    public static string PostCallWithBody(string url, string Token, string body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(PaysPrint.URL + url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(PaysPrint.URL + url, Method.Post);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Token", Token);
        request.AddHeader("Authorisedkey", PaysPrint.AuthKey);

        request.AddStringBody(body, DataFormat.Json);
        //request.AddParameter("application/json", body, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
       
        return response.Content.Replace("\r\n", "");
    }
}
public class PaysPrintDMTCheckCustomer : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string mobile { get; set; }
    [Required]
    public string bank3_flag { get; set; }

    public PaysPrintDMTCheckCustomer()
    {
        bank3_flag = "no";
    }

    public string GetJson()
    {
        PaysPrintDMTCheckCustomer ObjData = new PaysPrintDMTCheckCustomer();
        ObjData.mobile = this.mobile;
        ObjData.bank3_flag = this.bank3_flag;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}


public class PaysPrintDMTCustomerRegister : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string mobile { get; set; }
    [Required(ErrorMessage = "firstname is required")]
    [MinLength(3, ErrorMessage = "Please Enter Valid Firstname")]
    public string firstname { get; set; }
    [Required(ErrorMessage = "lastname is required")]
    [MinLength(3, ErrorMessage = "Please Enter Valid lastname")]
    public string lastname { get; set; }
    [Required(ErrorMessage = "address is required")]
    public string address { get; set; }
    [Required(ErrorMessage = "otp is required")]
    public string otp { get; set; }
    [Required(ErrorMessage = "pincode is required")]
    [MinLength(6, ErrorMessage = "Please Enter Valid pincode")]
    [MaxLength(6, ErrorMessage = "Please Enter Valid pincode")]
    public string pincode { get; set; }
    [Required(ErrorMessage = "pincode is required")]
    public string stateresp { get; set; }
    [Required(ErrorMessage = "pincode is required")]
    public string bank3_flag { get; set; }
    [Required(ErrorMessage = "pincode is required")]
    public string dob { get; set; }
    [Required(ErrorMessage = "pincode is required")]
    public string gst_state { get; set; }
    public string ekyc_id { get; set; }
    public string GetJson()
    {
        PaysPrintDMTCustomerRegister ObjData = new PaysPrintDMTCustomerRegister();
        ObjData.mobile = this.mobile;
        ObjData.firstname = this.firstname;
        ObjData.lastname = this.lastname;
        ObjData.address = this.address;
        ObjData.otp = this.otp;
        ObjData.pincode = this.pincode;
        ObjData.stateresp = this.stateresp;
        ObjData.ekyc_id = this.ekyc_id;
        ObjData.bank3_flag = this.bank3_flag;
        ObjData.dob = this.dob;
        ObjData.gst_state = this.gst_state;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}

public class DeleteBeniPaysP
{

    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    public string mobile { get; set; }
    public string bene_id { get; set; }

    public string GetJson()
    {
        DeleteBeniPaysP ObjData = new DeleteBeniPaysP();
        ObjData.mobile = this.mobile;
        ObjData.bene_id = this.bene_id;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class PaysPrintDMTBeniRegister : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string mobile { get; set; }
    [Required(ErrorMessage = "Benename is required")]
    public string benename { get; set; }
    [Required(ErrorMessage = "BankID is required")]
    public string bankid { get; set; }
    [Required(ErrorMessage = "Accno is required")]
    public string accno { get; set; }
    [Required(ErrorMessage = "IFSCCODE is required")]
    public string ifsccode { get; set; }
    public string verified { get; set; }
    public string gst_state { get; set; }
    public string dob { get; set; }
    public string address { get; set; }
    public string pincode { get; set; }

    public string GetJson()
    {
        PaysPrintDMTBeniRegister ObjData = new PaysPrintDMTBeniRegister();
        ObjData.mobile = this.mobile;
        ObjData.benename = this.benename;
        ObjData.bankid = this.bankid;
        ObjData.accno = this.accno;
        ObjData.ifsccode = this.ifsccode;
        ObjData.verified = this.verified;
        ObjData.gst_state = this.gst_state;
        ObjData.dob = this.dob;
        ObjData.address = this.address;
        ObjData.pincode = this.pincode;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}




public class PaysPrintDMTBeniList : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile No")]
    public string mobile { get; set; }


    public string GetJson()
    {
        PaysPrintDMTBeniList ObjData = new PaysPrintDMTBeniList();
        ObjData.mobile = this.mobile;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class PaysPrintDMTTransfer : IValidatableObject
{
    public string mobile { get; set; }
    public string referenceid { get; set; }
    public string pipe { get; set; }
    public string pincode { get; set; }
    public string address { get; set; }
    public string dob { get; set; }
    public string gst_state { get; set; }
    public int bene_id { get; set; }
    public string txntype { get; set; }
    public decimal amount { get; set; }

    public string lat { get; set; }
    public string stateresp { get; set; }
    public string otp { get; set; }
    public string @long { get; set; }

    public string GetJson()
    {
        PaysPrintDMTTransfer ObjData = new PaysPrintDMTTransfer();
        ObjData.mobile = this.mobile;
        ObjData.referenceid = this.referenceid;
        ObjData.pipe = this.pipe;
        ObjData.pincode = this.pincode;
        ObjData.address = this.address;
        ObjData.dob = this.dob;
        ObjData.gst_state = this.gst_state;
        ObjData.bene_id = this.bene_id;
        ObjData.txntype = this.txntype;
        ObjData.amount = this.amount;

        ObjData.lat = this.lat;
        ObjData.stateresp = this.stateresp;
        ObjData.otp = this.otp;
        ObjData.@long = this.@long;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        throw new NotImplementedException();
    }
}

public class PaysAEPSOnBoard
{
    public string merchantcode { get; set; }
    public string mobile { get; set; }
    public string is_new { get; set; }
    public string email { get; set; }
    public string firm { get; set; }
    public string callback { get; set; }
    public string GetJson()
    {
        PaysAEPSOnBoard ObjData = new PaysAEPSOnBoard();
        ObjData.merchantcode = this.merchantcode;
        ObjData.mobile = this.mobile;
        ObjData.is_new = this.is_new;
        ObjData.email = this.email;
        ObjData.firm = this.firm;
        ObjData.callback = this.callback;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}

public class PaysBeniFetch
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    public string mobile { get; set; }
    public int beneid { get; set; }

    public string GetJson()
    {
        PaysBeniFetch ObjData = new PaysBeniFetch();
        ObjData.mobile = this.mobile;
        ObjData.beneid = this.beneid;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }


}

public class DMTRequestPaysprint
{
    public string APIID { get; set; }

    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    public string mobile { get; set; }
    public string OrderID { get; set; }
    public string pipe { get; set; }
    public string AccountNO { get; set; }
    public string IFSC { get; set; }
    public int BeniID { get; set; }
    public string Mode { get; set; }
    public string BankName { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }

    public string stateresp { get; set; }
    public string OTP { get; set; }

    public string lat { get; set; }
    public string @long { get; set; }

    public string GetJson()
    {
        DMTRequestPaysprint ObjData = new DMTRequestPaysprint();
        ObjData.mobile = this.mobile;
        ObjData.OrderID = this.OrderID;
        ObjData.pipe = this.pipe;
        ObjData.AccountNO = this.AccountNO;
        ObjData.IFSC = this.IFSC;
        ObjData.BeniID = this.BeniID;
        ObjData.Mode = this.Mode;
        ObjData.Amount = this.Amount;
        ObjData.BankName = this.BankName;
        ObjData.Name = this.Name;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.stateresp = this.stateresp;
        ObjData.lat = this.lat;
        ObjData.stateresp = this.stateresp;
        ObjData.@long = this.@long;
        ObjData.OTP = this.OTP;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}

public class PaysprintPayoutBankAdd
{
    public int bankid { get; set; }
    public string merchant_code { get; set; }
    public string account { get; set; }
    public string ifsc { get; set; }
    public string name { get; set; }
    public string account_type { get; set; }

    public string GetJson()
    {
        PaysprintPayoutBankAdd ObjData = new PaysprintPayoutBankAdd();
        ObjData.bankid = this.bankid;
        ObjData.merchant_code = this.merchant_code;
        ObjData.account = this.account;
        ObjData.ifsc = this.ifsc;
        ObjData.name = this.name;
        ObjData.account_type = this.account_type;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}



public class CreditCardBillPayment
{
    public string refid { get; set; }
    public string name { get; set; }
    public string mobile { get; set; }
    public string card_number { get; set; }
    public string amount { get; set; }
    public string remarks { get; set; }
    public string network { get; set; }

    public string GetJson()
    {
        CreditCardBillPayment ObjData = new CreditCardBillPayment();
        ObjData.refid = this.refid;
        ObjData.name = this.name;
        ObjData.mobile = this.mobile;
        ObjData.card_number = this.card_number;
        ObjData.amount = this.amount;
        ObjData.remarks = this.remarks;
        ObjData.network = this.network;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




public class CreditCardBillPaymentOTPVerify
{
    public string refid { get; set; }
    public string name { get; set; }
    public string mobile { get; set; }
    public string card_number { get; set; }
    public string amount { get; set; }
    public string otp { get; set; }
    public string remarks { get; set; }
    public string network { get; set; }
    public string stateresp { get; set; }

    public string GetJson()
    {
        CreditCardBillPaymentOTPVerify ObjData = new CreditCardBillPaymentOTPVerify();
        ObjData.refid = this.refid;
        ObjData.name = this.name;
        ObjData.mobile = this.mobile;
        ObjData.card_number = this.card_number;
        ObjData.amount = this.amount;
        ObjData.otp = this.otp;
        ObjData.remarks = this.remarks;
        ObjData.network = this.network;
        ObjData.stateresp = this.stateresp;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class PaysNSDL
{
    public string refid { get; set; }
    public string title { get; set; }
    public string firstname { get; set; }
    public string lastname { get; set; }
    public string middlename { get; set; }
    public string email { get; set; }
    public string mode { get; set; }
    public string gender { get; set; }
    public string redirect_url { get; set; }
    public string merchant_code { get; set; }
    public string kyctype { get; set; }


    public string GetJson()
    {
        PaysNSDL ObjData = new PaysNSDL();
        ObjData.refid = this.refid;
        ObjData.title = this.title;
        ObjData.firstname = this.firstname;
        ObjData.lastname = this.lastname;
        ObjData.middlename = this.middlename;
        ObjData.mode = this.mode;
        ObjData.email = this.email;
        ObjData.gender = this.gender;
        ObjData.redirect_url = this.redirect_url;
        ObjData.merchant_code = this.merchant_code;
        ObjData.kyctype = this.kyctype;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}





public class PayoutSettlement
{
    public string bene_id { get; set; }
    public string amount { get; set; }
    public string refid { get; set; }
    public string mode { get; set; }

    public string GetJson()
    {
        PayoutSettlement ObjData = new PayoutSettlement();
        ObjData.bene_id = this.bene_id;
        ObjData.amount = this.amount;
        ObjData.refid = this.refid;
        ObjData.mode = this.mode;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class PasyRecharge
{
    public string @operator { get; set; }
    public string canumber { get; set; }
    public Int32 amount { get; set; }
    public string referenceid { get; set; }

    public string GetJson()
    {
        PasyRecharge ObjData = new PasyRecharge();
        ObjData.@operator = this.@operator;
        ObjData.canumber = this.canumber;
        ObjData.amount = this.amount;
        ObjData.referenceid = this.referenceid;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }


}


public class DMTRequestekyc
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }

    public string mobile { get; set; }
    public string lat { get; set; }
    public string @long { get; set; }
    public string aadhaar_number { get; set; }
    public string aadhaar_no { get; set; }
    public string data { get; set; }


    public string GetJson()
    {
        DMTRequestekyc ObjData = new DMTRequestekyc();
        ObjData.mobile = this.mobile;
        ObjData.lat = this.lat;
        ObjData.MethodName = this.MethodName;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.@long = this.@long;
        ObjData.aadhaar_number = this.aadhaar_number;
        ObjData.aadhaar_no = this.aadhaar_no;
        ObjData.data = this.data;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}





public class PaysPrintDMTCustomerRegisterAirtel
{
    public string mobile { get; set; }
    public string otp { get; set; }
    public string stateresp { get; set; }
    public string accessmode { get; set; }
    public string is_iris { get; set; }
    public string data { get; set; }
    public string GetJson()
    {
        PaysPrintDMTCustomerRegisterAirtel ObjData = new PaysPrintDMTCustomerRegisterAirtel();
        ObjData.mobile = this.mobile;
        ObjData.otp = this.otp;
        ObjData.stateresp = this.stateresp;
        ObjData.is_iris = this.is_iris;
        ObjData.accessmode = this.accessmode;
        ObjData.data = this.data;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}