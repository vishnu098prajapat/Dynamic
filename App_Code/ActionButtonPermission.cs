using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ActionButtonPermission
/// </summary>
public class ActionButtonPermission
{
    public int ID { get; set; } = 0;
    public int PageID { get; set; } = 0;
    public int RoleID { get; set; } = 0;
    public bool Active { get; set; } = false;
    public bool Delete { get; set; } = false;
    public bool Edit { get; set; } = false;
    public bool AddFund { get; set; } = false;
    public bool DeductFund { get; set; } = false;
    public bool Submit { get; set; } = false;
    public bool CheckStatus { get; set; } = false;
    public bool ForceFailed { get; set; } = false;
    public bool ForceSuccess { get; set; } = false;

    public ActionButtonPermission()
    {
       
    }

    public static ActionButtonPermission GetPermission(Int32 PageID,string RoleID)
    {
        cls_connection cls = new cls_connection();
        string Json = cls.select_data_scalar_string("ManagePageList 'GellByID','"+ RoleID + "','"+ PageID + "'");
        ActionButtonPermission ObjUpi;
        if (!string.IsNullOrEmpty(Json))
        {
            ObjUpi = JsonConvert.DeserializeObject<ActionButtonPermission>(Json.Replace("[", "").Replace("]", ""));
        }
        else
        {
            ObjUpi = new ActionButtonPermission();
        }
        return ObjUpi;
    }

}