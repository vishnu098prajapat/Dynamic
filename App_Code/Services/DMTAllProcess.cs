using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for DMTAllProcess
/// </summary>
public class DMTAllProcess
{

    public DMTAllProcess()
    {

    }

    public static async Task<PipeModuleSetting> GetPipeSettings(string pipe, string ServiceID, string ModuleName)
    {
        cls_connection cls = new cls_connection();
        PipeModuleSetting list = new PipeModuleSetting();

        string query = @"SELECT *
                     FROM PipeModuleSettings
                     WHERE ModuleName='" + ModuleName + "' and Pipe =(select id from PipeMaster where PipeName='" + pipe + "' and ServiceID ='" + ServiceID + "') and ServiceID ='" + ServiceID + "'";

        DataTable dt = await cls.select_data_dtAsync(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                list = new PipeModuleSetting
                {
                    ModuleName = row["ModuleName"].ToString(),
                    ISFace = Convert.ToBoolean(row["ISFace"]),
                    ISFing = Convert.ToBoolean(row["ISFinger"]),
                    IsIris = Convert.ToBoolean(row["IsIris"]),
                    WadhFace = row["WadhFace"].ToString(),
                    WadhFinger = row["WadhFinger"].ToString(),
                    WadhIris = row["WadhIris"].ToString(),
                    IsOTP = Convert.ToBoolean(row["IsOTP"]),
                    IsTPIN = Convert.ToBoolean(row["IsTPIN"]),
                    IsRequired = Convert.ToBoolean(row["IsRequired"])
                };
            }
        }

        return list;
    }

    public static DataTable ConvertJsonToDataTable(string json)
    {
        // Create final DataTable with required columns
        DataTable dt = new DataTable();
        dt.Columns.Add("BANKID", typeof(int));
        dt.Columns.Add("BANK NAME", typeof(string));
        dt.Columns.Add("IFSC", typeof(string));
        dt.Columns.Add("AddDate", typeof(DateTime));

        // Parse JSON
        JObject obj = JObject.Parse(json);
        JArray arr = (JArray)obj["data"];

        foreach (var item in arr)
        {
            DataRow row = dt.NewRow();

            row["BANKID"] = Convert.ToInt32(item["bank_id"]?.ToString() ?? "0");
            row["BANK NAME"] = item["bank_name"]?.ToString();
            row["IFSC"] = item["bank_ifsc"]?.ToString();
            row["AddDate"] = DateTime.Now;   // because AddDate not in JSON

            dt.Rows.Add(row);
        }

        return dt;
    }

    #region [ QueryRemitter ]
    //Pipe Bank-1 Fino Bank Paysprint
    public static async Task<CommonSearchResult> FinoPaysprintDMT(CommonSearch commonSearch)
    {
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            IsAadhar = true,
            IsCapture = false,
            IsOTP = false,
            IsTPIN = false,
            Limit = "0",
            UseLimit = "0",
            Name = ""
        };

        try
        {
            var requestPayload = new PaysPrintDMTCheckCustomer
            {
                mobile = commonSearch.Mobile
            };

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt/kyc/remitter/queryremitter",
                tokenProvider.GetToken(),
                requestPayload.GetJson()
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            string message = data["message"]?.ToString() ?? "Unknown error";

            result.Message = message;

            switch (responseCode)
            {
                case "1":
                    result.RCode = "0";
                    result.Name = $"{data["data"]?["fname"]} {data["data"]?["lname"]}".Trim();
                    result.Limit = data["data"]?["limit"]?.ToString() ?? "0";
                    break;

                case "2":
                    result.RCode = "1";
                    result.IsCapture = false;
                    result.IsAadhar = true;
                    break;

                case "3":
                    result.RCode = "2";
                    result.stateresp = data["data"]?["stateresp"]?.ToString();
                    result.ekyc_id = data["data"]?["ekyc_id"]?.ToString();

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

    //Pipe Bank-2 Airtel Bank Paysprint
    public static async Task<CommonSearchResult> AirtelPaysprintDMT(CommonSearch commonSearch)
    {
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = false,
            IsCapture = false,
            IsOTP = false,
            IsTPIN = false
        };

        try
        {
            // Prepare request payload
            var customerRegistration = new PaysPrintDMTCheckCustomer
            {
                mobile = commonSearch.Mobile
            };

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt-v2/remitter/queryremitter",
                tokenProvider.GetToken(),
                customerRegistration.GetJson()
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            result.Message = data["message"]?.ToString() ?? "Unknown error";

            switch (responseCode)
            {
                case "1": // Valid remitter found
                    result.RCode = "0";
                    result.Limit = data["data"]?["limit"]?.ToString() ?? "0";
                    break;

                case "2": // Needs Aadhaar KYC
                    result.RCode = "1";
                    result.IsAadhar = true;
                    result.Finger = "E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=";
                    result.Iris = "";
                    result.Face = "sgydIC09zzy6f8Lb3xaAqzKquKe9lFcNR9uTvYxFp+A=";
                    break;

                default: // Any other case (error/failure)
                    result.RCode = "3";
                    result.Finger = "E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=";
                    result.Iris = "";
                    result.Face = "sgydIC09zzy6f8Lb3xaAqzKquKe9lFcNR9uTvYxFp+A=";
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

    //Pipe Bank-3 Jio Bank Paysprint
    public static async Task<CommonSearchResult> JioPaysprintDMT(CommonSearch commonSearch)
    {
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = false,
            IsCapture = false,
            IsOTP = false,
            IsTPIN = false
        };

        try
        {
            // Get member details
            IWallet wallet = new Wallet();
            var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);

            // Prepare request payload
            var requestPayload = new
            {
                merchantcode = PaysPrint.PARTNERID,
                mobile = PaysPrint.JWTencryptionKey,
                latitude = memberDetails.latitude,
                longitude = memberDetails.longitude,
                accessmode = commonSearch.Source
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt-v6/Remitter/search",
                tokenProvider.GetToken(),
                requestJson
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            result.Message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            switch (responseCode)
            {
                case "1": // Valid remitter found
                    result.RCode = "0";
                    string fname = data["data"]?["fname"]?.ToString() ?? string.Empty;
                    string lname = data["data"]?["lname"]?.ToString() ?? string.Empty;
                    result.Name = $"{fname} {lname}".Trim();
                    result.Limit = data["data"]?["limit"]?.ToString() ?? "0";
                    break;

                case "0": // Needs capture/verification
                    result.RCode = "1";
                    result.IsCapture = true;
                    result.Finger = commonSearch.Mobile;
                    result.Iris = commonSearch.Mobile;
                    result.Face = commonSearch.Mobile;
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

    //Pipe Bank-4 Payout Bank




    //Pipe Bank-5 epaysolutions Bank Paysprint
    #endregion

    #region [ QueryRemitterEkyc ]

    //Pipe Bank-1 Fino Bank EKYC
    public static async Task<CommonSearchResult> FinoEKYCPaysprintDMT(CommonDMTEkycProcess commonSearch)
    {
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = false,
            IsCapture = true, // Default true since eKYC requires capture,
            IsOTP = false,
            IsTPIN = false
        };

        try
        {
            // Encrypt the Aadhaar payload
            string encryptedData = Aes256CbcEncrypter.EncryptString(
                commonSearch.Data,
                Encoding.ASCII.GetBytes(PaysPrint.AESKEY),
                Encoding.ASCII.GetBytes(PaysPrint.AESIV)
            );

            // Determine biometric mode
            string biometricMode =
                string.IsNullOrWhiteSpace(commonSearch.DeviceName) ||
                !string.Equals(commonSearch.DeviceName, "FACE", StringComparison.OrdinalIgnoreCase)
                    ? "2"   // Default to iris
                    : "4";  // Face

            // Prepare request
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                lat = commonSearch.latitude,
                @long = commonSearch.longitude,
                aadhaar_number = commonSearch.AadharNo,
                data = encryptedData,
                accessmode = commonSearch.Source,
                is_iris = biometricMode
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            // Send request
            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt/kyc/remitter/queryremitter/kyc",
                tokenProvider.GetToken(),
                requestJson
            );

            // Parse response
            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            result.Message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "1")
            {
                result.RCode = "0";
                result.stateresp = data["data"]?["stateresp"]?.ToString();
                result.ekyc_id = data["data"]?["ekyc_id"]?.ToString();
            }
            else
            {
                result.RCode = "3";
            }
        }
        catch (Exception ex)
        {
            result.RCode = "3";
            result.Message = $"Exception: {ex.Message}";
        }

        return result;
    }

    public static async Task<CommonSearchResult> FinoEKYCSoniDMT(CommonDMTEkycProcess commonSearch)
    {
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = false,
            IsCapture = true, // Default true since eKYC requires capture,
            IsOTP = false,
            IsTPIN = false
        };

        try
        {
            // Prepare request
            var requestPayload = new
            {
                aadhaar_number = commonSearch.AadharNo.Replace(" ", ""),
                lat = commonSearch.latitude,
                @long = commonSearch.longitude,
                MethodName = "ekyc",
                mobile = commonSearch.Mobile,
                data = commonSearch.Data,
                Pipe = commonSearch.Pipe,
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            // Send request
            string responseJson = await ApiPostCall.PostCallAsync(requestJson, SONITechnoCredentrial.DMT);

            // Parse response
            var data = JObject.Parse(responseJson);
            string responseCode = data["code"]?.ToString() ?? string.Empty;
            result.Message = data["mess"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "TXN")
            {
                result.RCode = "0";
                result.stateresp = data["data"]?["stateresp"]?.ToString();
                result.ekyc_id = data["data"]?["ekyc_id"]?.ToString();
            }
            else
            {
                result.RCode = "3";
            }
        }
        catch (Exception ex)
        {
            result.RCode = "3";
            result.Message = $"Exception: {ex.Message}";
        }

        return result;
    }

    //Pipe Bank-2 Airtel Bank EKYC
    public static async Task<CommonSearchResult> AirtelEKYCPaysprintDMT(CommonDMTEkycProcess commonSearch)
    {
        var result = new CommonSearchResult
        {
            Pipe = commonSearch.Pipe,
            Mobile = commonSearch.Mobile,
            Name = string.Empty,
            Limit = "0",
            UseLimit = "0",
            IsAadhar = true,
            IsCapture = false,
            IsOTP = false,
            IsTPIN = false
        };

        try
        {
            // Prepare request payload
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                aadhaar_no = commonSearch.AadharNo
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            // Send API request
            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt-v2/remitter/queryremitter/aadhar_verify",
                tokenProvider.GetToken(),
                requestJson
            );

            // Parse response
            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            result.Message = data["message"]?.ToString() ?? "Unknown error";

            if (responseCode == "1")
            {
                result.RCode = "0";
                result.stateresp = data["stateresp"]?.ToString();
                result.ekyc_id = string.Empty;
            }
            else
            {
                result.RCode = "3";
            }
        }
        catch (Exception ex)
        {
            result.RCode = "3";
            result.Message = $"Exception: {ex.Message}";
        }

        return result;
    }

    //Pipe Bank-5 epaysolutions Bank EKYC

    #endregion


    #region [ RegisterRemitter ]
    //Pipe Bank-1 Fino Bank RegisterRemitter
    public static async Task<string> FinoRegisterRemitterPaysprintDMT(CommonDMTRegistration commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                otp = commonSearch.OTP,
                stateresp = commonSearch.stateresp,
                ekyc_id = commonSearch.ekyc_id
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt/kyc/remitter/registerremitter",
                tokenProvider.GetToken(),
                requestJson
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            string message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            return responseCode == "1"
                ? ErrorShow.SuccessMessageAPI(message)
                : ErrorShow.ErrorMessageAPI(message);
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    public static async Task<string> FinoRegisterRemitterSoniDMT(CommonDMTRegistration commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                firstname = "Soni",
                mobile = commonSearch.Mobile,
                lastname = "Soni",
                address = commonSearch.Data,
                pincode = "302020",
                dob = "1995-01-01",
                otp = commonSearch.OTP,
                gst_state = "17",
                stateresp = commonSearch.stateresp,
                ekyc_id = commonSearch.ekyc_id,
                bank3_flag = "no",
                MethodName = "customerregistration",
                Pipe = commonSearch.Pipe,
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await ApiPostCall.PostCallAsync(requestJson, SONITechnoCredentrial.DMT);

            var data = JObject.Parse(responseJson);
            string responseCode = data["code"]?.ToString() ?? string.Empty;
            string message = data["mess"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            return responseCode == "TXN"
                ? ErrorShow.SuccessMessageAPI(message)
                : ErrorShow.ErrorMessageAPI(message);
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    public static async Task<string> RegisterRemitterDMTPayoutSettlementSoniTechno(PayoutCustomerRegistrationRequest commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token,
                MethodName = "customerregistration",
                Mobile = commonSearch.Mobile,
                AADHAR = commonSearch.Aadhar,
                PanAADHAR = commonSearch.Pan,
                Name = commonSearch.Name,
                ADDRESS = commonSearch.Address,
            };

            string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);

            string responseJson = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.MoneyURL);

            var data = JObject.Parse(responseJson);
            string responseCode = data["code"]?.ToString() ?? string.Empty;
            string message = data["mess"]?.ToString() ?? "Unknown error";


            return responseCode == "TXN"
                ? ErrorShow.SuccessMessageAPI(message)
                : ErrorShow.ErrorMessageAPI(message);
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    //Pipe Bank-2 Airtel Bank RegisterRemitter
    public static async Task<string> AirtelRegisterRemitterPaysprintDMT(CommonDMTRegistration commonSearch)
    {
        try
        {
            // Encrypt the data block
            string encryptedData = Aes256CbcEncrypter.EncryptString(
                commonSearch.Data,
                Encoding.ASCII.GetBytes(PaysPrint.AESKEY),
                Encoding.ASCII.GetBytes(PaysPrint.AESIV)
            );

            // Determine biometric mode
            string biometricMode =
                string.IsNullOrWhiteSpace(commonSearch.DeviceName) ||
                !string.Equals(commonSearch.DeviceName, "FACE", StringComparison.OrdinalIgnoreCase)
                    ? "2"   // Default to iris
                    : "4";  // Face

            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                otp = commonSearch.OTP,
                stateresp = commonSearch.stateresp,
                is_iris = biometricMode,
                data = encryptedData,
                accessmode = commonSearch.Source
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt-v2/remitter/registerremitter",
                tokenProvider.GetToken(),
                requestJson
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            string message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            return responseCode == "1"
                ? ErrorShow.SuccessMessageAPI(message)
                : ErrorShow.ErrorMessageAPI(message);
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    //Pipe Bank-4 Payout Bank With OTP


    //Pipe Bank-4 Payout Bank Customer Registration
    public static async Task<string> PayoutCustomerRegistrationRequest(PayoutCustomerRegistrationRequest customerRegistration)
    {
        if (customerRegistration == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();
        var otpService = new OTPService();

        string txtOTP = HttpUtility.HtmlEncode(customerRegistration.OTP);
        string tokenOTP = HttpUtility.HtmlEncode(customerRegistration.stateresp);

        // Blocked check
        if (otpService.IsBlocked(tokenOTP))
            return ErrorShow.ErrorMessageAPI("Too many attempts. Please try again later.");

        // OTP validation
        if (!otpService.ValidateOTP(tokenOTP, txtOTP))
        {
            otpService.IncrementAttempts(tokenOTP);
            return ErrorShow.ErrorMessageAPI("Invalid OTP.");
        }

        // Use parameterized query instead of string concatenation (safe way)
        DataTable dt = await cls.select_data_dtAsync("AddEditCustomerDMT '" + $"{customerRegistration.Name}" + "','" + customerRegistration.Mobile + "','" + customerRegistration.Address + "','" + customerRegistration.Pan + "','" + customerRegistration.Aadhar + "'," + customerRegistration.MemberID + "");

        if (dt.Rows.Count > 0)
            return ErrorShow.SuccessMessageAPI("Successfully registered.");
        else
            return ErrorShow.ErrorMessageAPI("Customer already exists.");
    }



    #endregion


    //Pipe Bank-1 Fino Register Beneficiary
    public static async Task<string> FinoRegisterBeneficiaryPaysprintDMT(CommonDMTRegisterBeneficiary commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                benename = commonSearch.BeniName,
                bankid = commonSearch.BankID,
                accno = commonSearch.AccountNo,
                ifsccode = commonSearch.IFSCCODE,
                verified = commonSearch.Verified
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt/kyc/beneficiary/registerbeneficiary",
                tokenProvider.GetToken(),
                requestJson
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            string message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "1")
            {
                var cres = new CommonDMTRegisterBeneficiaryResponse
                {
                    RCode = "0",
                    Message = message,
                    IsOTP = false,
                    stateresp = ""
                };

                string resp = JsonConvert.SerializeObject(cres);
                return ErrorShow.SuccessArrayAPI(resp);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(message);
            }
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }


    //Pipe Bank-2 Airtel Register Beneficiary
    public static async Task<string> AirtelRegisterBeneficiaryPaysprintDMT(CommonDMTRegisterBeneficiary commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                benename = commonSearch.BeniName,
                bankid = commonSearch.BankID,
                accno = commonSearch.AccountNo,
                ifsccode = commonSearch.IFSCCODE,
                verified = commonSearch.Verified
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt-v2/beneficiary/registerbeneficiary",
                tokenProvider.GetToken(),
                requestJson
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            string message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "1")
            {
                var cres = new CommonDMTRegisterBeneficiaryResponse
                {
                    RCode = "0",
                    Message = message,
                    IsOTP = false,
                    stateresp = ""
                };

                string resp = JsonConvert.SerializeObject(cres);
                return ErrorShow.SuccessArrayAPI(resp);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(message);
            }
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    //Pipe Bank-4 Payout Bank Add Beni
    public static async Task<string> PayoutAddBeni(CommonDMTRegisterBeneficiary commonSearch)
    {
        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();

        var pipeSetting = await GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "ADD_BENI");


        DataTable dtcus = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.Mobile + "'");

        if (dtcus.Rows.Count <= 0)
            return ErrorShow.ErrorMessageAPI("Remitter account is not registered. Please complete remitter registration before proceeding with money transfer.");


        OTPService oTPService = new OTPService();
        OTPToken oTPToken = new OTPToken();
        string str = string.Empty;
        string[] ValueArray = new string[4];

        // Get member details
        IWallet wallet = new Wallet();
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);

        string Message = "";

        if (pipeSetting.IsOTP)// for 
        {
            oTPToken = oTPService.GenerateOTP(Convert.ToString(memberDetails.Msrno));
            ValueArray[0] = "Dear";
            ValueArray[1] = oTPToken.OTP;
            str = SMS.SendWithV(commonSearch.Mobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, "1");
        }
        else if (pipeSetting.IsTPIN)
        {
            oTPToken = oTPService.GenerateOTPTpin(Convert.ToString(memberDetails.Msrno), memberDetails.LoginPin);
        }
        else
        {
            Int32 Val = 0;
            Val = await cls.update_dataAsync("AddEditBeneficiaryDMT " + dtcus.Rows[0]["ID"].ToString() + ",'" + commonSearch.BeniName + "','" + commonSearch.Mobile + "','" + commonSearch.AccountNo + "','" + commonSearch.IFSCCODE + "','" + commonSearch.Verified + "','" + commonSearch.BankName + "'");
            if (Val > 0)
            {
                Message = "Add Beneficiary Successfully";
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Already exists");
            }
        }

        var cres = new CommonDMTRegisterBeneficiaryResponse
        {
            RCode = "0",
            Message = Message,
            IsOTP = pipeSetting.IsOTP,
            IsTpin = pipeSetting.IsTPIN,
            stateresp = oTPToken.Token
        };
        string resp = JsonConvert.SerializeObject(cres);
        return ErrorShow.SuccessArrayAPI(resp);

    }

    public static async Task<string> DMTPayoutAddBeni(CommonDMTRegisterBeneficiary commonSearch)
    {
        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");


        var pipeSetting = await GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "ADD_BENI");


        OTPService oTPService = new OTPService();
        OTPToken oTPToken = new OTPToken();
        string str = string.Empty;
        string[] ValueArray = new string[4];

        // Get member details
        IWallet wallet = new Wallet();
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);

        string Message = "";

        if (pipeSetting.IsOTP)// for 
        {
            oTPToken = oTPService.GenerateOTP(Convert.ToString(memberDetails.Msrno));
            ValueArray[0] = "Dear";
            ValueArray[1] = oTPToken.OTP;
            str = SMS.SendWithV(commonSearch.Mobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, "1");
        }
        else if (pipeSetting.IsTPIN)
        {
            oTPToken = oTPService.GenerateOTPTpin(Convert.ToString(memberDetails.Msrno), memberDetails.LoginPin);
        }
        else
        {
            var requestPayload = new
            {
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token,
                MethodName = "addaccount",
                Name = commonSearch.BeniName,
                Mobile = commonSearch.Mobile,
                CustomerMobile = commonSearch.Mobile,
                BankName = commonSearch.BankName,
                accountNumber = commonSearch.AccountNo,
                IFSC = commonSearch.IFSCCODE,
                accountType = "IMPS",
                BankID = "0"
            };

            string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);

            string responseJson = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.MoneyURL);

            var data = JObject.Parse(responseJson);
            string responseCode = data["code"]?.ToString() ?? string.Empty;
            string message = data["mess"]?.ToString() ?? "Unknown error";


            var cress = new CommonDMTRegisterBeneficiaryResponse
            {
                RCode = "0",
                Message = message,
                IsOTP = pipeSetting.IsOTP,
                IsTpin = pipeSetting.IsTPIN,
                stateresp = oTPToken.Token
            };
            string resps = JsonConvert.SerializeObject(cress);


            return responseCode == "TXN"
                ? ErrorShow.SuccessArrayAPI(resps)
                : ErrorShow.ErrorMessageAPI(message);

        }

        var cres = new CommonDMTRegisterBeneficiaryResponse
        {
            RCode = "0",
            Message = Message,
            IsOTP = pipeSetting.IsOTP,
            IsTpin = pipeSetting.IsTPIN,
            stateresp = oTPToken.Token
        };
        string resp = JsonConvert.SerializeObject(cres);
        return ErrorShow.SuccessArrayAPI(resp);

    }

    public static async Task<string> FinoRegisterBeneficiarySoniDMT(CommonDMTRegisterBeneficiary commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                accno = commonSearch.AccountNo,
                benename = commonSearch.BeniName,
                mobile = commonSearch.Mobile,
                bankid = commonSearch.BankID,
                ifsccode = commonSearch.IFSCCODE,
                address = "108 12 Jaipur Rajasthan",
                dob = "1995-02-05",
                pincode = commonSearch.Verified,
                verified = commonSearch.Verified,
                gst_state = "17",
                Pipe = commonSearch.Pipe,
                MethodName = "addaccount",
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            string responseJson = await ApiPostCall.PostCallAsync(requestJson, SONITechnoCredentrial.DMT);

            var data = JObject.Parse(responseJson);
            string responseCode = data["code"]?.ToString() ?? string.Empty;
            string message = data["mess"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "TXN")
            {
                var cres = new CommonDMTRegisterBeneficiaryResponse
                {
                    RCode = "0",
                    Message = message,
                    IsOTP = false,
                    stateresp = ""
                };

                string resp = JsonConvert.SerializeObject(cres);
                return ErrorShow.SuccessArrayAPI(resp);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(message);
            }
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    //Pipe Bank-4 Payout Bank Add Beni With OTP
    public static async Task<string> PayoutAddBeniWithOTP(CommonDMTRegisterBeneficiaryWithOTP commonSearch)
    {
        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();
        var otpService = new OTPService();

        string txtOTP = HttpUtility.HtmlEncode(commonSearch.OTP);
        string tokenOTP = HttpUtility.HtmlEncode(commonSearch.stateresp);

        // Blocked check
        if (otpService.IsBlocked(tokenOTP))
            return ErrorShow.ErrorMessageAPI("Too many attempts. Please try again later.");

        // OTP validation
        if (!otpService.ValidateOTP(tokenOTP, txtOTP))
        {
            otpService.IncrementAttempts(tokenOTP);
            return ErrorShow.ErrorMessageAPI("Invalid OTP.");
        }



        DataTable dtcus = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.Mobile + "'");

        if (dtcus.Rows.Count <= 0)
            return ErrorShow.ErrorMessageAPI("Remitter account is not registered. Please complete remitter registration before proceeding with money transfer.");


        Int32 Val = 0;
        Val = await cls.update_dataAsync("AddEditBeneficiaryDMT " + dtcus.Rows[0]["ID"].ToString() + ",'" + commonSearch.BeniName + "','" + commonSearch.Mobile + "','" + commonSearch.AccountNo + "','" + commonSearch.IFSCCODE + "','" + commonSearch.Verified + "','" + commonSearch.BankName + "'");
        if (Val > 0)
        {
            var cres = new CommonDMTRegisterBeneficiaryResponse
            {
                RCode = "0",
                Message = "Add Beneficiary Successfully",
                IsOTP = false,
                IsTpin = false,
                stateresp = ""
            };
            string resp = JsonConvert.SerializeObject(cres);
            return ErrorShow.SuccessArrayAPI(resp);
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Already exists");
        }
    }


    public static async Task<string> DMTPayoutAddBeniWithOTP(CommonDMTRegisterBeneficiaryWithOTP commonSearch)
    {
        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();
        var otpService = new OTPService();

        string txtOTP = HttpUtility.HtmlEncode(commonSearch.OTP);
        string tokenOTP = HttpUtility.HtmlEncode(commonSearch.stateresp);

        // Blocked check
        if (otpService.IsBlocked(tokenOTP))
            return ErrorShow.ErrorMessageAPI("Too many attempts. Please try again later.");

        // OTP validation
        if (!otpService.ValidateOTP(tokenOTP, txtOTP))
        {
            otpService.IncrementAttempts(tokenOTP);
            return ErrorShow.ErrorMessageAPI("Invalid OTP.");
        }


        var requestPayload = new
        {
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token,
            MethodName = "addaccount",
            Name = commonSearch.BeniName,
            Mobile = commonSearch.Mobile,
            CustomerMobile = commonSearch.Mobile,
            accountNumber = commonSearch.AccountNo,
            IFSC = commonSearch.IFSCCODE,
            accountType = "IMPS",
            BankID = commonSearch.BankID,
            BankName = commonSearch.BankName,
        };

        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);

        string responseJson = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.MoneyURL);

        var data = JObject.Parse(responseJson);
        string responseCode = data["code"]?.ToString() ?? string.Empty;
        string message = data["mess"]?.ToString() ?? "Unknown error";

        var cres = new CommonDMTRegisterBeneficiaryResponse
        {
            RCode = "0",
            Message = "Add Beneficiary Successfully",
            IsOTP = false,
            IsTpin = false,
            stateresp = ""
        };
        string resp = JsonConvert.SerializeObject(cres);
        return ErrorShow.SuccessArrayAPI(resp);

    }



    #region [ Delete Beneficiary ]
    //Pipe Bank-1 Fino Delete Beneficiary
    public static async Task<string> FinoDeleteBeneficiaryPaysprintDMT(CommonDMTDeleteBeneficiary commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                bene_id = commonSearch.BaniID
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt/kyc/beneficiary/registerbeneficiary/deletebeneficiary",
                tokenProvider.GetToken(),
                requestJson
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            string message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "1")
            {
                return ErrorShow.SuccessMessageAPI(message);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(message);
            }
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }
    //Pipe Bank-2 Airtel Delete Beneficiary
    public static async Task<string> AirtelDeleteBeneficiaryPaysprintDMT(CommonDMTDeleteBeneficiary commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                bene_id = commonSearch.BaniID
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            var tokenProvider = new JWTBankSendRequestNewIntegration();
            string responseJson = await PaysPrintPostCall.PostCallWithBodyAsync(
                "dmt-v2/beneficiary/registerbeneficiary/deletebeneficiary",
                tokenProvider.GetToken(),
                requestJson
            );

            var data = JObject.Parse(responseJson);
            string responseCode = data["response_code"]?.ToString() ?? string.Empty;
            string message = data["message"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "1")
            {
                return ErrorShow.SuccessMessageAPI(message);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(message);
            }
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    //Pipe Bank-4 Payout Delete Beneficiary
    public static async Task<string> PayoutDeleteBeneficiaryDMT(CommonDMTDeleteBeneficiary commonSearch)
    {
        try
        {
            if (commonSearch == null)
                return ErrorShow.ErrorMessageAPI("Invalid request.");

            var cls = new cls_connection();

            DataTable dtcus = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.Mobile + "'");

            if (dtcus.Rows.Count <= 0)
                return ErrorShow.ErrorMessageAPI("Remitter account is not registered. Please complete remitter registration before proceeding with money transfer.");


            DataTable dtbeni = await cls.select_data_dtAsync("ManageBeneficiaryDMT 'BeniID','" + dtcus.Rows[0]["id"].ToString() + "'");

            if (dtbeni.Rows.Count <= 0)
                return ErrorShow.ErrorMessageAPI("Beneficiary account is not registered. Please complete Beneficiary registration before proceeding with money transfer.");


            Int32 val = await cls.update_dataAsync("ManageBeneficiaryDMT 'Delete','" + commonSearch.BaniID + "'");

            if (val == 1)
            {
                return ErrorShow.SuccessMessageAPI("The beneficiary has been deleted");
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Account Not Exists");
            }
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    public static async Task<string> FinoDeleteBeneficiarySoniDMT(CommonDMTDeleteBeneficiary commonSearch)
    {
        try
        {
            var requestPayload = new
            {
                mobile = commonSearch.Mobile,
                bene_id = commonSearch.BaniID,
                MethodName = "deletebeneficiary",
                Pipe = commonSearch.Pipe,
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token
            };

            string requestJson = JsonConvert.SerializeObject(requestPayload);

            string responseJson = await ApiPostCall.PostCallAsync(requestJson, SONITechnoCredentrial.DMT);

            var data = JObject.Parse(responseJson);
            string responseCode = data["code"]?.ToString() ?? string.Empty;
            string message = data["mess"]?.ToString()?.Replace("\n", "") ?? "Unknown error";

            if (responseCode == "TXN")
            {
                return ErrorShow.SuccessMessageAPI(message);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(message);
            }
        }
        catch (Exception ex)
        {
            return ErrorShow.ErrorMessageAPI($"Exception: {ex.Message}");
        }
    }

    #endregion

    #region [ Fetch Beneficiary ]
    //Pipe Bank-1 Fino FetchList Beneficiary
    public async static Task<string> FinoFetchListBeneficiaryPaysprintDMT(CommonDMTFetchListBeneficiary commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        object returnData = new
        {
            mobile = commonSearch.Mobile
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("dmt/kyc/beneficiary/registerbeneficiary/fetchbeneficiary", jj.GetToken(), request);


        JObject Data = JObject.Parse(Result);
        var doc = JsonDocument.Parse(Result);
        var dataArray = doc.RootElement.GetProperty("data");

        if (Convert.ToString(Data["response_code"].ToString()) == "1")
        {
            var beneficiaries = dataArray.EnumerateArray()
          .Select(item => new CommonDMTFetchListBeneficiaryResponse
          {
              bene_id = item.GetProperty("bene_id").GetString(),
              bankid = item.GetProperty("bankid").GetString(),
              bankname = item.GetProperty("bankname").GetString(),
              name = item.GetProperty("name").GetString(),
              accno = item.GetProperty("accno").GetString(),
              ifsc = item.GetProperty("ifsc").GetString(),
              verified = item.GetProperty("verified").GetString(),
              banktype = item.GetProperty("banktype").GetString(),

              // Optional/default fields
              Mobile = commonSearch.Mobile,
              Pipe = commonSearch.Pipe,
              ServiceID = commonSearch.ServiceID,
              MemberID = "",
              SendMaxAmt = 5000,
              SendminAmt = 1000,
              SplitAmt = 5000,
              IsDown = false,
              IsOTP = true
          })
         .ToList();

            string resp = Newtonsoft.Json.JsonConvert.SerializeObject(beneficiaries);

            return ErrorShow.SuccessArrayAPI(resp);
        }
        else
            return ErrorShow.ErrorMessageAPI(Convert.ToString(Data["message"]).Replace("\n", ""));
    }

    public async static Task<string> FinoFetchListBeneficiarySoniDMT(CommonDMTFetchListBeneficiary commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        object returnData = new
        {
            mobile = commonSearch.Mobile,
            MethodName = "beneficiarylist",
            Pipe = commonSearch.Pipe,
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        string Result = await ApiPostCall.PostCallAsync(request, SONITechnoCredentrial.DMT);

        JObject Data = JObject.Parse(Result);
        var doc = JsonDocument.Parse(Result);

        if (Convert.ToString(Data["code"].ToString()) == "TXN")
        {
            var dataArray = doc.RootElement.GetProperty("data");

            var beneficiaries = dataArray.EnumerateArray()
          .Select(item => new CommonDMTFetchListBeneficiaryResponse
          {
              bene_id = item.GetProperty("bene_id").GetString(),
              bankid = item.GetProperty("bankid").GetString(),
              bankname = item.GetProperty("bankname").GetString(),
              name = item.GetProperty("name").GetString(),
              accno = item.GetProperty("accno").GetString(),
              ifsc = item.GetProperty("ifsc").GetString(),
              verified = item.GetProperty("verified").GetString(),
              banktype = item.GetProperty("banktype").GetString(),

              // Optional/default fields
              Mobile = commonSearch.Mobile,
              Pipe = commonSearch.Pipe,
              ServiceID = commonSearch.ServiceID,
              MemberID = "",
              SendMaxAmt = 5000,
              SendminAmt = 1000,
              SplitAmt = 5000,
              IsDown = false,
              IsOTP = true
          })
         .ToList();

            string resp = Newtonsoft.Json.JsonConvert.SerializeObject(beneficiaries);

            return ErrorShow.SuccessArrayAPI(resp);
        }
        else
            return ErrorShow.ErrorMessageAPI(Convert.ToString(Data["mess"]).Replace("\n", ""));
    }

    //Pipe Bank-1 Airtel FetchList Beneficiary
    public async static Task<string> AirtelFetchListBeneficiaryPaysprintDMT(CommonDMTFetchListBeneficiary commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        object returnData = new
        {
            mobile = commonSearch.Mobile
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("dmt-v2/beneficiary/registerbeneficiary/fetchbeneficiary", jj.GetToken(), request);


        JObject Data = JObject.Parse(Result);
        var doc = JsonDocument.Parse(Result);
        var dataArray = doc.RootElement.GetProperty("data");

        if (Convert.ToString(Data["response_code"].ToString()) == "1")
        {
            var beneficiaries = dataArray.EnumerateArray()
          .Select(item => new CommonDMTFetchListBeneficiaryResponse
          {
              bene_id = item.GetProperty("bene_id").GetString(),
              bankid = item.GetProperty("bankid").GetString(),
              bankname = item.GetProperty("bankname").GetString(),
              name = item.GetProperty("name").GetString(),
              accno = item.GetProperty("accno").GetString(),
              ifsc = item.GetProperty("ifsc").GetString(),
              verified = item.GetProperty("verified").GetString(),
              banktype = item.GetProperty("banktype").GetString(),
              // Optional/default fields
              Mobile = commonSearch.Mobile,
              Pipe = commonSearch.Pipe,
              ServiceID = commonSearch.ServiceID,
              MemberID = "",
              SendMaxAmt = 5000,
              SendminAmt = 1000,
              SplitAmt = 5000,
              IsDown = false,
              IsOTP = true
          })
         .ToList();

            string resp = Newtonsoft.Json.JsonConvert.SerializeObject(beneficiaries);

            return ErrorShow.SuccessArrayAPI(resp);
        }
        else
            return ErrorShow.ErrorMessageAPI(Convert.ToString(Data["message"]).Replace("\n", ""));
    }


    //Pipe Bank-4 Payout FetchList Beneficiary
    public async static Task<string> PayoutBeniList(CommonDMTFetchListBeneficiary commonSearch)
    {
        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();

        DataTable dtcus = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.Mobile + "'");

        if (dtcus.Rows.Count <= 0)
            return ErrorShow.ErrorMessageAPI("Remitter account is not registered. Please complete remitter registration before proceeding with money transfer.");


        CommonSearchResult commonSearchResult = new CommonSearchResult();

        DataTable dtbeni = await cls.select_data_dtAsync("ManageBeneficiaryDMT 'GetAllActive','" + dtcus.Rows[0]["id"].ToString() + "'");


        var pipeSetting = await GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "DO_TXN");



        // 2. Decide ISOTP only once (clean logic)
        bool isOtp = pipeSetting?.IsTPIN == true
                        ? false                     // TPIN enabled → OTP OFF
                        : (pipeSetting?.IsOTP == true);


        var beneficiaries = dtbeni.AsEnumerable()
        .Select(row => new CommonDMTFetchListBeneficiaryResponse
        {
            bene_id = row["ID"]?.ToString() ?? "",
            bankid = row["ID"]?.ToString() ?? "",
            bankname = row["bankname"]?.ToString() ?? "",
            name = row["name"]?.ToString() ?? "",
            accno = row["accountNumber"]?.ToString() ?? "",
            ifsc = row["ifsc"]?.ToString() ?? "",
            verified = (Convert.ToBoolean(row["otpVerify"].ToString()) == true ? "1" : "0"),
            banktype = row["accountType"]?.ToString() ?? "",
            // defaults
            Mobile = commonSearch.Mobile,
            Pipe = commonSearch.Pipe,
            ServiceID = commonSearch.ServiceID,
            MemberID = "",
            SendMaxAmt = Convert.ToDecimal(dtcus.Rows[0]["Limit"]),
            SendminAmt = 1000,
            SplitAmt = Convert.ToDecimal(dtcus.Rows[0]["SingleSlot"]),
            IsDown = false,
            IsOTP = isOtp
        })
        .ToList();

        string resp = Newtonsoft.Json.JsonConvert.SerializeObject(beneficiaries);

        return ErrorShow.SuccessArrayAPI(resp);

    }


    public async static Task<string> DMTPayoutBeniList(CommonDMTFetchListBeneficiary commonSearch)
    {
        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();


        var pipeSetting = await GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "DO_TXN");

        var requestPayload = new
        {
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token,
            MethodName = "beneficiarylist",
            Mobile = commonSearch.Mobile
        };

        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);



        string responseJson = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.MoneyURL);
        var data = JObject.Parse(responseJson);

        // 2. Decide ISOTP only once (clean logic)
        bool isOtp = pipeSetting?.IsTPIN == true
                        ? false                     // TPIN enabled → OTP OFF
                        : (pipeSetting?.IsOTP == true);


        var dataTable = data?["data"];

        var beneficiaries = dataTable != null
            ? dataTable.AsEnumerable()
                .Select(row => new CommonDMTFetchListBeneficiaryResponse
                {
                    bene_id = row["BeniID"]?.ToString() ?? "",
                    bankid = row["ID"]?.ToString() ?? "",
                    bankname = row["BankName"]?.ToString() ?? "",
                    name = row["Name"]?.ToString() ?? "",
                    accno = row["accountNumber"]?.ToString() ?? "",
                    ifsc = row["ifsc"]?.ToString() ?? "",
                    verified = "1",
                    banktype = "All",

                    // defaults
                    Mobile = commonSearch.Mobile,
                    Pipe = commonSearch.Pipe,
                    ServiceID = commonSearch.ServiceID,
                    MemberID = "",
                    SendMaxAmt = 50000,
                    SendminAmt = 10000,
                    SplitAmt = 50000,
                    IsDown = false,
                    IsOTP = isOtp
                })
                .ToList()
            : new List<CommonDMTFetchListBeneficiaryResponse>();

        string resp = Newtonsoft.Json.JsonConvert.SerializeObject(beneficiaries);



        return ErrorShow.SuccessArrayAPI(resp);

    }

    #endregion


    #region [ Send OTP For TXN ]

    //Pipe Bank-2 Airtel Transaction send otp
    public async static Task<string> AirtelTransactionSendOtpPaysprintDMT(CommonSendMoneyOTP commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        object returnData = new
        {
            mobile = commonSearch.CustomerMobile,
            referenceid = commonSearch.OrderID,
            bene_id = commonSearch.BeniID,
            txntype = commonSearch.txntype,
            amount = commonSearch.amount,
            pincode = "302020",
            address = "Jaipur Mansraovar",
            gst_state = "17",
            dob = "1995-02-05",
            lat = commonSearch.latitude,
            @long = commonSearch.longitude
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("dmt-v2/transact/transact/send_otp", jj.GetToken(), request);


        JObject TransferData = JObject.Parse(Result);

        if (TransferData["response_code"].ToString().ToUpper() == "1")
        {
            object obj = new
            {
                agentID = commonSearch.OrderID,
                Message = "Transaction Process",
                stateresp = TransferData["stateresp"].ToString(),
                OTP = true
            };

            string Request = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return ErrorShow.SuccessArrayAPI(Request);
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(TransferData["message"].ToString().Replace("\n", ""));
        }
    }

    //Pipe Bank-1 Fino Transaction send otp
    public async static Task<string> FinoTransactionSendOtpPaysprintDMT(CommonSendMoneyOTP commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        object returnData = new
        {
            mobile = commonSearch.CustomerMobile,
            referenceid = commonSearch.OrderID,
            bene_id = commonSearch.BeniID,
            txntype = commonSearch.txntype,
            amount = commonSearch.amount,
            pincode = "302020",
            address = "Jaipur Mansraovar",
            gst_state = "17",
            dob = "1995-02-05",
            lat = commonSearch.latitude,
            @long = commonSearch.longitude
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("dmt/kyc/transact/transact/send_otp", jj.GetToken(), request);


        JObject TransferData = JObject.Parse(Result);

        if (Convert.ToString(TransferData["response_code"]).ToUpper() == "1")
        {
            object obj = new
            {
                agentID = commonSearch.OrderID,
                Message = Convert.ToString(TransferData["message"]),
                stateresp = TransferData["stateresp"].ToString(),
                OTP = true
            };

            string Request = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return ErrorShow.SuccessArrayAPI(Request);
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(Convert.ToString(TransferData["message"]).Replace("\n", ""));
        }

    }

    //Pipe Bank-4 Airtel Transaction send otp
    public async static Task<string> PayoutTransactionSendOtpPaysprintDMT(CommonSendMoneyOTP commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();

        DataTable dtcus = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.CustomerMobile + "'");

        if (dtcus.Rows.Count <= 0)
            return ErrorShow.ErrorMessageAPI("Remitter account is not registered. Please complete remitter registration before proceeding with money transfer.");


        DataTable dtbeni = await cls.select_data_dtAsync("ManageBeneficiaryDMT 'BeniID','" + commonSearch.BeniID + "'");

        if (dtbeni.Rows.Count <= 0)
            return ErrorShow.ErrorMessageAPI("Beneficiary account is not registered. Please complete Beneficiary registration before proceeding with money transfer.");

        var pipeSetting = await GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "DO_TXN");


        OTPService oTPService = new OTPService();
        OTPToken oTPToken = new OTPToken();
        string str = string.Empty;
        string[] ValueArray = new string[4];

        // Get member details
        IWallet wallet = new Wallet();
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);


        if (pipeSetting.IsOTP && pipeSetting.IsTPIN == false)// for 
        {
            oTPToken = oTPService.GenerateOTP(Convert.ToString(memberDetails.Msrno));
            ValueArray[0] = "Dear";
            ValueArray[1] = oTPToken.OTP;
            str = SMS.SendWithV(commonSearch.CustomerMobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, "1");
        }
        else if (pipeSetting.IsTPIN && pipeSetting.IsOTP == false)
        {
            oTPToken = oTPService.GenerateOTPTpin(Convert.ToString(memberDetails.Msrno), memberDetails.LoginPin);
        }

        object obj = new
        {
            agentID = commonSearch.OrderID,
            Message = "Transaction Process",
            stateresp = oTPToken.Token,
            OTP = true
        };

        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        return ErrorShow.SuccessArrayAPI(Request);
    }

    public async static Task<string> DMTPayoutTransactionSendOtp(CommonSendMoneyOTP commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        if (commonSearch == null)
            return ErrorShow.ErrorMessageAPI("Invalid request.");

        var cls = new cls_connection();

        var pipeSetting = await GetPipeSettings(commonSearch.Pipe, commonSearch.ServiceID, "DO_TXN");


        OTPService oTPService = new OTPService();
        OTPToken oTPToken = new OTPToken();
        string str = string.Empty;
        string[] ValueArray = new string[4];

        // Get member details
        IWallet wallet = new Wallet();
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);


        if (pipeSetting.IsOTP && pipeSetting.IsTPIN == false)// for 
        {
            oTPToken = oTPService.GenerateOTP(Convert.ToString(memberDetails.Msrno));
            ValueArray[0] = "Dear";
            ValueArray[1] = oTPToken.OTP;
            str = SMS.SendWithV(commonSearch.CustomerMobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, "1");
        }
        else if (pipeSetting.IsTPIN && pipeSetting.IsOTP == false)
        {
            oTPToken = oTPService.GenerateOTPTpin(Convert.ToString(memberDetails.Msrno), memberDetails.LoginPin);
        }

        object obj = new
        {
            agentID = commonSearch.OrderID,
            Message = "Transaction Process",
            stateresp = oTPToken.Token,
            OTP = true
        };

        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        return ErrorShow.SuccessArrayAPI(Request);
    }

    public async static Task<string> FinoTransactionSendOtpSoniDMT(CommonSendMoneyOTP commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        object returnData = new
        {
            Amount = commonSearch.amount.ToString().ToInt32(),
            BeniID = commonSearch.BeniID,
            Mode = "IMPS",
            MethodName = "sendamountotp",
            OrderID = commonSearch.OrderID,
            AccountNO = commonSearch.AccNumber,
            IFSC = commonSearch.Isfc,
            BankName = commonSearch.BankName,
            Name = commonSearch.BeneName,
            mobile = commonSearch.CustomerMobile,
            stateresp = "",
            OTP = "",
            Pipe = commonSearch.Pipe,
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        string Result = await ApiPostCall.PostCallAsync(request, SONITechnoCredentrial.DMT);


        JObject TransferData = JObject.Parse(Result);

        if (Convert.ToString(TransferData["code"]).ToUpper() == "TXN")
        {
            object obj = new
            {
                agentID = TransferData["data"]["agentID"].ToString(),
                Message = Convert.ToString(TransferData["mess"]),
                stateresp = TransferData["data"]["stateresp"].ToString(),
                OTP = true
            };

            string Request = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return ErrorShow.SuccessArrayAPI(Request);
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(Convert.ToString(TransferData["mess"]).Replace("\n", ""));
        }

    }
    #endregion

    #region [ Send OTP For TXN ]
    //Pipe Bank-2 Airtel Transaction Send
    public async static Task<PayoutCommonResponse> AirtelTransactionSendPaysprintDMT(CommonSendMoney commonSearch)
    {
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        object returnData = new
        {
            mobile = commonSearch.CustomerMobile,
            referenceid = commonSearch.OrderID,
            bene_id = commonSearch.BeniID,
            txntype = commonSearch.txntype,
            amount = commonSearch.Amount,
            otp = commonSearch.OTP,
            pincode = "302020",
            address = "Jaipur Mansraovar",
            gst_state = "17",
            dob = "1995-02-05",
            lat = commonSearch.latitude,
            stateresp = commonSearch.stateresp,
            @long = commonSearch.longitude
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("dmt-v2/transact/transact", jj.GetToken(), request);

        PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();

        payoutCommonResponse.Request = request;
        payoutCommonResponse.Result = Result;
        payoutCommonResponse.AccountNo = commonSearch.AccNumber;
        payoutCommonResponse.IFSC = commonSearch.Isfc;
        payoutCommonResponse.Name = commonSearch.BeneName;
        payoutCommonResponse.OrderID = commonSearch.OrderID;
        payoutCommonResponse.Amount = commonSearch.Amount;

        JObject TransferData = JObject.Parse(Result);

        string txn_status = Convert.ToString(TransferData["txn_status"]).ToUpper();
        string Status = "Pending";
        string message = Convert.ToString(TransferData["message"]).Replace("\n", "");


        if (Convert.ToBoolean(TransferData["status"]))
        {
            if (txn_status == "1" || txn_status == "2" || txn_status == "5" || txn_status == "3")
            {
                Status = "Success";
                if (txn_status == "2" || txn_status == "5" || txn_status == "3")
                    Status = "Pending";
            }

            payoutCommonResponse.RRN = Convert.ToString(TransferData["utr"]);
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = message;
            payoutCommonResponse.VendorID = Convert.ToString(TransferData["ackno"]);
            payoutCommonResponse.TXNCode = txn_status;
            payoutCommonResponse.TXNO = commonSearch.TXNO;
            payoutCommonResponse.GUID = commonSearch.GUID;

            return payoutCommonResponse;
        }
        else
        {
            Status = "Failed";
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = message;
            return payoutCommonResponse;
        }

    }
    //Pipe Bank-1 Fino Transaction Send
    public async static Task<PayoutCommonResponse> FinoTransactionSendPaysprintDMT(CommonSendMoney commonSearch)
    {
        PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();


        object returnData = new
        {
            mobile = commonSearch.CustomerMobile,
            referenceid = commonSearch.OrderID,
            bene_id = commonSearch.BeniID,
            txntype = commonSearch.txntype,
            amount = commonSearch.Amount,
            otp = commonSearch.OTP,
            pincode = "302020",
            address = "Jaipur Mansraovar",
            gst_state = "17",
            dob = "1995-02-05",
            lat = commonSearch.latitude,
            stateresp = commonSearch.stateresp,
            @long = commonSearch.longitude
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        payoutCommonResponse.Request = request;
        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("dmt/kyc/transact/transact", jj.GetToken(), request);

        payoutCommonResponse.Result = Result;


        payoutCommonResponse.Request = request;
        payoutCommonResponse.Result = Result;
        payoutCommonResponse.AccountNo = commonSearch.AccNumber;
        payoutCommonResponse.IFSC = commonSearch.Isfc;
        payoutCommonResponse.Name = commonSearch.BeneName;
        payoutCommonResponse.OrderID = commonSearch.OrderID;
        payoutCommonResponse.Amount = commonSearch.Amount;

        JObject TransferData = JObject.Parse(Result);

        string txn_status = Convert.ToString(TransferData["txn_status"]).ToUpper();
        string Status = "Pending";
        string message = Convert.ToString(TransferData["message"]).Replace("\n", "");


        if (Convert.ToBoolean(TransferData["status"]))
        {
            if (txn_status == "1" || txn_status == "2" || txn_status == "5" || txn_status == "3")
            {
                Status = "Success";
                if (txn_status == "2" || txn_status == "5" || txn_status == "3")
                    Status = "Pending";
            }

            payoutCommonResponse.RRN = Convert.ToString(TransferData["utr"]);
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = message;
            payoutCommonResponse.VendorID = Convert.ToString(TransferData["ackno"]);
            payoutCommonResponse.TXNCode = txn_status;
            payoutCommonResponse.TXNO = commonSearch.TXNO;
            payoutCommonResponse.GUID = commonSearch.GUID;

            return payoutCommonResponse;
        }
        else
        {
            Status = "Failed";
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = message;
            return payoutCommonResponse;
        }

    }

    //Pipe Bank-4 Fino Transaction Send
    public async static Task<PayoutCommonResponse> PayoutTransaction(CommonSendMoney commonSearch)
    {
        PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();


        IPayoutService payoutService = new PayoutService();

        IServiceList serviceList = new ServiceList();
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonSearch.ServiceID);

        // Get member details
        IWallet wallet = new Wallet();
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);




        string Status = "Pending";
        if (commonSearch == null)
        {
            Status = "Failed";
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = "Invalid request.";
            return payoutCommonResponse;
        }

        var cls = new cls_connection();

        DataTable dtcus = await cls.select_data_dtAsync("CheckCustomerDMT '" + commonSearch.CustomerMobile + "'");

        if (dtcus.Rows.Count <= 0)
        {
            Status = "Failed";
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = "Remitter account is not registered. Please complete remitter registration before proceeding with money transfer.";
            return payoutCommonResponse;
        }

        DataTable dtbeni = await cls.select_data_dtAsync("ManageBeneficiaryDMT 'BeniID','" + commonSearch.BeniID + "'");

        if (dtbeni.Rows.Count <= 0)
        {
            Status = "Failed";
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = "Beneficiary account is not registered. Please complete Beneficiary registration before proceeding with money transfer.";
            return payoutCommonResponse;
        }

        PayoutCommon payoutCommon = new PayoutCommon();

        payoutCommon.Amount = Convert.ToDecimal(commonSearch.Amount);
        payoutCommon.OrderID = commonSearch.OrderID;
        payoutCommon.AccountID = commonSearch.BeniID;
        payoutCommon.Mobile = commonSearch.CustomerMobile;
        payoutCommon.BankName = commonSearch.BankName;
        payoutCommon.Name = commonSearch.BeneName;
        payoutCommon.AccountNO = commonSearch.AccNumber;
        payoutCommon.IFSC = commonSearch.Isfc;
        payoutCommon.Mode = commonSearch.txntype;
        payoutCommon.MailID = memberDetails.Email;

        if (serviceData.APIID == 1)
            payoutCommonResponse = await payoutService.FundTransferInstantWithSoniTechno(payoutCommon);
        else if (serviceData.APIID == 2)
            payoutCommonResponse = await payoutService.FundTransferInstantWithPromptPe(payoutCommon);
        else if (serviceData.APIID == 1034)
            payoutCommonResponse = await payoutService.FundTransferInstantWithPayDMR(payoutCommon);
        else if (serviceData.APIID == 3)
            payoutCommonResponse = await payoutService.FundTransferInstantWithBetaSource(payoutCommon);
        else if (serviceData.APIID ==4)
            payoutCommonResponse = await payoutService.FundTransferrechargkit(payoutCommon);
        else if (serviceData.APIID == 6)
            payoutCommonResponse = await payoutService.FundTransferclassypay(payoutCommon);


        return payoutCommonResponse;
    }


    public async static Task<PayoutCommonResponse> DMTPayoutTransaction(CommonSendMoney commonSearch)
    {
        PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();


        IPayoutService payoutService = new PayoutService();

        IServiceList serviceList = new ServiceList();
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonSearch.ServiceID);

        // Get member details
        IWallet wallet = new Wallet();
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);




        string Status = "Pending";
        if (commonSearch == null)
        {
            Status = "Failed";
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = "Invalid request.";
            return payoutCommonResponse;
        }

        var cls = new cls_connection();


        PayoutCommon payoutCommon = new PayoutCommon();

        payoutCommon.Amount = Convert.ToDecimal(commonSearch.Amount);
        payoutCommon.OrderID = commonSearch.OrderID;
        payoutCommon.AccountID = commonSearch.BeniID;
        payoutCommon.Mobile = commonSearch.CustomerMobile;
        payoutCommon.BankName = commonSearch.BankName;
        payoutCommon.Name = commonSearch.BeneName;
        payoutCommon.AccountNO = commonSearch.AccNumber;
        payoutCommon.IFSC = commonSearch.Isfc;
        payoutCommon.Mode = commonSearch.txntype;
        payoutCommon.MailID = memberDetails.Email;
        payoutCommon.BeneID = commonSearch.BeniID;

        if (serviceData.APIID == 1)
            payoutCommonResponse = await payoutService.FundTransferDMTSoniTechno(payoutCommon);

        return payoutCommonResponse;
    }

    public async static Task<PayoutCommonResponse> FinoTransactionSendSoniDMT(CommonSendMoney commonSearch)
    {
        PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();


        object returnData = new
        {
            Amount = commonSearch.Amount.ToString().ToInt32(),
            BeniID = commonSearch.BeniID,
            Mode = "IMPS",
            Pipe = commonSearch.Pipe,
            MethodName = "sendamount",
            OrderID = commonSearch.OrderID,
            AccountNO = commonSearch.AccNumber,
            IFSC = commonSearch.Isfc,
            BankName = commonSearch.BankName,
            Name = commonSearch.BeneName,
            mobile = commonSearch.CustomerMobile,
            stateresp = commonSearch.stateresp,
            OTP = commonSearch.OTP,
            pipe = "bank1",
            APIID = SONITechnoCredentrial.APIID,
            Token = SONITechnoCredentrial.Token
        };
        string request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);

        payoutCommonResponse.Request = request;
        string Result = await ApiPostCall.PostCallAsync(request, SONITechnoCredentrial.DMT);

        payoutCommonResponse.Result = Result;
        payoutCommonResponse.Request = request;
        payoutCommonResponse.Result = Result;
        payoutCommonResponse.AccountNo = commonSearch.AccNumber;
        payoutCommonResponse.IFSC = commonSearch.Isfc;
        payoutCommonResponse.Name = commonSearch.BeneName;
        payoutCommonResponse.OrderID = commonSearch.OrderID;
        payoutCommonResponse.Amount = commonSearch.Amount;

        JObject TransferData = JObject.Parse(Result);


        string Status = "Pending";
        string message = Convert.ToString(TransferData["mess"]).Replace("\n", "");


        if (Convert.ToString(TransferData["code"]) == "TXN")
        {
            string txn_status = Convert.ToString(TransferData["data"]["Status"]).ToUpper();

            if (txn_status == "SUCCESS" || txn_status == "PENDING")
            {
                Status = txn_status;
            }
            else
            {
                Status = "Failed";
            }

            payoutCommonResponse.RRN = Convert.ToString(TransferData["data"]["RRN"]);
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = message;
            payoutCommonResponse.VendorID = Convert.ToString(TransferData["data"]["STID"]);
            payoutCommonResponse.TXNCode = txn_status;
            payoutCommonResponse.TXNO = commonSearch.TXNO;
            payoutCommonResponse.GUID = commonSearch.GUID;

            return payoutCommonResponse;
        }
        else
        {
            Status = "Failed";
            payoutCommonResponse.Status = Status;
            payoutCommonResponse.Message = message;
            return payoutCommonResponse;
        }

    }

    #endregion


    #region [ Verification ]
    //Pipe Bank-4 Payout Bank Add Beni
    public static async Task<PayoutCommonResponse> SoniTechnoVerification(CommonDMTBeneficiaryVerification commonSearch)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        // Get member details
        IWallet wallet = new Wallet();
        var memberDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);


        OTPService oTPService = new OTPService();
        OTPToken oTPToken;
        string str = string.Empty;
        string[] ValueArray = new string[4];


        AccountVerifyST accountVerifySTS = new AccountVerifyST();
        accountVerifySTS.APIID = SONITechnoCredentrial.APIID;
        accountVerifySTS.Token = SONITechnoCredentrial.Token;
        accountVerifySTS.MethodName = "accountverify";
        accountVerifySTS.OrderID = commonSearch.OrderID;
        accountVerifySTS.number = commonSearch.AccountNo;
        accountVerifySTS.ifsc = commonSearch.IFSCCODE;

        Response.Request = accountVerifySTS.GetJson();
        Response.IsTPIN = true;
        Response.IsOTP = false;
        Response.stateresp = "";

        if (Response.IsOTP)
        {
            Response.Status = "Success";
            Response.Message = "OTP Process";
            oTPToken = oTPService.GenerateOTP(Convert.ToString(memberDetails.Msrno));
            ValueArray[0] = "Dear";
            ValueArray[1] = oTPToken.OTP;
            str = SMS.SendWithV(commonSearch.CustomerMobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, "1");
            Response.stateresp = oTPToken.Token;
        }

        if (Response.IsTPIN)
        {
            Response.Status = "Success";
            Response.Message = "TPIN Process";

            oTPToken = oTPService.GenerateOTPTpin(Convert.ToString(memberDetails.Msrno), memberDetails.LoginPin);
            Response.stateresp = oTPToken.Token;
        }

        if (Response.IsTPIN == false && Response.IsOTP == false)
        {
            IServiceList serviceList = new ServiceList();

            var serviceData = await serviceList.CheckServiceStatusWithAPIID(AccountVerificationService.ServiceID);
            if (serviceData.Status != "Success")
            {
                Response.Message = serviceData.Reason;
                Response.Status = "Failed";
                return Response;
            }


            IPayoutService payoutService = new PayoutService();


            PayoutCommon payoutCommon = new PayoutCommon();

            payoutCommon.Amount = 0;
            payoutCommon.OrderID = commonSearch.OrderID;
            payoutCommon.AccountID = commonSearch.AccountNo;
            payoutCommon.Mobile = commonSearch.CustomerMobile;
            payoutCommon.BankName = commonSearch.BankName;
            payoutCommon.Name = commonSearch.CusName;
            payoutCommon.AccountNO = commonSearch.AccountNo;
            payoutCommon.IFSC = commonSearch.IFSCCODE;
            payoutCommon.Mode = "IMPS";
            payoutCommon.MailID = "test@gmail.com";

            Response.IsTPIN = false;
            Response.IsOTP = false;
            Response.stateresp = "";

            if (serviceData.APIID == 1)
            {
                Response = await payoutService.SoniVerification(payoutCommon);
            }
            else if (serviceData.APIID == 6)
            {
                Response = await payoutService.ClassypayVerification(payoutCommon);
            }

        }
        return Response;

    }
    public static async Task<PayoutCommonResponse> SoniTechnoVerificationWithOTP(CommonDMTBeneficiaryVerificationWithOTP commonSearch)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        IServiceList serviceList = new ServiceList();

        var serviceData = await serviceList.CheckServiceStatusWithAPIID(AccountVerificationService.ServiceID);
        if (serviceData.Status != "Success")
        {
            Response.Message = serviceData.Reason;
            Response.Status = "Failed";
            return Response;
        }


        if (commonSearch == null)
        {
            Response.Message = "Invalid request.";
            Response.Status = "Failed";
            return Response;
        }

        var otpService = new OTPService();

        string txtOTP = HttpUtility.HtmlEncode(commonSearch.OTP);
        string tokenOTP = HttpUtility.HtmlEncode(commonSearch.stateresp);

        // Blocked check
        if (otpService.IsBlocked(tokenOTP))
        {
            Response.Message = "Too many attempts. Please try again later.";
            Response.Status = "Failed";
            return Response;
        }

        // OTP validation
        if (!otpService.ValidateOTP(tokenOTP, txtOTP))
        {
            otpService.IncrementAttempts(tokenOTP);
            Response.Message = "Invalid OTP.";
            Response.Status = "Failed";
            return Response;
        }


        IPayoutService payoutService = new PayoutService();
       

        PayoutCommon payoutCommon = new PayoutCommon();

        payoutCommon.Amount = 0;
        payoutCommon.OrderID = commonSearch.OrderID;
        payoutCommon.AccountID = commonSearch.AccountNo;
        payoutCommon.Mobile = commonSearch.CustomerMobile;
        payoutCommon.BankName = commonSearch.BankName;
        payoutCommon.Name = commonSearch.CusName;
        payoutCommon.AccountNO = commonSearch.AccountNo;
        payoutCommon.IFSC = commonSearch.IFSCCODE;
        payoutCommon.Mode = "IMPS";
        payoutCommon.MailID = "test@gmail.com";

        Response.IsTPIN = false;
        Response.IsOTP = false;
        Response.stateresp = "";

        if (serviceData.APIID == 1)
        {
            Response = await payoutService.SoniVerification(payoutCommon);
        }
        else if (serviceData.APIID == 6)
        {
            Response = await payoutService.ClassypayVerification(payoutCommon);
        }

        
        return Response;

    }
    #endregion
}