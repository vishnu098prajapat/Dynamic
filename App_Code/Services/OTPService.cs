using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for OTPService
/// </summary>
public class OTPService
{
    private static readonly int MaxAttempts = 3;
    cls_connection cls;
    public OTPService()
    {
        cls = new cls_connection();
    }
    // Generate a 6-digit OTP
    public string GenerateOTPCode()
    {
        Random random = new Random();
        return random.Next(1000, 9999).ToString(); // 4-digit OTP
    }

    // Hash OTP using SHA-256
    public string HashOTP(string otp)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hashBytes); // Store as Base64 encoded string
        }
    }


    // Generate and save OTP in database
    public OTPToken GenerateOTP(string userId)
    {
        string otp = GenerateOTPCode();
        string token = System.Guid.NewGuid().ToString();
        string hashedOTP = HashOTP(otp);
        DateTime expiryTime = DateTime.Now.AddMinutes(5); // OTP expires in 5 minutes
        Int32 val = cls.update_data("ADDEDITOTP_Validation '" + userId + "','" + token + "','" + hashedOTP + "',0,0");
        return new OTPToken
        {
            UserID = userId,
            OTP = otp, // Keep plain OTP for email sending (not stored in DB)
            ExpiryTime = expiryTime,
            Token = token,
            Attempts = 0
        };
    }

    public OTPToken GenerateOTPTpin(string userId, string Tpin)
    {
        string otp = Tpin;
        string token = System.Guid.NewGuid().ToString();
        string hashedOTP = HashOTP(otp);
        DateTime expiryTime = DateTime.Now.AddMinutes(5); // OTP expires in 5 minutes
        Int32 val = cls.update_data("ADDEDITOTP_Validation '" + userId + "','" + token + "','" + hashedOTP + "',0,0");
        return new OTPToken
        {
            UserID = userId,
            OTP = otp, // Keep plain OTP for email sending (not stored in DB)
            ExpiryTime = expiryTime,
            Token = token,
            Attempts = 0
        };
    }


    // Get the stored OTP record from the database
    private OTPToken GetOTPToken(string token)
    {
        DataTable dthis = cls.select_data_dt("select * from OTP_Validation where token='"+ token + "' and IsUsed=0");
        if (dthis.Rows.Count > 0)
        {
            return new OTPToken
            {
                Token = dthis.Rows[0]["token"].ToString(),
                OTP = dthis.Rows[0]["OTP"].ToString(),
                ExpiryTime = Convert.ToDateTime(dthis.Rows[0]["EXPIRATIONTIME"].ToString()),
                IsUsed = dthis.Rows[0]["IsUsed"].ToString().ToBoolen(),
                Attempts = dthis.Rows[0]["Attempts"].ToString().ToInt32()
            };
        }

        return null;
    }


    public bool ValidateOTP(string token, string enteredOTP)
    {
        OTPToken storedOTP = GetOTPToken(token);

        if (storedOTP == null || storedOTP.IsUsed || storedOTP.ExpiryTime < DateTime.Now)
        {
            return false;
        }

        // Hash the entered OTP and compare it with the stored hash
        string hashedEnteredOTP = HashOTP(enteredOTP);
        if (hashedEnteredOTP == storedOTP.OTP) // Compare hashed OTPs
        {
            MarkOTPAsUsed(storedOTP.Token);
            return true;
        }

        return false;
    }

    // Mark OTP as used
    private void MarkOTPAsUsed(string otpTokenId)
    {
        Int32 val = cls.update_data("update  OTP_VALIDATION set IsUsed=1,UpdatedAt=GetDate() where token='" + otpTokenId + "' and IsUsed=0");
    }


    // Check if user is blocked after multiple failed attempts
    public bool IsBlocked(string token)
    {
        OTPToken storedOTP = GetOTPToken(token);
        return storedOTP != null && storedOTP.Attempts >= MaxAttempts;
    }


    // Increment the failed attempts counter
    public void IncrementAttempts(string token)
    {
        OTPToken storedOTP = GetOTPToken(token);
        if (storedOTP != null)
        {
            Int32 val = cls.update_data("update  OTP_VALIDATION set Attempts = Attempts + 1 where token='" + token + "' and IsUsed=0");
        }
    }

}