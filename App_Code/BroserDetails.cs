using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for BroserDetails
/// </summary>
public class BroserDetails
{
    public string BrowserName { get; set; }
    public string BrowserVersion { get; set; }
    public string OS { get; set; }
    public BroserDetails()
    {

    }

    public static string getOperatinSystemDetails(string browserDetails)
    {
        try
        {
            switch (browserDetails.Substring(browserDetails.LastIndexOf("Windows NT") + 11, 3).Trim())
            {
                case "6.2":
                    return "Windows 8";
                case "6.1":
                    return "Windows 7";
                case "6.0":
                    return "Windows Vista";
                case "5.2":
                    return "Windows XP 64-Bit Edition";
                case "5.1":
                    return "Windows XP";
                case "5.0":
                    return "Windows 2000";
                default:
                    return browserDetails.Substring(browserDetails.LastIndexOf("Windows NT"), 14);
            }
        }
        catch
        {
            if (browserDetails.Length > 149)
                return browserDetails.Substring(0, 149);
            else
                return browserDetails;
        }
    }
}