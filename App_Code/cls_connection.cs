using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
/// <summary>
/// Summary description for cls_connection
/// </summary>
/// 

public class cls_connection
{
    SqlConnection con = new SqlConnection();
    SqlDataAdapter ad = null;
    SqlCommand cmd = null;
    int result = 0;

    public cls_connection()
    {
        
            con.ConnectionString = ConfigurationManager.ConnectionStrings["mycon"].ConnectionString;
        
    }


    //  Function for open & close the connection
    #region connection_check

    public void open_connection()
    {
        
            if (con.State == ConnectionState.Closed)
                con.Open();
       
    }

    public void open_connectionasny()
    {
        
            if (con.State == ConnectionState.Closed)
                con.OpenAsync();
        
    }


    public void close_connection()
    {
        
            if (con.State == ConnectionState.Open)
                con.Close();
       
    }



    #endregion

    public DataTable select_data_dtNew(string str)
    {

        open_connection();
        DataTable dt = new DataTable();
        ad = new SqlDataAdapter(str, con);
        ad.Fill(dt);
        //close_connection();
        return dt;

    }
    //  Function for fetch the data 
    #region data_functions

    public DataSet select_data_ds(string str)
    {
        open_connection();
        DataSet ds = new DataSet();
        ad = new SqlDataAdapter(str, con);
        ad.Fill(ds);
        close_connection();
        return ds;
    }

    public DataTable select_data_dt(string str)
    {
        open_connection();
        DataTable dt = new DataTable();
        ad = new SqlDataAdapter(str, con);
        ad.Fill(dt);
        close_connection();
        return dt;
    }


    public async System.Threading.Tasks.Task<DataTable> select_data_dtAsync(string str)
    {
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["mycon"].ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(str, connection))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dataTable = new DataTable();

                    // Asynchronously fill the DataTable
                    await Task.Run(() => adapter.Fill(dataTable));

                    return dataTable;
                }
            }
        }
    }

    public SqlDataReader select_data_dr(string str)
    {
        open_connection();
        cmd = new SqlCommand(str, con);
        SqlDataReader dtr = cmd.ExecuteReader();
        close_connection();
        return dtr;
    }

    public int select_data_scalar_int(string str)
    {
        open_connection();
        cmd = new SqlCommand(str, con);
        if (cmd.ExecuteScalar() != DBNull.Value)
            result = Convert.ToInt32(cmd.ExecuteScalar());
        else
            result = 0;

        close_connection();
        return result;
    }

    public async Task<int> select_data_scalar_intAsny(string str)
    {
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["mycon"].ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(str, connection))
            {
               
                    // ExecuteScalarAsync returns the first column of the first row in the result set
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
               
            }
        }
    }

    public double select_data_scalar_double(string str)
    {
        double result1;
        open_connection();
        cmd = new SqlCommand(str, con);
        if (cmd.ExecuteScalar() != DBNull.Value)
            result1 = Convert.ToDouble(cmd.ExecuteScalar());
        else
            result1 = 0;

        close_connection();
        return result1;
    }

    public double select_data_scalar_double_IncTimeout(string str)
    {
        double result1;
        open_connection();
        cmd = new SqlCommand(str, con);
        cmd.CommandTimeout = 180;
        if (cmd.ExecuteScalar() != DBNull.Value)
            result1 = Convert.ToDouble(cmd.ExecuteScalar());
        else
            result1 = 0;

        close_connection();
        return result1;
    }

    public long select_data_scalar_long(string str)
    {
        long result1;
        open_connection();
        cmd = new SqlCommand(str, con);
        if (cmd.ExecuteScalar() != DBNull.Value)
            result1 = Convert.ToInt64(cmd.ExecuteScalar());
        else
            result1 = 0;

        close_connection();
        return result1;
    }

    public string select_data_scalar_string(string str)
    {
        string result1;
        open_connection();
        cmd = new SqlCommand(str, con);
        if (cmd.ExecuteScalar() != DBNull.Value)
            result1 = Convert.ToString(cmd.ExecuteScalar());
        else
            result1 = "";

        close_connection();
        return result1;
    }

    public async Task<string> select_data_scalar_stringAsync(string str)
    {
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["mycon"].ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(str, connection))
            {
                
                    // ExecuteScalarAsync returns the first column of the first row in the result set
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToString(result);
                
            }
        }
    }


    public int insert_data(string str)
    {
        open_connection();
        cmd = new SqlCommand(str, con);
        int x = cmd.ExecuteNonQuery();
        close_connection();
        return x;
    }

    public int delete_data(string str)
    {
        open_connection();
        cmd = new SqlCommand(str, con);
        int x = cmd.ExecuteNonQuery();
        close_connection();
        return x;
    }

    public int update_data(string str)
    {
        open_connection();
        cmd = new SqlCommand(str, con);
        int x = cmd.ExecuteNonQuery();
        close_connection();
        return x;
    }


    public async Task<int> update_dataAsync(string str)
    {
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["mycon"].ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(str, connection))
            {
               
                    // ExecuteNonQueryAsync executes the query without returning any results
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected;
               
            }
        }
    }

    public decimal select_data_scalar_decimal(string str)
    {
        decimal result1;
        open_connection();
        cmd = new SqlCommand(str, con);
        if (cmd.ExecuteScalar() != DBNull.Value)
            result1 = Convert.ToDecimal(cmd.ExecuteScalar());
        else
            result1 = 0;

        close_connection();
        return result1;
    }

    public async Task<decimal> selectValueAsDecimal(string str)
    {
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["mycon"].ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(str, connection))
            {
               // ExecuteScalarAsync returns the first column of the first row in the result set
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToDecimal(result);
               
            }
        }
    }


    public async Task<bool> GetBool(string str)
    {
        bool result1;
        open_connection();
        cmd = new SqlCommand(str, con);
        if (cmd.ExecuteScalar() != DBNull.Value)
            result1 = Convert.ToBoolean(await cmd.ExecuteScalarAsync());
        else
            result1 = false;

        close_connection();
        return result1;
    }

    #endregion
    public void fill_MemberType(DropDownList ddl, string tp)
    {
        DataTable dt = new DataTable();
        dt = select_data_dt("exec procmlm_GetActiveMemberType '" + tp + "'");
        ddl.DataSource = dt;
        ddl.DataTextField = "membertype";
        ddl.DataValueField = "membertypeid";
        ddl.DataBind();
        ddl.Items.Insert(0, new ListItem("Select Member Type", "0"));

    }

    public DataTable ConvertJSONToDataTable(string jsonString)
    {
        DataTable dt = new DataTable();
        //strip out bad characters
        string[] jsonParts = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");

        //hold column names
        List<string> dtColumns = new List<string>();

        //get columns
        foreach (string jp in jsonParts)
        {
            //only loop thru once to get column names
            string jps = jp.Replace(@""",""", @"#");
            jps = jps.Replace(",", "$");
            jps = jps.Replace(@"#", @""",""");
            string[] propData = Regex.Split(jps.Replace("{", "").Replace("}", ""), ",");
            foreach (string rowData in propData)
            {
               
                    int idx = rowData.IndexOf(":");
                    string n = rowData.Substring(0, idx - 1);
                    string v = rowData.Substring(idx + 1);
                    if (!dtColumns.Contains(n))
                    {
                        dtColumns.Add(n.Replace("\"", ""));
                    }
                

            }
            break; // TODO: might not be correct. Was : Exit For
        }

        //build dt
        foreach (string c in dtColumns)
        {
            dt.Columns.Add(c);
        }
        //get table data
        foreach (string jp in jsonParts)
        {
            string jps = jp.Replace(@""",""", @"#");
            jps = jps.Replace(",", "$");
            jps = jps.Replace(@"#", @""",""");
            string[] propData = Regex.Split(jps.Replace("{", "").Replace("}", ""), ",");
            DataRow nr = dt.NewRow();
            foreach (string rowData in propData)
            {
              
                    int idx = rowData.IndexOf(":");
                    string n = rowData.Substring(0, idx - 1).Replace("\"", "");
                    string v = rowData.Substring(idx + 1).Replace("\"", "").Replace(@"\", "");
                    nr[n] = v;
               

            }
            dt.Rows.Add(nr);
        }
        return dt;
    }

    public void CreateErrorLog(string ErrorMsg, string FormName)
    {
        select_data_dt("exec proc_ErrorLog '" + ErrorMsg + "','" + FormName + "'");
    }
}