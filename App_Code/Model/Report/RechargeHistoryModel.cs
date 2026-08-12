using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for RechargeHistoryModel
/// </summary>
namespace ReportModel
{
    public class RechargeHistoryModelRoot
    {
        public List<RechargeHistoryModel> ListOfItems { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public decimal TotalSuccess { get; set; }
        public decimal TotalPending { get; set; }
        public decimal TotalFailed { get; set; }
        public int TotalSCount { get; set; }
        public int TotalPCount { get; set; }
        public int TotalFCount { get; set; }

    }

    public class RechargeHistoryModel
    {
        public int MEMBERID { get; set; }
        public int APIID { get; set; }
        public bool Dispute { get; set; }
        public bool ReSolve { get; set; }
        public bool RejectDispute { get; set; }
        public string ServiceName { get; set; }
        public string OperatorName { get; set; }
        public int OperatorID { get; set; }
        public string LOGINID { get; set; }
        public string APINAME { get; set; }
        public string MemberName { get; set; }
        public string CreatedDate { get; set; }
        public string TransID { get; set; }
        public string MobileNo { get; set; }
        public string Status { get; set; }
        public string APIMessage { get; set; }
        public string img { get; set; }
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
    }
}