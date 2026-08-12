using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for Wallet
/// </summary>
public class Wallet : IWallet
{
    cls_connection cls;
    public Wallet()
    {
        cls = new cls_connection();
    }

    public async Task<decimal> GetAEPSBalance(string Msrno)
    {
        return await cls.selectValueAsDecimal("select AEPSBalance from VIEW_MEMBERBALANCE where Msrno=" + Msrno + "");
    }

    public async Task<DataTable> GetBalance(string Msrno)
    {
        return await cls.select_data_dtAsync("select * from VIEW_MEMBERBALANCE where Msrno=" + Msrno + "");
    }

    public async Task<decimal> GetMainBalance(string Msrno)
    {
        return await cls.selectValueAsDecimal("select BALANCE from VIEW_MEMBERBALANCE where Msrno=" + Msrno + "");
    }

    public async Task<DataTable> GetTransactionListAEPSWallet(string OrderID)
    {
        return await cls.select_data_dtAsync("select * from TBL_AEPSWALLETTRANSACTION where TransactionID='" + OrderID + "' and IsReversed=0");
    }


    public async Task<MemberAllowData> GetMemberAllowPermissionDetails(string msrno)
    {
        DataTable dthis = await cls.select_data_dtAsync(
            "SELECT * FROM VIEW_MEMBERDETAILS WHERE Msrno='" + msrno + "'"
        );

        if (dthis == null || dthis.Rows.Count == 0)
            return null;

        DataRow row = dthis.Rows[0];

        return new MemberAllowData
        {
            HoldUnHold = row["OnHold"] != DBNull.Value && Convert.ToBoolean(row["OnHold"]),
            IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
            KYC = row["IsKycApproved"] != DBNull.Value && Convert.ToBoolean(row["IsKycApproved"]),
            FirstTimeLogin = row["FirstLiveLogin"] != DBNull.Value && Convert.ToBoolean(row["FirstLiveLogin"]),

            MainBalance = row["MainBalance"] != DBNull.Value ? Convert.ToDecimal(row["MainBalance"]) : 0,
            AEPSBalance = row["AepsBalance"] != DBNull.Value ? Convert.ToDecimal(row["AepsBalance"]) : 0,
            latitude = row["Latitude"] != DBNull.Value ? Convert.ToDecimal(row["Latitude"]) : 0,
            longitude = row["Longitude"] != DBNull.Value ? Convert.ToDecimal(row["Longitude"]) : 0,
            HoldAMT = row["HoldAMT"] != DBNull.Value ? Convert.ToDecimal(row["HoldAMT"]) : 0,

            ShopStateID = row["ShopState"] != DBNull.Value ? Convert.ToInt32(row["ShopState"]) : 0,
            StateID = row["StateID"] != DBNull.Value ? Convert.ToInt32(row["StateID"]) : 0,
            PackgeID = row["PackageID"] != DBNull.Value ? Convert.ToInt32(row["PackageID"]) : 0,
            Msrno = row["Msrno"] != DBNull.Value ? Convert.ToInt32(row["Msrno"]) : 0,

            ShopName = Convert.ToString(row["ShopName"]),
            Address = Convert.ToString(row["Address"]),
            StateName = Convert.ToString(row["StateName"]),
            CityName = Convert.ToString(row["CityName"]),
            Pan = Convert.ToString(row["Pan"]),
            RoleID = Convert.ToString(row["RoleID"]),
            Name = Convert.ToString(row["Name"]),
            Pincode = Convert.ToString(row["Pincode"]),
            Mobile = Convert.ToString(row["Mobile"]),
            Email = Convert.ToString(row["Email"]),
            DOB = Convert.ToString(row["DOB"]),
            Token = Convert.ToString(row["AppToken"]),
            LoginPin = Convert.ToString(row["LoginPin"]),
            LoginID = Convert.ToString(row["LoginID"]),
            ParentID = row["parentid"] != DBNull.Value
                    ? row["parentid"].ToString().ToInt32()
                    : 0
        };
    }

    public async Task<MemberAllowData> GetMemberAllowPermissionDetailsByLoginID(string Msrno)
    {
        DataTable dthis = await cls.select_data_dtAsync(
            "select * from VIEW_MEMBERDETAILS where loginid='" + Msrno + "' or mobile='" + Msrno + "'"
        );

        if (dthis != null && dthis.Rows.Count > 0)
        {
            DataRow row = dthis.Rows[0];

            return new MemberAllowData
            {
                HoldUnHold = row["OnHold"] != DBNull.Value && Convert.ToBoolean(row["OnHold"]),
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                KYC = row["IsKycApproved"] != DBNull.Value && Convert.ToBoolean(row["IsKycApproved"]),
                FirstTimeLogin = row["FirstLiveLogin"] != DBNull.Value && Convert.ToBoolean(row["FirstLiveLogin"]),

                MainBalance = row["MainBalance"] != DBNull.Value ? Convert.ToDecimal(row["MainBalance"]) : 0,
                AEPSBalance = row["AepsBalance"] != DBNull.Value ? Convert.ToDecimal(row["AepsBalance"]) : 0,
                latitude = row["Latitude"] != DBNull.Value ? Convert.ToDecimal(row["Latitude"]) : 0,
                longitude = row["Longitude"] != DBNull.Value ? Convert.ToDecimal(row["Longitude"]) : 0,
                HoldAMT = row["HoldAMT"] != DBNull.Value ? Convert.ToDecimal(row["HoldAMT"]) : 0,

                ShopName = Convert.ToString(row["ShopName"]),
                Name = Convert.ToString(row["Name"]),
                Pincode = Convert.ToString(row["Pincode"]),
                Address = Convert.ToString(row["Address"]),
                StateName = Convert.ToString(row["StateName"]),
                AadharNo = Convert.ToString(row["Aadhar"]),
                Pan = Convert.ToString(row["Pan"]),
                DOB = Convert.ToString(row["DOB"]),
                RoleID = Convert.ToString(row["RoleID"]),
                Mobile = Convert.ToString(row["Mobile"]),
                Email = Convert.ToString(row["Email"]),
                Token = Convert.ToString(row["AppToken"]),
                LoginPin = Convert.ToString(row["LoginPin"]),
                LoginID = Convert.ToString(row["LoginID"]),

                PackgeID = row["PackageID"] != DBNull.Value
                    ? row["PackageID"].ToString().ToInt32()
                    : 0,

                Msrno = row["Msrno"] != DBNull.Value
                    ? row["Msrno"].ToString().ToInt32()
                    : 0,
                ParentID = row["parentid"] != DBNull.Value
                    ? row["parentid"].ToString().ToInt32()
                    : 0
            };
        }

        return null;
    }

    public async Task<DataTable> GetMemberDetails(string Msrno)
    {

        return await cls.select_data_dtAsync("select * from VIEW_MEMBERDETAILS where Msrno=" + Msrno + "");
    }

    public async Task<DataTable> GetMemberDetailsByLoginID(string LoginID)
    {
        return await cls.select_data_dtAsync("select * from VIEW_MEMBERDETAILS where mobile='" + LoginID + "' or loginid='" + LoginID + "'");
    }

    public async Task<string> GetOrderID()
    {
        return await Task.Run(() =>
        {
            // Get current date and time
            DateTime now = DateTime.Now;

            // Format date and time components
            string dateComponent = now.ToString("ddMMyy");
            string timeComponent = now.ToString("HHmmssfff");

            // Generate a random number as a unique identifier
            Random random = new Random();
            string uniqueIdentifier = random.Next(1000, 9999).ToString(); // You can adjust the range as needed

            // Combine components to form the UTR number
            string utrNumber = dateComponent + timeComponent + uniqueIdentifier;

            return utrNumber;
        });
    }

    public async Task<bool> MemberHoldOrNot(string Msrno)
    {
        return await cls.GetBool("select OnHold from Member where Msrno=" + Msrno + " and OnHold=1");
    }

    public async Task<DataTable> GetTransactionList(string OrderID)
    {
        return await cls.select_data_dtAsync("select * from TBL_EWALLETTRANSACTION where TransactionID='" + OrderID + "' and IsReversed=0");
    }

    public async Task<Int32> CheckCreditorNot(string Narration, string Msrno)
    {
        return await cls.select_data_scalar_intAsny("select count(*) from TBL_EWALLETTRANSACTION where Narration='" + Narration + "' and msrno=" + Msrno + "");
    }
}