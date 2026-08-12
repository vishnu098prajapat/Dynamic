using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JWTBankSendRequestNewIntegration
/// </summary>
public class JWTBankSendRequestNewIntegration
{
    public double timestamp { get; set; }
    public string partnerId { get; set; }
    public string reqid { get; set; }
    public JWTBankSendRequestNewIntegration()
    {
        //
         timestamp =Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        partnerId = PaysPrint.PARTNERID;
        reqid = DateTime.Now.ToString("yyyyMMddHHmmssffff");
    }

    public void SetLiveParameter()
    {
   
       
    }

    public string GetToken()
    {
        JWTBankSendRequestNewIntegration jWTBankSendRequest = new JWTBankSendRequestNewIntegration();
        string Token = JsonWebToken.Encode(jWTBankSendRequest, PaysPrint.JWTencryptionKey, JwtHashAlgorithm.HS256);
        return Token;
    }

    public string GetTokenVerification()
    {
        JWTBankSendRequestNewIntegration jWTBankSendRequest = new JWTBankSendRequestNewIntegration();
        jWTBankSendRequest.partnerId = verifya2z.PARTNERID;
        timestamp = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        reqid = DateTime.Now.ToString("yyyyMMddHHmmssffff");

        string Token = JsonWebToken.Encode(jWTBankSendRequest, verifya2z.JWTencryptionKey, JwtHashAlgorithm.HS256);
        return Token;
    }

    public string GetLiveToken()
    {
        JWTBankSendRequestNewIntegration jWTBankSendRequest = new JWTBankSendRequestNewIntegration();
        jWTBankSendRequest.SetLiveParameter();
        string Token = JsonWebToken.Encode(jWTBankSendRequest, PaysPrint.JWTencryptionKey, JwtHashAlgorithm.HS256);
        return Token;
    }
}