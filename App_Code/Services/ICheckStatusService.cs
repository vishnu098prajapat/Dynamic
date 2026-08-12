using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ICheckStatusService
/// </summary>
public interface ICheckStatusService
{
    Task<string> CheckStatusUPICollection(string OrderID);

    Task<string> CheckStatusDMT(string OrderID);
    Task<string> CheckStatusPaysettlement(string OrderID);
    Task<string> CheckStatusAEPSPaysettlement(string OrderID);
    Task<string> CheckStatusAEPS(string OrderID);
    Task<DataTable> GetDMTDetails(string OrderID);
    Task<DataTable> GetPaysetllementDetails(string OrderID);
    Task<string> CheckStatusInspay(string OrderID,string Date);
    Task<string> CheckStatusSafex(string OrderID,string VendorID);
    Task<Int32> ReviseDMT(string OrderID);
    Task<Int32> ReviseAEPSWallet(string OrderID);
}



public class CommonCheckStatusDMT
{
    public string Status { get; set; }
    public string OrderID { get; set; }
    public string VendorID { get; set; }
    public string RRN { get; set; }
    public string Reason { get; set; }
    public string Result { get; set; }
}


public class CommonCheckStatusAEPS
{
    public string Status { get; set; }
    public string TYPE { get; set; }
    public string OrderID { get; set; }
    public string VendorID { get; set; }
    public string RRN { get; set; }
    public string Reason { get; set; }
    public string Result { get; set; }
}
