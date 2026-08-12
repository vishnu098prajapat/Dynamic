using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text;
/// <summary>
/// Summary description for cls_myMember
/// </summary>
public class cls_myMember
{
    DataTable dtOperator = new DataTable();

    DataTable dtHistory = new DataTable();

    DataTable dtAPI = new DataTable();

    DataTable dtMix = new DataTable();

    DataTable dtCircle = new DataTable();

    cls_connection cls = new cls_connection();
    DataTable dtEWalletBalance = new DataTable();
    DataTable dtEWalletTransaction = new DataTable();


    public cls_myMember()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public string Get_Member_RecoverPass(int msrno)
    {
        string returndiv = "";

        return returndiv;
    }

    public DataTable Cyrus_GetMyLastTenTransactions(int msrno)
    {
        DataTable dt = new DataTable();
        dt = cls.select_data_dt("select top 10 *, (select OperatorName from tblRecharge_Operator where tblRecharge_Operator.OperatorID=tblRecharge_History.OperatorID) as OperatorName from tblRecharge_History where MsrNo=" + msrno.ToString() + " and Status<>'queued' order by HistoryID desc");
        return dt;
    }

    public string Get_Member_WalletInfo(int msrno)
    {
        string returndiv = "";

        return returndiv;
    }
    public string Get_Member_ResellerInfo(int msrno)
    {
        string returndiv = "";
        DataTable dt = new DataTable();
        dt = cls.select_data_dt("Select * from tblmlm_membermaster where msrno='" + msrno.ToString() + "'");
        if (dt.Rows.Count > 0)
        {
            returndiv = "<div style='width:100%; margin:0px; padding:0px; float:left;'><table width='100%' id='topopup2tbl'>";
            returndiv += "<tr><td>Member ID :</td><td>" + dt.Rows[0]["MemberID"].ToString() + "</td></tr>";
            returndiv += "<tr><td>Member Name :</td><td>" + dt.Rows[0]["FirstName"].ToString() + " " + dt.Rows[0]["Lastname"].ToString() + "</td></tr>";
            returndiv += "<tr><td>Email :</td><td>" + dt.Rows[0]["Email"] + "</td></tr>";
            //returndiv += "<tr><td>Password :</td><td>" + dt.Rows[0]["password"] + "</td></tr>";
            returndiv += "<tr><td>Website :</td><td>" + dt.Rows[0]["s_address"] + "</td></tr>";
            returndiv += "</table></div>";
        }
        else
        {
            returndiv = "<div style='width:100%; margin:0px; padding:0px; float:left;'><h2 style='text-align:center'>No result found</h2></div>";
        }
        return returndiv;
    }
    public int Wallet_Addfund(int msrno, string memberid, decimal amount, string narr, string mobile, string TransactionSource)
    {

        return 0;

    }
    public int Wallet_Deductfund(int msrno, string memberid, decimal amount, string narr, string mobile, string TransactionSource)
    {

        return 1;
    }
    public int Wallet_MakeTransaction(string memberid, decimal amount, string Factor, string narr, string TransactionSource)
    {
        return 1;
    }
    public int Cyrus_ChangeMobile(string mobile, int Msrno)
    {
        int x = cls.select_data_scalar_int("Select count(*) from tblmlm_membermaster where mobile='" + mobile + "' and msrno<>'" + Msrno.ToString() + "'");
        if (x == 0)
        {
            int i = cls.update_data("update tblMLM_MemberMaster set Mobile ='" + mobile + "' where MsrNo=" + Msrno.ToString());
            string[] valueArray = new string[1];
            valueArray[0] = mobile;
            //SMS.SendWithVar(mobile, 6, valueArray, Msrno);
            return 1;
        }
        else
        { return 0; }
    }
    public DataTable Cyrus_GetMyPackageList(int msrno)
    {
        DataTable dt = new DataTable();
        dt = cls.select_data_dt("Select PackageName,PackageID from tblmlm_package where MsrNo='" + msrno.ToString() + "' and IsActive=1 and IsDelete=0");
        return dt;
    }

    public int Cyrus_ChkEwalletBalance_BeforeTransaction(Decimal Amount, int msrno)
    {
        int mybal = 0;

        return mybal;
    }
    public int Cyrus_TransferFund(string fromMemberId, string TomemberID, int ismobile, int FromMsrno, Decimal amount, string TransactionSource, string GuiId)
    {
        string fromMobile = "", ToMobile = ""; int Tomsrno = 0; string membertypeid = "";

        DataTable dt = new DataTable();
        dt = cls.select_data_dt("Select Mobile,Memberid,membertypeid from tblmlm_membermaster where msrno='" + FromMsrno.ToString() + "'");
        if (dt.Rows.Count > 0)
        {
            fromMobile = dt.Rows[0]["Mobile"].ToString();
            fromMemberId = dt.Rows[0]["MemberID"].ToString();
            membertypeid = dt.Rows[0]["membertypeid"].ToString();
        }
        else { return 0; }
        if (ismobile == 1)
        {
            dt = cls.select_data_dt("select * from tblMLM_MemberMaster where (Mobile='" + TomemberID + "' or memberid='" + TomemberID + "') and parentmsrno='" + FromMsrno + "'");
            if (dt.Rows.Count > 0)
            {
                Tomsrno = Convert.ToInt32(dt.Rows[0]["msrno"]);
                ToMobile = dt.Rows[0]["Mobile"].ToString();
                TomemberID = dt.Rows[0]["MemberID"].ToString();
            }
            else { return 0; }
        }
        else
        {
            dt = cls.select_data_dt("select * from tblMLM_MemberMaster where memberid='" + TomemberID + "' and parentmsrno='" + FromMsrno + "'");
            if (dt.Rows.Count > 0)
            {
                Tomsrno = Convert.ToInt32(dt.Rows[0]["msrno"]);
                ToMobile = dt.Rows[0]["Mobile"].ToString();
                TomemberID = dt.Rows[0]["MemberID"].ToString();
            }
            else { return 0; }
        }

        if (string.IsNullOrEmpty(GuiId))
        {

            Wallet_MakeTransaction(fromMemberId, -Convert.ToDecimal(amount), "Dr", "Transferd to Member - " + TomemberID + " :: Via " + TransactionSource + " :: ", TransactionSource);
            Wallet_MakeTransaction(TomemberID, Convert.ToDecimal(amount), "Cr", "Receive fund from Member - " + fromMemberId + " :: Via " + TransactionSource + " :: ", TransactionSource);
            string[] valueArray = new string[2];
            valueArray[0] = ToMobile;
            valueArray[1] = amount.ToString();
            // SMS.SendWithVar(fromMobile, 5, valueArray, FromMsrno);
            DataTable dtmyWallet = new DataTable();
            //  dtmyWallet = objEWalletBalance.ManageEWalletBalance("GetBalanceByMsrNo", Tomsrno);
            //string[] valueArray1 = new string[2];
            //  valueArray1[0] = dtmyWallet.Rows[0]["Balance"].ToString();
            //  valueArray1[1] = amount.ToString();
            //   SMS.SendWithVar(ToMobile, 15, valueArray1, Tomsrno);
        }
        else
        {
            DataSet ds = new DataSet();
            ds = cls.select_data_ds("Exec [ProcMLM__HarishEWalletTransaction] '" + fromMemberId + "' , '" + TomemberID + "' , " + (Convert.ToDecimal(0) - Convert.ToDecimal(amount)) + ", " + Convert.ToDecimal(amount) + ", 'Transferd to Member - " + TomemberID + " :: Via " + TransactionSource + " :: ', 'Receive fund from Member - " + fromMemberId + " :: Via " + TransactionSource + " :: ','" + TransactionSource + "', '" + GuiId + "' ");
            if (ds.Tables.Count == 2)
            {
                if (ds.Tables[0].Rows[0][0].ToString() == "1" && ds.Tables[1].Rows[0][0].ToString() == "1")
                {
                    string[] valueArray = new string[2];
                    valueArray[0] = ToMobile;
                    valueArray[1] = amount.ToString();
                    //SMS.SendWithVar(fromMobile, 5, valueArray, FromMsrno);
                    DataTable dtmyWallet = new DataTable();
                    //  dtmyWallet = objEWalletBalance.ManageEWalletBalance("GetBalanceByMsrNo", Tomsrno);
                    string[] valueArray1 = new string[2];
                    valueArray1[0] = dtmyWallet.Rows[0]["Balance"].ToString();
                    valueArray1[1] = amount.ToString();
                    // SMS.SendWithVar(ToMobile, 15, valueArray1, Tomsrno);
                }
            }
        }
        return 1;
    }
    public string Cyrus_GetTransactionID_New()
    {
        Random rnd = new Random();
        Int64 month = rnd.Next(10000, 99999);
        month = Convert.ToInt64(month.ToString() + Convert.ToString(rnd.Next(10000, 99999)));
        int n = cls.select_data_scalar_int("Select Count(*) from tblRecharge_History where TransID='" + month.ToString() + "'");
        if (n > 0)
        {
            Cyrus_GetTransactionID_New();
        }
        return month.ToString();
    }
    public void FillDDL_Operator(DropDownList PrepaidDDL, DropDownList DTHDDL, DropDownList DataCardddl, DropDownList PostpaidDDL, DropDownList LandlineDDL, DropDownList ddlElectricity, DropDownList ddlGAS, DropDownList ddlInsurance, DropDownList ddlOtherOperator, DropDownList ddlWater, int PackageID)
    {
        //Prepaid

    }
    public void FillDDL_Circle(DropDownList ddlCirclePrepaid, DropDownList ddlCircleDatacard, DropDownList ddlCirclePostpaid, DropDownList ddlCircleLandline)
    {

    }
    public void FillDDL_Operator_B2C(DropDownList PrepaidDDL, DropDownList DTHDDL, DropDownList DataCardddl, DropDownList PostpaidDDL, DropDownList LandlineDDL, DropDownList ddlElectricity, DropDownList ddlGAS, DropDownList ddlInsurance, DropDownList ddlWater, int PackageID)
    {

    }
    public string Cyrus_apicall(string url)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(url, Method.Get);
        //request.AddHeader("Content-Type", "application/json");

        RestResponse response = client.Execute(request);
        //cls.insert_data("insert into LogsServiceData(URL,Result,Status) Values('" + url + "','" + response.Content + "','Hit')");

        return (response.Content);
    }
    public double Cyrus_getbalanceofAPI(int apiidno)
    {
        double returnvalue = 0;

        return returnvalue;
    }
    public string Cyrus_RechargeProcess_Ravi(int HistoryID, int MsrNo, string MobileNo, string caNumber, decimal RechargeAmount, int OperatorID, int CircleID, string TransID, string Cycle, string DueDate, string Status, string IPAddress, string MacAddress, string CustomerName, string CustomerNumber, string account, string TransactionSource, string CoupanCodeStr, int CoupanAmount, int SurchargeAmount, string promocode)
    {
        string returnStr = "";
        cls_connection ObjData = new cls_connection();
        int msrno = 0;
        decimal amount = 0;
        int id = 0;
        try
        {
            DataTable DtUrl = new DataTable();
            DtUrl = cls.select_data_dt("EXEC [ProcMLM_AddRecharge_Multiple] 0 ," + MsrNo + ",'" + MobileNo + "','" + caNumber + "'," + RechargeAmount + "," + OperatorID + "," + CircleID + ",'" + TransID + "' ,'" + Cycle + "','" + DueDate + "','Queued','" + IPAddress + "','" + MacAddress + "','" + CustomerName + "','" + CustomerNumber + "','" + account + "', '" + TransactionSource + "','" + CoupanCodeStr + "'," + CoupanAmount + "," + SurchargeAmount + ", '" + promocode + "'");

            if (DtUrl.Rows[0]["Yes"].ToString() == "Yes")
            {
                if (DtUrl.Rows[0]["AuthAPI"].ToString() != "")
                {
                    string AuthAPI = DtUrl.Rows[0]["AuthAPI"].ToString();
                    for (Int32 i = 0; i < AuthAPI.Split(',').Length; i++)
                    {
                        DtUrl = cls.select_data_dt("EXEC [ProcMLM_AddRecharge_MakeURL] 0 ," + MsrNo + ",'" + MobileNo + "','" + caNumber + "'," + RechargeAmount + "," + OperatorID + "," + CircleID + ",'" + TransID + "' ,'" + Cycle + "','" + DueDate + "','Queued','" + IPAddress + "','" + MacAddress + "','" + CustomerName + "','" + CustomerNumber + "','" + account + "', '" + TransactionSource + "','" + CoupanCodeStr + "'," + CoupanAmount + "," + SurchargeAmount + ", '" + promocode + "','" + AuthAPI.Split(',')[i].ToString() + "','" + DtUrl.Rows[0]["TransIDAUth"].ToString() + "'");
                        string ilast = (i == (AuthAPI.Split(',').Length - 1) ? "Last" : "Fisrt");
                        string Result = GetProcessResult(DtUrl, ilast);
                        string[] split = Result.Split(',');
                        if (split[0] == "Recharge Successful !!")
                        {
                            returnStr = Result;
                            break;
                        }
                        else if (split[0] == "Request Submited !!" || split[0]=="Request Submitted !!")
                        {
                            returnStr = Result;
                            break;
                        }
                        else if (ilast == "Last")
                        {
                            returnStr = Result;
                            break;
                        }
                    }
                }
                else
                {
                    returnStr = GetProcessResult(DtUrl, "Last");
                }
            }
            else
            {
                string str = DtUrl.Rows[0]["Error"].ToString();
                return str;
            }
        }
        catch (Exception ex)
        {
            //  dtHistory = objHistory.ManageHistory("Get", id);
            if (dtHistory.Rows.Count > 0)
            {
                if (dtHistory.Rows[0]["status"].ToString().ToLower() == "queued")
                {
                    //       DataTable i = objHistory.UpdateHistory("UpdateStatus", id, 0, 0, 0, 0, 0, "", "", "Pending", "", "Exception", "");
                    returnStr = "Request Submited !!," + id.ToString() + ",,,";
                }
                try
                {
                    cls.insert_data("insert into tbl_OrderMsg values (1, '" + ex.Message + "','" + msrno + "','" + amount + "',1,1,'" + id.ToString() + "','','','','',0,0,1,getdate(),getdate())");
                }
                catch { }
            }
        }
        return returnStr;
    }
    private string GetProcessResult(DataTable DtUrl, string Last)
    {
        StringBuilder returnStr = new StringBuilder();
        cls_connection ObjData = new cls_connection();
        int id = Convert.ToInt32(DtUrl.Rows[0]["Historyid"]);
        int msrno = Convert.ToInt32(DtUrl.Rows[0]["msrno"]);
        decimal amount = Convert.ToDecimal(DtUrl.Rows[0]["amount"]);
        string TransID = DtUrl.Rows[0]["transid"].ToString();


        DataTable dtAPI = ObjData.select_data_dt($"SELECT * FROM tblRecharge_API WHERE apiid={DtUrl.Rows[0]["APIID"]}");
        string Success = dtAPI.Rows[0]["Success"].ToString().ToLower();
        string Failed = dtAPI.Rows[0]["Failed"].ToString().ToLower();
        string Pending = dtAPI.Rows[0]["Pending"].ToString().ToLower();


        string result = "";
        string APIIDNO = DtUrl.Rows[0]["APIID"].ToString();

        string Status = "", TxID = "", OperatorRef = "", ErrorCode = "";

        result = Cyrus_apicall(DtUrl.Rows[0]["URL"].ToString());

        try
        {
            cls.update_data("insert into LogsServiceData(ServiceID,APIID,TransID,RequestData,ResponsetData)values('" + DtUrl.Rows[0]["SID"].ToString() + "','" + DtUrl.Rows[0]["APIID"].ToString() + "','" + DtUrl.Rows[0]["Transid"].ToString() + "','" + DtUrl.Rows[0]["URL"].ToString() + "','" + result + "')");
        }
        catch (Exception ex)
        {

        }
        string TypeData = dtAPI.Rows[0]["prm9val"].ToString();


        if (TypeData == "XML")
        {
            JObject ObjDataXMLKey = JObject.Parse(result);
            string[] xmlKetBala = dtAPI.Rows[0]["XMlKeyRecharge"].ToString().Split(',');//Recharge_Credit,Status
            JObject ObjDataPOST = null;
            for (int i = 0; i < xmlKetBala.Length; i++)
            {
                ObjDataPOST = JObject.Parse(ObjDataXMLKey["" + xmlKetBala[i] + ""].ToString());
                ObjDataXMLKey = JObject.Parse(ObjDataXMLKey["" + xmlKetBala[i] + ""].ToString());
            }


            Status = ObjDataPOST["" + dtAPI.Rows[0]["StatusPosition"].ToString() + ""].ToString();
            ErrorCode = ObjDataPOST["" + dtAPI.Rows[0]["ErrorCodePosition"].ToString() + ""].ToString();
            if (Status.ToString().ToLower() != Failed.ToLower())
            {
                TxID = ObjDataPOST["" + dtAPI.Rows[0]["TxIDPosition"].ToString() + ""].ToString();
                OperatorRef = ObjDataPOST["" + dtAPI.Rows[0]["OperatorRefPosition"].ToString() + ""].ToString();
            }
        }
        else if (TypeData == "JSON")
        {
            JObject ObjDataPOST = null;
            ObjDataPOST = JObject.Parse(result);
            if (dtAPI.Rows[0]["XMlKeyRecharge"].ToString() != "")
            {
                string[] xmlKetBala = dtAPI.Rows[0]["XMlKeyRecharge"].ToString().Split(',');

                for (int i = 0; i < xmlKetBala.Length; i++)
                {
                    ObjDataPOST = JObject.Parse(ObjDataPOST["" + xmlKetBala[i] + ""].ToString());
                }
            }
            Status = ObjDataPOST["" + dtAPI.Rows[0]["StatusPosition"].ToString() + ""].ToString().ToLower();
            if (Status.ToString().ToLower() != "failed")
            {
                TxID = Convert.ToString(ObjDataPOST["" + dtAPI.Rows[0]["TxIDPosition"].ToString() + ""]);
                OperatorRef = Convert.ToString(ObjDataPOST["" + dtAPI.Rows[0]["OperatorRefPosition"].ToString() + ""]);
            }
            ErrorCode = Convert.ToString(ObjDataPOST["" + dtAPI.Rows[0]["ErrorCodePosition"].ToString() + ""]);
        }
        else if (TypeData == "CSV")
        {
            char Splitter = Convert.ToChar(dtAPI.Rows[0]["Splitter"]);
            string[] split = result.Split(Splitter);

            Status = split.ElementAtOrDefault(Convert.ToInt32(dtAPI.Rows[0]["StatusPosition"]))?.ToLower();
            ErrorCode = split.ElementAtOrDefault(Convert.ToInt32(dtAPI.Rows[0]["ErrorCodePosition"]));
            TxID = split.ElementAtOrDefault(Convert.ToInt32(dtAPI.Rows[0]["TxIDPosition"]));
            OperatorRef = split.ElementAtOrDefault(Convert.ToInt32(dtAPI.Rows[0]["OperatorRefPosition"]));
        }

        if (Status == Success)
        {
            cls.select_data_dt($"ProcRecharge_UpdateHistory 'UpdateStatus', {id}, {msrno}, 0, 0, 0, 0, '', '', 'Success', '{TxID}', '', '{OperatorRef}'");
            returnStr.Append($"Recharge Successful !!, {id}, {ErrorCode}, {OperatorRef}, {TransID},");
        }

        if (Status.ToLower() == Success.ToLower())
        {
            DataTable i = cls.select_data_dt("ProcRecharge_UpdateHistory 'UpdateStatus'," + id + "," + Convert.ToInt32(msrno) + ", 0, 0, 0, 0, '', '', 'Success','" + TxID + "', '','" + OperatorRef + "'");
            returnStr.Append($"Recharge Successful !!, {id}, {ErrorCode}, {OperatorRef}, {TransID},");
        }
        else if (Status == Failed || Status == "103" || Status.Contains("last 3 Hour"))
        {
            if (Last.ToLower() == "last")
            {
                cls.select_data_dt($"ProcRecharge_UpdateHistory 'UpdateStatus', {id}, 0, 0, 0, 0, 0, '', '', 'Failed', '{TxID}', '{ErrorCode}', '{OperatorRef}'");
                string MemberID = cls.select_data_scalar_string($"SELECT loginid FROM member WHERE msrno={msrno}");
                string Narration = cls.select_data_scalar_string($"SELECT Narration FROM TBL_EWALLETTRANSACTION WHERE TransactionID='{TransID}'");
                cls.update_data($"PROC_EWALLETTRANSACTION '{MemberID}', {amount}, 'CR', 'Refund {Narration}', '', {ConstantsData.SRecharge}, '{TransID}'");
            }
            returnStr.Append($"Recharge Failed !!, {id}, {ErrorCode}, {OperatorRef}, {TransID},");
        }
        else if (Status == Pending)
        {
            cls.select_data_dt($"ProcRecharge_UpdateHistory 'UpdateStatus', {id}, 0, 0, 0, 0, 0, '', '', 'Pending', '{TxID}', '{ErrorCode}', '{OperatorRef}'");
            returnStr.Append($"Request Submited !!, {id}, {ErrorCode}, {OperatorRef}, {TransID},");
        }
        else
        {
            cls.select_data_dt($"ProcRecharge_UpdateHistory 'UpdateStatus', {id}, 0, 0, 0, 0, 0, '', '', 'Pending', '{TxID}', '{ErrorCode}', '{OperatorRef}'");
            returnStr.Append($"Request Submited !!, {id}, {ErrorCode}, {OperatorRef}, {TransID},");
        }


        return returnStr.ToString();
    }

    //public string Cyrus_RechargeProcess_Ravi(int HistoryID, int MsrNo, string MobileNo, string caNumber, decimal RechargeAmount, int OperatorID, int CircleID, string TransID, string Cycle, string DueDate, string Status, string IPAddress, string MacAddress, string CustomerName, string CustomerNumber, string account, string TransactionSource, string CoupanCodeStr, int CoupanAmount, int SurchargeAmount, string promocode)
    //{
    //    string returnStr = "";
    //    cls_connection ObjData = new cls_connection();
    //    int msrno = 0;
    //    decimal amount = 0;
    //    int id = 0;
    //    try
    //    {
    //        DataTable DtUrl = new DataTable();
    //        DtUrl = cls.select_data_dt("EXEC [ProcMLM_AddRecharge] 0 ," + MsrNo + ",'" + MobileNo + "','" + caNumber + "'," + RechargeAmount + "," + OperatorID + "," + CircleID + ",'" + TransID + "' ,'" + Cycle + "','" + DueDate + "','Queued','" + IPAddress + "','" + MacAddress + "','" + CustomerName + "','" + CustomerNumber + "','" + account + "', '" + TransactionSource + "','" + CoupanCodeStr + "'," + CoupanAmount + "," + SurchargeAmount + ", '" + promocode + "'");

    //        if (DtUrl.Rows[0]["Yes"].ToString() == "Yes")
    //        {
    //            id = Convert.ToInt32(DtUrl.Rows[0]["Historyid"].ToString());
    //            msrno = Convert.ToInt32(DtUrl.Rows[0]["msrno"].ToString());
    //            amount = Convert.ToDecimal(DtUrl.Rows[0]["amount"].ToString());
    //            TransID = DtUrl.Rows[0]["transid"].ToString();
    //            if (Convert.ToInt32(DtUrl.Rows[0]["APIID"]) == 0)
    //            {
    //                returnStr = "Bill Payment Successful !!," + id.ToString();
    //            }
    //            else
    //            {
    //                dtAPI = ObjData.select_data_dt("Select * from tblRecharge_API where apiid=" + DtUrl.Rows[0]["APIID"] + ""); // objAPI.ManageAPI("Get", Convert.ToInt32(DtUrl.Rows[0]["APIID"])); comment by vipin
    //                string Success = Convert.ToString(dtAPI.Rows[0]["Success"]);
    //                string Failed = Convert.ToString(dtAPI.Rows[0]["Failed"]);
    //                string Pending = Convert.ToString(dtAPI.Rows[0]["Pending"]);

    //                var TxID = "";
    //                var OperatorRef = "";
    //                var ErrorCode = "";
    //                string result = "";
    //                string APIIDNO = DtUrl.Rows[0]["APIID"].ToString();
    //                if (OperatorID == 178) // only for Offline Pancard service
    //                {
    //                    result = "1";
    //                    Status = "Pending";
    //                }
    //                else
    //                {
    //                    result = Cyrus_apicall(DtUrl.Rows[0]["URL"].ToString());
    //                }



    //                string TypeData = ObjData.select_data_scalar_string("select prm9val from tblRecharge_API where APIID='" + DtUrl.Rows[0]["APIID"].ToString() + "'");
    //                if (TypeData == "XML")
    //                {

    //                    // result = XMLTOJOSN.XmlToJSON(result);
    //                    XmlDocument doc = new XmlDocument();
    //                    doc.LoadXml(result);
    //                    result = JsonConvert.SerializeXmlNode(doc);

    //                    //                        XMlKeyRecharge
    //                    JObject ObjDataXMLKey = JObject.Parse(result);
    //                    // JObject ObjDataPOST = JObject.Parse(ObjDataXMLKey["" + dtAPI.Rows[0]["XMlKeyRecharge"].ToString() + ""].ToString());

    //                    string[] xmlKetBala = dtAPI.Rows[0]["XMlKeyRecharge"].ToString().Split(',');//Recharge_Credit,Status
    //                    JObject ObjDataPOST = null;
    //                    for (int i = 0; i < xmlKetBala.Length; i++)
    //                    {
    //                        ObjDataPOST = JObject.Parse(ObjDataXMLKey["" + xmlKetBala[i] + ""].ToString());
    //                        ObjDataXMLKey = JObject.Parse(ObjDataXMLKey["" + xmlKetBala[i] + ""].ToString());
    //                    }


    //                    Status = ObjDataPOST["" + dtAPI.Rows[0]["StatusPosition"].ToString() + ""].ToString();
    //                    Status = ObjData.select_data_scalar_string("SELECT CODEMESS FROM TBL_API_STATUS_ERROR_CODE WHERE ERRORCODE='" + Status + "' and APIID='" + DtUrl.Rows[0]["APIID"].ToString() + "'");
    //                    ErrorCode = ObjDataPOST["" + dtAPI.Rows[0]["ErrorCodePosition"].ToString() + ""].ToString();
    //                    if (Status.ToString().ToLower() != Failed.ToLower())
    //                    {
    //                        TxID = ObjDataPOST["" + dtAPI.Rows[0]["TxIDPosition"].ToString() + ""].ToString();
    //                        OperatorRef = ObjDataPOST["" + dtAPI.Rows[0]["OperatorRefPosition"].ToString() + ""].ToString();
    //                    }
    //                }
    //                else if (TypeData == "JSON")
    //                {
    //                    JObject ObjDataPOST = null;
    //                    ObjDataPOST = JObject.Parse(result);
    //                    if (dtAPI.Rows[0]["XMlKeyRecharge"].ToString() != "")
    //                    {
    //                        string[] xmlKetBala = dtAPI.Rows[0]["XMlKeyRecharge"].ToString().Split(',');

    //                        for (int i = 0; i < xmlKetBala.Length; i++)
    //                        {
    //                            ObjDataPOST = JObject.Parse(ObjDataPOST["" + xmlKetBala[i] + ""].ToString());
    //                        }
    //                    }
    //                    Status = ObjDataPOST["" + dtAPI.Rows[0]["StatusPosition"].ToString() + ""].ToString();
    //                    //  Status = ObjData.select_data_scalar_string("SELECT CODEMESS FROM TBL_API_STATUS_ERROR_CODE WHERE ERRORCODE='" + Status + "' and APIID='" + DtUrl.Rows[0]["APIID"].ToString() + "'");
    //                    if (Status.ToString().ToLower() != "failed")
    //                    {
    //                        TxID = ObjDataPOST["" + dtAPI.Rows[0]["TxIDPosition"].ToString() + ""].ToString();
    //                        OperatorRef = ObjDataPOST["" + dtAPI.Rows[0]["OperatorRefPosition"].ToString() + ""].ToString();
    //                    }
    //                    ErrorCode = ObjDataPOST["" + dtAPI.Rows[0]["ErrorCodePosition"].ToString() + ""].ToString();
    //                }
    //                else if (TypeData == "CSV")
    //                {
    //                    char Splitter = Convert.ToChar(dtAPI.Rows[0]["Splitter"]);
    //                    string[] split = result.Split(Splitter);
    //                    if (Convert.ToString(dtAPI.Rows[0]["TxIDPosition"]) != "")
    //                    {
    //                        try
    //                        {
    //                            int TxIDPosition = Convert.ToInt32(dtAPI.Rows[0]["TxIDPosition"]);
    //                            if (TxIDPosition <= split.Length)
    //                                TxID = split[TxIDPosition];
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                        }
    //                    }
    //                    if (Convert.ToString(dtAPI.Rows[0]["StatusPosition"]) != "")
    //                    {
    //                        try
    //                        {
    //                            int StatusPosition = Convert.ToInt32(dtAPI.Rows[0]["StatusPosition"]);
    //                            if (StatusPosition <= split.Length)
    //                                Status = split[StatusPosition];
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                        }
    //                    }
    //                    if (Convert.ToString(dtAPI.Rows[0]["OperatorRefPosition"]) != "")
    //                    {
    //                        try
    //                        {
    //                            int OperatorRefPosition = Convert.ToInt32(dtAPI.Rows[0]["OperatorRefPosition"]);
    //                            if (OperatorRefPosition <= split.Length)
    //                                OperatorRef = split[OperatorRefPosition];
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                        }
    //                    }
    //                    if (Convert.ToString(dtAPI.Rows[0]["ErrorCodePosition"]) != "")
    //                    {
    //                        try
    //                        {
    //                            int ErrorCodePosition = Convert.ToInt32(dtAPI.Rows[0]["ErrorCodePosition"]);
    //                            if (ErrorCodePosition <= split.Length)
    //                                ErrorCode = split[ErrorCodePosition];
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                        }
    //                    }
    //                }
    //                ObjData.update_data("INSERT INTO RECHARGEURL(URL,TRANSID)VALUES('" + Status + "','Vipin') ");
    //                if (Status.ToLower() == Success.ToLower())
    //                {
    //                    DataTable i = cls.select_data_dt("ProcRecharge_UpdateHistory 'UpdateStatus'," + id + "," + Convert.ToInt32(msrno) + ", 0, 0, 0, 0, '', '', 'Success','" + TxID + "', '','" + OperatorRef + "'");
    //                    returnStr = "Recharge Successful !!," + id.ToString() + "," + ErrorCode + "," + OperatorRef + "," + TransID + ",";
    //                }
    //                else if (Status.ToLower() == Failed.ToLower() || Status.ToLower() == "103" || Status.IndexOf("last 3 Hour") > 0)
    //                {
    //                    try
    //                    {
    //                        DataTable i = cls.select_data_dt("ProcRecharge_UpdateHistory 'UpdateStatus'," + id + ", 0, 0, 0, 0, 0, '','', 'Failed', '" + TxID + "','" + ErrorCode + "','" + OperatorRef + "'");
    //                    }
    //                    catch (Exception ex) { }
    //                    string MemberID = cls.select_data_scalar_string("select loginid from member where msrno=" + msrno + "");
    //                    string Narration = cls.select_data_scalar_string("select Narration from TBL_EWALLETTRANSACTION where TransactionID='" + TransID + "'");
    //                    cls.update_data("PROC_EWALLETTRANSACTION '" + MemberID + "'," + amount + ",'CR','" + ("Refund " + Narration) + "',''," + ConstantsData.SRecharge + ",'" + TransID + "'");
    //                    returnStr = "Recharge Failed !!," + id.ToString() + "," + ErrorCode + "," + OperatorRef + "," + TransID + ",";
    //                }
    //                else if (Status.ToLower() == Pending.ToLower())
    //                {

    //                    DataTable i = cls.select_data_dt("ProcRecharge_UpdateHistory 'UpdateStatus'," + id + ", 0, 0, 0, 0, 0, '', '','Pending','" + TxID + "','" + ErrorCode + "','" + OperatorRef + "'");
    //                    returnStr = "Request Submited !!," + id.ToString() + "," + ErrorCode + "," + OperatorRef + "," + TransID + ",";
    //                }
    //                else
    //                {
    //                    DataTable i = cls.select_data_dt("ProcRecharge_UpdateHistory 'UpdateStatu'," + id + ", 0, 0, 0, 0, 0, '', '','Pending','" + TxID + "','" + ErrorCode + "','" + OperatorRef + "'");

    //                    // DataTable i = objHistory.UpdateHistory("UpdateStatus", id, 0, 0, 0, 0, 0, "", "", "Pending", TxID, ErrorCode, OperatorRef);
    //                    returnStr = "Request Submited !!," + id.ToString() + "," + ErrorCode + "," + OperatorRef + "," + TransID + ",";
    //                }

    //            }
    //        }
    //        else
    //        {
    //            string str = DtUrl.Rows[0]["Error"].ToString();
    //            return str;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        //  dtHistory = objHistory.ManageHistory("Get", id);
    //        if (dtHistory.Rows.Count > 0)
    //        {
    //            if (dtHistory.Rows[0]["status"].ToString().ToLower() == "queued")
    //            {
    //                //       DataTable i = objHistory.UpdateHistory("UpdateStatus", id, 0, 0, 0, 0, 0, "", "", "Pending", "", "Exception", "");
    //                returnStr = "Request Submited !!," + id.ToString() + ",,,";
    //            }
    //            try
    //            {
    //                cls.insert_data("insert into tbl_OrderMsg values (1, '" + ex.Message + "','" + msrno + "','" + amount + "',1,1,'" + id.ToString() + "','','','','',0,0,1,getdate(),getdate())");
    //            }
    //            catch { }
    //        }
    //    }
    //    return returnStr;
    //}


    public DataTable createRechargeDataTable(string MobileNo, string caNumber, decimal RechargeAmount, int OperatorID, string OperatorName, int CircleID, string Cycle, string DueDate, string Status, string IPAddress, string MacAddress, string CustomerName, string CustomrtNumber, Double SurchargeAmount, String ServiceType)
    {
        DataTable kDt = new DataTable();
        kDt.Columns.Add("MobileNo");
        kDt.Columns.Add("caNumber");
        kDt.Columns.Add("RechargeAmount");
        kDt.Columns.Add("OperatorID");
        kDt.Columns.Add("OperatorName");
        kDt.Columns.Add("CircleID");
        kDt.Columns.Add("Cycle");
        kDt.Columns.Add("DueDate");
        kDt.Columns.Add("Status");
        kDt.Columns.Add("IPAddress");
        kDt.Columns.Add("MacAddress");
        kDt.Columns.Add("CustomerName");
        kDt.Columns.Add("CustomrtNumber");
        kDt.Columns.Add("SurchargeAmount");
        kDt.Columns.Add("ServiceType");
        kDt.Rows.Add(MobileNo, caNumber, RechargeAmount, OperatorID, OperatorName, CircleID, Cycle, DueDate, Status, IPAddress, MacAddress, CustomerName, CustomrtNumber, SurchargeAmount, ServiceType);
        return kDt;
    }
    public string RechargePendingProcess(int historyId, string TransactionSource)
    {
        var status = ""; string error_msg = ""; string APItxnid = ""; string OperatorRef = ""; string TxID = "";
        dtHistory = cls.select_data_dt("select * from tblrecharge_history where id=" + historyId + " and Status in ('Pending','Queued')");
        if (dtHistory.Rows.Count > 0)
        {
            int serviceType = Convert.ToInt32(dtHistory.Rows[0]["ServiceID"]);
            dtAPI = cls.select_data_dt("select * from tblRecharge_API where apiid=" + dtHistory.Rows[0]["APIID"] + "");
            string StatusPosition = Convert.ToString(dtAPI.Rows[0]["S_StatusPosition"]);
            string Success = Convert.ToString(dtAPI.Rows[0]["Success"]);
            string Failed = Convert.ToString(dtAPI.Rows[0]["Failed"]);

            if (dtAPI.Rows[0]["StatusURL"].ToString() != "")
            {
                string str = "";
                if (dtAPI.Rows[0]["S_prm1"].ToString() != "" && dtAPI.Rows[0]["S_prm1val"].ToString() != "")
                {
                    str = dtAPI.Rows[0]["StatusURL"].ToString() + dtAPI.Rows[0]["S_prm1"].ToString() + "=" + dtAPI.Rows[0]["S_prm1val"].ToString() + "&";
                }
                if (dtAPI.Rows[0]["S_prm2"].ToString() != "" && dtAPI.Rows[0]["S_prm2val"].ToString() != "")
                {
                    str = str + dtAPI.Rows[0]["S_prm2"].ToString() + "=" + dtAPI.Rows[0]["S_prm2val"].ToString() + "&";
                }
                if (dtAPI.Rows[0]["S_prm3"].ToString() != "")
                {
                    str = str + dtAPI.Rows[0]["S_prm3"].ToString() + "=" + Convert.ToString(dtHistory.Rows[0]["TransID"]);
                }
                if (serviceType == 5)
                {
                    str = str + "&ServiceType=DH";
                }
                else
                {
                    str = str + "&ServiceType=MR";
                }

                if (str.EndsWith("&"))
                    str = str.Substring(0, str.Length - 1);

                string result = Cyrus_apicall(str);
                string TypeData = cls.select_data_scalar_string("select S_prm4 from tblRecharge_API where APIID='" + dtHistory.Rows[0]["APIID"].ToString() + "'");
                if (TypeData == "XML")
                {

                }
                else if (TypeData == "JSON")
                {
                    JObject ObjDataPOST = null;
                    ObjDataPOST = JObject.Parse(result);
                    if (dtAPI.Rows[0]["XMlKeyRecharge"].ToString() != "")
                    {
                        string[] xmlKetBala = dtAPI.Rows[0]["XMlKeyRecharge"].ToString().Split(',');

                        for (int i = 0; i < xmlKetBala.Length; i++)
                        {
                            ObjDataPOST = JObject.Parse(ObjDataPOST["" + xmlKetBala[i] + ""].ToString());
                        }
                    }
                    status = ObjDataPOST["" + dtAPI.Rows[0]["S_StatusPosition"].ToString() + ""].ToString();
                    //  Status = ObjData.select_data_scalar_string("SELECT CODEMESS FROM TBL_API_STATUS_ERROR_CODE WHERE ERRORCODE='" + Status + "' and APIID='" + DtUrl.Rows[0]["APIID"].ToString() + "'");
                    if (status.ToString().ToLower() != "failed")
                    {
                        TxID = ObjDataPOST["" + dtAPI.Rows[0]["S_ApiTransPosition"].ToString() + ""].ToString();
                        OperatorRef = ObjDataPOST["" + dtAPI.Rows[0]["S_OpraterRefPosition"].ToString() + ""].ToString();
                    }
                    error_msg = "FAILURE";
                }
                else
                {
                    char Splitter = Convert.ToChar(dtAPI.Rows[0]["Splitter"]);
                    string[] split = result.Split(Splitter);
                    status = split[0];//StatusPosition
                    if (status.ToLower() == Failed.ToLower() || status.ToLower() == "reversed")
                    {
                        error_msg = "FAILURE";
                        APItxnid = Convert.ToString(dtHistory.Rows[0]["APITransID"]);
                    }
                }

            }
            //Success,BR0004YD8ZI5,1A4D98F545,,7/21/2020 4:47:38 PM,
            if (status.ToLower() == Success.ToLower())
            {
                DataTable i = cls.select_data_dt("ProcRecharge_UpdateHistory 'UpdateStatus'," + historyId + "," + Convert.ToInt32(dtHistory.Rows[0]["MemberID"].ToString()) + ", 0, 0, 0, 0, '', '', 'Success','" + TxID + "', '','" + OperatorRef + "'");
            }
            else if (status.ToLower() == Failed.ToLower() || status.ToLower() == "reversed" || status.ToLower() == "refunded" || status.ToLower() == "invalid" || status.IndexOf("last 3 Hour") > 0)
            {

                DataTable i = cls.select_data_dt("ProcRecharge_UpdateHistory 'UpdateStatus'," + historyId + ", 0, 0, 0, 0, 0, '','', 'Failed', '" + TxID + "','" + error_msg + "','" + OperatorRef + "'");

                string MemberID = cls.select_data_scalar_string("select loginid from member where msrno=" + dtHistory.Rows[0]["MemberID"].ToString() + "");
                string Narration = cls.select_data_scalar_string("select Narration from TBL_EWALLETTRANSACTION where TransactionID='" + dtHistory.Rows[0]["TransID"].ToString() + "'");
                cls.update_data("PROC_EWALLETTRANSACTION '" + MemberID + "'," + dtHistory.Rows[0]["Amount"].ToString() + ",'CR','" + ("Refund " + Narration) + "',''," + ConstantsData.SRecharge + ",'" + dtHistory.Rows[0]["TransID"].ToString() + "'");
            }
        }

        return status;
    }
    public void Cyrus_Recharge_ForceFailed(int historyId, string TransactionSource)
    {
        // dtHistory = objHistory.ManageHistory("GetAll", historyId);
        DataTable dtRefund = new DataTable();
        dtRefund = cls.select_data_dt("exec Cyrus_Recharge_Refund_ravi '" + historyId + "'");
        if (dtRefund.Rows.Count > 0)
        {
            if (!string.IsNullOrEmpty(dtRefund.Rows[0][0].ToString()))
            {
                if (Convert.ToInt32(dtRefund.Rows[0][0]) > 0)
                {
                    // DataTable i = objHistory.UpdateHistory("UpdateStatus", historyId, 0, 0, 0, 0, 0, "", "", "Failed", "", "Foced Failed", "");
                }
            }
        }

        try
        {
            #region[Send SMS]
            string mobile = dtHistory.Rows[0]["Mobileno"].ToString();
            string Amount = dtHistory.Rows[0]["RechargeAmount"].ToString();
            string transid = dtHistory.Rows[0]["TransID"].ToString();
            int MsrNo = Convert.ToInt32(dtHistory.Rows[0]["MsrNo"].ToString());

            // Send SMS to recharged mobile number means customers
            if (mobile.Length == 10)
            {
                string[] ValueArray = new string[3];
                ValueArray[0] = mobile;
                ValueArray[1] = Amount;
                ValueArray[2] = transid;
                //  SMS.SendWithVar(mobile, 3, ValueArray, MsrNo);
            }

            // Send SMS to Users whose make recharge
            string userMobileNo = cls.select_data_scalar_string("Select mobile from tblmlm_membermaster Where MsrNo=" + MsrNo + "");
            if (userMobileNo != mobile)
            {
                string[] valueArray = new string[3];
                valueArray[0] = mobile;
                valueArray[1] = Amount;
                valueArray[2] = transid;
                //  SMS.SendWithVar(userMobileNo, 3, valueArray, MsrNo);
            }
            #endregion
        }
        catch { }
    }
    public void Cyrus_Recharge_ForceSuccess(int historyId)
    {
        //   dtHistory = objHistory.ManageHistory("GetAll", historyId);
        //  DataTable i = objHistory.UpdateHistory("UpdateStatus", historyId, Convert.ToInt32(dtHistory.Rows[0]["MsrNo"]), 0, 0, 0, 0, "", "", "Success", "", "", "Force Success");

        try
        {
            #region[Send SMS]
            string mobile = dtHistory.Rows[0]["Mobileno"].ToString();
            string Amount = dtHistory.Rows[0]["RechargeAmount"].ToString();
            string transid = dtHistory.Rows[0]["TransID"].ToString();
            int MsrNo = Convert.ToInt32(dtHistory.Rows[0]["MsrNo"].ToString());

            // Send SMS to recharged mobile number means customers
            if (mobile.Length == 10)
            {
                string[] ValueArray = new string[3];
                ValueArray[0] = mobile;
                ValueArray[1] = Amount;
                ValueArray[2] = transid;
                //   SMS.SendWithVar(mobile, 2, ValueArray, MsrNo);
            }

            // Send SMS to Users whose make recharge
            DataTable dtMemberMaster = new DataTable();
            dtMemberMaster = cls.select_data_dt("Select * from tblmlm_membermaster Where MsrNo=" + MsrNo + "");
            if (dtMemberMaster.Rows[0]["Mobile"].ToString() != mobile)
            {
                string[] valueArray = new string[4];
                valueArray[0] = dtMemberMaster.Rows[0]["FirstName"].ToString();
                valueArray[1] = mobile;
                valueArray[2] = Amount;
                valueArray[3] = transid;
                //  SMS.SendWithVar(dtMemberMaster.Rows[0]["Mobile"].ToString(), 33, valueArray, MsrNo);
            }
            #endregion
        }
        catch { }
    }
    public static string GetIPAddress()
    {
        string IPAddress = string.Empty;
        string SearchName = string.Empty;
        String strHostName = HttpContext.Current.Request.UserHostAddress.ToString();
        try
        {
            IPAddress = System.Net.Dns.GetHostAddresses(strHostName).GetValue(0).ToString();
            return IPAddress;
        }
        catch
        {
            return strHostName;
        }
    }

    public static string GetMACAddress()
    {
        String sMacAddress = string.Empty;
        try
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
        }
        catch { sMacAddress = string.Empty; }
        return sMacAddress;
    }
}