using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for CreditCardService
/// </summary>
public class CreditCardService : ICreditCard
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    ICommissionService commissionService;
    ICommonService commonService;
    IPayoutService payoutService;
    private static string ServiceID = "57";
    public CreditCardService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commissionService = new CommissionService();
        commonService = new CommonService();
        payoutService = new PayoutService();

    }

    private bool IsValidSlab(SlabModel slabDetails, decimal amount)
    {
        return slabDetails.startVal <= amount &&
               slabDetails.endVal >= amount &&
               slabDetails.startVal > 0 &&
               slabDetails.endVal > 0;
    }

    public async Task<string> CreditCardPay(CreditCardRequest creditPayST, string MemberID, string Source)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(CreditCardService.ServiceID);
        if (serviceData.Status == "Success")
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
            if (!_memberAllowDetails.HoldUnHold)
            {
                decimal MyBalance = 0;
                MyBalance = _memberAllowDetails.MainBalance;
                if (MyBalance >= Convert.ToDecimal(creditPayST.Amount))
                {
                    string OrderID = DateTime.Now.ToString("yyyyMMddHHmmss") + _memberAllowDetails.Msrno;
                    string Narration = $"{creditPayST.CardNo}/{creditPayST.Name}/{OrderID}"; ;
                    string Description = "";

                    var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(creditPayST.Amount), serviceData.APIID.ToString(), CreditCardService.ServiceID, "CCPay");
                    if (!IsValidSlab(item, Convert.ToDecimal(creditPayST.Amount)))
                        return ErrorShow.ErrorMessageAPI("Slab configuration not set.");



                    decimal Surcahrge = item.surcharge;

                    Decimal GSTCalculation = (Surcahrge * item.Gst / 118);

                    Decimal PaySurcharge = (Surcahrge - GSTCalculation);
                    decimal deductAmount = Convert.ToDecimal(creditPayST.Amount) + Surcahrge;
                    DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + creditPayST.Amount + "','DR','" + Narration + "','" + Description + "'," + CreditCardService.ServiceID + ",'" + OrderID + "',0,0,0," + PaySurcharge + "," + GSTCalculation + "");
                    if (dtval.Rows[0]["msrno"].ToString() == "1")
                    {
                        Int32 Val = await cls.update_dataAsync("AddEditCreditCardBillPay " + _memberAllowDetails.Msrno + ",'" + OrderID + "','" + creditPayST.Name + "','" + creditPayST.CardNo + "','" + creditPayST.Mobileno + "','" + creditPayST.Amount + "','" + PaySurcharge + "','" + creditPayST.CardType + "','Bill Payment','','Pending','" + GSTCalculation + "'," + serviceData.APIID + ",'" + Source + "','" + CreditCardService.ServiceID + "','" + item.OpID + "','" + _memberAllowDetails.MainBalance + "'");
                        if (Val > 0)
                        {
                            CreditPayST creditCardBillPayment = new CreditPayST();
                            creditCardBillPayment.Amount = creditPayST.Amount.ToString();
                            creditCardBillPayment.Name = creditPayST.Name;
                            creditCardBillPayment.CardNo = creditPayST.CardNo;
                            creditCardBillPayment.NetWork = creditPayST.CardType;
                            creditCardBillPayment.OrderID = OrderID;
                            creditCardBillPayment.fetchId = creditPayST.FetchID;
                            creditCardBillPayment.APIID = SONITechnoCredentrial.APIID;
                            creditCardBillPayment.Token = SONITechnoCredentrial.Token;
                            creditCardBillPayment.MethodName = "paybill";

                            PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
                            if (serviceData.APIID == 1)
                            {
                                payoutCommonResponse = await payoutService.FundTransferCCPaySoniTechno(creditCardBillPayment);
                            }
                            //else if (serviceData.APIID == 1014)
                            //{
                            //    payoutCommonResponse = await payoutService.FundTransferCCPayInstantPay(creditCardBillPayment);
                            //}
                            else
                            {
                                return ErrorShow.ErrorMessageAPI("API Not Allow For Credit Card");
                            }

                            try
                            {
                                await commonService.SaveLogs(new LogsRequestModel() { ServiceID = AccountVerificationService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = OrderID, RequestData = payoutCommonResponse.Result, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
                            }
                            catch { };


                            if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
                            {
                                cls.update_data("update CreditCardBillPay set Status='" + payoutCommonResponse.Status + "',VenorID='" + payoutCommonResponse.VendorID + "',Message='" + payoutCommonResponse.Message + "' where OrderID='" + OrderID + "'");

                                if (payoutCommonResponse.Status.ToUpper() == "SUCCESS")
                                {
                                    await commissionService.GiveCCPayCommission(OrderID);
                                }

                                return ErrorShow.SuccessMessageAPI("Bill Pay Successfully");
                            }
                            else
                            {
                                Int32 ValUpdate = await cls.update_dataAsync("Update CreditCardBillPay set Status='Refund',Message='" + payoutCommonResponse.Message + "' where OrderID='" + OrderID + "'");
                                if (ValUpdate == 1)
                                {
                                    dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + Convert.ToDecimal(creditPayST.Amount) + ", 'CR','Refund - " + Narration + "',''," + CreditCardService.ServiceID + ",'" + OrderID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                                    if (dtval.Rows.Count > 0)
                                    {
                                        return ErrorShow.ErrorMessageAPI("Transaction Failed due to " + payoutCommonResponse.Message + ", But Amount Not Refund in Wallet Contact to Admin");
                                    }
                                    else
                                    {
                                        return ErrorShow.ErrorMessageAPI("Transaction Failed due to " + payoutCommonResponse.Message + ", But Amount Not Refund in Wallet Contact to Admin");
                                    }
                                }
                                else
                                    return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
                            }

                        }
                        else
                            return ErrorShow.ErrorMessageAPI("Low Balance");
                    }
                    else
                        return ErrorShow.ErrorMessageAPI("Low Balance");
                }
                return ErrorShow.ErrorMessageAPI("Low Balance");
            }
            else
                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }


    public async Task<string> BillerList()
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(CreditCardService.ServiceID);
        if (serviceData.Status == "Success")
        {
            object ff = new
            {
                MethodName = "billerlist",
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token,
            };
            string PayRequest = Newtonsoft.Json.JsonConvert.SerializeObject(ff);
            string Result = await ApiPostCall.PostCallAsync(PayRequest, SONITechnoCredentrial.CCpay);

            JObject TransferData = JObject.Parse(Result);
            if (TransferData["code"].ToString().ToUpper() != "ERR")
            {
                return ErrorShow.SuccessArrayAPI(TransferData["data"].ToString());
            }
            else
                return ErrorShow.ErrorMessageAPI(TransferData["mess"].ToString());

        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }

    public async Task<string> CreditCardBillFetch(dynamic Request)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(CreditCardService.ServiceID);
        if (serviceData.Status == "Success")
        {
            Request.MethodName = "billfetch";
            Request.APIID = SONITechnoCredentrial.APIID;
            Request.Token = SONITechnoCredentrial.Token;

            string PayRequest = Newtonsoft.Json.JsonConvert.SerializeObject(Request);

            string Result = await ApiPostCall.PostCallAsync(PayRequest, SONITechnoCredentrial.CCpay);

            JObject TransferData = JObject.Parse(Result);
            if (TransferData["code"].ToString().ToUpper() != "ERR")
            {
                return ErrorShow.SuccessArrayAPI(TransferData["data"].ToString());
            }
            else
                return ErrorShow.ErrorMessageAPI(TransferData["mess"].ToString());

        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }
}