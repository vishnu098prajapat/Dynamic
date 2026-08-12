using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Email
/// </summary>
public class Emails
{
    //public static void SendEmail(string templateName,params string[] arr)
    public static void SendEmail(string Name, string Email, string[] Data, string templateName)
    {

        string[] valueArray = new string[Data.Length];
        for (int i = 0; i < Data.Length; i++)
        {
            valueArray[i] = Data[i].ToString();
            // valueArray[1] = Email;
            // valueArray[2] = MobileNo;
            // valueArray[3] = Password;
            // valueArray[4] = TransactionPassword;
            // valueArray[5] = MemberID;
        }

        FlexiMail objSendMail = new FlexiMail();
        objSendMail.To = Email;
        objSendMail.CC = "";
        objSendMail.BCC = "";
        objSendMail.From = "management@sonitechno.com";
        objSendMail.FromName = "management@sonitechno.com";
        objSendMail.MailBodyManualSupply = false;
     
        objSendMail.MailBody = "hy";
        objSendMail.EmailTemplateFileName = templateName;
        objSendMail.Subject = "Registration";
        objSendMail.ValueArray = valueArray;
        objSendMail.Send();

    }
    public static void SendEmail(string Name, string Email, string Data, string templateName)
    {

        string[] valueArray = new string[Data.Length];
        for (int i = 0; i < Data.Length; i++)
        {
            valueArray[i] = Data[i].ToString();
            // valueArray[1] = Email;
            // valueArray[2] = MobileNo;
            // valueArray[3] = Password;
            // valueArray[4] = TransactionPassword;
            // valueArray[5] = MemberID;
        }

        FlexiMail objSendMail = new FlexiMail();
        objSendMail.To = Email;
        objSendMail.CC = "";
        objSendMail.BCC = "";
        objSendMail.From = Convert.ToString(ConfigurationManager.AppSettings["FromMail"]);
        objSendMail.FromName = Convert.ToString(ConfigurationManager.AppSettings["FromMail"]);
        objSendMail.MailBodyManualSupply = true;
        objSendMail.MailBody = Data;
        objSendMail.EmailTemplateFileName = templateName;
        objSendMail.Subject = templateName;
        objSendMail.ValueArray = valueArray;
        objSendMail.Send();

    }
}