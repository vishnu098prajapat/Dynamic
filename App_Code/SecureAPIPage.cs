using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

/// <summary>
/// Summary description for SecureAPIPage
/// </summary>
public class SecureAPIPage : System.Web.UI.Page
{
    // Methods that require token
    // GLOBAL SHARED OBJECTS
    protected readonly string ServerSecret = "K2d79J2l8pWm6b3Vh12N51qJx4tA7HjL9dA3pO==";
    protected readonly Int32 TimeP = 20;
    protected cls_myMember clsm = new cls_myMember();
    protected cls_connection objConnection = new cls_connection();

    // Dynamic runtime variables
    protected string DeviceID = "";
    protected string userID = "";
    protected string CID = "1";
    protected ApiHeaders headers = new ApiHeaders();

    // Methods that DO NOT require JWT token
    protected readonly List<string> MethodsRequiringToken = new List<string>
    {
        "getcompanydetails",
        "getcustomerdetails",
        "refreshtoken",
        "getrolelist",
        "sendotp",
        "checknumber",
        "getstate",
        "twowaylogin",
        "verifyotp",
        "signup",
        "getpostoffice"
    };

    protected override void OnLoad(EventArgs e)
    {
        if (Request.HttpMethod != "POST")
        {
            ErrorMessage("Only POST allowed.");
            return;
        }

        NameValueCollection nvc = Request.Form;

        if (string.IsNullOrWhiteSpace(nvc["Request"]))
        {
            ErrorMessage("Invalid Request.");
            return;
        }

        JObject jObj = JObject.Parse(nvc["Request"]);
        string methodName = Convert.ToString(jObj["MethodName"])
                                  .Trim().ToLower();

        // ⭐ Header Validation
        if (MethodsRequiringToken.Contains(methodName))
        {
            if (!ValidateHeadersNon()) return;
        }
        else
        {
            if (!ValidateHeaders()) return;
        }

        headers = GetHeaders();
        DeviceID = headers.DeviceId;

        // ⭐ Signature
        if (!ValidateSignature(nvc["Request"].ToString()))
        {
            // ErrorMessage("Invalid Signature.");
            //  return;
        }

        // ⭐ JWT Token
        if (!MethodsRequiringToken.Contains(methodName))
        {
            var principal = ValidateJwtToken(headers.Authorization);
            if (principal == null)
            {
                // ErrorMessage("Token expired.");
                //return;
            }
            try
            {
                SetUserContext(principal);
            }
            catch (Exception ex)
            {
                ErrorMessage("Token expired."); // Logout old device
                return;
            }

            // Get userID from context
            userID = UserContext.Current.UserID;

            // ⭐ Device/App/Platform Match
            if (headers.DeviceId != UserContext.Current.DeviceID ||
                headers.AppId != UserContext.Current.AppType ||
                headers.Platform != UserContext.Current.Role)
            {
                ErrorMessage("Device/App/Platform mismatch");
                return;
            }

            // ⭐ Check old device login
            string dbDeviceID = GetDeviceIDFromDB(userID); // <-- create this function in DB layer

            if (dbDeviceID != UserContext.Current.DeviceID)
            {
                ErrorMessage("FORCE_LOGOUT"); // Logout old device
                return;
            }

        }

        // Allow page to continue
        base.OnLoad(e);
    }



    public string GetDeviceIDFromDB(string userID)
    {
        string msrno = objConnection.select_data_scalar_string("select msrno from member where loginid='" + userID + "'");
        var dt = objConnection.select_data_dt(
            "SELECT TOP 1 * FROM DeviceData WHERE MSRNO='" + msrno + "' AND IsActive=1 order by id desc"
        );

        if (dt.Rows.Count == 0) return "";
        return dt.Rows[0]["DeviceID"].ToString();
    }


    private bool ValidateHeaders()
    {
        NameValueCollection headers = Request.Headers;

        string[] requiredHeaders = new string[]
        {
            "Authorization",
            "X-API-VERSION",
            "X-REQUEST-TIME",
            "X-DEVICE-ID",
            "X-APP-ID",
            "X-PLATFORM",
            "X-NONCE",
            "X-SIGNATURE",
            "X-LATITUDE",
            "X-LONGITUDE"
        };

        foreach (var header in requiredHeaders)
        {
            if (string.IsNullOrEmpty(headers[header]))
            {
                ErrorMessage(header + " header missing");
                return false;
            }
        }

        long timestamp;
        // 2. Validate X-REQUEST-TIME (timestamp)
        if (!long.TryParse(headers["X-REQUEST-TIME"], out timestamp))
        {
            ErrorMessage("Invalid X-REQUEST-TIME format.");
            return false;
        }

        // Timestamp validation (3 minutes window)
        long requestTime = Convert.ToInt64(headers["X-REQUEST-TIME"]);
        long serverTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (Math.Abs(serverTime - requestTime) > 600000)
        {
            //ErrorMessage("Request timestamp expired / replay attack");
            // return false;
        }


        // 3. Validate X-PLATFORM
        string platform = headers["X-PLATFORM"].ToString().ToLower();

        if (platform != "android" && platform != "ios" && platform != "web")
        {
            ErrorMessage("Invalid X-PLATFORM.");
            return false;
        }

        // 4. Validate X-API-VERSION
        string apiVersion = headers["X-API-VERSION"];

        string[] allowedVersions = { "1.0", "2.0", "3.0" };

        if (!allowedVersions.Contains(apiVersion))
        {
            ErrorMessage("Unsupported X-API-VERSION.");
            return false;
        }

        double latitude, longitude;
        // Validate latitude
        if (!double.TryParse(headers["X-LATITUDE"], out latitude) || latitude < -90 || latitude > 90)
        {
            ErrorMessage("Invalid X-LATITUDE.");
            return false;
        }

        // Validate longitude
        if (!double.TryParse(headers["X-LONGITUDE"], out longitude) || longitude < -180 || longitude > 180)
        {
            ErrorMessage("Invalid X-LONGITUDE.");
            return false;
        }

        // OPTIONAL: NONCE CHECK (prevent replay)
        // Save NONCE in DB or Cache and check for duplicates

        if (!IsInsideIndia(Convert.ToDecimal(headers["X-LATITUDE"]), Convert.ToDecimal(headers["X-LONGITUDE"])))
        {
             ErrorMessage("Invalid Location: Coordinates are outside India.");
            return false;
        }

        return true;
    }

    bool IsInsideIndia(decimal latitude, decimal longitude)
    {
        return (latitude >= 8 && latitude <= 37 &&
                longitude >= 68.7m && longitude <= 98m);
    }

    public ApiHeaders GetHeaders()
    {
        NameValueCollection headers = Request.Headers;
        return new ApiHeaders
        {
            Authorization = headers["Authorization"],
            ApiVersion = headers["X-API-VERSION"],
            RequestTime = headers["X-REQUEST-TIME"],
            DeviceId = headers["X-DEVICE-ID"],
            AppId = headers["X-APP-ID"],
            Platform = headers["X-PLATFORM"],
            Nonce = headers["X-NONCE"],
            Signature = headers["X-SIGNATURE"],
            Latitude = headers["X-LATITUDE"],
            Longitude = headers["X-LONGITUDE"],
            IP = ""
        };
    }

    private bool ValidateSignature(string requestBody)
    {
        string timestamp = Request.Headers["X-REQUEST-TIME"];
        string clientSignature = Request.Headers["X-SIGNATURE"];

        string calculatedSignature = GenerateSignature(requestBody, timestamp, ServerSecret);

        return string.Equals(clientSignature, calculatedSignature, StringComparison.OrdinalIgnoreCase);
    }
    private string GenerateSignature(string body, string timestamp, string secret)
    {
        string data = $"{body}|{timestamp}|{secret}";
        byte[] bytes = Encoding.UTF8.GetBytes(data);

        using (var sha = SHA256.Create())
        {
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public ClaimsPrincipal ValidateJwtToken(string token)
    {
        try
        {
            ITokenService tokenService = new TokenService(ServerSecret, TimeP);
            return tokenService.ReadTokenNew(token.Replace("Bearer ", ""));
        }
        catch
        {
            return null;
        }
    }

    private void ErrorMessage(string ErrorMessage)
    {
        string response = "{\"statuscode\":\"ERR\",\"status\":\"" + ErrorMessage + "\"}";
        if (ErrorMessage == "Token expired.")
        {
            Response.StatusCode = 201;
        }
        else if (ErrorMessage == "FORCE_LOGOUT")
        {
            Response.StatusCode = 220;
        }
        else
        {
            Response.StatusCode = 200;
        }
        Response.ContentType = "application/json";
        Response.Write(response);
        Response.End();
        return;
    }

    private bool ValidateHeadersNon()
    {
        NameValueCollection headers = Request.Headers;

        string[] requiredHeaders = new string[]
        {
            "X-API-VERSION",
            "X-REQUEST-TIME",
            "X-DEVICE-ID",
            "X-APP-ID",
            "X-PLATFORM",
            "X-NONCE",
            "X-SIGNATURE"
        };

        foreach (var header in requiredHeaders)
        {
            if (string.IsNullOrEmpty(headers[header]))
            {
                ErrorMessage(header + " header missing");
                return false;
            }
        }

        long timestamp;
        // 2. Validate X-REQUEST-TIME (timestamp)
        if (!long.TryParse(headers["X-REQUEST-TIME"], out timestamp))
        {
            ErrorMessage("Invalid X-REQUEST-TIME format.");
            return false;
        }

        // Timestamp validation (3 minutes window)
        long requestTime = Convert.ToInt64(headers["X-REQUEST-TIME"]);
        long serverTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (Math.Abs(serverTime - requestTime) > 1800000)
        {
            ErrorMessage("Request timestamp expired / replay attack");
            return false;
        }


        // 3. Validate X-PLATFORM
        string platform = headers["X-PLATFORM"].ToString().ToLower();

        if (platform != "android" && platform != "ios" && platform != "web")
        {
            ErrorMessage("Invalid X-PLATFORM.");
            return false;
        }

        // 4. Validate X-API-VERSION
        string apiVersion = headers["X-API-VERSION"];

        string[] allowedVersions = { "1.0", "2.0", "3.0" };

        if (!allowedVersions.Contains(apiVersion))
        {
            ErrorMessage("Unsupported X-API-VERSION.");
            return false;
        }


        // OPTIONAL: NONCE CHECK (prevent replay)
        // Save NONCE in DB or Cache and check for duplicates

        return true;
    }

    private void SetUserContext(ClaimsPrincipal principal)
    {

        var ctx = new AppUserContext
        {
            //UserID = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
            UserID = principal.FindFirst("UserID")?.Value,
            DeviceID = principal.FindFirst("device_id")?.Value,
            AppType = principal.FindFirst("app_id")?.Value,
            Role = principal.FindFirst("platform")?.Value
        };


        UserContext.Current = ctx;
    }
}



public class ApiHeaders
{
    public string Authorization { get; set; }
    public string ApiVersion { get; set; }
    public string RequestTime { get; set; }
    public string DeviceId { get; set; }
    public string AppId { get; set; }
    public string Platform { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string IP { get; set; }
}


public class AppUserContext
{
    public string UserID { get; set; }
    public string MobileNo { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string DeviceID { get; set; }
    public string AppType { get; set; }
}


public static class UserContext
{
    private const string _contextKey = "AppUserContext";

    public static AppUserContext Current
    {
        get
        {
            return (AppUserContext)HttpContext.Current.Items[_contextKey];
        }
        internal set
        {
            HttpContext.Current.Items[_contextKey] = value;
        }
    }
}



public class Datum
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string ServiceID { get; set; }
    public bool IsActive { get; set; }
    public DateTime AddDate { get; set; }
    public int ShowPosition { get; set; }
    public string CColor { get; set; }
    public string BColor { get; set; }
    public bool TypeSection { get; set; }
    public List<ServiceDatum> ServiceData { get; set; }
}

public class RootSServiceList
{
    public string statuscode { get; set; }
    public string status { get; set; }
    public List<Datum> data { get; set; }
}

public class ServiceDatum
{
    public bool ServicePurchaseOrNot { get; set; }
    public int ID { get; set; }
    public string Name { get; set; }
    public int SectionType { get; set; }
    public bool IsActive { get; set; }
    public int OnTime { get; set; }
    public int OffTime { get; set; }
    public int UserID { get; set; }
    public string URL { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; }
    public string ICON { get; set; }
    public int OrderBy { get; set; }
    public bool IsNew { get; set; }
    public bool IsComming { get; set; }
    public bool Onoff { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public bool KYCCHECK { get; set; }

}



// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class OrderReport
{
    public string opName { get; set; }
    public string TransID { get; set; }
    public bool IsDispute { get; set; }
    public string DisputeStatus { get; set; }
    public string Status { get; set; }
    public string opVal { get; set; }
    public string ServiceID { get; set; }
    public string Amount { get; set; }
    public string IMG { get; set; }
    public string CName { get; set; }
    public string CVal { get; set; }
    public List<OrderItemReport> OrderItems { get; set; }
    public List<OrderItemReport> ReceiptData { get; set; }

    public OrderReport()
    {
        opName = "Airtel";
        IsDispute = true;
        Status = "";
        opVal = "10";
        ServiceID = "3";
        Amount = "3";
        IMG = "3";
        CName = "Comm";
        DisputeStatus = "Pending";
        CVal = "2.2";
    }
}

public class OrderItemReport
{
    public string KeyText { get; set; }
    public string KeyVal { get; set; }
}

public class RootReport
{
    public List<OrderReport> Orders { get; set; }
}



public class PipeModuleSetting
{
    public string ModuleName { get; set; }
    public bool ISFace { get; set; }
    public bool ISFing { get; set; }
    public bool IsIris { get; set; }
    public string WadhFace { get; set; }
    public string WadhFinger { get; set; }
    public string WadhIris { get; set; }
    public bool IsOTP { get; set; }
    public bool IsTPIN { get; set; }
    public bool IsRequired { get; set; }
}



public class AEPSOnBoardStatusSetting
{
    public int ID { get; set; }
    public string MsrNo { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }

    public bool AEPSTwoWayRegistration { get; set; }
    public bool DailyAuthentication { get; set; }

    public DateTime AddDate { get; set; }
    public DateTime? AuthDate { get; set; }

    public string MerAuthTxnId { get; set; }
    public bool IsUserMerAuthTxnId { get; set; }

    public string AEPSID { get; set; }
    public string Mobile { get; set; }
    public string Result { get; set; }
    public string ProfilePic { get; set; }

    public bool DoKyc { get; set; }
    public bool DoBKyc { get; set; }
    public string Pin { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public DateTime? CDAuthDate { get; set; }
    public bool CDDailyAuthentication { get; set; }

    public bool APDailyAuthentication { get; set; }
    public DateTime? APAuthDate { get; set; }

    public bool Optional1 { get; set; }
    public bool Optional2 { get; set; }
    public DateTime? Optional1AuthDate { get; set; }
    public DateTime? Optional2AuthDate { get; set; }
}