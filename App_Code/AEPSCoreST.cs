using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for AEPSCoreST
/// </summary>
public class AEPSCoreST
{
    public AEPSCoreST()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}



public class AP
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string BankID { get; set; }
    public string DeviceName { get; set; }
    public string Amount { get; set; }
    public string mobilenumber { get; set; }
    public string adhaarnumber { get; set; }
    public string OrderID { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }
    public string GetJson()
    {
        AP ObjData = new AP();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "ap";
        ObjData.BankID = this.BankID;
        ObjData.Amount = this.Amount;
        ObjData.DeviceName = this.DeviceName;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.OrderID = this.OrderID;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        ObjData.data = this.data;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class CW
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string BankID { get; set; }
    public string DeviceName { get; set; }
    public string Amount { get; set; }
    public string mobilenumber { get; set; }
    public string adhaarnumber { get; set; }
    public string OrderID { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }
    public string GetJson()
    {
        CW ObjData = new CW();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "cw";
        ObjData.BankID = this.BankID;
        ObjData.Amount = this.Amount;
        ObjData.DeviceName = this.DeviceName;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.OrderID = this.OrderID;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        ObjData.data = this.data;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class CommonRequestSoni
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string BankID { get; set; }
    public string DeviceName { get; set; }
    public string Amount { get; set; }
    public string mobilenumber { get; set; }
    public string MerAuthTxnId { get; set; }
    
    public string adhaarnumber { get; set; }
    public string OrderID { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }
    public string GetJson()
    {
        CommonRequestSoni ObjData = new CommonRequestSoni();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.BankID = this.BankID;
        ObjData.Amount = this.Amount;
        ObjData.DeviceName = this.DeviceName;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.MerAuthTxnId = this.MerAuthTxnId;
        ObjData.OrderID = this.OrderID;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        ObjData.data = this.data;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}





public class MSST
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string BankID { get; set; }
    public string DeviceName { get; set; }
    public string mobilenumber { get; set; }
    public string adhaarnumber { get; set; }
    public string OrderID { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }
    public string GetJson()
    {
        MSST ObjData = new MSST();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "ms";
        ObjData.BankID = this.BankID;
        ObjData.DeviceName = this.DeviceName;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.OrderID = this.OrderID;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        ObjData.data = this.data;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}





public class BE
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string BankID { get; set; }
    public string DeviceName { get; set; }
    public string mobilenumber { get; set; }
    public string adhaarnumber { get; set; }
    public string OrderID { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }
    public string GetJson()
    {
        CW ObjData = new CW();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "be";
        ObjData.BankID = this.BankID;
        ObjData.DeviceName = this.DeviceName;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.OrderID = this.OrderID;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        ObjData.data = this.data;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




public class BankList
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
   
    public string GetJson()
    {
        BankList ObjData = new BankList();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "banklist";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class PaysAEPSOnBoardST
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string merchantcode { get; set; }
    public string MethodName { get; set; }

    public string mobile { get; set; }
    public string is_new { get; set; }
    public string email { get; set; }
    public string firm { get; set; }
    public string GetJson()
    {
        PaysAEPSOnBoardST ObjData = new PaysAEPSOnBoardST();
        ObjData.merchantcode = this.merchantcode;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "onboard";
        ObjData.mobile = this.mobile;
        ObjData.is_new = this.is_new;
        ObjData.email = this.email;
        ObjData.firm = this.firm;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}


public class CheckStatusAEPS
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string merchantcode { get; set; }
    public string MethodName { get; set; }
    public string mobile { get; set; }
    public string firm { get; set; }
    public string GetJson()
    {
        CheckStatusAEPS ObjData = new CheckStatusAEPS();
        ObjData.merchantcode = this.merchantcode;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "checkstatus";
        ObjData.mobile = this.mobile;
        ObjData.firm = this.firm;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}



