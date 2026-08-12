using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IAddFundService
/// </summary>
public interface IAddFundService
{
    Task<string> AddFund(string FromMeberID, string ToLoginID, decimal Amount, string Remark);
    Task<string> DeductFund(string FromMeberID, string ToLoginID, decimal Amount, string Remark);
    Task<string> DeductSettlement(string OrderID);
    Task<string> AddFundAEPSToMain(string FromMeberID, string ToLoginID, decimal Amount, string Remark);
}