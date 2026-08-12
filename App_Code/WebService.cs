using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
/// <summary>
/// Summary description for WebService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
[System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService
{

    public WebService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string GetBasicMemberData(Int32 Msrno)
    {
        List<MemberBasicDetails> empResult = new List<MemberBasicDetails>();
        cls_connection ObjData = new cls_connection();
        DataTable dt = new DataTable();
        dt = ObjData.select_data_dt("PROC_GET_MEMBER_BASIC_DETAILS '"+ Msrno + "'");
        //foreach (DataRow dr in dt.Rows)
        //{
        //    MemberBasicDetails ObjCls = new MemberBasicDetails();
        //    ObjCls.Name = dr["Name"].ToString();
        //    ObjCls.MemberID = dr["MemberID"].ToString();
        //    ObjCls.MainWallet = dr["MainWallet"].ToString();
        //    ObjCls.AepsWallet = dr["AepsWallet"].ToString();
        //    empResult.Add(ObjCls);
        //}
        return ConvertDataTabletoString(dt);
    }

    public static string ConvertDataTabletoString(DataTable dt)
    {
        System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
        Dictionary<string, object> row;
        foreach (DataRow dr in dt.Rows)
        {
            row = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                row.Add(col.ColumnName, dr[col]);
            }
            rows.Add(row);
        }
        return serializer.Serialize(rows).ToString();
    }
}
