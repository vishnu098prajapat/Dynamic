using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for StaticMethod
/// </summary>
public class StaticMethod
{
    public StaticMethod()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static DataTable GetMemberDetailsByServiceDetaisl(string AccountID,string Msrno)
    {
        cls_connection Objconnection = new cls_connection();
        return Objconnection.select_data_dt("ManageUserAccount 'GetAccountDetails'," + AccountID + ","+Msrno+"");
    }

    public static DataTable GetMemberDetailsByServiceDetaisl(string AccountID)
    {
        cls_connection Objconnection = new cls_connection();
        return Objconnection.select_data_dt("ManageUserAccount 'GetAccountDetailsByAccountNo',0,0,'" + AccountID + "'");
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
    public static string SuccessResponse(string Message)
    {
        string response = "{\"statuscode\":\"TXN\",\"status\":\"" + Message + "\"}";
        return response;
    }
    public static string SuccessArray(string json, string Message)
    {
        string response = "{\"statuscode\":\"TXN\",\"status\":\"success\",\"mess\":\"" + Message + "\",\"data\": " + json + "}";
        return response;
    }
    public static string ErrorResponse(string Message)
    {
        string response = "{\"statuscode\":\"ERR\",\"status\":\"" + Message + "\"}";
        return response;
    }


    public static string getipAddress()
    {

        string IpAddress = string.Empty;
        try
        {
            IpAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (IpAddress == " " || IpAddress == null)
            {
                IpAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                return IpAddress;
            }
            else
            {
                return IpAddress;
            }
        }
        catch (ArgumentNullException Ex)
        {
            return Ex.Message;
        }
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
