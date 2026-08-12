using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for Role
/// </summary>
public class Role : ICheckValue
{
    public Role()
    {
        IsDelete = false ;
        ID = 0;
    }
    public int ID { get; set; }
    public int ResCount { get; set; }
    public string Name { get; set; }
    public string StartID { get; set; }
    public bool IsActive { get; set; }
    public bool IsDelete { get; set; }
    public string Prefix { get; set; }

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
