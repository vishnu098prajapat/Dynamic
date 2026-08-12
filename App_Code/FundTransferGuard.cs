using System;
using System.Collections.Generic;
using System.Configuration;

/// <summary>
/// A fund credit that has been prepared and is waiting for the admin to prove
/// they authorised it.
///
/// The whole point of this object is that the AMOUNT and the RECIPIENT are
/// fixed at the moment the OTP is issued and are read back from here — never
/// from the input boxes — when the transfer is finally executed. The old page
/// kept only the OTP digits in Session["OTPAddFund"], read the amount from the
/// textbox at submit time, and never cleared the session value, so a single
/// verified OTP could be reused to push any amount to any member for as long
/// as the session lived.
/// </summary>
[Serializable]
public class PendingFundAdd
{
    /// <summary>member.msrno of the recipient.</summary>
    public long Msrno { get; set; }

    public string LoginId { get; set; }
    public string MemberName { get; set; }

    /// <summary>The amount the OTP authorises. Nothing else may be credited.</summary>
    public decimal Amount { get; set; }

    public string Remark { get; set; }

    /// <summary>"OTP" (code sent by SMS) or "TPIN" (admin's transaction PIN).</summary>
    public string Mode { get; set; }

    public string OtpSalt { get; set; }
    public string OtpHash { get; set; }

    /// <summary>
    /// One-time reference. It is written into the ledger as the transaction
    /// GUID and recorded as consumed the moment the credit succeeds, so a
    /// replayed postback cannot post the same credit twice.
    /// </summary>
    public string Token { get; set; }

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
            return s < 0 ? 0 : (int)Math.Round(s);
        }
    }
}

/// <summary>
/// Amount policy and replay protection for the admin fund-credit pages.
/// The OTP primitives themselves live in CredentialResetGuard and are shared.
/// </summary>
public static class FundTransferGuard
{
    // ---- tunables ---------------------------------------------------------

    public static decimal MinAmount
    {
        get { return ReadDecimal("AddFund_MinAmount", 1m, 1m, 1000000m); }
    }

    /// <summary>
    /// Ceiling for a single credit. There was no limit at all before, so one
    /// extra zero in the amount box credited ten times what was intended and
    /// the only way back was a manual reversal.
    /// </summary>
    public static decimal MaxAmount
    {
        get { return ReadDecimal("AddFund_MaxAmount", 200000m, 1m, 100000000m); }
    }

    public static int ValidityMinutes
    {
        get { return ReadInt("AddFund_OTP_ValidityMinutes", 5, 1, 30); }
    }

    public static int MaxAttempts
    {
        get { return ReadInt("AddFund_OTP_MaxAttempts", 5, 1, 10); }
    }

    public static int ResendCooldownSeconds
    {
        get { return ReadInt("AddFund_OTP_ResendSeconds", 60, 0, 600); }
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
        catch { }
        return fallback;
    }

    private static decimal ReadDecimal(string key, decimal fallback, decimal min, decimal max)
    {
        try
        {
            string raw = ConfigurationManager.AppSettings[key];
            decimal v;
            if (!string.IsNullOrEmpty(raw) && decimal.TryParse(raw.Trim(), out v))
            {
                if (v < min) { return min; }
                if (v > max) { return max; }
                return v;
            }
        }
        catch { }
        return fallback;
    }

    // ---- validation -------------------------------------------------------

    /// <summary>
    /// Parses and range-checks the amount. Returns null on success and puts the
    /// parsed value in <paramref name="amount"/>.
    ///
    /// The old page relied on TextMode="Number" plus an onkeypress filter, then
    /// called Convert.ToDecimal on whatever arrived. Both of those are browser
    /// side only: a hand-made POST could carry any string, and a bad one threw
    /// a FormatException that the catch-all turned into "Something went wrong".
    /// </summary>
    public static string ValidateAmount(string raw, out decimal amount)
    {
        amount = 0;

        if (raw == null || raw.Trim().Length == 0)
        {
            return "Please enter an amount.";
        }

        string cleaned = raw.Trim().Replace(",", "");

        if (!decimal.TryParse(cleaned, out amount))
        {
            return "The amount must be a number.";
        }

        if (amount <= 0)
        {
            return "The amount must be greater than zero.";
        }

        if (decimal.Round(amount, 2) != amount)
        {
            return "The amount cannot have more than two decimal places.";
        }

        if (amount < MinAmount)
        {
            return "The minimum amount is " + MinAmount.ToString("N2") + ".";
        }

        if (amount > MaxAmount)
        {
            return "The maximum amount for a single credit is " + MaxAmount.ToString("N2") +
                   ". Raise AddFund_MaxAmount in Web.config if a larger credit is genuinely needed.";
        }

        return null;
    }

    /// <summary>
    /// Keeps the remark to something that can safely be written into a ledger
    /// narration and read back later.
    /// </summary>
    public static string ValidateRemark(string remark)
    {
        if (remark == null) { remark = ""; }
        remark = remark.Trim();

        if (remark.Length == 0)
        {
            return "Please enter a remark so this credit can be traced later.";
        }

        if (remark.Length > 120)
        {
            return "The remark cannot be longer than 120 characters.";
        }

        foreach (char c in remark)
        {
            if (char.IsControl(c))
            {
                return "The remark contains characters that are not allowed.";
            }
        }

        return null;
    }

    // ---- replay protection ------------------------------------------------

    /// <summary>
    /// Tokens already turned into a ledger entry in this session.
    ///
    /// A double click, an impatient refresh, or a replayed postback would
    /// otherwise credit the member twice — the original had nothing at all
    /// standing between the Submit button and a second identical credit.
    /// </summary>
    public static bool IsConsumed(IDictionary<string, bool> consumed, string token)
    {
        if (consumed == null || string.IsNullOrEmpty(token)) { return false; }
        return consumed.ContainsKey(token);
    }

    public static void MarkConsumed(IDictionary<string, bool> consumed, string token)
    {
        if (consumed == null || string.IsNullOrEmpty(token)) { return; }
        consumed[token] = true;
    }

    /// <summary>
    /// Rupees in words, Indian numbering (lakh / crore) — shown next to the
    /// amount box so a mistyped extra zero is obvious before the OTP is sent
    /// rather than after the money has moved.
    /// </summary>
    public static string InWords(decimal amount)
    {
        if (amount <= 0) { return ""; }

        long rupees = (long)decimal.Truncate(amount);
        int paise = (int)((amount - rupees) * 100);

        string words = CroreScale(rupees);

        if (words.Length == 0) { words = "Zero"; }

        string result = words.Trim() + " Rupees";
        if (paise > 0)
        {
            result += " and " + TwoDigits(paise) + " Paise";
        }
        return result + " Only";
    }

    private static string CroreScale(long n)
    {
        string s = "";

        if (n >= 10000000)
        {
            s += CroreScale(n / 10000000) + " Crore ";
            n %= 10000000;
        }
        if (n >= 100000)
        {
            s += TwoDigits((int)(n / 100000)) + " Lakh ";
            n %= 100000;
        }
        if (n >= 1000)
        {
            s += TwoDigits((int)(n / 1000)) + " Thousand ";
            n %= 1000;
        }
        if (n >= 100)
        {
            s += TwoDigits((int)(n / 100)) + " Hundred ";
            n %= 100;
        }
        if (n > 0)
        {
            s += TwoDigits((int)n);
        }

        return s;
    }

    private static readonly string[] Ones = {
        "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
        "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
        "Seventeen", "Eighteen", "Nineteen"
    };

    private static readonly string[] Tens = {
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    };

    private static string TwoDigits(int n)
    {
        if (n <= 0) { return ""; }
        if (n < 20) { return Ones[n]; }

        string s = Tens[n / 10];
        if (n % 10 > 0) { s += " " + Ones[n % 10]; }
        return s;
    }
}
