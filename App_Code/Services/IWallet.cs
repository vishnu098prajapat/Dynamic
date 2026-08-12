using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IWallet
/// </summary>
public interface IWallet
{
    Task<DataTable> GetBalance(string Msrno);
    Task<DataTable> GetMemberDetails(string Msrno);
    Task<DataTable> GetMemberDetailsByLoginID(string LoginID);
    Task<MemberAllowData> GetMemberAllowPermissionDetails(string Msrno);
    Task<MemberAllowData> GetMemberAllowPermissionDetailsByLoginID(string Msrno);
    Task<decimal> GetMainBalance(string Msrno);
    Task<decimal> GetAEPSBalance(string Msrno);
    Task<bool> MemberHoldOrNot(string Msrno);
    Task<string> GetOrderID();
    Task<DataTable> GetTransactionListAEPSWallet(string OrderID);
    Task<DataTable> GetTransactionList(string OrderID);
    Task<Int32> CheckCreditorNot(string Narration, string Msrno);

}


public class MemberAllowData
{
    public bool HoldUnHold { get; set; }
    public bool IsActive { get; set; }
    public bool KYC { get; set; }
    public bool FirstTimeLogin { get; set; }
    public decimal HoldAMT { get; set; }
    public decimal latitude { get; set; }
    public decimal longitude { get; set; }
    public decimal MainBalance { get; set; }
    public decimal AEPSBalance { get; set; }
    public string LoginID { get; set; }
    public string Address { get; set; }
    public string StateName { get; set; }
    public string CityName { get; set; }
    public string ShopName { get; set; }
    public string Name { get; set; }
    public string Pincode { get; set; }
    public string DOB { get; set; }
    public string Mobile { get; set; }
    public string RoleID { get; set; }
    public string AadharNo { get; set; }
    public string LoginPin { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public string Pan { get; set; }
    public Int32 PackgeID { get; set; }
    public Int32 ShopStateID { get; set; }
    public Int32 StateID { get; set; }
    public Int32 Msrno { get; set; }
    public Int32 ParentID { get; set; }
}