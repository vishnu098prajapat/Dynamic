using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for Service
/// </summary>
public class Service
{
    public Service()
    {
        //CreateDate = new DateTime();
        //UpdatedDate = new DateTime();    
        ID = 0;
    }
    public int ID { get; set; }
    public string Name { get; set; }
    public string URL { get; set; }
    public string ImageURL { get; set; }
    public string Price { get; set; }
    public int SectionTypeID { get; set; }
    public bool IsActive { get; set; }
    public bool IsDelete { get; set; }
    //public System.DateTime CreateDate { get; set; }
    //public System.DateTime UpdatedDate { get; set; }
    

    public bool IsAnyNullOrEmpty(object myObject)
    {
        foreach (PropertyInfo pi in myObject.GetType().GetProperties())
        {
            //if (pi.PropertyType == typeof(string))
            //{
            //    string value = ""; //if (string.IsNullOrEmpty(value));
            //    if (string.IsNullOrEmpty(value))
            //    {
            //        return true;//false
            //    }
            //}
        }
        return true;

    }
}