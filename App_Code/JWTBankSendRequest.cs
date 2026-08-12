using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JWTBankSendRequest
/// </summary>
public class JWTBankSendRequest
{
    public long timestamp { get; set; }
    public string partnerId { get; set; }
    public string reqid { get; set; }
    public  JWTBankSendRequest()
    {
        timestamp =Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff")) ;
        partnerId = AEPSBankCredentials.PARTNERID;
        reqid = System.Guid.NewGuid().ToString().Replace("-","0");        
    }

    public void SetLiveParameter()
    {

        timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        partnerId = AEPSBankCredentials.PARTNERIDLive;
        reqid = System.Guid.NewGuid().ToString().Replace("-", "0");
    }

    public string GetToken()
    {
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        string Token = JsonWebToken.Encode(jWTBankSendRequest, AEPSBankCredentials.JWTencryptionKey, JwtHashAlgorithm.HS256);
        return Token;
    }
    public string GetLiveToken()
    {
        JWTBankSendRequest jWTBankSendRequest = new JWTBankSendRequest();
        jWTBankSendRequest.SetLiveParameter();
        string Token = JsonWebToken.Encode(jWTBankSendRequest, AEPSBankCredentials.JWTencryptionKeyLive, JwtHashAlgorithm.HS256);
        return Token;
    }
}