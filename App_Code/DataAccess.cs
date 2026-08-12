using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using System.Web.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Net.NetworkInformation;



/// <summary>
/// Summary description for DataAccess
/// </summary>
public class DataAccess
{
    HttpContext ctx = HttpContext.Current;
    DataSet ds = new DataSet();

    DataTable dt = new DataTable();
    SqlCommand cmd;
    static string connString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["mycon"].ConnectionString;
    DbCommand dbcommand;
    SqlDatabase db = new SqlDatabase(connString);
    public static SqlConnection scon = new SqlConnection(connString);

    #region Common Variable
    private string _loginname, _password, _spaction, _SponsorID, _ReferID, _Position, _ServiceID, _UserType, _CompanyID, _UserID, _ID, _ImageName, _Code;
    private string _MsrNo, _MemberID, _GroupID, _Prifix, _FirstName, _MiddleName, _LastName, _Gender, _TinNumber, _Status, _Quantity, _Cof;
    private string _nFirstName, _nLastName, _nAge, _PinNumber, _PinSrNo;
    private string _AddressLine1, _AddressLine2, _AddressLine3, _ZipCode, _CountryID, _StateID, _CityID, _Narration, _CalculationFactor;
    private string _PackageID, _Email, _Mobile, _Phone, _Fax, _Nname, _NRelation, _BankName, _BranchName, _BankAccount, _BankIFSC, _PanCard;
    private DateTime _DOB, _Ndob, _DOJ, _ToDate, _FromDate;
    private decimal _Amount, _InterstRate, _CommissionRate, _BinaryRate, _DirectRate, _PackagePv, _PackageCapping, _Price;
    private string _FromMember, _ToMember, _branchCode, _ipAddress, _macAddress;

    public string MacAddress
    {
        get { return _macAddress; }
        set { _macAddress = value; }
    }

    public string IpAddress
    {
        get { return _ipAddress; }
        set { _ipAddress = value; }
    }

    public string BranchCode
    {
        get { return _branchCode; }
        set { _branchCode = value; }
    }
    public string FromMemberID
    {
        get { return this._FromMember; }
        set { this._FromMember = value; }
    }
    public string ToMemberID
    {
        get { return this._ToMember; }
        set { this._ToMember = value; }
    }
    public decimal Price
    {
        get { return this._Price; }
        set { this._Price = value; }
    }
    public decimal PackagePv
    {
        get { return this._PackagePv; }
        set { this._PackagePv = value; }
    }
    public string PackageID
    {
        get { return this._PackageID; }
        set { this._PackageID = value; }
    }
    public decimal BinaryRate
    {
        get { return this._BinaryRate; }
        set { this._BinaryRate = value; }
    }
    public decimal DirectRate
    {
        get { return this._DirectRate; }
        set { this._DirectRate = value; }
    }
    public decimal CappingPv
    {
        get { return this._PackageCapping; }
        set { this._PackageCapping = value; }
    }
    public string Quantity
    {
        get { return this._Quantity; }
        set { this._Quantity = value; }
    }

    public string GroupID
    {
        get { return this._GroupID; }
        set { this._GroupID = value; }
    }
    public string Narration
    {
        get { return this._Narration; }
        set { this._Narration = value; }
    }
    public string CalculationFactor
    {
        get { return this._CalculationFactor; }
        set { this._CalculationFactor = value; }
    }
    public string Status
    {
        get { return this._Status; }
        set { this._Status = value; }
    }
    public string TinNumber
    {
        get { return this._TinNumber; }
        set { this._TinNumber = value; }
    }
    public string Code
    {
        get { return this._Code; }
        set { this._Code = value; }
    }
    public string Fax
    {
        get { return this._Fax; }
        set { this._Fax = value; }
    }
    public string ImageName
    {
        get { return this._ImageName; }
        set { this._ImageName = value; }
    }
    public string UserType
    {
        get { return this._UserType; }
        set { this._UserType = value; }
    }
    public string UserID
    {
        get { return this._UserID; }
        set { this._UserID = value; }
    }
    public string ID
    {
        get { return this._ID; }
        set { this._ID = value; }
    }
    public string CompanyID
    {
        get { return this._CompanyID; }
        set { this._CompanyID = value; }
    }
    public string spaction
    {
        get { return this._spaction; }
        set { this._spaction = value; }
    }
    public string loginname
    {
        get { return this._loginname; }
        set { this._loginname = value; }
    }
    public string SponsorID
    {
        get { return this._SponsorID; }
        set { this._SponsorID = value; }
    }
    public string ReferID
    {
        get { return this._ReferID; }
        set { this._ReferID = value; }
    }
    public string Position
    {
        get { return this._Position; }
        set { this._Position = value; }
    }
    public string password
    {
        get { return this._password; }
        set { this._password = value; }
    }
    public string MsrNo
    {
        get { return this._MsrNo; }
        set { this._MsrNo = value; }
    }
    public string MemberID
    {
        get { return this._MemberID; }
        set { this._MemberID = value; }
    }
    public string Prifix
    {
        get { return this._Prifix; }
        set { this._Prifix = value; }
    }
    public string FirstName
    {
        get { return this._FirstName; }
        set { this._FirstName = value; }
    }
    public string MiddleName
    {
        get { return this._MiddleName; }
        set { this._MiddleName = value; }
    }
    public string LastName
    {
        get { return this._LastName; }
        set { this._LastName = value; }
    }
    public string Gender
    {
        get { return this._Gender; }
        set { this._Gender = value; }
    }
    public string COF
    {
        get { return this._Cof; }
        set { this._Cof = value; }
    }
    public string AddressLine1
    {
        get { return this._AddressLine1; }
        set { this._AddressLine1 = value; }
    }
    public string AddressLine2
    {
        get { return this._AddressLine2; }
        set { this._AddressLine2 = value; }
    }
    public string AddressLine3
    {
        get { return this._AddressLine3; }
        set { this._AddressLine3 = value; }
    }
    public string ZipCode
    {
        get { return this._ZipCode; }
        set { this._ZipCode = value; }
    }
    public string CountryID
    {
        get { return this._CountryID; }
        set { this._CountryID = value; }
    }
    public string StateID
    {
        get { return this._StateID; }
        set { this._StateID = value; }
    }
    public string CityID
    {
        get { return this._CityID; }
        set { this._CityID = value; }
    }
    public string Email
    {
        get { return this._Email; }
        set { this._Email = value; }
    }
    public string Mobile
    {
        get { return this._Mobile; }
        set { this._Mobile = value; }
    }
    public string Phone
    {
        get { return this._Phone; }
        set { this._Phone = value; }
    }
    public string Nname
    {
        get { return this._Nname; }
        set { this._Nname = value; }
    }
    public string PinNumber
    {
        get { return this._PinNumber; }
        set { this._PinNumber = value; }
    }
    public string PinSrNo
    {
        get { return this._PinSrNo; }
        set { this._PinSrNo = value; }
    }
    public string NRelation
    {
        get { return this._NRelation; }
        set { this._NRelation = value; }
    }
    public string BankName
    {
        get { return this._BankName; }
        set { this._BankName = value; }
    }
    public string BranchName
    {
        get { return this._BranchName; }
        set { this._BranchName = value; }
    }
    public string BankAccount
    {
        get { return this._BankAccount; }
        set { this._BankAccount = value; }
    }
    public string BankIFSC
    {
        get { return this._BankIFSC; }
        set { this._BankIFSC = value; }
    }
    public string PanCard
    {
        get { return this._PanCard; }
        set { this._PanCard = value; }
    }
    public string nFirstName
    {
        get { return this._FirstName; }
        set { this._nFirstName = value; }
    }
    public string nLastName
    {
        get { return this._nLastName; }
        set { this._nLastName = value; }
    }
    public string nAge
    {
        get { return this._nAge; }
        set { this._nAge = value; }
    }
    public DateTime DOJ
    {
        get { return this._DOJ; }
        set { this._DOJ = value; }
    }
    public DateTime DOB
    {
        get { return this._DOB; }
        set { this._DOB = value; }
    }
    public DateTime Ndob
    {
        get { return this._Ndob; }
        set { this._Ndob = value; }
    }
    public DateTime ToDate
    {
        get { return this._ToDate; }
        set { this._ToDate = value; }
    }
    public DateTime FromDate
    {
        get { return this._FromDate; }
        set { this._FromDate = value; }
    }
    public string ServiceID
    {
        get { return this._ServiceID; }
        set { this._ServiceID = value; }
    }
    public decimal Amount
    {
        get { return this._Amount; }
        set { this._Amount = value; }
    }
    public decimal IntrestRate
    {
        get { return this._InterstRate; }
        set { this._InterstRate = value; }
    }
    public decimal CommissionRate
    {
        get { return this._CommissionRate; }
        set { this._CommissionRate = value; }
    }
    #endregion

    public DataAccess()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    #region Connection function
    public void CreateConnection()
    {
        if (scon.State != ConnectionState.Closed)
        {
            scon.Open();
        }
    }
    public void CloseConnection()
    {
        scon.Close();
    }
    #endregion

    #region Common Function
    public DataSet GetDataSet(string SQL)
    {
        DataSet dsInv = new DataSet();
        dbcommand = db.GetSqlStringCommand(SQL);
        dsInv = db.ExecuteDataSet(dbcommand);

        return dsInv;
    }
    public DataTable GetDataTable(string SQL)
    {
        DataTable dtInv = new DataTable();
        dbcommand = db.GetSqlStringCommand(SQL);
        ds = db.ExecuteDataSet(dbcommand);
        if (ds.Tables.Count > 0)
        {
            dtInv = ds.Tables[0];
        }
        return dtInv;
    }
    public void BindDropDownList(DropDownList ddl, string sql, string TextField, string ValueField)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable(sql);
        ddl.DataSource = dt;
        ddl.DataTextField = TextField;
        ddl.DataValueField = ValueField;
        ddl.DataBind();
        ddl.Items.Insert(0, new ListItem("--Select--", "0"));
    }
    public int GetMaxValue(string Columns, string TableName)
    {
        int Retval = 0;
        dbcommand = db.GetSqlStringCommand("SELECT ISNULL(MAX(" + Columns + "),0)+1 AS 'Retval'  FROM " + TableName + "");
        Retval = Convert.ToInt32(db.ExecuteScalar(dbcommand));
        return Retval;
    }
    public int ExecuteQuery(string qur)
    {
        int Retval = 0;
        dbcommand = db.GetSqlStringCommand(qur);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int ExecuteScalar(string qur)
    {
        int Retval = 0;
        dbcommand = db.GetSqlStringCommand(qur);
        Retval = Convert.ToInt32(db.ExecuteScalar(dbcommand));
        return Retval;
    }
    public int ExecuteIntScalar(string qur)
    {
        int Retval = 0;
        dbcommand = db.GetSqlStringCommand(qur);
        Retval = Convert.ToInt32(db.ExecuteScalar(dbcommand));
        return Retval;
    }
    public string ExecuteStringScalar(string qur)
    {
        string Retval = string.Empty;
        dbcommand = db.GetSqlStringCommand(qur);
        Retval = Convert.ToString(db.ExecuteScalar(dbcommand));
        return Retval;
    }

    public string insert_with_SP_STR(string sp_name, SqlParameter[] param1)
    {
        CreateConnection();


        cmd = new SqlCommand();
        cmd.Connection = scon;
        cmd.CommandText = sp_name;
        cmd.CommandType = CommandType.StoredProcedure;
        if (param1 != null)
        {
            for (int i = 0; i < param1.Length; i++)
            {
                cmd.Parameters.Add(param1[i]);
            }
        }

        string x = cmd.ExecuteScalar().ToString();
        CloseConnection();

        return x;
    }
    public DataSet select_with_SP(string sp_name, SqlParameter[] param)
    {


        cmd = new SqlCommand();
        cmd.CommandText = sp_name;
        cmd.Connection = scon;
        cmd.CommandType = CommandType.StoredProcedure;
        if (param != null)
        {
            for (int i = 0; i < param.Length; i++)
            {
                cmd.Parameters.Add(param[i]);
            }
        }

        SqlDataAdapter ad = new SqlDataAdapter();
        ad.SelectCommand = cmd;

        DataSet dsp = new DataSet();
        ad.Fill(dsp);


        return dsp;

    }
    public DataSet SP_fin_MoneyReceiptDetails(string accountno, double dueamt)
    {
        dbcommand = db.GetStoredProcCommand("[SP_fin_MoneyReceiptDetails_New]", accountno, dueamt);
        ds = db.ExecuteDataSet(dbcommand);
        return ds;
    }
    public int insert_int_SP_STR(string sp_name, SqlParameter[] param1)
    {
        CreateConnection();


        cmd = new SqlCommand();
        cmd.Connection = scon;
        cmd.CommandText = sp_name;
        cmd.CommandType = CommandType.StoredProcedure;
        if (param1 != null)
        {
            for (int i = 0; i < param1.Length; i++)
            {
                cmd.Parameters.Add(param1[i]);
            }
        }

        int x = cmd.ExecuteNonQuery();
        CloseConnection();

        return x;
    }
    #endregion

    #region Fill Country / State / City
    public void BindCountry(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("select CountryID,UPPER(CountryName)CountryName from tbl_CountryMaster where IsActive<>0");
        DataRow dr = dt.NewRow();
        dr["CountryName"] = "--Select--";
        dr["CountryID"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "CountryName";
        ddl.DataValueField = "CountryID";
        ddl.DataBind();
    }

    public void BindState(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("select StateID,UPPER(StateName)StateName from Loan_state_master where IsActive<>0 And CountryID='76'");
        DataRow dr = dt.NewRow();
        dr["StateName"] = "--Select--";
        dr["StateID"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "StateName";
        ddl.DataValueField = "StateID";
        ddl.DataBind();
    }
   
    public void BindCity(DropDownList ddl, string StateID)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("select id as CityID,UPPER(district)CityName from loan_master_district where IsActive<>0 And State_ID='" + StateID + "'");
        DataRow dr = dt.NewRow();
        dr["CityName"] = "--Select--";
        dr["CityID"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "CityName";
        ddl.DataValueField = "CityID";
        ddl.DataBind();
    }
    #endregion

    #region Fill Title
    public void BindTitle(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("select titleid,titlename from tbl_TitleMaster where IsActive<>0");
        DataRow dr = dt.NewRow();
        dr["TitleName"] = "--Select--";
        dr["TitleID"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "TitleName";
        ddl.DataValueField = "TitleID";
        ddl.DataBind();
    }
    #endregion

    #region Fill Banker
    public void BindBanker(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("select BankerID,UPPER(BankerName)BankerName from tbl_BankerMaster where IsActive<>0");
        DataRow dr = dt.NewRow();
        dr["BankerName"] = "--Select--";
        dr["BankerID"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "BankerName";
        ddl.DataValueField = "BankerID";
        ddl.DataBind();
    }
    #endregion

    #region Company Setting Function

    #region Edit Company
    public int UpdateCompanyInfo()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_setting_editcompany", CompanyID, FirstName, ImageName, AddressLine1, AddressLine2, AddressLine3, Phone, Mobile, Email, Fax, TinNumber, BranchCode, BranchName);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion

    #region Create /Edit Package
    public int ItemMaster()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_ItemMaster", spaction, ID, Code, FirstName, Price, PackagePv, CappingPv, DirectRate, BinaryRate, MiddleName, LastName, Status);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion

    #region Is Referral With Spill
    public string IsRefWithSpill(string introid, string refid)
    {
        string Ratval = "!";
        dbcommand = db.GetStoredProcCommand("IsRefWithSpill", introid, refid);
        Ratval = Convert.ToString(db.ExecuteScalar(dbcommand));
        return Ratval;
    }

    #endregion

    #region Is Referral Without Spill
    public string IsRefWithoutSpill(string introid, string refid)
    {
        string Ratval = "!";
        dbcommand = db.GetStoredProcCommand("IsRefWithoutSpill", introid, refid);
        Ratval = Convert.ToString(db.ExecuteScalar(dbcommand));
        return Ratval;
    }

    #endregion

    #region Update Pair Condition

    public int UpdatePairCondition()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_setting_paircondition", ID, CappingPv, BinaryRate, PackageID);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    #endregion

    #region Update Direct Income
    public int UpdateDirectCondition(string IsLimit, int Limit, string IsItem, decimal Multiplier)
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_setting_directcondition", IsLimit, Limit, IsItem, Multiplier);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion

    #region Update Spill Income
    public int UpdateSpillCondition(int LeftLimit, int RightLimit, decimal Multiplier)
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_setting_spillcondition", LeftLimit, RightLimit, Multiplier);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion

    #region Update Reward Condition
    public int UpdateRewardCondition(string Action, int ID, string Designation, int LeftLimit, int RightLimit, decimal Multiplier, string RewardName, string IsTimeLimit, int InDay, string IsActive)
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_setting_rewardcondition", Action, ID, Designation, LeftLimit, RightLimit, Multiplier, RewardName, IsTimeLimit, InDay, IsActive);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion

    #region Royalty Setting
    public int RoyaltySetting(string _Action, int _ID, string _PrintName, decimal _LeftCount, decimal _RightCount, decimal _FreshPair, string _CalculationFlag, string _MultiplierFlag, decimal _Multiplier, string _Status)
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Sp_Setting_RoyaltySetting", _Action, _ID, _PrintName, _LeftCount, _RightCount, _FreshPair, _CalculationFlag, _MultiplierFlag, _Multiplier, _Status);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion

    #region Performance Incentive Condition
    public int UpdatePerformanceIncentiveCondition(string ID, string LeftSide, string RightSide, string CalculationFlag, string MultiplierFlag, string Multiplier)
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_setting_performance", ID, LeftSide, RightSide, CalculationFlag, MultiplierFlag, Multiplier);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion
    #endregion

    #region Login For All Type Users
    public DataSet loginUser()
    {
        DataTable dtUserMaste = new DataTable();
        dbcommand = db.GetStoredProcCommand("sp_user_login", loginname, password, IpAddress, MacAddress);
        ds = db.ExecuteDataSet(dbcommand);
        return ds;
    }
    #endregion

    #region Admin Function
    public DataTable AdminLoginAuthentication()
    {
        DataTable dtUserMaste = new DataTable();
        dbcommand = db.GetStoredProcCommand("sp_AdminLoginAuthentication", loginname, password);
        ds = db.ExecuteDataSet(dbcommand);
        dtUserMaste = ds.Tables[0];
        return dtUserMaste;
    }

    //public int CreateEditUser()
    //{
    //    int Retval = 0;
    //    dbcommand = db.GetStoredProcCommand("sp_User_CreateEdit", spaction, MemberID, FirstName, Email, Mobile, loginname, password, UserType, CompanyID);
    //    Retval = db.ExecuteNonQuery(dbcommand);
    //    return Retval;
    //}

    public int CreateEditUser(string photopath)
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_User_CreateEdit", spaction, MemberID, FirstName, Email, Mobile, loginname, password, UserType, CompanyID, ID, photopath);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    #endregion

    #region Sign Up Function


    #endregion

    #region Member Function
    public DataTable SelectRewardDetail()
    {
        DataTable dtUserMaste = new DataTable();
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_SelectRewardStatusForMember", MemberID);
        ds = db.ExecuteDataSet(dbcommand);
        dtUserMaste = ds.Tables[0];
        return dtUserMaste;
    }
    public DataTable MemberLoginAuthentication()
    {
        DataTable dtUserMaste = new DataTable();
        dbcommand = db.GetStoredProcCommand("sp_MemberLoginAuthentication", loginname, password);
        ds = db.ExecuteDataSet(dbcommand);
        dtUserMaste = ds.Tables[0];
        return dtUserMaste;
    }
   

    public int InsertMember()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_member_insert", MemberID, GroupID, SponsorID, ReferID, Position, Prifix, FirstName, MiddleName, LastName, Gender, COF, DOB, PanCard, "CASH", loginname, password
                                                                , AddressLine1, CountryID, StateID, CityID, ZipCode, Mobile, Email
                                                                , nFirstName, nLastName, Ndob, nAge, BankName, BranchName, BankAccount, "", BankIFSC, PinNumber);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    #endregion

    #region Downline Function
    public DataTable SelectDirectTreeView()
    {
        DataTable dt = new DataTable();
        dbcommand = db.GetStoredProcCommand("sp_downline_select_directtreeview", MsrNo);
        ds = db.ExecuteDataSet(dbcommand);
        if (ds.Tables.Count > 0)
        {
            dt = ds.Tables[0];
        }
        return dt;
    }
    public DataSet SelectLeftRightMember()
    {
        DataSet DSelect = new DataSet();
        dbcommand = db.GetStoredProcCommand("SP_Select_LeftRightMember", MemberID, Status);
        DSelect = db.ExecuteDataSet(dbcommand);
        return DSelect;
    }
    public DataSet SelectDirectMember()
    {
        DataSet DSelect = new DataSet();
        dbcommand = db.GetStoredProcCommand("SP_SelectDirectMember", MemberID);
        DSelect = db.ExecuteDataSet(dbcommand);
        return DSelect;
    }
    public DataSet SelectDirectLevelMember()
    {
        DataSet DSelect = new DataSet();
        dbcommand = db.GetStoredProcCommand("SP_SelectDirectLevelMember", MemberID, Status);
        DSelect = db.ExecuteDataSet(dbcommand);
        return DSelect;
    }
    public DataSet SelectBinaryLevelMember()
    {
        DataSet DSelect = new DataSet();
        dbcommand = db.GetStoredProcCommand("SP_SelectBinaryLevelMember", MemberID, Status);
        DSelect = db.ExecuteDataSet(dbcommand);
        return DSelect;
    }
    #endregion

    #region Closing Function
    public int GenerateBinaryPayout()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_GenerateBinaryPayout");
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    //select due weekly payout
    public DataTable SelectDueWeeklyPayout()
    {
        DataTable dt = new DataTable();
        dbcommand = db.GetStoredProcCommand("dbo.sp_select_weeklypayout");
        ds = db.ExecuteDataSet(dbcommand);
        if (ds.Tables.Count > 0)
        {
            dt = ds.Tables[0];
        }
        return dt;
    }
    //select closing summary
    public DataSet SelectClosingSummary()
    {
        DataSet Data = new DataSet();
        dbcommand = db.GetStoredProcCommand("dbo.sp_selectclosingsummary");
        Data = db.ExecuteDataSet(dbcommand);
        return Data;
    }
    //
    public DataSet SelectClosingDetails(string type)
    {
        DataSet Data = new DataSet();
        dbcommand = db.GetStoredProcCommand("dbo.sp_selectclosingdetail", type);
        Data = db.ExecuteDataSet(dbcommand);
        return Data;
    }
    //Wallet Payment Details
    public DataSet SelectClosingDetailsFromWallet(string type)
    {
        DataSet Data = new DataSet();
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_SelectClosingDetail", type);
        Data = db.ExecuteDataSet(dbcommand);
        return Data;
    }
    // Select Closing Summary For Member
    public DataSet SelectClosingDetailsFromWalletToMember()
    {
        DataSet Data = new DataSet();
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_SelectClosingSummaryForMember", MemberID);
        Data = db.ExecuteDataSet(dbcommand);
        return Data;
    }
    //Wallet Payment Details
    public DataSet SelectClosingDetailsFromWalletToMember(string type, string memberid)
    {
        DataSet Data = new DataSet();
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_SelectClosingDetail", type);
        Data = db.ExecuteDataSet(dbcommand);
        return Data;
    }
    // Generate Daily Closing For Wallet
    public int Wallet_GenerateDailyPayout()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_GenerateDailyClosing");
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    // Generate Weekly Closing For Wallet
    public int Wallet_GenerateWeeklyPayout()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_GenerateWeeklyClosing");
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    // Generate Monthly Closing For Wallet
    public int Wallet_GenerateMonthlyPayout()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_GenerateMonthlyClosing");
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    #endregion

    #region Pin Wallet
    public int PinWalletTransaction()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_pinwallettransaction", MemberID, Amount, CalculationFactor, Narration);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public string VerifyBalanceForPinGenerate()
    {
        string Ratval = "!";
        dbcommand = db.GetStoredProcCommand("sp_pin_verifybalance", ID, Quantity, MemberID);
        Ratval = db.ExecuteScalar(dbcommand).ToString();
        return Ratval;
    }
    
    public int GeneratePin()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_pin_generate", ID, Quantity, MemberID);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public DataTable SelectGeneratedPin()
    {
        DataTable dt = new DataTable();
        dbcommand = db.GetStoredProcCommand("sp_pin_selectgenerated", Quantity, MemberID);
        ds = db.ExecuteDataSet(dbcommand);
        if (ds.Tables.Count > 0)
        {
            dt = ds.Tables[0];
        }

        return dt;


    }
    public string ValidatePin(string pinsrno, string pinnumber)
    {
        string Ratval = "!";
        dbcommand = db.GetStoredProcCommand("sp_pin_validate", pinsrno, pinnumber);
        Ratval = Convert.ToString(db.ExecuteScalar(dbcommand));
        return Ratval;
    }
    #endregion

    #region E-Wallet
    public string SendWithdrawalRequest()
    {
        string Ratval = "!";
        dbcommand = db.GetStoredProcCommand("SP_eWalletWithdrawalRequest", Code, MemberID, Amount);
        ds = db.ExecuteDataSet(dbcommand);
        if (ds.Tables.Count > 0)
        {
            Ratval = ds.Tables[0].Rows[0]["Ratval"].ToString();
        }

        return Ratval;


    }
    public int EwalletTranscation()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_ewallettransaction", MemberID, Amount, CalculationFactor, Narration);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    public string TransferEwalletToPwallet()
    {
        string Val = string.Empty;
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_EwalletToPwallet", FromMemberID, ToMemberID, Amount);
        Val = db.ExecuteScalar(dbcommand).ToString();
        return Val;

    }
    public string TransferPwalletToPwallet()
    {
        string Val = string.Empty;
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_PwalletToPwallet", FromMemberID, ToMemberID, Amount);
        Val = db.ExecuteScalar(dbcommand).ToString();
        return Val;

    }
    public string TransferEwalletToEwallet()
    {
        string Val = string.Empty;
        dbcommand = db.GetStoredProcCommand("Sp_Wallet_EwalletToEwallet", FromMemberID, ToMemberID, Amount);
        Val = db.ExecuteScalar(dbcommand).ToString();
        return Val;

    }
    #endregion


    #region [Affillate Module]
    public int AffillateCreate()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("Sp_Affi_CreateScript", spaction, ID, pageDescription, pagemeta, ToDate);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    #endregion

    #region Pawan
    //29-08-2012

    #region Variable
    private Boolean _Shipping, _Cash;
    private string _pageId, _pageTitel, _pageUrl, _pagemeta, _pageDescription, _FromMail, _ByMail, _Password, _SMTP, _PortNumber, _Balance, _Action;
    private string _NewsId, _Short_Titel, _Full_Titel, _StartDate, _EndDate, _NewsDes, _EventId, _EventDes, _Subject, _Category, _Priority, _Department, _Attechment, _Count, _Flage;
    public string Count
    {
        get { return this._Count; }
        set { this._Count = value; }
    }
    public string Flage
    {
        get { return this._Flage; }
        set { this._Flage = value; }
    }
    public string pageId
    {
        get { return this._pageId; }
        set { this._pageId = value; }
    }
    public string Action
    {
        get { return this._Action; }
        set { this._Action = value; }
    }
    public string pageTitel
    {
        get { return this._pageTitel; }
        set { this._pageTitel = value; }
    }

    public string pageUrl
    {
        get { return this._pageUrl; }
        set { this._pageUrl = value; }
    }
    public string pagemeta
    {
        get { return this._pagemeta; }
        set { this._pagemeta = value; }
    }
    public string pageDescription
    {
        get { return this._pageDescription; }
        set { this._pageDescription = value; }
    }

    public string NewsId
    {
        get { return this._NewsId; }
        set { this._NewsId = value; }
    }

    public string Full_Titel
    {
        get { return this._Full_Titel; }
        set { this._Full_Titel = value; }
    }

    public string Short_Titel
    {
        get { return this._Short_Titel; }
        set { this._Short_Titel = value; }
    }

    public string StartDate
    {
        get { return this._StartDate; }
        set { this._StartDate = value; }
    }
    public string EndDate
    {
        get { return this._EndDate; }
        set { this._EndDate = value; }
    }
    public string NewsDes
    {
        get { return this._NewsDes; }
        set { this._NewsDes = value; }
    }
    public string EventId
    {
        get { return this._EventId; }
        set { this._EventId = value; }
    }
    public string EventDes
    {
        get { return this._EventDes; }
        set { this._EventDes = value; }
    }
    public string FromMail
    {
        get { return this._FromMail; }
        set { this._FromMail = value; }
    }
    public string ByMail
    {
        get { return this._ByMail; }
        set { this._ByMail = value; }
    }
    public string Password
    {
        get { return this._Password; }
        set { this._Password = value; }
    }
    public string SMTP
    {
        get { return this._SMTP; }
        set { this._SMTP = value; }
    }
    public string PortNumber
    {
        get { return this._PortNumber; }
        set { this._PortNumber = value; }
    }
    public string Subject
    {
        get { return this._Subject; }
        set { this._Subject = value; }
    }
    public string Category
    {
        get { return this._Category; }
        set { this._Category = value; }
    }
    public string Priority
    {
        get { return this._Priority; }
        set { this._Priority = value; }
    }
    public string Department
    {
        get { return this._Department; }
        set { this._Department = value; }
    }
    public string Attechment
    {
        get { return this._Attechment; }
        set { this._Attechment = value; }
    }
    public string Balance
    {
        get { return this._Balance; }
        set { this._Balance = value; }
    }
    public Boolean Shipping
    {
        get { return this._Shipping; }
        set { this._Shipping = value; }
    }
    public Boolean Cash
    {
        get { return this._Cash; }
        set { this._Cash = value; }
    }
    #endregion

    #region Function
    public int CreateEditPage()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_cms_CreateEditPage", spaction, pageId, pageTitel, pageUrl, pagemeta, pageDescription);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    public int CreateLabel()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_Msg_CreateLabel", NewsDes);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int CreateEditNews()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_CreateEditNews", spaction, NewsId, Short_Titel, NewsDes, Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate));
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    public int MsgInformation()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_Msg_Information", ByMail, Subject, EventDes, Attechment);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int draftMsgInformation()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_DraftMsg_Information", ByMail, Subject, EventDes, Attechment);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int GenrateTicket()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_Ticket_Genrate", Category, Department, Subject, NewsDes, NewsId, Convert.ToDateTime(FromDate), Priority, Attechment, MemberID);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int TicketReply()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_Ticket_Reply", NewsId, Subject, Convert.ToDateTime(FromDate));
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int AdminMsgReply()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_Admin_Msg_Reply", NewsId, Subject, Convert.ToDateTime(FromDate));
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int CreateEditEvents()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_CreateEditEvents", spaction, EventId, Short_Titel, EventDes, Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate));
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    public int Email_Config()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("sp_support-emailConfig", FromMail, ByMail, Password, SMTP, PortNumber);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int CreateEditSession()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Acc_FinancalYear", spaction, EventId, Full_Titel, EventDes);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int CreateEditGroup()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Acc_GroupMaster", spaction, EventId, Full_Titel, EventDes, NewsId, Subject);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int CreateLedger()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Acc_LadgerMaster", spaction, EventId, Full_Titel, EventDes, Subject, NRelation);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    public int CreateLedgeropeningBal()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Acc_LadgerMaster", spaction, NewsId, EventId, NRelation, Balance);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int CreateVoucher()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Acc_VoucherMaster", spaction, Full_Titel, EventDes, Subject, FromMail, Balance);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int ConfigureVoucher()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Acc_VoucherConfiger", Balance, NewsId, Full_Titel);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }
    public int GenVoucher()
    {
        int Retval = 0;
        dbcommand = db.GetStoredProcCommand("Acc_VoucherGenerate", Balance, NewsId, EventId, Full_Titel);
        Retval = db.ExecuteNonQuery(dbcommand);
        return Retval;
    }

    public int UpdateMember()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_member_update", MemberID, Prifix, FirstName, MiddleName, LastName, Gender, DOB, PanCard
                                                                , AddressLine1, CountryID, StateID, CityID, ZipCode, Mobile, Email
                                                               , BankName, BranchName, BankAccount, FromMail, BankIFSC);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int changePass()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_change_Pass", MemberID, Phone, password);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int DedutionSetting()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_Dedution_setting", spaction, MemberID, FirstName, password);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    public int createCategory()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_Category_Product", spaction, MemberID, Email, FirstName, EventDes, password, StateID, CityID);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int AddProduct()
    {
        int Ratval = 0;
        // dbcommand = db.GetStoredProcCommand("shop_Product",spaction, MemberID, FirstName, EventDes, CountryID, StateID, CityID, ZipCode, Mobile, Email);
        dbcommand = db.GetStoredProcCommand("shop_Product", spaction, MemberID, FirstName, password, EventDes, CountryID, BankIFSC, StateID, CityID, BranchName, ZipCode, Shipping, Cash, _AddressLine3, Email, BankName);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    public int EwalletTransfer()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_ewallettransaction", FirstName, password, EventDes, CountryID);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }


    public int Updatestock()
    {
        int Ratval = 0;

        dbcommand = db.GetStoredProcCommand("shop_UpdateProductStock", MemberID, ZipCode);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    public int VendorMaster()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_VendorMaster", Action, spaction, FirstName, BankName, Phone, Mobile, AddressLine1, CountryID, StateID, CityID, ZipCode, Fax, Balance, BankAccount, EventDes, NewsDes, NewsId, Nname, Email);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }



    public int SalesMaster()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_SalesMaster", spaction, MemberID, FirstName, password, EventDes, CountryID, BankIFSC, StateID, ByMail, CityID, BranchName, ZipCode, Email, BankName, _AddressLine3);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }


    public int SalesTransation()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_SalesTransaction", spaction, MemberID, Prifix, FirstName, EventId, Short_Titel, Full_Titel, Category, Department, Subject, NewsDes, NewsId);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int AddsImage()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_Addsimage", spaction, MemberID, FirstName, password, EventDes, Full_Titel);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int PinwalletTransfer()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("sp_pinwallettransaction", Attechment, NRelation, Action, SMTP);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int OrderDispatch()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_OrderDispatch", spaction, MemberID, Status, TinNumber, EventDes, CountryID, StateID, ToDate);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int CourierVenderMaster()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_CourierVenderMaster", spaction, MemberID, FirstName, password, EventDes, CountryID, BankIFSC);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }


    public int InsertDashboardDetails()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("Sp_MemberDashBoard_Insert", MemberID);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    public int CreateTopUp()
    {
        int Val = 0;
        dbcommand = db.GetStoredProcCommand("sp_CreateTopUP", MemberID, PackageID);
        Val = db.ExecuteNonQuery(dbcommand);
        return Val;

    }
    public DataTable SearchDownlineCount()
    {
        DataTable dtReturn = new DataTable();
        dbcommand = db.GetStoredProcCommand("SP_Select_Downline", MemberID, Count, Flage);
        dtReturn = db.ExecuteDataSet(dbcommand).Tables[0];
        return dtReturn;

    }

    #endregion

    #region Preetma


    public int CreateUnit()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_UnitMaster", spaction, MemberID, FirstName, EventDes, CountryID, StateID);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    public int CreateCurrency()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_CurrencyMaster", spaction, MemberID, EventDes, CountryID, CityID, StateID, Mobile, ZipCode);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    public int CreateTax()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_TaxMaster", spaction, MemberID, EventDes, CountryID, CityID);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }

    public int CreateSchemaTax()
    {
        int Ratval = 0;
        dbcommand = db.GetStoredProcCommand("shop_TaxSchema", spaction, MemberID, EventDes, CountryID, Email, CityID);

        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    #endregion
    #endregion



    public string changedatetoint(string ddmmyy)
    {
        string mmddyy = "";
        //mmddyy = ddmmyy.Substring(3, 2) + "-" + ddmmyy.Substring(0, 2) + "-" + ddmmyy.Substring(6, 4);
        mmddyy = ddmmyy.Substring(6, 4) + ddmmyy.Substring(3, 2) + ddmmyy.Substring(0, 2);
        return mmddyy;
    }
    public string changedatetoint1(string ddmmyy)
    {
        string mmddyy = "";
        //mmddyy = ddmmyy.Substring(3, 2) + "-" + ddmmyy.Substring(0, 2) + "-" + ddmmyy.Substring(6, 4);
        mmddyy = ddmmyy.Substring(6, 4) + "-" + ddmmyy.Substring(3, 2) + "-" + ddmmyy.Substring(0, 2);
        return mmddyy;
    }
    public string changedatetoint2(string ddmmyy)
    {
        string mmddyy = "";
        //mmddyy = ddmmyy.Substring(3, 2) + "-" + ddmmyy.Substring(0, 2) + "-" + ddmmyy.Substring(6, 4);
        mmddyy = ddmmyy.Substring(3, 2) + "-" + ddmmyy.Substring(0, 2) + "-" + ddmmyy.Substring(6, 4);
        return mmddyy;
    }
    public DataSet Sp_Under_Customer_Report(string AgentID, string dt1, string dt2)
    {
        //dbcommand = db.GetStoredProcCommand("[Sp_Under_Agent_Report]", AgentID);
        //ds = db.ExecuteDataSet(dbcommand);
        string str = "exec Sp_Under_Agent_Report '" + AgentID + "'";
        if (dt1 != "" && dt2 != "")
            str = str + ",'" + dt1 + "','" + dt2 + "'";
        ds = GetDataSet(str);
        return ds;
    }
    public DataSet SP_AccountclosingReport(string accountno, string formno, int branchid, DateTime fromdate, DateTime todate, string maturitytype)
    {
        dbcommand = db.GetStoredProcCommand("SP_AccountclosingReport", accountno, formno, branchid, fromdate, todate, maturitytype);
        ds = db.ExecuteDataSet(dbcommand);
        return ds;
    }
    public void BindCareof(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("select * from tbl_careof_list where IsActive=1");
        //DataRow dr = dt.NewRow();
        //dr["TitleName"] = "--Select--";
        //dr["TitleID"] = "-1";
        //dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "cname";
        ddl.DataValueField = "cname";
        ddl.DataBind();
    }
    public DataTable CustomerLoginAuthentication()
    {
        DataTable dtusermaste = new DataTable();
        dbcommand = db.GetStoredProcCommand("sp_CustomerLoginAuthentication", loginname, password);
        ds = db.ExecuteDataSet(dbcommand);
        dtusermaste = ds.Tables[0];
        return dtusermaste;

    }
    public int changePassCustomer()
    {
        int Ratval = 0;


        dbcommand = db.GetStoredProcCommand("[sp_change_Pass_cust]", MemberID, Phone, password);
        Ratval = db.ExecuteNonQuery(dbcommand);
        return Ratval;
    }
    #region Vehicle Detail Methods
    public void Bindloan(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT  loancatid, loancatname FROM loan_catagory WHERE flag =1");
        ddl.DataSource = dt;
        ddl.DataTextField = "loancatname";
        ddl.DataValueField = "loancatid";
        ddl.DataBind();
        ddl.Items.Insert(0,new ListItem("--select--","0"));
    }
    public void Bindloansubcategory(DropDownList ddl,string loancatid)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT  loansubcatid, loansubcatname FROM loan_Sub_Category WHERE loancatid="+loancatid+"");
        ddl.DataSource = dt;
        ddl.DataTextField = "loansubcatname";
        ddl.DataValueField = "loansubcatid";
        ddl.DataBind();
        ddl.Items.Insert(0, new ListItem("--select--", "0"));
    }
    public void BindVehicleCategory(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT  vehicle_category_id, vehicle_category FROM tbl_vehicle_category_master WHERE isActive =1 ORDER BY vehicle_category");
        DataRow dr = dt.NewRow();
        dr["vehicle_category"] = "--Select--";
        dr["vehicle_category_id"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "vehicle_category";
        ddl.DataValueField = "vehicle_category_id";
        ddl.DataBind();
    }
    public void BindColors(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT color_id,color_name FROM tbl_color_master where IsActive<>0 ORDER BY color_name");
        DataRow dr = dt.NewRow();
        dr["color_name"] = "--Select--";
        dr["color_id"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "color_name";
        ddl.DataValueField = "color_id";
        ddl.DataBind();
    }
    public void BindVehicleType(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT vehicle_type_id, vehicle_type FROM tbl_vehicle_type_master where IsActive<>0 ORDER BY vehicle_type");
        DataRow dr = dt.NewRow();
        dr["vehicle_type"] = "--Select--";
        dr["vehicle_type_id"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "vehicle_type";
        ddl.DataValueField = "vehicle_type_id";
        ddl.DataBind();
    }
    public void BindVehicleMaker(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT maker_id,maker_name  FROM tbl_vehicle_maker_master where IsActive<>0 ORDER BY maker_name");
        DataRow dr = dt.NewRow();
        dr["maker_name"] = "--Select--";
        dr["maker_id"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "maker_name";
        ddl.DataValueField = "maker_id";
        ddl.DataBind();
    }
    public void BindVehicleMakerModel(string id, DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT maker_model_id, maker_model_name FROM tbl_vehicle_maker_model_master where maker_id=" + id + " AND IsActive<>0 ORDER BY maker_model_name");
        DataRow dr = dt.NewRow();
        dr["maker_model_name"] = "--Select--";
        dr["maker_model_id"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "maker_model_name";
        ddl.DataValueField = "maker_model_id";
        ddl.DataBind();
    }
    public void BindMember(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("Select ID as Customerid,LoginID as Customermainid from Member where isactive=1 ");
        ddl.DataSource = dt;
        ddl.DataTextField = "Customermainid";
        ddl.DataValueField = "Customerid";
        ddl.DataBind();
        ddl.Items.Insert(0,new ListItem("--select--","0"));
    }
    public void BindDealers(DropDownList ddl)
    {
        DataTable dt = new DataTable();
        dt = GetDataTable("SELECT vehicle_dealer_id,dealer_name+'('+dealer_city+')' AS dealer_name1 FROM tbl_vehicle_dealer_master WHERE isActive=1 ORDER BY dealer_name,dealer_city");
        DataRow dr = dt.NewRow();
        dr["dealer_name1"] = "--Select--";
        dr["vehicle_dealer_id"] = "-1";
        dt.Rows.InsertAt(dr, 0);
        ddl.DataSource = dt;
        ddl.DataTextField = "dealer_name1";
        ddl.DataValueField = "vehicle_dealer_id";
        ddl.DataBind();
    }


    #endregion

    #region Mac Address
    public string GetMACAddress()
    {
        //NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        String sMacAddress = string.Empty;
        //foreach (NetworkInterface adapter in nics)
        //{
        //    if (sMacAddress == String.Empty)// only return MAC Address from first card  
        //    {
        //        //IPInterfaceProperties properties = adapter.GetIPProperties(); Line is not required
        //        sMacAddress = adapter.GetPhysicalAddress().ToString();
        //    }
        //}
        return sMacAddress;
    }
    #endregion

    #region hold application
    public int chkholdapp()
    {
        int retval = 0;
        DataTable dt = new DataTable();
        dt = GetDataTable("Select ishold from tbl_Setting_Company where compid=1");
        if (dt.Rows[0][0] != DBNull.Value)
        {
            retval = Convert.ToInt32(dt.Rows[0][0].ToString());
        }
        else
        { retval = 0; }
        return retval;

    }
    #endregion
}