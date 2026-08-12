using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ServiceList
/// </summary>
public class ServiceList :IServiceList
{
    cls_connection cls;
    IAPIService aPIService;
    public ServiceList()
    {
        cls = new cls_connection();
        aPIService = new APIService();
    }

    public async Task<ServiceWithAPIIDStatus> CheckServiceStatusWithAPIID(string ServiceID)
    {
        ServiceWithAPIIDStatus serviceWithAPIIDStatus = new ServiceWithAPIIDStatus();
        DataTable dtService = await GetServiceData(ServiceID);
        if (dtService.Rows.Count > 0)
        {
            DataTable dtAPIDData = await aPIService.GetAPIIDData(dtService.Rows[0]["APIID"].ToString());
            if (dtAPIDData.Rows.Count > 0)
            {
                if (Convert.ToBoolean(dtAPIDData.Rows[0]["IsActive"]))
                {
                    if (Convert.ToBoolean(dtService.Rows[0]["Onoff"]))
                    {
                        var date = DateTime.Now;
                        if (date.Hour >= Convert.ToInt32(dtService.Rows[0]["OnTime"].ToString()) && date.Hour <= Convert.ToInt32(dtService.Rows[0]["OffTime"].ToString()))
                        {
                            serviceWithAPIIDStatus.Status = "Success";
                            serviceWithAPIIDStatus.APIID = dtService.Rows[0]["APIID"].ToString().ToInt32();
                            serviceWithAPIIDStatus.Price = dtService.Rows[0]["price"].ToString();
                            serviceWithAPIIDStatus.Reason = "";
                            serviceWithAPIIDStatus.Reason = "";
                        }
                        else
                        {
                            serviceWithAPIIDStatus.Status = "Failed";
                            serviceWithAPIIDStatus.Price = "0";
                            serviceWithAPIIDStatus.APIID = dtService.Rows[0]["APIID"].ToString().ToInt32();
                            serviceWithAPIIDStatus.Reason = $"Service Working Time {dtService.Rows[0]["OnTime"].ToString()} to {dtService.Rows[0]["OffTime"].ToString()}";
                        }
                    }
                    else
                    {
                        serviceWithAPIIDStatus.Status = "Failed";
                        serviceWithAPIIDStatus.APIID = dtService.Rows[0]["APIID"].ToString().ToInt32();
                        serviceWithAPIIDStatus.Reason = dtService.Rows[0]["Reason"].ToString();
                    }
                }
                else
                {
                    serviceWithAPIIDStatus.Status = "Failed";
                    serviceWithAPIIDStatus.APIID = 1;
                    serviceWithAPIIDStatus.Reason = "API In DeActive Mode";
                }
            }
            else
            {
                serviceWithAPIIDStatus.Status = "Failed";
                serviceWithAPIIDStatus.APIID = 1;
                serviceWithAPIIDStatus.Reason = "API Not Found";
            }
        }
        else
        {
            serviceWithAPIIDStatus.Status = "Failed";
            serviceWithAPIIDStatus.APIID = 1;
            serviceWithAPIIDStatus.Reason = "Service Not Found";
        }
        return serviceWithAPIIDStatus;
    }

   

    public async Task<DataTable> GetServiceData(string ServiceID)
    {
        return await cls.select_data_dtAsync("select * from Service where ID="+ ServiceID + "");
    }
}