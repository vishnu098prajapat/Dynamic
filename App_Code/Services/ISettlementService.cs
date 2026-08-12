using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ISettlementService
/// </summary>
public interface ISettlementService
{
    Task<DataTable> GetBankDetails(string MemberID);
    Task<String> Settlement(string MemberID,string BankID,string SType,string Mode,string Amount);

}