using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

/// <summary>
/// Shared plumbing for the admin report pages.
///
/// Every converted report was reimplementing the same two hundred lines:
/// safe field access, totals over the full result set, an in-memory search, an
/// Excel and CSV writer, and the diagnostics panel that explains an empty grid.
/// It lives here once instead.
///
/// Two rules the whole file exists to enforce:
///
///  1. A missing column must cost you that column, not the page. Eval throws
///     the moment a stored procedure renames something, and because the bind
///     happens inside a try/catch the result is a silently empty grid with no
///     clue why. Field() returns "" instead and records what was missing.
///
///  2. Totals are computed over the FULL cached result set, never the visible
///     page. A summary that only adds up one page of twenty contradicts itself
///     as you page through, which is worse than no summary.
/// </summary>
public static class ReportHelper
{
    // ---- field access -----------------------------------------------------

    /// <summary>
    /// Reads a column from a bound repeater row, returning "" when the result
    /// set does not have it. <paramref name="missing"/> collects the names that
    /// were absent so the page can report them.
    /// </summary>
    public static string Field(object dataItem, string column, List<string> missing)
    {
        var row = dataItem as DataRowView;
        if (row == null) { return ""; }

        if (!row.Row.Table.Columns.Contains(column))
        {
            if (missing != null && !missing.Contains(column)) { missing.Add(column); }
            return "";
        }

        object v = row[column];
        if (v == null || v == DBNull.Value) { return ""; }
        return Convert.ToString(v).Trim();
    }

    /// <summary>Value of a column on a DataRow, or "" when absent or null.</summary>
    public static string Cell(DataRow row, string column)
    {
        if (row == null) { return ""; }
        if (!row.Table.Columns.Contains(column) || row[column] == null || row[column] == DBNull.Value)
        {
            return "";
        }
        return row[column].ToString().Trim();
    }

    /// <summary>Em dash placeholder, so an empty cell does not look truncated.</summary>
    public static string Show(string value)
    {
        if (string.IsNullOrEmpty(value)) { return "&mdash;"; }
        return HttpUtility.HtmlEncode(value);
    }

    /// <summary>Two decimal money, junk passed through rather than swallowed.</summary>
    public static string Money(string value)
    {
        decimal d;
        if (decimal.TryParse(value, out d)) { return d.ToString("N2"); }
        return string.IsNullOrEmpty(value) ? "0.00" : HttpUtility.HtmlEncode(value);
    }

    /// <summary>Whole rupees, for amount columns where paise are noise.</summary>
    public static string Money0(string value)
    {
        decimal d;
        if (decimal.TryParse(value, out d)) { return d.ToString("N0"); }
        return string.IsNullOrEmpty(value) ? "0" : HttpUtility.HtmlEncode(value);
    }

    /// <summary>
    /// Masks all but the last four digits of a card or account number.
    /// Reports get screen-shared and exported; the last four are enough to
    /// recognise a transaction.
    /// </summary>
    public static string MaskTail(string value, int keep)
    {
        if (string.IsNullOrEmpty(value)) { return ""; }

        string s = value.Trim().Replace(" ", "").Replace("-", "");
        if (s.Length <= keep) { return s; }

        return new string('X', s.Length - keep) + s.Substring(s.Length - keep);
    }

    // ---- status -----------------------------------------------------------

    /// <summary>CSS class for the status pill.</summary>
    public static string StatusPill(string status)
    {
        string s = (status == null ? "" : status).Trim().ToUpperInvariant();

        if (s == "SUCCESS") { return "pill p-s"; }
        if (s == "PENDING" || s == "PROCESSING" || s == "INITIATED") { return "pill p-p"; }
        if (s == "REFUND" || s == "REVERSED" || s == "REVERSAL") { return "pill p-r"; }
        return "pill p-f";
    }

    // ---- totals -----------------------------------------------------------

    /// <summary>
    /// DataTable.Compute with every failure mode handled: no rows, a null
    /// result, a column the expression refers to that does not exist.
    /// </summary>
    public static string Compute(DataTable dt, string expression, string filter, string fallback)
    {
        try
        {
            if (dt == null || dt.Rows.Count == 0) { return fallback; }

            object v = dt.Compute(expression, filter);
            if (v == null || v == DBNull.Value) { return fallback; }

            decimal d;
            if (decimal.TryParse(v.ToString(), out d)) { return d.ToString("N0"); }
            return v.ToString();
        }
        catch
        {
            return fallback;
        }
    }

    /// <summary>
    /// Adds up one or more numeric columns across rows with the given status.
    ///
    /// Iterates rather than using Compute because these columns are routinely
    /// null on rows that never reached settlement, and Compute propagates a
    /// single null straight through to the total.
    ///
    /// <paramref name="statusColumn"/> is a parameter because the procedures do
    /// not agree: most return "Status", the payout report returns
    /// "RequestSTatus".
    /// </summary>
    public static string SumOf(DataTable dt, string statusColumn, string status, params string[] columns)
    {
        try
        {
            if (dt == null || dt.Rows.Count == 0) { return "0"; }

            bool filterByStatus = !string.IsNullOrEmpty(status) &&
                                  !string.IsNullOrEmpty(statusColumn) &&
                                  dt.Columns.Contains(statusColumn);

            decimal total = 0;

            foreach (DataRow r in dt.Rows)
            {
                if (filterByStatus &&
                    !string.Equals(Convert.ToString(r[statusColumn]), status, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (string col in columns)
                {
                    if (!dt.Columns.Contains(col) || r[col] == null || r[col] == DBNull.Value)
                    {
                        continue;
                    }

                    decimal v;
                    if (decimal.TryParse(Convert.ToString(r[col]), out v)) { total += v; }
                }
            }

            return total.ToString("N2");
        }
        catch
        {
            return "0";
        }
    }

    /// <summary>Row count for a status, over the whole result set.</summary>
    public static string CountOf(DataTable dt, string statusColumn, string status)
    {
        try
        {
            if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains(statusColumn)) { return "0"; }

            int n = 0;
            foreach (DataRow r in dt.Rows)
            {
                if (string.Equals(Convert.ToString(r[statusColumn]), status, StringComparison.OrdinalIgnoreCase))
                {
                    n++;
                }
            }
            return n.ToString("N0");
        }
        catch
        {
            return "0";
        }
    }

    // ---- search -----------------------------------------------------------

    /// <summary>
    /// Filters the cached rows in memory across the given columns.
    ///
    /// Done here rather than pushed into the stored procedure because most of
    /// these procedures either have no search parameter or take one whose
    /// behaviour is not documented anywhere, and guessing against a live
    /// production database is not worth it. It is also faster: repeat searches
    /// never touch SQL again.
    /// </summary>
    public static DataTable ApplySearch(DataTable dt, string term, params string[] fields)
    {
        if (dt == null || dt.Rows.Count == 0) { return dt; }

        term = (term == null ? "" : term.Trim());
        if (term.Length == 0) { return dt; }

        string needle = term.ToUpperInvariant();
        DataTable result = dt.Clone();

        foreach (DataRow r in dt.Rows)
        {
            bool hit = false;

            foreach (string f in fields)
            {
                if (!dt.Columns.Contains(f) || r[f] == null || r[f] == DBNull.Value) { continue; }

                if (Convert.ToString(r[f]).ToUpperInvariant().IndexOf(needle, StringComparison.Ordinal) >= 0)
                {
                    hit = true;
                    break;
                }
            }

            if (hit) { result.ImportRow(r); }
        }

        return result;
    }

    // ---- paging -----------------------------------------------------------

    /// <summary>Returns just the requested page of rows, same schema.</summary>
    public static DataTable Slice(DataTable all, int pageIndex, int pageSize)
    {
        DataTable slice = all.Clone();

        int start = pageIndex * pageSize;
        int count = Math.Min(pageSize, all.Rows.Count - start);

        for (int i = start; i < start + count; i++)
        {
            slice.ImportRow(all.Rows[i]);
        }

        return slice;
    }

    /// <summary>"Showing 1–25 of 1,240 · page 1 of 50"</summary>
    public static string PageInfo(int total, int pageIndex, int pageSize)
    {
        if (total == 0) { return "Nothing to show"; }

        int start = pageIndex * pageSize;
        int count = Math.Min(pageSize, total - start);
        int totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return "Showing " + (start + 1) + "–" + (start + count) +
               " of " + total.ToString("N0") +
               "  ·  page " + (pageIndex + 1) + " of " + totalPages;
    }

    // ---- export -----------------------------------------------------------

    private static string CsvCell(string value)
    {
        if (value == null) { value = ""; }
        value = value.Replace("\"", "\"\"");

        if (value.IndexOfAny(new char[] { ',', '"', '\n', '\r' }) >= 0)
        {
            return "\"" + value + "\"";
        }
        return value;
    }

    public static string BuildCsv(DataTable all, string[] columns, string[] headers)
    {
        var sb = new StringBuilder();

        var head = new List<string>();
        foreach (string h in headers) { head.Add(CsvCell(h)); }
        sb.AppendLine(string.Join(",", head.ToArray()));

        foreach (DataRow r in all.Rows)
        {
            var cells = new List<string>();
            foreach (string col in columns) { cells.Add(CsvCell(Cell(r, col))); }
            sb.AppendLine(string.Join(",", cells.ToArray()));
        }

        return sb.ToString();
    }

    public static string BuildXls(DataTable all, string[] columns, string[] headers)
    {
        var sb = new StringBuilder();

        sb.Append("<table border='1'><tr>");
        foreach (string h in headers)
        {
            sb.Append("<th>").Append(HttpUtility.HtmlEncode(h)).Append("</th>");
        }
        sb.Append("</tr>");

        foreach (DataRow r in all.Rows)
        {
            sb.Append("<tr>");
            foreach (string col in columns)
            {
                // mso-number-format text, so long numbers (mobile, order ID,
                // RRN, card number) do not arrive in Excel as 9.87654E+11.
                sb.Append("<td style=\"mso-number-format:'\\@'\">")
                  .Append(HttpUtility.HtmlEncode(Cell(r, col)))
                  .Append("</td>");
            }
            sb.Append("</tr>");
        }

        sb.Append("</table>");
        return sb.ToString();
    }

    /// <summary>
    /// Streams a download. Callers must have registered the button as a
    /// PostBackTrigger — a file cannot travel through an async postback,
    /// the UpdatePanel swallows it.
    /// </summary>
    public static void WriteDownload(HttpResponse response, string content, string fileName, string contentType)
    {
        response.Clear();
        response.Buffer = true;
        response.AddHeader("content-disposition", "attachment;filename=" + fileName);
        response.Charset = "";
        response.ContentType = contentType;
        // UTF-8 BOM so Excel reads Indian names and the rupee sign correctly.
        response.BinaryWrite(new byte[] { 0xEF, 0xBB, 0xBF });
        response.Output.Write(content);
        response.Flush();
        response.End();
    }
}
