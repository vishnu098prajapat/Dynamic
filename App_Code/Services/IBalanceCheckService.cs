using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IBalanceCheckService
/// </summary>
public interface IBalanceCheckService
{
    Task<String> GetBalance(string APIID);
}