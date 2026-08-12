using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.SessionState;

/// <summary>
/// The last few money transfers, as JSON, for the live panel on the dashboard.
///
/// WHY THIS IS NOT A WEBSOCKET
///
/// A socket is worth its complexity when the server has something to push. Here
/// it does not: the truth lives in a SQL table that nothing notifies us about,
/// so a SignalR hub would have to poll that table itself and then push. Same
/// database load, plus an OWIN startup, extra assemblies in Bin and a dependency
/// on the WebSocket protocol being enabled on the host — three new ways for a
/// live panel to break, for no new information.
///
/// So the client polls this, and the design work went into making the polling
/// cheap instead:
///
///   * The response carries a FINGERPRINT of the rows. The client sends back the
///     last one it saw, and when nothing has changed the reply is {"same":true} —
///     a few bytes, and the DataTable is never even serialised.
///   * Ten rows, ordered and capped in SQL.
///   * The client stops polling when the tab is hidden, so a dashboard left open
///     overnight costs nothing.
///
/// A status changing counts as a change, not just a new row — that is the whole
/// point of the panel, and it is why the fingerprint covers status and not only
/// the id.
/// </summary>
public class LiveFeedHandler : IHttpHandler, IReadOnlySessionState
{
    /// <summary>
    /// Ten. Enough to see what is happening, small enough that the query stays
    /// trivial when this runs every few seconds on every open dashboard.
    /// </summary>
    private const int Rows = 10;

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        // A live feed must never be cached — by the browser or by anything in
        // front of it. A cached feed is a feed that silently stops.
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.Cache.SetNoStore();
        context.Response.AddHeader("X-Content-Type-Options", "nosniff");

        // Same session check as the pages. Without this the handler would hand
        // customer names and account numbers to anyone who knew the URL.
        if (!SessionManager.IsSessionAdmin)
        {
            context.Response.StatusCode = 401;
            context.Response.Write("{\"error\":\"session\"}");
            return;
        }

        string since = (context.Request.QueryString["v"] ?? "").Trim();

        try
        {
            Write(context, since);
        }
        catch (Exception ex)
        {
            // The panel shows a quiet "reconnecting" line and keeps trying. It
            // must not take the dashboard down with it.
            context.Response.StatusCode = 200;
            context.Response.Write("{\"error\":" + Json(ex.Message) + "}");
        }
    }

    /// <summary>
    /// Services this feed covers, in the order they should be tried.
    ///
    /// Named rather than "every table ServiceUsageHelper knows about", because
    /// settlement is a member moving their own money and does not belong in a
    /// stream of customer transactions — it would pad the ten rows with
    /// something nobody is watching for.
    /// </summary>
    private static readonly string[] Feeds =
        { "MoneyTransfer", "UPITransfer", "Aeps_history", "tblRecharge_History" };

    /// <summary>
    /// A short line of context per service, from whichever column that table
    /// actually has. Checked against the real schema before it reaches SQL, so
    /// a table without any of them simply shows nothing rather than failing.
    /// </summary>
    private static readonly string[] DetailCandidates =
        { "CustomerName", "BeneficiaryName", "CustomerMobile", "MobileNo", "Mobile",
          "AadharNumber", "AadhaarNo", "VPA", "UPIID", "AccountNO", "Number", "OperatorName" };

    private static void Write(HttpContext context, string since)
    {
        var cls = new cls_connection();

        // One UNION over the four services rather than four queries and a merge
        // in C#. SQL does the ordering and the TOP, so exactly ten rows come
        // back however busy any one service is — and a quiet service cannot
        // crowd out a busy one, which is what merging fixed-size per-table
        // slices would do.
        var union = new StringBuilder();
        int used = 0;

        foreach (ServiceUsageHelper.ServiceTable st in ServiceUsageHelper.Tables(cls))
        {
            if (!st.Usable) { continue; }
            if (Array.IndexOf(Feeds, st.Table) < 0) { continue; }

            string id = st.IdCol == null
                ? "cast(t.[" + st.DateCol + "] as nvarchar(40))"
                : "cast(t.[" + st.IdCol + "] as nvarchar(60))";

            string detail = Detail(cls, st);

            if (used > 0) { union.Append(" union all "); }
            used++;

            union.Append("select N'").Append(st.Service.Replace("'", "''")).Append("' as Svc")
                 .Append(", isnull(").Append(id).Append(", '') as Rid")
                 .Append(", t.[").Append(st.DateCol).Append("] as At")
                 .Append(", cast(isnull(t.[").Append(st.AmountCol).Append("],0) as decimal(18,2)) as Amt")
                 .Append(", cast(isnull(t.[").Append(st.StatusCol).Append("], '') as nvarchar(40)) as Sts")
                 .Append(", ").Append(detail).Append(" as Detail")
                 .Append(", t.[").Append(st.MemberCol).Append("] as MemKey")
                 .Append(", N'").Append(JoinColFor(st)).Append("' as MemKind")
                 .Append(" from [").Append(st.Table).Append("] t");
        }

        if (used == 0)
        {
            context.Response.Write("{\"v\":\"none\",\"rows\":[]}");
            return;
        }

        // The member is joined once, outside the union, on whichever key that
        // service stores. tblRecharge_History keys on member.ID while the rest
        // key on msrno — handing a table the wrong one returns nothing at all,
        // silently, which is how a service quietly vanishes from a feed.
        string sql =
            "select top " + Rows + " x.*, " +
            "  isnull(mm.[LoginID], '') as LoginID_, isnull(mm.[Name], '') as MemberName_ " +
            "from (" + union + ") x " +
            "left join [member] mm " +
            "  on (x.MemKind = 'msrno' and mm.[msrno] = x.MemKey) " +
            "  or (x.MemKind = 'ID'    and mm.[ID]    = x.MemKey) " +
            "order by x.At desc";

        DataTable dt = cls.select_data_dt(sql);

        if (dt == null || dt.Rows.Count == 0)
        {
            context.Response.Write("{\"v\":\"empty\",\"rows\":[]}");
            return;
        }

        // The fingerprint covers the service, the id AND the status of every row
        // on screen. Keying on the newest id alone would miss the case this
        // panel exists for: nothing new arriving, but a pending transfer turning
        // into a success or a failure.
        var fp = new StringBuilder();

        foreach (DataRow r in dt.Rows)
        {
            fp.Append(Cell(r, "Svc")).Append('/').Append(Cell(r, "Rid"))
              .Append(':').Append(Cell(r, "Sts")).Append('|');
        }

        string version = Hash(fp.ToString());

        if (version == since)
        {
            context.Response.Write("{\"same\":true,\"v\":" + Json(version) + "}");
            return;
        }

        var sb = new StringBuilder();
        sb.Append("{\"v\":").Append(Json(version)).Append(",\"rows\":[");

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            DataRow r = dt.Rows[i];

            if (i > 0) { sb.Append(','); }

            // Service is part of the key. Two tables can easily hand out the
            // same id, and a collision would make a recharge look like the
            // money transfer above it had changed status.
            sb.Append("{\"id\":").Append(Json(Cell(r, "Svc") + "/" + Cell(r, "Rid")))
              .Append(",\"svc\":").Append(Json(Cell(r, "Svc")))
              // The bare reference, so a row can be clicked straight through to
              // the transaction search. Sent apart from "id" because id is
              // deliberately prefixed with the service to keep it unique across
              // tables, and that prefix is not something anyone can search for.
              .Append(",\"ref\":").Append(Json(Cell(r, "Rid")))
              .Append(",\"at\":").Append(Json(When(Cell(r, "At"))))
              .Append(",\"amount\":").Append(Json(Money(Cell(r, "Amt"))))
              .Append(",\"status\":").Append(Json(Cell(r, "Sts")))
              // Masked. This panel sits open on a screen all day, often with
              // other people in the room, and a customer's full name is not
              // needed to see that a transaction went through.
              .Append(",\"customer\":").Append(Json(Shorten(Cell(r, "Detail"))))
              .Append(",\"member\":").Append(Json(Cell(r, "LoginID_")))
              .Append(",\"memberName\":").Append(Json(Cell(r, "MemberName_")))
              .Append("}");
        }

        sb.Append("]}");
        context.Response.Write(sb.ToString());
    }

    /// <summary>
    /// The detail expression for one table, built only from columns it really
    /// has. An unchecked column name here would take the whole feed down, not
    /// just one service — the union fails as a unit.
    /// </summary>
    private static string Detail(cls_connection cls, ServiceUsageHelper.ServiceTable st)
    {
        try
        {
            DataTable cols = cls.select_data_dt(
                "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" +
                st.Table.Replace("'", "''") + "'");

            if (cols != null)
            {
                foreach (string want in DetailCandidates)
                {
                    foreach (DataRow c in cols.Rows)
                    {
                        if (string.Equals(Convert.ToString(c["COLUMN_NAME"]), want,
                                          StringComparison.OrdinalIgnoreCase))
                        {
                            return "cast(isnull(t.[" + want + "], '') as nvarchar(60))";
                        }
                    }
                }
            }
        }
        catch
        {
        }

        return "cast('' as nvarchar(60))";
    }

    /// <summary>
    /// Which member column this table's key points at. Mirrors the rule in
    /// ServiceUsageHelper: anything msrno-shaped joins member.msrno, a MemberID
    /// joins member.ID.
    /// </summary>
    private static string JoinColFor(ServiceUsageHelper.ServiceTable st)
    {
        return st.MemberCol.ToUpperInvariant().IndexOf("MSRNO", StringComparison.Ordinal) >= 0
            ? "msrno"
            : "ID";
    }

    // ---- formatting --------------------------------------------------------

    /// <summary>
    /// "2 min ago" style, because on a live panel the useful question is how
    /// stale a row is, not what o'clock it was.
    /// </summary>
    private static string When(string raw)
    {
        DateTime d;
        if (!DateTime.TryParse(raw, out d)) { return raw; }

        TimeSpan age = DateTime.Now - d;

        if (age.TotalSeconds < 0) { return d.ToString("HH:mm"); }
        if (age.TotalSeconds < 60) { return "just now"; }
        if (age.TotalMinutes < 60) { return (int)age.TotalMinutes + " min ago"; }
        if (age.TotalHours < 24) { return (int)age.TotalHours + " hr ago"; }

        return d.ToString("dd MMM HH:mm");
    }

    private static string Money(string raw)
    {
        decimal v;
        return decimal.TryParse(raw, out v) ? v.ToString("N2") : raw;
    }

    /// <summary>
    /// First name plus an initial. Enough to recognise a transaction you are
    /// watching, not enough to be a customer list on a shared screen.
    /// </summary>
    private static string Shorten(string name)
    {
        string s = (name ?? "").Trim();
        if (s.Length == 0) { return ""; }

        string[] parts = s.Split(' ');
        if (parts.Length == 1) { return parts[0]; }

        return parts[0] + " " + parts[parts.Length - 1].Substring(0, 1) + ".";
    }

    private static string Cell(DataRow r, string column)
    {
        if (r == null || !r.Table.Columns.Contains(column)) { return ""; }

        object v = r[column];
        if (v == null || v == DBNull.Value) { return ""; }

        return Convert.ToString(v).Trim();
    }

    /// <summary>
    /// A short, stable fingerprint. Only ever compared with itself, so this is
    /// about cheapness and not about being hard to reverse.
    /// </summary>
    private static string Hash(string s)
    {
        unchecked
        {
            int h = 23;
            foreach (char c in s) { h = h * 31 + c; }
            return h.ToString("x8") + "-" + s.Length.ToString("x");
        }
    }

    /// <summary>
    /// A JSON string literal.
    ///
    /// Written out rather than pulled from a serialiser because these values
    /// are customer names and bank names from a live table, and one unescaped
    /// quote or angle bracket would either break the parse or let markup out of
    /// a data field.
    /// </summary>
    private static string Json(string value)
    {
        if (value == null) { return "\"\""; }

        var sb = new StringBuilder("\"");

        foreach (char c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '<': sb.Append("\\u003c"); break;
                case '>': sb.Append("\\u003e"); break;
                case '&': sb.Append("\\u0026"); break;
                default:
                    if (c < ' ') { sb.Append("\\u").Append(((int)c).ToString("x4")); }
                    else { sb.Append(c); }
                    break;
            }
        }

        return sb.Append('"').ToString();
    }

    public bool IsReusable
    {
        get { return false; }
    }
}
