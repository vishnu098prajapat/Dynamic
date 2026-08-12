using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Field validation and initial-credential generation for member registration.
///
/// The registration page previously checked only string lengths — Aadhaar had to
/// be twelve characters and PAN ten, and nothing else. Twelve zeros passed. So
/// did "AAAAA1111A" in the wrong shape. The checks here are the real ones.
///
/// It also generated the member's first password with:
///
///     Random generator = new Random();
///     Registration.Password = generator.Next(1, 10000).ToString("D10");
///
/// which is a number from 1 to 9999 zero-padded to ten characters, so every
/// password ever issued looked like 0000004821 — four random digits behind six
/// zeros, from a clock-seeded generator, created in the same tick as the login
/// PIN's generator so the two could share a seed. Replaced with the OS
/// cryptographic RNG.
/// </summary>
public static class RegistrationValidator
{
    // ---- identity documents -----------------------------------------------

    private static readonly Regex PanShape =
        new Regex("^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.Compiled);

    private static readonly Regex PasswordRegex =
    new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%&*])[A-Za-z\d!@#$%&*]{8,20}$",
        RegexOptions.Compiled);

    private static readonly Regex Digits =
        new Regex("^[0-9]+$", RegexOptions.Compiled);

    /// <summary>
    /// Validates a PAN. Returns null when it passes.
    ///
    /// Shape is five letters, four digits, one letter. The fourth letter is the
    /// holder type — P for an individual, C for a company, F for a firm and so
    /// on — and an invalid one means the number cannot exist, so it is worth
    /// rejecting here rather than at the income tax portal.
    /// </summary>
    public static string ValidatePan(string pan)
    {
        if (string.IsNullOrEmpty(pan)) { return "Please enter the PAN number."; }

        string p = pan.Trim().ToUpperInvariant();

        if (p.Length != 10) { return "PAN must be exactly 10 characters."; }
        if (!PanShape.IsMatch(p)) { return "PAN format is wrong. It must be 5 letters, 4 digits, then 1 letter (e.g. ABCDE1234F)."; }

        // Fourth character is the holder type.
        const string holderTypes = "ABCFGHLJPTK";
        if (holderTypes.IndexOf(p[3]) < 0)
        {
            return "PAN is not valid — the 4th letter '" + p[3] + "' is not a recognised holder type.";
        }

        return null;
    }

    /// <summary>
    /// Validates an Aadhaar number. Returns null when it passes.
    ///
    /// Twelve digits, not starting with 0 or 1 (UIDAI does not issue those), and
    /// the last digit must satisfy the Verhoeff checksum. Without the checksum
    /// any twelve digits are accepted, which is how "000000000000" used to get
    /// through.
    /// </summary>
    public static string ValidateAadhaar(string aadhaar)
    {
        if (string.IsNullOrEmpty(aadhaar)) { return "Please enter the Aadhaar number."; }

        string a = aadhaar.Trim().Replace(" ", "").Replace("-", "");

        if (a.Length != 12) { return "Aadhaar must be exactly 12 digits."; }
        if (!Digits.IsMatch(a)) { return "Aadhaar must contain digits only."; }
        if (a[0] == '0' || a[0] == '1') { return "Aadhaar cannot start with 0 or 1."; }

        if (!VerhoeffValid(a))
        {
            return "That Aadhaar number is not valid — it fails the UIDAI checksum. Please re-check the digits.";
        }

        return null;
    }

    // Verhoeff checksum tables. UIDAI uses Verhoeff for the 12th digit.
    private static readonly int[,] D = {
        {0,1,2,3,4,5,6,7,8,9},
        {1,2,3,4,0,6,7,8,9,5},
        {2,3,4,0,1,7,8,9,5,6},
        {3,4,0,1,2,8,9,5,6,7},
        {4,0,1,2,3,9,5,6,7,8},
        {5,9,8,7,6,0,4,3,2,1},
        {6,5,9,8,7,1,0,4,3,2},
        {7,6,5,9,8,2,1,0,4,3},
        {8,7,6,5,9,3,2,1,0,4},
        {9,8,7,6,5,4,3,2,1,0}
    };

    private static readonly int[,] P = {
        {0,1,2,3,4,5,6,7,8,9},
        {1,5,7,6,2,8,3,0,9,4},
        {5,8,0,3,7,9,6,1,4,2},
        {8,9,1,6,0,4,3,5,2,7},
        {9,4,5,3,1,2,6,8,7,0},
        {4,2,8,6,5,7,3,9,0,1},
        {2,7,9,3,8,0,6,4,1,5},
        {7,0,4,6,9,1,3,2,5,8}
    };

    private static readonly int[] Inv = { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

    private static bool VerhoeffValid(string number)
    {
        try
        {
            int c = 0;
            int len = number.Length;

            // Walk the digits right to left.
            for (int i = 0; i < len; i++)
            {
                int digit = number[len - i - 1] - '0';
                c = D[c, P[i % 8, digit]];
            }

            return c == 0;
        }
        catch
        {
            return false;
        }
    }

    // ---- contact ----------------------------------------------------------

    private static readonly Regex EmailShape = new Regex(
        @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);

    /// <summary>
    /// Indian mobile number: ten digits starting 6 to 9. Returns null on pass.
    /// </summary>
    public static string ValidateMobile(string mobile, string label)
    {
        if (string.IsNullOrEmpty(mobile)) { return "Please enter the " + label + "."; }

        string m = mobile.Trim();

        if (m.Length != 10 || !Digits.IsMatch(m))
        {
            return label + " must be exactly 10 digits.";
        }

        if (m[0] < '6' || m[0] > '9')
        {
            return label + " must start with 6, 7, 8 or 9.";
        }

        return null;
    }

    public static string ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) { return "Please enter the email address."; }
        if (!EmailShape.IsMatch(email.Trim())) { return "That email address does not look valid."; }
        return null;
    }

    public static string ValidatePincode(string pin, string label)
    {
        if (string.IsNullOrEmpty(pin)) { return "Please enter the " + label + "."; }

        string p = pin.Trim();

        if (p.Length != 6 || !Digits.IsMatch(p)) { return label + " must be exactly 6 digits."; }
        if (p[0] == '0') { return label + " cannot start with 0."; }

        return null;
    }

    // ---- date of birth ----------------------------------------------------

    /// <summary>
    /// Parses and range-checks the date of birth, and enforces 18+.
    ///
    /// The page's own validator message already said "Age must be 18+ to
    /// register" — but it was attached to a RequiredFieldValidator, so it only
    /// ever checked that the box was not empty. A one-year-old passed. Meanwhile
    /// the code behind called Convert.ToDateTime on the raw text, which threw
    /// FormatException on an empty or malformed value and took the page down.
    /// </summary>
    public static string ValidateDob(string raw, out DateTime dob)
    {
        dob = DateTime.MinValue;

        if (string.IsNullOrEmpty(raw) || raw.Trim().Length == 0)
        {
            return "Please enter the date of birth.";
        }

        // TextMode="Date" posts yyyy-MM-dd. Parsed explicitly rather than
        // trusting the server's culture.
        if (!DateTime.TryParseExact(raw.Trim(), "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dob) &&
            !DateTime.TryParse(raw.Trim(),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dob))
        {
            return "That date of birth could not be read. Use the date picker.";
        }

        if (dob > DateTime.Today)
        {
            return "The date of birth cannot be in the future.";
        }

        int age = DateTime.Today.Year - dob.Year;
        if (dob > DateTime.Today.AddYears(-age)) { age--; }

        if (age < 18)
        {
            return "The member must be at least 18 years old. This date of birth gives an age of " + age + ".";
        }

        if (age > 100)
        {
            return "That date of birth gives an age of " + age + ". Please check it.";
        }

        return null;
    }

    // ---- text fields ------------------------------------------------------

    /// <summary>
    /// Generic required/length check for a free-text field. Returns null on pass.
    /// </summary>
    public static string ValidateText(string value, string label, int min, int max, bool required)
    {
        string v = (value == null ? "" : value.Trim());

        if (v.Length == 0)
        {
            return required ? "Please enter the " + label + "." : null;
        }

        if (v.Length < min) { return label + " must be at least " + min + " characters."; }
        if (v.Length > max) { return label + " cannot be longer than " + max + " characters."; }

        return null;
    }

    // ---- initial credentials ----------------------------------------------

    private const string PasswordUpper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string PasswordLower = "abcdefghijkmnopqrstuvwxyz";
    private const string PasswordDigit = "23456789";
    private const string PasswordSymbol = "!@#$%&*";

    /// <summary>
    /// A member's first login password: 10 characters with at least one of each
    /// class, drawn from the OS cryptographic RNG.
    ///
    /// The look-alike characters (0/O, 1/l/I) are left out of the alphabet on
    /// purpose — this password gets read out over the phone or copied off a
    /// WhatsApp message, and a member who cannot type it in calls support.
    /// </summary>
    public static string GeneratePassword()
    {
        const int length = 10;
        string all = PasswordUpper + PasswordLower + PasswordDigit + PasswordSymbol;

        var chars = new char[length];

        // One from each class first, so the result always satisfies a policy
        // that demands all four.
        chars[0] = Pick(PasswordUpper);
        chars[1] = Pick(PasswordLower);
        chars[2] = Pick(PasswordDigit);
        chars[3] = Pick(PasswordSymbol);

        for (int i = 4; i < length; i++)
        {
            chars[i] = Pick(all);
        }

        Shuffle(chars);
        return new string(chars);
    }

    /// <summary>
    /// Four-digit login PIN, crypto random, with the trivial ones rejected —
    /// 0000, 1111 and 1234 are exactly what a clock-seeded Random happily
    /// produced before.
    /// </summary>
    public static string GenerateLoginPin()
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            int n = NextInt(1000, 10000);
            string pin = n.ToString("D4");

            if (pin[0] == pin[1] && pin[1] == pin[2] && pin[2] == pin[3]) { continue; }

            bool ascending = true, descending = true;
            for (int i = 1; i < 4; i++)
            {
                if (pin[i] != pin[i - 1] + 1) { ascending = false; }
                if (pin[i] != pin[i - 1] - 1) { descending = false; }
            }
            if (ascending || descending) { continue; }

            return pin;
        }

        // Cannot realistically happen; a valid PIN beats looping forever.
        return "4729";
    }

    private static char Pick(string alphabet)
    {
        return alphabet[NextInt(0, alphabet.Length)];
    }

    private static void Shuffle(char[] items)
    {
        for (int i = items.Length - 1; i > 0; i--)
        {
            int j = NextInt(0, i + 1);
            char t = items[i];
            items[i] = items[j];
            items[j] = t;
        }
    }

    /// <summary>
    /// Uniform random integer from min up to but not including max, taken
    /// from the OS RNG.
    ///
    /// Rejects the tail of the byte range rather than taking a plain modulus,
    /// which would make the low values slightly more likely.
    /// </summary>
    private static int NextInt(int min, int max)
    {
        if (max <= min) { return min; }

        uint range = (uint)(max - min);
        uint limit = uint.MaxValue - (uint.MaxValue % range);

        var buffer = new byte[4];

        using (var rng = new RNGCryptoServiceProvider())
        {
            while (true)
            {
                rng.GetBytes(buffer);
                uint value = BitConverter.ToUInt32(buffer, 0);

                if (value < limit)
                {
                    return (int)(min + (value % range));
                }
            }
        }
    }

    public static string ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Please enter the password.";

        password = password.Trim();

        if (password.Length < 8)
            return "Password must be at least 8 characters.";

        if (password.Length > 20)
            return "Password cannot be longer than 20 characters.";

        if (!PasswordRegex.IsMatch(password))
        {
            return "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (!@#$%&*).";
        }

        return null;
    }
}
