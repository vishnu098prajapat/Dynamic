using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for HistorySearchModel
/// </summary>
/// 
namespace ReportModel
{
    public class HistorySearchModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public SearchParams SearchParams { get; set; }
    }


    public class SearchParams
    {
        public string SearchByValue { get; set; }
        public string AccountNo { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Status { get; set; }
        public string Operator { get; set; }
        public string Service { get; set; }
    }

   
}