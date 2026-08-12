using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IQueryRemitter
/// </summary>
public interface IQueryRemitter
{
    Task<CommonSearchResult> FinoBankSoniTechnoQueryRemitter(CommonSearch commonSearch);
    Task<CommonSearchResult> PayoutDMTQueryRemitter(CommonSearch commonSearch);
    Task<CommonSearchResult> DMTPayoutDMTQueryRemitter(CommonSearch commonSearch);
    Task<CommonSearchResult> PayoutDMTWithOTP(CommonSearchWithOTP commonSearch);
}