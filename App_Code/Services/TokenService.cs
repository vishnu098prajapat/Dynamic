using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
/// <summary>
/// Summary description for TokenService
/// </summary>
public class TokenService : ITokenService
{
    private readonly string _secretKey;
    public static readonly string Keys = "43654354353454353453453454";
    public static readonly string sKeys = "K2d79J2l8pWm6b3Vh12N51qJx4tA7HjL9dA3pO==";
    private readonly int _expiryInMinutes;

    public TokenService(string secretKey, int expiryInMinutes)
    {
        _secretKey = sKeys;
        _expiryInMinutes = expiryInMinutes;
    }

    public async Task<String> GenerateToken(string userId, string username, string role, string ID)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.NameId, ID),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the token
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64) // Issued at
        };

        var token = new JwtSecurityToken(
            issuer: "YourIssuer",
            audience: "YourAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryInMinutes),
            signingCredentials: credentials
        );

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task<String> GenerateTokenMobile(ValidateReturn user, string deviceId, string appId, string platform)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.LoginID.ToString()),
        new Claim("UserID", user.LoginID.ToString()),
        new Claim("device_id", deviceId),
        new Claim("app_id", appId),
        new Claim("platform", platform),
        new Claim("session_id", Guid.NewGuid().ToString("N")),
        new Claim("nonce", Guid.NewGuid().ToString("N")),
        new Claim("login_time", DateTime.UtcNow.ToString("o")),
        new Claim(JwtRegisteredClaimNames.Iat,
            new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64)
    };

        var token = new JwtSecurityToken(
            issuer: ConstantsData.CompanyName,
            audience: "zxpay",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryInMinutes),
            signingCredentials: creds
        );

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task<String> ReadToken(string Token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtTokenObj = tokenHandler.ReadJwtToken(Token);
        var expiration = jwtTokenObj.ValidTo;
        if (expiration < DateTime.UtcNow)
        {
            return await Task.FromResult(ErrorShow.ErrorMessageAPI("Token expired"));
        }
        else
        {
            // Extract claims
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(jwtTokenObj.Claims));

            // Get a specific claim value (e.g., "name")
            var userID = claimsPrincipal.FindFirst("sub")?.Value;
            var username = claimsPrincipal.FindFirst("unique_name")?.Value;
            var ID = claimsPrincipal.FindFirst("nameid")?.Value;

            object Res = new
            {
                Mobile = userID,
                Name = username,
                Msrno = ID,
            };
            string jsonString = JsonSerializer.Serialize(Res);


            return await Task.FromResult(ErrorShow.SuccessArrayAPI(jsonString));
        }
    }

    public ClaimsPrincipal ReadTokenNew(string token)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
           
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = ConstantsData.CompanyName,
                ValidateAudience = true,
                ValidAudience = "zxpay",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero   // remove clock skew
            };
            SecurityToken validatedToken;
            return tokenHandler.ValidateToken(token, validationParams, out validatedToken);
        }
        catch(Exception ex)
        {
            return null;
        }
    }

    public string GetUserID(ClaimsPrincipal principal)
    {
        return principal?.FindFirst("UserID")?.Value;
    }
    
}