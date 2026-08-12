using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


/// <summary>
/// Summary description for QueryRemitter
/// </summary>
public class QueryRemitter : IQueryRemitter
{
    cls_connection cls;
    public QueryRemitter()
    {
        cls = new cls_connection();
    }
    public async Task<CommonSearchResult> DMTPayoutDMTQueryRemitter(CommonSearch commonSearch)
    {
        var pipeSetting = await DMTAllProcess.GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "CUS_LOGIN");
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Face = string.Empty,
            Finger = string.Empty,
            Iris = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = false,
            IsCapture = false,
            IsOTP = pipeSetting.IsOTP,
            IsTPIN = pipeSetting.IsTPIN
        };

        var requestPayload = new
        {
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token,
            MethodName = "customerdetails",
            Mobile = commonSearch.Mobile,
            Pipe= commonSearch.Pipe
        };

        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);



        string responseJson = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.MoneyURL);

        var data = JObject.Parse(responseJson);
        string responseCode = data["code"]?.ToString() ?? string.Empty;
        string message = data["mess"]?.ToString() ?? "Unknown error";

        result.Message = message;

        switch (responseCode)
        {
            case "TXN":
                result.RCode = "0";
                result.Name = $"{data["data"][0]?["Name"]}".Trim();
                result.Limit = data["data"][0]?["TotalLimit"]?.ToString() ?? "0";
                break;
            case "ERR":
                result.RCode = "4";
                result.IsCapture = false;
                result.IsAadhar = false;
                result.IsOTP = pipeSetting.IsOTP;
                result.IsTPIN = pipeSetting.IsTPIN;
                break;
            default:
                result.RCode = "3";
                break;
        };
        return result;
    }
    public async Task<CommonSearchResult> FinoBankSoniTechnoQueryRemitter(CommonSearch commonSearch)
    {
        var pipeSetting = await DMTAllProcess.GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "CUS_LOGIN");

        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            IsAadhar = false,
            IsCapture = false,
            IsOTP = false,
            IsTPIN = false,
            Limit = "0",
            UseLimit = "0",
            Name = "",
            Finger = pipeSetting.WadhFinger,
            Iris = pipeSetting.WadhIris,
            Face = pipeSetting.WadhFace
        };

        try
        {

           
            // Prepare request
            var requestPayload = new
            {
                MethodName = "customerdetails",
                mobile = commonSearch.Mobile,
                Pipe = commonSearch.Pipe,
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);


            string responseJson = await ApiPostCall.PostCallAsync(requestJson, SONITechnoCredentrial.DMT);


            var data = JObject.Parse(responseJson);
            string responseCode = data["code"]?.ToString() ?? string.Empty;
            string message = data["mess"]?.ToString() ?? "Unknown error";

            result.Message = message;

            switch (responseCode)
            {
                case "TXN":
                    if (data["data"]["response_code"] == null)
                    {
                        result.RCode = "0";
                        result.Name = $"{data["data"]?["fname"]} {data["data"]?["lname"]}".Trim();
                        result.Limit = data["data"]?["limit"]?.ToString() ?? "0";
                    }
                    else if (data["data"]["response_code"].ToString() == "3")
                    {
                        result.RCode = "2";
                        result.stateresp = data["data"]?["stateresp"]?.ToString();
                        result.ekyc_id = data["data"]?["ekyc_id"]?.ToString();
                    }
                    else if (data["data"]["response_code"].ToString() == "2")
                    {
                        result.RCode = "1";
                        result.IsCapture = false;
                        result.IsAadhar = true;
                    }
                    else
                        result.RCode = "3";
                    break;
                default:
                    result.RCode = "3";
                    break;
            }
        }
        catch (Exception ex)
        {
            // Optional: Log exception here
            result.RCode = "3";
            result.Message = $"Exception: {ex.Message}";
        }
        return result;
    }
    public async Task<CommonSearchResult> PayoutDMTQueryRemitter(CommonSearch commonSearch)
    {
        var pipeSetting = await DMTAllProcess.GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "CUS_LOGIN");

        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Face = string.Empty,
            Finger = string.Empty,
            Iris = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = false,
            IsCapture = false,
            IsOTP = pipeSetting.IsOTP,
            IsTPIN = pipeSetting.IsTPIN
        };

        DataTable dtcus = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.Mobile + "'");

        OTPService oTPService = new OTPService();
        OTPToken oTPToken;
        string str = string.Empty;
        string[] ValueArray = new string[4];
        try
        {
            // Get member details
            IWallet wallet = new Wallet();
            var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);

            Int32 CusCount = dtcus.Rows.Count;

            switch (CusCount)
            {
                case 1: // Valid remitter found
                    result.RCode = "0";
                    result.IsOTP = pipeSetting.IsOTP;
                    result.IsTPIN = pipeSetting.IsTPIN;
                    result.Name = dtcus.Rows[0]["Name"].ToString();
                    result.Limit = dtcus.Rows[0]["Limit"].ToString();
                    result.UseLimit = dtcus.Rows[0]["Climit"].ToString();
                    if (result.IsOTP)
                    {
                        oTPToken = oTPService.GenerateOTP(Convert.ToString(memberDetails.Msrno));
                        ValueArray[0] = "Dear";
                        ValueArray[1] = oTPToken.OTP;
                        str = SMS.SendWithV(commonSearch.Mobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, "1");
                        result.stateresp = oTPToken.Token;
                    }

                    if (result.IsTPIN)
                    {
                        oTPToken = oTPService.GenerateOTPTpin(Convert.ToString(memberDetails.Msrno), memberDetails.LoginPin);
                        result.stateresp = oTPToken.Token;
                    }
                    break;

                case 0: // Needs capture/verification
                    result.RCode = "4";
                    result.IsCapture = false;
                    result.Message = "No matching customer was found. Please ensure the details are correct and try again.";
                    if (result.IsOTP)
                    {
                        oTPToken = oTPService.GenerateOTP(Convert.ToString(memberDetails.Msrno));
                        ValueArray[0] = "Dear";
                        ValueArray[1] = oTPToken.OTP;
                        str = SMS.SendWithV(commonSearch.Mobile, 34, ValueArray, "1");
                        result.stateresp = oTPToken.Token;
                    }
                    if (result.IsTPIN)
                    {
                        oTPToken = oTPService.GenerateOTPTpin(Convert.ToString(memberDetails.Msrno), memberDetails.LoginPin);
                        result.stateresp = oTPToken.Token;
                    }
                    break;

                default: // Any unknown response
                    result.RCode = "3";
                    break;
            }
        }
        catch (Exception ex)
        {
            result.RCode = "3";
            result.Message = $"Exception: {ex.Message}";
        }

        return result;
    }
    public async Task<CommonSearchResult> PayoutDMTWithOTP(CommonSearchWithOTP commonSearch)
    {
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Face = string.Empty,
            Finger = string.Empty,
            Iris = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = false,
            IsCapture = false,
            IsOTP = false,
            IsTPIN = false,
            RCode = "3", // default to error
            Message = "Unexpected error"
        };

        var cls = new cls_connection();
        var otpService = new OTPService();

        string otp = HttpUtility.HtmlEncode(commonSearch.OTP);
        string otpToken = HttpUtility.HtmlEncode(commonSearch.stateresp);

        // 1. Blocked check
        if (otpService.IsBlocked(otpToken))
        {
            result.RCode = "3";
            result.Message = "Too many attempts. Please try again later.";
            return result;
        }

        // 2. OTP validation
        if (!otpService.ValidateOTP(otpToken, otp))
        {
            otpService.IncrementAttempts(otpToken);
            result.RCode = "3";
            result.Message = "Invalid OTP.";
            return result;
        }

        try
        {
            // 3. Check customer
            DataTable customerTable = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.Mobile + "'");

            // 4. Fetch wallet/member details (if required)
            IWallet wallet = new Wallet();
            var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);

            if (customerTable.Rows.Count == 1)
            {
                // ✅ Valid remitter found
                var row = customerTable.Rows[0];
                result.RCode = "0";
                result.IsOTP = false;
                result.IsTPIN = false;
                result.Name = row["Name"]?.ToString() ?? "";
                result.Limit = row["Limit"]?.ToString() ?? "0";
                result.UseLimit = row["Climit"]?.ToString() ?? "0";
                result.Message = "Success";
            }
            else if (customerTable.Rows.Count == 0)
            {
                // ❌ No remitter found → Needs registration
                result.RCode = "4";
                result.IsCapture = false;
                result.IsOTP = true;
                result.IsTPIN = false;
                result.Message = "Customer not registered. Please complete verification.";
            }
            else
            {
                // ⚠ Unexpected multiple records
                result.RCode = "3";
                result.Message = "Multiple customer records found. Please contact support.";
            }
        }
        catch (Exception ex)
        {
            // Centralized error handling
            result.RCode = "3";
            result.Message = $"Error occurred: {ex.Message}";
            // TODO: log exception (ex) for debugging
        }

        return result;
    }
}