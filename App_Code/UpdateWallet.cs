using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for UpdateWallet
/// </summary>
public static class UpdateWallet
{
    
    public static string UpdateWalletMember(HtmlGenericControl currnetBalance, HtmlGenericControl lblAEPSBal)
    {
        cls_connection ObjConnection = new cls_connection();
        DataTable dtMember = new DataTable();
        dtMember = ObjConnection.select_data_dt("select * from Member where msrno="+HttpContext.Current.Session["MemberMsrNo"].ToString()+"");
        if (dtMember.Rows.Count > 0)
        {
            currnetBalance.InnerHtml = ObjConnection.select_data_scalar_string("select isnull(balance,0)as Balance from TBL_EWALLETBALANCE where msrno = '" + dtMember.Rows[0]["msrno"] + "'");
            lblAEPSBal.InnerHtml = ObjConnection.select_data_scalar_string("select isnull(balance,0)as Balance from tblMLM_AEPS2EWalletBalance where msrno = '" + dtMember.Rows[0]["msrno"] + "'");
        }
        return "";
    }
}