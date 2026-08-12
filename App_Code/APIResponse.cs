using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for APIResponse
/// </summary>
public class APIResponse
{
    public APIResponse()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static string SuccessMessage(string ErrorMessage)
    {
        string response = "{\"statuscode\":\"TXN\",\"status\":\"" + ErrorMessage + "\"}";
        
        return response;
    }

    public static string ErrorMessage(string ErrorMessage)
    {
        string response = "{\"statuscode\":\"ERR\",\"status\":\"" + ErrorMessage + "\"}";
        return response;
    }
}