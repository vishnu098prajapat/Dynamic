using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ICICIPayoutRequest
/// </summary>

public class ICICIPayoutRequest
{
    public string AGGRID { get; set; }
    //public string AGGRNAME { get; set; }
    public string CORPID { get; set; }
    public string USERID { get; set; }
    public string URN { get; set; }
    public string ACCOUNTNO { get; set; }
    public string FROMDATE { get; set; }
    public string TODATE { get; set; }


    public ICICIPayoutRequest()
    {
        AGGRID = "OTOE0033";
        //AGGRNAME = "IDSHAAT";
        CORPID = "570037426";
        USERID = "USER1";
        URN = "SR192393007";
        ACCOUNTNO = "133005501125";
        FROMDATE = "11-10-2020";
        TODATE = "13-10-2020";
    }
    public string GetJson()
    {
        ICICIPayoutRequest ObjData = new ICICIPayoutRequest();
        ObjData.AGGRID = this.AGGRID;
        //ObjData.AGGRNAME = this.AGGRNAME;
        ObjData.CORPID = this.CORPID;
        ObjData.USERID = this.USERID;
        ObjData.URN = this.URN;
        ObjData.ACCOUNTNO = this.ACCOUNTNO;
        ObjData.FROMDATE = this.FROMDATE;
        ObjData.TODATE = this.TODATE;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class ICICIBalanceFetch
{
    public string AGGRID { get; set; }
    //public string AGGRNAME { get; set; }
    public string CORPID { get; set; }
    public string USERID { get; set; }
    public string URN { get; set; }
    public string ACCOUNTNO { get; set; }



    public ICICIBalanceFetch()
    {
        AGGRID = "OTOE0033";
        //AGGRNAME = "IDSHAAT";
        CORPID = "570037426";// "CIBNEXT";
        USERID = "USER1";// "CIBTESTING6";
        URN = "SR192393007";
        ACCOUNTNO = "133005501125";
       
    }
    public string GetJson()
    {
        ICICIBalanceFetch ObjData = new ICICIBalanceFetch();
        ObjData.AGGRID = this.AGGRID;
        //ObjData.AGGRNAME = this.AGGRNAME;
        ObjData.CORPID = this.CORPID;
        ObjData.USERID = this.USERID;
        ObjData.URN = this.URN;
        ObjData.ACCOUNTNO = this.ACCOUNTNO;
      

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}