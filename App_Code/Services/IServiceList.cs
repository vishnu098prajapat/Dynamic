using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for IServiceList
/// </summary>
public interface IServiceList
{
    Task<DataTable> GetServiceData(string ServiceID);
    Task<ServiceWithAPIIDStatus> CheckServiceStatusWithAPIID(string ServiceID);

}


public class ServiceAllowData
{
    public bool IsActive { get; set; }
    public bool OnTime { get; set; }
    public decimal OffTime { get; set; }
    public bool Onoff { get; set; }
    public string Reason { get; set; }
    public Int32 APIID { get; set; }
}



public class ServiceWithAPIIDStatus
{
    public string Reason { get; set; }
    public string Status { get; set; }
    public string Price { get; set; }
    public Int32 APIID { get; set; }

}