using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for BalanceCheckService
/// </summary>
public class BalanceCheckService : IBalanceCheckService
{
    cls_connection ObjData;
    public BalanceCheckService()
    {
        ObjData = new cls_connection();
    }

    public async Task<string> GetBalance(string APIID)
    {
        if (APIID == "1")
            return await GetOtherAPI(APIID);
        else if (APIID == "3")
            return await GetBalanceBetaSource();
		else if (APIID == "2")
            return await GetBalancePayDMR();
        else if (APIID == "4")
            return await GetBalancerechargkit();
        else if (APIID == "6")
            return await GetBalanceclassypay();
        else
            return "";
    }


    public string apicall(string url)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(url, Method.Get);
        //request.AddHeader("Content-Type", "application/json");

        RestResponse response = client.Execute(request);

        return response.Content;
    }

    public async Task<string> GetOtherAPI(string APIID)
    {
        DataTable dtAPI = new DataTable();
        dtAPI = await ObjData.select_data_dtAsync("ProcRecharge_ManageAPI 'GetAll',  " + Convert.ToInt32(APIID) + "");
        //dtAPI = cls.select_data_dt("Select * from tblRecharge_API_POSTXMLCSV where apiid=" + litAPIID.Text + "");
        if (dtAPI.Rows[0]["BalanceURL"].ToString() != "")
        {
            string strBalanceAPI = "";
            if (dtAPI.Rows[0]["B_prm1"].ToString() != "" && dtAPI.Rows[0]["B_prm1val"].ToString() != "")
            {
                strBalanceAPI = dtAPI.Rows[0]["BalanceURL"].ToString() + dtAPI.Rows[0]["B_prm1"].ToString() + "=" + dtAPI.Rows[0]["B_prm1val"].ToString() + "&";
            }
            if (dtAPI.Rows[0]["B_prm2"].ToString() != "" && dtAPI.Rows[0]["B_prm2val"].ToString() != "")
            {
                strBalanceAPI = strBalanceAPI + dtAPI.Rows[0]["B_prm2"].ToString() + "=" + dtAPI.Rows[0]["B_prm2val"].ToString() + "&";
            }
            if (dtAPI.Rows[0]["B_prm3"].ToString() != "" && dtAPI.Rows[0]["B_prm3val"].ToString() != "")
            {
                strBalanceAPI = strBalanceAPI + dtAPI.Rows[0]["B_prm3"].ToString() + "=" + dtAPI.Rows[0]["B_prm3val"].ToString() + "&";
            }
            if (dtAPI.Rows[0]["B_prm4"].ToString() != "" && dtAPI.Rows[0]["B_prm4val"].ToString() != "")
            {
                strBalanceAPI = strBalanceAPI + dtAPI.Rows[0]["B_prm4"].ToString() + "=" + dtAPI.Rows[0]["B_prm4val"].ToString();
            }


            if (strBalanceAPI.EndsWith("&"))
                strBalanceAPI = strBalanceAPI.Substring(0, strBalanceAPI.Length - 1);
            //litBalanceURL.Text = strBalanceAPI;


            string result = apicall(strBalanceAPI);
            //string[] split = result.Split(',');

            string Success = Convert.ToString(dtAPI.Rows[0]["Success"]);
            string Failed = Convert.ToString(dtAPI.Rows[0]["Failed"]);
            string Pending = Convert.ToString(dtAPI.Rows[0]["Pending"]);
            string Status = "";
            string TypeData = ObjData.select_data_scalar_string("select B_prm4val from tblRecharge_API where APIID='" + dtAPI.Rows[0]["APIID"].ToString() + "'");
            if (TypeData == "XML")
            {
                result = "";// XMLTOJOSN.XmlToJSON(result);
                JObject ObjDataXMLKey = JObject.Parse(result);
                string[] xmlKetBala = dtAPI.Rows[0]["XMlKeyBalance"].ToString().Split(',');//Recharge_Credit,Status
                JObject ObjDataPOST = null;
                for (int i = 0; i < xmlKetBala.Length; i++)
                {
                    ObjDataPOST = JObject.Parse(ObjDataXMLKey["" + xmlKetBala[i] + ""].ToString());
                    ObjDataXMLKey = JObject.Parse(ObjDataXMLKey["" + xmlKetBala[i] + ""].ToString());
                }


                if (dtAPI.Rows[0]["StatusBalance"].ToString() != "")
                {
                    Status = ObjDataPOST["" + dtAPI.Rows[0]["StatusBalance"].ToString() + ""].ToString();
                    if (Status.ToString().ToLower() != Failed.ToLower())
                    {
                        return ObjDataPOST["" + dtAPI.Rows[0]["B_BalancePosition"].ToString() + ""].ToString();
                    }
                    else
                    {
                        return ObjDataPOST["" + dtAPI.Rows[0]["DescriptionBalance"].ToString() + ""].ToString();
                    }
                }
                else
                {
                    return ObjDataPOST["" + dtAPI.Rows[0]["B_BalancePosition"].ToString() + ""].ToString();
                }
            }
            else if (TypeData == "JSON")
            {
                JObject ObjDataPOST = JObject.Parse(result);

                if (dtAPI.Rows[0]["XMlKeyBalance"].ToString() != "")
                {
                    string[] xmlKetBala = dtAPI.Rows[0]["XMlKeyBalance"].ToString().Split(',');

                    for (int i = 0; i < xmlKetBala.Length; i++)
                    {
                        ObjDataPOST = JObject.Parse(ObjDataPOST["" + xmlKetBala[i] + ""].ToString());
                    }
                }

                if (dtAPI.Rows[0]["StatusBalance"].ToString() != "" && dtAPI.Rows[0]["StatusBalance"] != null)
                {
                    Status = ObjDataPOST["" + dtAPI.Rows[0]["StatusBalance"].ToString() + ""].ToString();
                    if (Status.ToString().ToLower() != Failed.ToLower())
                    {
                        return ObjDataPOST["" + dtAPI.Rows[0]["B_BalancePosition"].ToString() + ""].ToString();
                    }
                    else
                    {
                        return ObjDataPOST["" + dtAPI.Rows[0]["DescriptionBalance"].ToString() + ""].ToString();

                    }
                }
                else
                {
                    return ObjDataPOST["" + dtAPI.Rows[0]["B_BalancePosition"].ToString() + ""].ToString();
                }

            }
            else if (TypeData == "CSV")
            {
                string[] split = result.Split(Convert.ToChar(dtAPI.Rows[0]["Splitter"]));

                return split[Convert.ToInt32(dtAPI.Rows[0]["B_BalancePosition"].ToString())];
            }


        }
        return "0";


    }

    public async Task<string> GetBalanceBetaSource()
    {
        try
        {
            string requestBody = @"
        {
            ""MethodName"": ""balance"",
""APIID"": """",
""Token"": """"
        }";
            var result = await BetaSource.PostCall(requestBody);

            JObject jj = JObject.Parse(result);
            if (jj["code"]?.ToString() == "TXN")
            {
                return jj["data"]["Balance"]?.ToString() ?? "0.00";
            }
            else
            {
                return jj["mess"]?.ToString() ?? "Unknown error";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }


    public async Task<string> GetBalancerechargkit()
    {
        string Result = await rechargkit.GetBalanceAsync();
        JObject jj = JObject.Parse(Result);
        if (jj["error"].ToString().ToUpper() == "0")
            return jj["payout_wallet_amount"].ToString();
        else
            return jj["msg"].ToString();
    }

    public async Task<string> GetBalanceclassypay()
    {
        string Result = await classypay.GetCall("", "payment/v3/check-balance");
        JObject jj = JObject.Parse(Result);
        if (jj["status"].ToString().ToUpper() == "SUCCESS")
            return jj["data"]["balance"].ToString();
        else
            return jj["message"].ToString();
    }

    public async Task<string> GetBalancePayDMR()
    {
        try
        {
            string result = await PayDMR.GetBalance();
            JObject jj = JObject.Parse(result);

            string status = jj["status"]?.ToString() ?? "";

            if (status.ToUpper() == "SUCCESS")
            {
                string normalBalance = jj["balance"]?["normal_balance"]?.ToString() ?? "0.00";
                string cleanedBalance = normalBalance.Replace(",", "");
                return cleanedBalance;
            }
            else
            {
                return jj["message"]?.ToString() ?? "Balance fetch failed";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}