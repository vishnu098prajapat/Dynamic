using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for Bank
/// </summary>
public class Bank : ICheckValue
{
    public Bank()
    {
        //CreateDate = new DateTime();
        //UpdatedDate = new DateTime();
        IsDelete = false;
        Class = "fa fa-university";
        ID = 0;
    }
    public int ID { get; set; }
    public string Name { get; set; }
    public string IFSC { get; set; }
    public string Class { get; set; }
    public bool IsActive { get; set; }
    public bool IsDelete { get; set; }

    //public System.DateTime CreateDate { get; set; }
    //public System.DateTime UpdatedDate { get; set; }    

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