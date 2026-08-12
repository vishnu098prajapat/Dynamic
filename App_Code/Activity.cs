using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Activity
/// </summary>
public class Activity
{
    public static void AddActivity(string EMPID,string ServiceID,string TransID,string Description,string Msrno,string Status,cls_connection cls)
    {
        cls.update_data("AddEditActivityEmp '"+EMPID+ "','" + ServiceID + "','" + TransID + "','" + Description + "','" + Msrno + "','" + Status + "'");
    }

    public Activity()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}