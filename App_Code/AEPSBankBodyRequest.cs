using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for AEPSBankBodyRequest
/// </summary>
public class AEPSBankBodyRequest
{
    //Aeps 
    AEPSBankCall aEPSBankCall = new AEPSBankCall();

    public string GetBalance()
    {
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string Result = aEPSBankCall.GetCall("balance/balance/aepsbalance", jWTBankSendRequest.GetToken());
        return Result;
    }

    public string GetBankList()
    {
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        cls_connection objConnection = new cls_connection();


        string Result = aEPSBankCall.GetCall("balance/balance/banklist", jWTBankSendRequest.GetToken());
        return Result;
    }

    public string BalanceEnquiry(BALANCEENQUIRY bALANCEENQUIRY)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(bALANCEENQUIRY.GetJson(), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESKEY), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESIV));

        string Result = aEPSBankCall.PostCall("enquiry/balanceenquiryjson/index", jWTBankSendRequest.GetToken(), BodyEncrptio);        
        return Result;
    }

    public string CashWithdraw(CashWithdraw cashWithdraw)
    {
        cls_connection objConnection = new cls_connection();

        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(cashWithdraw.GetJson(), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESKEY), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESIV));

        string Result = aEPSBankCall.PostCall("withdraw/cashwithdrawaljson/index", jWTBankSendRequest.GetToken(), BodyEncrptio);


        return Result;
    }
    //End Aeps

    //MINISTATEMENT
    public string GetStatelist()
    {
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string Result = aEPSBankCall.GetCallLive("merchantonboard/state/statelist", jWTBankSendRequest.GetLiveToken());
        return Result;
    }

    public string MerchantOnboarding(MerchantOnboarding merchantOnboarding)
    {
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        jWTBankSendRequest.SetLiveParameter();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(merchantOnboarding.GetJson(), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESKEYLive), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESIVLive));

        string Result = aEPSBankCall.PostCallLive("merchantonboard/Merchantonboardjson/index", jWTBankSendRequest.GetLiveToken(), BodyEncrptio);

        return Result;
    }

    public string GetMinistatementBankList()
    {
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string Result = aEPSBankCall.GetCallLive("balance/balance/banklist", jWTBankSendRequest.GetLiveToken());
        return Result;
    }
    public string MiniStatement(MiniStatement1 miniStatement)
    {
        cls_connection objConnection = new cls_connection();

        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(miniStatement.GetJson(), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESKEY), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESIV));

        string Result = aEPSBankCall.PostCall("aeps/ministatement/index", jWTBankSendRequest.GetToken(), BodyEncrptio);


        return Result;
    }
    //End

    public string AADHARPAY(AADHARPAY aADHARPAY)
    {
        cls_connection objConnection = new cls_connection();

        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(aADHARPAY.GetJson(), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESKEY), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESIV));

        string Result = aEPSBankCall.PostCall("aadharpay/aadharpayjson/index", jWTBankSendRequest.GetToken(), BodyEncrptio);


        return Result;
    }

    ////public string CheckStatusMATM(MATMCheckStatus mATMCheckStatus)
    ////{
    ////    cls_connection objConnection = new cls_connection();
    ////    JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
    ////    jWTBankSendRequest.SetLiveParameter();
    ////    jWTBankSendRequest.product = "MATM";

    ////    string BodyEncrptio = Aes256CbcEncrypter.EncryptString(mATMCheckStatus.GetJson(), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESKEYLive), Encoding.ASCII.GetBytes(AEPSBankCredentials.AESIVLive));
    ////    string Result = aEPSBankCall.PostCallLive("matm/matmquery/query", jWTBankSendRequest.GetLiveTokenMATM(), BodyEncrptio);
    ////    objConnection.update_data("insert into tbl_Apeslog(url)values('" + Result + "')");
    ////    return Result;
    ////}
}


public class CommonAEPSProperty
{
    public long timestamp { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public string transcationtype { get; set; }
    public string nationalbankidentification { get; set; }
    public string requestremarks { get; set; }
    public string mobilenumber { get; set; }
    public string adhaarnumber { get; set; }
    public string referenceno { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }
    public CommonAEPSProperty()
    {
        timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        requestremarks = "Successful";
    }
}



public class BALANCEENQUIRY : CommonAEPSProperty
{

    public string GetJson()
    {
        BALANCEENQUIRY ObjData = new BALANCEENQUIRY();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = this.timestamp;
        ObjData.transcationtype = this.transcationtype;
        ObjData.latitude = objLocation.Latitude;
        ObjData.longitude = objLocation.Longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = this.requestremarks;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = objLocation.IPAddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class CashWithdraw : CommonAEPSProperty
{

    public int amount { get; set; }

    public string GetJson()
    {
        CashWithdraw ObjData = new CashWithdraw();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = this.timestamp;
        ObjData.transcationtype = this.transcationtype;
        ObjData.latitude = objLocation.Latitude;
        ObjData.longitude = objLocation.Longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = this.requestremarks;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = objLocation.IPAddress;
        ObjData.amount = this.amount;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class AADHARPAY 
{

    public long timestamp { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public string nationalbankidentification { get; set; }
    public string requestremarks { get; set; }
    public string mobilenumber { get; set; }
    public string transcationtype { get; set; }
    public string adhaarnumber { get; set; }
    public string referenceno { get; set; }
    public string accessmodetype { get; set; }
    public int amount { get; set; }

    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }
    public string pipe { get; set; }
    public string is_iris { get; set; }

    public string GetJson()
    {
        AADHARPAY ObjData = new AADHARPAY();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        ObjData.transcationtype = this.transcationtype;
        ObjData.latitude = objLocation.Latitude;
        ObjData.longitude = objLocation.Longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = "Successful";
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = objLocation.IPAddress;
        ObjData.amount = this.amount;
        ObjData.pipe = "bank1";
        ObjData.is_iris = "No";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class MerchantOnboarding
{
    public string timestamp { get; set; }
    public string phone { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public string name { get; set; }
    public string firmname { get; set; }
    public string email { get; set; }
    public string pincode { get; set; }
    public string address { get; set; }
    public int stateid { get; set; }
    public string pannumber { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string city { get; set; }

    public MerchantOnboarding()
    {
        timestamp = Convert.ToString(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
    }
    public string GetJson()
    {
        MerchantOnboarding ObjData = new MerchantOnboarding();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = this.timestamp;
        ObjData.latitude = objLocation.Latitude;
        ObjData.longitude = objLocation.Longitude;
        ObjData.name = this.name;
        ObjData.phone = this.phone;
        ObjData.firmname = this.firmname;
        ObjData.email = this.email;
        ObjData.city = this.city;
        ObjData.pincode = this.pincode;
        ObjData.address = this.address;
        ObjData.stateid = this.stateid;
        ObjData.pannumber = this.pannumber;
        ObjData.stateid = this.stateid;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = objLocation.IPAddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}

public class MiniStatement
{
    public long timestamp { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    //public string transcationtype { get; set; }
    public string nationalbankidentification { get; set; }
    public string requestremarks { get; set; }
    public string mobilenumber { get; set; }
    public string adhaarnumber { get; set; }
    public string referenceno { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string data { get; set; }

    public string GetJson()
    {
        MiniStatement ObjData = new MiniStatement();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        ObjData.latitude = objLocation.Latitude;
        ObjData.longitude = objLocation.Longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = "Successful";
        ObjData.mobilenumber = this.mobilenumber;
        //ObjData.transcationtype = this.transcationtype;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = objLocation.IPAddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class MATMCheckStatus
{
    public string reference { get; set; }
    public string GetJson()
    {
        MATMCheckStatus ObjData = new MATMCheckStatus();
        ObjData.reference = this.reference;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class MinistatementOutPut
{
    public string date { get; set; }
    public string txnType { get; set; }
    public string amount { get; set; }
    public string narration { get; set; }
}

public class MinistatementOutPutRoot
{
    public bool status { get; set; }
    public int ackno { get; set; }
    public long datetime { get; set; }
    public double balanceamount { get; set; }
    public string bankrrn { get; set; }
    public string message { get; set; }
    public IList<MinistatementOutPut> ministatement { get; set; }
    public string statementtype { get; set; }
    public int response { get; set; }
    public int responsecode { get; set; }
}
