using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for RechargeResponse
/// </summary>
public class RechargeResponse
{
    public string STID { get; set; }
    public string Status { get; set; }
    public string OperatorRef { get; set; }
    public string Message { get; set; }
    public string ClientID { get; set; }
    public string ResponseType { get; set; }

    public RechargeResponse()
    {
        ResponseType = "csv";
    }

    public string GetJson()
    {
        RechargeResponse rechargeResponse = new RechargeResponse();
        rechargeResponse.STID = this.STID;
        rechargeResponse.Status = this.Status;
        rechargeResponse.OperatorRef = this.OperatorRef;
        rechargeResponse.Message = this.Message;
        rechargeResponse.ClientID = this.ClientID;
        rechargeResponse.ResponseType = this.ResponseType;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(rechargeResponse);
        return jsonString;
    }
    public string GetResult
    {
        get
        {
            //if (this.ResponseType.ToUpper() == "JSON")
            //{
            //    RechargeResponse rechargeResponse = new RechargeResponse();
            //    rechargeResponse.STID = this.STID;
            //    rechargeResponse.Status = this.Status;
            //    rechargeResponse.OperatorRef = this.OperatorRef;
            //    rechargeResponse.Message = this.Message;
            //    rechargeResponse.ClientID = this.ClientID;
            //    rechargeResponse.ResponseType = this.ResponseType;
            //    string jsonString;
            //    jsonString = JsonConvert.SerializeObject(rechargeResponse);
            //    return jsonString;
            //}
            //else
            {
                return String.Concat(this.Status.ToString(), ",", this.Message, ",", this.STID, ",", this.OperatorRef, ",", this.ClientID);

            }
            //Places commas between fields, string quotes around address
        }
    }

}