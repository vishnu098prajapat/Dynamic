using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for CommonService
/// </summary>
public class CommonService :ICommonService
{
    cls_connection cls;
    IServiceList serviceList;

    public CommonService()
    {
        cls = new cls_connection();
        serviceList = new ServiceList();
    }

    public async Task<DataTable> GetAEPSBankStatus(string BankName, string Msrno)
    {
        DataTable dthistory = await cls.select_data_dtAsync("select iif(DailyAuthentication=1 and convert(nvarchar(30),authdate,112)=convert(nvarchar(30),getdate(),112),cast(1 as bit),cast(0 as bit)) as DailyAuthentication,* from AEPSOnBoardStatus where msrno=" + Msrno + " and name='" + BankName + "'");
        return dthistory;
    }

    public async Task<decimal> GetSurcharge(Int32 PackageID, decimal Amount, Int32 ServiceID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(ServiceID.ToString());
        if (serviceData.Status == "Success")
        {
            var item = MoneyTransfer_EkoAPI.GetSurchargeCommonCommisison(PackageID.ToString(), Amount, serviceData.APIID.ToString(), ServiceID.ToString());
            return item.Item1;
        }
        else
        {
            return 0;
        }
    }

    public async Task<Tuple<bool, string>> GetServiceStatus(string ID)
    {
        DataTable dtActivePayoutAPI =await cls.select_data_dtAsync("select * from Service where ID=" + ID + " and IsActive=1");
        if (dtActivePayoutAPI.Rows.Count > 0)
        {
            if (Convert.ToBoolean(dtActivePayoutAPI.Rows[0]["Onoff"]))
            {
                return Tuple.Create(true, "");
            }
            else
            {
                return Tuple.Create(false, dtActivePayoutAPI.Rows[0]["Reason"].ToString());
            }
        }
        else
        {
            return Tuple.Create(false, "Service Not Active So Can not Use");
        }

    }

    public async Task<string> GETClientIP(HttpRequest Request)
    {
        return await Task.Run(() =>
        {
            string ipAddress = string.Empty;

            if (Request != null)
            {
                if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }
                else if (Request.UserHostAddress.Length != 0)
                {
                    ipAddress = Request.UserHostAddress;
                }
            }
            return ipAddress;
        });
    }

    public async Task<DataTable> GetRechargeHistory(string TransID)
    {
        return await cls.select_data_dtAsync("select * from tblRecharge_History where transid='"+ TransID + "'");
    }


    public async Task<Int32> SaveLogs(LogsRequestModel logsRequestModel)
    {
        return await cls.update_dataAsync("ADDEDITLogsServiceData "+ logsRequestModel.ServiceID + ",'"+ logsRequestModel.APIID + "','"+ logsRequestModel.TransID + "','"+ logsRequestModel.RequestData + "','" + logsRequestModel.ResponseData + "','" + logsRequestModel.CallBackData + "'");
    }
}