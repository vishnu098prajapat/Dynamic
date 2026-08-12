using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
/// <summary>
/// Summary description for AEPSBankBodyRequestNew
/// </summary>
public class AEPSBankBodyRequestNew
{
    AEPSBankCall aEPSBankCall = new AEPSBankCall();
    string AESKEY = "";//"d84bc53c5920ce12";
    string AESIV = "";//"4de273406ca97161";
    public string GetBalance()
    {
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string Result = PaysPrintPostCall.PostCall("aeps/banklist/index", JWTBankSendRequestNewIntegration.GetToken());
        return Result;
    }

    public string GetBankList()
    {
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string Result = PaysPrintPostCall.PostCall("aeps/banklist/index", JWTBankSendRequestNewIntegration.GetToken());
        return Result;
    }
    public string ThreeCornMATMBackResponse(string ID, string Status)
    {
        string Re = "{\"reference\":\"" + ID + "\",\"status\":\"" + Status + "\"}";
        JWTBankSendRequestNewIntegration jWTBankSendRequest = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(Re, Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string Result = PaysPrintPostCall.PostCallBodyEncrption("matm/threeway/update", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }

    public string CommonUse(CashWithdraw1 cashWithdraw, string Method)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(cashWithdraw.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption(Method, JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }

    public static async System.Threading.Tasks.Task<string> MerchantRegistrationekycApp(cls_connection ObjConnection, DataTable dtMember, AEPSTWORegistration aEPS, string pipe)
    {
        DataTable dtreAEPSData = await ObjConnection.select_data_dtAsync("select * from AEPSOnBoardStatus where Name='" + pipe + "' and msrno=" + dtMember.Rows[0]["msrno"].ToString() + "");
        if (dtreAEPSData.Rows.Count > 0)
        {
            string Memjid = dtreAEPSData.Rows[0]["AEPSID"].ToString();
            JWTBankSendRequestNewIntegration dd = new JWTBankSendRequestNewIntegration();
            string BodyEncrptio = Aes256CbcEncrypter.EncryptString(aEPS.data, Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

            object re = new
            {
                merchantcode = Memjid,
                aadhaar = dtMember.Rows[0]["Aadhar"].ToString(),
                piddata = BodyEncrptio,
                dob = dtMember.Rows[0]["DOB"].ToString(),
            };
            string Request = Newtonsoft.Json.JsonConvert.SerializeObject(re);

            ObjConnection.update_data("insert into logdata(data)values('" + Request + "')");


            string Result = PaysPrintPostCall.PostCallWithBody("onboard/onboard/activate_merchant", dd.GetToken(), Request);
            ObjConnection.update_data("insert into logdata(data)values('" + Result + "')");
            JObject Data = JObject.Parse(Result);
            if (Convert.ToBoolean(Data["status"].ToString()))
            {
                ObjConnection.update_data("update AEPSOnBoardStatus set DoKyc=1 where msrno='" + dtMember.Rows[0]["msrno"].ToString() + "' and name='" + pipe + "'");
                return ErrorShow.SuccessArray(Result);
            }
            else
            {
                return ErrorShow.ErrorMessage(Data["message"].ToString().Replace("\n", ""));
            }
        }
        else
        {
            return ErrorShow.ErrorMessage("No Record Found");
        }
    }


    public static async Task<string> BECWMS(AEPSRequest aEPSRequest)
    {
        IAEPSService aEPSService = new AEPSService();
        string Result = await aEPSService.BE(aEPSRequest);
        return Result;
    }
    public static async System.Threading.Tasks.Task<string> AuthenticateRegistration(cls_connection ObjConnection, DataTable dtMember, AEPSTWORegistration aEPS, string pipe)
    {
        DataTable dtreAEPSData =await ObjConnection.select_data_dtAsync("select * from AEPSOnBoardStatus where Name='" + pipe + "' and msrno=" + dtMember.Rows[0]["msrno"].ToString() + "");
        if (dtreAEPSData.Rows.Count > 0)
        {
            string ipAddress = ConstantsData.getipAddress();
            string RefID = ConstantsData.GenerateUtrAsync();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            JWTBankSendRequestNewIntegration ff = new JWTBankSendRequestNewIntegration();
            var options = new RestClientOptions("https://api.paysprint.in/api/v1/service/aepsbodyforpartner/encodebody")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("https://api.paysprint.in/api/v1/service/aepsbodyforpartner/encodebody", Method.Post);
            request.AddHeader("Token", ff.GetToken());
            request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
            request.AddHeader("Cookie", "secure");
            request.AddParameter("accessmodetype", "SITE");
            request.AddParameter("adhaarnumber", dtMember.Rows[0]["Aadhar"].ToString());
            request.AddParameter("mobilenumber", dtMember.Rows[0]["Mobile"].ToString());
            request.AddParameter("latitude", dtMember.Rows[0]["Latitude"].ToString());
            request.AddParameter("longitude", dtMember.Rows[0]["Longitude"].ToString());
            request.AddParameter("referenceno", RefID);
            string Memjid = dtreAEPSData.Rows[0]["AEPSID"].ToString();

            request.AddParameter("submerchantid", Memjid);
            request.AddParameter("timestamp", DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"));
            request.AddParameter("data", HttpUtility.HtmlDecode(aEPS.data));
            request.AddParameter("ipaddress", ipAddress);
            request.AddParameter("key", PaysPrint.AESKEY);
            request.AddParameter("iv", PaysPrint.AESIV);
            RestResponse response = client.Execute(request);
            JObject jj = JObject.Parse(response.Content);
            JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
            string BodyEncrptio = jj["body"].ToString();
            string Result = "";
            if (pipe.ToUpper() == "BANK2")
            {
                Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/kyc/Twofactorkyc/authentication", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
            }
            else
            {
                Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/kyc/Twofactorkyc/auth_login", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
            }
            JObject Data = JObject.Parse(Result);
            if (Convert.ToBoolean(Data["status"].ToString()))
            {
                ObjConnection.update_data("update AEPSOnBoardStatus set DailyAuthentication=1,AuthDate=getdate() where msrno='" + dtMember.Rows[0]["msrno"].ToString() + "' and Name='" + pipe + "'");

                return ErrorShow.SuccessArray(Result);
            }
            else
            {
                if (Data["errorcode"] != null)
                {
                    if (Data["errorcode"].ToString() == "2")
                    {
                        ObjConnection.update_data("update AEPSOnBoardStatus set DailyAuthentication=1,AuthDate=getdate() where msrno='" + dtMember.Rows[0]["msrno"].ToString() + "' and Name='" + pipe + "'");
                        return ErrorShow.SuccessArray(Result);
                    }
                    else
                        return ErrorShow.ErrorMessage(Data["message"].ToString().Replace("\n", ""));
                }
                else
                {

                    return ErrorShow.ErrorMessage(Data["message"].ToString().Replace("\n", ""));
                }
            }
        }
        else
        {
            return ErrorShow.ErrorMessage("No Record Found");
        }
    }

    public static async System.Threading.Tasks.Task<string> MerchantRegistration(cls_connection ObjConnection, DataTable dtMember, AEPSTWORegistration aEPS, string pipe)
    {
        DataTable dtreAEPSData =await ObjConnection.select_data_dtAsync("select * from AEPSOnBoardStatus where Name='" + pipe + "' and msrno=" + dtMember.Rows[0]["msrno"].ToString() + "");
        if (dtreAEPSData.Rows.Count > 0)
        {
            string ipAddress = ConstantsData.getipAddress();
            string RefID = ConstantsData.GenerateUtrAsync();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            JWTBankSendRequestNewIntegration ff = new JWTBankSendRequestNewIntegration();
            var options = new RestClientOptions("https://api.paysprint.in/api/v1/service/aepsbodyforpartner/encodebody")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("https://api.paysprint.in/api/v1/service/aepsbodyforpartner/encodebody", Method.Post);
            request.AddHeader("Token", ff.GetToken());
            request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
            request.AddHeader("Cookie", "secure");
            request.AddParameter("accessmodetype", "SITE");
            request.AddParameter("adhaarnumber", dtMember.Rows[0]["Aadhar"].ToString());
            request.AddParameter("mobilenumber", dtMember.Rows[0]["Mobile"].ToString());
            request.AddParameter("latitude", dtMember.Rows[0]["Latitude"].ToString());
            request.AddParameter("longitude", dtMember.Rows[0]["Longitude"].ToString());
            request.AddParameter("referenceno", RefID);
            string Memjid = dtreAEPSData.Rows[0]["AEPSID"].ToString();

            request.AddParameter("submerchantid", Memjid);
            request.AddParameter("timestamp", DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"));
            request.AddParameter("data", HttpUtility.HtmlDecode(aEPS.data));
            request.AddParameter("ipaddress", ipAddress);
            request.AddParameter("key", PaysPrint.AESKEY);
            request.AddParameter("iv", PaysPrint.AESIV);
            RestResponse response = client.Execute(request);
            JObject jj = JObject.Parse(response.Content);
            JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
            string BodyEncrptio = jj["body"].ToString();
            string Result = "";
            if (pipe.ToUpper() == "BANK2")
            {
                Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/kyc/Twofactorkyc/registration", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
            }
            else
            {
                Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/kyc/Twofactorkyc/register_agent", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
            }
            JObject Data = JObject.Parse(Result);
            if (Convert.ToBoolean(Data["status"].ToString()))
            {
                ObjConnection.update_data("update AEPSOnBoardStatus set AEPSTwoWayRegistration=1 where msrno='" + dtMember.Rows[0]["msrno"].ToString() + "' and name='" + pipe + "'");

                return ErrorShow.SuccessArray(Result);
            }
            else
            {
                return ErrorShow.ErrorMessage(Data["message"].ToString().Replace("\n", ""));
            }
        }
        else
        {
            return ErrorShow.ErrorMessage("No Record Found");
        }
    }



    public static async System.Threading.Tasks.Task<string> AuthenticateRegistrationCW(cls_connection ObjConnection, DataTable dtMember, AEPSTWORegistration aEPS, string pipe)
    {
        DataTable dtreAEPSData =await ObjConnection.select_data_dtAsync("select * from AEPSOnBoardStatus where Name='" + pipe + "' and msrno=" + dtMember.Rows[0]["msrno"].ToString() + "");
        if (dtreAEPSData.Rows.Count > 0)
        {
            string ipAddress = ConstantsData.getipAddress();
            string RefID = ConstantsData.GenerateUtrAsync();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            JWTBankSendRequestNewIntegration ff = new JWTBankSendRequestNewIntegration();
            var options = new RestClientOptions("https://api.paysprint.in/api/v1/service/aepsbodyforpartner/encodebody")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("https://api.paysprint.in/api/v1/service/aepsbodyforpartner/encodebody", Method.Post);
            request.AddHeader("Token", ff.GetToken());
            request.AddHeader("Authorisedkey", PaysPrint.AuthKey);
            request.AddHeader("Cookie", "secure");
            request.AddParameter("accessmodetype", "SITE");
            request.AddParameter("adhaarnumber", dtMember.Rows[0]["Aadhar"].ToString());
            request.AddParameter("mobilenumber", dtMember.Rows[0]["Mobile"].ToString());
            request.AddParameter("latitude", dtMember.Rows[0]["Latitude"].ToString());
            request.AddParameter("longitude", dtMember.Rows[0]["Longitude"].ToString());
            request.AddParameter("referenceno", RefID);
            string Memjid = dtreAEPSData.Rows[0]["AEPSID"].ToString();

            request.AddParameter("submerchantid", Memjid);
            request.AddParameter("timestamp", DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"));
            request.AddParameter("data", HttpUtility.HtmlDecode(aEPS.data));
            request.AddParameter("ipaddress", ipAddress);
            request.AddParameter("key", PaysPrint.AESKEY);
            request.AddParameter("iv", PaysPrint.AESIV);
            RestResponse response = client.Execute(request);
            JObject jj = JObject.Parse(response.Content);
            JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
            string BodyEncrptio = jj["body"].ToString();
            string Result = "";
            if (pipe.ToUpper() == "BANK2")
            {
                Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/kyc/Twofactorkyc/merchant_authencity", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
            }
            else
            {
                Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/kyc/Twofactorkyc/agent_authencity", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
            }


            JObject Data = JObject.Parse(Result);
            if (Convert.ToBoolean(Data["status"].ToString()))
            {
                ObjConnection.update_data("update AEPSOnBoardStatus set MerAuthTxnId='" + Data["MerAuthTxnId"].ToString() + "',IsUserMerAuthTxnId=0,DailyAuthentication=1,AuthDate=getdate() where msrno='" + dtMember.Rows[0]["msrno"].ToString() + "' and Name='" + pipe + "'");

                return ErrorShow.SuccessArray(Result);
            }
            else
            {
                return ErrorShow.ErrorMessage(Data["message"].ToString().Replace("\n", ""));
            }
        }
        else
        {
            return ErrorShow.ErrorMessage("No Record Found");
        }
    }
   



    public string BalanceEnquiry(BALANCEENQUIRY1 bALANCEENQUIRY)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(bALANCEENQUIRY.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/balanceenquiry/index", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }
    public void ThreeCornAEPSCW(string ID, string Status)
    {
        string Re = "{\"reference\":\"" + ID + "\",\"status\":\"" + Status + "\"}";
        JWTBankSendRequestNewIntegration jWTBankSendRequest = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(Re, Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/threeway/threeway", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);

    }
    public string CheckStatus(CheckSTatusRNF bALANCEENQUIRY)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(bALANCEENQUIRY.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/aepsquery/query", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }


    public string CheckStatusAadhar(CheckSTatusRNF bALANCEENQUIRY)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(bALANCEENQUIRY.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption("aadharpay/aadharpayquery/query", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }
    public string authcashwithdrawbank2(CashWithdrawNewAuth cashWithdraw)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(cashWithdraw.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/authcashwithdraw/index", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }
    public string CashWithdraw(CashWithdraw1 cashWithdraw)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(cashWithdraw.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/cashwithdraw/index", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }
    //End Aeps

    //MINISTATEMENT
    public string GetStatelist()
    {
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string Result = aEPSBankCall.GetCallLive("merchantonboard/state/statelist", JWTBankSendRequestNewIntegration.GetLiveToken());
        return Result;
    }

    public string MerchantOnboarding(MerchantOnboarding merchantOnboarding)
    {
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        JWTBankSendRequestNewIntegration.SetLiveParameter();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(merchantOnboarding.GetJson(), Encoding.ASCII.GetBytes(AESKEY), Encoding.ASCII.GetBytes(AESIV));

        string Result = aEPSBankCall.PostCallLive("merchantonboard/Merchantonboardjson/index", JWTBankSendRequestNewIntegration.GetLiveToken(), BodyEncrptio);

        return Result;
    }

    public string GetMinistatementBankList()
    {
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string Result = aEPSBankCall.GetCallLive("balance/balance/banklist", JWTBankSendRequestNewIntegration.GetLiveToken());
        return Result;
    }

    public string AADHARPAY(AADHARPAY1 aADHARPAY)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(aADHARPAY.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption("aadharpay/aadharpay/index", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;
    }

    public string MiniStatement(MiniStatement1 miniStatement)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(miniStatement.GetJson(), Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));

        string Result = PaysPrintPostCall.PostCallBodyEncrption("aeps/ministatement/index", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);
        return Result;


    }
    //End

    public string CheckStatusMATM(MATMCheckStatus mATMCheckStatus)
    {
        cls_connection objConnection = new cls_connection();
        JWTBankSendRequestNewIntegration jWTBankSendRequest = new JWTBankSendRequestNewIntegration();

        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(mATMCheckStatus.GetJson(), Encoding.ASCII.GetBytes(AESKEY), Encoding.ASCII.GetBytes(AESIV));
        string Result = PostCall("https://paysprint.in/service-api/api/v1/service/matm/matmquery/query", jWTBankSendRequest.GetToken(), BodyEncrptio);

        return Result;
    }

    public void ThreeCornMATM(string ID, string Status)
    {
        string Re = "{\"reference\":\"" + ID + "\",\"status\":\"" + Status + "\"}";
        JWTBankSendRequestNewIntegration jWTBankSendRequest = new JWTBankSendRequestNewIntegration();
        string BodyEncrptio = Aes256CbcEncrypter.EncryptString(Re, Encoding.ASCII.GetBytes(PaysPrint.AESKEY), Encoding.ASCII.GetBytes(PaysPrint.AESIV));
        JWTBankSendRequestNewIntegration JWTBankSendRequestNewIntegration = new JWTBankSendRequestNewIntegration();
        string Result = PaysPrintPostCall.PostCallBodyEncrption("matm/threeway/update", JWTBankSendRequestNewIntegration.GetToken(), BodyEncrptio);

    }

    public string PostCall(string url, string Token, string Body)
    {

        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(url, Method.Post);

        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Token", Token);
        request.AddHeader("PARTNERID", "PS0046");//PS0033
        request.AddHeader("Authorisedkey", "MzAyMTY3ODRQUzAwNDY2ZTI1NTE1NzM5MWZkY2VhMmY1NjI1Yjc0OTJmNWZkZA==");
        request.AddParameter("application/json", "{\"body\":\"" + Body + "\"}", ParameterType.RequestBody);
        RestResponse response = client.Execute(request);

        return response.Content.ToString();
    }
    public string PostCallLive(string url, string Token, string Body)
    {


        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(AEPSBankCredentials.URLLive + url, Method.Post);

        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Token", Token);
        request.AddHeader("PARTNERID", AEPSBankCredentials.PARTNERIDLive);
        request.AddParameter("application/x-www-form-urlencoded", "{\"body\":\"" + Body + "\"}", ParameterType.RequestBody);
        //request.AddParameter("body", Data);
        RestResponse response = client.Execute(request);


        return response.Content.ToString();
    }

}



public class CashWithdrawNewAuth : CommonAEPSProperty1
{

    public int amount { get; set; }
    public string MerAuthTxnId { get; set; }

    public string GetJson()
    {
        CashWithdrawNewAuth ObjData = new CashWithdrawNewAuth();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = this.timestamp;
        ObjData.transactiontype = this.transactiontype;
        ObjData.MerAuthTxnId = this.MerAuthTxnId;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = this.requestremarks;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.pipe = this.pipe;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        ObjData.amount = this.amount;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class CommonAEPSProperty1
{
    public long timestamp { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public string transactiontype { get; set; }
    public string nationalbankidentification { get; set; }
    public string requestremarks { get; set; }
    public string mobilenumber { get; set; }
    public string adhaarnumber { get; set; }
    public string referenceno { get; set; }
    public string accessmodetype { get; set; }
    public string submerchantid { get; set; }
    public string ipaddress { get; set; }
    public string pipe { get; set; }
    public string data { get; set; }
    public string is_iris { get; set; }
    public CommonAEPSProperty1()
    {
        //Location objLocation = new Location();
        //objLocation = objLocation.GetLocationData();
        timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        longitude = "77.2167";
        latitude = "28.6667"; // objLocation.Latitude;
        accessmodetype = "APP";
        requestremarks = "Successful";
        submerchantid = "";
        ipaddress = "103.148.156.93";
        pipe = "bank1";
        is_iris = "No";
        //data = "<?xml version=\"1.0\"?><PidData><ResperrCode = \"0\" errInfo = \"Success\" fCount = \"1\" fType = \"0\" nmPoints = \"41\" qScore = \"67\" /><DeviceInfodpId = \"MANTRA.MSIPL\" rdsId = \"MANTRA.WIN.001\" rdsVer = \"1.0.2\" mi = \"MFS100\" mc = \"MIIEGDCCAwCgAwIBAgIEAas/ADANBgkqhkiG9w0BAQsFADCB6jEqMCgGA1UEAxMhRFMgTWFudHJhIFNvZnRlY2ggSW5kaWEgUHZ0IEx0ZCA3MUMwQQYDVQQzEzpCIDIwMyBTaGFwYXRoIEhleGEgb3Bwb3NpdGUgR3VqYXJhdCBIaWdoIENvdXJ0IFMgRyBIaWdod2F5MRIwEAYDVQQJEwlBaG1lZGFiYWQxEDAOBgNVBAgTB0d1amFyYXQxHTAbBgNVBAsTFFRlY2huaWNhbCBEZXBhcnRtZW50MSUwIwYDVQQKExxNYW50cmEgU29mdGVjaCBJbmRpYSBQdnQgTHRkMQswCQYDVQQGEwJJTjAeFw0yMDEwMTQxNjM4NTRaFw0yMDExMTMxNjUzMjhaMIGwMSUwIwYDVQQDExxNYW50cmEgU29mdGVjaCBJbmRpYSBQdnQgTHRkMR4wHAYDVQQLExVCaW9tZXRyaWMgTWFudWZhY3R1cmUxDjAMBgNVBAoTBU1TSVBMMRIwEAYDVQQHEwlBSE1FREFCQUQxEDAOBgNVBAgTB0dVSkFSQVQxCzAJBgNVBAYTAklOMSQwIgYJKoZIhvcNAQkBFhVzdXBwb3J0QG1hbnRyYXRlYy5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDlqJbRkgMuuuqJ+3ybzWtNbH6qe5pHrhE+ncWWSkNO/fySgNxARdN+HzUxi1adQlrcANhgZ9KBGIKs6Z3994v/9EtlEeGoNq2sX+WHXKZUwAzZhDfbu+7DYnF/nq2i39s4JjjgOAotrF97qUNXCXzty5EUAkwUqyn1nADH8ePfbmQSm791s0qIdeRZNlsOxRKyckwP5yW4gdNEwucd/S34AEAKJyj5hsEMwDJMXO/uKs0wfhCsZVSl5FdpB3YBulggCo2Hmgx2H+QccdpFS+YKbSjrbaZHn6ZWGco67oRlIfHUqXSuhUrkPN0pGd5phIgLDkOxPClhvGDdryBEzDO3AgMBAAEwDQYJKoZIhvcNAQELBQADggEBAEiCiONDTQiDbiTiN05SRl65pfBi4BhPmBJ/laD3APwUAVbsWdWGCvKKVtJ8xCXGP0UTntRQK8AS2ihQJG00socJkDcKU/iuIb6EjYUQK6o+ElURV3Uz+9au9WrnSGNh5woFY4c5kUmf7q8NaTiE9PuC20ZolUgygptBKfnGlOefEwkUQ1FWbKsTm46qLCsoO5xkRduPdsqABlHvM7efeSG/9Yd8qDb5JZ14Rdr9wXZviaUmRjPncIwuVjRV7S1yJKbT4wqEMp1IJ6A3q0gGl1PKo+3Fm/3fqJK9IgKFiwb5i5CmFkKwXcANZwFIeOloRVcJS4/kbj6gl1sCjs7rySg=\" dc = \"600da087-d6d4-436c-8fee-8cab0b40bdd8\" >                      <additional_info ><Param name = \"srno\" value = \"2088165\" /><Param name = \"sysid\" value = \"6E9FEBF168230BBFBFF0\" /><Param name = \"ts\" value = \"2020-10-14T23:13:01+05:30\" /></additional_info></DeviceInfo><Skey ci = \"20221021\" > eSX6zO / l + xcVrVaD0PbxYsxEnRRx + vXVHdnqJPqyfYvZtWl5IE / BrBE5K03IDDZ9PlgyG2ERH6DPYtk + nuWU4ukMR8rWhyjsAM3BAgetbTtgM7 + Q2rXaopol89yUIWWI7N6b8Cae9ohfkDjaVapugK / eUwvS / FSRNhM68v4uoNzYFU1 / 4thtWiZ3qFT7 + Kuej / SAYusGdSmhXeE / ucpNp6gXe / yvQPBcQuVnHoPT4BpsFMlDC64Ae6NPZyIppdZratHDB7nLNTdAn9nyoGZmqlANa / MKmI6EK4wkRQx + 3KU4QO58FvtMH3PFEDonf3JincwyyesVcfxZyWCvLoEUfw ==</ Skey ><Hmac> Zp1dk / EURvKfMcdWgBQmMa + mmuKyo + YFyIz9wOE1bIe1oP3LwYzbX6eZ + ZkJ5rZ5 </ Hmac ><Data type = \"X\" > MjAyMC0xMC0xNFQyMzoxMzowMdwC8jc7cS9lQb9kHbINer18dYsIiksPRXhLhub8 / 5G6Xa6T93eco392eCxYgmsJBiH11xyzZ3qty3rBcQNI9Zz8MqJH4R6YB1iJQ + DEwJmHiR5hJuf0nZTxGnGfBIP8q + nV4oDH5HfcSl7zipCpaS9wfcHvrgS1jJWe1XeVmknlXStosjEEhs8esxgED44J0Geu8Kl + BZ7CCQXgbWz8BW1F1DFmoCFLjt534xFBn//L8iyUOQsspQOiwkKKM/JCtgOi4DKr1iHTdpfd8q+dpt9RrgMZnCcuUbQSdBqKsgF6SlqlsbPU1AnfqnxqYpp65S7JrkHNWhwwP0KvcEdrz2pSaRUKasdOT1NmQlxuEp7x0GKxis/s5kGWXpwLrv2NjqyXplRvUmYpt0t7xbhr4AE6+V5AjPipoz1zBbcG42WzHpNxUPFqYPj0AywmhE6m/Jl4Nrwa90HrS4hYltL0ll8iOhLUrpiMFupqkom2aFjRKg7/ALe/QA4Gq1zHYRb4qu/9hRrTUc7s/EzoNbh3zqLDu67xdJ9NTHo4cLXJpA1F/BFqoEjg7HGm5xCNtEuF+8eckmClraPHI1yhxKECT556lOrQ/0+sMhDPxC+eLFziyOU+700P3WcgMoYC4qM1BaEyIXM4oZYc+A5UEqwPz5QpMFpWZITXH8DnZ9A+JKqQLeGvWq/zp/DebCegTCh0Zo7peDaUk2Vq5G/4VdaBzeS1okvMk/RpIuZR5i9T4fPi9yCv1XoBl18y2U+zJ4YXusejJwQVP9fEVppM9g3dC6d0rkKl2PqnkNSHdgu93Uwgz4KfBfHGH7eCndDxrKS9OlnVDwfCI95Uir8FwHEZXB6g9PtVQhFYXH53FA8Sd2NbzIrstKIyMx+O52d1+/ssIjxz7wZ7fLsImanhTpNQ68XrEPl/pCA8uiHhL+7kJyQfqqfwtgLhQA2G/exCma4wKf1XpLahBgKlmj/VKQsvOVcQ8sLWFXbU73oEJkQ16o8totLRF0zvUsyjDX7jmNBmE64tiyUCoG815DfKr7htJBz7P2LpvK0sQ2PRSBhRRXYHD/tLyD9g5MnLgYe9jsT9B2PqsDK7arXye4Jo3SciaKL4k6T0MTxdQZRZQ4niSC3rrIXy2hmkF53iDoSKODQwojQk56R0+TEnwb1Vg0j3m1ySaoWO1vOfIxKjFWqfEMewRjHG8RuvA7raYqkqhY1SBK/uBH8=</Data></PidData>";
    }
}



public class BALANCEENQUIRY1 : CommonAEPSProperty1
{

    public string GetJson()
    {
        BALANCEENQUIRY1 ObjData = new BALANCEENQUIRY1();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = this.timestamp;
        ObjData.transactiontype = this.transactiontype;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = this.requestremarks;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class AADHARPAY1
{

    public string timestamp { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public string nationalbankidentification { get; set; }
    public string requestremarks { get; set; }
    public string mobilenumber { get; set; }
    public string transcationType { get; set; }
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
        AADHARPAY1 ObjData = new AADHARPAY1();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
        ObjData.transcationType = "M";
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
        ObjData.pipe = "bank2";
        ObjData.is_iris = "No";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}

public class CashWithdraw1 : CommonAEPSProperty1
{

    public int amount { get; set; }
    public string MerAuthTxnId { get; set; }

    public string GetJson()
    {
        CashWithdraw1 ObjData = new CashWithdraw1();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = this.timestamp;
        ObjData.transactiontype = this.transactiontype;
        ObjData.MerAuthTxnId = this.MerAuthTxnId;
        ObjData.latitude = this.latitude;
        ObjData.longitude = this.longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = this.requestremarks;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = this.ipaddress;
        ObjData.amount = this.amount;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}



public class MiniStatement1
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
    public string pipe { get; set; }
    public string is_iris { get; set; }

    public string GetJson()
    {
        MiniStatement1 ObjData = new MiniStatement1();

        Location objLocation = new Location();
        objLocation = objLocation.GetLocationData();
        ObjData.timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        ObjData.latitude = objLocation.Latitude;
        ObjData.longitude = objLocation.Longitude;
        ObjData.nationalbankidentification = this.nationalbankidentification;
        ObjData.requestremarks = this.requestremarks;
        ObjData.mobilenumber = this.mobilenumber;
        ObjData.transcationtype = "MS";
        ObjData.adhaarnumber = this.adhaarnumber;
        ObjData.data = this.data;
        ObjData.referenceno = this.referenceno;
        ObjData.accessmodetype = this.accessmodetype;
        ObjData.submerchantid = this.submerchantid;
        ObjData.ipaddress = objLocation.IPAddress;
        ObjData.pipe = "bank2";
        ObjData.is_iris = "No";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}






public class CheckSTatusRNF
{


    public string reference { get; set; }

    public string GetJson()
    {
        CheckSTatusRNF ObjData = new CheckSTatusRNF();


        ObjData.reference = this.reference;

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}