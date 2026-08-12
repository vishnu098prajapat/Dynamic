using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for CashFree
/// </summary>
public class CashFree
{
    public static string ClientID = "";
    public static string Secret = "";


    public static string ClientIDVan = "";
    public static string SecretVan = "";


    public static string ClientIDPayout = "";
    public static string SecretPayout = "";
    public static string URL = "https://api.cashfree.com/pg/";
    public static string URLPayout = "https://payout-api.cashfree.com/";
    public static string URLPan = "https://api.cashfree.com/verification/pan";


}

public class Createorders
{
    public int amount { get; set; }
    public string currency { get; set; }
    public string receipt { get; set; }

    public Createorders()
    {
        currency = "INR";
        receipt = "rcptid_11";
        //amount =100;
    }

    public string GetJson()
    {
        Createorders ObjData = new Createorders();
        ObjData.amount = this.amount;
        ObjData.currency = this.currency;
        ObjData.receipt = this.receipt;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class TokenCashFree
{
    public string orderId { get; set; }
    public int orderAmount { get; set; }
    public string orderCurrency { get; set; }

    public string GetJson()
    {
        TokenCashFree ObjData = new TokenCashFree();
        ObjData.orderId = this.orderId;
        ObjData.orderAmount = this.orderAmount;
        ObjData.orderCurrency = "INR";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class CustomerDetailsCashFee
{
    public string customer_id { get; set; }
    public string customer_email { get; set; }
    public string customer_phone { get; set; }
}

public class OrderMeta
{
    public string return_url { get; set; }
}

public class CashWebOrderID
{
    public string order_id { get; set; }
    public int order_amount { get; set; }
    public string order_currency { get; set; }
    public string order_note { get; set; }
    public CustomerDetailsCashFee customer_details { get; set; }
    public OrderMeta order_meta { get; set; }

    public string GetJson()
    {
        CashWebOrderID ObjData = new CashWebOrderID();
        ObjData.order_id = this.order_id;
        ObjData.order_amount = this.order_amount;
        ObjData.order_currency = this.order_currency;
        ObjData.order_note = this.order_note;
        ObjData.customer_details = this.customer_details;
        ObjData.order_meta = this.order_meta;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class CustomerDetailsPG
{
    public string name { get; set; }
    public string email { get; set; }
    public string contact { get; set; }
    public int amount { get; set; }

    public CustomerDetailsPG()
    {

    }

    public string GetJson()
    {
        CustomerDetailsPG ObjData = new CustomerDetailsPG();
        ObjData.name = this.name;
        ObjData.contact = this.contact;
        ObjData.email = this.email;
        ObjData.amount = this.amount;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class createVACshFree
{
    public string vAccountId { get; set; }
    public string name { get; set; }
    public string phone { get; set; }
    public string email { get; set; }
    public string notifGroup { get; set; }

    public string GetJson()
    {
        createVACshFree ObjData = new createVACshFree();
        ObjData.vAccountId = this.vAccountId;
        ObjData.name = this.name;
        ObjData.phone = this.phone;
        ObjData.email = this.email;
        ObjData.notifGroup = this.notifGroup;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class ApiPostCallCashFree
{
    public static string PostCall(string Json, string url)
    {
        var baseAddress = CashFree.URL + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();

        var request = new RestRequest(baseAddress,Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientID);
        request.AddHeader("x-client-secret", CashFree.Secret);
        request.AddHeader("Authorization", GetAuthToken());
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static string GetAuthToken()
    {
        try
        {
            var baseAddress = "https://cac-api.cashfree.com/cac/v1/authorize";
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            var options = new RestClientOptions("")
            {
                MaxTimeout = -1,
            };

            var client = new RestClient();

           
            var request = new RestRequest(baseAddress,Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("x-client-id", CashFree.ClientID);
            request.AddHeader("x-client-secret", CashFree.Secret);
            request.AddParameter("application/json", "{\r\n    \"status\": \"SUCCESS\",\r\n    \"message\": \"Token generated\",\r\n    \"subCode\": \"200\",\r\n    \"data\": {\"token\":\"" + System.Guid.NewGuid().ToString() + "\", \"expiry\":1564130052}\r\n}", ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            JObject jData = JObject.Parse(response.Content);
            return jData["data"]["token"].ToString();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static string PostCallWeb(string Json, string url)
    {
        var baseAddress = CashFree.URL + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();


        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientID);
        request.AddHeader("x-client-secret", CashFree.Secret);
        request.AddHeader("x-api-version", "2022-01-01");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }
    public static string CheckStatus(string OrderID)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();

        var request = new RestRequest("https://api.cashfree.com/pg/orders/" + OrderID + "/payments",Method.Get);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-api-version", "2021-05-21");
        request.AddHeader("x-client-id", CashFree.ClientID);
        request.AddHeader("x-client-secret", CashFree.Secret);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

}



public class ApiPostCallCashFreeVan
{
    public static string PostCall(string Json, string url)
    {
        var baseAddress = "https://cac-api.cashfree.com/" + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;


        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();


        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientIDVan);
        request.AddHeader("x-client-secret", CashFree.SecretVan);
        string Token = GetAuthToken1();
        cls_connection cls = new cls_connection();
        request.AddHeader("Authorization", "Bearer " + Token);
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static string GetAuthToken1()
    {
        var baseAddress = "https://cac-api.cashfree.com/cac/v1/authorize";
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();


        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientIDVan);
        request.AddHeader("x-client-secret", CashFree.SecretVan);
        request.AddParameter("application/json", "{\r\n    \"status\": \"SUCCESS\",\r\n    \"message\": \"Token generated\",\r\n    \"subCode\": \"200\",\r\n    \"data\": {\"token\":\"" + System.Guid.NewGuid().ToString() + "\", \"expiry\":1564130052}\r\n}", ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        JObject jData = JObject.Parse(response.Content);
        try
        {
            return jData["data"]["token"].ToString();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static string PostCallWeb(string Json, string url)
    {
        var baseAddress = CashFree.URL + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();


        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientIDVan);
        request.AddHeader("x-client-secret", CashFree.SecretVan);
        request.AddHeader("x-api-version", "2022-01-01");
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }
    public static string CheckStatus(string OrderID)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        
        var request = new RestRequest("https://api.cashfree.com/pg/orders/" + OrderID + "/payments",Method.Get);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-api-version", "2021-05-21");
        request.AddHeader("x-client-id", CashFree.ClientIDVan);
        request.AddHeader("x-client-secret", CashFree.SecretVan);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

}



public class BeneDetailsCashFree
{
    public string bankAccount { get; set; }
    public string ifsc { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string address1 { get; set; }
    public string vpa { get; set; }

}

public class PayoutRootCashFree
{
    public string amount { get; set; }
    public string transferId { get; set; }
    public string transferMode { get; set; }
    public string remarks { get; set; }
    public BeneDetailsCashFree beneDetails { get; set; }

    public string GetJson()
    {
        PayoutRootCashFree ObjData = new PayoutRootCashFree();
        ObjData.amount = this.amount;
        ObjData.transferId = this.transferId;
        ObjData.transferMode = this.transferMode;
        ObjData.beneDetails = this.beneDetails;
        ObjData.remarks = this.remarks;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class ApiPostCallCashFreePayout
{
    public static string PostCall(string Json, string url)
    {
        var baseAddress = CashFree.URLPayout + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();

        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientIDPayout);
        request.AddHeader("x-client-secret", CashFree.SecretPayout);
        request.AddHeader("Authorization", "Bearer " + GetAuthToken());
        request.AddParameter("application/json", Json, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }



    public static string PostCallGet(string url)
    {
        var baseAddress = CashFree.URLPayout + url;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();

        var request = new RestRequest(url, Method.Post);
        request.AddHeader("Content-Type", "application/json");

        request.AddHeader("Authorization", "Bearer " + GetAuthToken());

        RestResponse response = client.Execute(request);
        return response.Content;
    }
    public static string GetAuthToken()
    {
        var baseAddress = "https://payout-api.cashfree.com/payout/v1/authorize";
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions(baseAddress)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();

        var request = new RestRequest(baseAddress, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientIDPayout);
        request.AddHeader("x-client-secret", CashFree.SecretPayout);
        request.AddParameter("application/json", "{\r\n    \"status\": \"SUCCESS\",\r\n    \"message\": \"Token generated\",\r\n    \"subCode\": \"200\",\r\n    \"data\": {\"token\":\"" + System.Guid.NewGuid().ToString() + "\", \"expiry\":1564130052}\r\n}", ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        JObject jData = JObject.Parse(response.Content);
        return jData["data"]["token"].ToString();
    }

    public static string CheckStatus(string OrderID)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("https://api.cashfree.com/pg/orders/" + OrderID + "/payments")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();

        var request = new RestRequest("https://api.cashfree.com/pg/orders/" + OrderID + "/payments", Method.Get);

      
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-api-version", "2021-05-21");
        request.AddHeader("x-client-id", CashFree.ClientID);
        request.AddHeader("x-client-secret", CashFree.Secret);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

}





public class ApiPostCallCashFreePan
{
    public static string PostCall(string Pan)
    {
        var baseAddress = CashFree.URLPan;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();

        var request = new RestRequest(baseAddress, Method.Post);

        
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-client-id", CashFree.ClientIDPayout);
        request.AddHeader("x-client-secret", CashFree.SecretPayout);
        request.AddParameter("application/json", " {\r\n        \"name\":\"vipin\",\r\n        \"pan\":\"" + Pan + "\"\r\n    }", ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }



}