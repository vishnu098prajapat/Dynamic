using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for RegistrationMandatory
/// </summary>
public class RegistrationMandatory
{
 
    public bool IsName { set; get; } = true;
    public bool IsEmail { set; get; } = true;
    public bool IsMobile { set; get; } = true;
    public bool IsAlterNativeMobileNumber { set; get; } = true;
    public bool IsAddress { set; get; } = true;
    public bool IsAadhar { set; get; } = true;
    public bool IsPan { set; get; } = true;
    public bool IsPinCode { set; get; } = true;
    public bool IsDOB { set; get; } = true;
    public bool IsShopName { set; get; } = true;
    public bool IsCityName { set; get; } = true;
    public bool IsShopAddress { set; get; } = true;


   
    public static RegistrationMandatory GetPermission()
    {
        cls_connection cls = new cls_connection();
        string Result = cls.select_data_scalar_string("SELECT top  1 *  FROM RegistrationMandatory where ID=1 FOR JSON AUTO").Replace("[", "").Replace("]", "");
        RegistrationMandatory myDeserializedClass = JsonConvert.DeserializeObject<RegistrationMandatory>(Result);
        return myDeserializedClass;
    }
}