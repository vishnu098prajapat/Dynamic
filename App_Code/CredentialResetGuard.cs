using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Shared guard for the two admin pages that overwrite a member's login
/// password (SetPassWord.aspx) and transaction PIN (SetPin.aspx).
///
/// These are the highest-privilege actions in the panel — they hand an admin
/// full control of a member's money — so the flow is deliberately strict:
///
///  * the OTP is generated with a cryptographic RNG, not System.Random, which
///    is seeded from the clock and is trivially predictable;
///  * only a salted SHA-256 of the OTP is kept, compared in constant time;
///  * the OTP is BOUND to the member and to the exact secret it authorised.
///    The old code kept the OTP alone in Session["OTPEdit"], so an admin could
///    request an OTP for member A, switch the dropdown to member B, and redeem
///    it against B. The OTP authorised nothing in particular;
///  * OTPs expire, verification attempts are capped, and re-sends are rate
///    limited so the SMS gateway cannot be used as a flood tool.
///
/// The pending secret itself lives only in server-side session state (the app
/// runs sessionState mode="InProc", so it never leaves this process) and is
/// wiped as soon as the reset completes, fails or is abandoned.
/// </summary>
[Serializable]
public class PendingCredentialReset
{
    /// <summary>VIEW_MEMBERDETAILS.ID of the member this OTP authorises.</summary>
    public int MemberId { get; set; }

    /// <summary>Member primary key actually passed to PROC_MANAGE_PASSWROD.</summary>
    public long Msrno { get; set; }

    /// <summary>Login ID, kept for the audit trail and the on-screen summary.</summary>
    public string LoginId { get; set; }

    /// <summary>Member name, for the confirmation banner.</summary>
    public string MemberName { get; set; }

    /// <summary>
    /// The new password / PIN, held server-side. It is captured at request
    /// time so the input boxes can be real password fields — those do not
    /// round-trip their value through the browser, which is why the original
    /// page had to leave the new password sitting in a plain-text box that was
    /// re-rendered into the HTML on every postback.
    /// </summary>
    public string NewSecret { get; set; }

    /// <summary>Salt for the OTP hash.</summary>
    public string OtpSalt { get; set; }

    /// <summary>Salted SHA-256 of the OTP. The OTP itself is never stored.</summary>
    public string OtpHash { get; set; }

    public DateTime IssuedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public int Attempts { get; set; }

    public bool IsExpired
    {
        get { return DateTime.UtcNow > ExpiresUtc; }
    }

    public int SecondsRemaining
    {
        get
        {
            double s = (ExpiresUtc - DateTime.UtcNow).TotalSeconds;
            if (s < 0) { return 0; }
            return (int)Math.Round(s);
        }
    }
}

/// <summary>
/// Policy checks, OTP generation and SQL-literal escaping for the credential
/// reset pages.
/// </summary>
public static class CredentialResetGuard
{
    // ---- tunables ---------------------------------------------------------

    /// <summary>How long an OTP stays valid. Default 5 minutes.</summary>
    public static int ValidityMinutes
    {
        get { return ReadInt("Reset_OTP_ValidityMinutes", 5, 1, 30); }
    }

    /// <summary>Wrong-OTP attempts allowed before the request is destroyed.</summary>
    public static int MaxAttempts
    {
        get { return ReadInt("Reset_OTP_MaxAttempts", 5, 1, 10); }
    }

    /// <summary>Minimum gap between two OTP requests, in seconds.</summary>
    public static int ResendCooldownSeconds
    {
        get { return ReadInt("Reset_OTP_ResendSeconds", 60, 0, 600); }
    }

    private static int ReadInt(string key, int fallback, int min, int max)
    {
        try
        {
            string raw = ConfigurationManager.AppSettings[key];
            int v;
            if (!string.IsNullOrEmpty(raw) && int.TryParse(raw.Trim(), out v))
            {
                if (v < min) { return min; }
                if (v > max) { return max; }
                return v;
            }
        }
        catch
        {
            // A bad config value must not take the page down; fall through.
        }
        return fallback;
    }

    // ---- OTP --------------------------------------------------------------

    /// <summary>
    /// Six-digit OTP from the OS cryptographic RNG.
    ///
    /// System.Random is seeded from the system clock: two OTPs generated in the
    /// same tick come out identical, and the sequence is predictable to anyone
    /// who can guess roughly when it was created. That is not acceptable for a
    /// code that authorises taking over a member's account.
    /// </summary>
    public static string GenerateOtp()
    {
        byte[] buffer = new byte[4];

        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(buffer);
        }

        // Strip the sign bit, then fold into 000000-999999.
        uint value = BitConverter.ToUInt32(buffer, 0) & 0x7FFFFFFF;
        return (value % 1000000).ToString("D6");
    }

    /// <summary>16 random bytes, base64, used to salt the OTP hash.</summary>
    public static string GenerateSalt()
    {
        byte[] buffer = new byte[16];

        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(buffer);
        }

        return Convert.ToBase64String(buffer);
    }

    /// <summary>Salted SHA-256 of an OTP, hex encoded.</summary>
    public static string HashOtp(string otp, string salt)
    {
        if (otp == null) { otp = ""; }
        if (salt == null) { salt = ""; }

        using (var sha = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(salt + "|" + otp.Trim());
            byte[] hash = sha.ComputeHash(bytes);

            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Length-independent, content-independent comparison.
    ///
    /// A plain string equality check returns as soon as it hits a differing
    /// character, so the time it takes leaks how much of the value was right.
    /// Over enough requests that is enough to recover a code one digit at a
    /// time. This always walks the full length.
    /// </summary>
    public static bool ConstantTimeEquals(string a, string b)
    {
        if (a == null || b == null) { return false; }
        if (a.Length != b.Length) { return false; }

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
    }

    // ---- SQL --------------------------------------------------------------

    /// <summary>
    /// Escapes a value for use inside a single-quoted SQL literal by doubling
    /// apostrophes, which is what SQL Server expects.
    ///
    /// The rest of this codebase strips apostrophes instead. For a password
    /// field that is the wrong call twice over: it silently changes the secret
    /// the admin typed (so the member cannot log in with what they were told),
    /// and it invites the habit of "cleaning" input rather than escaping it.
    /// Doubling preserves the character and closes the injection.
    /// </summary>
    public static string SqlLiteral(string value)
    {
        if (value == null) { return ""; }
        return value.Replace("'", "''");
    }

    // ---- policy -----------------------------------------------------------

    private static readonly Regex HasUpper = new Regex("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex HasLower = new Regex("[a-z]", RegexOptions.Compiled);
    private static readonly Regex HasDigit = new Regex("[0-9]", RegexOptions.Compiled);
    private static readonly Regex HasSymbol = new Regex("[#?!@$%^&*()\\-_=+\\[\\]{};:,.<>/\\\\|~`'\"]", RegexOptions.Compiled);
    private static readonly Regex OnlyDigits = new Regex("^[0-9]+$", RegexOptions.Compiled);

    /// <summary>
    /// Validates a new login password. Returns null when it passes, otherwise a
    /// message safe to show the admin.
    ///
    /// The value is NOT trimmed here — the original validated the untrimmed
    /// text but saved the trimmed one, so a password ending in a space passed
    /// the strength check and then a different string was stored. Callers must
    /// trim before validating, and save exactly what they validated.
    /// </summary>
    public static string ValidatePassword(string password, string loginId, string mobile)
    {
        if (string.IsNullOrEmpty(password))
        {
            return "Please enter a new password.";
        }

        if (password.Length < 8 || password.Length > 20)
        {
            return "Password must be between 8 and 20 characters.";
        }

        foreach (char c in password)
        {
            // Printable ASCII only. Spaces, tabs and pasted unicode look
            // identical on screen but break login for the member later.
            if (c < 33 || c > 126)
            {
                return "Password cannot contain spaces or special unicode characters.";
            }
        }

        if (!HasUpper.IsMatch(password)) { return "Password must contain at least one capital letter."; }
        if (!HasLower.IsMatch(password)) { return "Password must contain at least one small letter."; }
        if (!HasDigit.IsMatch(password)) { return "Password must contain at least one number."; }
        if (!HasSymbol.IsMatch(password)) { return "Password must contain at least one special character."; }

        if (!string.IsNullOrEmpty(loginId) && loginId.Length >= 3 &&
            password.IndexOf(loginId, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Password must not contain the member login ID.";
        }

        if (!string.IsNullOrEmpty(mobile) && mobile.Length >= 6 &&
            password.IndexOf(mobile, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Password must not contain the member mobile number.";
        }

        if (IsObviousPassword(password))
        {
            return "That password is too common. Please choose a different one.";
        }

        return null;
    }

    private static bool IsObviousPassword(string password)
    {
        string p = password.ToLowerInvariant();

        string[] banned = {
            "password", "passw0rd", "admin", "welcome", "qwerty",
            "12345678", "123456789", "1234567890", "abcd1234", "india"
        };

        for (int i = 0; i < banned.Length; i++)
        {
            if (p.Contains(banned[i])) { return true; }
        }

        return false;
    }

    /// <summary>
    /// Validates a new 4-digit transaction PIN. Returns null when it passes.
    ///
    /// A TPIN releases money, so the trivial ones are rejected outright — the
    /// original only checked that the length was 4, which let 0000 and 1234
    /// straight through. The numeric check is done here rather than left to the
    /// onkeypress handler the old page relied on, because a keypress filter is
    /// bypassed by pasting, by autofill, or by simply posting the form.
    /// </summary>
    public static string ValidatePin(string pin, string mobile)
    {
        if (string.IsNullOrEmpty(pin))
        {
            return "Please enter a new TPIN.";
        }

        if (pin.Length != 4 || !OnlyDigits.IsMatch(pin))
        {
            return "TPIN must be exactly 4 digits.";
        }

        // All four digits the same: 0000, 1111 ...
        if (pin[0] == pin[1] && pin[1] == pin[2] && pin[2] == pin[3])
        {
            return "TPIN cannot be the same digit four times.";
        }

        // Straight runs up or down: 1234, 4321 ...
        bool ascending = true, descending = true;
        for (int i = 1; i < pin.Length; i++)
        {
            if (pin[i] != pin[i - 1] + 1) { ascending = false; }
            if (pin[i] != pin[i - 1] - 1) { descending = false; }
        }
        if (ascending || descending)
        {
            return "TPIN cannot be a running sequence like 1234 or 4321.";
        }

        if (!string.IsNullOrEmpty(mobile) && mobile.Length >= 4 &&
            pin == mobile.Substring(mobile.Length - 4))
        {
            return "TPIN cannot be the last 4 digits of the member mobile number.";
        }

        return null;
    }

    /// <summary>
    /// Masks a mobile number for display, e.g. 98XXXXXX10. The admin needs
    /// enough to confirm the OTP is going to the right handset, not the whole
    /// number rendered into a page that may be screen-shared.
    /// </summary>
    public static string MaskMobile(string mobile)
    {
        if (string.IsNullOrEmpty(mobile)) { return ""; }

        string m = mobile.Trim();
        if (m.Length <= 4) { return m; }

        return m.Substring(0, 2) + new string('X', m.Length - 4) + m.Substring(m.Length - 2);
    }
}
