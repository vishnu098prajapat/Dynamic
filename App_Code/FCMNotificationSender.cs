using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;

public class FirebaseSender
{
    private const string ServiceAccountFile = "dhanpay.json";
    private const string FcmEndpoint = "https://fcm.googleapis.com/v1/projects/dhan-pay-deca1/messages:send";

    public async Task SendNotificationAsync(string Token, string title, string body, string route, string image)
    {
        var token = await GetAccessTokenAsync();

        var payload = new
        {
            message = new
            {
                token = Token,
                notification = new
                {
                    title = title,
                    body = body,
                    image = image
                },
                data = new
                {
                    title = title,
                    body = body,
                    msg = body,
                    route = route,
                    image = image
                }
            }
        };

        var client = new RestClient("https://fcm.googleapis.com");
        var request = new RestRequest("v1/projects/dhan-pay-deca1/messages:send", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(payload);

        var response = await client.ExecuteAsync(request);
        Console.WriteLine("Response: " + response.Content);
    }


    public async Task SendNotificationAsyncAll(string Token, string title, string body, string route, string image)
    {
        var token = await GetAccessTokenAsync();

        var payload = new
        {
            message = new
            {
                topic = "all",
                notification = new
                {
                    title = title,
                    body = body,
                    image = "https://cdn.pixabay.com/photo/2015/10/29/14/38/web-1012467_1280.jpg"
                },
                data = new
                {
                    title = title,
                    body = body,
                    msg = body,
                    route = route,
                    image = image
                }
            }
        };

        var client = new RestClient("https://fcm.googleapis.com");
        var request = new RestRequest("v1/projects/dhan-pay-deca1/messages:send", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(payload);

        var response = await client.ExecuteAsync(request);
        Console.WriteLine("Response: " + response.Content);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServiceAccountFile));
        dynamic creds = JsonConvert.DeserializeObject(json);

        string privateKey = creds["private_key"];
        string clientEmail = creds["client_email"];

        var now = DateTime.UtcNow;
        var header = new JwtHeader(new SigningCredentials(
            new RsaSecurityKey(CreateRsa(privateKey)),
            SecurityAlgorithms.RsaSha256));

        var payload = new JwtPayload
        {
            { "iss", clientEmail },
            { "scope", "https://www.googleapis.com/auth/firebase.messaging" },
            { "aud", "https://oauth2.googleapis.com/token" },
            { "iat", new DateTimeOffset(now).ToUnixTimeSeconds() },
            { "exp", new DateTimeOffset(now.AddMinutes(60)).ToUnixTimeSeconds() }
        };

        var jwt = new JwtSecurityToken(header, payload);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtString = tokenHandler.WriteToken(jwt);

        var client = new RestClient("https://oauth2.googleapis.com/token");
        var request = new RestRequest("https://oauth2.googleapis.com/token", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer");
        request.AddParameter("assertion", jwtString);

        var response = await client.ExecuteAsync(request);
        var result = JsonConvert.DeserializeObject<dynamic>(response.Content);
        return result["access_token"];
    }

    private RSA CreateRsa(string privateKey)
    {
        var keyText = privateKey
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "");

        var keyBytes = Convert.FromBase64String(keyText);
        var keyParameter = PrivateKeyFactory.CreateKey(keyBytes);
        var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyParameter);

        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParams);
        return rsa;
    }
}
