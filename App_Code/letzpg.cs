using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for letzpg
/// </summary>
public class letzpg
{

    public static string PAY_ID = "";
    public static string MERCHANTNAME = "";
    public static string Salt = "";
    public static string TXNTYPE = "SALE";
    public static string RETURN_URL = "https://mavyfintech.com/letspgcallback.aspx";
    public letzpg()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}


public class pgRequest
{
    public string PAY_ID { get; set; }
    public string MERCHANTNAME { get; set; }
    public string ORDER_ID { get; set; }
    public string AMOUNT { get; set; }
    public string TXNTYPE { get; set; }
    public string CUST_NAME { get; set; }
    public string CUST_STREET_ADDRESS1 { get; set; }
    public string CUST_ZIP { get; set; }
    public string CUST_EMAIL { get; set; }
    public string PRODUCT_DESC { get; set; }
    public string CURRENCY_CODE { get; set; }
    public string RETURN_URL { get; set; }
    public string CUST_PHONE { get; set; }
    public string HASH { get; set; }
    public string Salt { get; set; }

    public pgRequest()
    {
        PAY_ID = letzpg.PAY_ID;
        MERCHANTNAME = letzpg.MERCHANTNAME;
        TXNTYPE = letzpg.TXNTYPE;
        RETURN_URL = letzpg.RETURN_URL;
        Salt = letzpg.Salt;
    }
}
