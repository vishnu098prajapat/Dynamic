using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for Registration
/// </summary>
public class Registration :  IValidatableObject
{
    public Registration()
    {
        ID = 0;
    }
    
    public int ID { get; set; }
    [Required]
    public int RoleID { get; set; }
    [Required]
    public string CityName { get; set; }
    [Required]
    public int TitleID { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string ShopName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Mobile { get; set; }
    public string AlterNativeMobile { get; set; }
    [Required]
    public string Aadhar { get; set; }
    [Required]
    public string Pan { get; set; }
    [Required]
    public int GenderID { get; set; }
    [Required]
    public int LoginPin { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public string PinCode { get; set; }
    [Required]
    public string DOB { get; set; }
    public int CityID { get; set; }
    [Required]
    public int StateID { get; set; }
    [Required]
    public int PackageID { get; set; }
    public int ParentID { get; set; }
    public int SalesID { get; set; }
    public bool IsActive { get; set; }
    public string ShopAddress { get; set; }
    public int ShopStateID { get; set; }
    public string ShopCityName { get; set; }

    public string ShopPinCode { get; set; }

    public string PostOffice { get; set; }

    public string BusinessPostOffice { get; set; }

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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Pan))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}