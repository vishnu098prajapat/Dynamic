using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

/// <summary>
/// Summary description for AEPSService
/// </summary>
public class AEPSService : IAEPSService
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    private static string ServiceID = "17";
    ICommissionService commissionService;
    ICommonService commonService;

    public AEPSService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commissionService = new CommissionService();
        commonService = new CommonService();

    }

    public async Task<string> GetWadhDetails(string Pipe, string Module)
    {
        DataTable dtpipe = await cls.select_data_dtAsync("select * from PipeMaster where PipeName='" + Pipe + "' and IsActive=1");
        if (dtpipe.Rows.Count > 0)
        {
            DataTable dtpipeData = await cls.select_data_dtAsync("select * from PipeModuleSettings where ModuleName='" + Module + "' and ServiceID='" + dtpipe.Rows[0]["ServiceID"].ToString() + "' and Pipe='" + dtpipe.Rows[0]["ID"].ToString() + "'");
            if (dtpipeData.Rows.Count > 0)
            {
                var aEPSResponseNew = new
                {
                    Module = dtpipeData.Rows[0]["ModuleName"].ToString(),
                    Pipe = Pipe,
                    WadhFace = dtpipeData.Rows[0]["WadhFace"].ToString(),
                    WadhFinger = dtpipeData.Rows[0]["WadhFinger"].ToString(),
                    WadhIris = dtpipeData.Rows[0]["WadhIris"].ToString(),
                    IsOTP = Convert.ToBoolean(dtpipeData.Rows[0]["IsOTP"]),
                    IsRequired = Convert.ToBoolean(dtpipeData.Rows[0]["IsRequired"]),
                    IsTPIN = Convert.ToBoolean(dtpipeData.Rows[0]["IsTPIN"])
                };
                string response = JsonConvert.SerializeObject(aEPSResponseNew);
                return ErrorShow.SuccessArrayAPI(response);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Pipe Module not assign");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Pipe Not Assign to any service");
        }
    }

    public async Task<string> BE(AEPSRequest aEPSRequest)
    {
        // Check Service Availability
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(aEPSRequest.ServiceID);
        if (serviceData?.Status != "Success")
            return ErrorShow.ErrorMessageAPI(serviceData?.Reason ?? "Service Unavailable");

        // Get User Permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        if (_memberAllowDetails == null)
            return ErrorShow.ErrorMessageAPI("User Not Found");


        // Check if user is allowed
        if (_memberAllowDetails.HoldUnHold)
            return ErrorShow.ErrorMessageAPI("Not Allowed, Contact Admin");


        // Generate Reference ID
        string refID = await wallet.GetOrderID();
        aEPSRequest.BCCode = _memberAllowDetails.LoginID;
        aEPSRequest.BCName = _memberAllowDetails.ShopName;
        aEPSRequest.OrderID = refID;



        var aEPSResponse = new AEPSResponseAPI();

        // Validate AEPS Status
        DataTable dtValc = await GetAEPSStatus(aEPSRequest.BankType, _memberAllowDetails.Msrno.ToString());
        if (dtValc.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Outlet Not Found");


        aEPSRequest.AEPSID = dtValc.Rows[0]["aepsid"].ToString();
        aEPSRequest.AEPSPin = "1234";
        if (Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString()) > 0)
            aEPSRequest.Latitude = Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString());

        if (Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString()) > 0)
            aEPSRequest.Longitude = Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString());

        if (aEPSRequest.TType == "CD")
        {
            if (_memberAllowDetails.MainBalance < (aEPSRequest.Amount + _memberAllowDetails.HoldAMT))
            {
                return ErrorShow.ErrorMessageAPI("Low Balance");
            }
        }



        // Mask Aadhaar Number
        string maskedAadhaar = $"xxxx-xxxx-{aEPSRequest.Adhaarnumber?.Substring(8, 4)}";

        Int32 Val = await cls.update_dataAsync("Exec Proc_Add_edit_AEPS " + _memberAllowDetails.Msrno + ", '" + refID + "','" + aEPSRequest.Amount + "', '" + aEPSRequest.TType + "', " + aEPSRequest.BankNo + ",'" + maskedAadhaar + "', '" + aEPSRequest.MobileNO + "', 'Pending','','" + aEPSRequest.DeviceName + "','','','" + aEPSRequest.Source + "','','','" + aEPSRequest.IP + "','" + serviceData.APIID + "','" + aEPSRequest.BankName + "','','" + aEPSRequest.ServiceID + "'");
        if (Val <= 0)
            return ErrorShow.ErrorMessageAPI("Try After Sometime");


        if (aEPSRequest.TType == "BE")
            aEPSResponse = await DOBESoniTechno(aEPSRequest);
        else if (aEPSRequest.TType == "MS")
            aEPSResponse = await DOMSSoniTechno(aEPSRequest);
        else if (aEPSRequest.TType == "CW")
            aEPSResponse = await DOCWSoniTechno(aEPSRequest);
        else if (aEPSRequest.TType == "CD")
            aEPSResponse = await DOCDSoniTechno(aEPSRequest);
        else if (aEPSRequest.TType == "AW")
            aEPSResponse = await DOAWSoniTechno(aEPSRequest);

        // Log Transaction
        try
        {
            await commonService.SaveLogs(new LogsRequestModel()
            {
                ServiceID = aEPSRequest.ServiceID.ToInt32(),
                APIID = serviceData.APIID,
                TransID = aEPSRequest.OrderID,
                RequestData = aEPSResponse?.Request.Replace("'", "") ?? string.Empty,
                ResponseData = aEPSResponse?.Response.Replace("'", "") ?? string.Empty,
                CallBackData = ""
            });
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Logging Error: {logEx.Message}");
        }


        // Handle AEPS Response
        Val = await cls.update_dataAsync($@"
            EXEC proc_Credit_AEPS_History 
            '{aEPSRequest.OrderID}', '{_memberAllowDetails.Msrno}', '{aEPSRequest.TType}', 
            '{aEPSResponse?.StatusM ?? "Failed"}', '{aEPSResponse?.Message ?? "Error"}', 0, 
            '{aEPSResponse?.BankRRN ?? ""}', '{aEPSResponse?.VendorID ?? ""}', ''
        ");


        if (aEPSResponse.Status && aEPSResponse.TType == "CW")
        {
            string narration = "", Decs = "";
            narration = $"{aEPSResponse.TType}/{aEPSRequest.BankType}/{aEPSRequest.OrderID}/{maskedAadhaar}/{aEPSRequest.Amount}";
            Decs = $"AEPS Amount - {aEPSRequest.Amount}. Done With Aadhar No {maskedAadhaar}. TransID - {aEPSRequest.OrderID}";
            cls.update_data("exec PROC_AEPSWALLETTRANSACTION '" + aEPSRequest.UserID + "','" + aEPSRequest.Amount + "','CR','" + narration + "','" + Decs + "','" + aEPSRequest.ServiceID + "','" + aEPSRequest.OrderID + "'");
            if (aEPSResponse.TType == "CW" || aEPSResponse.TType == "MS")
            {
                Int32 vl = await commissionService.GiveAEPSCommission(aEPSRequest.OrderID);
            }
        }

        if (aEPSResponse.Status && aEPSResponse.TType == "AW")
        {
            string narration = "", Decs = "";
            narration = $"{aEPSResponse.TType}/{aEPSRequest.BankType}/{aEPSRequest.OrderID}/{maskedAadhaar}/{aEPSRequest.Amount}";
            Decs = $"AadharPay Amount - {aEPSRequest.Amount}. Done With Aadhar No {maskedAadhaar}. TransID - {aEPSRequest.OrderID}";
            decimal surcharge = cls.select_data_scalar_decimal("select dbo.GetAadharSurcharge(" + aEPSRequest.Amount + "," + _memberAllowDetails.PackgeID + ")");

            cls.update_data("exec PROC_AEPSWALLETTRANSACTION '" + aEPSRequest.UserID + "','" + aEPSRequest.Amount + "','CR','" + narration + "','" + Decs + "','" + aEPSRequest.ServiceID + "','" + aEPSRequest.OrderID + "',0,0,0," + surcharge + "");
        }

        if (aEPSResponse.Status && aEPSResponse.TType == "CD")
        {
            string narration = "", Decs = "";
            narration = $"{aEPSResponse.TType}/{aEPSRequest.BankType}/{aEPSRequest.OrderID}/{maskedAadhaar}/{aEPSRequest.Amount}";
            Decs = $"CashDeposit Amount - {aEPSRequest.Amount}. Done With Aadhar No {maskedAadhaar}. TransID - {aEPSRequest.OrderID}";

            cls.update_data("exec PROC_EWALLETTRANSACTION '" + aEPSRequest.UserID + "','" + aEPSRequest.Amount + "','DR','" + narration + "','" + Decs + "','" + aEPSRequest.ServiceID + "','" + aEPSRequest.OrderID + "',0,0,0,0");
        }


        var aEPSResponseNew = new AEPSResponseAPI
        {
            Status = aEPSResponse.Status,
            StatusM = aEPSResponse.StatusM,
            Message = aEPSResponse.Message,
            BCCode = aEPSResponse.BCCode,
            BCName = aEPSResponse.BCName,
            MobileNO = aEPSResponse.MobileNO,
            Adhaarnumber = aEPSResponse.Adhaarnumber,
            BankName = aEPSResponse.BankName,
            DeviceName = aEPSResponse.DeviceName,
            BalanceAmount = aEPSResponse.BalanceAmount,
            BankRRN = aEPSResponse.BankRRN,
            OrderID = aEPSResponse.OrderID,
            VendorID = aEPSResponse.VendorID,
            TType = aEPSResponse.TType,
            Title = aEPSResponse.Title,
            IP = aEPSResponse.IP,
            Latitude = aEPSResponse.Latitude,
            Longitude = aEPSResponse.Longitude,
            TransDate = aEPSResponse.TransDate,
            Amount = aEPSResponse.Amount,
            ministatement = aEPSResponse.ministatement
        };
        string response = JsonConvert.SerializeObject(aEPSResponseNew);
        return aEPSResponse.Status ? ErrorShow.SuccessArrayAPI(response) : ErrorShow.ErrorMessageAPI(aEPSResponse.Message);
    }

    public async Task<string> SENDOTPAEPS(AEPSRequest aEPSRequest)
    {
        // Check Service Availability
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(aEPSRequest.ServiceID);
        if (serviceData?.Status != "Success")
            return ErrorShow.ErrorMessageAPI(serviceData?.Reason ?? "Service Unavailable");

        // Get User Permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        if (_memberAllowDetails == null)
            return ErrorShow.ErrorMessageAPI("User Not Found");


        // Check if user is allowed
        if (_memberAllowDetails.HoldUnHold)
            return ErrorShow.ErrorMessageAPI("Not Allowed, Contact Admin");


        // Generate Reference ID
        string refID = await wallet.GetOrderID();
        aEPSRequest.BCCode = _memberAllowDetails.LoginID;
        aEPSRequest.BCName = _memberAllowDetails.ShopName;
        aEPSRequest.OrderID = refID;



        var aEPSResponse = new AEPSResponseAPI();

        // Validate AEPS Status
        DataTable dtValc = await GetAEPSStatus(aEPSRequest.BankType, _memberAllowDetails.Msrno.ToString());
        if (dtValc.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Outlet Not Found");


        aEPSRequest.AEPSID = dtValc.Rows[0]["aepsid"].ToString();
        aEPSRequest.AEPSPin = "1234";
        if (Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString()) > 0)
            aEPSRequest.Latitude = Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString());

        if (Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString()) > 0)
            aEPSRequest.Longitude = Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString());


        // Mask Aadhaar Number
        string maskedAadhaar = $"xxxx-xxxx-{aEPSRequest.Adhaarnumber?.Substring(8, 4)}";


        if (aEPSRequest.TType == "SendOTPCW")
            aEPSResponse = await DOOTPCWSoniTechno(aEPSRequest);


        // Log Transaction
        try
        {
            await commonService.SaveLogs(new LogsRequestModel()
            {
                ServiceID = aEPSRequest.ServiceID.ToInt32(),
                APIID = serviceData.APIID,
                TransID = aEPSRequest.OrderID,
                RequestData = aEPSResponse?.Request.Replace("'", "") ?? string.Empty,
                ResponseData = aEPSResponse?.Response.Replace("'", "") ?? string.Empty,
                CallBackData = ""
            });
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Logging Error: {logEx.Message}");
        }



        var aEPSResponseNew = new AEPSResponseAPI
        {
            Status = aEPSResponse.Status,
            TransactionId = aEPSResponse.TransactionId,
            UidaiDataTxn = aEPSResponse.UidaiDataTxn,
            txnReferenceNo = aEPSResponse.txnReferenceNo,
            requestid = aEPSResponse.requestid,
            authRequestId = aEPSResponse.authRequestId,

            Message = aEPSResponse.Message

        };
        string response = JsonConvert.SerializeObject(aEPSResponseNew);
        return aEPSResponse.Status ? ErrorShow.SuccessArrayAPI(response) : ErrorShow.ErrorMessageAPI(aEPSResponse.Message);
    }

    public async Task<string> Authentication(AuthencationRequest aEPSRequest)
    {
        // Check Service Availability
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(aEPSRequest.ServiceID);
        if (serviceData?.Status != "Success")
            return ErrorShow.ErrorMessageAPI(serviceData?.Reason ?? "Service Unavailable");

        // Get User Permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        if (_memberAllowDetails == null)
            return ErrorShow.ErrorMessageAPI("User Not Found");


        // Check if user is allowed
        if (_memberAllowDetails.HoldUnHold)
            return ErrorShow.ErrorMessageAPI("Not Allowed, Contact Admin");


        // Generate Reference ID
        string refID = await wallet.GetOrderID();
        aEPSRequest.OrderID = refID;
        aEPSRequest.adhaarnumber = _memberAllowDetails.AadharNo;



        var aEPSResponse = new AuthencationResponse();

        // Validate AEPS Status
        DataTable dtValc = await GetAEPSStatus(aEPSRequest.pipe, _memberAllowDetails.Msrno.ToString());
        if (dtValc.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Outlet Not Found");


        aEPSRequest.AEPSID = dtValc.Rows[0]["aepsid"].ToString();
        aEPSRequest.MobileNO = dtValc.Rows[0]["mobile"].ToString();
        aEPSRequest.AEPSPin = "1234";

        if (Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString()) > 0)
            aEPSRequest.latitude = (dtValc.Rows[0]["Latitude"].ToString());

        if (Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString()) > 0)
            aEPSRequest.longitude = (dtValc.Rows[0]["Longitude"].ToString());

        // Mask Aadhaar Number
        string maskedAadhaar = $"xxxx-xxxx-{_memberAllowDetails.AadharNo?.Substring(8, 4)}";



        Int32 Val = await cls.update_dataAsync("Exec Proc_Add_edit_AEPS " + _memberAllowDetails.Msrno + ", '" + refID + "',0, 'MAEPSF', 0,'" + maskedAadhaar + "', '" + aEPSRequest.MobileNO + "', 'Pending','','" + aEPSRequest.DeviceName + "','','','" + aEPSRequest.Source + "','','','" + aEPSRequest.ipaddress + "','" + serviceData.APIID + "','Authentication','','" + aEPSRequest.ServiceID + "'");
        if (Val <= 0)
            return ErrorShow.ErrorMessageAPI("Try After Sometime");


        if (aEPSRequest.TType == "MAEPSF")
            aEPSResponse = await AuthenticateSoni(aEPSRequest);



        // Log Transaction
        try
        {
            await commonService.SaveLogs(new LogsRequestModel()
            {
                ServiceID = aEPSRequest.ServiceID.ToInt32(),
                APIID = serviceData.APIID,
                TransID = aEPSRequest.OrderID,
                RequestData = aEPSResponse?.Request ?? string.Empty,
                ResponseData = aEPSResponse?.Response ?? string.Empty,
                CallBackData = ""
            });
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Logging Error: {logEx.Message}");
        }


        // Handle AEPS Response
        Val = await cls.update_dataAsync($@"
            EXEC proc_Credit_AEPS_History 
            '{aEPSRequest.OrderID}', '{_memberAllowDetails.Msrno}', '{aEPSRequest.TType}', 
            '{aEPSResponse?.StatusM ?? "Failed"}', '{aEPSResponse?.Message ?? "Error"}', 0, 
            '', '', ''
        ");


        if (aEPSResponse.Status && aEPSRequest.TType == "MAEPSF")
        {
            await cls.update_dataAsync("update AEPSOnBoardStatus set DailyAuthentication=1,AuthDate=getdate() where msrno='" + _memberAllowDetails.Msrno + "' and Name='" + aEPSRequest.pipe + "'");
        }


        var aEPSResponseNew = new AuthencationResponse
        {
            Status = aEPSResponse.Status,
            StatusM = aEPSResponse.StatusM,
            Message = aEPSResponse.Message,
        };
        string response = JsonConvert.SerializeObject(aEPSResponseNew);
        return aEPSResponse.Status ? ErrorShow.SuccessArrayAPI(response) : ErrorShow.ErrorMessageAPI(aEPSResponse.Message);

    }

    public async Task<string> Registration(AuthencationRequest aEPSRequest)
    {
        string sId = aEPSRequest.ServiceID;

        // Check Service Availability
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(aEPSRequest.ServiceID);
        if (serviceData?.Status != "Success")
            return ErrorShow.ErrorMessageAPI(serviceData?.Reason ?? "Service Unavailable");


        // Get User Permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        if (_memberAllowDetails == null)
            return ErrorShow.ErrorMessageAPI("User Not Found");

        // Check if user is allowed
        if (_memberAllowDetails.HoldUnHold)
            return ErrorShow.ErrorMessageAPI("Not Allowed, Contact Admin");


        // Generate Reference ID
        string refID = await wallet.GetOrderID();
        aEPSRequest.OrderID = refID;
        aEPSRequest.adhaarnumber = _memberAllowDetails.AadharNo;



        var aEPSResponse = new AuthencationResponse();

        // Validate AEPS Status
        DataTable dtValc = await GetAEPSStatus(aEPSRequest.pipe, _memberAllowDetails.Msrno.ToString());
        if (dtValc.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Outlet Not Found");


        aEPSRequest.AEPSID = dtValc.Rows[0]["aepsid"].ToString();
        aEPSRequest.MobileNO = dtValc.Rows[0]["mobile"].ToString();
        aEPSRequest.AEPSPin = dtValc.Rows[0]["Pin"].ToString();

        if (Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString()) > 0)
            aEPSRequest.latitude = (dtValc.Rows[0]["Latitude"].ToString());

        if (Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString()) > 0)
            aEPSRequest.longitude = (dtValc.Rows[0]["Longitude"].ToString());

        // Mask Aadhaar Number
        string maskedAadhaar = $"xxxx-xxxx-{_memberAllowDetails.AadharNo?.Substring(8, 4)}";


        aEPSRequest.ServiceType = "AEPS";



        Int32 Val = await cls.update_dataAsync("Exec Proc_Add_edit_AEPS " + _memberAllowDetails.Msrno + ", '" + refID + "',0, '" + aEPSRequest.TType + "', 0,'" + maskedAadhaar + "', '" + aEPSRequest.MobileNO + "', 'Pending','','" + aEPSRequest.DeviceName + "','','','" + aEPSRequest.Source + "','','','" + aEPSRequest.ipaddress + "','" + serviceData.APIID + "','Authentication','','" + AEPSService.ServiceID + "','" + _memberAllowDetails.MainBalance + "','" + (_memberAllowDetails.MainBalance) + "'");
        if (Val <= 0)
            return ErrorShow.ErrorMessageAPI("Try After Sometime");

        //if (serviceData.APIID == 1014)
        //{
        //    aEPSResponse = await FingpayAuthentication(aEPSRequest);
        //}
        //else if (serviceData.APIID == 1010)
        //{
        //    aEPSResponse = await AuthenticateEzyPay(aEPSRequest);
        //}
        //else if (serviceData.APIID == 1012)
        //{
        //    aEPSResponse = await RegistrationPaysprint(aEPSRequest);
        //}

        // Log Transaction
        try
        {
            await commonService.SaveLogs(new LogsRequestModel()
            {
                ServiceID = sId.ToInt32(),
                APIID = serviceData.APIID,
                TransID = aEPSRequest.OrderID,
                RequestData = aEPSResponse?.Request ?? string.Empty,
                ResponseData = aEPSResponse?.Response ?? string.Empty,
                CallBackData = ""
            });
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Logging Error: {logEx.Message}");
        }

        // Handle AEPS Response
        Val = await cls.update_dataAsync($@"
            EXEC proc_Credit_AEPS_History 
            '{aEPSRequest.OrderID}', '{_memberAllowDetails.Msrno}', '{aEPSRequest.TType}', 
            '{aEPSResponse?.StatusM ?? "Failed"}', '{aEPSResponse?.Message ?? "Error"}', 0, 
            '', '', ''
        ");


        if (aEPSResponse.Status && aEPSRequest.TType == "2FAR")
        {
            cls.update_data("update AEPSOnBoardStatus set AEPSTwoWayRegistration=1 where msrno='" + _memberAllowDetails.Msrno + "' and Name='" + aEPSRequest.pipe + "'");
        }



        var aEPSResponseNew = new AuthencationResponse
        {
            Status = aEPSResponse.Status,
            StatusM = aEPSResponse.StatusM,
            Message = aEPSResponse.Message,
        };
        string response = JsonConvert.SerializeObject(aEPSResponseNew);
        return aEPSResponse.Status ? ErrorShow.SuccessArrayAPI(response) : ErrorShow.ErrorMessageAPI(aEPSResponse.Message);

    }

    public async Task<string> EKYC(AuthencationRequest aEPSRequest)
    {

        string sId = aEPSRequest.ServiceID;

        // Check Service Availability
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(sId);
        if (serviceData?.Status != "Success")
            return ErrorShow.ErrorMessageAPI(serviceData?.Reason ?? "Service Unavailable");



        // Get User Permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        if (_memberAllowDetails == null)
            return ErrorShow.ErrorMessageAPI("User Not Found");


        // Check if user is allowed
        if (_memberAllowDetails.HoldUnHold)
            return ErrorShow.ErrorMessageAPI("Not Allowed, Contact Admin");


        // Generate Reference ID
        string refID = await wallet.GetOrderID();
        aEPSRequest.OrderID = refID;
        aEPSRequest.adhaarnumber = _memberAllowDetails.AadharNo;



        var aEPSResponse = new AuthencationResponse();

        // Validate AEPS Status
        DataTable dtValc = await GetAEPSStatus(aEPSRequest.pipe, _memberAllowDetails.Msrno.ToString());
        if (dtValc.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Outlet Not Found");


        aEPSRequest.AEPSID = dtValc.Rows[0]["aepsid"].ToString();
        aEPSRequest.MobileNO = dtValc.Rows[0]["mobile"].ToString();
        aEPSRequest.AEPSPin = dtValc.Rows[0]["Pin"].ToString();
        aEPSRequest.DOB = _memberAllowDetails.DOB;

        if (Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString()) > 0)
            aEPSRequest.latitude = (dtValc.Rows[0]["Latitude"].ToString());

        if (Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString()) > 0)
            aEPSRequest.longitude = (dtValc.Rows[0]["Longitude"].ToString());

        // Mask Aadhaar Number
        string maskedAadhaar = $"xxxx-xxxx-{_memberAllowDetails.AadharNo?.Substring(8, 4)}";

        aEPSRequest.ServiceType = "AEPS";



        Int32 Val = await cls.update_dataAsync("Exec Proc_Add_edit_AEPS " + _memberAllowDetails.Msrno + ", '" + refID + "',0, '" + aEPSRequest.TType + "', 0,'" + maskedAadhaar + "', '" + aEPSRequest.MobileNO + "', 'Pending','','" + aEPSRequest.DeviceName + "','','','" + aEPSRequest.Source + "','','','" + aEPSRequest.ipaddress + "','" + serviceData.APIID + "','Authentication','','" + AEPSService.ServiceID + "','" + _memberAllowDetails.MainBalance + "','" + _memberAllowDetails.MainBalance + "'");
        if (Val <= 0)
            return ErrorShow.ErrorMessageAPI("Try After Sometime");


        if (aEPSRequest.pipe == "bank8" || aEPSRequest.pipe == "bank9")
            aEPSResponse = await EKYCSoni(aEPSRequest);
        else if (aEPSRequest.pipe == "bank7")
            aEPSResponse = await EKYCSoniBank7(aEPSRequest);

        // Log Transaction
        try
        {
            await commonService.SaveLogs(new LogsRequestModel()
            {
                ServiceID = aEPSRequest.ServiceID.ToInt32(),
                APIID = serviceData.APIID,
                TransID = aEPSRequest.OrderID,
                RequestData = aEPSResponse?.Request ?? string.Empty,
                ResponseData = aEPSResponse?.Response ?? string.Empty,
                CallBackData = ""
            });
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Logging Error: {logEx.Message}");
        }

        // Handle AEPS Response
        Val = await cls.update_dataAsync($@"
            EXEC proc_Credit_AEPS_History 
            '{aEPSRequest.OrderID}', '{_memberAllowDetails.Msrno}', '{aEPSRequest.TType}', 
            '{aEPSResponse?.StatusM ?? "Failed"}', '{aEPSResponse?.Message ?? "Error"}', 0, 
            '', '', ''
            ");



        if (aEPSResponse.Status && aEPSRequest.TType == "EKYC")
        {
            cls.update_data("update AEPSOnBoardStatus set DoKyc=1 where msrno='" + _memberAllowDetails.Msrno + "' and Name='" + aEPSRequest.pipe + "'");
        }



        var aEPSResponseNew = new AuthencationResponse
        {
            Status = aEPSResponse.Status,
            StatusM = aEPSResponse.StatusM,
            Message = aEPSResponse.Message,
            Optional1 = aEPSResponse.Optional1,
            Optional2 = aEPSResponse.Optional2,
        };
        string response = JsonConvert.SerializeObject(aEPSResponseNew);
        return aEPSResponse.Status ? ErrorShow.SuccessArrayAPI(response) : ErrorShow.ErrorMessageAPI(aEPSResponse.Message);

    }

    public async Task<string> SENDOTPEKYC(AuthencationRequest aEPSRequest)
    {

        string sId = aEPSRequest.ServiceID;

        // Check Service Availability
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(sId);
        if (serviceData?.Status != "Success")
            return ErrorShow.ErrorMessageAPI(serviceData?.Reason ?? "Service Unavailable");



        // Get User Permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        if (_memberAllowDetails == null)
            return ErrorShow.ErrorMessageAPI("User Not Found");


        // Check if user is allowed
        if (_memberAllowDetails.HoldUnHold)
            return ErrorShow.ErrorMessageAPI("Not Allowed, Contact Admin");


        // Generate Reference ID
        string refID = await wallet.GetOrderID();
        aEPSRequest.OrderID = refID;
        aEPSRequest.adhaarnumber = _memberAllowDetails.AadharNo;



        var aEPSResponse = new AuthencationResponse();

        // Validate AEPS Status
        DataTable dtValc = await GetAEPSStatus(aEPSRequest.pipe, _memberAllowDetails.Msrno.ToString());
        if (dtValc.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Outlet Not Found");


        aEPSRequest.AEPSID = dtValc.Rows[0]["aepsid"].ToString();
        aEPSRequest.MobileNO = dtValc.Rows[0]["mobile"].ToString();
        aEPSRequest.AEPSPin = dtValc.Rows[0]["Pin"].ToString();
        aEPSRequest.DOB = _memberAllowDetails.DOB;

        if (Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString()) > 0)
            aEPSRequest.latitude = (dtValc.Rows[0]["Latitude"].ToString());

        if (Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString()) > 0)
            aEPSRequest.longitude = (dtValc.Rows[0]["Longitude"].ToString());

        // Mask Aadhaar Number
        string maskedAadhaar = $"xxxx-xxxx-{_memberAllowDetails.AadharNo?.Substring(8, 4)}";

        aEPSRequest.ServiceType = "AEPS";


        if (aEPSRequest.pipe == "bank7")
            aEPSResponse = await finootp(aEPSRequest);



        var aEPSResponseNew = new AuthencationResponse
        {
            Status = aEPSResponse.Status,
            StatusM = aEPSResponse.StatusM,
            Message = aEPSResponse.Message,
            Optional1 = aEPSResponse.Optional1,
            Optional2 = aEPSResponse.Optional2,
        };
        string response = JsonConvert.SerializeObject(aEPSResponseNew);
        return aEPSResponse.Status ? ErrorShow.SuccessArrayAPI(response) : ErrorShow.ErrorMessageAPI(aEPSResponse.Message);

    }

    public async Task<string> VerifyOTPEKYC(AuthencationRequest aEPSRequest)
    {

        string sId = aEPSRequest.ServiceID;

        // Check Service Availability
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(sId);
        if (serviceData?.Status != "Success")
            return ErrorShow.ErrorMessageAPI(serviceData?.Reason ?? "Service Unavailable");



        // Get User Permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        if (_memberAllowDetails == null)
            return ErrorShow.ErrorMessageAPI("User Not Found");


        // Check if user is allowed
        if (_memberAllowDetails.HoldUnHold)
            return ErrorShow.ErrorMessageAPI("Not Allowed, Contact Admin");


        // Generate Reference ID
        string refID = await wallet.GetOrderID();
        aEPSRequest.OrderID = refID;
        aEPSRequest.adhaarnumber = _memberAllowDetails.AadharNo;



        var aEPSResponse = new AuthencationResponse();

        // Validate AEPS Status
        DataTable dtValc = await GetAEPSStatus(aEPSRequest.pipe, _memberAllowDetails.Msrno.ToString());
        if (dtValc.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Outlet Not Found");


        aEPSRequest.AEPSID = dtValc.Rows[0]["aepsid"].ToString();
        aEPSRequest.MobileNO = dtValc.Rows[0]["mobile"].ToString();
        aEPSRequest.AEPSPin = dtValc.Rows[0]["Pin"].ToString();
        aEPSRequest.DOB = _memberAllowDetails.DOB;

        if (Convert.ToDouble(dtValc.Rows[0]["Latitude"].ToString()) > 0)
            aEPSRequest.latitude = (dtValc.Rows[0]["Latitude"].ToString());

        if (Convert.ToDouble(dtValc.Rows[0]["Longitude"].ToString()) > 0)
            aEPSRequest.longitude = (dtValc.Rows[0]["Longitude"].ToString());

        // Mask Aadhaar Number
        string maskedAadhaar = $"xxxx-xxxx-{_memberAllowDetails.AadharNo?.Substring(8, 4)}";

        aEPSRequest.ServiceType = "AEPS";


        if (aEPSRequest.pipe == "bank7")
            aEPSResponse = await finootpverify(aEPSRequest);



        var aEPSResponseNew = new AuthencationResponse
        {
            Status = aEPSResponse.Status,
            StatusM = aEPSResponse.StatusM,
            Message = aEPSResponse.Message,
            Optional1 = aEPSResponse.Optional1,
            Optional2 = aEPSResponse.Optional2,
        };
        string response = JsonConvert.SerializeObject(aEPSResponseNew);
        return aEPSResponse.Status ? ErrorShow.SuccessArrayAPI(response) : ErrorShow.ErrorMessageAPI(aEPSResponse.Message);

    }

    public async Task<AuthencationResponse> finootp(AuthencationRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AuthencationResponse();

        AEPSSendOTP finoSendOTP = new AEPSSendOTP();
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(aEPSRequest.UserID);
        finoSendOTP.merchantLoginId = aEPSRequest.AEPSID;

        finoSendOTP.mobileNumber = aEPSRequest.MobileNO;
        finoSendOTP.aadharNumber = aEPSRequest.adhaarnumber;
        finoSendOTP.panNumber = _memberAllowDetails.Pan;
        finoSendOTP.longitude = Convert.ToDouble(aEPSRequest.longitude);
        finoSendOTP.latitude = Convert.ToDouble(aEPSRequest.latitude);
        finoSendOTP.APIID = SONITechnoCredentrial.APIID;
        finoSendOTP.Token = SONITechnoCredentrial.Token;
        finoSendOTP.MethodName = "sendotp";
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(finoSendOTP);

        string Result = ApiPostCall.PostCall(RequestJson, SONITechnoCredentrial.AEPSCorePaytm);
        JObject obj = JObject.Parse(Result);
        if (Convert.ToString(obj["code"]) == "TXN")
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = true,
                StatusM = "Success",
                Message = obj["mess"].ToString(),
                Request = RequestJson,
                Response = Result,
                Optional1 = Convert.ToString(obj["data"]["encodeFPTxnId"]),
                Optional2 = Convert.ToString(obj["data"]["primaryKeyId"]),
            };
        }
        else
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = false,
                StatusM = "Failed",
                Message = obj["mess"].ToString(),
                Request = RequestJson,
                Response = Result,
                Optional1 = "",
                Optional2 = "",
            };
        }

        return aEPSResponseAPI;
    }

    public async Task<AuthencationResponse> finootpverify(AuthencationRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AuthencationResponse();

        AEPSverifyOTP finoVerifyOTP = new AEPSverifyOTP();
        finoVerifyOTP.merchantLoginId = aEPSRequest.AEPSID;

        finoVerifyOTP.otp = aEPSRequest.OTP;
        finoVerifyOTP.primaryKeyId = aEPSRequest.Optional2;
        finoVerifyOTP.encodeFPTxnId = aEPSRequest.Optional1;
        finoVerifyOTP.APIID = SONITechnoCredentrial.APIID;
        finoVerifyOTP.Token = SONITechnoCredentrial.Token;
        finoVerifyOTP.MethodName = "verifydotp";

        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(finoVerifyOTP);

        string Result = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.AEPSCorePaytm);
        JObject obj = JObject.Parse(Result);
        if (Convert.ToString(obj["code"]) == "TXN")
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = true,
                StatusM = "Success",
                Message = obj["mess"].ToString(),
                Request = RequestJson,
                Response = Result,
                Optional1 = aEPSRequest.Optional1,
                Optional2 = aEPSRequest.Optional2,
            };
        }
        else
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = false,
                StatusM = "Failed",
                Message = obj["mess"].ToString(),
                Request = RequestJson,
                Response = Result,
                Optional1 = "",
                Optional2 = "",
            };
        }

        return aEPSResponseAPI;
    }

    public async Task<AuthencationResponse> AuthenticateSoni(AuthencationRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AuthencationResponse();
        var tType = aEPSRequest.pipe?.ToLower();

        AEPSAuthentication aEPSAuthentication = new AEPSAuthentication();
        aEPSAuthentication.APIID = SONITechnoCredentrial.APIID;
        aEPSAuthentication.Token = SONITechnoCredentrial.Token;

        if (tType == "bank8")
            aEPSAuthentication.MethodName = "authencation1";
        else if (tType == "bank9")
            aEPSAuthentication.MethodName = "authencation2";
        else
            aEPSAuthentication.MethodName = "authencation";

        aEPSAuthentication.isFacialTan = false;
        aEPSAuthentication.OrderID = aEPSRequest.OrderID;
        aEPSAuthentication.merchantLoginId = aEPSRequest.AEPSID;
        aEPSAuthentication.mobileNumber = aEPSRequest.MobileNO;
        aEPSAuthentication.nationalBankIdentificationNumber = "508534";
        aEPSAuthentication.indicatorforUID = "0";
        aEPSAuthentication.adhaarNumber = aEPSRequest.adhaarnumber;
        aEPSAuthentication.devicename = aEPSRequest.DeviceName;
        aEPSAuthentication.capture = aEPSRequest.Data;
        aEPSAuthentication.IP = aEPSRequest.ipaddress;
        aEPSAuthentication.Source = "App";
        aEPSAuthentication.latitude = Convert.ToDouble(aEPSRequest.latitude);
        aEPSAuthentication.longitude = Convert.ToDouble(aEPSRequest.longitude);

        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(aEPSAuthentication);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);

        JObject Data = JObject.Parse(Result);
        if (Data["code"].ToString().ToUpper() == "TXN")
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = true,
                StatusM = "Success",
                Message = Data["mess"].ToString(),
                Request = Request,
                Response = Result,
            };
        }
        else
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = false,
                StatusM = "Failed",
                Message = Data["mess"].ToString(),
                Request = Request,
                Response = Result,
            };
        }

        return aEPSResponseAPI;
    }

    public async Task<AuthencationResponse> EKYCSoni(AuthencationRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AuthencationResponse();
        var tType = aEPSRequest.pipe?.ToLower();


        AEPSAuthentication aEPSAuthentication = new AEPSAuthentication();
        aEPSAuthentication.APIID = SONITechnoCredentrial.APIID;
        aEPSAuthentication.Token = SONITechnoCredentrial.Token;
        if (tType == "bank8")
            aEPSAuthentication.MethodName = "ekyc1";
        else if (tType == "bank9")
            aEPSAuthentication.MethodName = "ekycfino";
        else
            aEPSAuthentication.MethodName = "ekyc";

        aEPSAuthentication.isFacialTan = false;
        aEPSAuthentication.OrderID = aEPSRequest.OrderID;
        aEPSAuthentication.merchantLoginId = aEPSRequest.AEPSID;
        aEPSAuthentication.mobileNumber = aEPSRequest.MobileNO;
        aEPSAuthentication.nationalBankIdentificationNumber = "508534";
        aEPSAuthentication.indicatorforUID = aEPSRequest.DOB;
        aEPSAuthentication.adhaarNumber = aEPSRequest.adhaarnumber;
        aEPSAuthentication.devicename = aEPSRequest.DeviceName;
        aEPSAuthentication.capture = aEPSRequest.Data;
        aEPSAuthentication.IP = aEPSRequest.ipaddress;
        aEPSAuthentication.Source = "App";
        aEPSAuthentication.latitude = Convert.ToDouble(aEPSRequest.latitude);
        aEPSAuthentication.longitude = Convert.ToDouble(aEPSRequest.longitude);

        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(aEPSAuthentication);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);

        JObject Data = JObject.Parse(Result);
        if (Data["code"].ToString().ToUpper() == "TXN")
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = true,
                StatusM = "Success",
                Message = Data["mess"].ToString(),
                Request = Request,
                Response = Result,
            };
        }
        else
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = false,
                StatusM = "Failed",
                Message = Data["mess"].ToString(),
                Request = Request,
                Response = Result,
            };
        }

        return aEPSResponseAPI;
    }


    public async Task<AuthencationResponse> EKYCSoniBank7(AuthencationRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AuthencationResponse();
        var tType = aEPSRequest.pipe?.ToLower();


        AEPSKYC finoCapture = new AEPSKYC();

        finoCapture.merchantLoginId = aEPSRequest.AEPSID;
        finoCapture.primaryKeyId = aEPSRequest.Optional2.ToInt32();
        finoCapture.encodeFPTxnId = aEPSRequest.Optional1;
        finoCapture.requestRemarks = "test";
        finoCapture.nationalBankIdentificationNumber = "";
        finoCapture.indicatorforUID = "0";
        finoCapture.devicename = aEPSRequest.DeviceName;
        finoCapture.adhaarNumber = aEPSRequest.adhaarnumber;
        finoCapture.capture = HttpUtility.HtmlDecode(aEPSRequest.Data);

        finoCapture.APIID = SONITechnoCredentrial.APIID;
        finoCapture.Token = SONITechnoCredentrial.Token;
        finoCapture.MethodName = "ekyc";


        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(finoCapture);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);

        JObject Data = JObject.Parse(Result);
        if (Data["code"].ToString().ToUpper() == "TXN")
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = true,
                StatusM = "Success",
                Message = Data["mess"].ToString(),
                Request = Request,
                Response = Result,
            };
        }
        else
        {
            aEPSResponseAPI = new AuthencationResponse
            {
                Status = false,
                StatusM = "Failed",
                Message = Data["mess"].ToString(),
                Request = Request,
                Response = Result,
            };
        }

        return aEPSResponseAPI;
    }

    public async Task<AEPSResponseAPI> DOBESoniTechno(AEPSRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AEPSResponseAPI();

        var tType = aEPSRequest.BankType?.ToLower();
        AEPSFingpayBE cashWithdraw = new AEPSFingpayBE();
        cashWithdraw.APIID = SONITechnoCredentrial.APIID;
        cashWithdraw.Token = SONITechnoCredentrial.Token;

        if (tType == "bank8")
            cashWithdraw.MethodName = "be1";
        else if (tType == "bank9")
            cashWithdraw.MethodName = "be2";
        else
            cashWithdraw.MethodName = "be";


        cashWithdraw.isFacialTan = (aEPSRequest.DeviceName == "Face");
        cashWithdraw.OrderID = aEPSRequest.OrderID;
        cashWithdraw.Amount = aEPSRequest.Amount.ToString();
        cashWithdraw.merchantLoginId = aEPSRequest.AEPSID;
        cashWithdraw.mobileNumber = aEPSRequest.MobileNO;
        cashWithdraw.CustomerNO = aEPSRequest.CusMobileNO;
        cashWithdraw.BankName = aEPSRequest.BankName;
        cashWithdraw.Source = aEPSRequest.Source;
        cashWithdraw.nationalBankIdentificationNumber = aEPSRequest.BankNo;
        cashWithdraw.indicatorforUID = "0";
        cashWithdraw.adhaarNumber = aEPSRequest.Adhaarnumber;
        cashWithdraw.devicename = aEPSRequest.DeviceName;
        cashWithdraw.IP = aEPSRequest.IP;
        cashWithdraw.TType = aEPSRequest.TType;
        cashWithdraw.capture = HttpUtility.HtmlDecode(aEPSRequest.Data);
        cashWithdraw.latitude = Convert.ToDouble(aEPSRequest.Latitude);
        cashWithdraw.longitude = Convert.ToDouble(aEPSRequest.Longitude);
        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(cashWithdraw);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);
        JObject dataResponse = JObject.Parse(Result);

        bool isSuccess = dataResponse["code"].ToString().ToUpper() == "TXN";

        aEPSResponseAPI = new AEPSResponseAPI
        {
            Status = isSuccess,
            StatusM = isSuccess ? "Success" : "Failed",
            Message = dataResponse["data"]?["message"]?.ToString() ?? dataResponse["mess"].ToString(),
            BCCode = aEPSRequest.BCCode ?? "",
            BCName = aEPSRequest.BCName ?? "",
            MobileNO = aEPSRequest.MobileNO ?? "",
            Adhaarnumber = aEPSRequest.Adhaarnumber ?? "",
            BankName = aEPSRequest.BankName ?? "",
            DeviceName = aEPSRequest.DeviceName ?? "",
            TransDate = System.DateTime.Now.ToString(),
            BalanceAmount = isSuccess ? dataResponse["data"]?["balanceamount"]?.ToString() ?? "" : "",
            BankRRN = isSuccess ? dataResponse["data"]?["bankrrn"]?.ToString() ?? "" : "",
            OrderID = aEPSRequest.OrderID ?? "",
            VendorID = isSuccess ? dataResponse["data"]?["ackno"]?.ToString() ?? "" : "",
            TType = aEPSRequest.TType,
            Title = "Balance Enquiry",
            IP = aEPSRequest.IP ?? "",
            Latitude = aEPSRequest.Latitude,
            Longitude = aEPSRequest.Longitude,
            Request = Request,
            Amount = aEPSRequest.Amount.ToString(),
            Response = Result,
            ministatement = null
        };

        return aEPSResponseAPI;
    }

    public async Task<AEPSResponseAPI> DOMSSoniTechno(AEPSRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AEPSResponseAPI();

        var tType = aEPSRequest.BankType?.ToLower();


        AEPSFingpayBE cashWithdraw = new AEPSFingpayBE();
        cashWithdraw.APIID = SONITechnoCredentrial.APIID;
        cashWithdraw.Token = SONITechnoCredentrial.Token;

        if (tType == "bank8")
            cashWithdraw.MethodName = "ms1";
        else if (tType == "bank9")
            cashWithdraw.MethodName = "ms2";
        else
            cashWithdraw.MethodName = "ms";

        // cashWithdraw.MethodName = (tType == "bank8" || tType == "bank9") ? "ms1" : "ms";
        cashWithdraw.isFacialTan = false;
        cashWithdraw.OrderID = aEPSRequest.OrderID;
        cashWithdraw.Amount = aEPSRequest.Amount.ToString();
        cashWithdraw.merchantLoginId = aEPSRequest.AEPSID;
        cashWithdraw.mobileNumber = aEPSRequest.MobileNO;
        cashWithdraw.CustomerNO = aEPSRequest.CusMobileNO;
        cashWithdraw.BankName = aEPSRequest.BankName;
        cashWithdraw.Source = aEPSRequest.Source;
        cashWithdraw.nationalBankIdentificationNumber = aEPSRequest.BankNo;
        cashWithdraw.indicatorforUID = "0";
        cashWithdraw.adhaarNumber = aEPSRequest.Adhaarnumber;
        cashWithdraw.devicename = aEPSRequest.DeviceName;
        cashWithdraw.IP = aEPSRequest.IP;
        cashWithdraw.TType = aEPSRequest.TType;
        cashWithdraw.capture = HttpUtility.HtmlDecode(aEPSRequest.Data);
        cashWithdraw.latitude = Convert.ToDouble(aEPSRequest.Latitude);
        cashWithdraw.longitude = Convert.ToDouble(aEPSRequest.Longitude);
        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(cashWithdraw);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);
        JObject dataResponse = JObject.Parse(Result);

        bool isSuccess = dataResponse["code"].ToString().ToUpper() == "TXN";

        aEPSResponseAPI = new AEPSResponseAPI
        {
            Status = isSuccess,
            StatusM = isSuccess ? "Success" : "Failed",
            Message = dataResponse["data"]?["message"]?.ToString() ?? dataResponse["mess"].ToString(),
            BCCode = aEPSRequest.BCCode ?? "",
            BCName = aEPSRequest.BCName ?? "",
            MobileNO = aEPSRequest.MobileNO ?? "",
            Adhaarnumber = aEPSRequest.Adhaarnumber ?? "",
            BankName = aEPSRequest.BankName ?? "",
            DeviceName = aEPSRequest.DeviceName ?? "",
            TransDate = System.DateTime.Now.ToString(),
            BalanceAmount = isSuccess ? dataResponse["data"]?["balanceamount"]?.ToString() ?? "" : "",
            BankRRN = isSuccess ? dataResponse["data"]?["bankrrn"]?.ToString() ?? "" : "",
            OrderID = aEPSRequest.OrderID ?? "",
            VendorID = isSuccess ? dataResponse["data"]?["ackno"]?.ToString() ?? "" : "",
            TType = aEPSRequest.TType,
            Title = "Balance Enquiry",
            IP = aEPSRequest.IP ?? "",
            Latitude = aEPSRequest.Latitude,
            Longitude = aEPSRequest.Longitude,
            Request = Request,
            Amount = aEPSRequest.Amount.ToString(),
            Response = Result,
            ministatement = isSuccess ? JsonConvert.DeserializeObject<List<Ministatement>>(dataResponse["data"]["ministatement"].ToString()) : null
        };

        return aEPSResponseAPI;
    }
    public async Task<AEPSResponseAPI> DOCWSoniTechno(AEPSRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AEPSResponseAPI();

        var tType = aEPSRequest.BankType?.ToLower();


        AEPSFingpayBE cashWithdraw = new AEPSFingpayBE();
        cashWithdraw.APIID = SONITechnoCredentrial.APIID;
        cashWithdraw.Token = SONITechnoCredentrial.Token;
        if (tType == "bank8")
            cashWithdraw.MethodName = "cw1";
        else if (tType == "bank9")
            cashWithdraw.MethodName = "cw2";
        else
            cashWithdraw.MethodName = "cw";

        cashWithdraw.isFacialTan = false;
        cashWithdraw.OrderID = aEPSRequest.OrderID;
        cashWithdraw.Amount = aEPSRequest.Amount.ToString();
        cashWithdraw.merchantLoginId = aEPSRequest.AEPSID;
        cashWithdraw.mobileNumber = aEPSRequest.MobileNO;
        cashWithdraw.CustomerNO = aEPSRequest.CusMobileNO;
        cashWithdraw.BankName = aEPSRequest.BankName;
        cashWithdraw.Source = aEPSRequest.Source;
        cashWithdraw.nationalBankIdentificationNumber = aEPSRequest.BankNo;
        cashWithdraw.indicatorforUID = "0";
        cashWithdraw.adhaarNumber = aEPSRequest.Adhaarnumber;
        cashWithdraw.devicename = aEPSRequest.DeviceName;
        cashWithdraw.IP = aEPSRequest.IP;
        cashWithdraw.TType = aEPSRequest.TType;
        cashWithdraw.TransactionId = aEPSRequest.TransactionId;
        cashWithdraw.UidaiDataTxn = aEPSRequest.UidaiDataTxn;
        cashWithdraw.txnReferenceNo = aEPSRequest.txnReferenceNo;
        cashWithdraw.authRequestId = aEPSRequest.authRequestId;
        cashWithdraw.requestid = aEPSRequest.requestid;
        cashWithdraw.capture = HttpUtility.HtmlDecode(aEPSRequest.Data);
        cashWithdraw.latitude = Convert.ToDouble(aEPSRequest.Latitude);
        cashWithdraw.longitude = Convert.ToDouble(aEPSRequest.Longitude);
        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(cashWithdraw);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);

        await cls.update_dataAsync("insert into logdata(data)values('" + Result + "')");
        JObject dataResponse = JObject.Parse(Result);

        bool isSuccess = dataResponse["code"].ToString().ToUpper() == "TXN";

        aEPSResponseAPI = new AEPSResponseAPI
        {
            Status = isSuccess,
            StatusM = isSuccess ? "Success" : "Failed",
            Message = dataResponse["data"]?["message"]?.ToString() ?? dataResponse["mess"].ToString(),
            BCCode = aEPSRequest.BCCode ?? "",
            BCName = aEPSRequest.BCName ?? "",
            MobileNO = aEPSRequest.MobileNO ?? "",
            Adhaarnumber = aEPSRequest.Adhaarnumber ?? "",
            BankName = aEPSRequest.BankName ?? "",
            DeviceName = aEPSRequest.DeviceName ?? "",
            TransDate = System.DateTime.Now.ToString(),
            BalanceAmount = isSuccess ? dataResponse["data"]?["balanceamount"]?.ToString() ?? "" : "",
            BankRRN = isSuccess ? dataResponse["data"]?["bankrrn"]?.ToString() ?? "" : "",
            OrderID = aEPSRequest.OrderID ?? "",
            VendorID = isSuccess ? dataResponse["data"]?["ackno"]?.ToString() ?? "" : "",
            TType = aEPSRequest.TType,
            Title = "Balance Enquiry",
            IP = aEPSRequest.IP ?? "",
            Latitude = aEPSRequest.Latitude,
            Longitude = aEPSRequest.Longitude,
            Request = Request,
            Amount = aEPSRequest.Amount.ToString(),
            Response = Result,
            ministatement = null
        };

        return aEPSResponseAPI;
    }


    public async Task<AEPSResponseAPI> DOOTPCWSoniTechno(AEPSRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AEPSResponseAPI();

        var tType = aEPSRequest.BankType?.ToLower();


        AEPSFingpayBE cashWithdraw = new AEPSFingpayBE();
        cashWithdraw.APIID = SONITechnoCredentrial.APIID;
        cashWithdraw.Token = SONITechnoCredentrial.Token;

        cashWithdraw.MethodName = "sendotpcw";


        cashWithdraw.isFacialTan = false;
        cashWithdraw.OrderID = aEPSRequest.OrderID;
        cashWithdraw.Amount = aEPSRequest.Amount.ToString();
        cashWithdraw.merchantLoginId = aEPSRequest.AEPSID;
        cashWithdraw.mobileNumber = aEPSRequest.MobileNO;
        cashWithdraw.CustomerNO = aEPSRequest.CusMobileNO;
        cashWithdraw.BankName = aEPSRequest.BankName;
        cashWithdraw.Source = aEPSRequest.Source;
        cashWithdraw.nationalBankIdentificationNumber = aEPSRequest.BankNo;
        cashWithdraw.indicatorforUID = "0";
        cashWithdraw.adhaarNumber = aEPSRequest.Adhaarnumber;
        cashWithdraw.devicename = aEPSRequest.DeviceName;
        cashWithdraw.IP = aEPSRequest.IP;
        cashWithdraw.TType = aEPSRequest.BankType;
        cashWithdraw.capture = HttpUtility.HtmlDecode(aEPSRequest.Data);
        cashWithdraw.latitude = Convert.ToDouble(aEPSRequest.Latitude);
        cashWithdraw.longitude = Convert.ToDouble(aEPSRequest.Longitude);
        cashWithdraw.TransactionId = aEPSRequest.TransactionId;
        cashWithdraw.UidaiDataTxn = aEPSRequest.UidaiDataTxn;
        cashWithdraw.txnReferenceNo = aEPSRequest.txnReferenceNo;
        cashWithdraw.authRequestId = aEPSRequest.authRequestId;
        cashWithdraw.requestid = aEPSRequest.requestid;

        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(cashWithdraw);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);

        JObject dataResponse = JObject.Parse(Result);

        bool isSuccess = dataResponse["code"].ToString().ToUpper() == "TXN";

        aEPSResponseAPI = new AEPSResponseAPI
        {
            Status = isSuccess,

            TransactionId = isSuccess ? dataResponse["data"]?["TransactionId"]?.ToString() : null,
            UidaiDataTxn = isSuccess ? dataResponse["data"]?["UidaiDataTxn"]?.ToString() : null,
            txnReferenceNo = isSuccess ? dataResponse["data"]?["txnReferenceNo"]?.ToString() : null,
            requestid = isSuccess ? dataResponse["data"]?["requestid"]?.ToString() : null,
            authRequestId = isSuccess ? dataResponse["data"]?["authRequestId"]?.ToString() : null,

            Message = isSuccess
         ? dataResponse["data"]?["Message"]?.ToString()
         : dataResponse["mess"]?.ToString()
        };

        return aEPSResponseAPI;
    }
    public async Task<AEPSResponseAPI> DOCDSoniTechno(AEPSRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AEPSResponseAPI();

        var tType = aEPSRequest.BankType?.ToLower();


        AEPSFingpayBE cashWithdraw = new AEPSFingpayBE();
        cashWithdraw.APIID = SONITechnoCredentrial.APIID;
        cashWithdraw.Token = SONITechnoCredentrial.Token;
        if (tType == "bank8")
            cashWithdraw.MethodName = "cd";


        cashWithdraw.isFacialTan = false;
        cashWithdraw.OrderID = aEPSRequest.OrderID;
        cashWithdraw.Amount = aEPSRequest.Amount.ToString();
        cashWithdraw.merchantLoginId = aEPSRequest.AEPSID;
        cashWithdraw.mobileNumber = aEPSRequest.MobileNO;
        cashWithdraw.CustomerNO = aEPSRequest.CusMobileNO;
        cashWithdraw.BankName = aEPSRequest.BankName;
        cashWithdraw.Source = aEPSRequest.Source;
        cashWithdraw.nationalBankIdentificationNumber = aEPSRequest.BankNo;
        cashWithdraw.indicatorforUID = "0";
        cashWithdraw.adhaarNumber = aEPSRequest.Adhaarnumber;
        cashWithdraw.devicename = aEPSRequest.DeviceName;
        cashWithdraw.IP = aEPSRequest.IP;
        cashWithdraw.TType = aEPSRequest.TType;
        cashWithdraw.capture = HttpUtility.HtmlDecode(aEPSRequest.Data);
        cashWithdraw.latitude = Convert.ToDouble(aEPSRequest.Latitude);
        cashWithdraw.longitude = Convert.ToDouble(aEPSRequest.Longitude);
        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(cashWithdraw);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);
        JObject dataResponse = JObject.Parse(Result);

        bool isSuccess = dataResponse["code"].ToString().ToUpper() == "TXN";

        aEPSResponseAPI = new AEPSResponseAPI
        {
            Status = isSuccess,
            StatusM = isSuccess ? "Success" : "Failed",
            Message = dataResponse["data"]?["message"]?.ToString() ?? dataResponse["mess"].ToString(),
            BCCode = aEPSRequest.BCCode ?? "",
            BCName = aEPSRequest.BCName ?? "",
            MobileNO = aEPSRequest.MobileNO ?? "",
            Adhaarnumber = aEPSRequest.Adhaarnumber ?? "",
            BankName = aEPSRequest.BankName ?? "",
            DeviceName = aEPSRequest.DeviceName ?? "",
            TransDate = System.DateTime.Now.ToString(),
            BalanceAmount = isSuccess ? dataResponse["data"]?["balanceamount"]?.ToString() ?? "" : "",
            BankRRN = isSuccess ? dataResponse["data"]?["bankrrn"]?.ToString() ?? "" : "",
            OrderID = aEPSRequest.OrderID ?? "",
            VendorID = isSuccess ? dataResponse["data"]?["ackno"]?.ToString() ?? "" : "",
            TType = aEPSRequest.TType,
            Title = "Balance Enquiry",
            IP = aEPSRequest.IP ?? "",
            Latitude = aEPSRequest.Latitude,
            Longitude = aEPSRequest.Longitude,
            Request = Request,
            Amount = aEPSRequest.Amount.ToString(),
            Response = Result,
            ministatement = null
        };

        return aEPSResponseAPI;
    }

    public async Task<AEPSResponseAPI> DOAWSoniTechno(AEPSRequest aEPSRequest)
    {
        var aEPSResponseAPI = new AEPSResponseAPI();

        var tType = aEPSRequest.BankType?.ToLower();


        AEPSFingpayBE cashWithdraw = new AEPSFingpayBE();
        cashWithdraw.APIID = SONITechnoCredentrial.APIID;
        cashWithdraw.Token = SONITechnoCredentrial.Token;
        if (tType == "bank8")
            cashWithdraw.MethodName = "aw";
        else if (tType == "bank9")
            cashWithdraw.MethodName = "aw";
        else
            cashWithdraw.MethodName = "aw";

        cashWithdraw.isFacialTan = false;
        cashWithdraw.OrderID = aEPSRequest.OrderID;
        cashWithdraw.Amount = aEPSRequest.Amount.ToString();
        cashWithdraw.merchantLoginId = aEPSRequest.AEPSID;
        cashWithdraw.mobileNumber = aEPSRequest.MobileNO;
        cashWithdraw.CustomerNO = aEPSRequest.CusMobileNO;
        cashWithdraw.BankName = aEPSRequest.BankName;
        cashWithdraw.Source = aEPSRequest.Source;
        cashWithdraw.nationalBankIdentificationNumber = aEPSRequest.BankNo;
        cashWithdraw.indicatorforUID = "0";
        cashWithdraw.adhaarNumber = aEPSRequest.Adhaarnumber;
        cashWithdraw.devicename = aEPSRequest.DeviceName;
        cashWithdraw.IP = aEPSRequest.IP;
        cashWithdraw.TType = aEPSRequest.TType;
        cashWithdraw.capture = HttpUtility.HtmlDecode(aEPSRequest.Data);
        cashWithdraw.latitude = Convert.ToDouble(aEPSRequest.Latitude);
        cashWithdraw.longitude = Convert.ToDouble(aEPSRequest.Longitude);
        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(cashWithdraw);
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.AEPSCorePaytm);
        JObject dataResponse = JObject.Parse(Result);

        bool isSuccess = dataResponse["code"].ToString().ToUpper() == "TXN";

        aEPSResponseAPI = new AEPSResponseAPI
        {
            Status = isSuccess,
            StatusM = isSuccess ? "Success" : "Failed",
            Message = dataResponse["data"]?["message"]?.ToString() ?? dataResponse["mess"].ToString(),
            BCCode = aEPSRequest.BCCode ?? "",
            BCName = aEPSRequest.BCName ?? "",
            MobileNO = aEPSRequest.MobileNO ?? "",
            Adhaarnumber = aEPSRequest.Adhaarnumber ?? "",
            BankName = aEPSRequest.BankName ?? "",
            DeviceName = aEPSRequest.DeviceName ?? "",
            TransDate = System.DateTime.Now.ToString(),
            BalanceAmount = isSuccess ? dataResponse["data"]?["balanceamount"]?.ToString() ?? "" : "",
            BankRRN = isSuccess ? dataResponse["data"]?["bankrrn"]?.ToString() ?? "" : "",
            OrderID = aEPSRequest.OrderID ?? "",
            VendorID = isSuccess ? dataResponse["data"]?["ackno"]?.ToString() ?? "" : "",
            TType = aEPSRequest.TType,
            Title = "Balance Enquiry",
            IP = aEPSRequest.IP ?? "",
            Latitude = aEPSRequest.Latitude,
            Longitude = aEPSRequest.Longitude,
            Request = Request,
            Amount = aEPSRequest.Amount.ToString(),
            Response = Result,
            ministatement = null
        };

        return aEPSResponseAPI;
    }

    public async Task<DataTable> GetAEPSStatus(string BankName, string Msrno)
    {
        DataTable dthistory = await cls.select_data_dtAsync("select iif(DailyAuthentication=1 and convert(nvarchar(30),authdate,112)=convert(nvarchar(30),getdate(),112),cast(1 as bit),cast(0 as bit)) as DailyAuthentication,* from AEPSOnBoardStatus where msrno=" + Msrno + " and name='" + BankName + "'");
        return dthistory;
    }

    public async Task<string> GetBankList(string OutLetID)
    {
        return await ApiPostCallInstantPay.GetBankListAsync(OutLetID);
    }





    public async Task<Int32> UpdateAuthentication(string BankName, string Msrno)
    {
        return await cls.update_dataAsync("update AEPSOnBoardStatus set DailyAuthentication=1,AuthDate=getdate() where msrno=" + Msrno + " and Name='" + BankName + "'");
    }

    public string ComputeMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2")); // Convert to hex

            return sb.ToString();
        }
    }


}