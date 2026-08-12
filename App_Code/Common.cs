using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for Common
/// </summary>
public static class Common
{
    public static void BindDropDown(DropDownList ddl, DataTable dt, string Text, string Value)
    {
        ddl.DataSource = dt;
        ddl.DataTextField = Text;
        ddl.DataValueField = Value;
        ddl.DataBind();
    }

    public static int CheckServiceAssignOrNot(string Msrno, string ServiceID, cls_connection cls)
    {
        return cls.select_data_scalar_int("select  dbo.CheckServiceAssignOrNot(" + Msrno + "," + ServiceID + ")");
    }

    public static void BindRadioButtonList(RadioButtonList rdl, DataTable dt, string Text, string Value)
    {
        rdl.DataSource = dt;
        rdl.DataTextField = Text;
        rdl.DataValueField = Value;
        rdl.DataBind();
    }
    public static void BindServiceList(Repeater rpt, string Msrno, cls_connection cls)
    {
        DataTable dt = cls.select_data_dt("FillDropDown 'ActiveServiceMobile'," + Msrno + "");
        if (dt.Rows.Count > 0)
        {
            rpt.DataSource = dt;
            rpt.DataBind();
        }
        else
        {
            rpt.DataSource = null;
            rpt.DataBind();
        }

    }

    public static Tuple<bool, string> GetServiceStatus(string ID, cls_connection cls)
    {
        DataTable dtActivePayoutAPI = cls.select_data_dt("select * from Service where ID=" + ID + " and IsActive=1");
        if (dtActivePayoutAPI.Rows.Count > 0)
        {
            if (Convert.ToBoolean(dtActivePayoutAPI.Rows[0]["Onoff"]))
            {
                return Tuple.Create(true, "");
            }
            else
            {
                return Tuple.Create(false, dtActivePayoutAPI.Rows[0]["Reason"].ToString());
            }
        }
        else
        {
            return Tuple.Create(false, "Service Not Active So Can not Use");
        }

    }

    public static string StateData()
    {
        string result = API_Call_GET("http://uat.dhansewa.com/Common/GetState");
        return result;
    }

    public static string API_Call_GET(string url)
    {
        try
        {

            using (var client = new System.Net.WebClient())
            {
                string result = client.DownloadString(url);
                return result;
            }
        }
        catch
        {
            return "0";
        }
    }

    public static string copyFrom(NameValueCollection a)
    {
        IDictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var k in a.AllKeys)
        {
            dict.Add(k, a[k]);
        }
        string myJsonString = (new JavaScriptSerializer()).Serialize(dict);
        return myJsonString;

    }

    public static Tuple<bool, string, string> CheckAuthForTransaction(MemberPermission mem, TextBox txtOTP, Label label, string Mobile, string CompanyID,Page page)
    {
        if (mem != null)
        {
            if (mem.TwoWay)
            {
                if (mem.IsTpin)
                {
                    label.Text = "Your TPIN";
                    txtOTP.MaxLength = 4;
                    //txtOTP.Text = "Enter Your Transaction PIN";
                    txtOTP.Attributes["placeholder"] = "Enter Your Transaction PIN";
                    ErrorShow.SuccessNotify(page, "Enter Your Transaction PIN");
                    return Tuple.Create(true, "", "");
                }
                else if (mem.IsOTP)
                {
                    Random generator = new Random();
                    int r = generator.Next(1000, 9999);
                    string OTP = r.ToString();
                    string[] ValueArray = new string[2];
                    ValueArray[0] = "User";
                    txtOTP.MaxLength = 4;
                    ValueArray[1] = OTP;
                    HttpContext.Current.Session["AddFundMemberOTP"] = OTP;
                    string str = SMS.SendWithV(Mobile, ConstantsData.Admin_Fund_Add_OTP_SMS, ValueArray, CompanyID);
                    ErrorShow.SuccessNotify(page, "OTP send on Your Register Mobile Number");
                    label.Text = "Your OTP";
                   // txtOTP.Text = "Enter OTP send on Your Number";
                    txtOTP.Attributes["placeholder"] = "Enter OTP send on Your Number";
                    return Tuple.Create(true, "", "");
                }
                else if (mem.IsGoogleAuth)
                {
                    label.Text = "Authenticator Code";
                    txtOTP.MaxLength = 6;
                    //txtOTP.Text = "Please Enter Your google Authenticator Code";
                    txtOTP.Attributes["placeholder"] = "Please Enter Your google Authenticator Code";

                    ErrorShow.SuccessNotify(page, "Your google Authenticator Code Check in Your Mobile");
                    return Tuple.Create(true, "", "");
                }
                else
                {
                    return Tuple.Create(true, "", "");
                }
            }
            else
            {
                return Tuple.Create(false, "", "");
            }
        }
        else
        {
            return Tuple.Create(false, "", "");
        }
    }


    public static Tuple<bool, string, string> CheckAuthForDeductTransaction(MemberPermission mem, TextBox txtOTP, Label label, string Mobile, string CompanyID, Page page)
    {
        if (mem != null)
        {
            if (mem.TwoWay)
            {
                if (mem.IsTpin)
                {
                    label.Text = "Your TPIN";
                    txtOTP.MaxLength = 4;
                    //txtOTP.Text = "Enter Your Transaction PIN";
                    txtOTP.Attributes["placeholder"] = "Enter Your Transaction PIN";
                    ErrorShow.SuccessNotify(page, "Enter Your Transaction PIN");
                    return Tuple.Create(true, "", "");
                }
                else if (mem.IsOTP)
                {
                    Random generator = new Random();
                    int r = generator.Next(1000, 9999);
                    string OTP = r.ToString();
                    string[] ValueArray = new string[2];
                    ValueArray[0] = "User";
                    txtOTP.MaxLength = 4;
                    ValueArray[1] = OTP;
                    HttpContext.Current.Session["AddFundMemberOTP"] = OTP;
                    string str = SMS.SendWithV(Mobile, 18, ValueArray, CompanyID);
                    ErrorShow.SuccessNotify(page, "OTP send on Your Register Mobile Number");
                    label.Text = "Your OTP";
                    // txtOTP.Text = "Enter OTP send on Your Number";
                    txtOTP.Attributes["placeholder"] = "Enter OTP send on Your Number";
                    return Tuple.Create(true, "", "");
                }
                else if (mem.IsGoogleAuth)
                {
                    label.Text = "Authenticator Code";
                    txtOTP.MaxLength = 6;
                    //txtOTP.Text = "Please Enter Your google Authenticator Code";
                    txtOTP.Attributes["placeholder"] = "Please Enter Your google Authenticator Code";

                    ErrorShow.SuccessNotify(page, "Your google Authenticator Code Check in Your Mobile");
                    return Tuple.Create(true, "", "");
                }
                else
                {
                    return Tuple.Create(true, "", "");
                }
            }
            else
            {
                return Tuple.Create(false, "", "");
            }
        }
        else
        {
            return Tuple.Create(false, "", "");
        }
    }




    public static string encrypt(string Request)
    {
        using (
            AesManaged aes = CreateAes(safepay.MerchantKey))
        {
            ICryptoTransform encryptor = aes.CreateEncryptor();
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                        sw.Write(Request);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }


    private static AesManaged CreateAes(string key)
    {
        var aes = new AesManaged();
        aes.Key = Convert.FromBase64String(key); //UTF8-Encoding
        aes.IV = System.Text.Encoding.UTF8.GetBytes("0123456789abcdef");//UT8-Encoding
        return aes;
    }


    public static string decrypt(string text)
    {
        using (var aes = CreateAes(safepay.MerchantKey))
        {
            ICryptoTransform decryptor = aes.CreateDecryptor();
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

        }
    }

    public static bool CheckVAerifyAuth(MemberPermission permission, string OTP, string TY,Page page)
    {
        if (permission.IsTpin)
        {
            if (OTP == TY)
            {
                return true;
            }
            else
            {
                ErrorShow.ErrorNotify(page, "Transaction Pin not Match");
                return false;
            }
        }
        else if (permission.IsOTP)
        {
            if (OTP == HttpContext.Current.Session["AddFundMemberOTP"].ToString())
            {
                HttpContext.Current.Session["AddFundMemberOTP"] = null;
                return true;
            }
            else
            {
                ErrorShow.ErrorNotify(page, "OTP Not Match");
                return false;
            }
        }
        else if (permission.IsGoogleAuth)
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            if (tfa.ValidateTwoFactorPIN(permission.AuthKey, OTP))
            {
                return true;
            }
            else
            {
                ErrorShow.ErrorNotify(page, "Google Authenticator Not Match");
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public static Tuple<bool, string, string> CheckAuthForTransactionTPINOnly(MemberPermission mem, TextBox txtOTP, Label label, string Mobile, string CompanyID, Page page)
    {
        if (mem != null)
        {
            if (mem.TwoWay)
            {
                if (mem.IsTpin)
                {
                    label.Text = "Your TPIN";
                    txtOTP.MaxLength = 4;
                    txtOTP.Attributes["placeholder"] = "Enter Your Transaction PIN";
                    txtOTP.TextMode = TextBoxMode.Password;
                    ErrorShow.SuccessNotify(page, "Enter Your Transaction PIN");
                    return Tuple.Create(true, "", "");
                }
                else
                {
                    return Tuple.Create(true, "", "");
                }
            }
            else
            {
                return Tuple.Create(false, "", "");
            }
        }
        else
        {
            return Tuple.Create(false, "", "");
        }
    }

    public static bool CheckVerifyAuthTPINOnly(MemberPermission permission, string enteredTPIN, string actualTPIN, Page page)
    {
        if (permission == null)
        {
            ErrorShow.ErrorNotify(page, "Permission not found");
            return false;
        }

   
        if (permission.TwoWay && permission.IsTpin)
        {

            if (string.IsNullOrEmpty(enteredTPIN) || string.IsNullOrWhiteSpace(enteredTPIN))
            {
                ErrorShow.ErrorNotify(page, "Please enter Transaction PIN");
                return false;
            }

  
            if (enteredTPIN.Trim() == actualTPIN.Trim())
            {
                return true;
            }
            else
            {
                ErrorShow.ErrorNotify(page, "Invalid Transaction PIN");
                return false;
            }
        }
        else
        {
       
            return true;
        }
    }

    public static void SendMail(string ToMailID, string CC, string Subject, string Message,string Attachment)
    {
        DataTable dt = new DataTable();
        dt = new cls_connection().select_data_dt("SELECT * FROM MAILSetting where isactive=1");
        if (dt.Rows.Count > 0)
        {
            MailMessage msgs = new MailMessage();
            msgs.To.Add(ToMailID);
            msgs.CC.Add(CC);
            Attachment = HttpContext.Current.Server.MapPath("~/images/" + Attachment);//  MapPath("../images/"+ Attachment);
            if (File.Exists(Attachment))
            {
                Attachment attachment = new Attachment(Attachment);
                msgs.Attachments.Add(attachment);
            }

            MailAddress address = new MailAddress(Convert.ToString(dt.Rows[0]["FromMail"]));
            msgs.From = address;
            msgs.Subject = Subject;
            string htmlBody = Message;
            msgs.Body = htmlBody;
            msgs.IsBodyHtml = true;
            SmtpClient client = new SmtpClient();
            client.Host = Convert.ToString(dt.Rows[0]["SMTP"]).Trim();
            client.Port = Convert.ToInt32(dt.Rows[0]["Port"].ToString());
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(Convert.ToString(dt.Rows[0]["FromMail"]).Trim(), Convert.ToString(dt.Rows[0]["Password"]).Trim());
            client.Send(msgs);
        }
    }

    public static void SendMailStatic(string ToMailID, string Subject, string Message)
    {
        MailMessage msgs = new MailMessage();
        msgs.To.Add(ToMailID);
        msgs.CC.Add("dd@gmail.com");
        MailAddress address = new MailAddress("infdo@svvpay.in");
        msgs.From = address;
        msgs.Subject = Subject;
        string htmlBody = Message;
        msgs.Body = htmlBody;
        msgs.IsBodyHtml = true;
        SmtpClient client = new SmtpClient();
        client.Host = "smtpout.secureserver.net";
        client.Port = 25;
        client.UseDefaultCredentials = false;
        client.Credentials = new System.Net.NetworkCredential("info@svvpay.in", "Vishnu@2024");
        client.Send(msgs);
    }
}