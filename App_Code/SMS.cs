using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Data;
using RestSharp;
using System.Threading.Tasks;

public static class SMS
{

    static string[] arrTemplate = new string[]
    {
        "Zero",
        "Your current balance is @v0@. Thanks",//1
        "Your Number @v0@ is successfully recharged with amount @v1@. TxID is @v2@. Thanks",//2
        "Recharge amount @v1@ for number @v0@ is failed. TxID is @v2@. Thanks",//3
        "Recharge amount @v1@ for number @v0@ is pending. TxID is @v2@. Thanks",//4
        "You have successfully transfer amount @v1@ to number @v0@. Thanks",//5
        "Your mobile no successfully changed. New mobile no is @v0@. Thanks",//6
        "Status of TxID @v0@ is @v1@ Thanks.",//7
        "Dear Member, Last recharges are @v0@. Thanks",//8--Done
        "Dear  {#var0#} Welcome to Star Pay Digital Seva your user id  {#var1#} Password is  {#var2#} and Login Pin is  {#var3#}. Star Pay Digital Seva",//9
        //"Dear @v0@, Welcome to our Company, You have registered successfully, Now you can login.",//9
        "You have successfully credit Rs. @v0@, Now your total amount is Rs. @v1@. Thanks",//10
        "Dear @v0@, Welcome to our Company, Your Student Registration successfully done, Login with userid:@v1@ and password:@v2@.",//11    
        "Dear @v0@, Welcome to our Company, Your password recover successfully, Login with user-id:@v1@ and password @v2@. Thanks",//12
        "You have a new fund request of Rs. @v0@ from @v1@. Thanks",//13
        "Dear @v0@, Welcome to the Our Company, Your Id : @v1@, Password : @v2@. Thanks",//14
        "You have successfully credit Rs.  {#var0#}, Now your total amount is Rs.  {#var1#}. Thanks", //15
        "Dear Customer, Your Number @v0@ is successfully recharged with amount @v1@, TxID is @v2@, Current Bal @v3@. Thanks", //16
        "Invalid Request, Please try again.", //17
        "System Error, Please try again later.", //18
        "Your account has been debited with amount @v1@ by @v0@. Thanks",//19
        "You have successfully Debit with Rs. @v0@, Now your total amount is Rs. @v1@. Thanks",//20                
        "{#var1#} is the OTP for Star Pay Digital Seva Registration. Don't share to any one. Star Pay Digital Seva",//21        
        "Hello @v0@, I am using wablepaymentsolution.com Join to rechage fast and smoothly. Thanks by @v1@", //22
        "Congratulation! Your friend @v0@ successfully joined us. You are rewarded with @v1@ points.", //23
        "You have successfully credit Points. @v0@ for redeem  @v1@ Points.", //24
        "Dear @v0@, Recahrge of Rs @v1@ on @v2@ has been refunded. Your current balance is @v3@.", //25
        "Dear @v0@ , Your password is : @v1@",//26
        "You have successfully  added in your wallet.Your Current EWallet Balance  is Rs. @v1@. Thanks", //27
        "You have successfully  successfully deducted Rs. @v0@, Now Your Current EWallet Balance  is Rs. @v1@. Thanks", //28
        "Dear @v0@ , your fund request for amount @v1@ has been approved. Thanks ", //29
        "You have successfully Sending Force Success Request and Your amount is @v0@. Thanks", //30
        "You have successfully Sending Force Failed Request and Your amount is @v0@. Thanks", //31
        "Dear Customer, Rs. @v0@ has been successfully added in your wallet through Payment Gateway.", //32
        "Dear @v0@, Mobile No. @v1@ is recharged with Amount @v2@. TxID is @v3@. Thanks", //33
        "Dear @v0@, Admin sent a message to you-. Subject- @v1@. Message-@v2@ Thanks", //34
        "Dear @v0@, Do not share your login otp for portal security. @v1@ OTP to accessing your Account.Powered by IDS Pay.", //35
        "Dear @v0@, Do not share your login Details for portal security. @v1@ Password and @v2@ LoginPin.Powered by IDS Pay.", //36
        "{#var1#} is the OTP for Star Pay Digital Seva Registration. Don't share to any one. Star Pay Digital Seva",//37
        "Dear {#var0#}, Your Login OTP is @v1@ Keep it confidential for security reason. IDSPay",//38
        "Dear {#var0#}, Your doc for KYC is Approved. Star Pay Digital Seva",//39
        "Dear {#var0#}, Your {#var1#} doc for KYC is Rejected. Star Pay Digital Seva",//40
        "Dear Merchant, Your OTP is {#var1#} to Add Settlement A/c Keep it confidential for security reason. Star Pay Digital Seva ",//41
        "Dear {#var0#}, Your Verify OTP is {#var1#} Keep it confidential for security reason. IDSPay",//42
        "You are converted to Merchant, New Login details are Login id is @v1@, password is @v2@ and LoginPin is @v3@",//43
        "Dear Merchant, INR-@v0@  is added to your IDSPay wallet. Thank you for Choosing IDSPay",//44
    };

    public  static string SendWithV(string Mobile, Int32 TemCount, string[] ValueArray, string CompanyMemberID)
    {
        try
        {
            cls_connection objConnection = new cls_connection();
            DataTable dtSMSData = objConnection.select_data_dt("ManageSMSTemplate 'GetByID','" + CompanyMemberID + "'," + TemCount + "");
            if (dtSMSData.Rows.Count > 0)
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

                string Result = "", smsMessage="";
                if (Convert.ToBoolean(dtSMSData.Rows[0]["IsSMS"]))
                {
                    smsMessage = GetString(dtSMSData.Rows[0]["Template"].ToString(), ValueArray);
                    string baseurl = GetBaseURLAPI(smsMessage, Mobile, dtSMSData.Rows[0]["TemplateID"].ToString(),"1");
                    Result = SendSMS(baseurl);
                }

                if (Convert.ToBoolean(dtSMSData.Rows[0]["IsWhatsApp"]))
                {
                    smsMessage = GetString(dtSMSData.Rows[0]["WhatsAppTemplate"].ToString(), ValueArray);
                    string baseurl = GetBaseURLAPI(smsMessage, Mobile, dtSMSData.Rows[0]["TemplateID"].ToString(),"0");
                    Result = SendSMS(baseurl);
                }

                if (Convert.ToBoolean(dtSMSData.Rows[0]["IsEmail"]))
                {
                    
                }

                string[] split = Result.Trim().Split(' ');

                return split[0];
            }
            else
            {
                return "";
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }


    private static string SendSMS(string baseurl)
    {
        var options = new RestClientOptions(baseurl)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();

        var request = new RestRequest(baseurl, Method.Get);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static string GetBaseURL(string Message, string Mobile, string DLT)
    {
        cls_connection objConnection = new cls_connection();
        DataTable dtSMS = objConnection.select_data_dt("select * from SMSSetting where IsActive=1 and id>1");
        string BaseURL = "";
        if (dtSMS.Rows.Count > 0)
        {
            BaseURL = dtSMS.Rows[0]["URL"].ToString();
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["SenderText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["SenderText"].ToString(), "=", dtSMS.Rows[0]["Sender"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["CountryText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["CountryText"].ToString(), "=", dtSMS.Rows[0]["country"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["RouteText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["RouteText"].ToString(), "=", dtSMS.Rows[0]["route"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param1Text"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param1Text"].ToString(), "=", dtSMS.Rows[0]["Param1Val"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param2Text"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param2Text"].ToString(), "=", dtSMS.Rows[0]["Param2Val"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param3Text"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param3Text"].ToString(), "=", dtSMS.Rows[0]["country"].ToString() + Mobile);
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param3Val"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param3Val"].ToString(), "=", Message);
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["DltText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["DltText"].ToString(), "=", DLT);
            }
            return BaseURL;
        }
        else
        {
            return BaseURL;
        }

    }

    public static string GetBaseURLAPI(string Message, string Mobile, string DLT, string APIID)
    {
        cls_connection objConnection = new cls_connection();
        DataTable dtSMS = objConnection.select_data_dt("select * from SMSSetting where IsActive=1 and Integrationtype=" + APIID + "");
        string BaseURL = "";
        if (dtSMS.Rows.Count > 0)
        {
            BaseURL = dtSMS.Rows[0]["URL"].ToString();
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["SenderText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["SenderText"].ToString(), "=", dtSMS.Rows[0]["Sender"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["CountryText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["CountryText"].ToString(), "=", dtSMS.Rows[0]["country"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["RouteText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["RouteText"].ToString(), "=", dtSMS.Rows[0]["route"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param1Text"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param1Text"].ToString(), "=", dtSMS.Rows[0]["Param1Val"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param2Text"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param2Text"].ToString(), "=", dtSMS.Rows[0]["Param2Val"].ToString());
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param3Text"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param3Text"].ToString(), "=", dtSMS.Rows[0]["country"].ToString() + Mobile);
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["Param3Val"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["Param3Val"].ToString(), "=", Message);
            }
            if (!string.IsNullOrEmpty(dtSMS.Rows[0]["DltText"].ToString()))
            {
                BaseURL = string.Concat(BaseURL, "&", dtSMS.Rows[0]["DltText"].ToString(), "=", DLT);
            }
            return BaseURL;
        }
        else
        {
            return BaseURL;
        }

    }


    public static string SendAllSMS(string Mobile, string ValueArray)
    {
        try
        {
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            WebClient client = new WebClient();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

            string smsMessage = ValueArray;

            string baseurl = "http://smsp.myoperator.co/api/sendhttp.php?authkey=296499AjkLpvv8oR5d91a424&mobiles=" + Mobile + "&message=" + smsMessage + "&sender=IDSPAY&route=4&route=4&country=91";
            Stream data = client.OpenRead(baseurl);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            string[] split = s.Trim().Split(' ');
            return split[0];
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }



    public static string GetString(string Template, string[] ValueArray)
    {
        string fileData = Template;
        if ((ValueArray == null))
        {
            fileData = HttpUtility.UrlDecode(fileData);
            return fileData;
        }
        else
        {
            for (int i = ValueArray.GetLowerBound(0); i <= ValueArray.GetUpperBound(0); i++)
            {
                fileData = fileData.Replace("{#var" + i.ToString() + "#}", (string)ValueArray[i]);
            }
            fileData = HttpUtility.UrlDecode(fileData);
            return fileData;
        }
    }
    public static string GetStringMail(string Template, string[] ValueArray)
    {
        string fileData = Template;
        if ((ValueArray == null))
        {
            fileData = HttpUtility.UrlDecode(fileData);
            return fileData;
        }
        else
        {
            for (int i = ValueArray.GetLowerBound(0); i <= ValueArray.GetUpperBound(0); i++)
            {
                fileData = fileData.Replace("{#var" + i.ToString() + "#}", (string)ValueArray[i]);
            }
            fileData = HttpUtility.HtmlDecode(fileData);
            return fileData;
        }
    }

}