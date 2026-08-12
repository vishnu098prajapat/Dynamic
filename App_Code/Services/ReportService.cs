using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using ReportModel;

/// <summary>
/// Summary description for ReportService
/// </summary>
public class ReportService<T> : IReportService<RechargeHistoryModelRoot>
{
    cls_connection cls;
    public ReportService()
    {
        cls = new cls_connection();
    }

    public async Task<RechargeHistoryModelRoot> GetReport(HistorySearchModel searchModeld,string MemberID)
    {
        DataTable dthis =await cls.select_data_dtAsync("GetPaginatedRechargeData "+ MemberID + "," + searchModeld.PageIndex + "," + searchModeld.PageSize + "," + searchModeld.SearchParams.Service + ",'" + searchModeld.SearchParams.FromDate + "','" + searchModeld.SearchParams.ToDate + "','" + searchModeld.SearchParams.Status + "','" + searchModeld.SearchParams.SearchByValue + "'");
        if (dthis.Rows.Count == 0)
        {
            // Return an empty result if no data is found
            return new RechargeHistoryModelRoot
            {
                ListOfItems = new List<RechargeHistoryModel>(),
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = 0,
                PageSize = searchModeld.PageSize,
                StartPage = 1,
                EndPage = 0,
                TotalSuccess = 0,
                TotalPending = 0,
                TotalFailed = 0,
                TotalSCount = 0,
                TotalPCount = 0,
                TotalFCount = 0
            };
        }
        else
        {
            return new RechargeHistoryModelRoot
            {
                ListOfItems = JsonConvert.DeserializeObject<List<RechargeHistoryModel>>(dthis.Rows[0]["ListOfItems"].ToString()),
                TotalItems = dthis.Rows[0]["TotalItems"].ToString().ToInt32(),
                TotalPages = dthis.Rows[0]["TotalPages"].ToString().ToInt32(),
                CurrentPage = dthis.Rows[0]["CurrentPage"].ToString().ToInt32(),
                PageSize = dthis.Rows[0]["PageSize"].ToString().ToInt32(),
                StartPage = 1,
                EndPage = dthis.Rows[0]["TotalItems"].ToString().ToInt32() / dthis.Rows[0]["PageSize"].ToString().ToInt32(),
                TotalSuccess = Convert.ToDecimal(dthis.Rows[0]["TotalSuccess"].ToString()),
                TotalPending = Convert.ToDecimal(dthis.Rows[0]["TotalPending"].ToString()),
                TotalFailed = Convert.ToDecimal(dthis.Rows[0]["TotalFailed"].ToString()),
                TotalSCount = Convert.ToInt32(dthis.Rows[0]["TotalSCount"].ToString()),
                TotalPCount = Convert.ToInt32(dthis.Rows[0]["TotalPCount"].ToString()),
                TotalFCount = Convert.ToInt32(dthis.Rows[0]["TotalFCount"].ToString())
            };
        }
    }
}



public class DMTReportService<T> : IReportService<DMTHistoryModelRoot>
{
    cls_connection cls;
    public DMTReportService()
    {
        cls = new cls_connection();
    }

    public async Task<DMTHistoryModelRoot> GetReport(HistorySearchModel searchModeld, string MemberID)
    {
        DataTable dthis = cls.select_data_dt("GetPaginatedDMTData " + MemberID + "," + searchModeld.PageIndex + "," + searchModeld.PageSize + "," + searchModeld.SearchParams.Service + ",'" + searchModeld.SearchParams.FromDate + "','" + searchModeld.SearchParams.ToDate + "','" + searchModeld.SearchParams.Status + "','" + searchModeld.SearchParams.SearchByValue + "'");
        if (dthis.Rows.Count == 0)
        {
            // Return an empty result if no data is found
            return new DMTHistoryModelRoot
            {
                ListOfItems = new List<DMTHistoryModel>(),
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = 0,
                PageSize = searchModeld.PageSize,
                StartPage = 1,
                EndPage = 0,
                TotalSuccess = 0,
                TotalPending = 0,
                TotalFailed = 0,
                TotalSCount = 0,
                TotalPCount = 0,
                TotalFCount = 0
            };
        }
        else
        {
            return new DMTHistoryModelRoot
            {
                ListOfItems = JsonConvert.DeserializeObject<List<DMTHistoryModel>>(dthis.Rows[0]["ListOfItems"].ToString()),
                TotalItems = dthis.Rows[0]["TotalItems"].ToString().ToInt32(),
                TotalPages = dthis.Rows[0]["TotalPages"].ToString().ToInt32(),
                CurrentPage = dthis.Rows[0]["CurrentPage"].ToString().ToInt32(),
                PageSize = dthis.Rows[0]["PageSize"].ToString().ToInt32(),
                StartPage = 1,
                EndPage = dthis.Rows[0]["TotalItems"].ToString().ToInt32() / dthis.Rows[0]["PageSize"].ToString().ToInt32(),
                TotalSuccess = Convert.ToDecimal(dthis.Rows[0]["TotalSuccess"].ToString()),
                TotalPending = Convert.ToDecimal(dthis.Rows[0]["TotalPending"].ToString()),
                TotalFailed = Convert.ToDecimal(dthis.Rows[0]["TotalFailed"].ToString()),
                TotalSCount = Convert.ToInt32(dthis.Rows[0]["TotalSCount"].ToString()),
                TotalPCount = Convert.ToInt32(dthis.Rows[0]["TotalPCount"].ToString()),
                TotalFCount = Convert.ToInt32(dthis.Rows[0]["TotalFCount"].ToString())
            };
        }
    }
}