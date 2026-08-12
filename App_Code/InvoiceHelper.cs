using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;

/// <summary>
/// Monthly GST figures per member, built from the same five transaction tables
/// the business analysis reports use.
///
/// WHAT THIS PRODUCES AND WHY IT IS SPLIT IN TWO
///
/// Under GST a supply to a registered customer and a supply to an unregistered
/// one are reported differently:
///
///   B2B  — the customer has a GSTIN. Every invoice is reported individually,
///          because the customer claims input credit against it.
///   B2C  — the customer has no GSTIN. Retailers usually fall here. These are
///          NOT reported invoice by invoice; they are consolidated by place of
///          supply and tax rate.
///
/// So this returns one set of rows and the page decides which side each member
/// falls on, from whether a GSTIN is present. That single field changes how the
/// money is reported, which is why it is discovered from the database rather
/// than assumed.
///
/// WHERE EVERY FIGURE COMES FROM
///
/// Nothing here is invented. The panel already stores all of it:
///
///   supplier GSTIN, PAN, state   GSTSetting      (Admin -> Manage GST setting)
///   member GSTIN                 GSTDetail       (Member -> GST details)
///   member state                 member.Stateid -> State.Name
///   charge, GST, TDS             the five transaction tables
///
/// The member's GSTIN decides B2B or B2C; their state decides the place of
/// supply; the company's state decides whether the tax is CGST+SGST or IGST.
/// Guessing any of the three produces a filing that is confidently wrong, so
/// each is read from the database and reported as missing when it is absent.
/// </summary>
public static class InvoiceHelper
{
    // ---- supplier: the company's own registration --------------------------

    /// <summary>
    /// The company's GST registration, read from the GSTSetting table that
    /// Admin -> Manage GST setting already maintains.
    ///
    /// This started out as placeholder constants. It should not have: the panel
    /// already had a screen for entering exactly these details, and inventing a
    /// second home for them would have meant an invoice that disagreed with the
    /// company's own settings page. Read from where the operator types it.
    /// </summary>
    public class Supplier
    {
        public string Name = "";
        public string OwnerName = "";
        public string Gstin = "";
        public string Pan = "";
        public string Cin = "";
        public string Address = "";
        public string StateName = "";
        public string StateCode = "";
        public int StateId;

        public string Problem = "";

        /// <summary>An invoice cannot be raised without a GSTIN on it.</summary>
        public bool CanInvoice { get { return Gstin.Length > 0; } }
    }

    private const string SupplierCacheKey = "InvoiceHelper_Supplier_v1";

    /// <summary>
    /// The company registration to invoice from.
    ///
    /// GSTSetting can hold more than one row; the most recent active one wins,
    /// which matches what the settings screen shows at the top. Cached for the
    /// request rather than the application, because an operator who has just
    /// corrected the GSTIN should see it on the next page, not after a restart.
    /// </summary>
    public static Supplier GetSupplier(cls_connection cls)
    {
        if (HttpContext.Current != null)
        {
            var cached = HttpContext.Current.Items[SupplierCacheKey] as Supplier;
            if (cached != null) { return cached; }
        }

        var s = new Supplier();

        try
        {
            DataTable dt = cls.select_data_dt(
                "select top 1 g.*, st.[Name] as StateName_, st.[StateCode] as StateCode_ " +
                "from [View_GSTSetting] g " +
                "left join [State] st on st.[ID] = g.[StateID] " +
                "order by g.[ID] desc");

            if (dt == null || dt.Rows.Count == 0)
            {
                s.Problem = "No company GST registration has been entered yet. " +
                            "Add one under Admin -> Manage GST setting; until then an invoice has no GSTIN to carry.";
            }
            else
            {
                DataRow r = dt.Rows[0];

                s.Name = Cell(r, "Name");
                s.OwnerName = Cell(r, "OwnerName");
                s.Gstin = Cell(r, "GSTNo");
                s.Pan = Cell(r, "Pan");
                s.Cin = Cell(r, "CIN");
                s.Address = Cell(r, "Address");
                s.StateName = Cell(r, "StateName_");
                s.StateCode = Cell(r, "StateCode_");

                int id;
                if (int.TryParse(Cell(r, "StateID"), out id)) { s.StateId = id; }

                if (s.Gstin.Length == 0)
                {
                    s.Problem = "The company GST registration has no GST number on it.";
                }
                else if (s.StateId <= 0)
                {
                    // Without the company's own state there is no way to tell an
                    // intra-state supply from an inter-state one, and the two are
                    // filed under different heads for the same total.
                    s.Problem = "The company GST registration has no state set, so CGST/SGST cannot be told apart from IGST.";
                }
            }
        }
        catch (Exception ex)
        {
            s.Problem = "Could not read the company GST registration: " + ex.Message;
        }

        if (HttpContext.Current != null) { HttpContext.Current.Items[SupplierCacheKey] = s; }

        return s;
    }

    private static string Cell(DataRow r, string column)
    {
        if (r == null || !r.Table.Columns.Contains(column)) { return ""; }

        object v = r[column];
        if (v == null || v == DBNull.Value) { return ""; }

        return Convert.ToString(v).Trim();
    }

    /// <summary>
    /// Values that are still ours to choose rather than the company's to enter.
    /// </summary>
    public static class InvoiceSettings
    {
        /// <summary>
        /// SAC for the service being billed. 997158 covers financial
        /// transaction processing and clearing house services. It is a
        /// reasonable default rather than a verified one, and should be
        /// confirmed against what is actually filed.
        /// </summary>
        public const string DefaultSac = "997158";

        public const decimal GstRate = 18m;
    }

    // ---- schema discovery --------------------------------------------------

    /// <summary>
    /// The column carrying the tax the member was charged.
    ///
    /// Read from the table rather than recomputed. These columns are written at
    /// the moment of the transaction and are what the member's wallet was
    /// actually debited by — an invoice must report what was charged, not what
    /// a formula says should have been. If the two ever disagree, the invoice
    /// agreeing with the ledger is the one that survives a query.
    /// </summary>
    private static readonly string[] GstCandidates =
        { "GST", "Gst", "gst", "GSTAmount", "TaxAmount" };

    /// <summary>The charge the GST sits on — the taxable value.</summary>
    private static readonly string[] TaxableCandidates =
        { "Surcharge", "SurCharge", "surcharge", "Charge", "charges", "ServiceCharge" };

    private static readonly string[] TdsCandidates =
        { "TDS", "Tds", "tds", "TDSAmount" };

    /// <summary>
    /// The member's own GSTIN. Its presence is what makes a member B2B.
    /// </summary>
    private static readonly string[] MemberGstinCandidates =
        { "GSTIN", "GstIn", "Gstin", "GSTNo", "GSTNumber", "GstNo" };

    /// <summary>
    /// The member's state — the place of supply a B2C sale is grouped under.
    /// </summary>
    private static readonly string[] MemberStateCandidates =
        { "StateName", "Statename" };

    /// <summary>
    /// The member's state as an ID. LoginService already does exactly this
    /// join — "(select Name from State where id=member.Stateid)" — so the
    /// relationship is not a guess.
    /// </summary>
    private static readonly string[] MemberStateIdCandidates =
        { "Stateid", "StateID", "StateId", "State_ID" };

    private const string CacheKey = "InvoiceHelper_Schema_v1";

    public class TaxColumns
    {
        public string Table;
        public string Service;
        public string MemberCol;
        public string DateCol;
        public string StatusCol;
        public string GstCol;
        public string TaxableCol;
        public string TdsCol;

        /// <summary>Nothing to bill from this table.</summary>
        public bool HasTax { get { return GstCol != null || TaxableCol != null; } }
    }

    /// <summary>
    /// Where the member's own GSTIN and state live, if they live anywhere.
    ///
    /// Reported rather than assumed. Without a GSTIN column every member reads
    /// as B2C, and without a state every B2C sale would have to be consolidated
    /// under one made-up place of supply — which is a filing error, not a
    /// display one. The page says so instead.
    /// </summary>
    public class MemberTaxFields
    {
        public string GstinCol;      // a GSTIN column on member, if there is one
        public string StateCol;      // a state NAME on member, if there is one
        public string StateIdCol;    // member.Stateid -> State.ID
        public bool HasGstDetail;    // the GSTDetail table the member fills in
        public string Problem = "";

        /// <summary>A GSTIN can come from GSTDetail or from member itself.</summary>
        public bool CanSplitB2B { get { return HasGstDetail || GstinCol != null; } }

        /// <summary>A place of supply can come from the join or from a name column.</summary>
        public bool CanGroupByState { get { return StateIdCol != null || StateCol != null; } }
    }

    public static MemberTaxFields MemberFields(cls_connection cls)
    {
        var f = new MemberTaxFields();

        try
        {
            List<string> cols = Columns(cls, "member");

            // The member's state is an ID into the State table, not a name on
            // the member row. Joining is the only way to get a place of supply
            // that matches what the operator picked on the registration screen.
            f.StateIdCol = FirstPresent(cols, MemberStateIdCandidates);
            f.StateCol = FirstPresent(cols, MemberStateCandidates);

            // A GSTIN lives in GSTDetail, entered by the member themselves. A
            // column on member is only a fallback for installs that kept it
            // there.
            f.GstinCol = FirstPresent(cols, MemberGstinCandidates);
            f.HasGstDetail = TableExists(cls, "GSTDetail");

            var missing = new List<string>();
            if (!f.CanSplitB2B) { missing.Add("GSTIN"); }
            if (!f.CanGroupByState) { missing.Add("state"); }

            if (missing.Count > 0)
            {
                f.Problem = "There is no " + string.Join(" and no ", missing.ToArray()) +
                            " to read for members.";
            }
        }
        catch (Exception ex)
        {
            f.Problem = "Could not read the member table: " + ex.Message;
        }

        return f;
    }

    private static bool TableExists(cls_connection cls, string table)
    {
        try
        {
            DataTable dt = cls.select_data_dt(
                "select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '" +
                table.Replace("'", "''") + "'");

            return dt != null && dt.Rows.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// The billable columns on each of the five tables. Cached per application
    /// lifetime, like the business analysis schema.
    /// </summary>
    public static List<TaxColumns> Tables(cls_connection cls)
    {
        HttpApplicationState app = HttpContext.Current == null ? null : HttpContext.Current.Application;

        if (app != null)
        {
            var cached = app[CacheKey] as List<TaxColumns>;
            if (cached != null) { return cached; }
        }

        var list = new List<TaxColumns>();

        foreach (ServiceUsageHelper.ServiceTable st in ServiceUsageHelper.Tables(cls))
        {
            if (!st.Usable) { continue; }

            var t = new TaxColumns();
            t.Table = st.Table;
            t.Service = st.Service;
            t.MemberCol = st.MemberCol;
            t.DateCol = st.DateCol;
            t.StatusCol = st.StatusCol;

            try
            {
                List<string> cols = Columns(cls, st.Table);
                t.GstCol = FirstPresent(cols, GstCandidates);
                t.TaxableCol = FirstPresent(cols, TaxableCandidates);
                t.TdsCol = FirstPresent(cols, TdsCandidates);
            }
            catch
            {
                // Left with nulls; HasTax is false and the caller names it.
            }

            list.Add(t);
        }

        if (app != null) { app[CacheKey] = list; }

        return list;
    }

    public static void ClearSchemaCache()
    {
        if (HttpContext.Current != null && HttpContext.Current.Application != null)
        {
            HttpContext.Current.Application.Remove(CacheKey);
        }
    }

    private static List<string> Columns(cls_connection cls, string table)
    {
        var names = new List<string>();

        DataTable dt = cls.select_data_dt(
            "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" +
            table.Replace("'", "''") + "'");

        if (dt == null) { return names; }

        foreach (DataRow r in dt.Rows) { names.Add(Convert.ToString(r["COLUMN_NAME"])); }

        return names;
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

    // ---- the figures -------------------------------------------------------

    /// <summary>
    /// One row per member per service for the period: taxable value, GST and
    /// TDS, with the member's GSTIN and state attached.
    ///
    /// SUCCESSFUL TRANSACTIONS ONLY. A failed transaction is reversed, its
    /// charge with it, so billing tax on one would invoice a member for a
    /// service they never received — and the reversal would never show up to
    /// correct it.
    ///
    /// <paramref name="skipped"/> comes back naming any table that carries no
    /// tax columns. Those are left out of the totals, and a page that does not
    /// say so is a page whose totals cannot be reconciled against anything.
    /// </summary>
    public static DataTable ByMember(cls_connection cls, string fromDate, string toDate,
                                     out List<string> skipped, out string error)
    {
        skipped = new List<string>();
        error = "";

        MemberTaxFields fields = MemberFields(cls);

        var usable = new List<TaxColumns>();

        foreach (TaxColumns t in Tables(cls))
        {
            if (t.HasTax) { usable.Add(t); }
            else { skipped.Add(t.Service); }
        }

        if (usable.Count == 0)
        {
            error = "None of the transaction tables carry a GST or surcharge column.";
            return null;
        }

        // GSTIN, preferring the registration the member actually filled in.
        // A column on member is only a fallback; GSTDetail is the screen where
        // this is maintained and is therefore the one that is kept current.
        string gstinSelect;

        if (fields.HasGstDetail)
        {
            string fallback = fields.GstinCol == null
                ? "''"
                : "isnull(cast(m.[" + fields.GstinCol + "] as nvarchar(20)), '')";

            gstinSelect =
                "isnull(nullif(ltrim(rtrim((select top 1 cast(gd.[GSTNo] as nvarchar(20)) " +
                "from [GSTDetail] gd where gd.[Msrno] = m.msrno " +
                "order by gd.[ID] desc))), ''), " + fallback + ") as Gstin";
        }
        else if (fields.GstinCol != null)
        {
            gstinSelect = "isnull(cast(m.[" + fields.GstinCol + "] as nvarchar(20)), '') as Gstin";
        }
        else
        {
            gstinSelect = "'' as Gstin";
        }

        // Contact and PAN, where the member table has them.
        //
        // PAN is not decoration on a TDS report — a deduction cannot be filed
        // against a person without one, and at a higher rate if it is missing.
        // Carrying it here means the report can show who is missing one BEFORE
        // the return is prepared rather than after it is rejected.
        List<string> memberCols = Columns(cls, "member");

        string mobileCol = FirstPresent(memberCols, new string[] { "Mobile", "MobileNo", "Phone" });
        string panCol = FirstPresent(memberCols, new string[] { "Pan", "PAN", "PanNo", "PANNumber" });

        string mobileSelect = mobileCol == null
            ? "'' as Mobile"
            : "isnull(cast(m.[" + mobileCol + "] as nvarchar(20)), '') as Mobile";

        string panSelect = panCol == null
            ? "'' as Pan"
            : "isnull(cast(m.[" + panCol + "] as nvarchar(20)), '') as Pan";

        // Place of supply. Joined through State when the member carries a state
        // id, which is how the registration screen stores it.
        //
        // An unknown state is labelled Unmapped rather than defaulted to the
        // company's own — a B2C sale filed under the wrong state is a
        // correction later, and a blank one is a question now.
        string stateSelect;
        string stateIdSelect = "0 as StateId";

        if (fields.StateIdCol != null)
        {
            stateSelect =
                "isnull(nullif(ltrim(rtrim((select top 1 cast(stx.[Name] as nvarchar(60)) " +
                "from [State] stx where stx.[ID] = m.[" + fields.StateIdCol + "]))), ''), 'Unmapped') as StateName";

            stateIdSelect = "isnull(cast(m.[" + fields.StateIdCol + "] as int), 0) as StateId";
        }
        else if (fields.StateCol != null)
        {
            stateSelect = "isnull(nullif(ltrim(rtrim(cast(m.[" + fields.StateCol + "] as nvarchar(60)))), ''), 'Unmapped') as StateName";
        }
        else
        {
            stateSelect = "'Unmapped' as StateName";
        }

        var union = new StringBuilder();

        for (int i = 0; i < usable.Count; i++)
        {
            TaxColumns t = usable[i];

            if (i > 0) { union.Append(" union all "); }

            string taxable = t.TaxableCol == null
                ? "0"
                : "cast(isnull(x.[" + t.TaxableCol + "],0) as decimal(18,2))";

            string gst = t.GstCol == null
                ? "0"
                : "cast(isnull(x.[" + t.GstCol + "],0) as decimal(18,2))";

            string tds = t.TdsCol == null
                ? "0"
                : "cast(isnull(x.[" + t.TdsCol + "],0) as decimal(18,2))";

            union.Append("select mm.msrno as msrno")
                 .Append(", N'").Append(t.Service.Replace("'", "''")).Append("' as Service")
                 .Append(", ").Append(taxable).Append(" as Taxable")
                 .Append(", ").Append(gst).Append(" as Gst")
                 .Append(", ").Append(tds).Append(" as Tds")
                 .Append(", 1 as Txns")
                 .Append(" from [").Append(t.Table).Append("] x")
                 .Append(" join member mm on mm.").Append(JoinCol(t)).Append(" = x.[").Append(t.MemberCol).Append("]")
                 .Append(" where upper(ltrim(rtrim(cast(x.[").Append(t.StatusCol).Append("] as nvarchar(50))))) = 'SUCCESS'")
                 .Append(" and x.[").Append(t.DateCol).Append("] >= '").Append(Safe(fromDate)).Append("'")
                 .Append(" and x.[").Append(t.DateCol).Append("] < dateadd(day, 1, '").Append(Safe(toDate)).Append("')");
        }

        string sql =
            "select m.msrno, m.LoginID, m.Name, m.ShopName, " +
            gstinSelect + ", " + stateSelect + ", " + stateIdSelect + ", " + mobileSelect + ", " + panSelect + ", " +
            "u.Service, " +
            "sum(u.Taxable) as Taxable, sum(u.Gst) as Gst, sum(u.Tds) as Tds, sum(u.Txns) as Txns " +
            "from (" + union + ") u " +
            "join member m on m.msrno = u.msrno " +
            "group by m.msrno, m.LoginID, m.Name, m.ShopName, " +
            (fields.GstinCol == null ? "" : "m.[" + fields.GstinCol + "], ") +
            (fields.StateIdCol != null ? "m.[" + fields.StateIdCol + "], "
                : fields.StateCol != null ? "m.[" + fields.StateCol + "], " : "") +
            (mobileCol == null ? "" : "m.[" + mobileCol + "], ") +
            (panCol == null ? "" : "m.[" + panCol + "], ") +
            "u.Service " +
            "having sum(u.Taxable) <> 0 or sum(u.Gst) <> 0 or sum(u.Tds) <> 0 " +
            "order by m.LoginID, u.Service";

        try
        {
            return cls.select_data_dt(sql);
        }
        catch (Exception ex)
        {
            error = "The invoice query failed: " + ex.Message;
            return null;
        }
    }

    private static string JoinCol(TaxColumns t)
    {
        return t.MemberCol.ToUpperInvariant().IndexOf("MSRNO", StringComparison.Ordinal) >= 0
            ? "msrno"
            : "ID";
    }

    // ---- tax split ---------------------------------------------------------

    /// <summary>
    /// Splits a GST amount into CGST/SGST or IGST.
    ///
    /// Decided by comparing STATE IDs, not names. Two rows can spell the same
    /// state differently — "Uttar Pradesh", "UTTAR PRADESH", a trailing space —
    /// and a name comparison would call those a different state and file the
    /// whole month as IGST. The ids come from one table and cannot disagree.
    ///
    /// The total is identical either way, which is exactly what makes getting
    /// this wrong easy to miss: the invoice still adds up, it is simply filed
    /// under the wrong heads.
    ///
    /// A member with no state falls back to intra-state, because that is the
    /// common case, and every page marks those rows so the assumption is
    /// visible rather than silent.
    /// </summary>
    public static void SplitTax(int memberStateId, int supplierStateId, decimal gst,
                                out decimal cgst, out decimal sgst, out decimal igst)
    {
        bool sameState =
            memberStateId <= 0 ||
            supplierStateId <= 0 ||
            memberStateId == supplierStateId;

        if (sameState)
        {
            cgst = Math.Round(gst / 2m, 2);

            // The remainder rather than a second rounding, so the two halves
            // always add back to the whole. Rounding each independently can
            // lose a paisa against the total on the same document.
            sgst = gst - cgst;
            igst = 0m;
        }
        else
        {
            cgst = 0m;
            sgst = 0m;
            igst = gst;
        }
    }

    /// <summary>
    /// Same split, for callers that only have the state names.
    ///
    /// Kept because the manual invoice page has no ids to work with — the
    /// operator types a state. Comparing names is weaker, and this is the only
    /// place it is acceptable.
    /// </summary>
    public static void SplitTaxByName(string memberState, string supplierState, decimal gst,
                                      out decimal cgst, out decimal sgst, out decimal igst)
    {
        bool sameState =
            string.IsNullOrEmpty(memberState) ||
            memberState == "Unmapped" ||
            string.IsNullOrEmpty(supplierState) ||
            string.Equals(memberState.Trim(), supplierState.Trim(), StringComparison.OrdinalIgnoreCase);

        if (sameState)
        {
            cgst = Math.Round(gst / 2m, 2);
            sgst = gst - cgst;
            igst = 0m;
        }
        else
        {
            cgst = 0m;
            sgst = 0m;
            igst = gst;
        }
    }

    /// <summary>
    /// An invoice number that is stable for a member and a month.
    ///
    /// Derived rather than stored, so re-opening the same month reprints the
    /// same number instead of issuing a new one. A tax invoice whose number
    /// changes every time it is viewed cannot be reconciled with anything.
    /// </summary>
    public static string InvoiceNumber(string yearMonth, string loginId, int msrno)
    {
        string ym = (yearMonth ?? "").Replace("-", "");
        string who = (loginId ?? "").Trim();

        if (who.Length == 0) { who = "M" + msrno; }

        return "INV/" + ym + "/" + who.ToUpperInvariant();
    }

    private static string Safe(string value)
    {
        if (value == null) { return ""; }
        return value.Trim().Replace("'", "''");
    }

    /// <summary>
    /// The first and last day of a yyyy-MM month, validated.
    ///
    /// Dates reach the query as literals, so the month is shaped here rather
    /// than trusted from a query string.
    /// </summary>
    public static bool MonthRange(string yearMonth, out string from, out string to)
    {
        from = "";
        to = "";

        DateTime d;
        if (!DateTime.TryParseExact((yearMonth ?? "").Trim() + "-01", "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out d))
        {
            return false;
        }

        from = d.ToString("yyyy-MM-dd");
        to = d.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
        return true;
    }
}
