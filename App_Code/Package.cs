using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for Package
/// </summary>
public class Package : ICheckValue
{
    public Package()
    {
        //CreateDate = new DateTime();
        //UpdatedDate = new DateTime();        
        ID = 0;
        Price = 0;
    }
    public int ID { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }  
    //public System.DateTime CreateDate { get; set; }
    //public System.DateTime UpdatedDate { get; set; }
    public decimal Price { get; set; }

    public bool IsAnyNullOrEmpty(object myObject)
    {
        //foreach (PropertyInfo pi in myObject.GetType().GetProperties())
        //{
        //    if (pi.PropertyType == typeof(string))
        //    {
        //        string value = (string)pi.GetValue(myObject);
        //        if (string.IsNullOrEmpty(value))
        //        {
        //            return false;
        //        }
        //    }
        //}
        return true;

    }
}