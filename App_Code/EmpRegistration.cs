using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
/// <summary>
/// Summary description for EmpRegistration
/// </summary>
public class EmpRegistration
{
    public EmpRegistration()
    {
        ID = 0;
        Age = 21;
        CountryID = 1;
        StateID = 1;
        CityID = 1;
        PinCode = "302020";
    }

    public int ID { get; set; }
    public int RoleID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public int Age { get; set; }
    public int CountryID { get; set; }
    public int StateID { get; set; }
    public int CityID { get; set; }
    public string PinCode { get; set; }

    public string Password { get; set; }
    public string TransactionPassword { get; set; }
    public string Address { get; set; }
    public bool IsActive { get; set; }
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