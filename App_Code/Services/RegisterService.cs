using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for RegisterService
/// </summary>
public class RegisterService :IRegisterService
{
    cls_connection cls;
    public RegisterService()
    {
        cls = new cls_connection();
    }

    public async Task<int> CheckNumber(string Number)
    {
       return await cls.select_data_scalar_intAsny("select count(*) from Member where Mobile='" + Number + "'");
    }

    public async Task<int> PackageID(string RoleID,string CompanyID)
    {
        return await cls.select_data_scalar_intAsny("select top 1 packageID from Role where ID='" + RoleID + "' and CompanyMemberID="+ CompanyID + "");

    }

    public async Task<string> Register(Registration registration, string Source, string CompanyID)
    {
        //registration.PackageID = await PackageID(registration.RoleID.ToString(), CompanyID);
        if (registration.PackageID > 0)
        {
            DataTable dtM = new DataTable();                       
            dtM = await cls.select_data_dtAsync("AddEditMember " + registration.ID + ",'" + registration.RoleID + "','" + registration.TitleID + "','" + registration.Name + "','" + registration.Email + "','" + registration.Mobile + "','" + registration.GenderID + "','" + registration.LoginPin + "','" + registration.Password + "','" + registration.Address + "','" + registration.CityID + "','" + registration.PackageID + "'," + registration.ParentID + ",'" + registration.AlterNativeMobile + "','" + registration.Aadhar + "','" + registration.Pan + "','" + registration.PinCode + "','" + registration.DOB + "','" + registration.ShopName + "','" + registration.CityName + "'," + registration.StateID + ",'" + CompanyID + "','0','0','" + Source + "','" + registration.ShopAddress + "','" + registration.ShopStateID + "','" + registration.ShopCityName + "','" + registration.SalesID + "','" + registration.ShopPinCode + "','"+registration.PostOffice+"','"+registration.BusinessPostOffice+"'");

            if (dtM.Rows.Count > 0)
            {
                if (dtM.Rows[0]["Status"].ToString() == "1")
                {
                    string[] ValueArray = new string[4];
                    ValueArray[0] = registration.Name;
                    ValueArray[1] = dtM.Rows[0]["Result"].ToString();
                    ValueArray[2] = registration.Password;
                    ValueArray[3] = registration.LoginPin.ToString();
                    SMS.SendWithV(registration.Mobile, ConstantsData.Registration_SMS, ValueArray, CompanyID);
                    return ErrorShow.SuccessMessageAPI("Your Login ID -" + dtM.Rows[0]["Result"].ToString() + "");
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI(dtM.Rows[0]["Result"].ToString());
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Try After some time");
            }
        }
        else
            return ErrorShow.ErrorMessageAPI("Package Not Assign On Role");
    }
}