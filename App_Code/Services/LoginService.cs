using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for LoginService
/// </summary>
public class LoginService : ILoginService
{
    cls_connection cls;
    ITokenService _tokenService;
    IWallet wallet;

    public LoginService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
    }

    //public LoginService()
    //{
    //    cls = new cls_connection();
    //}

    public async Task<string> GetID(string LoginID)
    {
        return await cls.select_data_scalar_stringAsync("select msrno from Member where loginid='" + LoginID + "' or Mobile='" + LoginID + "'");
    }

    public async Task<MemberPermission> GetPermission(string Msrno)
    {
        string Result = await cls.select_data_scalar_stringAsync("SELECT *  FROM MemberSecurity where msrno=" + Msrno + "  FOR JSON AUTO");
        MemberPermission myDeserializedClass = JsonConvert.DeserializeObject<MemberPermission>(Result.Replace("[", "").Replace("]", ""));
        return myDeserializedClass;
    }

    public async Task<int> IncrementFailedAttempts(string LoginID)
    {
        return await cls.update_dataAsync("update member set LoginCount=LoginCount+1 where loginid='" + LoginID + "'");
    }

    public async Task<int> IncrementSuccessAttempts(string LoginID)
    {
        return await cls.update_dataAsync("update member set LoginCount=0 where loginid='" + LoginID + "'");
    }

    public async Task<int> LockAccount(string LoginID)
    {
        return await cls.update_dataAsync("update member set AccountLock=1 where loginid='" + LoginID + "'");
    }

    public async Task<int> DeActiveAccount(string LoginID)
    {
        return await cls.update_dataAsync("update member set isactive=0 where loginid='" + LoginID + "' or mobile='" + LoginID + "'");
    }

    public async Task<string> LoginPin(string LoginID)
    {
        return await cls.select_data_scalar_stringAsync("select LoginPin from Member where loginid='" + LoginID + "' or Mobile='" + LoginID + "'");
    }

    public async Task<int> SaveLoginDetails(Login login)
    {
        return await cls.update_dataAsync("SaveLastLogin '" + login.LoginID + "','" + login.IP + "','" + login.OS + "','" + login.Browser + "','" + login.Latitude + "','" + login.Longitude + "'");
    }

    public async Task<Int32> SendLoginOTP(string Mobile, string CompanyID)
    {
        Random generator = new Random();
        int r = generator.Next(1000, 9999);
        string OTP = r.ToString();
        string Message = "Login OTP is : " + OTP;
        // HttpContext.Current.Session["OTP"] = OTP;
        await Task.Delay(100);
        string[] ValueArray = new string[2];
        ValueArray[0] = "admin";
        ValueArray[1] = OTP;
        string str = SMS.SendWithV(Mobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, CompanyID);
        return OTP.ToInt32();
    }

    public bool IsStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Regex for strong password:
        // - At least 8 characters
        // - At least one uppercase letter
        // - At least one lowercase letter
        // - At least one digit
        // - At least one special character
        // - No spaces
        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
        return Regex.IsMatch(password, pattern);
    }

    public string GetPasswordStrengthMessage(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "Password cannot be empty.";

        if (password.Length < 6)
            return "Password must be at least 6 characters long.";

        if (!password.Any(char.IsUpper))
            return "Password must include at least one uppercase letter.";

        if (!password.Any(char.IsLower))
            return "Password must include at least one lowercase letter.";

        if (!password.Any(char.IsDigit))
            return "Password must include at least one numeric digit.";

        if (!password.Any(ch => "!@#$%^&*()_+{}[]|:;'<>?,./".Contains(ch)))
            return "Password must include at least one special character.";

        if (password.Contains(" "))
            return "Password must not contain spaces.";

        return "Password is strong.";
    }

    public async Task<Tuple<Int32, string>> RsetPassword(string Msrno, string Pass)
    {
        DataTable dt = await wallet.GetMemberDetails(Msrno);
        if (dt.Rows.Count > 0)
        {
            if (dt.Rows[0]["Isactive"].ToString().ToBoolen() == true)
            {
                if (!string.IsNullOrEmpty(Pass))
                {
                    string message = GetPasswordStrengthMessage(Pass);
                    if (message == "Password is strong.")
                    {
                        Int32 Val = await cls.update_dataAsync("PROC_MANAGE_PASSWROD '" + dt.Rows[0]["msrno"].ToString() + "','CHANGEPWD','" + Pass + "'");
                        if (Val > 0)
                            return new Tuple<int, string>(Val, "Password changed successfully.");
                        else
                            return new Tuple<int, string>(0, "Failed to update password. Please try again.");
                    }
                    else
                    {
                        return new Tuple<int, string>(0, message);
                    }

                }
                else
                    return new Tuple<Int32, string>(0, "Password is not strong enough.");
            }
            else
                return new Tuple<Int32, string>(0, "User Details Not in active mode.");
        }
        else
            return new Tuple<Int32, string>(0, "User Details Not Found");
    }


    public async Task<Tuple<Int32, string>> RsetTpin(string Msrno, string Pass)
    {
        DataTable dt = await wallet.GetMemberDetails(Msrno);
        if (dt.Rows.Count > 0)
        {
            if (dt.Rows[0]["Isactive"].ToString().ToBoolen() == true)
            {
                if (!string.IsNullOrEmpty(Pass))
                {
                    if (Pass.Length==4)
                    {
                        Int32 Val = await cls.update_dataAsync("PROC_MANAGE_PASSWROD '" + dt.Rows[0]["msrno"].ToString() + "','CHANGEPIN','" + Pass + "'");
                        if (Val > 0)
                            return new Tuple<int, string>(Val, "Tpin changed successfully.");
                        else
                            return new Tuple<int, string>(0, "Failed to update Tpin. Please try again.");
                    }
                    else
                    {
                        return new Tuple<int, string>(0, "4 length Tpin Allow");
                    }

                }
                else
                    return new Tuple<Int32, string>(0, "Password is not strong enough.");
            }
            else
                return new Tuple<Int32, string>(0, "User Details Not in active mode.");
        }
        else
            return new Tuple<Int32, string>(0, "User Details Not Found");
    }

    public async Task<ValidateReturn> Validate(Login login)
    {
        ValidateReturn validateReturn = new ValidateReturn();
        DataTable dt = await cls.select_data_dtAsync("select IsKycApproved,FirstLiveLogin,msrno,HASHBYTES('SHA2_512', '" + login.Password + "'+CAST(Salt AS NVARCHAR(36))) as Comparepass,LoginID,CAST(Salt AS NVARCHAR(36))Salt,Password,Isactive,AccountLock,LoginCount,NAME,Mobile,Email,(select RedirectUrl from role where id=member.RoleID)Role,(select Name from State where id=member.Stateid)Statename,macaddress from member where (Mobile='" + login.LoginID + "' or LoginiD='" + login.LoginID + "') and isactive=1");
        if (dt.Rows.Count > 0)
        {
            if (!Convert.ToBoolean(dt.Rows[0]["AccountLock"].ToString()))
            {
                if (dt.Rows[0]["LoginCount"].ToString().ToInt32() <= 2)
                {
                    byte[] hashValueComparepassBytes = (byte[])dt.Rows[0]["Comparepass"];
                    string SHash = BitConverter.ToString(hashValueComparepassBytes).Replace("-", "");
                    byte[] hashValueBytes = (byte[])dt.Rows[0]["Password"];
                    string hashValueHex = BitConverter.ToString(hashValueBytes).Replace("-", "");
                    bool hashesMatch = StringComparer.OrdinalIgnoreCase.Compare(hashValueHex, SHash) == 0;
                    if (hashesMatch)
                    {
                        await IncrementSuccessAttempts(login.LoginID);
                        string CheckIP = await CheckLoginIP(login.LoginID, login.IP);
                        if (CheckIP == "")
                            CheckIP = "0";

                        validateReturn.ReVerify = (CheckIP.ToInt32() <= 0 ? false : true);
                        if (!validateReturn.ReVerify == false)
                        {
                            await SaveLoginDetails(login);
                        }
                        cls.update_data("update member Set Latitude='" + login.Latitude + "',Longitude='" + login.Longitude + "' where msrno='" + dt.Rows[0]["msrno"].ToString() + "' and Latitude=0");
                        validateReturn.IsValid = true;
                        validateReturn.Reason = "User Valid";
                        validateReturn.Msrno = dt.Rows[0]["msrno"].ToString();
                        validateReturn.NAME = dt.Rows[0]["Name"].ToString();
                        validateReturn.Email = dt.Rows[0]["Email"].ToString();
                        validateReturn.RoleRedirectLink = dt.Rows[0]["Role"].ToString();
                        validateReturn.Statename = dt.Rows[0]["Statename"].ToString();
                        validateReturn.macaddress = dt.Rows[0]["macaddress"].ToString();
                        validateReturn.Mobile = dt.Rows[0]["Mobile"].ToString();
                        validateReturn.LoginID = dt.Rows[0]["LoginID"].ToString();
                        validateReturn.KYC = Convert.ToBoolean(dt.Rows[0]["IsKycApproved"].ToString());
                        validateReturn.FirstTimeLogin = Convert.ToBoolean(dt.Rows[0]["FirstLiveLogin"].ToString());
                    }
                    else
                    {
                        await IncrementFailedAttempts(login.LoginID);
                        validateReturn.IsValid = false;
                        validateReturn.Reason = "Incorrect Password";
                    }
                }
                else
                {
                    await LockAccount(login.LoginID);
                    validateReturn.IsValid = false;
                    validateReturn.Reason = "Your account has been locked due to multiple failed login attempts. Please try again later or reset your password.";
                }
            }
            else
            {
                validateReturn.IsValid = false;
                validateReturn.Reason = "Your account has been locked due to multiple failed login attempts. Please try again later or reset your password.";
            }
        }
        else
        {
            validateReturn.Reason = "In-Valid Account details";
            validateReturn.IsValid = false;
        }
        return validateReturn;
    }


    public async Task<string> ValidateMobile(Login login)
    {
        var item = await Validate(login);
        if (item.IsValid)
        {
            _tokenService = new TokenService("545345545345545345545345545345", 1);

            var token = await _tokenService.GenerateToken(item.Mobile, item.NAME, item.RoleRedirectLink, item.Msrno);


            object Res = new
            {
                Token = token,
                ExpiresAt = 300,
                user_details = new
                {
                    mobile = item.Mobile,
                    name = item.NAME,
                    roleid = 5,
                    id = item.Msrno,
                    iskyc = true,
                    stype = 1, // stype 1 means OTP,2 Means TPIN,3 Means Google Authencator
                    email = item.Email,
                    profile_picture = "",
                    skey = "543543",
                    loginpin = 1234
                },

            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(Res);

            return ErrorShow.SuccessArrayAPI(jsonString);
        }
        else
        {
            return ErrorShow.SuccessMessage(item.Reason);
        }
    }


    public async Task<String> SendOTP(string Stype, string Mobile, string Token)
    {
        _tokenService = new TokenService("545345545345545345545345545345", 1);
        string item = await _tokenService.ReadToken(Token);
        JObject jdata = JObject.Parse(item);
        if (jdata["code"].ToString() == "TXN")
        {
            Random generator = new Random();
            int r = generator.Next(1000, 9999);
            string OTP = r.ToString();
            await Task.Delay(100);
            string[] ValueArray = new string[2];
            ValueArray[0] = jdata["data"]["Name"].ToString();
            ValueArray[1] = OTP;
            string str = SMS.SendWithV(Mobile, ConstantsData.Admin_Login_OTP_SMS, ValueArray, "1");
            return "";
        }
        else
        {
            return "";
        }
    }

    public async Task<ValidateReturn> ValidateWithOTP(Login login, string ho)
    {
        ValidateReturn validateReturn = new ValidateReturn();
        DataTable dt = await cls.select_data_dtAsync("select IsKycApproved,FirstLiveLogin,msrno,HASHBYTES('SHA2_512', '" + login.Password + "'+CAST(Salt AS NVARCHAR(36))) as Comparepass,LoginID,CAST(Salt AS NVARCHAR(36))Salt,Password,Isactive,AccountLock,LoginCount,NAME,Mobile,Email,(select RedirectUrl from role where id=member.RoleID)Role,(select Name from State where id=member.Stateid)Statename,macaddress from member where Mobile='" + login.LoginID + "' or LoginiD='" + login.LoginID + "'");
        if (dt.Rows.Count > 0)
        {
            if (!Convert.ToBoolean(dt.Rows[0]["AccountLock"].ToString()))
            {
                if (dt.Rows[0]["LoginCount"].ToString().ToInt32() <= 2)
                {
                    byte[] hashValueComparepassBytes = (byte[])dt.Rows[0]["Comparepass"];
                    string SHash = BitConverter.ToString(hashValueComparepassBytes).Replace("-", "");
                    byte[] hashValueBytes = (byte[])dt.Rows[0]["Password"];
                    string hashValueHex = BitConverter.ToString(hashValueBytes).Replace("-", "");
                    bool hashesMatch = StringComparer.OrdinalIgnoreCase.Compare(hashValueHex, SHash) == 0;
                    if (hashesMatch)
                    {
                        await IncrementSuccessAttempts(login.LoginID);
                        string CheckIP = await CheckLoginIP(login.LoginID, login.IP);
                        if (CheckIP == "")
                        {
                            CheckIP = "0";
                        }
                        validateReturn.ReVerify = (CheckIP.ToInt32() <= 0 ? false : true);
                        if (ho == "1")
                        {
                            await SaveLoginDetails(login);
                        }
                        cls.update_data("update member Set Latitude='" + login.Latitude + "',Longitude='" + login.Longitude + "' where msrno='" + dt.Rows[0]["msrno"].ToString() + "' and Latitude=0");
                        validateReturn.IsValid = true;
                        validateReturn.Reason = "User Valid";
                        validateReturn.Msrno = dt.Rows[0]["msrno"].ToString();
                        validateReturn.NAME = dt.Rows[0]["Name"].ToString();
                        validateReturn.Email = dt.Rows[0]["Email"].ToString();
                        validateReturn.RoleRedirectLink = dt.Rows[0]["Role"].ToString();
                        validateReturn.Statename = dt.Rows[0]["Statename"].ToString();
                        validateReturn.macaddress = dt.Rows[0]["macaddress"].ToString();
                        validateReturn.Mobile = dt.Rows[0]["Mobile"].ToString();
                        validateReturn.LoginID = login.LoginID;
                        validateReturn.KYC = Convert.ToBoolean(dt.Rows[0]["IsKycApproved"].ToString());
                        validateReturn.FirstTimeLogin = Convert.ToBoolean(dt.Rows[0]["FirstLiveLogin"].ToString());
                    }
                    else
                    {
                        await IncrementFailedAttempts(login.LoginID);
                        validateReturn.IsValid = false;
                        validateReturn.Reason = "Incorrect Password";
                    }
                }
                else
                {
                    await LockAccount(login.LoginID);
                    validateReturn.IsValid = false;
                    validateReturn.Reason = "Your account has been locked due to multiple failed login attempts. Please try again later or reset your password.";
                }
            }
            else
            {
                validateReturn.IsValid = false;
                validateReturn.Reason = "Your account has been locked due to multiple failed login attempts. Please try again later or reset your password.";
            }
        }
        else
        {
            validateReturn.Reason = "In-Valid Account details";
            validateReturn.IsValid = false;
        }
        return validateReturn;
    }

    public async Task<string> CheckLoginIP(string LoginID, string IP)
    {
        return await cls.select_data_scalar_stringAsync("WITH LastRow AS (SELECT TOP 1 * FROM MEMBERLOGINHISTORY where loginid = '" + LoginID + "' ORDER BY ID DESC ) SELECT iif(ip = '" + IP + "', 0, 1) FROM LastRow;");
    }
}