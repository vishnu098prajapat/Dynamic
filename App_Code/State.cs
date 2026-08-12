using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for State
/// </summary>
public class State : ICheckValue
{
    public State()
    {
        //CreateDate = new DateTime();
        //UpdatedDate = new DateTime();
        IsDelete = false;
        ID = 0;
        CountryID = 26;
    }
    public int ID { get; set; }
    public string Name { get; set; }
    public string CircleCode { get; set; }
    public int CountryID { get; set; }
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