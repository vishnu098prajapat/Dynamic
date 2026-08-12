using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SessionManager
/// </summary>
public class SessionManager
{
    //public static bool IsSession { get; set; }
    public static string RedirectUrlA = "Default.aspx";
    public static string RedirectUrlM = "Default.aspx";
    public SessionManager()
    {
        if (HttpRuntime.AppDomainAppId == null)
        {

        }
        //
        // TODO: Add constructor logic here
        //
    }
    public static DataTable CurrentSessionAdmin
    {
        get
        {
            DataTable session =
              (DataTable)HttpContext.Current.Session["dtEmployee"];

            return session;
        }
    }
    private static bool IsHttpRuntime()
    {
        if (HttpRuntime.AppDomainAppId == null)
        {
            throw new ArgumentNullException(@"HttpRuntime.AppDomainAppId is null.  SessionManager can only be used in a web application");
        }
        return true;
    }
    public static DataTable CurrentSession
    {
        get
        {
            DataTable session =
              (DataTable)HttpContext.Current.Session["dtMember"];

            return session;
        }
    }


    public static bool IsSession
    {
        get
        {
            if (HttpContext.Current.Session["dtMember"] != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }

    public static bool IsSessionAdmin
    {
        get
        {
            if (HttpContext.Current.Session["dtEmployee"] != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}