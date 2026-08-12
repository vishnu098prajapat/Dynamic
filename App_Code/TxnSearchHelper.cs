using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;

/// <summary>
/// Finds one transaction across every service table from a single reference —
/// an order id, an RRN, a bank RRN, a customer mobile or an account number.
///
/// WHY THIS EXISTS
///
/// When a customer complains, the admin has exactly one string to go on and no
/// idea which service it belongs to. Today that means opening DMT history, then
/// AEPS history, then UPI, then recharge, guessing the date range in each. Four
/// screens to answer "did this go through". This asks all of them at once.
///
/// WHY IT DISCOVERS COLUMNS INSTEAD OF NAMING THEM
///
/// The reference columns are spelled differently on every table — OrderID and
/// RRN on MoneyTransfer, bankrrn and referenceno on Aeps_history, TransID on
/// tblRecharge_History — and most of them are only ever reached through a stored
/// procedure, so the C# never mentions them. A hard-coded list would compile,
/// run, and quietly search four columns instead of eleven. The operator would
/// see "not found" and conclude the transaction does not exist.
///
/// That is the failure this file is built around: a search that finds nothing
/// looks identical whether it looked everywhere or barely looked at all. So the
/// columns are read from INFORMATION_SCHEMA, and the result carries the list of
/// what was actually searched. "Not found" is only trustworthy next to the list
/// of places it was not found in.
///
/// WHY THE MATCH IS EXACT
///
/// A reference lookup is not a text search. LIKE '%12345%' across five
/// transaction tables is five table scans on a live database, and it returns
/// every unrelated row that happens to contain those digits — which is worse
/// than nothing, because the operator then reads a status off the wrong row.
/// Equality can use an index; a leading wildcard never can.
/// </summary>
public static class TxnSearchHelper
{
    /// <summary>
    /// One matched transaction, flattened to what the results list shows. The
    /// full row is fetched separately, only for the one the operator opens.
    /// </summary>
    public class Hit
    {
        // Properties rather than fields on purpose: a Repeater binds through
        // DataBinder.Eval, which reads properties only. Public fields bind to
        // nothing and the grid renders empty with no error at all.
        public string Service { get; set; }      // "AEPS · Aadhaar Pay"
        public string Family { get; set; }       // "AEPS"
        public string Table { get; set; }
        public string KeyCol { get; set; }       // how to fetch this row again
        public string KeyValue { get; set; }
        public string MatchedOn { get; set; }    // WHICH column the term hit
        public string Status { get; set; }
        public string ApiName { get; set; }
        public string LoginId { get; set; }
        public string MemberName { get; set; }
        public string Amount { get; set; }
        public string At { get; set; }

        /// <summary>What the detail view is opened with, as one safe token.</summary>
        public string Handle
        {
            get { return Table + "|" + KeyCol + "|" + KeyValue; }
        }
    }

    /// <summary>
    /// The outcome of a search, including what was looked at. The negative
    /// result is the important one, and it is only meaningful with its scope
    /// attached.
    /// </summary>
    public class Result
    {
        public string Term = "";
        public List<Hit> Hits = new List<Hit>();
        public List<string> Searched = new List<string>();   // "Money transfer: OrderID, RRN, CustomerMobile"
        public List<string> Skipped = new List<string>();
        public string Error = "";
        public int Columns;

        public bool Any { get { return Hits.Count > 0; } }
    }

    /// <summary>
    /// What makes a column a reference, matched on the STEM of its name rather
    /// than the whole thing.
    ///
    /// WHY THIS IS NOT A LIST OF COLUMN NAMES
    ///
    /// It was, and it failed in the field on the first day. A list of exact
    /// spellings has to be right about every table, and it is wrong silently:
    /// the column is simply not searched, the operator sees "not found", and
    /// nothing anywhere says a column was missed. Every install, every schema
    /// change and every new service is another chance to be quietly wrong.
    ///
    /// A stem cannot be wrong in that direction. "refer" catches referenceno,
    /// RefNo, ReferenceNumber and BankReference without any of them being known
    /// in advance. "order" catches OrderID, OrderNo and ApiOrderID. The cost is
    /// the opposite error — searching a column that is not a reference — and
    /// that one is harmless: an equality test against a column nobody would
    /// type finds nothing and costs one index probe.
    ///
    /// Order matters. The first stem to match names the label, so Aadhaar is
    /// listed before the generic "number" and a column called adhaarnumber is
    /// labelled and masked as Aadhaar rather than shown as a plain number.
    /// </summary>
    private static readonly string[][] Stems =
    {
        new string[] { "ORDER",   "Order ID" },
        new string[] { "RRN",     "RRN" },
        new string[] { "UTR",     "UTR" },
        new string[] { "REFER",   "Reference" },
        new string[] { "REFNO",   "Reference" },
        new string[] { "REFID",   "Reference" },

        // Aadhaar first, so its own spellings never fall through to "NUMBER".
        new string[] { "ADHAAR",  "Aadhaar" },
        new string[] { "AADHAAR", "Aadhaar" },
        new string[] { "AADHAR",  "Aadhaar" },
        new string[] { "ADHAR",   "Aadhaar" },

        new string[] { "TXN",     "Transaction ID" },
        new string[] { "TRANS",   "Transaction ID" },
        new string[] { "MOBILE",  "Mobile" },
        new string[] { "ACCOUNT", "Account no" },
        new string[] { "ACNO",    "Account no" },
        new string[] { "VPA",     "UPI ID" },

        // Anchored to the start of the name, unlike every other stem. "UPI"
        // sits inside GroupID — GRO-UPI-D — and an unanchored stem would label
        // a group key as a UPI address. A caret means "only at the beginning".
        new string[] { "^UPI",    "UPI ID" },
        new string[] { "RECEIPT", "Receipt" },
        new string[] { "NUMBER",  "Number" }
    };

    /// <summary>
    /// Word fragments that disqualify a column however well its stem matched.
    ///
    /// This is what keeps the stems from over-reaching. "TRANS" alone would
    /// take TransDate, TransAmount, TransStatus and TransType — four columns
    /// that are not references and, worse, three of which hold values an
    /// operator might plausibly type. Matching a status or an amount by
    /// accident would return a pile of unrelated transactions and look like a
    /// working search, which is the failure this whole page is built to avoid.
    ///
    /// Checked BEFORE the stems, so a name has to survive this list first.
    /// </summary>
    private static readonly string[] NeverSearch =
    {
        "DATE", "TIME", "STATUS", "AMOUNT", "AMT", "BALANCE", "OPENING", "CLOSING",
        "COMMISSION", "SURCHARGE", "GST", "TDS", "TYPE", "MODE",
        "NAME", "ADDRESS", "PINCODE", "DESCRIPTION", "MESSAGE", "REMARK", "REASON",
        "DATA", "RESPONSE", "REQUEST", "CALLBACK", "PASSWORD", "TOKEN", "SECRET",
        "LATITUDE", "LONGITUDE", "ISACTIVE", "ISDELETE", "FLAG"

        // Two obvious-looking entries are deliberately absent, both because a
        // fragment is inside a longer word that means something else:
        //
        //   the tally word  is inside AccountNO
        //   the fee word    is inside RechargeNumber
        //
        // Either one silently removes a column an operator searches by every
        // day. Neither is needed: a fee or a tally column is decimal or has no
        // reference stem, so it is already excluded twice over. They are named
        // here in prose rather than in the array on purpose.
    };

    /// <summary>
    /// SQL types a reference can live in.
    ///
    /// Text and whole numbers only. Excluding decimal, money and float is not
    /// tidiness — those are amounts and balances, and an operator searching for
    /// "1500" would otherwise match every transaction of that value across
    /// every service.
    /// </summary>
    private static readonly string[] SearchableTypes =
        { "varchar", "nvarchar", "char", "nchar", "int", "bigint", "smallint" };

    /// <summary>
    /// Columns that must never be printed whole, whatever the operator typed.
    ///
    /// An admin investigating a complaint needs to confirm an account number, so
    /// that stays readable. An Aadhaar number is different: it is regulated, it
    /// is not needed to resolve a transaction, and a support screen is exactly
    /// where it leaks from. It is matchable and shown as the last four digits.
    /// </summary>
    private static readonly string[] MaskAlways =
        { "adhaarnumber", "AadharNumber", "AadhaarNo", "AadhaarNumber", "Aadhar", "Aadhaar" };

    /// <summary>
    /// Columns holding raw API request and response payloads. Kept out of the
    /// summary grid and shown separately, collapsed: they answer "what did the
    /// provider actually say", which is the whole reason an admin opens one of
    /// these, but a 4KB JSON blob in a table cell hides every other field.
    /// </summary>
    private static readonly string[] PayloadCols =
        { "RequestData", "ResponseTData", "ResponseData", "CallBackData", "BeneficiaryData", "Response" };

    private const string CacheKey = "TxnSearchHelper_Fields_v2";

    /// <summary>The searchable columns a table really has, in stem order.</summary>
    private class Fields
    {
        public string Table = "";
        public List<string> Cols = new List<string>();
        public List<string> Labels = new List<string>();
        public string KeyCol;    // uniquely addresses one row, for the detail view

        /// <summary>
        /// Every column on the table with the reason it was or was not
        /// searched. Nobody reads this until a search fails, and then it is the
        /// only thing worth reading.
        /// </summary>
        public List<string[]> Audit = new List<string[]>();
    }

    // ---- discovery ---------------------------------------------------------

    private static Dictionary<string, Fields> Discover(cls_connection cls)
    {
        HttpApplicationState app = HttpContext.Current == null ? null : HttpContext.Current.Application;

        if (app != null)
        {
            var cached = app[CacheKey] as Dictionary<string, Fields>;
            if (cached != null) { return cached; }
        }

        var map = new Dictionary<string, Fields>(StringComparer.OrdinalIgnoreCase);

        foreach (ServiceUsageHelper.ServiceTable st in ServiceUsageHelper.Tables(cls))
        {
            if (!st.Usable) { continue; }

            var f = new Fields();
            f.Table = st.Table;

            try
            {
                DataTable cols = cls.select_data_dt(
                    "select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH " +
                    "from INFORMATION_SCHEMA.COLUMNS " +
                    "where TABLE_NAME = '" + st.Table.Replace("'", "''") + "'");

                if (cols == null || cols.Rows.Count == 0) { continue; }

                bool hasId = false;

                // Structural columns. Already known from the schema mapping and
                // never a reference, so they are excluded by identity rather
                // than by name — APIID would otherwise survive every filter and
                // make "3" match a third of the table.
                var structural = new List<string>();
                Add(structural, st.MemberCol);
                Add(structural, st.AmountCol);
                Add(structural, st.StatusCol);
                Add(structural, st.DateCol);
                Add(structural, st.TypeCol);
                Add(structural, st.ApiCol);

                foreach (DataRow r in cols.Rows)
                {
                    string name = Convert.ToString(r["COLUMN_NAME"]);
                    string type = Convert.ToString(r["DATA_TYPE"]).ToLowerInvariant();
                    string upper = name.ToUpperInvariant();

                    if (string.Equals(name, "ID", StringComparison.OrdinalIgnoreCase)) { hasId = true; }

                    // A reference is short. Excluding the wide text columns keeps
                    // the payload blobs out of the WHERE clause, where they would
                    // turn one indexed lookup into a scan of every stored API
                    // response on the table.
                    int len;
                    bool wide = int.TryParse(Convert.ToString(r["CHARACTER_MAXIMUM_LENGTH"]), out len) &&
                                (len < 0 || len > 200);

                    if (!Contains(SearchableTypes, type))
                    {
                        f.Audit.Add(new string[] { name, type, "no", "not a text or whole-number column" });
                        continue;
                    }

                    if (wide)
                    {
                        f.Audit.Add(new string[] { name, type, "no", "too wide to be a reference" });
                        continue;
                    }

                    if (Contains(structural, name))
                    {
                        f.Audit.Add(new string[] { name, type, "no", "the table's own member / amount / status / date / type / API column" });
                        continue;
                    }

                    string banned = Fragment(upper, NeverSearch);

                    if (banned != null)
                    {
                        f.Audit.Add(new string[] { name, type, "no", "name contains " + banned });
                        continue;
                    }

                    string label = null;

                    foreach (string[] s in Stems)
                    {
                        bool hit = s[0][0] == '^'
                            ? upper.StartsWith(s[0].Substring(1), StringComparison.Ordinal)
                            : upper.IndexOf(s[0], StringComparison.Ordinal) >= 0;

                        if (hit) { label = s[1]; break; }
                    }

                    if (label == null)
                    {
                        f.Audit.Add(new string[] { name, type, "no", "does not look like a reference" });
                        continue;
                    }

                    f.Cols.Add(name);
                    f.Labels.Add(label);
                    f.Audit.Add(new string[] { name, type, "yes", label });
                }

                // The detail view needs to fetch this exact row back. A plain ID
                // is the only column that can be trusted to identify one — an RRN
                // can repeat across a reversal, and opening the wrong row is how
                // an admin reads a success off a failed transaction.
                f.KeyCol = hasId ? "ID" : st.IdCol;
            }
            catch
            {
                // A table whose schema cannot be read is reported as skipped by
                // the caller rather than silently searched as if it were empty.
            }

            map[st.Table] = f;
        }

        if (app != null) { app[CacheKey] = map; }

        return map;
    }

    private static void Add(List<string> list, string value)
    {
        if (!string.IsNullOrEmpty(value) && !list.Contains(value)) { list.Add(value); }
    }

    /// <summary>The first banned fragment inside a name, or null.</summary>
    private static string Fragment(string upperName, string[] fragments)
    {
        foreach (string s in fragments)
        {
            if (upperName.IndexOf(s, StringComparison.Ordinal) >= 0) { return s; }
        }
        return null;
    }

    /// <summary>
    /// Every column on every service table, with whether it is searched and
    /// why not.
    ///
    /// Exists so that a failed search has somewhere to go. Without it the only
    /// way to find out why a reference was not matched is to open the database,
    /// which the person hitting the problem usually cannot do — so the report
    /// comes back as "search is broken" and the actual column name never
    /// surfaces. Rows are { table, column, type, searched, note }.
    /// </summary>
    public static List<string[]> Columns(cls_connection cls)
    {
        var rows = new List<string[]>();

        foreach (ServiceUsageHelper.ServiceTable st in ServiceUsageHelper.Tables(cls))
        {
            Fields f;
            if (!Discover(cls).TryGetValue(st.Table, out f)) { continue; }

            foreach (string[] a in f.Audit)
            {
                rows.Add(new string[] { st.Table, a[0], a[1], a[2], a[3] });
            }
        }

        return rows;
    }

    /// <summary>Forgets the discovered columns, for use after a schema change.</summary>
    public static void ClearCache()
    {
        if (HttpContext.Current != null && HttpContext.Current.Application != null)
        {
            HttpContext.Current.Application.Remove(CacheKey);
        }
    }

    // ---- search ------------------------------------------------------------

    /// <summary>
    /// Every transaction whose reference equals the given term, across all
    /// services, newest first.
    /// </summary>
    public static Result Search(cls_connection cls, string rawTerm, int max)
    {
        return Search(cls, rawTerm, max, false);
    }

    /// <summary>
    /// The same search, optionally matching anywhere inside a column.
    ///
    /// <paramref name="contains"/> is off by default and has to be asked for.
    /// It cannot use an index, so it reads every row of five transaction
    /// tables, and it will return rows that merely share a run of digits. That
    /// is a bad default and a reasonable choice for an operator who has half a
    /// reference and no other way forward — so it is offered, with the cost
    /// stated on screen, rather than being decided for them.
    /// </summary>
    public static Result Search(cls_connection cls, string rawTerm, int max, bool contains)
    {
        var res = new Result();
        res.Term = (rawTerm ?? "").Trim();

        if (res.Term.Length < 4)
        {
            res.Error = "Type at least 4 characters of an order ID, RRN, mobile or account number.";
            return res;
        }

        if (max <= 0 || max > 200) { max = 50; }

        Dictionary<string, Fields> fields = Discover(cls);
        string term = res.Term.ToUpperInvariant().Replace("'", "''");

        var union = new StringBuilder();
        int used = 0;

        foreach (ServiceUsageHelper.ServiceTable st in ServiceUsageHelper.Tables(cls))
        {
            if (!st.Usable)
            {
                res.Skipped.Add(st.Service + " \u2014 " + st.Problem);
                continue;
            }

            Fields f;
            if (!fields.TryGetValue(st.Table, out f) || f.Cols.Count == 0)
            {
                res.Skipped.Add(st.Service + " \u2014 no reference column on " + st.Table + " that this page knows how to search.");
                continue;
            }

            if (f.KeyCol == null)
            {
                res.Skipped.Add(st.Service + " \u2014 " + st.Table + " has no column that identifies a single row, so a result could not be opened.");
                continue;
            }

            var searched = new StringBuilder();
            var where = new StringBuilder();
            var matched = new StringBuilder("case");

            for (int i = 0; i < f.Cols.Count; i++)
            {
                string test = Test(f.Cols[i], term, contains);

                if (where.Length > 0) { where.Append(" or "); }
                where.Append(test);

                matched.Append(" when ").Append(test)
                       .Append(" then N'").Append(f.Labels[i].Replace("'", "''"))
                       .Append(" (").Append(f.Cols[i].Replace("'", "''")).Append(")'");

                if (searched.Length > 0) { searched.Append(", "); }
                searched.Append(f.Cols[i]);
                res.Columns++;
            }

            matched.Append(" else N'' end");

            res.Searched.Add(st.Service + ": " + searched);

            // The provider name, resolved here rather than in the page. A row
            // whose APIID no longer has a tblRecharge_API entry shows the bare id
            // instead of a blank — "provider 7, which is no longer configured" is
            // a finding; an empty cell reads as "no provider" and is not.
            string api = st.ApiCol == null
                ? "N''"
                : "isnull(a.[APIName], N'API ' + ltrim(rtrim(cast(t." + Q(st.ApiCol) + " as nvarchar(20)))))";

            if (used > 0) { union.Append(" union all "); }
            used++;

            union.Append("select ").Append(ServiceUsageHelper.LabelSql(st, "t.")).Append(" as Svc")
                 .Append(", N'").Append(st.Service.Replace("'", "''")).Append("' as Family")
                 .Append(", N'").Append(st.Table.Replace("'", "''")).Append("' as Tbl")
                 .Append(", N'").Append(f.KeyCol.Replace("'", "''")).Append("' as KeyCol")
                 .Append(", cast(t.").Append(Q(f.KeyCol)).Append(" as nvarchar(80)) as KeyVal")
                 .Append(", ").Append(matched).Append(" as MatchedOn")
                 .Append(", t.").Append(Q(st.DateCol)).Append(" as At")
                 .Append(", cast(isnull(t.").Append(Q(st.AmountCol)).Append(",0) as decimal(18,2)) as Amt")
                 .Append(", cast(isnull(t.").Append(Q(st.StatusCol)).Append(", '') as nvarchar(60)) as Sts")
                 .Append(", ").Append(api).Append(" as ApiName")
                 .Append(", t.").Append(Q(st.MemberCol)).Append(" as MemKey")
                 .Append(", N'").Append(ServiceUsageHelper.JoinColFor(st)).Append("' as MemKind")
                 .Append(" from ").Append(Q(st.Table)).Append(" t");

            if (st.ApiCol != null)
            {
                union.Append(" left join [tblRecharge_API] a on a.[APIID] = t.").Append(Q(st.ApiCol));
            }

            union.Append(" where ").Append(where);
        }

        if (used == 0)
        {
            res.Error = "No service table could be searched. See the notes below.";
            return res;
        }

        // The member is joined once, outside the union, on whichever key each
        // table stores. tblRecharge_History keys on member.ID while the others
        // key on msrno, and handing a table the wrong one returns nothing at
        // all — silently — which is how a whole service disappears from a search.
        string sql =
            "select top " + max + " x.*, isnull(mm.[LoginID],'') as LoginID_, isnull(mm.[Name],'') as MemberName_ " +
            "from (" + union + ") x " +
            "left join [member] mm " +
            "  on (x.MemKind = 'msrno' and mm.[msrno] = x.MemKey) " +
            "  or (x.MemKind = 'ID'    and mm.[ID]    = x.MemKey) " +
            "order by x.At desc";

        DataTable dt;

        try
        {
            dt = cls.select_data_dt(sql);
        }
        catch (Exception ex)
        {
            res.Error = "The search query failed: " + ex.Message;
            return res;
        }

        if (dt == null) { return res; }

        foreach (DataRow r in dt.Rows)
        {
            var h = new Hit();
            h.Service = ReportHelper.Cell(r, "Svc");
            h.Family = ReportHelper.Cell(r, "Family");
            h.Table = ReportHelper.Cell(r, "Tbl");
            h.KeyCol = ReportHelper.Cell(r, "KeyCol");
            h.KeyValue = ReportHelper.Cell(r, "KeyVal");
            h.MatchedOn = ReportHelper.Cell(r, "MatchedOn");
            h.Status = ReportHelper.Cell(r, "Sts");
            h.ApiName = ReportHelper.Cell(r, "ApiName");
            h.LoginId = ReportHelper.Cell(r, "LoginID_");
            h.MemberName = ReportHelper.Cell(r, "MemberName_");
            h.Amount = ReportHelper.Cell(r, "Amt");
            h.At = ReportHelper.Cell(r, "At");

            res.Hits.Add(h);
        }

        return res;
    }

    /// <summary>
    /// The comparison for one column. Trimmed, upper-cased and cast to text on
    /// both sides so a bigint order id and a padded varchar RRN both match what
    /// the operator typed.
    /// </summary>
    private static string Test(string col, string term, bool contains)
    {
        string lhs = "upper(ltrim(rtrim(cast(t." + Q(col) + " as nvarchar(100)))))";

        // The wildcards are escaped so a reference containing % or _ — which a
        // UPI id legitimately can — matches itself instead of matching most of
        // the table.
        return contains
            ? lhs + " like N'%" + Escape(term) + "%' escape '\\'"
            : lhs + " = N'" + term + "'";
    }

    /// <summary>Neutralises LIKE wildcards inside a term the operator typed.</summary>
    private static string Escape(string term)
    {
        return term.Replace("\\", "\\\\").Replace("%", "\\%")
                   .Replace("_", "\\_").Replace("[", "\\[");
    }

    // ---- one transaction ---------------------------------------------------

    /// <summary>
    /// Everything stored about one transaction, for the detail panel.
    /// </summary>
    public class Detail
    {
        public bool Found;
        public string Error = "";
        public string Service = "";
        public string Table = "";
        public string ApiName = "";
        public string Status = "";
        public string LoginId = "";
        public string MemberName = "";
        public string MemberShop = "";
        public string MemberMobile = "";

        /// <summary>Ordinary fields, in the order the table declares them.</summary>
        public List<string[]> Fields = new List<string[]>();

        /// <summary>Raw provider payloads, shown collapsed.</summary>
        public List<string[]> Payloads = new List<string[]>();
    }

    /// <summary>
    /// One row, fetched by the key the search handed back.
    ///
    /// The table name is checked against the discovered set rather than trusted.
    /// It arrives in a query string, and a page that concatenates a query string
    /// into a FROM clause is a page that reads any table in the database.
    /// </summary>
    public static Detail Open(cls_connection cls, string table, string keyCol, string keyValue)
    {
        var d = new Detail();

        ServiceUsageHelper.ServiceTable st = null;

        foreach (ServiceUsageHelper.ServiceTable s in ServiceUsageHelper.Tables(cls))
        {
            if (string.Equals(s.Table, table, StringComparison.OrdinalIgnoreCase) && s.Usable) { st = s; break; }
        }

        if (st == null)
        {
            d.Error = "That is not a service table this page can open.";
            return d;
        }

        Dictionary<string, Fields> fields = Discover(cls);
        Fields f;

        if (!fields.TryGetValue(st.Table, out f) || f.KeyCol == null ||
            !string.Equals(f.KeyCol, keyCol, StringComparison.OrdinalIgnoreCase))
        {
            d.Error = "That row cannot be addressed on " + st.Table + ".";
            return d;
        }

        d.Table = st.Table;

        try
        {
            string sql =
                "select top 1 * from " + Q(st.Table) + " t " +
                "where cast(t." + Q(f.KeyCol) + " as nvarchar(80)) = N'" +
                (keyValue ?? "").Trim().Replace("'", "''") + "'";

            DataTable dt = cls.select_data_dt(sql);

            if (dt == null || dt.Rows.Count == 0)
            {
                d.Error = "That transaction is no longer on " + st.Table + ".";
                return d;
            }

            DataRow r = dt.Rows[0];
            d.Found = true;

            d.Status = ReportHelper.Cell(r, st.StatusCol);
            d.Service = st.Service;

            if (st.TypeCol != null)
            {
                string code = ReportHelper.Cell(r, st.TypeCol);
                if (code.Length > 0) { d.Service = st.Service + " \u00b7 " + code; }
            }

            d.ApiName = ApiName(cls, st, r);
            Member(cls, st, r, d);

            foreach (DataColumn c in dt.Columns)
            {
                string name = c.ColumnName;
                string value = ReportHelper.Cell(r, name);

                if (Contains(PayloadCols, name))
                {
                    if (value.Length > 0) { d.Payloads.Add(new string[] { name, value }); }
                    continue;
                }

                if (Contains(MaskAlways, name)) { value = ReportHelper.MaskTail(value, 4); }

                d.Fields.Add(new string[] { name, value });
            }
        }
        catch (Exception ex)
        {
            d.Error = "Could not open that transaction: " + ex.Message;
        }

        return d;
    }

    /// <summary>The provider this row went through, by name where one is configured.</summary>
    private static string ApiName(cls_connection cls, ServiceUsageHelper.ServiceTable st, DataRow r)
    {
        if (st.ApiCol == null) { return "not recorded on this service"; }

        string id = ReportHelper.Cell(r, st.ApiCol);
        if (id.Length == 0) { return "not recorded on this transaction"; }

        int n;
        if (!int.TryParse(id, out n)) { return id; }

        try
        {
            DataTable dt = cls.select_data_dt(
                "select top 1 [APIName] from [tblRecharge_API] where [APIID] = " + n);

            if (dt != null && dt.Rows.Count > 0)
            {
                string name = ReportHelper.Cell(dt.Rows[0], "APIName");
                if (name.Length > 0) { return name + "  (API " + n + ")"; }
            }
        }
        catch
        {
        }

        // Naming the gap rather than showing a blank. An id with no row in
        // tblRecharge_API means a provider that was removed, and that is exactly
        // the sort of thing an admin chasing a stuck transaction wants to see.
        return "API " + n + " \u2014 no longer configured";
    }

    /// <summary>The member who put this transaction through.</summary>
    private static void Member(cls_connection cls, ServiceUsageHelper.ServiceTable st, DataRow r, Detail d)
    {
        string key = ReportHelper.Cell(r, st.MemberCol);
        if (key.Length == 0) { return; }

        int n;
        if (!int.TryParse(key, out n) || n <= 0) { return; }

        try
        {
            DataTable dt = cls.select_data_dt(
                "select top 1 LoginID, Name, ShopName, Mobile from [member] where " +
                (ServiceUsageHelper.JoinColFor(st) == "msrno" ? "[msrno] = " : "[ID] = ") + n);

            if (dt == null || dt.Rows.Count == 0) { return; }

            d.LoginId = ReportHelper.Cell(dt.Rows[0], "LoginID");
            d.MemberName = ReportHelper.Cell(dt.Rows[0], "Name");
            d.MemberShop = ReportHelper.Cell(dt.Rows[0], "ShopName");
            d.MemberMobile = ReportHelper.Cell(dt.Rows[0], "Mobile");
        }
        catch
        {
        }
    }

    // ---- helpers -----------------------------------------------------------

    /// <summary>
    /// Case-insensitive membership.
    ///
    /// Takes IList rather than string[] so the fixed arrays above and the
    /// per-table List built during discovery both go through one method. Two
    /// overloads would be two places for the comparison rule to drift, and the
    /// rule here — ignore case — is the whole point: SQL Server hands back
    /// AccountNO where the code says AccountNo.
    /// </summary>
    private static bool Contains(IList<string> list, string name)
    {
        foreach (string s in list)
        {
            if (string.Equals(s, name, StringComparison.OrdinalIgnoreCase)) { return true; }
        }
        return false;
    }

    private static string Q(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) { return "[]"; }
        return "[" + identifier.Replace("]", "]]") + "]";
    }
}
