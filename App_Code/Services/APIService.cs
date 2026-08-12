using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for APIService
/// </summary>
public class APIService : IAPIService
{
    cls_connection cls;
    public APIService()
    {
        cls = new cls_connection();
    }

    public async Task<DataTable> GetAPIIDData(string APIID)
    {
        return await cls.select_data_dtAsync("select * from tblRecharge_API where APIID=" + APIID + "");
    }
}