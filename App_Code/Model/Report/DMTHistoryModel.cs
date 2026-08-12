using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DMTHistoryModel
/// </summary>
/// 
namespace ReportModel
{
    public class DMTHistoryModelRoot
    {
        public List<DMTHistoryModel> ListOfItems { get; set; }
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


    public class DMTHistoryModel
    {
        public int MEMBERID { get; set; }
        public string ServiceName { get; set; }
        public int APIID { get; set; }
        public string From { get; set; }
        public string AccountNo { get; set; }
        public string IFSC { get; set; }
        public string BankName { get; set; }
        public string BeneficiaryData { get; set; }
        public string GroupID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string OrderID { get; set; }
        public string VendorID { get; set; }
        public string RRN { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string VerifyName { get; set; }
        public double Amount { get; set; }
        public double Surcharge { get; set; }
        public double GST { get; set; }
        public double TDS { get; set; }
        public double CashBack { get; set; }
        public double UplineCommission { get; set; }
        public string LoginID { get; set; }
        public string APIName { get; set; }
        public string MemberName { get; set; }
        public string CreatedDate { get; set; }
        public bool Dispute { get; set; }
        public bool ReSolve { get; set; }
        public bool RejectDispute { get; set; }
    }

}