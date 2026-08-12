using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ITokenService
/// </summary>
public interface ITokenService
{
    Task<string> GenerateToken(string userId, string username, string role, string ID);
    Task<string> GenerateTokenMobile(ValidateReturn user, string deviceId, string appId, string platform);
    Task<string> ReadToken(string Token);
    ClaimsPrincipal ReadTokenNew(string Token);
    string GetUserID(ClaimsPrincipal claimsPrincipal);

}