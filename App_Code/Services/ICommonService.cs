using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ICommonService
/// </summary>
public interface ICommonService
{
    Task<string> GETClientIP(HttpRequest context);
    Task<Int32> SaveLogs(LogsRequestModel logsRequestModel);
    Task<decimal> GetSurcharge(Int32 PackageID,decimal Amount,Int32 ServiceID);
    Task<DataTable> GetAEPSBankStatus(string BankName, string Msrno);
    Task<DataTable> GetRechargeHistory(string TransID);
    Task<Tuple<bool, string>> GetServiceStatus(string ServiceID);
}