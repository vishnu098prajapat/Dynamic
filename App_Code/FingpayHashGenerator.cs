using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for FingpayHashGenerator
/// </summary>
public class FingpayHashGenerator
{
    public static readonly int SymmetricKeySize = 128;
    public static readonly string SupermerchantID = "";
    public static readonly string Supermerchantloginid = "";
    public static readonly string supermerchantpassword = "";
    public static readonly string Password = "";
    public static readonly string merchantLoginPin = "";
    public static readonly string pss = "";
    public static readonly string Pin = "";
    public static readonly string SecretKey = "";
    public static readonly string OBURL = "https://fingpayap.tapits.in/fpaepsweb/api/onboarding/merchant/simple/creation/v2";//"https://fingpayap.tapits.in/fpaepsweb/api/onboarding/merchant/creation/without/encryption";
    public static readonly string SendOTP = "https://fpekyc.tapits.in/fpekyc/api/ekyc/merchant/v1/sendotp";
    public static readonly string VerifyOTP = "https://fpekyc.tapits.in/fpekyc/api/ekyc/merchant/v1/validateotp";
    public static readonly string Bekyc = "https://fpekyc.tapits.in/fpekyc/api/ekyc/merchant/v1/biometric";
    public static readonly string AUth = "https://fingpayap.tapits.in/fpaepsservice/auth/tfauth/merchant/simple/validate/aadhar";
    public static readonly string BE = "https://fingpayap.tapits.in/fpaepsservice/api/balanceInquiry/merchant/v2/getBalance";
    public static readonly string CW = "https://fingpayap.tapits.in/fpaepsservice/api/cashWithdrawal/merchant/v2/withdrawal";
    public static readonly string AW = "https://fingpayap.tapits.in/fpaepsservice/api/aadhaarPay/merchant/v2/pay";
    public static readonly string MS = "https://fingpayap.tapits.in/fpaepsservice/api/miniStatement/merchant/v2/statement";
    public static readonly string CD = "https://fingpayap.tapits.in/fpaepsservice/api/CashDeposit/merchant/simple/deposit";
    public static readonly string ThreeCorn = "https://fpanalytics.tapits.in/fpcollectservice/api/threeway/aggregators";

    public static string PostCall(string Request, string MethoName, string Hash, string trnTimestamp)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(MethoName)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(MethoName, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("trnTimestamp", trnTimestamp);
        request.AddHeader("hash", Hash);
        request.AddHeader("deviceIMEI", "352801082418919");
        request.AddParameter("application/json", Request, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static async Task<string> PostCallAsync(string Request, string MethoName, string Hash, string trnTimestamp)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(MethoName)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(MethoName, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("trnTimestamp", trnTimestamp);
        request.AddHeader("hash", Hash);
        request.AddHeader("deviceIMEI", "352801082418919");
        request.AddParameter("application/json", Request, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static async Task<string> PostCallAsyncCD(string Request, string MethoName, string Hash, string trnTimestamp)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(MethoName)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(MethoName, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("trnTimestamp", trnTimestamp);
        request.AddHeader("superMerchantId", FingpayHashGenerator.SupermerchantID);
        request.AddHeader("hash", Hash);
        request.AddHeader("deviceIMEI", "352801082418919");
        request.AddParameter("application/json", Request, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

    public static string PostCallThree(string Request, string MethoName, string Hash, string trnTimestamp)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(MethoName)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(MethoName, Method.Post);

        request.AddHeader("txnDate", trnTimestamp);
        request.AddHeader("hash", Hash);
        request.AddHeader("superMerchantLoginId", FingpayHashGenerator.Supermerchantloginid);
        request.AddHeader("superMerchantid", FingpayHashGenerator.SupermerchantID);
        request.AddHeader("Content-Type", "application/json");
        request.AddParameter("application/json", Request, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);


        return response.Content;
    }

    public static string GenerateSha256Hash(string inputr)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Compute the hash
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(inputr));

            // Convert hash bytes to Base64 string
            string base64Hash = Convert.ToBase64String(bytes);

            return base64Hash;
        }
    }


    public static string GenerateSessionKey()
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = SymmetricKeySize;
            aes.GenerateKey();
            return BitConverter.ToString(aes.Key).Replace("-", "").ToLower();
        }
    }


    public static string EncryptUsingSessionKey(byte[] skey, byte[] data)
    {

        //string dataToEncrypt = "Hello, World!";
        //byte[] data = Encoding.UTF8.GetBytes(dataToEncrypt);

        //// Generating a 128-bit AES key for testing
        //using (Aes aes = Aes.Create())
        //{
        //    aes.KeySize = 128;
        //    aes.GenerateKey();
        //    byte[] sessionKey = aes.Key;

        //    string encryptedData = EncryptUsingSessionKey(sessionKey, data);
        //    Console.WriteLine("Encrypted Data: " + encryptedData);
        //}
        using (Aes aes = Aes.Create())
        {
            aes.Key = skey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Generate a new IV for each encryption to ensure security
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Write the IV to the start of the stream
                memoryStream.Write(iv, 0, iv.Length);

                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                }

                byte[] encryptedData = memoryStream.ToArray();
                return Convert.ToBase64String(encryptedData);
            }
        }
    }

}




public class MerchantOnboardingFindpay
{
    public String username;
    public String password;
    public DateTime timestamp;
    public double latitude;
    public double longitude;
    public int supermerchantId;
    public List<MerchantModel> merchants;

    public MerchantOnboardingFindpay()
    {
        username = FingpayHashGenerator.Supermerchantloginid;
        password = FingpayHashGenerator.Password;
        timestamp = System.DateTime.Now;
        latitude = 15.54;
        longitude = 15.54;
        supermerchantId = FingpayHashGenerator.SupermerchantID.ToInt32();
    }


    public string GetJson()
    {
        MerchantOnboardingFindpay ObjData = new MerchantOnboardingFindpay();
        ObjData.username = this.username;
        ObjData.password = this.password;
        ObjData.timestamp = this.timestamp;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.supermerchantId = this.supermerchantId;
        ObjData.merchants = this.merchants;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class MerchantModel
{
    public String merchantLoginId;
    public String merchantLoginPin;
    public String merchantpassword;
    public String merchantName;
    public MerchantAddress merchantAddress;
    public String merchantPhoneNumber;
    public String companyLegalName;
    public String companyMarketingName;
    public Kyc kyc;
    public Settlement settlement;
    public String emailId;
    public String shopAndPanImage;
    public String cancellationCheckImages;
    public String ekycDocuments;
    public String merchantPinCode;
    public String tan;
    public String merchantCityName;
    public String merchantDistrictName;

    public MerchantModel()
    {
        merchantpassword = "585201";
    }
}

public class MerchantAddress
{
    public String merchantAddress;
    public String merchantState;

    public MerchantAddress()
    {
        merchantAddress = "S HOSALLI";
        merchantState = "12";
    }
}


public class Kyc
{
    public String userPan;
    public String aadhaarNumber;
    public String gstinNumber;
    public String companyOrShopPan;
    public bool shopAndPanImage;

    

    public Kyc()
    {
        gstinNumber = "09ABECS5457P1ZI";
        companyOrShopPan = "ABECS5457P";
        shopAndPanImage = true;
    }
}


public class Settlement
{
    public String companyBankAccountNumber;
    public String bankIfscCode;
    public String companyBankName;
    public String bankBranchName;
    public String bankAccountName;
}




// Send OTP
public class FinoSendOTP
{
    public string superMerchantId { get; set; }
    public string merchantLoginId { get; set; }
    public string transactionType { get; set; }
    public string mobileNumber { get; set; }
    public string aadharNumber { get; set; }
    public string panNumber { get; set; }
    public string matmSerialNumber { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }

    public FinoSendOTP()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
        matmSerialNumber = "";
        transactionType = "EKY";
        latitude = 16.7698065000;
        longitude = 77.1337764000;
    }

    public string GetJson()
    {
        FinoSendOTP ObjData = new FinoSendOTP();
        ObjData.superMerchantId = this.superMerchantId;
        ObjData.merchantLoginId = this.merchantLoginId;
        ObjData.transactionType = this.transactionType;
        ObjData.mobileNumber = this.mobileNumber;
        ObjData.aadharNumber = this.aadharNumber;
        ObjData.panNumber = this.panNumber;
        ObjData.matmSerialNumber = this.matmSerialNumber;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class FinoVerifyOTP
{
    public string superMerchantId { get; set; }
    public string merchantLoginId { get; set; }
    public string otp { get; set; }
    public string primaryKeyId { get; set; }
    public string encodeFPTxnId { get; set; }

    public FinoVerifyOTP()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
    }

    public string GetJson()
    {
        FinoVerifyOTP ObjData = new FinoVerifyOTP();
        ObjData.superMerchantId = this.superMerchantId;
        ObjData.merchantLoginId = this.merchantLoginId;
        ObjData.otp = this.otp;
        ObjData.primaryKeyId = this.primaryKeyId;
        ObjData.encodeFPTxnId = this.encodeFPTxnId;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




//Capture Thumb

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class CaptureResponse
{
    public string errCode { get; set; }
    public string errInfo { get; set; }
    public string fCount { get; set; }
    public string fType { get; set; }
    public string iCount { get; set; }
    public object iType { get; set; }
    public string pCount { get; set; }
    public string pType { get; set; }
    public string nmPoints { get; set; }
    public string qScore { get; set; }
    public string dpID { get; set; }
    public string rdsID { get; set; }
    public string rdsVer { get; set; }
    public string dc { get; set; }
    public string mi { get; set; }
    public string mc { get; set; }
    public string ci { get; set; }
    public string sessionKey { get; set; }
    public string hmac { get; set; }
    public string PidDatatype { get; set; }
    public string Piddata { get; set; }
}

public class CardnumberORUID
{
    public string nationalBankIdentificationNumber { get; set; }
    public string indicatorforUID { get; set; }
    public string adhaarNumber { get; set; }
}



public class P2CardnumberORUID
{
    public string nationalBankIdentificationNumber { get; set; }
    public string indicatorforUID { get; set; }
    public string adhaarNumber { get; set; }
}



public class FinoCapture
{
    public string superMerchantId { get; set; }
    public string merchantLoginId { get; set; }
    public int primaryKeyId { get; set; }
    public string encodeFPTxnId { get; set; }
    public string requestRemarks { get; set; }
    public CardnumberORUID cardnumberORUID { get; set; }
    public CaptureResponse captureResponse { get; set; }

    public FinoCapture()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
    }

    public string GetJson()
    {
        FinoCapture ObjData = new FinoCapture();
        ObjData.superMerchantId = this.superMerchantId;
        ObjData.merchantLoginId = this.merchantLoginId;
        ObjData.primaryKeyId = this.primaryKeyId;
        ObjData.encodeFPTxnId = this.encodeFPTxnId;
        ObjData.requestRemarks = this.requestRemarks;
        ObjData.cardnumberORUID = this.cardnumberORUID;
        ObjData.captureResponse = this.captureResponse;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}





//Authencation
public class FinoCaptureAuth
{

    public bool isFacialTan { get; set; }
    public double latitude { get; set; }

    
    public string requestRemarks { get; set; }
    public double longitude { get; set; }
    public string transactionType { get; set; }
    public string merchantUserName { get; set; }
    public string merchantPin { get; set; }
    public string superMerchantId { get; set; }
    public string merchantTranId { get; set; }
    public string mobileNumber { get; set; }
    //   public string tefPkId { get; set; }
    //   public string fingpayTransactionId { get; set; }
    public string serviceType { get; set; }
    public P2CardnumberORUID cardnumberORUID { get; set; }
    public CaptureResponse captureResponse { get; set; }

    public FinoCaptureAuth()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
        serviceType = "AEPS";
        transactionType = "AUO";
        requestRemarks = "TN3000CA0006530";
        isFacialTan = false;
        // tefPkId = "TN3000CA0006530";
        //  fingpayTransactionId = "TN3000CA0006530";
    }

    public string GetJson()
    {
        FinoCaptureAuth ObjData = new FinoCaptureAuth();
        ObjData.superMerchantId = this.superMerchantId;
        ObjData.latitude = this.latitude;
        //ObjData.tefPkId = this.tefPkId;
        //  ObjData.fingpayTransactionId = this.fingpayTransactionId;
        ObjData.longitude = this.longitude;
        ObjData.transactionType = this.transactionType;
        ObjData.merchantUserName = this.merchantUserName;
        ObjData.merchantPin = this.merchantPin;
        ObjData.merchantTranId = this.merchantTranId;
        ObjData.mobileNumber = this.mobileNumber;
        ObjData.serviceType = this.serviceType;
        ObjData.requestRemarks = this.requestRemarks;
        ObjData.cardnumberORUID = this.cardnumberORUID;
        ObjData.captureResponse = this.captureResponse;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



//BE Request


//Authencation
public class FinoCaptureBE
{
    public bool isFacialTan { get; set; }
    public string languageCode { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string mobileNumber { get; set; }
    public string paymentType { get; set; }
    public string timestamp { get; set; }
    public string requestRemarks { get; set; }
    public string transactionType { get; set; }
    public string merchantUserName { get; set; }
    public string merchantPin { get; set; }
    public string superMerchantId { get; set; }
    public string merchantTransactionId { get; set; }

    public string serviceType { get; set; }
    public P2CardnumberORUID cardnumberORUID { get; set; }
    public CaptureResponse captureResponse { get; set; }

    public FinoCaptureBE()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
        languageCode = "en";
        paymentType = "B";
        serviceType = "AEPS";
        timestamp = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
        transactionType = "BE";
        requestRemarks = "TN3000CA0006530";
    }

    public string GetJson()
    {
        FinoCaptureBE ObjData = new FinoCaptureBE();
        ObjData.languageCode = this.languageCode;
        ObjData.paymentType = this.paymentType;
        ObjData.timestamp = this.timestamp;
        ObjData.superMerchantId = this.superMerchantId;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.transactionType = this.transactionType;
        ObjData.merchantUserName = this.merchantUserName;
        ObjData.merchantPin = this.merchantPin;
        ObjData.merchantTransactionId = this.merchantTransactionId;
        ObjData.mobileNumber = this.mobileNumber;
        ObjData.serviceType = this.serviceType;
        ObjData.requestRemarks = this.requestRemarks;
        ObjData.cardnumberORUID = this.cardnumberORUID;
        ObjData.captureResponse = this.captureResponse;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




public class FinoCaptureCW
{
    public bool isFacialTan { get; set; }
    public string languageCode { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string mobileNumber { get; set; }
    public string paymentType { get; set; }
    public string timestamp { get; set; }
    public string requestRemarks { get; set; }
    public string transactionType { get; set; }
    public string merchantUserName { get; set; }
    public string merchantPin { get; set; }
    public string superMerchantId { get; set; }
    public decimal transactionAmount { get; set; }
    public string merchantTranId { get; set; }
    public string deviceTransactionId { get; set; }

    public string serviceType { get; set; }
    public P2CardnumberORUID cardnumberORUID { get; set; }
    public CaptureResponse captureResponse { get; set; }

    public FinoCaptureCW()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
        languageCode = "en";
        paymentType = "B";
        serviceType = "AEPS";
        timestamp = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
        transactionType = "CW";
        requestRemarks = "TN3000CA0006530";
        isFacialTan = false;
    }

    public string GetJson()
    {
        FinoCaptureCW ObjData = new FinoCaptureCW();

        ObjData.isFacialTan = this.isFacialTan;
        ObjData.languageCode = this.languageCode;
        ObjData.paymentType = this.paymentType;
        ObjData.timestamp = this.timestamp;
        ObjData.superMerchantId = this.superMerchantId;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.transactionType = this.transactionType;
        ObjData.merchantUserName = this.merchantUserName;
        ObjData.merchantPin = this.merchantPin;
        ObjData.merchantTranId = this.merchantTranId;
        ObjData.deviceTransactionId = this.deviceTransactionId;

        ObjData.mobileNumber = this.mobileNumber;
        ObjData.serviceType = this.serviceType;
        ObjData.requestRemarks = this.requestRemarks;
        ObjData.cardnumberORUID = this.cardnumberORUID;
        ObjData.captureResponse = this.captureResponse;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class FinoCaptureCD
{
    public bool isFacialTan { get; set; }
    public string languageCode { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string mobileNumber { get; set; }
    public string paymentType { get; set; }
    public string timestamp { get; set; }
    public string requestRemarks { get; set; }
    public string transactionType { get; set; }
    public string merchantUserName { get; set; }
    public string merchantPin { get; set; }
    public string superMerchantId { get; set; }
    public decimal transactionAmount { get; set; }
    public string merchantTranId { get; set; }
    public string deviceTransactionId { get; set; }

    public P2CardnumberORUID cardnumberORUID { get; set; }
    public CaptureResponse captureResponse { get; set; }

    public FinoCaptureCD()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
        languageCode = "en";
        paymentType = "B";
        timestamp = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
        transactionType = "CD";
        requestRemarks = "TN3000CA0006530";
        isFacialTan = false;
    }

    public string GetJson()
    {
        FinoCaptureCD ObjData = new FinoCaptureCD();

        ObjData.isFacialTan = this.isFacialTan;
        ObjData.languageCode = this.languageCode;
        ObjData.paymentType = this.paymentType;
        ObjData.timestamp = this.timestamp;
        ObjData.superMerchantId = this.superMerchantId;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.transactionType = this.transactionType;
        ObjData.merchantUserName = this.merchantUserName;
        ObjData.merchantPin = this.merchantPin;
        ObjData.merchantTranId = this.merchantTranId;
        ObjData.deviceTransactionId = this.deviceTransactionId;

        ObjData.mobileNumber = this.mobileNumber;
        ObjData.requestRemarks = this.requestRemarks;
        ObjData.cardnumberORUID = this.cardnumberORUID;
        ObjData.captureResponse = this.captureResponse;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

//MS



public class FinoCaptureMS
{
    public bool isFacialTan { get; set; }

    public string languageCode { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string mobileNumber { get; set; }
    public string paymentType { get; set; }
    public string timestamp { get; set; }
    public string requestRemarks { get; set; }
    public string transactionType { get; set; }
    public string merchantUserName { get; set; }
    public string merchantPin { get; set; }
    public string superMerchantId { get; set; }
    public string merchantTranId { get; set; }

    public string serviceType { get; set; }
    public P2CardnumberORUID cardnumberORUID { get; set; }
    public CaptureResponse captureResponse { get; set; }

    public FinoCaptureMS()
    {
        superMerchantId = FingpayHashGenerator.SupermerchantID;
        languageCode = "en";
        paymentType = "B";
        serviceType = "AEPS";
        timestamp = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
        transactionType = "MS";
        requestRemarks = "TN3000CA0006530";
        isFacialTan = false;
    }

}






public class MerchantOnboardingFindpayNew
{
    public String username;
    public String password;
    public DateTime timestamp;
    public double latitude;
    public double longitude;
    public int supermerchantId;
    public string ipAddress;
    public MerchantModelV1 merchant;

    public MerchantOnboardingFindpayNew()
    {
        username = FingpayHashGenerator.Supermerchantloginid;
        password = FingpayHashGenerator.Password; 
        timestamp = System.DateTime.Now;
        latitude = 15.54;
        longitude = 15.54;
        supermerchantId = FingpayHashGenerator.SupermerchantID.ToInt32();
    }


    public string GetJson()
    {
        MerchantOnboardingFindpayNew ObjData = new MerchantOnboardingFindpayNew();
        ObjData.username = this.username;
        ObjData.password = this.password;
        ObjData.timestamp = this.timestamp;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.supermerchantId = this.supermerchantId;
        ObjData.merchant = this.merchant;
        ObjData.ipAddress = this.ipAddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




public class MerchantModelV1
{
    public String merchantLoginId;
    public String merchantLoginPin;
    public String firstName;
    public String lastName;
    public String middleName;
    public String merchantPhoneNumber;
    public MerchantAddressV1 merchantAddress;
    public String companyLegalName;
    public String userType;
    public Int32 companyType;
    public String emailId;
    public bool certificateOfIncorporationImage;
    public Kyc kyc;
    public SettlementV1 settlementV1;
    public bool tradeBusinessProof;
    public bool termsConditionCheck;
    public bool cancelledChequeImages;
    public bool physicalVerification;
    public bool videoKycWithLatLongData;
    public MerchantKycAddressDataModel merchantKycAddressData;



    //public String merchantpassword;
    //public String companyMarketingName;
    //public String shopAndPanImage;
    //public String cancellationCheckImages;
    //public String ekycDocuments;
    //public String merchantPinCode;
    //public String tan;

    public MerchantModelV1()
    {
       
        tradeBusinessProof = false;
        termsConditionCheck = false;
        cancelledChequeImages = false;
        physicalVerification = true;
        videoKycWithLatLongData = false;
        //merchantLoginPin = "1936";

    }
}



public class MerchantAddressV1
{
    public String merchantAddress1;
    public String merchantAddress2;
    public Int32 merchantState;
    public String merchantCityName;
    public String merchantDistrictName;
    public String merchantPinCode;
}




public class SettlementV1
{
    public String companyBankAccountNumber;
    public String bankIfscCode;
    public String companyBankName;
    public String bankBranchName;
    public String bankAccountName;
}



public class MerchantKycAddressDataModel
{
    public String shopAddress;
    public String shopCity;
    public String shopDistrict;
    public Int32 shopState;
    public String shopPincode;
    public Double shopLatitude;
    public Double shopLongitude;
}