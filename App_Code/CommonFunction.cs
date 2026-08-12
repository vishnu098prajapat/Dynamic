using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for CommonFunction
/// </summary>
public class CommonFunction
{
    public static void CheckPurchaseService(int ServiceID,string Msrno,Page page)
    {
        cls_connection ObjConnection = new cls_connection();
        Int32 Val = ObjConnection.select_data_scalar_int("select count(*) from MEMBERSERVICE where MSRNO="+ Msrno + " and "+ ServiceID + " in(select * from dbo.Function_Split(service,','))");
        if (Val == 0)
        {
            //ErrorShow.AlertMessageWithRedirect(page, "You are not authorized. So please Active your Service", "ActiveService.aspx");
        }
    }

    public static bool IsValidAadharNumber(string aadharNumber)
    {
        // Check if Aadhar number is 12 digits
        if (aadharNumber.Length != 12)
        {
            return false;
        }

        // Perform a simple checksum verification
        int checksum = int.Parse(aadharNumber[11].ToString());
        int sum = 0;

        for (int i = 0; i < 11; i++)
        {
            int digit = int.Parse(aadharNumber[i].ToString());
            sum = (sum + digit) % 10;
            sum = (sum * 2) % 10;
        }

        return (sum + checksum) % 10 == 0;
    }
}