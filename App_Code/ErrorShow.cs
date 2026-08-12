using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for ErrorShow
/// </summary>
public static class ErrorShow
{
    
   public static void Success(Page page,string Message)
    {                      
        ScriptManager.RegisterClientScriptBlock(page, page.GetType(), "pagename", "alertify.success('"+ Message + "').set({title:'Arrive'})", true);
    }

    public static void Error(Page page1, string Message)
    {
      
        ScriptManager.RegisterClientScriptBlock(page1, page1.GetType(), "pagename", "alertify.error('" + Message + "').set({title:'Arrive'})", true);
    }

    public static void AlertMessage(Page page1, string Message,string Title)
    {
        ScriptManager.RegisterClientScriptBlock(page1, page1.GetType(), "pagename", "alertify.alert('"+ Message + "', function () {                }).set({title:'"+ Title + "'});", true);
    }

    public static void AlertMessageWithRedirect(Page page1, string Message,string Message1, string Title)
    {
        ScriptManager.RegisterClientScriptBlock(page1, page1.GetType(), "pagename", "alertify.alert('" + Message + "', function () {   window.open('"+ Message1 + "','_self');             }).set({title:'" + Title + "'});", true);
    }

    public static void AlertMessageWithRedirectNewTab(Page page1, string Message, string Message1, string Title)
    {

        ScriptManager.RegisterClientScriptBlock(page1, page1.GetType(), "pagename", "alertify.alert('" + Message + "', function () {   window.open('" + Message1 + "','_blank');             }).set({title:'" + Title + "'});", true);
    }
    public static void AlertMessageWithClosePopUp(Page page1, string Message, string Message1)
    {
        ScriptManager.RegisterClientScriptBlock(page1, page1.GetType(), "pagename", "alertify.alert('" + Message + "', function () {   $('#AdCls').click()            }).set({title:'Arrive'});", true);
    }

    public static string SuccessMessage(string ErrorMessage)
    {
        string response = "{\"statuscode\":\"TXN\",\"status\":\"" + ErrorMessage + "\"}";
        return response;
    }

    public static string SuccessMessageAPI(string ErrorMessage)
    {
        string response = "{\"code\":\"TXN\",\"mess\":\"" + ErrorMessage + "\"}";
        return response;
    }
    public static string SuccessArray(string json)
    {
        string response = "{\"statuscode\":\"TXN\",\"status\":\"Transaction Successful\",\"data\": " + json + "}";
        return response;
    }

    public static string SuccessArrayAPI(string json)
    {
        string response = "{\"code\":\"TXN\",\"mess\":\"Transaction Successful\",\"data\": " + json + "}";
        return response;
    }


    public static string ErrorMessage(string ErrorMessage)
    {
        string response = "{\"statuscode\":\"ERR\",\"status\":\"" + ErrorMessage + "\"}";
        return response;
    }

    public static string ErrorMessageAPI(string ErrorMessage)
    {
        string response = "{\"code\":\"ERR\",\"mess\":\"" + ErrorMessage + "\"}";
        return response;
    }

    ///Nofify
    public static void ErrorNotify(Page page1, string Message)
    {
        ScriptManager.RegisterClientScriptBlock(page1, page1.GetType(), "pagename", "notif({type: 'error',msg: '" + Message + "',position: 'right'});", true);
    }

    public static void SuccessNotify(Page page1, string Message)
    {        
        ScriptManager.RegisterClientScriptBlock(page1, page1.GetType(), "pagename", "notif({type: 'success',msg: '" + Message + "',position: 'right'});", true);
    }
}