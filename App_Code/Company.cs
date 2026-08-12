using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for Company
/// </summary>
public class Company
{
    public Company()
    {
        ID = 0;
    }

    public int ID { get; set; } = 0;
    public string Name { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string WebsiteURL { get; set; } = "";
    public string AndroidURL { get; set; } = "";
    public string Logo { get; set; } = "";
    public string Email { get; set; } = "";
    public string AlternateEmail { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string AlternateMobile { get; set; } = "";
    public string Address { get; set; } = "";
    public string CopyRight { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string FaceBook { get; set; } = "";
    public string WhastApp { get; set; } = "";
    public string Instagram { get; set; } = "";
    public string Twiter { get; set; } = "";
    public string Youtube { get; set; } = "";

    public string BankName { get; set; } = "";
    public string ACName { get; set; } = "";
    public string ACType { get; set; } = "";
    public string ACNumber { get; set; } = "";
    public string IFSC { get; set; } = "";
    public string MICRCode { get; set; } = "";
    public string Fevicon { get; set; } = "";
    public string Signature { get; set; } = "";
    public string MemberID { get; set; } = "";
    public string BodyColor { get; set; } = "";
    public string LeftColor { get; set; } = "";
    public string HeaderColor { get; set; } = "";


    public string Map { get; set; } = "";

    //public System.DateTime CreateDate { get; set; }
    //public System.DateTime UpdatedDate { get; set; }


    public bool IsAnyNullOrEmpty(object myObject)
    {
        foreach (PropertyInfo pi in myObject.GetType().GetProperties())
        {
            if (pi.PropertyType == typeof(string))
            {
                string value = (string)pi.GetValue(myObject);
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
            }
        }
        return true;

    }

    public static Company GetCompanyInfo()
    {
        Company company = new Company();
        DataTable dt = GetCompany();
        if (dt.Rows.Count > 0)
        {
            company.Name = dt.Rows[0]["Name"].ToString();
            company.ID = Convert.ToInt32(dt.Rows[0]["ID"].ToString());
            company.OwnerName = dt.Rows[0]["OwnerName"].ToString();
            company.WebsiteURL = dt.Rows[0]["WebsiteURL"].ToString();
            company.Logo = dt.Rows[0]["Logo"].ToString();
            company.Email = dt.Rows[0]["Email"].ToString();
            company.AlternateEmail = dt.Rows[0]["AlternateEmail"].ToString();
            company.Mobile = dt.Rows[0]["Mobile"].ToString();
            company.AlternateMobile = dt.Rows[0]["AlternateMobile"].ToString();
            company.WebsiteURL = dt.Rows[0]["WebsiteURL"].ToString();
            company.Address = dt.Rows[0]["Address"].ToString();
            company.CopyRight = dt.Rows[0]["CopyRight"].ToString();
            company.FaceBook = dt.Rows[0]["FaceBook"].ToString();
            company.WhastApp = dt.Rows[0]["WhastApp"].ToString();
            company.AndroidURL = dt.Rows[0]["AndroidURL"].ToString();
            company.Instagram = dt.Rows[0]["Instagram"].ToString();
            company.Twiter = dt.Rows[0]["Twiter"].ToString();
            company.Youtube = dt.Rows[0]["Youtube"].ToString();
            company.BankName = dt.Rows[0]["BankName"].ToString();
            company.ACName = dt.Rows[0]["ACName"].ToString();
            company.ACType = dt.Rows[0]["ACType"].ToString();
            company.ACNumber = dt.Rows[0]["ACNumber"].ToString();
            company.IFSC = dt.Rows[0]["IFSC"].ToString();
            company.MICRCode = dt.Rows[0]["MICRCode"].ToString();
            company.Fevicon = dt.Rows[0]["Feviconicon"].ToString();
            company.Signature = dt.Rows[0]["signature"].ToString();
            company.MemberID = dt.Rows[0]["MemberID"].ToString();
            company.BodyColor = dt.Rows[0]["BodyColor"].ToString();
            company.LeftColor = dt.Rows[0]["LeftColor"].ToString();
            company.HeaderColor = dt.Rows[0]["HeaderColor"].ToString();
            company.Map = dt.Rows[0]["Map"].ToString();
        }
        return company;
    }
    public static DataTable GetCompany()
    {
        if (HttpContext.Current.Session["dtCompany"] == null)
        {
            string url = HttpContext.Current.Request.Url.Host;
            //and websiteurl like '%esrpay%'
            cls_connection cls = new cls_connection();
            DataTable dth = cls.select_data_dt("select top 1 * from company where isactive=1  Order by id asc"); //and websiteurl like '%"+ url + "%' 
            if (dth.Rows.Count > 0)
            {
                HttpContext.Current.Session["dtCompany"] = dth;
                HttpContext.Current.Application["URL"] = "../images/Company/" + dth.Rows[0]["logo"].ToString();
                HttpContext.Current.Application["CiD"] = dth.Rows[0]["ID"].ToString();
            }
            return dth;

        }
        else
        {
            return (DataTable)HttpContext.Current.Session["dtCompany"];
        }
    }


}