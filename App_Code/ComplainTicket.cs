using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for ComplainTicket
/// </summary>
public class ComplainTicket
{
    public ComplainTicket()
    {
        ID = 0;
    }
    public int ID { get; set; } 
    public int ServiceID { get; set; }
    public string Message { get; set; }
    //
    // TODO: Add constructor logic here
    //
    public bool IsAnyNullOrEmpty(object myObject)
    {
        foreach (PropertyInfo pi in myObject.GetType().GetProperties())
        {
            if (pi.PropertyType == typeof(string))
            {
                string value = (string)pi.GetValue(myObject);
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
            }
        }
        return true;

    }
}