using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for CheckStatusService
/// </summary>
public class CheckStatusService : ICheckStatusService
{
    cls_connection cls;
    IWallet wallet;
    ICommissionService commissionService;
    public CheckStatusService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        commissionService = new CommissionService();
    }

    public async Task<DataTable> GetUPIDetails(string OrderID)
    {
        return await cls.select_data_dtAsync("select * from FundAdd_UPI_Virtaul_Transaction where order_id='" + OrderID + "'");
    }

    public async Task<string> CheckStatusUPICollection(string OrderID)
    {
        DataTable dthis = await GetUPIDetails(OrderID);
        if (dthis.Rows.Count > 0)
        {
            String Result = "";

            Result = await tezindia.PostCallCheckStatus(OrderID, "");


            if (!string.IsNullOrEmpty(Result))
            {
                JObject jResult = JObject.Parse(Result);
                if (Convert.ToBoolean(jResult["status"]))
                {
                    if (jResult["result"]["txnStatus"].ToString().ToUpper() == "SUCCESS")
                    {
                        string amount = jResult["result"]["amount"].ToString();
                        string utr = jResult["result"]["utr"].ToString();
                        string customer_mobile = jResult["result"]["customer_mobile"].ToString();
                        IWallet wallet = new Wallet();
                        var _memberDetails = await wallet.GetMemberAllowPermissionDetails(dthis.Rows[0]["msrno"].ToString());
                        Int32 Val = await cls.update_dataAsync("update FundAdd_UPI_Virtaul_Transaction set bank_ref_num='" + utr + "',Status='Success' where order_id='" + OrderID + "'");
                        if (Val > 0)
                        {
                            string narration = $"UPI/{amount}/{utr}/{customer_mobile}";
                            string Dec = "";
                            await cls.update_dataAsync("exec PROC_EWALLETTRANSACTION '" + _memberDetails.LoginID + "','" + (Convert.ToDecimal(amount)) + "','Cr','" + narration + "','" + Dec + "',19,'" + utr + "',0,0,0,'" + dthis.Rows[0]["Surcharge"].ToString() + "','" + dthis.Rows[0]["GST"].ToString() + "'");

                        }
                    }
                    else if (jResult["result"]["txnStatus"].ToString().ToUpper() == "FAILURE")
                    {
                        Int32 Val = await cls.update_dataAsync("update FundAdd_UPI_Virtaul_Transaction set bank_ref_num='" + jResult["message"].ToString() + "',Status='Failed' where order_id='" + OrderID + "'");
                        if (Val > 0)
                        {

                        }
                    }
                    return ErrorShow.SuccessMessageAPI(jResult["message"].ToString());
                }
                else
                {
                    return Result;
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Response Blank");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Transaction not found");
        }
    }

    public async Task<int> ReviseDMT(string OrderID)
    {
        DataTable dthis = await wallet.GetTransactionList(OrderID);
        if (dthis.Rows.Count > 0)
        {
            for (Int32 i = 0; i < dthis.Rows.Count; i++)
            {
                string loginid = await cls.select_data_scalar_stringAsync("select loginid from member where msrno='" + dthis.Rows[0]["msrno"].ToString() + "'");
                string Narrtion = "";
                if (dthis.Rows[i]["Factor"].ToString().ToUpper() == "DR")
                {
                    Narrtion = "Refund/" + dthis.Rows[i]["Narration"].ToString();
                }
                else
                {
                    Narrtion = "Revert/" + dthis.Rows[i]["Narration"].ToString();
                }

                Int32 Val = await wallet.CheckCreditorNot(Narrtion, dthis.Rows[0]["msrno"].ToString());
                if (Val == 0)
                {
                    if (dthis.Rows[i]["Factor"].ToString().ToUpper() == "DR")
                    {
                        DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + loginid + "','" + dthis.Rows[i]["Amount"].ToString() + "','CR','Refund/" + dthis.Rows[i]["Narration"].ToString() + "',''," + ConstantsData.SMoneyTransfer + ",'" + dthis.Rows[i]["TRANSACTIONID"].ToString() + "',0,0,0,'" + dthis.Rows[i]["Surcharge"].ToString() + "','" + dthis.Rows[i]["GST"].ToString() + "'");
                    }
                    else
                    {
                        DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + loginid + "','" + dthis.Rows[i]["Amount"].ToString() + "','DR','Revert/" + dthis.Rows[i]["Narration"].ToString() + "',''," + ConstantsData.SMoneyTransfer + ",'" + dthis.Rows[i]["TRANSACTIONID"].ToString() + "',0,'" + dthis.Rows[i]["TDS"].ToString() + "','" + dthis.Rows[i]["Commission"].ToString() + "','" + dthis.Rows[i]["Surcharge"].ToString() + "','" + dthis.Rows[i]["GST"].ToString() + "'");
                    }
                }

            }
            return 1;
        }
        else
            return 0;
    }

    public async Task<int> ReviseAEPSWallet(string OrderID)
    {
        DataTable dthis = await wallet.GetTransactionListAEPSWallet(OrderID);
        if (dthis.Rows.Count > 0)
        {
            for (Int32 i = 0; i < dthis.Rows.Count; i++)
            {
                string loginid = await cls.select_data_scalar_stringAsync("select loginid from member where msrno='" + dthis.Rows[0]["msrno"].ToString() + "'");
                string Narrtion = "";
                if (dthis.Rows[i]["Factor"].ToString().ToUpper() == "DR")
                {
                    Narrtion = "Refund/" + dthis.Rows[i]["Narration"].ToString();
                }
                else
                {
                    Narrtion = "Revert/" + dthis.Rows[i]["Narration"].ToString();
                }

                Int32 Val = await wallet.CheckCreditorNot(Narrtion, dthis.Rows[0]["msrno"].ToString());
                if (Val == 0)
                {
                    if (dthis.Rows[i]["Factor"].ToString().ToUpper() == "DR")
                    {
                        DataTable dtval = await cls.select_data_dtAsync("exec PROC_AEPSWALLETTRANSACTION '" + loginid + "','" + dthis.Rows[i]["Amount"].ToString() + "','CR','Refund/" + dthis.Rows[i]["Narration"].ToString() + "',''," + ConstantsData.SMoneyTransfer + ",'" + dthis.Rows[i]["TRANSACTIONID"].ToString() + "',0,0,0,'" + dthis.Rows[i]["Surcharge"].ToString() + "','" + dthis.Rows[i]["GST"].ToString() + "'");
                    }
                    else
                    {
                        DataTable dtval = await cls.select_data_dtAsync("exec PROC_AEPSWALLETTRANSACTION '" + loginid + "','" + dthis.Rows[i]["Amount"].ToString() + "','DR','Revert/" + dthis.Rows[i]["Narration"].ToString() + "',''," + ConstantsData.SMoneyTransfer + ",'" + dthis.Rows[i]["TRANSACTIONID"].ToString() + "',0,'" + dthis.Rows[i]["TDS"].ToString() + "','" + dthis.Rows[i]["Commission"].ToString() + "','" + dthis.Rows[i]["Surcharge"].ToString() + "','" + dthis.Rows[i]["GST"].ToString() + "'");
                    }
                }

            }
            return 1;
        }
        else
            return 0;
    }

    public async Task<string> CheckStatusDMT(string OrderID)
    {
        DataTable dthis = await GetDMTDetails(OrderID);
        if (dthis.Rows.Count > 0)
        {
            String Result = "";
            //InstantPay
            if (dthis.Rows[0]["APIID"].ToString() == "9")
            {
                Result = await CheckStatusInspay(OrderID, Convert.ToDateTime(dthis.Rows[0]["AddDate"].ToString()).ToString("yyyy-MM-dd"));
            }
            else if (dthis.Rows[0]["APIID"].ToString() == "1009")
            {
                Result = await CheckStatusInspay(OrderID, Convert.ToDateTime(dthis.Rows[0]["AddDate"].ToString()).ToString("yyyy-MM-dd"));
            }
            else if (dthis.Rows[0]["APIID"].ToString() == "7")
            {
                Result = await CheckStatusSafex(OrderID, dthis.Rows[0]["VendorID"].ToString());
            }
            
            else if (dthis.Rows[0]["APIID"].ToString() == "1010")
            {
                Result = await CheckStatusezypay(OrderID, dthis.Rows[0]["VendorID"].ToString());
            }


            if (!string.IsNullOrEmpty(Result))
            {
                JObject jResult = JObject.Parse(Result);
                if (jResult["code"].ToString() != "ERR")
                {
                    if (jResult["data"]["Status"].ToString().ToUpper() == "SUCCESS")
                    {
                        Int32 Val = await cls.update_dataAsync("update moneytransfer set Message='" + jResult["data"]["Reason"].ToString() + "',Status='" + jResult["data"]["Status"].ToString() + "',RRN='" + jResult["data"]["RRN"].ToString() + "',Response='" + jResult["data"]["Result"].ToString() + "' where OrderID='" + OrderID + "'");
                        if (Val > 0)
                        {
                            await commissionService.GiveDMTCommission(OrderID);
                        }
                    }
                    else if (jResult["data"]["Status"].ToString().ToUpper() == "FAILED")
                    {
                        Int32 Val = await cls.update_dataAsync("update moneytransfer set Message='" + jResult["data"]["Reason"].ToString() + "',Status='" + jResult["data"]["Status"].ToString() + "',RRN='" + jResult["data"]["RRN"].ToString() + "',Response='" + jResult["data"]["Result"].ToString() + "' where OrderID='" + OrderID + "'");
                        if (Val > 0)
                        {
                            await ReviseDMT(OrderID);
                        }
                    }
                    return ErrorShow.SuccessMessageAPI(jResult["data"]["Reason"].ToString());
                }
                else
                {
                    return Result;
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Response Blank");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Transaction not found");
        }
    }

    public async Task<string> CheckStatusAEPSPaysettlement(string OrderID)
    {
        DataTable dthis = await GetAEPSPaysetllementDetails(OrderID);
        if (dthis.Rows.Count > 0)
        {
            String Result = "";
            //InstantPay
            if (dthis.Rows[0]["PaymentFrom"].ToString() == "9")
            {
                Result = await CheckStatusInspay(OrderID, Convert.ToDateTime(dthis.Rows[0]["RequestDate"].ToString()).ToString("yyyy-MM-dd"));
            }
            else if (dthis.Rows[0]["PaymentFrom"].ToString() == "1009")
            {
                Result = await CheckStatusInspay(OrderID, Convert.ToDateTime(dthis.Rows[0]["RequestDate"].ToString()).ToString("yyyy-MM-dd"));
            }
            else if (dthis.Rows[0]["PaymentFrom"].ToString() == "7")
            {
                Result = await CheckStatusSafex(OrderID, dthis.Rows[0]["TransactionID"].ToString());
            }
           
            else if (dthis.Rows[0]["PaymentFrom"].ToString() == "5")
            {
                Result = await CheckStatuspas(OrderID, dthis.Rows[0]["TransactionID"].ToString(), Convert.ToDateTime(dthis.Rows[0]["RequestDate"].ToString()).ToString("yyyy-MM-dd").ToString(), Convert.ToDateTime(dthis.Rows[0]["RequestDate"].ToString()).ToString("yyyy-MM-dd").ToString());
            }

            if (!string.IsNullOrEmpty(Result))
            {
                JObject jResult = JObject.Parse(Result);
                if (jResult["code"].ToString() != "ERR")
                {
                    if (jResult["data"]["Status"].ToString().ToUpper() == "SUCCESS")
                    {
                        cls.update_data("EXEC PROC_AEPS2WITHDRAW_REQUEST_ACTION 'AEPS2WITHDRAW_REQUEST_APPROVE'," + dthis.Rows[0]["ID"].ToString() + ",'" + jResult["data"]["VendorID"].ToString() + "','" + jResult["data"]["Status"].ToString() + "','TXN','" + jResult["data"]["Reason"].ToString() + "','" + jResult["data"]["RRN"].ToString() + "','" + Result + "'," + ConstantsData.SSettlement + "");
                    }
                    else if (jResult["data"]["Status"].ToString().ToUpper() == "FAILED")
                    {
                        cls.update_data("EXEC PROC_AEPS2WITHDRAW_REQUEST_ACTION 'AEPS2WITHDRAW_REQUEST_DELETE'," + dthis.Rows[0]["ID"].ToString() + ",'" + jResult["data"]["VendorID"].ToString() + "','" + jResult["data"]["Status"].ToString() + "','ERR','" + jResult["data"]["Reason"].ToString() + "','" + jResult["data"]["RRN"].ToString() + "','" + Result + "'," + ConstantsData.SSettlement + "");
                    }
                    return ErrorShow.SuccessMessageAPI(jResult["data"]["Reason"].ToString());
                }
                else
                {
                    return Result;
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Response Blank");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Transaction not found");
        }
    }

    public async Task<string> CheckStatusPaysettlement(string OrderID)
    {
        DataTable dthis = await GetPaysetllementDetails(OrderID);
        if (dthis.Rows.Count > 0)
        {
            String Result = "";
            //InstantPayf
            if (dthis.Rows[0]["APIID"].ToString() == "9")
            {
                Result = await CheckStatusInspay(OrderID, Convert.ToDateTime(dthis.Rows[0]["AddDate"].ToString()).ToString("yyyy-MM-dd"));
            }
            else if (dthis.Rows[0]["APIID"].ToString() == "1009")
            {
                Result = await CheckStatusInspay(OrderID, Convert.ToDateTime(dthis.Rows[0]["AddDate"].ToString()).ToString("yyyy-MM-dd"));
            }
            else if (dthis.Rows[0]["APIID"].ToString() == "7")
            {
                Result = await CheckStatusSafex(OrderID, dthis.Rows[0]["VendorID"].ToString());
            }
            else if (dthis.Rows[0]["APIID"].ToString() == "6")
            {
                Result = await classypayCheckStatus(dthis.Rows[0]["VendorID"].ToString());
            }

            if (!string.IsNullOrEmpty(Result))
            {
                JObject jResult = JObject.Parse(Result);
                if (jResult["code"].ToString() != "ERR")
                {
                    if (jResult["data"]["Status"].ToString().ToUpper() == "SUCCESS")
                    {
                        Int32 Val = await cls.update_dataAsync("update PaySettlement set Message='" + jResult["data"]["Reason"].ToString() + "',Status='" + jResult["data"]["Status"].ToString() + "',RRN='" + jResult["data"]["RRN"].ToString() + "',Response='" + jResult["data"]["Result"].ToString() + "' where OrderID='" + OrderID + "'");
                        if (Val > 0)
                        {
                            await GiveCommissionPayout(OrderID);
                        }
                    }
                    else if (jResult["data"]["Status"].ToString().ToUpper() == "FAILED")
                    {
                        Int32 Val = await cls.update_dataAsync("update PaySettlement set Message='" + jResult["data"]["Reason"].ToString() + "',Status='" + jResult["data"]["Status"].ToString() + "',RRN='" + jResult["data"]["RRN"].ToString() + "',Response='" + jResult["data"]["Result"].ToString() + "' where OrderID='" + OrderID + "'");
                        if (Val > 0)
                        {
                            await ReviseDMT(OrderID);
                        }
                    }
                    return ErrorShow.SuccessMessageAPI(jResult["data"]["Reason"].ToString());
                }
                else
                {
                    return Result;
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Response Blank");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Transaction not found");
        }
    }

    public async Task<int> GiveCommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from moneytransfer where orderid='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("DMT_DistributeCommission " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["OrderID"].ToString() + "'");
            if (Val > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }
    public async Task<int> GiveCommissionPayout(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from PaySettlement where orderid='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("PaySettlement_DistributeCommission " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["OrderID"].ToString() + "'");
            if (Val > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }


    public async Task<string> CheckStatusInspay(string OrderID, string Date)
    {
        CheckStatusInstantPay checkStatusInstantPay = new CheckStatusInstantPay();
        checkStatusInstantPay.externalRef = OrderID;
        checkStatusInstantPay.transactionDate = Date;
        string Result = await ApiPostCallInstantPay.PostCallCheckStatusASYNC(checkStatusInstantPay.GetJson());
        if (!string.IsNullOrEmpty(Result))
        {
            JObject Data = JObject.Parse(Result);
            if (Data["data"].ToString() != "")
            {
                if (InstantPayout.arrTemplate.Contains(Data["data"]["transactionStatusCode"].ToString()))
                {
                    CommonCheckStatusDMT commonCheckStatusDMT = new CommonCheckStatusDMT();

                    if (Data["data"]["transactionStatusCode"].ToString().ToUpper() == "TXN" || Data["data"]["transactionStatusCode"].ToString().ToUpper() == "TUP")
                    {
                        String Status = "Pending";
                        if (Data["data"]["transactionStatusCode"].ToString().ToUpper() == "TXN")
                        {
                            Status = "Success";
                        }
                        commonCheckStatusDMT.OrderID = OrderID;
                        commonCheckStatusDMT.Reason = Data["data"]["transactionStatus"].ToString();
                        commonCheckStatusDMT.Status = Status;
                        commonCheckStatusDMT.RRN = Data["data"]["transactionReferenceId"].ToString();
                        commonCheckStatusDMT.Result = Result;
                        commonCheckStatusDMT.VendorID = "";
                    }
                    else
                    {
                        commonCheckStatusDMT.OrderID = OrderID;
                        commonCheckStatusDMT.Reason = Data["data"]["transactionStatus"].ToString();
                        commonCheckStatusDMT.Status = "Failed";
                        commonCheckStatusDMT.RRN = Data["data"]["transactionReferenceId"].ToString();
                        commonCheckStatusDMT.Result = Result;
                        commonCheckStatusDMT.VendorID = "";
                    }

                    string jsonString = JsonSerializer.Serialize(commonCheckStatusDMT);
                    return ErrorShow.SuccessArrayAPI(jsonString);
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI(Data["status"].ToString());
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(Data["status"].ToString());
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Blank Response");
        }
    }

    public async Task<DataTable> GetDMTDetails(string OrderID)
    {
        return await cls.select_data_dtAsync("select * from moneytransfer where orderid='" + OrderID + "'");
    }


    public async Task<DataTable> GetPaysetllementDetails(string OrderID)
    {
        return await cls.select_data_dtAsync("select * from PaySettlement where orderid='" + OrderID + "'");
    }

    public async Task<DataTable> GetAEPSPaysetllementDetails(string OrderID)
    {
        return await cls.select_data_dtAsync("select * from AEPS_WithdrwalRequest where Requestid='" + OrderID + "'");
    }


    public async Task<string> CheckStatusezypay(string OrderID, string payoutId)
    {
        object Requst = new
        {
            token = ezypay.token,
            requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
            txnid = payoutId
        };
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Fetch/txnStatus");
        JObject TransferData = JObject.Parse(Result);

        if (TransferData["Resp_code"].ToString() == "RCS")
        {
            CommonCheckStatusDMT commonCheckStatusDMT = new CommonCheckStatusDMT();
            string Status = "";
            if (TransferData["data"]["txnstatus"].ToString().ToUpper() == "PENDING" || TransferData["data"]["txnstatus"].ToString().ToUpper() == "INPROGRESS")
                Status = "Pending";
            else if (TransferData["data"]["txnstatus"].ToString().ToUpper() == "FAILED" || TransferData["data"]["txnstatus"].ToString().ToUpper() == "REJECTED")
                Status = "Failed";
            else if (TransferData["data"]["txnstatus"].ToString().ToUpper() == "SUCCESS")
                Status = "Success";

            if (Status.ToUpper() == "SUCCESS" || Status.ToUpper() == "PENDING")
            {
                if (Status.ToUpper() == "SUCCESS")
                {
                    Status = "Success";
                }
                commonCheckStatusDMT.OrderID = OrderID;
                commonCheckStatusDMT.Reason = TransferData["data"]["txnstatus"].ToString();
                commonCheckStatusDMT.Status = Status;
                commonCheckStatusDMT.RRN = TransferData["data"]["txnid"].ToString();
                commonCheckStatusDMT.Result = Result;
                commonCheckStatusDMT.VendorID = "";
            }
            else
            {
                commonCheckStatusDMT.OrderID = OrderID;
                commonCheckStatusDMT.Reason = TransferData["data"]["txnstatus"].ToString();
                commonCheckStatusDMT.Status = "Failed";
                commonCheckStatusDMT.RRN = "";
                commonCheckStatusDMT.Result = Result;
                commonCheckStatusDMT.VendorID = "";
            }

            string jsonString = JsonSerializer.Serialize(commonCheckStatusDMT);
            return ErrorShow.SuccessArrayAPI(jsonString);
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(TransferData["Resp_desc"].ToString());
        }

    }


    public async Task<string> CheckStatusSafex(string OrderID, string payoutId)
    {
        string hh = "{\"header\": {\"operatingSystem\": \"android\",\"sessionId\": \"" + safepay.MerchantId + "\",\"version\": \"1.0.0\"},\"userInfo\": {},\"transaction\": {\"requestType\": \"TMH\",\"requestSubType\": \"STCHK\",\"tranCode\": 0,\"txnAmt\": 0.0},\"payOutBean\": {\"orderRefNo\": \"" + OrderID + "\",\"payoutId\": \"" + payoutId + "\"}}";
        string Request = Common.encrypt(hh);
        string ResultEncrpted = await safepay.PostCallWithBodyAsync(Request);
        JObject jResponse = JObject.Parse(ResultEncrpted);
        if (jResponse["status"] == null)
        {
            string Result = Common.decrypt(jResponse["payload"].ToString());
            JObject TransferData = JObject.Parse(Result);

            if (TransferData["response"]["code"].ToString() == "0001" || TransferData["response"]["code"].ToString() == "0000" || TransferData["response"]["code"].ToString() == "0002" || TransferData["response"]["code"].ToString() == "0003")
            {
                CommonCheckStatusDMT commonCheckStatusDMT = new CommonCheckStatusDMT();
                string Status = "";
                if (TransferData["payOutBean"]["txnStatus"].ToString().ToUpper() == "PENDING" || TransferData["payOutBean"]["txnStatus"].ToString().ToUpper() == "INPROGRESS")
                    Status = "Pending";
                else if (TransferData["payOutBean"]["txnStatus"].ToString().ToUpper() == "FAILED" || TransferData["payOutBean"]["txnStatus"].ToString().ToUpper() == "REJECTED")
                    Status = "Failed";
                else if (TransferData["payOutBean"]["txnStatus"].ToString() == "SUCCESS")
                    Status = "Success";

                if (Status.ToUpper() == "SUCCESS" || Status.ToUpper() == "PENDING")
                {
                    if (Status.ToUpper() == "SUCCESS")
                    {
                        Status = "Success";
                    }
                    commonCheckStatusDMT.OrderID = OrderID;
                    commonCheckStatusDMT.Reason = TransferData["payOutBean"]["statusDesc"].ToString();
                    commonCheckStatusDMT.Status = Status;
                    commonCheckStatusDMT.RRN = TransferData["payOutBean"]["bankRefNo"].ToString();
                    commonCheckStatusDMT.Result = Result;
                    commonCheckStatusDMT.VendorID = "";
                }
                else
                {
                    commonCheckStatusDMT.OrderID = OrderID;
                    commonCheckStatusDMT.Reason = TransferData["payOutBean"]["statusDesc"].ToString();
                    commonCheckStatusDMT.Status = "Failed";
                    commonCheckStatusDMT.RRN = "";
                    commonCheckStatusDMT.Result = Result;
                    commonCheckStatusDMT.VendorID = "";
                }

                string jsonString = JsonSerializer.Serialize(commonCheckStatusDMT);
                return ErrorShow.SuccessArrayAPI(jsonString);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(jResponse["userMessage"].ToString());
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(jResponse["userMessage"].ToString());
        }
    }

    public async Task<string> classypayCheckStatus(string OrderID)
    {
        string request = "payment/v3/payout/status?txnId="+ OrderID + "";
        string Result = await classypay.PostCall("", request);
        JObject TransferData = JObject.Parse(Result);

        CommonCheckStatusDMT Response = new CommonCheckStatusDMT();


        if (Convert.ToString(TransferData["status"]).ToUpper() != "FALSE")
        {
            string Status = TransferData["status"].ToString().ToUpper();
            if (Status == "SUCCESS")
            {
                Status = "SUCCESS";
            }
            else if (Status == "PENDING")
            {
                Status = "PENDING";
            }
            else if (Status == "FAILURE")
            {
                Status = "Failed";
            }
            else
            {
                Status = "PENDING";
            }
            Response.Status = Status;
            Response.Reason = Convert.ToString(TransferData["message"]);
            Response.VendorID = Convert.ToString(TransferData["data"]?["txnId"].ToString());
            Response.RRN = Convert.ToString(TransferData["data"]?["rrn"].ToString());
            Response.Result = Result;
            Response.OrderID = Convert.ToString(TransferData["data"]?["apiPartnerRequestId"].ToString());

            string jsonString = JsonSerializer.Serialize(Response);
            return ErrorShow.SuccessArrayAPI(jsonString);
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(TransferData["message"].ToString());           
        }
      
    }

    public async Task<string> CheckStatuspas(string OrderID, string payoutId, string FDate, string TDate)
    {
        string request = "{\"refid\":\"" + OrderID + "\",\"ackno\":\"" + payoutId + "\"}";
        JWTBankSendRequestNewIntegration jj = new JWTBankSendRequestNewIntegration();
        string Result = await PaysPrintPostCall.PostCallWithBodyAsync("payout/payout/status", jj.GetToken(), request);
        JObject TransferData = JObject.Parse(Result);

        if (Convert.ToBoolean(TransferData["status"].ToString()))
        {
            CommonCheckStatusDMT commonCheckStatusDMT = new CommonCheckStatusDMT();
            string Status = "";
            if (TransferData["data"]["txn_status"].ToString().ToUpper() == "2" || TransferData["data"]["txn_status"].ToString().ToUpper() == "4" || TransferData["data"]["txn_status"].ToString().ToUpper() == "3")
                Status = "Pending";
            else if (TransferData["data"]["txn_status"].ToString().ToUpper() == "0")
                Status = "Failed";
            else if (TransferData["data"]["txn_status"].ToString().ToUpper() == "1")
                Status = "Success";

            if (Status.ToUpper() == "SUCCESS" || Status.ToUpper() == "PENDING")
            {
                if (Status.ToUpper() == "SUCCESS")
                {
                    Status = "Success";
                }
                commonCheckStatusDMT.OrderID = OrderID;
                commonCheckStatusDMT.Reason = TransferData["data"]["status"].ToString();
                commonCheckStatusDMT.Status = Status;
                commonCheckStatusDMT.RRN = TransferData["data"]["utr"].ToString();
                commonCheckStatusDMT.Result = Result;
                commonCheckStatusDMT.VendorID = payoutId;
            }
            else
            {
                commonCheckStatusDMT.OrderID = OrderID;
                commonCheckStatusDMT.Reason = TransferData["data"]["status"].ToString();
                commonCheckStatusDMT.Status = "Failed";
                commonCheckStatusDMT.RRN = "";
                commonCheckStatusDMT.Result = Result;
                commonCheckStatusDMT.VendorID = payoutId;
            }

            string jsonString = JsonSerializer.Serialize(commonCheckStatusDMT);
            return ErrorShow.SuccessArrayAPI(jsonString);
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(TransferData["message"].ToString());
        }

    }
    public async Task<DataTable> GetAEPSDetails(string OrderID)
    {
        return await cls.select_data_dtAsync("select * from aeps_history where referenceno='" + OrderID + "'");
    }

    public async Task<string> CheckStatusAEPS(string OrderID)
    {
        DataTable dthis = await GetAEPSDetails(OrderID);
        if (dthis.Rows.Count > 0)
        {
            if (string.Equals(dthis.Rows[0]["Status"].ToString(), "PENDING", StringComparison.OrdinalIgnoreCase))
            {
                var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(dthis.Rows[0]["msrno"].ToString());
                if (!_memberAllowDetails.HoldUnHold)
                {
                    CommonCheckStatusAEPS commonCheckStatusAEPS = new CommonCheckStatusAEPS();
                    commonCheckStatusAEPS.TYPE = dthis.Rows[0]["transcationtype"].ToString();
                    //Paysprint
                    if (dthis.Rows[0]["APIID"].ToString() == "1010")
                    {
                        commonCheckStatusAEPS = await CheckStatusAEPSEzpay(OrderID, Convert.ToDateTime(dthis.Rows[0]["AddDate"].ToString()).ToString("yyyy-MM-dd"));
                    }
                    if (!string.IsNullOrEmpty(commonCheckStatusAEPS.Status))
                    {
                        commonCheckStatusAEPS.TYPE = dthis.Rows[0]["transcationtype"].ToString();
                        if (commonCheckStatusAEPS.Status.ToUpper() == "SUCCESS")
                        {
                            Int32 Val = await cls.update_dataAsync("update aeps_history set status='Success',Response='" + commonCheckStatusAEPS.Result + "',message='" + commonCheckStatusAEPS.Reason + "' where referenceno='" + OrderID + "'");
                            if (Val > 0)
                            {
                                if (string.Equals(commonCheckStatusAEPS.TYPE, "CW", StringComparison.OrdinalIgnoreCase))
                                {
                                    ICommissionService comm = new CommissionService();
                                    await comm.GiveAEPSCommission(OrderID);
                                    await comm.GiveAEPSAmount(OrderID, _memberAllowDetails.LoginID, dthis.Rows[0]["msrno"].ToString());
                                }
                            }
                            return ErrorShow.SuccessMessageAPI(commonCheckStatusAEPS.Reason);

                        }
                        else if (commonCheckStatusAEPS.Status.ToUpper() == "FAILED")
                        {
                            Int32 Val = await cls.update_dataAsync("update aeps_history set status='Failed',Response='" + commonCheckStatusAEPS.Result + "',message='" + commonCheckStatusAEPS.Reason + "' where referenceno='" + OrderID + "'");
                            return ErrorShow.ErrorMessageAPI(commonCheckStatusAEPS.Reason);

                        }
                        else
                        {
                            return ErrorShow.SuccessMessageAPI("Your Under Process");
                        }
                    }
                    else
                    {
                        return ErrorShow.ErrorMessageAPI("Response Blank");
                    }
                }
                else
                    return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Tranaction Not in Pending Mode");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Transaction not found");
        }
    }

    public async Task<CommonCheckStatusAEPS> CheckStatusAEPSEzpay(string OrderID,string date)
    {
        object returnData = new
        {
            txn_requestid = OrderID,
            txn_request_date = date,
            requestid = await wallet.GetOrderID(),
            token=ezypay.token
        };

       

        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);
        string Result = await ezypay.Postcall(Request, "Fetch/txnStatus");
        JObject TransferData = JObject.Parse(Result);

        CommonCheckStatusAEPS commonCheckStatusAEPS = new CommonCheckStatusAEPS();
        if (TransferData["Resp_code"].ToString().ToUpper() != "RCS")
        {

            commonCheckStatusAEPS.Status = "Failed";
            commonCheckStatusAEPS.Reason = TransferData["Resp_desc"].ToString();
            commonCheckStatusAEPS.OrderID = OrderID;
            commonCheckStatusAEPS.Result = Result;
        }
        else
        {
            if (TransferData["data"]["txnstatus"].ToString() == "SUCCESS")
            {
                commonCheckStatusAEPS.Status = "Success";
                commonCheckStatusAEPS.Reason = TransferData["Resp_desc"].ToString();
                commonCheckStatusAEPS.VendorID = TransferData["data"]["txnid"].ToString();
                commonCheckStatusAEPS.RRN = TransferData["data"]["opid"].ToString();
                commonCheckStatusAEPS.Result = Result;
                commonCheckStatusAEPS.OrderID = OrderID;
            }
            else if (TransferData["data"]["txnstatus"].ToString() == "FAILED")
            {
                commonCheckStatusAEPS.Status = "Failed";
                commonCheckStatusAEPS.Reason = TransferData["Resp_desc"].ToString();
                commonCheckStatusAEPS.VendorID = TransferData["data"]["txnid"].ToString();
                commonCheckStatusAEPS.RRN = TransferData["data"]["opid"].ToString();
                commonCheckStatusAEPS.Result = Result;
                commonCheckStatusAEPS.OrderID = OrderID;
            }
            else
            {
                commonCheckStatusAEPS.Status = "Pending";
                commonCheckStatusAEPS.Reason = TransferData["Resp_desc"].ToString();
                commonCheckStatusAEPS.VendorID = "";
                commonCheckStatusAEPS.RRN = "";
                commonCheckStatusAEPS.Result = Result;
                commonCheckStatusAEPS.OrderID = OrderID;
            }
        }

        return commonCheckStatusAEPS;
    }

}