using Dispute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IDisputeService
/// </summary>
public interface IDisputeService
{
    Task<ResponseBack> ApplyDispute(ApplyDispute applyDispute, string MemberID);
}