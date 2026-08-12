using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for LogsRequestModel
/// </summary>
public class LogsRequestModel
{
    public int ServiceID { get; set; }
    public int APIID { get; set; }
    public string RequestData { get; set; }
    public string ResponseData { get; set; }
    public string CallBackData { get; set; }
    public string TransID { get; set; }
}