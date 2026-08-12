using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for PayWordDMT
/// </summary>
public class PayWordDMT
{
    public static string MerChantID = "4";
    public static string merchantUserId = "ddddd";
    public static string JWTsecretkey = "dddd";
    public PayWordDMT()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}


public class GetSenderDetailsRequest
{
    public string senderMobile { get; set; }
    public string merchantUserId { get; set; }

    public GetSenderDetailsRequest()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }

}

public class GetSenderDetailsRequestRoot
{
    public GetSenderDetailsRequest data { get; set; }
    public string GetJson()
    {
        GetSenderDetailsRequestRoot ObjData = new GetSenderDetailsRequestRoot();
        ObjData.data = this.data;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


/// Customer Registration
public class SenderBeneficiaryCustomerReg
{
    public string name { get; set; }
    public string accountNumber { get; set; }
    public string ifsc { get; set; }
    public string accountType { get; set; }
    public string mobileNo { get; set; }
    public string emailId { get; set; }



}

public class DataCustomerReg
{
    public string senderMobileNo { get; set; }
    public string senderGender { get; set; }
    public string senderName { get; set; }
    public string merchantUserId { get; set; }
    public SenderBeneficiaryCustomerReg senderBeneficiary { get; set; }

    public DataCustomerReg()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class CustomerRegRoot
{
    public DataCustomerReg data { get; set; }
    public string GetJson()
    {
        CustomerRegRoot ObjData = new CustomerRegRoot();
        ObjData.data = this.data;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}
/// </summary>

///VerifyOTP
public class VerifyOTPData
{
    public string senderId { get; set; }
    public string merchantUserId { get; set; }
    public string senderMobileNo { get; set; }
    public string beneficiaryId { get; set; }
    public string requestFor { get; set; }
    public string otp { get; set; }
    public string referenceNumber { get; set; }
    public VerifyOTPData()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class VerifyOTPRoot
{
    public VerifyOTPData data { get; set; }
}
///End

///Beneficairy Add After Result
public class BeneficairyAddAfterResult
{
    public string senderMobileNo { get; set; }
    public string senderId { get; set; }
    public string beneficiaryId { get; set; }
    public string msg { get; set; }
}
///

///Otp Send
public class OTPSend
{
    public string senderId { get; set; }
    public string merchantUserId { get; set; }
    public string senderMobileNo { get; set; }
    public string beneficiaryId { get; set; }
    public string requestFor { get; set; }
    public string referenceNumber { get; set; }

    public OTPSend()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class OTPSendRoot
{
    public OTPSend data { get; set; }
}
///

///Add beneficaiary
public class AddSenderBeneficiary
{
    public string name { get; set; }
    public string accountNumber { get; set; }
    public string ifsc { get; set; }
    public string accountType { get; set; }
    public string transferType { get; set; }
    public string mobileNo { get; set; }
}

public class AddBenData
{
    public string merchantUserId { get; set; }
    public string senderId { get; set; }
    public string senderMobileNo { get; set; }
    public AddSenderBeneficiary senderBeneficiary { get; set; }

    public AddBenData()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class AddBenDataRoot
{
    public AddBenData data { get; set; }
}
///


public class BenListData
{
    public string beneficiaryId { get; set; }
    public string name { get; set; }
    public string accountNumber { get; set; }
    public string ifsc { get; set; }
    public string active { get; set; }
    public string accountType { get; set; }
    public string lastValidatedAt { get; set; }
    public bool otpVerify { get; set; }
    public object transferType { get; set; }
}

public class BenListRoot
{
    public IList<BenListData> data { get; set; }
}



public class MoneyTransferData
{
    public string merchantUserId { get; set; }
    public string senderId { get; set; }
    public string beneficiaryId { get; set; }
    public string amount { get; set; }
    public string senderMobileNo { get; set; }
    public string referenceNumber { get; set; }
    public string transferType { get; set; }

    public MoneyTransferData()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class MoneyTransferRoot
{
    public MoneyTransferData data { get; set; }
    public string GetJson()
    {
        MoneyTransferRoot ObjData = new MoneyTransferRoot();
        ObjData.data = this.data;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

}


public class GetBalanceData
{
    public string merchantUserId { get; set; }
    public GetBalanceData()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class GetBalanceDataRoot
{
    public GetBalanceData data { get; set; }


}
public class CheckStatusData
{
    public string referenceNumber { get; set; }
    public string merchantUserId { get; set; }
    public CheckStatusData()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class CheckStatusDataRoot
{
    public CheckStatusData data { get; set; }
}


public class CheckStatusRequestData
{
    public string referenceNumber { get; set; }
    public string merchantUserId { get; set; }
    public CheckStatusRequestData()
    {
        merchantUserId = PayWordDMT.merchantUserId;
    }
}

public class CheckStatusRequestDataRoot
{
    public CheckStatusRequestData data { get; set; }
    public string GetJson()
    {
        CheckStatusRequestDataRoot ObjData = new CheckStatusRequestDataRoot();
        ObjData.data = this.data;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class ApiPostCallPayWordDMT
{

    public static string PostCall(string Json, string url)
    {
        var baseAddress = url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();

       
        var request = new RestRequest(baseAddress,Method.Post);


        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-merchant-id", "13");
        //request.AddHeader("merchantUserId", "1028");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);

        var response = client.Execute(request);
        return response.Content.ToString();



    }



}