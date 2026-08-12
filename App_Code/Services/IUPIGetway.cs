using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IUPIGetway
/// </summary>
public interface IUPIGetway
{
    Task<String> UPIRequest(string MemberID, Int32 Amount,string Source);
}