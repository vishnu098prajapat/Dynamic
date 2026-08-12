using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;

/// <summary>
/// Pulls one member's (or a whole downline's) business together across the five
/// transaction tables so an admin can see what services are actually being used.
///
/// WHY THIS DISCOVERS THE SCHEMA INSTEAD OF HARD-CODING IT
///
/// Every one of these tables is reached through a stored procedure
/// everywhere else in the solution, so the column names never appear in the C#.
/// tblRecharge_History is the only one whose member key is visible in code
/// ("where MsrNo="). For MoneyTransfer, UPITransfer and Aeps_history the amount,
/// status, date and member columns are not written down anywhere I can read.
///
/// Guessing them would produce a page that looks authoritative and quietly
/// reports the wrong numbers — which for a business analysis screen is worse
/// than reporting nothing. So this asks INFORMATION_SCHEMA what the columns
/// really are, matches them against a ranked list of the names this codebase
/// uses elsewhere, and says plainly when it cannot map one rather than
/// substituting a zero.
///
/// The lookup runs once per application lifetime and is cached.
/// </summary>
public static class ServiceUsageHelper
{
    // ---- the services ------------------------------------------------------

    public class ServiceTable
    {
        public string Service;      // family label shown to the operator
        public string Table;        // real table name
        public string MemberCol;    // resolved
        public string AmountCol;    // resolved
        public string StatusCol;    // resolved
        public string DateCol;      // resolved
        public string TypeCol;      // optional, e.g. Aeps_history.TxnType
        public string ApiCol;       // optional, the provider this row went through
        public string IdCol;        // the row's own identifier, for a live feed
        public string Problem;      // non-null when it could not be mapped
        public string MemberNote;   // why this member column was chosen

        public bool Usable
        {
            get { return Problem == null; }
        }
    }

    /// <summary>
    /// Candidate names in preference order. These are the spellings this
    /// codebase actually uses; the first one the table really has wins.
    /// </summary>
    /// <summary>
    /// The member column each table really uses, stated per table rather than
    /// guessed from a ranked list.
    ///
    /// This matters more than it looks. tblRecharge_History keys on MemberID
    /// (which holds member.ID); the other three key on msrno. member has BOTH
    /// an ID and an msrno and they are different numbers, so handing a table
    /// the wrong one returns NOTHING — no error, no warning, just an empty
    /// report that reads as "this member did no business".
    ///
    /// A global preference list got this wrong: tblRecharge_History also has a
    /// MsrNo column (this codebase queries it), so a list that preferred MsrNo
    /// picked the wrong one. Hence the explicit map. The generic list below is
    /// only a fallback for a table not named here.
    /// </summary>
    private static readonly Dictionary<string, string> PreferredMemberCol =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "MoneyTransfer",          "msrno" },
            { "UPITransfer",            "msrno" },
            { "Aeps_history",           "msrno" },
            { "tblRecharge_History",    "MemberID" },
            { "AEPS_WithdrwalRequest",  "msrno" }
        };

    /// <summary>
    /// Status and date columns stated per table for the same reason as the
    /// member column: the generic lists below are ranked guesses, and a table
    /// that has BOTH names gets the wrong one.
    ///
    /// AEPS_WithdrwalRequest is the case in point. Its status lives in
    /// RequestStatus and its timestamp in RequestDate, but the generic lists
    /// rank plain "Status" and "AddDate" first. Picking a status column that
    /// never says 'Success' would report every settlement as failed; picking the
    /// wrong date column would drop rows out of the period silently.
    /// </summary>
    private static readonly Dictionary<string, string> PreferredStatusCol =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "AEPS_WithdrwalRequest", "RequestStatus" }
        };

    private static readonly Dictionary<string, string> PreferredDateCol =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "AEPS_WithdrwalRequest", "RequestDate" }
        };

    private static readonly string[] MemberCandidates =
        { "MsrNo", "Msrno", "msrno", "MSRNO", "MemberID", "Memberid", "MemberId" };

    private static readonly string[] AmountCandidates =
        { "Amount", "AMOUNT", "amount", "TransAmount", "Amt" };

    private static readonly string[] StatusCandidates =
        { "Status", "STATUS", "status", "RequestSTatus", "RequestStatus", "TransStatus" };

    private static readonly string[] DateCandidates =
        { "AddDate", "Adddate", "adddate", "CreatedDate", "RequestDate", "TransDate", "EntryDate", "DOT" };

    /// <summary>
    /// Optional. Aeps_history carries a TxnType — CW for a cash withdrawal, and
    /// separate codes for balance enquiry, mini statement and Aadhaar Pay. Only
    /// the withdrawal moves money; the rest are free lookups that still write a
    /// row. Counting them together makes the AEPS numbers meaningless: a member
    /// who checked two hundred balances would show as two hundred transactions
    /// worth nothing, and the success rate would describe enquiries rather than
    /// business. Where this column exists the service is split by it.
    /// </summary>
    private static readonly string[] TypeCandidates =
        { "TxnType", "Txntype", "txntype", "TransType", "TransactionType", "ServiceType" };

    /// <summary>
    /// Type-column name candidates for tables where the generic list would pick
    /// the wrong column.
    ///
    /// AEPS_WithdrwalRequest is a settlement: the member moves their own AEPS
    /// balance either to their bank account or to their main wallet, and those
    /// are two different pieces of behaviour. The table also carries a mode
    /// (IMPS / NEFT), and the generic list is perfectly capable of grabbing that
    /// instead — which would split the settlement by transfer rail rather than
    /// by destination, and look plausible while answering a question nobody
    /// asked. The names below are tried first, and whichever is chosen is then
    /// checked against the values it actually holds (see TypeMustContain).
    /// </summary>
    private static readonly Dictionary<string, string[]> PreferredTypeCandidates =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Aeps_history", new string[] { "TxnType", "TransType", "TransactionType" } },
            { "AEPS_WithdrwalRequest", new string[] {
                "SType", "WithdrawType", "WithdrwalType", "SettlementType",
                "RequestType", "PayoutType", "TransferType", "Type" } }
        };

    /// <summary>
    /// Values a type column must really contain before it is trusted.
    ///
    /// The Bank/Wallet split for AEPS_WithdrwalRequest is set by a stored
    /// procedure — PROC_AEPS_WITHDRWALREQUEST is passed 'Bank' or 'Wallet' — so
    /// the parameter name is visible in this codebase but the column name it
    /// lands in is not. Rather than guess and hope, the candidate column is
    /// sampled: if its values do not actually include Bank or Wallet, the other
    /// short text columns are scanned for one that does. If nothing matches, the
    /// service simply is not split — a single honest "Settlement" tile beats a
    /// confident split along the wrong axis.
    /// </summary>
    private static readonly Dictionary<string, string[]> TypeMustContain =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "AEPS_WithdrwalRequest", new string[] { "BANK", "WALLET" } }
        };

    /// <summary>
    /// Readable names for type codes, so the screen says what the code means.
    ///
    /// Only codes that are confirmed appear here. An unknown code falls through
    /// to "Service · CODE" rather than being given an invented meaning — a
    /// mislabelled tile is worse than a raw one, because nobody double-checks a
    /// label that reads like English.
    ///
    /// AW is Aadhaar Pay, which is a different product from a cash withdrawal:
    /// the customer pays a merchant with their fingerprint instead of drawing
    /// cash. It gets its own name rather than an "AEPS · AW" suffix so it reads
    /// as the separate service it is.
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, string>> TypeLabels =
        new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Aeps_history",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "CW", "AEPS cash withdrawal" },
                    { "AW", "Aadhaar Pay" }
                }
            }
        };

    // Bumped whenever the resolution rules change, so a running application
    // picks up the new mapping instead of serving the cached old one.
    /// <summary>
    /// The column naming which provider a transaction went through.
    ///
    /// Every one of these tables carries one, and tblRecharge_API turns the id
    /// into a name. Optional on purpose: a table without it is reported as NOT
    /// MEASURED rather than folded into whichever API happens to be first,
    /// because "this service has no API breakdown" and "this service all went
    /// through API 3" are completely different findings.
    /// </summary>
    private static readonly string[] ApiCandidates =
        { "APIID", "ApiID", "ApiId", "Apiid", "apiid", "API_ID", "ProviderID" };

    /// <summary>
    /// The column identifying one row.
    ///
    /// Every table spells this differently, and there is no safe default: a
    /// live feed that cannot tell two rows apart looks connected and never
    /// updates, which is worse than an error. TransID is listed but appears on
    /// none of the base tables — it is added by the reporting procedures, and
    /// assuming it cost a working feed once already.
    /// </summary>
    private static readonly string[] IdCandidates =
        { "OrderID", "TransactionID", "TRANSACTIONID", "TransID", "RequestID", "RRN", "ID" };

    // Bumped whenever the resolution rules change, so a running application
    // picks up the new mapping instead of serving the cached old one.
    private const string CacheKey = "ServiceUsageHelper_Schema_v4";

    /// <summary>
    /// The service tables with their columns resolved. Cached in Application
    /// state so INFORMATION_SCHEMA is read once, not on every page view.
    /// </summary>
    public static List<ServiceTable> Tables(cls_connection cls)
    {
        HttpApplicationState app = HttpContext.Current == null ? null : HttpContext.Current.Application;

        if (app != null)
        {
            var cached = app[CacheKey] as List<ServiceTable>;
            if (cached != null) { return cached; }
        }

        var list = new List<ServiceTable>();
        list.Add(Resolve(cls, "Money transfer", "MoneyTransfer"));
        list.Add(Resolve(cls, "UPI transfer", "UPITransfer"));
        list.Add(Resolve(cls, "AEPS", "Aeps_history"));
        list.Add(Resolve(cls, "Recharge & bills", "tblRecharge_History"));

        // Settlement is the member moving their own AEPS earnings out — to their
        // bank account or across to their main wallet. It is not new business
        // won from a customer, so it is labelled as its own family rather than
        // folded into AEPS, and the Bank and Wallet legs are reported apart
        // because they answer different questions: how much is leaving the
        // system, versus how much is being recycled inside it.
        list.Add(Resolve(cls, "Settlement", "AEPS_WithdrwalRequest"));

        if (app != null) { app[CacheKey] = list; }

        return list;
    }

    /// <summary>Forgets the cached schema, for use after a database change.</summary>
    public static void ClearSchemaCache()
    {
        if (HttpContext.Current != null && HttpContext.Current.Application != null)
        {
            HttpContext.Current.Application.Remove(CacheKey);
        }
    }

    private static ServiceTable Resolve(cls_connection cls, string service, string table)
    {
        var st = new ServiceTable();
        st.Service = service;
        st.Table = table;

        try
        {
            DataTable cols = cls.select_data_dt(
                "select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH " +
                "from INFORMATION_SCHEMA.COLUMNS " +
                "where TABLE_NAME = '" + table.Replace("'", "''") + "'");

            if (cols == null || cols.Rows.Count == 0)
            {
                st.Problem = "Table " + table + " was not found in this database.";
                return st;
            }

            var names = new List<string>();
            var dateNames = new List<string>();
            var shortText = new List<string>();

            foreach (DataRow r in cols.Rows)
            {
                string name = Convert.ToString(r["COLUMN_NAME"]);
                names.Add(name);

                string type = Convert.ToString(r["DATA_TYPE"]).ToLowerInvariant();
                if (type.IndexOf("date", StringComparison.Ordinal) >= 0) { dateNames.Add(name); }

                // Short text columns are the only ones a Bank/Wallet style flag
                // can live in, and they are cheap to sample. A description or a
                // JSON response column is neither, so the length cap keeps the
                // value scan off them.
                if (type == "varchar" || type == "nvarchar" || type == "char" || type == "nchar")
                {
                    int len;
                    if (int.TryParse(Convert.ToString(r["CHARACTER_MAXIMUM_LENGTH"]), out len) &&
                        len > 0 && len <= 50)
                    {
                        shortText.Add(name);
                    }
                }
            }

            // Preferred column first, and only if the table really has it.
            string preferred;
            if (PreferredMemberCol.TryGetValue(table, out preferred))
            {
                st.MemberCol = FirstPresent(names, new string[] { preferred });

                if (st.MemberCol != null)
                {
                    st.MemberNote = "configured for this table";
                }
                else
                {
                    st.MemberCol = FirstPresent(names, MemberCandidates);
                    st.MemberNote = "configured column '" + preferred +
                                    "' is not on this table — fell back to a guess";
                }
            }
            else
            {
                st.MemberCol = FirstPresent(names, MemberCandidates);
                st.MemberNote = "guessed (no explicit mapping for this table)";
            }
            st.AmountCol = FirstPresent(names, AmountCandidates);
            st.StatusCol = Pick(names, table, PreferredStatusCol, StatusCandidates);
            st.DateCol = Pick(names, table, PreferredDateCol, DateCandidates);
            st.TypeCol = ResolveTypeCol(cls, table, names, shortText);
            st.ApiCol = FirstPresent(names, ApiCandidates);
            st.IdCol = FirstPresent(names, IdCandidates);

            // If none of the usual date names matched, take the first real date
            // column rather than giving up — a report without a date filter is
            // useless, but the wrong member or amount column is dangerous, so
            // only this one falls back.
            if (st.DateCol == null && dateNames.Count > 0) { st.DateCol = dateNames[0]; }

            var missing = new List<string>();
            if (st.MemberCol == null) { missing.Add("member"); }
            if (st.AmountCol == null) { missing.Add("amount"); }
            if (st.StatusCol == null) { missing.Add("status"); }
            if (st.DateCol == null) { missing.Add("date"); }

            if (missing.Count > 0)
            {
                st.Problem = "Could not identify the " + string.Join(", ", missing.ToArray()) +
                             " column" + (missing.Count > 1 ? "s" : "") + " on " + table + ".";
            }
        }
        catch (Exception ex)
        {
            st.Problem = "Could not read the schema for " + table + ": " + ex.Message;
        }

        return st;
    }

    private static string FirstPresent(List<string> actual, string[] candidates)
    {
        foreach (string c in candidates)
        {
            foreach (string a in actual)
            {
                if (string.Equals(a, c, StringComparison.OrdinalIgnoreCase)) { return a; }
            }
        }
        return null;
    }

    /// <summary>
    /// The column this table was explicitly configured with, or the best guess
    /// from the ranked list if it has no entry (or the configured name is not
    /// really there).
    /// </summary>
    private static string Pick(List<string> names, string table,
                               Dictionary<string, string> preferred, string[] candidates)
    {
        string configured;

        if (preferred.TryGetValue(table, out configured))
        {
            string hit = FirstPresent(names, new string[] { configured });
            if (hit != null) { return hit; }
        }

        return FirstPresent(names, candidates);
    }

    /// <summary>
    /// The column a service should be split by, verified against its own values
    /// where the values are known in advance.
    ///
    /// Name matching alone is not enough here. Several columns on a settlement
    /// table look like a type — a transfer mode, a provider code, a channel —
    /// and picking any of them produces a chart that is internally consistent
    /// and answers the wrong question. Where TypeMustContain names the values
    /// expected, the candidate is sampled first and the other short text columns
    /// are scanned only if it does not carry them. Nothing matching means the
    /// service is reported whole, which is honest.
    /// </summary>
    private static string ResolveTypeCol(cls_connection cls, string table,
                                         List<string> names, List<string> shortText)
    {
        string[] byName;
        string chosen = PreferredTypeCandidates.TryGetValue(table, out byName)
            ? FirstPresent(names, byName)
            : FirstPresent(names, TypeCandidates);

        string[] wanted;
        if (!TypeMustContain.TryGetValue(table, out wanted))
        {
            return chosen;   // nothing to verify against
        }

        if (chosen != null && ColumnHolds(cls, table, chosen, wanted)) { return chosen; }

        // Scan, cheapest first: the name-shaped candidates, then anything else
        // short enough to be a flag. Capped so an unexpectedly wide table cannot
        // turn one schema lookup into fifty queries.
        int scanned = 0;

        foreach (string col in shortText)
        {
            if (scanned++ >= 15) { break; }
            if (string.Equals(col, chosen, StringComparison.OrdinalIgnoreCase)) { continue; }

            if (ColumnHolds(cls, table, col, wanted)) { return col; }
        }

        return null;
    }

    /// <summary>
    /// True when a sample of this column actually contains one of the given
    /// values. Sampled rather than scanned in full — the question is "is this
    /// the flag column", and a few thousand rows answer that without a table
    /// scan on a live database. Runs once per application lifetime.
    /// </summary>
    private static bool ColumnHolds(cls_connection cls, string table, string col, string[] wanted)
    {
        try
        {
            var inList = new StringBuilder();

            foreach (string w in wanted)
            {
                if (inList.Length > 0) { inList.Append(','); }
                inList.Append('\'').Append(w.ToUpperInvariant().Replace("'", "''")).Append('\'');
            }

            string sql =
                "select count(*) as Hits from (" +
                "  select distinct upper(ltrim(rtrim(cast(" + Q(col) + " as nvarchar(60))))) as V" +
                "  from (select top 5000 " + Q(col) + " from " + Q(table) + ") s" +
                ") d where d.V in (" + inList + ")";

            DataTable dt = cls.select_data_dt(sql);

            if (dt == null || dt.Rows.Count == 0) { return false; }

            int hits;
            return int.TryParse(ReportHelper.Cell(dt.Rows[0], "Hits"), out hits) && hits > 0;
        }
        catch
        {
            // A column that cannot be cast to text is not the flag column.
            return false;
        }
    }

    /// <summary>
    /// The SQL expression that names a row's service.
    ///
    /// Built here and used for both the SELECT and the GROUP BY so the label and
    /// the grouping cannot drift apart — a label that disagrees with its own
    /// group is a report that adds the wrong rows together.
    ///
    /// <paramref name="alias"/> is "" or "t." depending on whether the caller's
    /// query aliases the table.
    /// </summary>
    public static string LabelSql(ServiceTable st, string alias)
    {
        string family = "N'" + st.Service.Replace("'", "''") + "'";

        if (st.TypeCol == null) { return family; }

        string raw = "ltrim(rtrim(isnull(cast(" + alias + Q(st.TypeCol) +
                     " as nvarchar(30)), 'Other')))";

        // Unknown codes keep the family name and show the code as-is.
        string suffixed = family + " + N' \u00b7 ' + " + raw;

        Dictionary<string, string> map;
        if (!TypeLabels.TryGetValue(st.Table, out map) || map.Count == 0) { return suffixed; }

        var sb = new StringBuilder();
        sb.Append("case upper(").Append(raw).Append(")");

        foreach (KeyValuePair<string, string> kv in map)
        {
            sb.Append(" when N'").Append(kv.Key.ToUpperInvariant().Replace("'", "''"))
              .Append("' then N'").Append(kv.Value.Replace("'", "''")).Append("'");
        }

        sb.Append(" else ").Append(suffixed).Append(" end");

        return sb.ToString();
    }

    // ---- downline ----------------------------------------------------------

    /// <summary>
    /// The two keys a downline can be identified by.
    ///
    /// member has BOTH an ID and an msrno, and they are different numbers —
    /// "select ID from member where msrno = ..." exists in this codebase, which
    /// only makes sense if they differ. The transaction tables do not agree on
    /// which one they store: tblRecharge_History keys on MsrNo, others may key
    /// on MemberID. Handing the msrno list to a table that stores IDs returns
    /// nothing at all, silently — which is exactly what a business report must
    /// never do. So both lists are carried and each table is given the one that
    /// matches its own column.
    /// </summary>
    public class DownlineKeys
    {
        public string MsrnoList = "";
        public string IdList = "";
        public int Count;
        public string LinkNote = "";

        public bool Empty
        {
            get { return MsrnoList.Length == 0 && IdList.Length == 0; }
        }
    }

    /// <summary>
    /// What a member dropdown's selected value actually points at.
    ///
    /// The member dropdowns bind their value on a column aliased "ID", but that
    /// alias is wrong: the value is really an msrno. AddFund binds the same way
    /// and then uses the selected value as an msrno —
    ///
    ///     FillDropDown(ddlMember, "MemberList", "LoginID", "ID", ...)
    ///     int.TryParse(ddlMember.SelectedValue, out msrno);
    ///     select * from View_MemberBalance where msrno = &lt;that value&gt;
    ///
    /// so at least one of these procedures returns msrno under the name ID, and
    /// the two are different numbers. Treating an msrno as an ID finds the wrong
    /// member or none, the tree walk then returns nobody, and a distributor with
    /// a full downline reads as having none.
    ///
    /// So the value is looked up against both columns and whichever matches
    /// wins. Real member.ID and member.msrno come back, along with a note saying
    /// which the dropdown was really giving.
    /// </summary>
    public class MemberRef
    {
        public int Id;
        public int Msrno;
        public string LoginId = "";
        public string Name = "";
        public string MatchNote = "";

        public bool Found
        {
            get { return Id > 0; }
        }
    }

    public static MemberRef ResolveMember(cls_connection cls, int selectedValue)
    {
        var m = new MemberRef();

        if (selectedValue <= 0)
        {
            m.MatchNote = "no member selected";
            return m;
        }

        try
        {
            // Both interpretations in one round trip, tagged so the winner is
            // known without a second query. msrno is listed first because that
            // is what the member dropdowns actually carry — the value column is
            // aliased "ID" but holds msrno.
            DataTable dt = cls.select_data_dt(
                "select ID, msrno, LoginID, Name, 'msrno' as MatchedOn from member where msrno = " + selectedValue +
                " union all " +
                "select ID, msrno, LoginID, Name, 'ID' as MatchedOn from member where ID = " + selectedValue);

            if (dt == null || dt.Rows.Count == 0)
            {
                m.MatchNote = "no member has ID or msrno = " + selectedValue;
                return m;
            }

            // Prefer the msrno match when both exist. The dropdown carries
            // msrno, so that is the reading the operator intended; two different
            // members can legitimately have ID=5 and msrno=5.
            DataRow chosen = dt.Rows[0];

            foreach (DataRow r in dt.Rows)
            {
                if (Convert.ToString(r["MatchedOn"]) == "msrno") { chosen = r; break; }
            }

            int.TryParse(ReportHelper.Cell(chosen, "ID"), out m.Id);
            int.TryParse(ReportHelper.Cell(chosen, "msrno"), out m.Msrno);
            m.LoginId = ReportHelper.Cell(chosen, "LoginID");
            m.Name = ReportHelper.Cell(chosen, "Name");

            string matchedOn = Convert.ToString(chosen["MatchedOn"]);
            m.MatchNote = "dropdown value " + selectedValue + " matched member." + matchedOn +
                          "  ->  ID " + m.Id + ", msrno " + m.Msrno;

            if (dt.Rows.Count > 1)
            {
                m.MatchNote += "  (it also matches a different member by " +
                               (matchedOn == "msrno" ? "ID" : "msrno") + " — " + matchedOn + " was used)";
            }

            return m;
        }
        catch (Exception ex)
        {
            m.MatchNote = "could not resolve the selected member: " + ex.Message;
            return m;
        }
    }

    /// <summary>
    /// Every member at or below the given one, as both an msrno list and an ID
    /// list ready to drop into an IN clause.
    ///
    /// member.ParentID holds the parent's MSRNO, and the member dropdowns hand
    /// out msrno too (their value column is aliased "ID" but is not one). So
    /// selecting a distributor means: take their msrno, find every member whose
    /// ParentID is that msrno, and repeat down the tree.
    ///
    /// The ID walk is still run as a safety net. Getting this wrong does not
    /// error — it returns the one member you selected and no children, so a
    /// distributor with a full downline reads as having none. Running both and
    /// keeping whichever finds people means a schema that disagrees with the
    /// expectation degrades to a correct answer instead of an empty one, and the
    /// diagnostics panel says which was used.
    /// </summary>
    public static DownlineKeys Downline(cls_connection cls, int rootMemberId, bool includeSelf)
    {
        DownlineKeys byMsrno = WalkTree(cls, rootMemberId, includeSelf, "msrno");
        DownlineKeys byId = WalkTree(cls, rootMemberId, includeSelf, "ID");

        // ParentID holds the parent's msrno, so that walk is the expected one
        // and wins ties. The ID walk is kept only as a safety net: if a database
        // ever stored IDs there instead, this still finds the downline rather
        // than reporting an empty tree.
        DownlineKeys chosen;

        if (byMsrno.Count >= byId.Count)
        {
            chosen = byMsrno;
            chosen.LinkNote = "member.ParentID matched against member.msrno";
        }
        else
        {
            chosen = byId;
            chosen.LinkNote = "member.ParentID matched against member.ID  (unexpected — " +
                              "ParentID normally holds msrno)";
        }

        chosen.LinkNote += "  (ID walk found " + byId.Count +
                           ", msrno walk found " + byMsrno.Count + ")";

        if (chosen.Count <= 1)
        {
            chosen.LinkNote += "  —  no downline found under this member";
        }

        return chosen;
    }

    /// <summary>
    /// One pass of the tree, joining ParentID against the given column.
    ///
    /// MAXRECURSION is capped so a cycle — which a mis-set upline can create —
    /// errors instead of spinning the server.
    /// </summary>
    private static DownlineKeys WalkTree(cls_connection cls, int rootMemberId,
                                         bool includeSelf, string linkCol)
    {
        var keys = new DownlineKeys();

        try
        {
            string sql =
                ";with tree as ( " +
                "  select ID, msrno, ParentID, 0 as depth from member where ID = " + rootMemberId +
                "  union all " +
                "  select m.ID, m.msrno, m.ParentID, t.depth + 1 " +
                "  from member m join tree t on m.ParentID = t." + Q(linkCol) + " " +
                "  where t.depth < 12 " +
                ") " +
                "select distinct ID, msrno from tree " +
                (includeSelf ? "" : "where depth > 0 ") +
                "option (maxrecursion 100)";

            DataTable dt = cls.select_data_dt(sql);

            if (dt == null || dt.Rows.Count == 0) { return keys; }

            var msrnos = new StringBuilder();
            var ids = new StringBuilder();

            foreach (DataRow r in dt.Rows)
            {
                int id;
                if (int.TryParse(Convert.ToString(r["ID"]), out id) && id > 0)
                {
                    if (ids.Length > 0) { ids.Append(','); }
                    ids.Append(id);
                }

                int msrno;
                if (int.TryParse(Convert.ToString(r["msrno"]), out msrno) && msrno > 0)
                {
                    if (msrnos.Length > 0) { msrnos.Append(','); }
                    msrnos.Append(msrno);
                }
            }

            keys.MsrnoList = msrnos.ToString();
            keys.IdList = ids.ToString();
            keys.Count = dt.Rows.Count;

            return keys;
        }
        catch (Exception ex)
        {
            keys.LinkNote = "tree walk on " + linkCol + " failed: " + ex.Message;
            return keys;
        }
    }

    /// <summary>
    /// One member's own keys.
    /// </summary>
    public static DownlineKeys Single(cls_connection cls, int memberId)
    {
        var keys = new DownlineKeys();

        try
        {
            DataTable dt = cls.select_data_dt("select ID, msrno from member where ID = " + memberId);

            if (dt == null || dt.Rows.Count == 0) { return keys; }

            int id, msrno;
            if (int.TryParse(ReportHelper.Cell(dt.Rows[0], "ID"), out id) && id > 0)
            {
                keys.IdList = id.ToString();
            }
            if (int.TryParse(ReportHelper.Cell(dt.Rows[0], "msrno"), out msrno) && msrno > 0)
            {
                keys.MsrnoList = msrno.ToString();
            }

            keys.Count = 1;
            return keys;
        }
        catch
        {
            return keys;
        }
    }

    /// <summary>
    /// The key list that matches this table's own member column. A column named
    /// MemberID gets IDs; anything msrno-shaped gets msrnos.
    /// </summary>
    public static string KeyFor(ServiceTable st, DownlineKeys keys)
    {
        if (st == null || st.MemberCol == null || keys == null) { return ""; }

        string col = st.MemberCol.ToUpperInvariant();

        if (col.IndexOf("MSRNO", StringComparison.Ordinal) >= 0)
        {
            return keys.MsrnoList;
        }

        // MemberID, MemberId, Memberid — all mean member.ID.
        return keys.IdList;
    }

    /// <summary>
    /// The member column to join this table back to. A table keyed on msrno
    /// joins member.msrno; one keyed on MemberID joins member.ID. Without this
    /// the per-member grid would join an ID against msrno and return nothing.
    /// </summary>
    public static string JoinColFor(ServiceTable st)
    {
        return st.MemberCol.ToUpperInvariant().IndexOf("MSRNO", StringComparison.Ordinal) >= 0
            ? "msrno"
            : "ID";
    }

    /// <summary>Which key a table will be given, for the diagnostics panel.</summary>
    public static string KeyNameFor(ServiceTable st)
    {
        if (st == null || st.MemberCol == null) { return "?"; }

        return st.MemberCol.ToUpperInvariant().IndexOf("MSRNO", StringComparison.Ordinal) >= 0
            ? "member.msrno"
            : "member.ID";
    }

    // ---- aggregate ---------------------------------------------------------

    /// <summary>
    /// One row per service: transaction count, successful count and value,
    /// failed count, and the first and last time the member used it.
    ///
    /// Aggregated in SQL rather than fetched and counted in C#, because on a
    /// busy member these four tables together run to hundreds of thousands of
    /// rows and none of that detail reaches the screen.
    /// </summary>
    public static DataTable Summary(cls_connection cls, DownlineKeys keys,
                                    string fromDate, string toDate, out string warning)
    {
        warning = "";

        var usable = new List<ServiceTable>();
        var skipped = new List<string>();

        foreach (ServiceTable st in Tables(cls))
        {
            if (st.Usable) { usable.Add(st); }
            else { skipped.Add(st.Service + " — " + st.Problem); }
        }

        if (skipped.Count > 0)
        {
            warning = string.Join("  ", skipped.ToArray());
        }

        if (usable.Count == 0 || keys == null || keys.Empty) { return null; }

        var sb = new StringBuilder();

        for (int i = 0; i < usable.Count; i++)
        {
            ServiceTable st = usable[i];

            if (i > 0) { sb.Append(" union all "); }

            // Where the table has a type column the service is split by it, so a
            // cash withdrawal is reported apart from an Aadhaar Pay, and a bank
            // settlement apart from a wallet one. The label is built in SQL
            // rather than stitched on afterwards, so the grouping and the label
            // can never disagree.
            //
            // Family is the base service, carried alongside because the label no
            // longer always starts with it: AW is reported as "Aadhaar Pay", not
            // as "AEPS" plus a code. Matching a tile back to its own table by
            // reading the label would have broken exactly there.
            string label = LabelSql(st, "");

            sb.Append("select ").Append(label).Append(" as Service")
              .Append(", N'").Append(st.Service.Replace("'", "''")).Append("' as Family")
              .Append(", count(*) as Txns")
              .Append(", sum(case when ").Append(SuccessTest(st)).Append(" then 1 else 0 end) as SuccessTxns")
              .Append(", isnull(sum(case when ").Append(SuccessTest(st))
              .Append(" then cast(isnull(").Append(Q(st.AmountCol))
              .Append(",0) as decimal(18,2)) else 0 end),0) as SuccessValue")
              .Append(", sum(case when ").Append(FailedTest(st)).Append(" then 1 else 0 end) as FailedTxns")
              .Append(", sum(case when ").Append(PendingTest(st)).Append(" then 1 else 0 end) as PendingTxns")
              .Append(", min(").Append(Q(st.DateCol)).Append(") as FirstUsed")
              .Append(", max(").Append(Q(st.DateCol)).Append(") as LastUsed")
              .Append(" from ").Append(Q(st.Table))
              .Append(" where ").Append(Q(st.MemberCol)).Append(" in (").Append(KeyFor(st, keys)).Append(")")
              .Append(" and ").Append(Q(st.DateCol)).Append(" >= '").Append(Safe(fromDate)).Append("'")
              .Append(" and ").Append(Q(st.DateCol)).Append(" < dateadd(day, 1, '").Append(Safe(toDate)).Append("')");

            // Grouped by the whole label expression rather than by the raw code
            // underneath it. Two codes that map to the same name then land in one
            // row, and a group can never describe something its own label does not.
            if (st.TypeCol != null)
            {
                sb.Append(" group by ").Append(label);
            }
        }

        try
        {
            return cls.select_data_dt(sb.ToString());
        }
        catch (Exception ex)
        {
            warning = (warning.Length > 0 ? warning + "  " : "") +
                      "The summary query failed: " + ex.Message;
            return null;
        }
    }

    /// <summary>
    /// Per-member breakdown across the downline, so an admin can see which
    /// people in the tree are actually producing and which are dormant.
    /// </summary>
    public static DataTable ByMember(cls_connection cls, DownlineKeys keys,
                                     string fromDate, string toDate, int topN)
    {
        var usable = new List<ServiceTable>();

        foreach (ServiceTable st in Tables(cls))
        {
            if (st.Usable) { usable.Add(st); }
        }

        if (usable.Count == 0 || keys == null || keys.Empty) { return null; }

        var union = new StringBuilder();

        for (int i = 0; i < usable.Count; i++)
        {
            ServiceTable st = usable[i];

            if (i > 0) { union.Append(" union all "); }

            // Normalised to msrno at source, so the outer join is a plain
            // equality on one column whichever key this table stores.
            union.Append("select mm.msrno as msrno")
                 .Append(", '").Append(st.Service.Replace("'", "''")).Append("' as Service")
                 .Append(", case when ").Append(SuccessTestT(st))
                 .Append(" then cast(isnull(t.").Append(Q(st.AmountCol))
                 .Append(",0) as decimal(18,2)) else 0 end as Val")
                 .Append(", case when ").Append(SuccessTestT(st)).Append(" then 1 else 0 end as Ok")
                 .Append(" from ").Append(Q(st.Table)).Append(" t")
                 .Append(" join member mm on mm.").Append(JoinColFor(st))
                 .Append(" = t.").Append(Q(st.MemberCol))
                 .Append(" where t.").Append(Q(st.MemberCol)).Append(" in (").Append(KeyFor(st, keys)).Append(")")
                 .Append(" and t.").Append(Q(st.DateCol)).Append(" >= '").Append(Safe(fromDate)).Append("'")
                 .Append(" and t.").Append(Q(st.DateCol)).Append(" < dateadd(day, 1, '").Append(Safe(toDate)).Append("')");
        }

        string sql =
            "select top " + topN + " m.msrno, m.LoginID, m.Name, m.ShopName, " +
            "count(*) as Txns, sum(x.Ok) as SuccessTxns, sum(x.Val) as SuccessValue " +
            "from (" + union + ") x " +
            "join member m on m.msrno = x.msrno " +
            "group by m.msrno, m.LoginID, m.Name, m.ShopName " +
            "order by sum(x.Val) desc";

        try
        {
            return cls.select_data_dt(sql);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// One row per member per service, for the member x service grid.
    ///
    /// Returned long rather than pivoted, because the set of services is not
    /// fixed — AEPS contributes one entry per transaction type it actually saw,
    /// so the columns are only known once the data comes back. The page turns
    /// this into columns.
    ///
    /// Members with no activity at all are still returned, with zeros, via the
    /// left join from member. A distributor asking "who in my ten is not
    /// working" needs the silent ones listed, not omitted.
    /// </summary>
    public static DataTable ByMemberService(cls_connection cls, DownlineKeys keys,
                                            string fromDate, string toDate)
    {
        var usable = new List<ServiceTable>();

        foreach (ServiceTable st in Tables(cls))
        {
            if (st.Usable) { usable.Add(st); }
        }

        if (usable.Count == 0 || keys == null || keys.Empty) { return null; }

        var union = new StringBuilder();

        for (int i = 0; i < usable.Count; i++)
        {
            ServiceTable st = usable[i];

            if (i > 0) { union.Append(" union all "); }

            string label = LabelSql(st, "t.");

            // Normalised to msrno at source, so the outer join is a plain
            // equality on one column whichever key this table stores.
            union.Append("select mm.msrno as msrno")
                 .Append(", ").Append(label).Append(" as Service")
                 .Append(", case when ").Append(SuccessTestT(st))
                 .Append(" then cast(isnull(t.").Append(Q(st.AmountCol))
                 .Append(",0) as decimal(18,2)) else 0 end as Val")
                 .Append(", case when ").Append(SuccessTestT(st)).Append(" then 1 else 0 end as Ok")
                 .Append(", 1 as Any1")
                 .Append(" from ").Append(Q(st.Table)).Append(" t")
                 .Append(" join member mm on mm.").Append(JoinColFor(st))
                 .Append(" = t.").Append(Q(st.MemberCol))
                 .Append(" where t.").Append(Q(st.MemberCol)).Append(" in (").Append(KeyFor(st, keys)).Append(")")
                 .Append(" and t.").Append(Q(st.DateCol)).Append(" >= '").Append(Safe(fromDate)).Append("'")
                 .Append(" and t.").Append(Q(st.DateCol)).Append(" < dateadd(day, 1, '").Append(Safe(toDate)).Append("')");
        }

        // Left join from member so a member who did nothing still appears.
        string sql =
            "select m.msrno, m.LoginID, m.Name, m.ShopName, " +
            "isnull(x.Service, '') as Service, " +
            "isnull(sum(x.Val), 0) as SuccessValue, " +
            "isnull(sum(x.Ok), 0) as SuccessTxns, " +
            "isnull(sum(x.Any1), 0) as Txns " +
            "from member m " +
            "left join (" + union + ") x on x.msrno = m.msrno " +
            "where m.msrno in (" + keys.MsrnoList + ") " +
            "group by m.msrno, m.LoginID, m.Name, m.ShopName, isnull(x.Service, '') " +
            "order by m.LoginID";

        try
        {
            return cls.select_data_dt(sql);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// One row per service per API provider.
    ///
    /// Answers the question the per-member reports cannot: not "who is doing
    /// business" but "which provider is actually carrying it, and which one is
    /// failing". A service that looks healthy overall can be one provider at
    /// 99% hiding another at 40%, and the only place that shows is here.
    ///
    /// <paramref name="keys"/> may be null, which means the whole company
    /// rather than one member's tree — an API is a company-wide contract, so
    /// that is the normal way to read this.
    ///
    /// A table with no API column is NOT silently dropped and NOT folded into
    /// whichever provider sorts first. It comes back named in
    /// <paramref name="unmeasured"/> so the page can say so, because "no API
    /// breakdown for this service" and "all of it went through one API" look
    /// identical on a chart and mean opposite things.
    /// </summary>
    public static DataTable ByApi(cls_connection cls, DownlineKeys keys,
                                  string fromDate, string toDate,
                                  out string warning, out List<string> unmeasured)
    {
        warning = "";
        unmeasured = new List<string>();

        var usable = new List<ServiceTable>();
        var skipped = new List<string>();

        foreach (ServiceTable st in Tables(cls))
        {
            if (!st.Usable) { skipped.Add(st.Service + " — " + st.Problem); continue; }

            if (st.ApiCol == null)
            {
                unmeasured.Add(st.Service);
                continue;
            }

            usable.Add(st);
        }

        if (skipped.Count > 0) { warning = string.Join("  ", skipped.ToArray()); }

        if (usable.Count == 0) { return null; }

        // A null key set means every member. An empty one means a member whose
        // keys could not be worked out, which must return nothing rather than
        // quietly widening to the whole company.
        bool wholeCompany = keys == null;
        if (!wholeCompany && keys.Empty) { return null; }

        var sb = new StringBuilder();

        for (int i = 0; i < usable.Count; i++)
        {
            ServiceTable st = usable[i];

            if (i > 0) { sb.Append(" union all "); }

            string label = LabelSql(st, "t.");

            // The provider's name, or its bare id when tblRecharge_API has no
            // row for it. A blank cell would read as "no provider", when what
            // it really means is "a provider that is no longer configured" —
            // which is worth seeing.
            string apiName =
                "isnull(a.[APIName], N'API ' + ltrim(rtrim(cast(t." + Q(st.ApiCol) + " as nvarchar(20)))))";

            sb.Append("select ").Append(label).Append(" as Service")
              .Append(", N'").Append(st.Service.Replace("'", "''")).Append("' as Family")
              .Append(", cast(isnull(t.").Append(Q(st.ApiCol)).Append(",0) as int) as ApiId")
              .Append(", ").Append(apiName).Append(" as ApiName")
              .Append(", count(*) as Txns")
              .Append(", sum(case when ").Append(SuccessTestT(st)).Append(" then 1 else 0 end) as SuccessTxns")
              .Append(", isnull(sum(case when ").Append(SuccessTestT(st))
              .Append(" then cast(isnull(t.").Append(Q(st.AmountCol))
              .Append(",0) as decimal(18,2)) else 0 end),0) as SuccessValue")
              .Append(", sum(case when ").Append(FailedTestT(st)).Append(" then 1 else 0 end) as FailedTxns")
              .Append(", sum(case when ").Append(PendingTestT(st)).Append(" then 1 else 0 end) as PendingTxns")
              .Append(", max(t.").Append(Q(st.DateCol)).Append(") as LastUsed")
              .Append(" from ").Append(Q(st.Table)).Append(" t")
              .Append(" left join [tblRecharge_API] a on a.[APIID] = t.").Append(Q(st.ApiCol))
              .Append(" where t.").Append(Q(st.DateCol)).Append(" >= '").Append(Safe(fromDate)).Append("'")
              .Append(" and t.").Append(Q(st.DateCol)).Append(" < dateadd(day, 1, '").Append(Safe(toDate)).Append("')");

            if (!wholeCompany)
            {
                sb.Append(" and t.").Append(Q(st.MemberCol))
                  .Append(" in (").Append(KeyFor(st, keys)).Append(")");
            }

            // Grouped by the whole label expression rather than by the raw code
            // underneath it, so the group can never describe something its own
            // label does not.
            sb.Append(" group by ");

            if (st.TypeCol != null) { sb.Append(label).Append(", "); }

            sb.Append("cast(isnull(t.").Append(Q(st.ApiCol)).Append(",0) as int), ").Append(apiName);
        }

        try
        {
            return cls.select_data_dt(sb.ToString());
        }
        catch (Exception ex)
        {
            warning = (warning.Length > 0 ? warning + "  " : "") +
                      "The API query failed: " + ex.Message;
            return null;
        }
    }

    // ---- status tests ------------------------------------------------------
    //
    // Written once each. When the same expression is spelled out three times in
    // a query builder, one of the three eventually gets edited on its own.

    private static string Norm(ServiceTable st)
    {
        return "upper(ltrim(rtrim(cast(" + Q(st.StatusCol) + " as nvarchar(50)))))";
    }

    private static string SuccessTest(ServiceTable st)
    {
        return Norm(st) + " = 'SUCCESS'";
    }

    /// <summary>Same test, for the queries that alias the table as t.</summary>
    private static string SuccessTestT(ServiceTable st)
    {
        return NormT(st) + " = 'SUCCESS'";
    }

    /// <summary>The normalised status, for queries that alias the table as t.</summary>
    private static string NormT(ServiceTable st)
    {
        return "upper(ltrim(rtrim(cast(t." + Q(st.StatusCol) + " as nvarchar(50)))))";
    }

    private static string FailedTestT(ServiceTable st)
    {
        return NormT(st) + " in ('FAILED','FAILURE','REFUND','REFUNDED','REVERSED')";
    }

    private static string PendingTestT(ServiceTable st)
    {
        return NormT(st) + " in ('PENDING','QUEUED','PROCESSING','INITIATED')";
    }

    private static string FailedTest(ServiceTable st)
    {
        return Norm(st) + " in ('FAILED','FAILURE','REFUND','REFUNDED','REVERSED')";
    }

    private static string PendingTest(ServiceTable st)
    {
        return Norm(st) + " in ('PENDING','QUEUED','PROCESSING','INITIATED')";
    }

    // ---- helpers -----------------------------------------------------------

    /// <summary>
    /// Brackets an identifier that came from INFORMATION_SCHEMA. It is already
    /// a real column name from the database rather than user input, but a
    /// bracketed identifier also survives names with spaces or reserved words.
    /// </summary>
    private static string Q(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) { return "[]"; }
        return "[" + identifier.Replace("]", "]]") + "]";
    }

    private static string Safe(string value)
    {
        if (value == null) { return ""; }
        return value.Trim().Replace("'", "''");
    }

    /// <summary>
    /// A yyyy-MM-dd date, or the fallback when the text is not a date. Dates
    /// reach the query as literals, so they must be shaped here rather than
    /// trusted.
    /// </summary>
    public static string SafeDate(string raw, DateTime fallback)
    {
        DateTime d;

        if (DateTime.TryParseExact((raw ?? "").Trim(), "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out d))
        {
            return d.ToString("yyyy-MM-dd");
        }

        if (DateTime.TryParse((raw ?? "").Trim(),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out d))
        {
            return d.ToString("yyyy-MM-dd");
        }

        return fallback.ToString("yyyy-MM-dd");
    }
}
