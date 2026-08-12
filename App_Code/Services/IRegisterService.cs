using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IRegisterService
/// </summary>
public interface IRegisterService
{
    Task<string> Register(Registration registration,string Source,string CompanyID);
    Task<Int32> CheckNumber(string Number);
    Task<Int32> PackageID(string RoleID, string CompanyID);
}