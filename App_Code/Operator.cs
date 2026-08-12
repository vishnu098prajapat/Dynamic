using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for Operator
/// </summary>
public class Operator
{
    public Operator()
    {
        //CreateDate = new DateTime();
        //UpdatedDate = new DateTime();       
        ID = 0;
    }
    public int ID { get; set; }
    public string Name { get; set; }
    public string OperatorCode { get; set; }
    public int ServiceID { get; set; }

    public bool IsActive { get; set; }
    
    //public System.DateTime CreateDate { get; set; }
    //public System.DateTime UpdatedDate { get; set; }
   

    public bool IsAnyNullOrEmpty(object myObject)
    {
        foreach (PropertyInfo pi in myObject.GetType().GetProperties())
        {
            if (pi.PropertyType == typeof(string))
            {
                string value = ""; //if (string.IsNullOrEmpty(value));
                if (string.IsNullOrEmpty(value))
                {
                    return true; //false
                }
            }
        }
        return true;

    }

}
