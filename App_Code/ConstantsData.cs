using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Weborb.Message;

/// <summary>
/// Summary description for ConstantsData
/// </summary>
public class ConstantsData
{
    public static int OfflineAPIID { get; set; } = 1;
    //public static bool IsSession { get; set; }
    public static readonly string CompanyName = "JANTASERVICE";
    public static readonly string PackageMessage = "Package Update Successfully";
    public static readonly string SMSCategory = "SMSCategory Update Successfully";
    public static readonly string SMSCategoryActive = "SMSCategory Active/DeActive Successfully";
    public static readonly string RedirectUrlM = "Default.aspx";
    public static readonly string SOffline = "35";


    //TDS Part
    public static bool _TDS { get; set; } = false;
    public static decimal _TDSVal { get; set; } = 2;


    //ICON Path
    public static readonly string ActiveIcon = "../images/Active.png";
    public static readonly string DeActiveIcon = "../images/Delete.png";
    public static readonly string DeleteIcon = "../images/DeleteIcon.png";
    public static readonly string ForceFailedIcon = "../images/forcefailed.png";
    public static readonly string EditIcon = "../images/Edit.png";
    public static readonly string EyeOpen = "../images/eyeopen.png";
    public static readonly string EyeClose = "../images/eyeclose.png";
    public static readonly string Child = "../images/child.png";
    public static readonly string NoImage = "NoImage.png";
    public static readonly string SMS = "../images/sms.png";
    public static readonly string kycA = "../images/vkyca.png";
    public static readonly string kycP = "../images/vkycp.png";
    public static readonly string gmail = "../images/gmail.png";
    public static readonly string lockM = "../images/lock.png";
    public static readonly string unlocked = "../images/unlocked.png";
    public static readonly string checkstatus = "../images/checkstatus.png";
    public static readonly string fs = "../images/forcesuccess.png";
    public static readonly string News = "../images/news.png";
    public static readonly string plan = "../images/plan.png";
    public static readonly string sendmoney = "../images/plan.png";
    public static readonly string addfund = "../images/icon/addfund.png";


    //Page ID
    public static readonly Int32 Role = 1;
    public static readonly Int32 Package = 2;
    public static readonly Int32 AssignRole = 3;
    public static readonly Int32 Service = 4;
    public static readonly Int32 AepsCommission = 5;
    public static readonly Int32 MATMCommission = 6;
    public static readonly Int32 AadharSurcharge = 7;
    public static readonly Int32 DMTSurcharge = 8;
    public static readonly Int32 PayoutSurcharge = 9;
    public static readonly Int32 UPISurcharge = 10;
    public static readonly Int32 UPITransferSurcharge = 11;
    public static readonly Int32 VirtualSurcharge = 12;
    public static readonly Int32 RechargeCommission = 13;
    public static readonly Int32 CashDepositCommission = 14;
    public static readonly Int32 MiniStatement = 15;
    public static readonly Int32 DMTHistory = 16;
    public static readonly Int32 AEPSHistory = 17;
    public static readonly Int32 Payout = 18;
    public static readonly Int32 SAadharPay = 18;
    public static readonly Int32 UPI = 19;
    public static readonly Int32 UPITransfer = 20;
    public static readonly Int32 Recharge = 21;
    public static readonly Int32 MATM = 22;
    public static readonly Int32 Pan = 23;
    public static readonly Int32 AadharPay = 24;
    public static readonly Int32 FundRequestPage = 25;
    public static readonly Int32 AddFund = 26;
    public static readonly Int32 DeductFund = 27;
    public static readonly Int32 AddAEPSFund = 28;
    public static readonly Int32 DeductAEPSFund = 29;
    public static readonly Int32 MemberReport = 30;

    //Amount Limit
    public static readonly Int32 AepsSetLimitLow = 100;
    public static readonly Int32 AepsSetLimitHigh = 10000;
    public static readonly Int32 DMTSetLimitLow = 9;
    public static readonly Int32 DMTSetLimitHigh = 500000;
    public static readonly Int32 MATMSetLimit = 10000;
    public static readonly Int32 AadharSetLimit = 10000;


    //Payment Array
    public static string[] PaymentType = new string[] { "IMPS", "NEFT" };

    //ServiceCategory 
    public static readonly Int32 RechargeServiceCategory = 1;
    public static readonly Int32 BBPSServiceCategory = 2;
    public static readonly Int32 BankingServiceCategory = 3;
    public static readonly Int32 TravelServiceCategory = 4;


    //Payment Mode
    public static string[] PaymentMode = new string[] { "Select Payment Mode", "NEFT/IMPS/RTGS", "Cheque/DD", "Cash", "UPI" };

    //SMS ID
    public static readonly Int32 Admin_Login_OTP_SMS = 1;
    public static readonly Int32 Admin_Fund_Add_OTP_SMS = 2;
    public static readonly Int32 Admin_Deduct_Fund_OTP_SMS = 3;
    public static readonly Int32 Registration_SMS = 4;
    public static readonly Int32 Login_OTP_SMS = 5;
    public static readonly Int32 Transation_OTP_SMS = 6;
    public static readonly Int32 Forgot_Password_SMS = 7;
    public static readonly Int32 Bank_Add_OTP_SMS = 8;
    public static readonly Int32 Sign_Up_OTP_SMS = 9;
    public static readonly Int32 FundRequest = 10;
    public static readonly Int32 FundApprove = 11;
    public static readonly Int32 KYCReject = 12;
    public static readonly Int32 KYCApprove = 13;
    public static readonly Int32 LoginOtherDevice = 14;


    //ServiceID
    public static readonly Int32 SRecharge = 1;
    public static readonly Int32 SPostPaid = 2;
    public static readonly Int32 SDTH = 3;
    public static readonly Int32 SElectricity = 4;
    public static readonly Int32 SWater = 5;
    public static readonly Int32 SGas = 6;
    public static readonly Int32 SGasBooking = 7;
    public static readonly Int32 SInsurance = 8;
    public static readonly Int32 SInternet = 9;
    public static readonly Int32 SLandline = 10;
    public static readonly Int32 SEmi = 11;
    public static readonly Int32 SFastTag = 12;
    public static readonly Int32 SEducation = 13;
    public static readonly Int32 SCableTv = 14;
    public static readonly Int32 SMunicipalTax = 15;

    // DMT
    public static readonly Int32 SMoneyTransfer = 16;
    public static readonly Int32 SAccountVerification = 17;
    public static readonly Int32 SMoneyTransferCommission = 18;
    public static readonly Int32 SMoneyTransferCashBack = 19;
    public static readonly Int32 SMoneyTransferEKYC = 56;
    // END DMT

    //AEPS
    public static readonly Int32 SAEPSCW = 20;
    public static readonly Int32 SAEPSMS = 21;
    public static readonly Int32 SAEPSAP = 22;
    public static readonly Int32 SAEPSCD = 23;
    public static readonly Int32 SAEPSCWCommision = 24;
    public static readonly Int32 SAEPSMSCommision = 25;
    public static readonly Int32 SAEPSCDCommision = 26;
    public static readonly Int32 SAEPSAPCommision = 27;
    public static readonly Int32 SAEPSOnBoard = 49;

    // END AEPS

    //UPI Transfer
    public static readonly Int32 SUPITransfer = 28;
    public static readonly Int32 SUPIVerification = 29;
    public static readonly Int32 SUPITransferCommission = 30;
    public static readonly Int32 SUPITransferCashBack = 31;
    // END UPI Transfer

    //UPI Load
    public static readonly Int32 SUPILoad = 32;
    // END UPI Load

    //NSDL Pan
    public static readonly Int32 SPanCard = 33;
    public static readonly Int32 SPanCardCommission = 34;
    // END NSDL Pan

    //NSDL Pan
    public static readonly Int32 SMatm = 35;
    public static readonly Int32 SMatmCommission = 36;
    // END NSDL Pan

    public static readonly Int32 SSettlement = 37;


    //Add Deduct Fund
    public static readonly Int32 SFundAdd = 38;
    public static readonly Int32 SFundTransfer = 39;
    public static readonly Int32 SFundRecevied = 40;
    public static readonly Int32 SDeductFund = 41;
    public static readonly Int32 SHoldAmt = 42;
    //END Add Deduct Fund

    public static readonly Int32 SBus = 43;
    public static readonly Int32 SFlight = 44;
    public static readonly Int32 SIrctc = 45;

    //Recharge Commission
    public static readonly Int32 SRCommission = 46;
    public static readonly Int32 SServiceActive = 47;
    public static readonly Int32 SServiceActiveCommission = 48;

    public static readonly Int32 SPlanPurchase = 56;

    public static readonly Int32 SVan = 50;

    public static readonly Int32 SCreditBillPay = 51;
    public static readonly Int32 SCreditBillPayCommission = 52;

    public static readonly Int32 SPG = 53;
    public static readonly Int32 SPGCommission = 54;

    public static readonly Int32 SProductPurhase = 55;


    // DMT PPI
    public static readonly Int32 SDMTPPI = 57;
    public static readonly Int32 SDMTPPIAccountVerification = 58;
    public static readonly Int32 SDMTPPICommission = 59;
    public static readonly Int32 SDMTPPICashBack = 60;
    public static readonly Int32 SDMTPPIEKYC = 61;
    // END DMT PPI


    //Email ID
    public static readonly string RegistrationEmailTemplate = "welcome.htm";
    public static readonly string ForgetmailTemplate = "Forget.htm";
    public static readonly string TransactionTemplate = "Transaction.htm";
    public static readonly string VerifyTemplate = "Verify.htm";
    public static readonly string AccountHoldTemplate = "AccountHold.htm";
    public static readonly string AccountActiveTemplate = "AccountActive.htm";


    public static string getipAddress()
    {
        string ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        return ipAddress;
    }




    public static string GenerateUtrAsync()
    {
        Random random = new Random();
        // Get current date and time
        DateTime now = DateTime.Now;
        // await Task.Delay(1); // Simulate a delay of 100 milliseconds
        // Generate a random number between 1000 and 9999
        int randomNumber = random.Next(1000, 9999);

        // Format the UTR string using current date and random number
        string utr = $"{now:yyyyMMddHHmmss}{randomNumber}";

        return utr;
    }


    public static void Clear(ControlCollection ctrls)
    {
        foreach (Control ctrl in ctrls)
        {
            if (ctrl is TextBox)
                ((TextBox)ctrl).Text = string.Empty;
            else if (ctrl is DropDownList)
                ((DropDownList)ctrl).ClearSelection();
            else if (ctrl is Repeater)
            {
                ((Repeater)ctrl).Dispose();
                ((Repeater)ctrl).DataBind();
            }


            Clear(ctrl.Controls);
        }
    }
    public static string SerializeToJSon(DataTable dt)
    {
        JavaScriptSerializer ser = new JavaScriptSerializer();
        List<Dictionary<string, object>> dataRows = new List<Dictionary<string, object>>();
        dt.Rows.Cast<DataRow>().ToList().ForEach(dataRow =>
        {
            var row = new Dictionary<string, object>();
            dt.Columns.Cast<DataColumn>().ToList().ForEach(column =>
            {
                row.Add(column.ColumnName, dataRow[column]);
            });
            dataRows.Add(row);
        });
        return ser.Serialize(dataRows);
    }

    public static DataTable GetJSONToDataTableUsingNewtonSoftDll(string JSONData)
    {
        DataTable dt = (DataTable)JsonConvert.DeserializeObject(JSONData, (typeof(DataTable)));
        return dt;
    }

    public static string GetBranchCode(string PinCode)
    {
        // Allow TLS versions
        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 |    
            SecurityProtocolType.Tls11 |
            SecurityProtocolType.Tls;

        // Ignore SSL certificate errors
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (
                object sender,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

        var options = new RestClientOptions("https://api.postalpincode.in")
        {
            MaxTimeout = -1,

            // Ignore SSL validation for RestSharp
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };

        var client = new RestClient(options);

        var request = new RestRequest($"/pincode/{PinCode}", Method.Get);

        RestResponse response = client.Execute(request);

        return response.Content;
    }


    public ConstantsData()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}