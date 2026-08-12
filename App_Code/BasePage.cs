using System;
using System.Web;
using System.Web.UI;

public class BasePage : Page
{
    cls_connection cls = new cls_connection();
    ITokenService tokenService = new TokenService("", 20);
    public string CurrentUserID { get; private set; }   // <-- यही आप हर page में use करोगे

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        string token = HttpContext.Current.Request.QueryString["LoginID"];

        if (string.IsNullOrEmpty(token))
        {
            Response.Redirect(ConstantsData.RedirectUrlM);
            return;
        }

        var principal = tokenService.ReadTokenNew(token);
        if (principal == null)
        {
            ErrorShow.AlertMessage(this, "Token expired.", ConstantsData.CompanyName);
            Response.End();
            return;
        }

        // Save UserID
        CurrentUserID = tokenService.GetUserID(principal);

        // Load Member Session
        Session["dtMember"] = cls.select_data_dt(
            "select * from View_MemberDetails where loginid = '" + CurrentUserID + "'"
        );

        if (Session["dtMember"] == null)
        {
            Response.Redirect(ConstantsData.RedirectUrlM);
            return;
        }
    }
}