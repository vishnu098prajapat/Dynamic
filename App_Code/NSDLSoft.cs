using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for NSDLSoft
/// </summary>
public class NSDLSoft
{
    public NSDLSoft()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}



public class NSDLMerchantRegistration
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string Name { get; set; }
    public string mobile { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string Panno { get; set; }
    public string AdharNo { get; set; }

    public string GetJson()
    {
        NSDLMerchantRegistration ObjData = new NSDLMerchantRegistration();
        ObjData.Name = this.Name;
        ObjData.mobile = this.mobile;
        ObjData.Email = this.Email;
        ObjData.Address = this.Address;
        ObjData.Panno = this.Panno;
        ObjData.AdharNo = this.AdharNo;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}




public class newPanApplySoft
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string title { get; set; }
    public string lastName { get; set; }
    public string firstName { get; set; }
    public string middleName { get; set; }
    public string mobile { get; set; }
    public string fatherLastName { get; set; }
    public string fatherMiddleName { get; set; }
    public string fatherFirstName { get; set; }
    public string dob { get; set; }
    public string email { get; set; }
    public string isdCode { get; set; }
    public string gender { get; set; }
    public string parentNamePrint { get; set; }
    public string place { get; set; }
    public string txnid { get; set; }
    public string returnUrl { get; set; }
    public string agentId { get; set; }

    public string GetJson()
    {
        newPanApplySoft ObjData = new newPanApplySoft();
        ObjData.title = this.title;
        ObjData.lastName = this.lastName;
        ObjData.firstName = this.firstName;
        ObjData.middleName = this.middleName;
        ObjData.mobile = this.mobile;
        ObjData.fatherLastName = this.fatherLastName;
        ObjData.fatherMiddleName = this.fatherMiddleName;
        ObjData.fatherFirstName = this.fatherFirstName;
        ObjData.dob = this.dob;
        ObjData.email = this.email;
        ObjData.isdCode = this.isdCode;
        ObjData.gender = this.gender;
        ObjData.parentNamePrint = this.parentNamePrint;
        ObjData.place = this.place;
        ObjData.txnid = this.txnid;
        ObjData.returnUrl = this.returnUrl;
        ObjData.agentId = this.agentId;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class panCorrectionApply
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string title { get; set; }
    public string lastName { get; set; }
    public string firstName { get; set; }
    public string middleName { get; set; }
    public string mobile { get; set; }
    public string fatherLastName { get; set; }
    public string fatherMiddleName { get; set; }
    public string fatherFirstName { get; set; }
    public string dob { get; set; }
    public string email { get; set; }
    public string isdCode { get; set; }
    public string gender { get; set; }
    public string parentNamePrint { get; set; }
    public string place { get; set; }
    public string txnid { get; set; }
    public string returnUrl { get; set; }
    public string agentId { get; set; }
    public string pan { get; set; }

    public string pan1 { get; set; }
    public string pan2 { get; set; }
    public string pan3 { get; set; }
    public string pan4 { get; set; }


    public string GetJson()
    {
        panCorrectionApply ObjData = new panCorrectionApply();
        ObjData.title = this.title;
        ObjData.lastName = this.lastName;
        ObjData.firstName = this.firstName;
        ObjData.middleName = this.middleName;
        ObjData.mobile = this.mobile;
        ObjData.fatherLastName = this.fatherLastName;
        ObjData.fatherMiddleName = this.fatherMiddleName;
        ObjData.fatherFirstName = this.fatherFirstName;
        ObjData.dob = this.dob;
        ObjData.email = this.email;
        ObjData.isdCode = this.isdCode;
        ObjData.gender = this.gender;
        ObjData.parentNamePrint = this.parentNamePrint;
        ObjData.place = this.place;
        ObjData.txnid = this.txnid;
        ObjData.returnUrl = this.returnUrl;
        ObjData.agentId = this.agentId;
        ObjData.pan = this.pan;
        ObjData.pan1 = this.pan1;
        ObjData.pan2 = this.pan2;
        ObjData.pan3 = this.pan3;
        ObjData.pan4 = this.pan4;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class NSDLSoftCallBackSoft
{
    public string OrderID { get; set; }
    public string firstName { get; set; }
    public string middleName { get; set; }
    public string lastName { get; set; }
    public string fatherLastName { get; set; }
    public string fatherMiddleName { get; set; }
    public string fatherFirstName { get; set; }
    public string dob { get; set; }
    public string mobile { get; set; }
    public string email { get; set; }
    public string STID { get; set; }
    public string ack_no { get; set; }
    public string Amount { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public string AgentID { get; set; }
    public string TransactionDate { get; set; }
}





public class INCOMPLETEAPPLY
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string inc_txn_id { get; set; }
    public string txnid { get; set; }


    public string GetJson()
    {
        INCOMPLETEAPPLY ObjData = new INCOMPLETEAPPLY();
        ObjData.inc_txn_id = this.inc_txn_id;
        ObjData.txnid = this.txnid;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}