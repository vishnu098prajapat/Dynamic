using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

/// <summary>
/// Summary description for BindDropDown
/// </summary>
public static class BindDropDown
{
    public static void FillDropDown(DropDownList ddl, string ActionName, string Name, string Value, string DefaultMessage)
    {
        cls_connection objData = new cls_connection();
        DataTable dt = new DataTable();
        dt = objData.select_data_dt("FillDropDown '" + ActionName + "'");
        ddl.DataSource = dt;
        ddl.DataTextField = Name;
        ddl.DataValueField = Value;
        ddl.DataBind();
        ddl.Items.Insert(0, new ListItem(DefaultMessage, "0"));

    }

    public static void FillDropDown(RadioButtonList ddl, string ActionName, string Name, string Value, string DefaultMessage)
    {
        cls_connection objData = new cls_connection();
        DataTable dt = new DataTable();
        dt = objData.select_data_dt("FillDropDown '" + ActionName + "'");
        ddl.DataSource = dt;
        ddl.DataTextField = Name;
        ddl.DataValueField = Value;
        ddl.DataBind();
    }

    public static string BankDownList()
    {
        string BankResult = ApiPostCallInstantPay.GetBankList("42342");

        cls_connection objData = new cls_connection();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(BankResult);

        string json = JsonConvert.SerializeXmlNode(doc);
        DataTable dt = GetJSONToDataTableUsingNewtonSoftDll(JObject.Parse(json)["xml"]["data"]["item"].ToString());

        DataView dv = new DataView(dt);
        dv.RowFilter = "is_down=1"; // query example = "id = 10"
        var idlist = dv.Table.AsEnumerable().Where(r => r.Field<string>("is_down") == "1").Select(r => r.Field<string>("bank_name")).ToArray();
        string result = string.Join(",", idlist);

        return result.ToString();
    }

    public static void FillDropDown(DropDownList ddl, string ActionName, string Name, string Value, string DefaultMessage, string ByValue)
    {
        cls_connection objData = new cls_connection();
        DataTable dt = new DataTable();
        dt = objData.select_data_dt("FillDropDown '" + ActionName + "','" + ByValue + "'");
        if (dt.Rows.Count > 0)
        {
            ddl.DataSource = dt;
            ddl.DataTextField = Name;
            ddl.DataValueField = Value;
            ddl.DataBind();
            ddl.Items.Insert(0, new ListItem(DefaultMessage, "0"));
        }
        else
        {
            ddl.Items.Insert(0, new ListItem(DefaultMessage, "0"));
        }

    }

    public static void FillDropDown(DropDownList ddl, string ActionName, string Name, string Value, string DefaultMessage, string ByValue, string CompanyID)
    {
        cls_connection objData = new cls_connection();
        DataTable dt = new DataTable();
        dt = objData.select_data_dt("FillDropDown '" + ActionName + "','" + ByValue + "'," + CompanyID + "");
        if (dt.Rows.Count > 0)
        {
            ddl.DataSource = dt;
            ddl.DataTextField = Name;
            ddl.DataValueField = Value;
            ddl.DataBind();
            ddl.Items.Insert(0, new ListItem(DefaultMessage, "0"));
        }
        else
        {
            ddl.Items.Insert(0, new ListItem(DefaultMessage, "0"));
        }

    }

    public static void FillDropDown(Repeater ddl, string ActionName, string ByValue, string CompanyID)
    {
        cls_connection objData = new cls_connection();
        DataTable dt = new DataTable();
        dt = objData.select_data_dt("FillDropDown '" + ActionName + "','" + ByValue + "'," + CompanyID + "");
        if (dt.Rows.Count > 0)
        {
            ddl.DataSource = dt;
            ddl.DataBind();
        }


    }


    public static void FillDropDown(Repeater ddl, string ActionName, string ByValue)
    {
        cls_connection objData = new cls_connection();
        DataTable dt = new DataTable();
        dt = objData.select_data_dt("FillDropDown '" + ActionName + "','" + ByValue + "'");
        if (dt.Rows.Count > 0)
        {
            ddl.DataSource = dt;
            ddl.DataBind();
        }
    }

    public static void FillDropDown(DropDownList ddl, string ActionName, string Name, string Value, string DefaultMessage, int RoleID)
    {
        cls_connection objData = new cls_connection();
        DataTable dt = new DataTable();
        dt = objData.select_data_dt("select * from package where id in (select * from Split((select packageid from role where id=" + RoleID + "), ','))");
        ddl.DataSource = dt;
        ddl.DataTextField = Name;
        ddl.DataValueField = Value;
        ddl.DataBind();
        ddl.Items.Insert(0, new ListItem(DefaultMessage, "0"));

    }

    public static bool GetBankDown(string BankName)
    {
        string BankResult = ApiPostCallInstantPay.GetBankList("54");


        XmlDocument doc = new XmlDocument();
        doc.LoadXml(BankResult);

        string json = JsonConvert.SerializeXmlNode(doc);

        DataTable dt = GetJSONToDataTableUsingNewtonSoftDll(JObject.Parse(json)["xml"]["data"]["item"].ToString());


        DataRow[] results = dt.Select("bank_name like '%" + BankName + "%' and is_down=1");

        if (results.Length > 0)
        {
            return true;
        }

        return false;
    }
    public static bool GetBankDown1(string BankName)
    {
        string BankResult = ApiPostCallInstantPay.GetBankList("54");

        cls_connection objData = new cls_connection();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(BankResult);

        string json = JsonConvert.SerializeXmlNode(doc);
        DataTable dt = GetJSONToDataTableUsingNewtonSoftDll(JObject.Parse(json)["xml"]["data"]["item"].ToString());
        objData.update_data("insert into tbl_Apeslog(url)values('" + json + "')");


        DataRow[] results = dt.Select("bank_name like '%" + BankName + "%' and is_down=1");

        if (results.Length > 0)
        {
            return true;
        }

        return false;
    }
    public static DataTable GetDownList()
    {
        string BankResult = ApiPostCallInstantPay.GetBankList("432");

        cls_connection objData = new cls_connection();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(BankResult);

        string json = JsonConvert.SerializeXmlNode(doc);
        DataTable dt = GetJSONToDataTableUsingNewtonSoftDll(JObject.Parse(json)["xml"]["data"]["item"].ToString());

        //DataTable dtReturn = new DataTable();
        //DataRow[] results = dt.Select("is_down=1");
        // dtReturn.Rows.Add(results);
        return dt;
    }

    public static DataTable GetJSONToDataTableUsingNewtonSoftDll(string JSONData)
    {
        DataTable dt = (DataTable)JsonConvert.DeserializeObject(JSONData, (typeof(DataTable)));
        return dt;
    }
}