using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for InstantPayout
/// </summary>
/// 

public class BiometricData
{
    public string encryptedAadhaar { get; set; }
    public string dc { get; set; }
    public string ci { get; set; }
    public string hmac { get; set; }
    public string dpId { get; set; }
    public string mc { get; set; }
    public string pidDataType { get; set; }
    public string sessionKey { get; set; }
    public string mi { get; set; }
    public string rdsId { get; set; }
    public string errCode { get; set; }
    public string errInfo { get; set; }
    public string fCount { get; set; }
    public string fType { get; set; }
    public int iCount { get; set; }
    public string iType { get; set; }
    public int pCount { get; set; }
    public string pType { get; set; }
    public string srno { get; set; }
    public string sysid { get; set; }
    public string ts { get; set; }
    public string pidData { get; set; }
    public string qScore { get; set; }
    public string nmPoints { get; set; }
    public string rdsVer { get; set; }
}

public class ActionKYVRoot
{
    public string type { get; set; }
    public string bankiin { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string externalRef { get; set; }
    public BiometricData biometricData { get; set; }

    public string GetJson()
    {
        ActionKYVRoot signupOTPVerify = new ActionKYVRoot();
        signupOTPVerify.latitude = this.latitude;
        signupOTPVerify.longitude = this.longitude;
        signupOTPVerify.type = this.type;
        signupOTPVerify.bankiin = this.bankiin;
        signupOTPVerify.externalRef = this.externalRef;
        signupOTPVerify.biometricData = this.biometricData;


        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}

public class InstantPayout
{
    public readonly static string token = "";
    public readonly static string debit_account = "";
    public readonly static string accountNumber = "";
    public readonly static string X_Ipay_Auth_Code = "";
    public readonly static string X_Ipay_Client_Id = "=";
    public readonly static string X_Ipay_Client_Secret = "";
    public readonly static string X_Ipay_Endpoint_Ip = "103.76.249.170";
    public readonly static string bankId = "0";
    public readonly static string bankProfileId = "0";
    public readonly static string Url = "https://api.instantpay.in/payments/";
    public readonly static string AccountVerifyUrl = "https://api.instantpay.in/identity/";
    public readonly static string CheckStatus = "https://api.instantpay.in/reports/txnStatus";
    public readonly static string KYCURL = "https://api.instantpay.in/user/outlet/signup/";
    public readonly static string DMTURL = "https://api.instantpay.in/fi/remit/out/domestic/";
    public readonly static string EncryptionKey = "";
    public static string GetEncryptAadhar(string data)
    {
        string EncryptionKey = InstantPayout.token;
        byte[] IV = ASCIIEncoding.ASCII.GetBytes("91543c0ce2ff7bf4");
        byte[] clearBytes = Encoding.Unicode.GetBytes(data);
        AesCryptoServiceProvider crypt_provider;
        crypt_provider = new AesCryptoServiceProvider();
        crypt_provider.KeySize = 256;
        crypt_provider.Key = ASCIIEncoding.ASCII.GetBytes(EncryptionKey);
        crypt_provider.IV = IV;
        crypt_provider.Mode = CipherMode.CBC;
        crypt_provider.Padding = PaddingMode.PKCS7;
        ICryptoTransform transform = crypt_provider.CreateEncryptor();
        byte[] encrypted_bytes = transform.TransformFinalBlock(ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length);
        byte[] encryptedData = new byte[encrypted_bytes.Length + IV.Length];
        IV.CopyTo(encryptedData, 0);
        encrypted_bytes.CopyTo(encryptedData, IV.Length);
        data = Convert.ToBase64String(encryptedData);
        return data;
    }

    public readonly static string[] arrTemplate = new string[]
    {
        "TXN",
        "TUP",
        "RPI",
        "UAD",
        "IAC",
        "IAT",
        "AAB",
        "IAB",
        "ISP",
        "DID",
        "DTX",
        "IAN",
        "IRA",
        "DTB",
        "RBT",
        "SPE",
        "SPD",
        "UED",
        "IEC",
        "IRT",
        "ITI",
        "TSU",
        "IPE",
        "ISE",
        "TRP",
        "OUI",
        "ODI",
        "TDE",
        "DLS",
        "RNF",
        "RAR",
        "IVC",
        "IUA",
        "SNA",
        "ERR",
        "FAB",
        "TRP",
        "ERR",
        "UFC",
        "OLR",
        "OTP",
        "EOP",
        "ONV",
        "RAB",

    };
}



public class InstantPayoutWithAccount
{
    public readonly static string token = "709840ae4ac15439709840ae4ac15439";
    public readonly static string debit_account = "0";
    public readonly static string accountNumber = "042663400002636";
    public readonly static string X_Ipay_Auth_Code = "1";
    public readonly static string X_Ipay_Client_Id = "YWY3OTAzYzNlM2ExZTJlOd3dLX14sW66Ywa5W9QXz5U=";
    public readonly static string X_Ipay_Client_Secret = "a8f0212e411045bd648353773ed32433de16e6ce9611f445b827b6eb41e50293";
    public readonly static string X_Ipay_Endpoint_Ip = "122.176.125.90";
    public readonly static string bankId = "0";
    public readonly static string bankProfileId = "33709572112";
    public readonly static string Url = "https://api.instantpay.in/payments/";
    public readonly static string AccountVerifyUrl = "https://api.instantpay.in/identity/";
    public readonly static string CheckStatus = "https://api.instantpay.in/reports/txnStatus";
    public readonly static string KYCURL = "https://api.instantpay.in/user/outlet/signup/";
    public readonly static string DMTURL = "https://api.instantpay.in/fi/remit/out/domestic/";
    public readonly static string EncryptionKey = "";
   
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Payer
{
    public string bankId { get; set; }
    public string bankProfileId { get; set; }
    public string accountNumber { get; set; }

    public Payer()
    {
        bankId = InstantPayout.bankId;
        bankProfileId = InstantPayout.bankProfileId;
        accountNumber = InstantPayout.debit_account;
    }
}


public class PayerWithAccount
{
    public string bankId { get; set; }
    public string bankProfileId { get; set; }
    public string accountNumber { get; set; }

    public PayerWithAccount()
    {
        bankId = InstantPayoutWithAccount.bankId;
        bankProfileId = InstantPayoutWithAccount.bankProfileId;
        accountNumber = InstantPayoutWithAccount.accountNumber;
    }
}

public class Payee
{
    public string name { get; set; }
    public string accountNumber { get; set; }
    public string bankIfsc { get; set; }
}

public class RootInstantPayout
{
    public Payer payer { get; set; }
    public Payee payee { get; set; }
    public string transferMode { get; set; }
    public string transferAmount { get; set; }
    public string externalRef { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string remarks { get; set; }
    public string alertEmail { get; set; }
    public string purpose { get; set; }




    public string GetJson()
    {
        RootInstantPayout ObjData = new RootInstantPayout();
        ObjData.payer = this.payer;
        ObjData.payee = this.payee;
        ObjData.transferMode = this.transferMode;
        ObjData.transferAmount = this.transferAmount;
        ObjData.externalRef = this.externalRef;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.remarks = this.remarks;
        ObjData.alertEmail = "v@gmail.com";
        ObjData.purpose = "OTHERS";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class RootInstantPayoutWithAccount
{
    public PayerWithAccount payer { get; set; }
    public Payee payee { get; set; }
    public string transferMode { get; set; }
    public string transferAmount { get; set; }
    public string externalRef { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string remarks { get; set; }
    public string alertEmail { get; set; }
    public string purpose { get; set; }




    public string GetJson()
    {
        RootInstantPayoutWithAccount ObjData = new RootInstantPayoutWithAccount();
        ObjData.payer = this.payer;
        ObjData.payee = this.payee;
        ObjData.transferMode = this.transferMode;
        ObjData.transferAmount = this.transferAmount;
        ObjData.externalRef = this.externalRef;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.remarks = this.remarks;
        ObjData.alertEmail = "v@gmail.com";
        ObjData.purpose = "OTHERS";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class AccountVerifyInstantPay
{
    public Payee payee { get; set; }
    public string externalRef { get; set; }
    public string consent { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string isCached { get; set; }

    public AccountVerifyInstantPay()
    {
        isCached = "1";
        consent = "Y";
    }

    public string GetJson()
    {
        AccountVerifyInstantPay ObjData = new AccountVerifyInstantPay();
        ObjData.payee = this.payee;
        ObjData.externalRef = this.externalRef;
        ObjData.consent = this.consent;
        ObjData.externalRef = this.externalRef;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.isCached = this.isCached;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class UPIVerifyInstantPay
{
    public Payee payee { get; set; }
    public string externalRef { get; set; }
    public string consent { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string isCached { get; set; }

    public UPIVerifyInstantPay()
    {
        isCached = "1";
        consent = "Y";
    }

    public string GetJson()
    {
        UPIVerifyInstantPay ObjData = new UPIVerifyInstantPay();
        ObjData.payee = this.payee;
        ObjData.externalRef = this.externalRef;
        ObjData.consent = this.consent;
        ObjData.externalRef = this.externalRef;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.isCached = this.isCached;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}
public class CheckStatusInstantPay
{
    public string transactionDate { get; set; }
    public string externalRef { get; set; }

    public string GetJson()
    {
        CheckStatusInstantPay ObjData = new CheckStatusInstantPay();
        ObjData.transactionDate = this.transactionDate;
        ObjData.externalRef = this.externalRef;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class ApiPostCallInstantPay
{
    public static string PostCall(string Json, string url, string UerAgent)
    {
        var baseAddress = InstantPayout.Url + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static async Task<string> PostCallASYNC(string Json, string url, string OutLetID)
    {
        var baseAddress = InstantPayout.DMTURL + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutLetID);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }


    public static async Task<string> PostCallCheckStatusASYNC(string Json)
    {
        var baseAddress = InstantPayout.CheckStatus;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static async Task<string> PostCallDMTASYNC(string Json, string url, string OutLetID)
    {
        var baseAddress = InstantPayout.DMTURL + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutLetID);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static async Task<string> GetCallDMTASYNC(string Data, string url, string OutLetID)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var baseAddress = InstantPayout.DMTURL + url;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(baseAddress, Method.Get);
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutLetID);
        RestResponse response = await client.ExecuteAsync(request);

        return response.Content;
    }

    public static async Task<string> GetCallDMTASYNCDelete(string Data, string url, string OutLetID)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var baseAddress = InstantPayout.DMTURL + url;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(baseAddress, Method.Delete);
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutLetID);
        RestResponse response = await client.ExecuteAsync(request);

        return response.Content;
    }

    public static async Task<string> PostCallWithFullURL(string Json, string url, string UerAgent)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static string PostCallWithFullURL(string Json, string url, string UerAgent, string OutletID)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutletID);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static async Task<string> PostCallMSCWBEAsync(string Json, string url, string OutletID)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutletID);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static string AccountVerifyPostCall(string Json, string url, string UerAgent)
    {
        var baseAddress = InstantPayout.AccountVerifyUrl + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        //client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36";

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static string CheckStatus(string Json, string UerAgent)
    {
        var baseAddress = InstantPayout.CheckStatus;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(baseAddress, Method.Post);
        //client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36";

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static string GetBankList(string OutLet)
    {

        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var client = new RestClient("https://api.instantpay.in/fi/aeps/banks");
        //client.Timeout = -1;
        var request = new RestRequest("https://api.instantpay.in/fi/aeps/banks", Method.Get);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutLet);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static async Task<string> GetBankListAsync(string OutLet)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("https://api.instantpay.in/fi/aeps/banks")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("https://api.instantpay.in/fi/aeps/banks", Method.Get);
        request.AddHeader("X-Ipay-Auth-Code", "1");
        request.AddHeader("accept", "application/json");
        request.AddHeader("X-Ipay-Auth-Code", InstantPayout.X_Ipay_Auth_Code);
        request.AddHeader("X-Ipay-Client-Id", InstantPayout.X_Ipay_Client_Id);
        request.AddHeader("X-Ipay-Client-Secret", InstantPayout.X_Ipay_Client_Secret);
        request.AddHeader("X-Ipay-Endpoint-Ip", InstantPayout.X_Ipay_Endpoint_Ip);
        request.AddHeader("X-Ipay-Outlet-Id", OutLet);
        RestResponse response = await client.GetAsync(request);
        return response.Content;
    }
}


public class SignupInitiate
{
    public string mobile { get; set; }
    public string pan { get; set; }
    public string email { get; set; }
    public string aadhaar { get; set; }
    public string bankAccountNo { get; set; }
    public string bankIfsc { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string consent { get; set; } = "Y";

    public string GetJson()
    {
        SignupInitiate ObjData = new SignupInitiate();
        ObjData.mobile = this.mobile;
        ObjData.pan = this.pan;
        ObjData.email = this.email;
        ObjData.aadhaar = this.aadhaar;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.bankAccountNo = this.bankAccountNo;
        ObjData.bankIfsc = this.bankIfsc;
        ObjData.consent = this.consent;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class SignupOTPVerify
{
    public string otpReferenceID { get; set; }
    public string otp { get; set; }
    public string hash { get; set; }

    public string GetJson()
    {
        SignupOTPVerify signupOTPVerify = new SignupOTPVerify();
        signupOTPVerify.otpReferenceID = this.otpReferenceID;
        signupOTPVerify.otp = this.otp;
        signupOTPVerify.hash = this.hash;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}


public class RemitterRegistration
{
    public string mobile { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string pinCode { get; set; }

    public string GetJson()
    {
        RemitterRegistration ObjData = new RemitterRegistration();
        ObjData.mobile = this.mobile;
        ObjData.firstName = this.firstName;
        ObjData.lastName = this.lastName;
        ObjData.pinCode = this.pinCode;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }


}


public class BeneficiaryRegistration
{
    public string remitterMobile { get; set; }
    public string firstName { get; set; }
    public string ifsc { get; set; }
    public string accountNumber { get; set; }
    public string bankId { get; set; }

    public string GetJson()
    {
        BeneficiaryRegistration ObjData = new BeneficiaryRegistration();
        ObjData.remitterMobile = this.remitterMobile;
        ObjData.firstName = this.firstName;
        ObjData.ifsc = this.ifsc;
        ObjData.accountNumber = this.accountNumber;
        ObjData.bankId = this.ifsc;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class BeneficiaryDelete
{
    public string remitterMobile { get; set; }
    public string beneficiaryId { get; set; }
    public string GetJson()
    {
        BeneficiaryDelete ObjData = new BeneficiaryDelete();
        ObjData.remitterMobile = this.remitterMobile;
        ObjData.beneficiaryId = this.beneficiaryId;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class OtpVerification
{
    public string otpReference { get; set; }
    public string otp { get; set; }
    public string GetJson()
    {
        OtpVerification ObjData = new OtpVerification();
        ObjData.otpReference = this.otpReference;
        ObjData.otp = this.otp;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class RemitterUpdate
{
    public string mobile { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string pinCode { get; set; }

    public string GetJson()
    {
        RemitterUpdate ObjData = new RemitterUpdate();
        ObjData.mobile = this.mobile;
        ObjData.firstName = this.firstName;
        ObjData.lastName = this.lastName;
        ObjData.pinCode = this.pinCode;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class FundTransfer
{
    public string remitterMobile { get; set; }
    public string beneficiaryId { get; set; }
    public string transferMode { get; set; }
    public int transferAmount { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string externalRef { get; set; }

    public string GetJson()
    {
        FundTransfer ObjData = new FundTransfer();
        ObjData.remitterMobile = this.remitterMobile;
        ObjData.beneficiaryId = this.beneficiaryId;
        ObjData.transferMode = this.transferMode;
        ObjData.transferAmount = this.transferAmount;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.externalRef = this.externalRef;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class InstantPayBEBody
{
    public string encryptedAadhaar { get; set; }
    public string dc { get; set; }
    public string ci { get; set; }
    public string hmac { get; set; }
    public string dpId { get; set; }
    public string mc { get; set; }
    public string pidDataType { get; set; }
    public string sessionKey { get; set; }
    public string mi { get; set; }
    public string rdsId { get; set; }
    public string errCode { get; set; }
    public string errInfo { get; set; }
    public string fCount { get; set; }
    public string fType { get; set; }
    public int iCount { get; set; }
    public string iType { get; set; }
    public int pCount { get; set; }
    public string pType { get; set; }
    public string srno { get; set; }
    public string sysid { get; set; }
    public string ts { get; set; }
    public string pidData { get; set; }
    public string qScore { get; set; }
    public string nmPoints { get; set; }
    public string rdsVer { get; set; }
}

public class InstantPayCWRoot
{
    public string bankiin { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string mobile { get; set; }
    public string amount { get; set; }
    public string externalRef { get; set; }
    public InstantPayBEBody biometricData { get; set; }

    public string GetJson()
    {
        InstantPayCWRoot signupOTPVerify = new InstantPayCWRoot();
        signupOTPVerify.bankiin = this.bankiin;
        signupOTPVerify.latitude = this.latitude;
        signupOTPVerify.longitude = this.longitude;
        signupOTPVerify.mobile = this.mobile;
        signupOTPVerify.amount = this.amount;
        signupOTPVerify.externalRef = this.externalRef;
        signupOTPVerify.biometricData = this.biometricData;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}

public class InstantPayBERoot
{
    public string bankiin { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string mobile { get; set; }
    public string externalRef { get; set; }
    public InstantPayBEBody biometricData { get; set; }

    public string GetJson()
    {
        InstantPayBERoot signupOTPVerify = new InstantPayBERoot();
        signupOTPVerify.bankiin = this.bankiin;
        signupOTPVerify.latitude = this.latitude;
        signupOTPVerify.longitude = this.longitude;
        signupOTPVerify.mobile = this.mobile;
        signupOTPVerify.externalRef = this.externalRef;
        signupOTPVerify.biometricData = this.biometricData;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}



public class BalanceCheckInstant
{
    public string bankProfileId { get; set; }
    public string accountNumber { get; set; }
    public string externalRef { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }

    public string GetJson()
    {
        BalanceCheckInstant signupOTPVerify = new BalanceCheckInstant();
        signupOTPVerify.bankProfileId = InstantPayout.bankProfileId;
        signupOTPVerify.latitude = "20.126";
        signupOTPVerify.longitude = "78.3228";
        signupOTPVerify.externalRef = System.Guid.NewGuid().ToString();
        signupOTPVerify.accountNumber = InstantPayout.accountNumber;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}



public class BalanceCheckInstantWithAccount
{
    public string bankProfileId { get; set; }
    public string accountNumber { get; set; }
    public string externalRef { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }

    public string GetJson()
    {
        BalanceCheckInstantWithAccount signupOTPVerify = new BalanceCheckInstantWithAccount();
        signupOTPVerify.bankProfileId = InstantPayoutWithAccount.bankProfileId;
        signupOTPVerify.latitude = "20.126";
        signupOTPVerify.longitude = "78.3228";
        signupOTPVerify.externalRef = System.Guid.NewGuid().ToString();
        signupOTPVerify.accountNumber = InstantPayoutWithAccount.accountNumber;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}


//mantra

public class Xml
{
    [JsonProperty("@version")]
    public string Version { get; set; }
}

public class Resp
{
    [JsonProperty("@errCode")]
    public string ErrCode { get; set; }

    [JsonProperty("@errInfo")]
    public string ErrInfo { get; set; }

    [JsonProperty("@fCount")]
    public string FCount { get; set; }

    [JsonProperty("@fType")]
    public string FType { get; set; }

    [JsonProperty("@nmPoints")]
    public string NmPoints { get; set; }

    [JsonProperty("@qScore")]
    public string QScore { get; set; }
}

public class Param
{
    [JsonProperty("@name")]
    public string Name { get; set; }

    [JsonProperty("@value")]
    public object Value { get; set; }
}

public class AdditionalInfo
{
    public List<Param> Param { get; set; }
}

public class DeviceInfo
{
    [JsonProperty("@dpId")]
    public string DpId { get; set; }

    [JsonProperty("@rdsId")]
    public string RdsId { get; set; }

    [JsonProperty("@rdsVer")]
    public string RdsVer { get; set; }

    [JsonProperty("@mi")]
    public string Mi { get; set; }

    [JsonProperty("@mc")]
    public string Mc { get; set; }

    [JsonProperty("@dc")]
    public string Dc { get; set; }
    public AdditionalInfo additional_info { get; set; }
}

public class Skey
{
    [JsonProperty("@ci")]
    public string Ci { get; set; }

    [JsonProperty("#text")]
    public string Text { get; set; }
}

public class DataMantra
{
    [JsonProperty("@type")]
    public string Type { get; set; }

    [JsonProperty("#text")]
    public string Text { get; set; }
}

public class PidData
{
    public Resp Resp { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
    public Skey Skey { get; set; }
    public string Hmac { get; set; }
    public DataMantra Data { get; set; }
}

public class Mantra
{
    [JsonProperty("?xml")]
    public Xml Xml { get; set; }
    public PidData PidData { get; set; }
}




public class XmlMorpho
{
    [JsonProperty("@version")]
    public string Version { get; set; }
}

public class RespMorpho
{
    [JsonProperty("@errCode")]
    public string ErrCode { get; set; }

    [JsonProperty("@errInfo")]
    public string ErrInfo { get; set; }

    [JsonProperty("@fCount")]
    public string FCount { get; set; }

    [JsonProperty("@fType")]
    public string FType { get; set; }

    [JsonProperty("@nmPoints")]
    public string NmPoints { get; set; }

    [JsonProperty("@qScore")]
    public string QScore { get; set; }
}

public class ParamMorpho
{
    [JsonProperty("@name")]
    public string Name { get; set; }

    [JsonProperty("@value")]
    public object Value { get; set; }
}

public class AdditionalInfoMorpho
{
    public Param Param { get; set; }
}
public class DeviceInfoMorpho
{
    [JsonProperty("@dpId")]
    public string DpId { get; set; }

    [JsonProperty("@rdsId")]
    public string RdsId { get; set; }

    [JsonProperty("@rdsVer")]
    public string RdsVer { get; set; }

    [JsonProperty("@mi")]
    public string Mi { get; set; }

    [JsonProperty("@mc")]
    public string Mc { get; set; }

    [JsonProperty("@dc")]
    public string Dc { get; set; }
    public AdditionalInfoMorpho additional_info { get; set; }
}

public class SkeyMorpho
{
    [JsonProperty("@ci")]
    public string Ci { get; set; }

    [JsonProperty("#text")]
    public string Text { get; set; }
}

public class DataMorpho
{
    [JsonProperty("@type")]
    public string Type { get; set; }

    [JsonProperty("#text")]
    public string Text { get; set; }
}

public class PidDataMorpho
{
    public RespMorpho Resp { get; set; }
    public DeviceInfoMorpho DeviceInfo { get; set; }
    public SkeyMorpho Skey { get; set; }
    public string Hmac { get; set; }
    public DataMorpho Data { get; set; }
}

public class RooteeeMorpho
{
    [JsonProperty("?xml")]
    public XmlMorpho Xml { get; set; }
    public PidDataMorpho PidData { get; set; }
}




public class RootCCPay
{
    public PayerCCPay payer { get; set; }
    public Payee payee { get; set; }
    public string transferMode { get; set; }
    public string transferAmount { get; set; }
    public string externalRef { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string remarks { get; set; }
    public string alertEmail { get; set; }

    public RootCCPay()
    {
        alertEmail = "";
        remarks = "Pay";
    }


    public string GetJson()
    {
        RootCCPay signupOTPVerify = new RootCCPay();
        signupOTPVerify.payer = this.payer;
        signupOTPVerify.payee = this.payee;
        signupOTPVerify.transferMode = this.transferMode;
        signupOTPVerify.transferAmount = this.transferAmount;
        signupOTPVerify.externalRef = this.externalRef;
        signupOTPVerify.latitude = this.latitude;
        signupOTPVerify.longitude = this.longitude;
        signupOTPVerify.remarks = this.remarks;
        signupOTPVerify.alertEmail = this.alertEmail;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(signupOTPVerify);
        return jsonString;
    }
}




public class PayerCCPay
{
    public string bankId { get; set; }
    public string bankProfileId { get; set; }
    public string accountNumber { get; set; }
    public string name { get; set; }
    public string paymentMode { get; set; }
    public string cardNumber { get; set; }
    public string cardSecurityCode { get; set; }
    public CardExpiry cardExpiry { get; set; }
    public string referenceNumber { get; set; }

    public PayerCCPay()
    {
        cardNumber = "";
        bankId = "0";
        bankProfileId = "0";
        accountNumber = "8005575599";
        name = "Instantpay";
        paymentMode = "NETBANKING";
        cardSecurityCode = "";
        CardExpiry cardExpiry = new CardExpiry();
        this.cardExpiry = cardExpiry;
    }
}


public class CardExpiry
{
    public string month { get; set; }
    public string year { get; set; }
    public CardExpiry()
    {
        month = "";
        year = "";
    }
}