using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for Login
/// </summary>
public class Login
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "LoginID must be between 5 and 10 characters.")]
    public string LoginID { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 20 characters.")]
    public string Password { get; set; }

    public byte[] HashValue { get; private set; }

    [Required(ErrorMessage = "Latitude is required.")]
    public string Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    public string Longitude { get; set; }
    public string IP { get; set; }
    public string OS { get; set; }
    public string Browser { get; set; }
    public string DeviceID { get; set; }

    public Login()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static string GenerateSHA512String(string inputString)
    {
        SHA512 sha512 = SHA512Managed.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(inputString);
        byte[] hash = sha512.ComputeHash(bytes);

        string hashValueHex = BitConverter.ToString(hash).Replace("-", "");
        return (hashValueHex);
    }

    public Tuple<bool, string> Validate(string LoginID, string Passwors, cls_connection cls)
    {
        DataTable dt = cls.select_data_dt("select HASHBYTES('SHA2_512', '" + Passwors + "'+CAST(Salt AS NVARCHAR(36))) as Comparepass,LoginID,CAST(Salt AS NVARCHAR(36))Salt,Password,Isactive,AccountLock,LoginCount from member where loginid='" + LoginID + "'");
        if (dt.Rows.Count > 0)
        {
            if (!Convert.ToBoolean(dt.Rows[0]["AccountLock"].ToString()))
            {
                if (dt.Rows[0]["LoginCount"].ToString().ToInt32() <= 2)
                {
                    byte[] hashValueComparepassBytes = (byte[])dt.Rows[0]["Comparepass"];
                    string SHash = BitConverter.ToString(hashValueComparepassBytes).Replace("-", "");
                    byte[] hashValueBytes = (byte[])dt.Rows[0]["Password"];
                    string hashValueHex = BitConverter.ToString(hashValueBytes).Replace("-", "");
                    bool hashesMatch = StringComparer.OrdinalIgnoreCase.Compare(hashValueHex, SHash) == 0;
                    if (hashesMatch)
                    {
                        IncrementSuccessAttempts(LoginID, cls);
                        return Tuple.Create(true, "User Valid");
                    }
                    else
                    {
                        IncrementFailedAttempts(LoginID, cls);
                        return Tuple.Create(false, "Incorrect Password");
                    }
                }
                else
                {
                    LockAccount(LoginID, cls);
                    return Tuple.Create(false, "Your account has been locked due to multiple failed login attempts. Please try again later or reset your password.");
                }
            }
            else
            {
                return Tuple.Create(false, "Your account has been locked due to multiple failed login attempts. Please try again later or reset your password.");
            }
        }
        else
        {
            return Tuple.Create(false, "In-Valid Account details");
        }
    }

    private void LockAccount(string username, cls_connection cls)
    {
        cls.update_data("update member set AccountLock=1 where loginid='" + username + "'");
    }
    private void IncrementFailedAttempts(string username, cls_connection cls)
    {
        cls.update_data("update member set LoginCount=LoginCount+1 where loginid='" + username + "'");
    }

    private void IncrementSuccessAttempts(string username, cls_connection cls)
    {
        cls.update_data("update member set LoginCount=0 where loginid='" + username + "'");
    }
}