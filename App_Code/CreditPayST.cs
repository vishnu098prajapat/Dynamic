using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CreditPayST
/// </summary>
public class CreditPayST
{
    public string APIID { get; set; }

    public string Token { get; set; }
    public string MethodName { get; set; }
    public string CardNo { get; set; }
    public string Name { get; set; }
    public string NetWork { get; set; }
    public string OrderID { get; set; }
    public string Amount { get; set; }
    public string fetchId { get; set; }
    public string MobileNO { get; set; }



    public string GetJson()
    {
        CreditPayST ObjData = new CreditPayST();
        ObjData.CardNo = this.CardNo;
        ObjData.Name = this.Name;
        ObjData.NetWork = this.NetWork;
        ObjData.OrderID = this.OrderID;
        ObjData.Amount = this.Amount;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.fetchId = this.fetchId;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}