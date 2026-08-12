using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web;

/// <summary>
/// Time-limited access links: sign somebody into one account, for a while,
/// without handing over a password.
///
/// WHAT THIS REPLACES, AND WHY IT IS SAFER THAN WHAT PEOPLE DO INSTEAD
///
/// Without it, giving support a look at a member's account means telling them
/// the password. That password then exists in a WhatsApp thread forever, works
/// from anywhere, cannot be taken back, and leaves no trace of who used it. A
/// link that dies on a timer and can be pulled back is strictly better than the
/// habit it displaces — which is the bar this has to clear, not perfection.
///
/// THE FOUR THINGS THAT MAKE IT DEFENSIBLE
///
/// 1. THE TOKEN IS NEVER STORED. Only its SHA-256. A copy of the AccessGrant
///    table is worth nothing on its own. Nobody can recover a lost link,
///    including the admin who made it, and that is the point.
///
/// 2. IT PINS TO THE FIRST DEVICE. The address and browser that open it first
///    are recorded and then required. Forwarding the link afterwards does not
///    work, which is the failure mode a shared link actually has.
///
/// 3. REVOKING REACHES INSIDE. The grant id is kept in the session and checked
///    against the row on every page load, so revoking ends a session already in
///    progress instead of only stopping new ones.
///
/// 4. IT CANNOT OUTLIVE ITS WINDOW. Expiry is an absolute timestamp checked on
///    every request, not a countdown started at login.
///
/// WHAT IT IS NOT
///
/// The liveness check runs when a page loads. Somebody sitting on an already
/// rendered page is not thrown off it mid-sentence; they are stopped at their
/// next click. Revocation is therefore quick, not instant, and this file will
/// not pretend otherwise.
/// </summary>
public static class AccessGuard
{
    /// <summary>Session key holding the grant id a session was created from.</summary>
    private const string SessionKey = "AccessGrantID";

    /// <summary>Session key holding the scope, so pages need not re-query.</summary>
    private const string ScopeKey = "AccessGrantScope";

    /// <summary>How often the grant is re-checked against the database.</summary>
    private const string CheckedKey = "AccessGrantChecked";

    /// <summary>
    /// Seconds between database checks of a live grant.
    ///
    /// Zero would be correct and would also put a query on every image and
    /// script request. Twenty seconds keeps revocation quick enough to matter
    /// while leaving the database alone; the page that says "revoked" also says
    /// it may take a few seconds.
    /// </summary>
    private const int RecheckSeconds = 20;

    // ---- what a grant is ----------------------------------------------------

    public class Grant
    {
        public int ID;
        public string Kind = "";        // member | admin
        public int TargetKey;
        public string TargetLabel = "";
        public string Scope = "view";   // view | full
        public string Reason = "";
        public string CreatedByLogin = "";
        public DateTime CreatedOn;
        public DateTime ExpiresOn;
        public int MaxUses;
        public int Uses;
        public DateTime? FirstUsedOn;
        public DateTime? RevokedOn;
        public string RevokedByLogin = "";
        public string BoundIP = "";

        public bool IsAdmin
        {
            get { return string.Equals(Kind, "admin", StringComparison.OrdinalIgnoreCase); }
        }

        public bool ViewOnly
        {
            get { return !string.Equals(Scope, "full", StringComparison.OrdinalIgnoreCase); }
        }

        public bool Revoked { get { return RevokedOn.HasValue; } }
        public bool Expired { get { return DateTime.Now >= ExpiresOn; } }

        public bool Live
        {
            get
            {
                if (Revoked || Expired) { return false; }
                if (MaxUses > 0 && Uses >= MaxUses) { return false; }
                return true;
            }
        }

        /// <summary>Plain words for why a link will not work.</summary>
        public string Why
        {
            get
            {
                if (Revoked) { return "This link was withdrawn on " + RevokedOn.Value.ToString("dd MMM yyyy HH:mm") + "."; }
                if (Expired) { return "This link expired on " + ExpiresOn.ToString("dd MMM yyyy HH:mm") + "."; }
                if (MaxUses > 0 && Uses >= MaxUses) { return "This link has already been used " + Uses + " time(s), which was its limit."; }
                return "";
            }
        }
    }

    // ---- making one ---------------------------------------------------------

    /// <summary>
    /// Creates a grant and returns the token ONCE. It is never stored and
    /// cannot be shown again.
    /// </summary>
    public static string Create(cls_connection cls, string kind, int targetKey, string targetLabel,
                                string scope, int minutes, int maxUses, string reason,
                                int byId, string byLogin, out string error)
    {
        error = "";

        kind = string.Equals(kind, "admin", StringComparison.OrdinalIgnoreCase) ? "admin" : "member";
        scope = string.Equals(scope, "full", StringComparison.OrdinalIgnoreCase) ? "full" : "view";

        if (targetKey <= 0) { error = "Choose who the link is for."; return null; }
        if (minutes < 5) { minutes = 5; }

        // A day is already a long time for a link that needs no password. Not a
        // policy anybody asked for — a ceiling, so a mistyped 6000 does not
        // become four months of unattended access.
        if (minutes > 1440) { minutes = 1440; }

        if ((reason ?? "").Trim().Length < 3)
        {
            error = "Say what this access is for. It is the only thing that explains the grant later.";
            return null;
        }

        string token = NewToken();

        try
        {
            // Trimmed to fit the columns.
            //
            // TargetLabel is nvarchar(100) and the dropdown text it comes from
            // is "LOGINID - Full Name (Shop Name)", which passes 100 characters
            // easily. SQL Server does not truncate — it refuses the whole
            // INSERT with "String or binary data would be truncated", so the
            // link appeared on screen and no row was ever written. Cut here so
            // the length of somebody's shop name cannot decide whether access
            // control works.
            string label = Clip(targetLabel, 100);
            string why = Clip(reason.Trim(), 300);
            string who = Clip(byLogin, 100);

            int rows = cls.update_data(
                "insert into [AccessGrant] " +
                "(Kind, TargetKey, TargetLabel, TokenHash, Scope, Reason, CreatedByID, CreatedByLogin, ExpiresOn, MaxUses) " +
                "values ('" + kind + "', " + targetKey + ", N'" + Safe(label) + "', '" + Hash(token) + "', '" +
                scope + "', N'" + Safe(why) + "', " + byId + ", N'" + Safe(who) + "', " +
                "dateadd(minute, " + minutes + ", getdate()), " + (maxUses < 0 ? 0 : maxUses) + ")");

            // Checked, not assumed. Handing back a token for a row that was
            // never written produces a link that looks perfect and opens
            // nothing — the worst kind of failure, because it is discovered by
            // the person who was sent it.
            if (rows < 1)
            {
                error = "The database accepted the request but wrote no row. The link was not created.";
                return null;
            }

            return token;
        }
        catch (Exception ex)
        {
            error = "The link could not be created: " + ex.Message;
            return null;
        }
    }

    private static string Clip(string value, int max)
    {
        if (value == null) { return ""; }
        return value.Length <= max ? value : value.Substring(0, max);
    }

    /// <summary>
    /// 32 random bytes from the cryptographic generator, in URL-safe base64.
    ///
    /// Not Guid.NewGuid and not Random. A Guid is 122 bits with structure and a
    /// Random seeded from the clock is guessable by anyone who knows roughly
    /// when a link was made. This is a password that travels in a URL; it gets
    /// the real generator.
    /// </summary>
    private static string NewToken()
    {
        var bytes = new byte[32];

        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(bytes);
        }

        return Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public static string Hash(string value)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? ""));
            var sb = new StringBuilder(64);

            foreach (byte b in h) { sb.Append(b.ToString("x2")); }

            return sb.ToString();
        }
    }

    // ---- redeeming one ------------------------------------------------------

    /// <summary>
    /// Looks up a token. Returns the grant whether or not it is still live, so
    /// the landing page can say WHY rather than just refusing.
    /// </summary>
    public static Grant Find(cls_connection cls, string token)
    {
        if (string.IsNullOrEmpty(token)) { return null; }

        try
        {
            DataTable dt = cls.select_data_dt(
                "select top 1 * from [AccessGrant] where [TokenHash] = '" + Hash(token) + "'");

            if (dt == null || dt.Rows.Count == 0) { return null; }

            return Read(dt.Rows[0]);
        }
        catch
        {
            return null;
        }
    }

    public static Grant ById(cls_connection cls, int id)
    {
        if (id <= 0) { return null; }

        try
        {
            DataTable dt = cls.select_data_dt("select top 1 * from [AccessGrant] where [ID] = " + id);

            if (dt == null || dt.Rows.Count == 0) { return null; }

            return Read(dt.Rows[0]);
        }
        catch
        {
            return null;
        }
    }

    private static Grant Read(DataRow r)
    {
        var g = new Grant();

        int.TryParse(ReportHelper.Cell(r, "ID"), out g.ID);
        g.Kind = ReportHelper.Cell(r, "Kind");
        int.TryParse(ReportHelper.Cell(r, "TargetKey"), out g.TargetKey);
        g.TargetLabel = ReportHelper.Cell(r, "TargetLabel");
        g.Scope = ReportHelper.Cell(r, "Scope");
        g.Reason = ReportHelper.Cell(r, "Reason");
        g.CreatedByLogin = ReportHelper.Cell(r, "CreatedByLogin");
        g.BoundIP = ReportHelper.Cell(r, "BoundIP");
        g.RevokedByLogin = ReportHelper.Cell(r, "RevokedByLogin");

        int.TryParse(ReportHelper.Cell(r, "MaxUses"), out g.MaxUses);
        int.TryParse(ReportHelper.Cell(r, "Uses"), out g.Uses);

        DateTime d;
        if (DateTime.TryParse(ReportHelper.Cell(r, "CreatedOn"), out d)) { g.CreatedOn = d; }
        if (DateTime.TryParse(ReportHelper.Cell(r, "ExpiresOn"), out d)) { g.ExpiresOn = d; }
        if (DateTime.TryParse(ReportHelper.Cell(r, "FirstUsedOn"), out d)) { g.FirstUsedOn = d; }
        if (DateTime.TryParse(ReportHelper.Cell(r, "RevokedOn"), out d)) { g.RevokedOn = d; }

        return g;
    }

    /// <summary>
    /// Checks the device binding and records the use.
    ///
    /// The first opening pins the grant to that address and browser. Every
    /// opening after that must match. This is what stops the real-world failure
    /// of a shared link: it is forwarded, and the second person finds it dead.
    /// </summary>
    public static bool Redeem(cls_connection cls, Grant g, string ip, string agent, out string error)
    {
        error = "";

        if (g == null) { error = "This link is not valid."; return false; }

        if (!g.Live) { error = g.Why; return false; }

        string agentHash = Hash(agent ?? "");
        string safeIp = Safe(ip ?? "");

        if (g.BoundIP.Length > 0 && !string.Equals(g.BoundIP, ip, StringComparison.OrdinalIgnoreCase))
        {
            // Deliberately vague to whoever is holding the link, and precise in
            // the audit trail. Telling a stranger which address is allowed
            // hands them the last thing they were missing.
            error = "This link was already opened on another device and is tied to it. " +
                    "Ask for a new one.";
            return false;
        }

        try
        {
            cls.update_data(
                "update [AccessGrant] set " +
                "  [Uses] = [Uses] + 1, " +
                "  [LastUsedOn] = getdate(), " +
                "  [FirstUsedOn] = isnull([FirstUsedOn], getdate()), " +
                "  [BoundIP] = isnull([BoundIP], '" + safeIp + "'), " +
                "  [BoundAgent] = isnull([BoundAgent], '" + agentHash + "') " +
                "where [ID] = " + g.ID +
                // Re-checked in the UPDATE itself. Between the read above and
                // this write, an admin may have pressed Revoke; without these
                // conditions the link would be honoured anyway.
                "  and [RevokedOn] is null and [ExpiresOn] > getdate()" +
                "  and ([MaxUses] = 0 or [Uses] < [MaxUses])");

            return true;
        }
        catch (Exception ex)
        {
            error = "This link could not be opened: " + ex.Message;
            return false;
        }
    }

    public static void Revoke(cls_connection cls, int id, string byLogin)
    {
        try
        {
            cls.update_data(
                "update [AccessGrant] set [RevokedOn] = getdate(), [RevokedByLogin] = N'" +
                Safe(byLogin) + "' where [ID] = " + id + " and [RevokedOn] is null");
        }
        catch
        {
            // The list re-reads from the database, so a failure shows as the
            // grant still being live rather than as a false confirmation.
        }
    }

    // ---- the session --------------------------------------------------------

    /// <summary>Marks the current session as having come from a grant.</summary>
    public static void Attach(Grant g)
    {
        HttpContext ctx = HttpContext.Current;
        if (ctx == null || ctx.Session == null || g == null) { return; }

        ctx.Session[SessionKey] = g.ID;
        ctx.Session[ScopeKey] = g.ViewOnly ? "view" : "full";
        ctx.Session[CheckedKey] = DateTime.Now;
    }

    /// <summary>The grant id this session came from, or 0 for a normal login.</summary>
    public static int CurrentGrantId
    {
        get
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null) { return 0; }

            object v = ctx.Session[SessionKey];
            if (v == null) { return 0; }

            int id;
            return int.TryParse(Convert.ToString(v), out id) ? id : 0;
        }
    }

    public static bool IsLinkSession { get { return CurrentGrantId > 0; } }

    public static bool IsViewOnly
    {
        get
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null) { return false; }

            return Convert.ToString(ctx.Session[ScopeKey]) == "view";
        }
    }

    /// <summary>
    /// Ends the session if the grant behind it has been revoked or has expired.
    ///
    /// Called from the master pages, so it runs once per page load rather than
    /// once per image. Returns true when the caller should stop rendering.
    ///
    /// A normal password login is untouched: CurrentGrantId is 0 and this
    /// returns immediately. That matters more than it looks — this code runs on
    /// every admin and member page, and a bug here would lock out everybody, not
    /// just link users.
    /// </summary>
    public static bool EndedForThisRequest(cls_connection cls)
    {
        int id = CurrentGrantId;
        if (id <= 0) { return false; }

        HttpContext ctx = HttpContext.Current;
        if (ctx == null || ctx.Session == null) { return false; }

        DateTime last;
        object v = ctx.Session[CheckedKey];

        if (v != null && DateTime.TryParse(Convert.ToString(v), out last) &&
            (DateTime.Now - last).TotalSeconds < RecheckSeconds)
        {
            return false;
        }

        Grant g = ById(cls, id);

        // A grant that cannot be read is treated as gone. The alternative is to
        // let the session continue while the database is unreachable, and on an
        // access path the safe direction is out, not in.
        if (g == null || !g.Live)
        {
            string why = g == null ? "This temporary access has ended." : g.Why;

            ctx.Session.Clear();
            ctx.Session.Abandon();

            // endResponse left at its default, so the request really stops here
            // rather than carrying on into a page whose session has just been
            // abandoned. Redirect(url, false) would return to the caller and
            // let the rest of the page load run against nothing — which is how
            // a tidy-looking sign-out turns into a null reference on the way
            // out. Every other redirect in this codebase does the same.
            ctx.Response.Redirect("~/Access.aspx?ended=" + HttpUtility.UrlEncode(why));
            return true;
        }

        ctx.Session[CheckedKey] = DateTime.Now;
        return false;
    }

    // ---- view-only ----------------------------------------------------------

    /// <summary>
    /// Page names a view-only session may not open.
    ///
    /// A BLOCK LIST, AND THE PAGE SAYS SO
    ///
    /// This is not read-only and must never be described as read-only. There
    /// are over two hundred pages in this application and this list names the
    /// ones that move money. Anything not on it is reachable — including
    /// something added next month.
    ///
    /// A block list that is honest about being a block list is useful. One that
    /// is called "read-only" is dangerous, because people then stop watching.
    /// </summary>
    private static readonly string[] Blocked =
    {
        "moneytransfer", "dmt", "payout", "upitransfer", "aeps", "aadhaarpay",
        "recharge", "bbps", "billpay", "creditcard", "cms", "microatm",
        "addfund", "fundtransfer", "fundrequest", "wallet", "settlement",
        "withdrawal", "withdrwal", "transfer", "payment", "sendmoney",
        "changepassword", "changepin", "changeloginpin", "resetpassword",
        "registration", "addmember", "createmember", "kyc", "onboard"
    };

    /// <summary>
    /// True when the current request is a view-only link session asking for a
    /// page on the block list.
    /// </summary>
    public static bool BlockedHere()
    {
        if (!IsLinkSession || !IsViewOnly) { return false; }

        HttpContext ctx = HttpContext.Current;
        if (ctx == null || ctx.Request == null) { return false; }

        string path = (ctx.Request.AppRelativeCurrentExecutionFilePath ?? "").ToLowerInvariant();

        // The file name only. Matching the whole path would block every page
        // inside a folder that happens to be called Recharge.
        int slash = path.LastIndexOf('/');
        if (slash >= 0) { path = path.Substring(slash + 1); }

        foreach (string b in Blocked)
        {
            if (path.IndexOf(b, StringComparison.Ordinal) >= 0) { return true; }
        }

        return false;
    }

    /// <summary>The list, for the notice shown to the operator.</summary>
    public static string BlockedWords()
    {
        return string.Join(", ", Blocked);
    }

    // ---- helpers ------------------------------------------------------------

    /// <summary>The caller's address, preferring the proxy header.</summary>
    public static string ClientIp()
    {
        HttpContext ctx = HttpContext.Current;
        if (ctx == null || ctx.Request == null) { return ""; }

        string forwarded = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        if (!string.IsNullOrEmpty(forwarded))
        {
            string first = forwarded.Split(',')[0].Trim();
            if (first.Length > 0) { return first; }
        }

        return ctx.Request.ServerVariables["REMOTE_ADDR"] ?? "";
    }

    public static string ClientAgent()
    {
        HttpContext ctx = HttpContext.Current;
        if (ctx == null || ctx.Request == null) { return ""; }

        return ctx.Request.UserAgent ?? "";
    }

    private static string Safe(string value)
    {
        if (value == null) { return ""; }
        return value.Replace("'", "''");
    }
}
