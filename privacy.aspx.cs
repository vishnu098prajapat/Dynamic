using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class privacy : System.Web.UI.Page
{
    public Company company;
    cls_connection cls = new cls_connection();

    protected void Page_Load(object sender, EventArgs e)
    {
        company = Company.GetCompanyInfo();
        if (!IsPostBack)
        {
            PrivacyPolicy();
        }
    }

    public void PrivacyPolicy()
    {
        DataTable dt = cls.select_data_dt("EXEC ManagePrivacyPolicy 'Get','" + 0 + "' ,'" + company.MemberID + "'");
        if (dt.Rows.Count > 0)
        {
            rptDataPrivacy.DataSource = dt;
            rptDataPrivacy.DataBind();
        }
    }
}
