using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for RechargeSwitchingService
/// </summary>
public class RechargeSwitchingService
{

    cls_connection cls;
    public RechargeSwitchingService()
    {
        cls = new cls_connection();
    }


    public void ProcessRecharge(int msrNo, int operatorId, int packageId, decimal rechargeAmount, out string activeApi, out string authApi)
    {
        activeApi = string.Empty;
        authApi = string.Empty;

        List<DataRow> switchRows = GetSwitchingRows("USER", msrNo, operatorId, null);

        if (switchRows.Count == 0)
            switchRows = GetSwitchingRows("amount", null, operatorId, null);

        if (switchRows.Count == 0)
            switchRows = GetSwitchingRows("package", null, operatorId, packageId);

        if (switchRows.Count == 0)
            switchRows = GetSwitchingRows("opeator", null, operatorId, null);


        if (switchRows.Count == 0)
            return;


        foreach (DataRow row in switchRows)
        {
            string slabStr = row["SLAB"].ToString();
            string multipleSwitch = row["MULTIPLE_SWITCH"].ToString();
            string type = row["SWITCHTYPE"].ToString();

            // Apply slab validation only if TYPE is 'amountwise'
            string[] slabs = slabStr.Split(',');
            bool matched = false;

            foreach (string slab in slabs)
            {
                if (slab.Contains("-")) // range slab like 10-100
                {
                    string[] parts = slab.Split('-');
                    Int32 min, max;
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out  min) &&
                        int.TryParse(parts[1], out  max))
                    {
                        if (rechargeAmount >= min && rechargeAmount <= max)
                        {
                            matched = true;
                            break;
                        }
                    }
                }
                else // exact amount slab like 10,20,30
                {
                    int exactAmount;
                    if (int.TryParse(slab, out  exactAmount))
                    {
                        if (rechargeAmount == exactAmount)
                        {
                            matched = true;
                            break;
                        }
                    }
                }
            }

            if (matched)
            {
                activeApi = row["ACTIVEAPI"].ToString();
                if (multipleSwitch.Split(',').Length > 1)
                    authApi = multipleSwitch;
                return;
            }
        }
    }


    private List<DataRow> GetSwitchingRows(string switchType, int? msrNo, int operatorId, int? packageId)
    {
        List<DataRow> result = new List<DataRow>();

        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["mycon"].ConnectionString))
        {
            conn.Open();

            string query = "SELECT SLAB, ACTIVEAPI, MULTIPLE_SWITCH, SWITCHTYPE " +
                           "FROM RECHARGESWITCHINGNEWCONCEPT " +
                           "WHERE SWITCHTYPE = @SwitchType AND OPID = @OperatorId";

            if (switchType == "USER")
                query += " AND MSRNO = @MsrNo";

            if (switchType == "package")
                query += " AND PACKAGEID = @PackageId";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SwitchType", switchType);
                cmd.Parameters.AddWithValue("@OperatorId", operatorId);

                if (switchType == "USER")
                    cmd.Parameters.AddWithValue("@MsrNo", msrNo ?? 0);

                if (switchType == "package")
                    cmd.Parameters.AddWithValue("@PackageId", packageId ?? 0);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    result = dt.AsEnumerable().ToList();
                }
            }
        }

        return result;
    }

}