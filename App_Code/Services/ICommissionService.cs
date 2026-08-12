using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ICommissionService
/// </summary>
public interface ICommissionService
{
    Task<Int32> GiveBBPSCommission(string TransID);
    Task<Int32> GiveDMTCommission(string TransID);
    Task<Int32> GiveDMTPPICommission(string TransID);
    
    Task<Int32> GiveUPITransferCommission(string TransID);
    Task<Int32> GivePayCommission(string TransID);
    Task<Int32> GiveAEPSCommission(string TransID);
    Task<Int32> GiveCCPayCommission(string TransID);
    Task<Int32> GiveAEPSAmount(string TransID, string LoginID, string Msrno);

}