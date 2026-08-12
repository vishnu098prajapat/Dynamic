using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IUPITransfer
/// </summary>
public interface IUPITransfer
{
    Task<string> UPIVerification(UPIVerfication uPIVerfication, string MemberID);
    Task<string> GETUPIIDName(string UPIID);
    Task<string> UPITransfer(UPITransferRequestST uPITransferRequestST, string MemberID,string Source);

}