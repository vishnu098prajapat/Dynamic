using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Summary description for SafeDMTService
/// </summary>
public class SafeDMTService : ISafeDMTService
{
    cls_connection cls;
    IServiceList serviceList;
    IWallet wallet;
    IPayoutService payoutService;
    private static string ServiceID = "60";
    ICommissionService commissionService;
    ICommonService commonService;

    public SafeDMTService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        payoutService = new PayoutService();
        commissionService = new CommissionService();
        commonService = new CommonService();

    }
    public async Task<string> AccountVerification(CommonAccountVerificationRequest accountVerifyST, string MemberID)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();


        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(AccountVerificationService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        // Get member details
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Not Allow. Contact Admin.");
        }

        //// Step 5: Get surcharge and commission details
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
            _memberAllowDetails.PackgeID.ToString(),
            0,
            serviceData.APIID.ToString(),
            AccountVerificationService.ServiceID, "DMTV"
        );

        if (item.surcharge == 0)
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


        string hddGroupTransID = await wallet.GetOrderID();
        decimal MyBalance = 0;
        decimal charge = 0M;
        decimal mySurcharge = item.surcharge;
        MyBalance = _memberAllowDetails.MainBalance;
        if (MyBalance >= (charge + _memberAllowDetails.HoldAMT + mySurcharge))
        {

            Decimal GSTCalculation = 0;
            Decimal GSTCalculationC = 0;// mySurcharge - GSTCalculation;


            const decimal gstRate = 18m;
            decimal gstMultiplier = gstRate / 100;


            GSTCalculationC = item.IsPlusMinGst
   ? item.surcharge * gstMultiplier
   : item.surcharge - (item.surcharge / (1 + gstMultiplier));

            mySurcharge = item.IsPlusMinGst
                ? item.surcharge
                : item.surcharge - GSTCalculationC;


            string agentID = await wallet.GetOrderID();
            accountVerifyST.OrderID = agentID;

            string Dec = $"{accountVerifyST.AccountNO}/{accountVerifyST.IFSC}/{agentID}";
            string narration = $"{accountVerifyST.AccountNO}/{accountVerifyST.IFSC}/{agentID}";
            PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();


            PayoutCommon payoutCommon = new PayoutCommon();

            payoutCommon.Amount = 0;
            payoutCommon.OrderID = agentID;
            payoutCommon.AccountID = "123";
            payoutCommon.Mobile = accountVerifyST.CusMobile;
            payoutCommon.BankName = accountVerifyST.BankName;
            payoutCommon.Name = accountVerifyST.CusName;
            payoutCommon.AccountNO = accountVerifyST.AccountNO;
            payoutCommon.IFSC = accountVerifyST.IFSC;
            payoutCommon.Mode = "IMPS";
            payoutCommon.MailID = "test@gmail.com";


            if (serviceData.APIID == 1)
            {
                payoutCommonResponse = await payoutService.SoniVerification(payoutCommon);
            }
            else if (serviceData.APIID == 6)
            {
                payoutCommonResponse = await payoutService.ClassypayVerification(payoutCommon);
            }


            try
            {
                await commonService.SaveLogs(new LogsRequestModel() { ServiceID = AccountVerificationService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = payoutCommonResponse.Request, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
            }
            catch { };



            if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
            {
                int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + charge + "," + mySurcharge + ",0,'Success','Verification','" + payoutCommonResponse.Message + "','" + agentID + "','" + payoutCommonResponse.VendorID + "','" + payoutCommonResponse.RRN + "','" + payoutCommonResponse.Result + "',0,'" + accountVerifyST.BankName + "','" + accountVerifyST.AccountNO + "','" + accountVerifyST.IFSC + "','ALL','" + payoutCommonResponse.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + accountVerifyST.CusName + "','" + accountVerifyST.CusMobile + "','" + payoutCommonResponse.Name + "','" + GSTCalculationC + "','" + accountVerifyST.Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + accountVerifyST.IP + "','" + SafeDMTService.ServiceID + "','" + item.OpID + "','" + _memberAllowDetails.MainBalance + "','" + (_memberAllowDetails.MainBalance - item.surcharge) + "'");
                if (Val > 0)
                {
                    DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + charge + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SAccountVerification + ",'" + agentID + "',0,0,0,'" + mySurcharge + "','" + GSTCalculationC + "'");
                    string jsonString = JsonSerializer.Serialize(payoutCommonResponse);
                    return ErrorShow.SuccessMessageAPI(payoutCommonResponse.Name);
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI("Try After Sometime");
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(payoutCommonResponse.Message);
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Insufficient Balance");
        }



    }

    public async Task<PayoutCommonResponse> VSoniTechno(AccountVerificationIserve request)
    {
        PayoutCommonResponse Response = new PayoutCommonResponse();

        AccountVerifyST accountVerifySTS = new AccountVerifyST();
        accountVerifySTS.APIID = SONITechnoCredentrial.APIID;
        accountVerifySTS.Token = SONITechnoCredentrial.Token;
        accountVerifySTS.MethodName = "accountverify";
        accountVerifySTS.OrderID = request.clientReferenceNo;
        accountVerifySTS.number = request.beneAccountNo;
        accountVerifySTS.ifsc = request.beneifsc;
        string Request = accountVerifySTS.GetJson();
        Response.Request = Request;
        string Result = await ApiPostCall.PostCallAsync(Request, SONITechnoCredentrial.MoneyURL);
        Response.Result = Result;

        JObject TransferData = JObject.Parse(Result);
        if (TransferData["code"].ToString().ToUpper() == "TXN")
        {
            if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS")
            {
                string Status = TransferData["status"].ToString();
                Response.Status = TransferData["data"]["Status"].ToString();
                Response.Message = TransferData["mess"].ToString();
                Response.VendorID = TransferData["data"]["STID"].ToString();
                Response.TXNCode = "";
                Response.AccountNo = request.beneAccountNo;
                Response.IFSC = request.beneifsc;
                Response.RRN = TransferData["data"]["RRN"].ToString();
                Response.Result = Result;
                Response.Name = TransferData["data"]["Name"].ToString();
                Response.OrderID = TransferData["data"]["OrderID"].ToString();
            }

        }
        else
        {
            Response.Status = "Failed";
            Response.Message = TransferData["mess"].ToString();
            Response.VendorID = TransferData["mess"].ToString();
            Response.TXNCode = "";
            Response.AccountNo = request.beneAccountNo;
            Response.IFSC = request.beneifsc;
            Response.RRN = "";
            Response.Result = Result;
            Response.Name = "";
            Response.OrderID = request.clientReferenceNo;
        }
        return Response;
    }


    public async Task<string> AddBeni(AddAccount addAccount, string MemberID, string CusID, string VerifyOTPAccount, string BankName)
    {
        Int32 Val = 0;
        Val = await cls.update_dataAsync("AddEditBeneficiaryDMT " + CusID + ",'" + addAccount.Name + "','" + addAccount.Mobile + "','" + addAccount.accountNumber + "','" + addAccount.IFSC + "','" + VerifyOTPAccount + "','" + BankName + "'");
        if (Val > 0)
        {
            return ErrorShow.SuccessMessageAPI("Add Beneficiary Successfully");
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Already exists");
        }

    }

    public async Task<Int32> DeleteBeni(string No)
    {

        return await cls.update_dataAsync("ManageBeneficiaryDMT 'Delete','" + No + "'");
    }

    private bool IsValidSlab(SlabModel slabDetails, decimal amount)
    {
        return slabDetails.startVal <= amount &&
               slabDetails.endVal >= amount &&
               slabDetails.startVal > 0 &&
               slabDetails.endVal > 0;
    }


    public async Task<string> DoTransaction(DMTRequestCommon addAccount, MemberAllowData _memberAllowDetails, Int32 factorAmount, string hddGroupTransID, ServiceWithAPIIDStatus serviceData, string Source, string CusNo, string CusName)
    {
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(factorAmount), serviceData.APIID.ToString(), addAccount.ServiceID, "MTE");
        if (!IsValidSlab(item, factorAmount))
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");
        addAccount.Amount = factorAmount;
        decimal mySurcharge = 0;
        mySurcharge = mySurcharge + item.surcharge;

        Decimal GSTCalculation = 0;// (mySurcharge - (mySurcharge * 100 / 118));
        Decimal PaySurcharge = 0;// (mySurcharge - GSTCalculation);

        GSTCalculation = (item.IsGst == false ? 0 : (mySurcharge * item.Gst) / 100);
        PaySurcharge = (mySurcharge - GSTCalculation);

        string agentID = await wallet.GetOrderID();
        addAccount.OrderID = agentID;
        string Dec = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";
        string narration = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";

        DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + factorAmount + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
        if (dtval.Rows.Count > 0)
        {
            if (dtval.Rows[0]["msrno"].ToString() == "1")
            {
                int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + factorAmount + "," + PaySurcharge + "," + addAccount.BeneID + ",'Pending','" + addAccount.Mode + "','','" + agentID + "','','','',0,'" + addAccount.BankName + "','" + addAccount.Accountno + "','" + addAccount.IFSC + "','ALL','" + addAccount.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + addAccount.CusName + "','" + addAccount.CusMobile + "','" + addAccount.Name + "','" + GSTCalculation + "','" + Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + addAccount.IP + "','" + addAccount.ServiceID + "'," + item.OpID + ",'" + _memberAllowDetails.MainBalance + "'");
                if (Val > 0)
                {
                    PayoutCommon payoutCommon = new PayoutCommon();
                    PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
                    payoutCommon.Amount = factorAmount;
                    payoutCommon.OrderID = addAccount.OrderID;
                    payoutCommon.AccountID = addAccount.BeneID;
                    payoutCommon.Mobile = addAccount.RemitterMobile;
                    payoutCommon.BankName = addAccount.BankName;
                    payoutCommon.Name = addAccount.Name;
                    payoutCommon.AccountNO = addAccount.Accountno;
                    payoutCommon.IFSC = addAccount.IFSC;
                    payoutCommon.Mode = addAccount.Mode;
                    payoutCommon.MailID = addAccount.Email;


                    if (serviceData.APIID == 1)
                        payoutCommonResponse = await payoutService.FundTransferInstantWithSoniTechno(payoutCommon);
                    else if (serviceData.APIID == 2)
                        payoutCommonResponse = await payoutService.FundTransferInstantWithPromptPe(payoutCommon);
                    else if (serviceData.APIID == 3)
                        payoutCommonResponse = await payoutService.FundTransferInstantWithBetaSource(payoutCommon);
                    else if (serviceData.APIID == 4)
                        payoutCommonResponse = await payoutService.FundTransferrechargkit(payoutCommon);
                    else if (serviceData.APIID == 6)
                        payoutCommonResponse = await payoutService.FundTransferclassypay(payoutCommon);
                    else
                    {
                        return ErrorShow.ErrorMessageAPI("API Not Allow For Payout");
                    }
                    try
                    {
                        await commonService.SaveLogs(new LogsRequestModel() { ServiceID = SafeDMTService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = payoutCommonResponse.Request, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
                    }
                    catch { };


                    if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
                    {
                        cls.update_data("Update MoneyTransfer set Status='" + payoutCommonResponse.Status + "',VendorID='" + payoutCommonResponse.VendorID + "',Response='" + payoutCommonResponse.Result + "',Message='" + payoutCommonResponse.Message + "',RRN='" + payoutCommonResponse.RRN + "' where OrderID='" + agentID + "'");
                        object returnData = new
                        {
                            agentID = hddGroupTransID,
                            Message = "Transaction Process",
                            status = payoutCommonResponse.Status
                        };

                        if (payoutCommonResponse.Status.ToUpper() == "SUCCESS")
                        {
                            Int32 UpdateCommission = await commissionService.GiveDMTCommission(agentID);
                        }

                        string Request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);
                        return ErrorShow.SuccessArrayAPI(Request);
                    }
                    else
                    {
                        Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Failed',RefundDate=getdate(),Message='" + payoutCommonResponse.Message + "',Response='" + payoutCommonResponse.Result + "' where OrderID='" + agentID + "'");
                        if (ValUpdate == 1)
                        {
                            dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + addAccount.Amount + ", 'CR','Refund - " + narration + "',''," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                            if (dtval.Rows.Count > 0)
                                return ErrorShow.ErrorMessageAPI(payoutCommonResponse.Message);
                            else
                                return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
                        }
                        else
                            return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
                    }
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI("Try After Sometime");
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Low Balance");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }
    }




    //public async Task<string> FundTransfer(DMTRequestCommon addAccount, string MemberID, string Source, string OutLetID)
    //{
    //    var serviceData = await serviceList.CheckServiceStatusWithAPIID(SafeDMTService.ServiceID);
    //    if (serviceData.Status == "Success")
    //    {
    //        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
    //        if (!_memberAllowDetails.HoldUnHold)
    //        {
    //            if (addAccount.Amount.ToString() != "" && addAccount.Amount > 0)
    //            {
    //                string hddGroupTransID = await wallet.GetOrderID();
    //                decimal MyBalance = 0;
    //                MyBalance = _memberAllowDetails.MainBalance;
    //                if (MyBalance >= (addAccount.Amount + _memberAllowDetails.HoldAMT))
    //                {

    //                    decimal mySurcharge = 0;
    //                    int factorAmount = addAccount.Amount;
    //                    int totAmount = addAccount.Amount;
    //                    int remainingAmount = factorAmount;
    //                    int loop = totAmount / factorAmount;
    //                    mySurcharge = 0;
    //                    var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(remainingAmount), serviceData.APIID.ToString(), SafeDMTService.ServiceID, "MTE");
    //                    if (!IsValidSlab(item, factorAmount))
    //                        return ErrorShow.ErrorMessageAPI("Slab configuration not set.");

    //                    mySurcharge = mySurcharge + item.surcharge;

    //                    Decimal GSTCalculation = (mySurcharge - (mySurcharge * 100 / 118));
    //                    Decimal PaySurcharge = (mySurcharge - GSTCalculation);
    //                    string agentID = await wallet.GetOrderID();
    //                    addAccount.OrderID = agentID;
    //                    string Dec = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";
    //                    string narration = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";

    //                    DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + addAccount.Amount + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
    //                    if (dtval.Rows.Count > 0)
    //                    {
    //                        if (dtval.Rows[0]["msrno"].ToString() == "1")
    //                        {
    //                            int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + addAccount.Amount + "," + PaySurcharge + "," + addAccount.BeneID + ",'Pending','" + addAccount.Mode + "','','" + agentID + "','','','',0,'" + addAccount.BankName + "','" + addAccount.Accountno + "','" + addAccount.IFSC + "','ALL','" + addAccount.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + addAccount.CusName + "','" + addAccount.CusMobile + "','" + addAccount.Name + "','" + GSTCalculation + "','" + Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + addAccount.IP + "','" + SafeDMTService.ServiceID + "'," + item.OpID + "");
    //                            if (Val > 0)
    //                            {
    //                                PayoutCommon payoutCommon = new PayoutCommon();
    //                                PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
    //                                payoutCommon.Amount = addAccount.Amount;
    //                                payoutCommon.OrderID = addAccount.OrderID;
    //                                payoutCommon.AccountID = addAccount.BeneID;
    //                                payoutCommon.Mobile = addAccount.RemitterMobile;
    //                                payoutCommon.BankName = addAccount.BankName;
    //                                payoutCommon.Name = addAccount.Name;
    //                                payoutCommon.AccountNO = addAccount.Accountno;
    //                                payoutCommon.IFSC = addAccount.IFSC;
    //                                payoutCommon.Mode = addAccount.Mode;
    //                                payoutCommon.MailID = _memberAllowDetails.ShopName.ToString();


    //                                if (serviceData.APIID == 2)
    //                                    payoutCommonResponse = await payoutService.FundTransferPaymonk(payoutCommon);
    //                                else if (serviceData.APIID == 3)
    //                                    payoutCommonResponse = await payoutService.FundTransferPaymonkNew(payoutCommon);
    //                                else if (serviceData.APIID == 1014)
    //                                    payoutCommonResponse = await payoutService.FundTransferJaijuirecharge(payoutCommon);
    //                                else if (serviceData.APIID == 1011)
    //                                    payoutCommonResponse = await payoutService.FundTransferOnepaisa(payoutCommon);

    //                                try
    //                                {
    //                                    await commonService.SaveLogs(new LogsRequestModel() { ServiceID = SafeDMTService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = payoutCommonResponse.Request, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
    //                                }
    //                                catch { };


    //                                if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
    //                                {
    //                                    cls.update_data("Update MoneyTransfer set Status='" + payoutCommonResponse.Status + "',VendorID='" + payoutCommonResponse.VendorID + "',Response='" + payoutCommonResponse.Result + "',Message='" + payoutCommonResponse.Message + "',RRN='" + payoutCommonResponse.RRN + "' where OrderID='" + agentID + "'");
    //                                    object returnData = new
    //                                    {
    //                                        agentID = agentID,
    //                                        Message = "Transaction Process",
    //                                        status = payoutCommonResponse.Status
    //                                    };

    //                                    if (payoutCommonResponse.Status.ToUpper() == "SUCCESS")
    //                                    {
    //                                        Int32 UpdateCommission = await commissionService.GiveDMTCommission(agentID);
    //                                    }

    //                                    string Request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);
    //                                    return ErrorShow.SuccessArrayAPI(Request);
    //                                }
    //                                else
    //                                {
    //                                    Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Failed',RefundDate=getdate(),Message='" + payoutCommonResponse.Message + "',Response='" + payoutCommonResponse.Result + "' where OrderID='" + agentID + "'");
    //                                    if (ValUpdate == 1)
    //                                    {
    //                                        dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + addAccount.Amount + ", 'CR','Refund - " + narration + "',''," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
    //                                        if (dtval.Rows.Count > 0)
    //                                            return ErrorShow.ErrorMessageAPI(payoutCommonResponse.Message);
    //                                        else
    //                                            return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
    //                                    }
    //                                    else
    //                                        return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
    //                                }
    //                            }
    //                            else
    //                            {
    //                                return ErrorShow.ErrorMessageAPI("Try After Sometime");
    //                            }
    //                        }
    //                        else
    //                        {
    //                            return ErrorShow.ErrorMessageAPI("Low Balance");
    //                        }
    //                    }
    //                    else
    //                    {
    //                        return ErrorShow.ErrorMessageAPI("Low Balance");
    //                    }
    //                }
    //                else
    //                {
    //                    return ErrorShow.ErrorMessageAPI("Insufficient Balance");
    //                }
    //            }
    //            else
    //            {
    //                return ErrorShow.ErrorMessageAPI("Please enter right Amount");
    //            }
    //        }
    //        else
    //            return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
    //    }
    //    else
    //        return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    //}



    public async Task<string> FundTransfer(DMTRequestCommon addAccount, string MemberID, string Source, string OutLetID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(addAccount.ServiceID);
        if (serviceData.Status == "Success")
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
            if (!_memberAllowDetails.HoldUnHold)
            {
                if (addAccount.Amount.ToString() != "" && addAccount.Amount > 0)
                {
                    string hddGroupTransID = await wallet.GetOrderID();
                    decimal MyBalance = 0;
                    MyBalance = _memberAllowDetails.MainBalance;
                    if (MyBalance >= (addAccount.Amount + _memberAllowDetails.HoldAMT))
                    {
                        string Result = "";

                        int factorAmount = addAccount.Amount;


                        int totAmount = addAccount.Amount.ToString().Replace(".00", "").ToInt32();
                        int remainingAmount = totAmount % factorAmount;
                        int loop = totAmount / factorAmount;
                        for (int i = 0; i < loop; i++)
                        {
                            Result = await DoTransaction(addAccount, _memberAllowDetails, factorAmount, hddGroupTransID, serviceData, Source, addAccount.CusMobile, addAccount.CusName);
                        }
                        if (remainingAmount > 0)
                        {
                            Result = await DoTransaction(addAccount, _memberAllowDetails, remainingAmount, hddGroupTransID, serviceData, Source, addAccount.CusMobile, addAccount.CusName);
                        }
                        return Result;


                    }
                    else
                    {
                        return ErrorShow.ErrorMessageAPI("Insufficient Balance");
                    }
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI("Please enter right Amount");
                }
            }
            else
                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }

    public async Task<string> GetBankListAsync(string OurLetID)
    {

        return await ApiPostCallInstantPay.GetCallDMTASYNC("", "banks", OurLetID);
    }

    public async Task<DataTable> GetBeniList(string ID)
    {

        return await cls.select_data_dtAsync("ManageBeneficiaryDMT 'GetAllActive','" + ID + "'");
    }

    public async Task<DataTable> GetBeniListWithAccount(string ID, string AccountNo)
    {

        return await cls.select_data_dtAsync("ManageBeneficiaryDMT 'GetAllActiveBYAccount','" + ID + "','" + AccountNo + "'");
    }

    public async Task<string> GetCusResult(string No, string OurLetID, string MemberID)
    {
        string Result = await ApiPostCallInstantPay.GetCallDMTASYNC(No, "remitterProfile?mobile=" + No + "", OurLetID);
        JObject Data = JObject.Parse(Result);
        if (Data["statuscode"].ToString().ToUpper() == "TXN")
        {
            DataTable dt = new DataTable();
            dt = await cls.select_data_dtAsync("AddEditCustomerDMT '" + $"{Data["data"]["firstName"].ToString()} {Data["data"]["lastName"].ToString()}" + "','" + Data["data"]["mobile"].ToString() + "','" + Data["data"]["address"].ToString() + "','',''," + MemberID + "");
            if (dt.Rows.Count > 0)
            {

            }
            return ErrorShow.SuccessArrayAPI(Data["data"].ToString());
        }
        else
        {

            return ErrorShow.ErrorMessageAPI(Data["status"].ToString());
        }
    }

    public async Task<DataTable> GetCusResultList(string No)
    {

        return await cls.select_data_dtAsync("CheckCustomerDMT '" + No + "'");
    }

    public async Task<DataTable> GetCustomerAsync(string Number)
    {

        return await cls.select_data_dtAsync("select * from CustomerDMT where mobile='" + Number + "' and isactive=1");
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


    public async Task<string> SendOTPOnRegistration(RemitterRegistration customerRegistration, string MemberID, string OutLetID)
    {

        DataTable dt = new DataTable();
        dt = await cls.select_data_dtAsync("AddEditCustomerDMT '" + $"{customerRegistration.firstName} {customerRegistration.lastName}" + "','" + customerRegistration.mobile + "','','',''," + MemberID + "");
        if (dt.Rows.Count > 0)
        {
            return ErrorShow.SuccessMessageAPI("Successfully Register");
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Already Add");
        }

    }

    public async Task<string> OTPValiDate(OtpVerification otpVerification, string MemberID, string OutLetID)
    {
        string Result = await ApiPostCallInstantPay.PostCallDMTASYNC(otpVerification.GetJson(), "otpVerification", OutLetID);
        JObject Data = JObject.Parse(Result);
        if (Data["statuscode"].ToString().ToUpper() != "OTP")
        {
            if (Data["statuscode"].ToString().ToUpper() == "TXN")
                return ErrorShow.SuccessMessageAPI(Data["status"].ToString());
            else
                return ErrorShow.ErrorMessageAPI(Data["status"].ToString());

        }
        else
        {
            return ErrorShow.SuccessMessageAPI(Data["status"].ToString());
        }
    }


    public async Task<Int32> AddBeniINDataBase(JArray Data, string CusID)
    {
        Int32 Val = 0;
        foreach (JObject item in Data)
        {
            Val = await cls.update_dataAsync("AddEditBeneficiaryDMT " + CusID + ",'" + item["name"].ToString() + "','','" + item["accno"].ToString() + "','" + item["ifsc"].ToString() + "','0','" + item["bankname"].ToString() + "','" + item["bankid"].ToString() + "'");

        }
        return 1;
    }

    public string decrypt(string text)
    {
        using (var aes = CreateAes(safepay.MerchantKey))
        {
            ICryptoTransform decryptor = aes.CreateDecryptor();
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

        }
    }


    AesManaged CreateAes(string key)
    {
        var aes = new AesManaged();
        aes.Key = Convert.FromBase64String(key); //UTF8-Encoding
        aes.IV = System.Text.Encoding.UTF8.GetBytes("0123456789abcdef");//UT8-Encoding
        return aes;
    }

    public async Task<string> HandleCallBack(string Data)
    {
        JObject jData = JObject.Parse(Data);
        JObject jDataResult = JObject.Parse(decrypt(jData["payload"].ToString()));
        DataTable dthistory = await cls.select_data_dtAsync("select * from Moneytransfer where status='Pending' and orderid='" + jDataResult["transactionDetails"]["orderRefNo"].ToString() + "'");
        DataTable dthistoryAEPSSettlement = await cls.select_data_dtAsync("select * from Moneytransfer where status='Pending' and orderid='" + jDataResult["transactionDetails"]["orderRefNo"].ToString() + "'");
        if (dthistory.Rows.Count > 0)
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(dthistory.Rows[0]["msrno"].ToString());

            string Dec = $"{dthistory.Rows[0]["AccountNo"].ToString()}/{dthistory.Rows[0]["IFSC"].ToString()}/{dthistory.Rows[0]["OrderID"].ToString()}";
            string narration = $"{dthistory.Rows[0]["AccountNo"].ToString()}/{dthistory.Rows[0]["IFSC"].ToString()}/{dthistory.Rows[0]["OrderID"].ToString()}";

            if (jDataResult["transactionDetails"]["txnStatus"].ToString() == "SUCCESS" && jDataResult["transactionDetails"]["statusDesc"].ToString() != "SPCD_1017-Awaiting confirmation from beneficiary bank")
            {
                Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Success',RRN='" + jDataResult["transactionDetails"]["bankRefNo"].ToString() + "',Message='" + jDataResult["transactionDetails"]["statusDesc"].ToString() + "',Response='" + decrypt(jData["payload"].ToString()) + "' where OrderID='" + dthistory.Rows[0]["OrderID"].ToString() + "'");
                Int32 UpdateCommission = await commissionService.GiveDMTCommission(dthistory.Rows[0]["OrderID"].ToString());

            }
            else if (jDataResult["transactionDetails"]["txnStatus"].ToString() == "FAILED" || jDataResult["transactionDetails"]["txnStatus"].ToString().ToUpper() == "REVERSAL")
            {
                Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Failed',Message='" + jDataResult["transactionDetails"]["statusDesc"].ToString() + "',Response='" + decrypt(jData["payload"].ToString()) + "' where OrderID='" + dthistory.Rows[0]["OrderID"].ToString() + "'");
                if (ValUpdate == 1)
                {
                    DataTable dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + dthistory.Rows[0]["Amount"].ToString() + ", 'CR','Refund - " + narration + "',''," + ConstantsData.SMoneyTransfer + ",'" + dthistory.Rows[0]["OrderID"].ToString() + "',0,0,0,'" + dthistory.Rows[0]["Surcharge"].ToString() + "','" + dthistory.Rows[0]["GST"].ToString() + "'");
                    if (dtval.Rows.Count > 0)
                    {
                        return ErrorShow.ErrorMessageAPI("Transaction Failed");
                    }
                }
            }
        }
        return "";
    }

    public async Task<string> HandleCallBackIserverPayout(string Data)
    {
        JObject jData = JObject.Parse(Data);
        DataTable dthistory = await cls.select_data_dtAsync("select * from Moneytransfer where status='Pending' and orderid='" + jData["ClientRefID"].ToString() + "'");
        if (dthistory.Rows.Count > 0)
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(dthistory.Rows[0]["msrno"].ToString());

            string Dec = $"{dthistory.Rows[0]["AccountNo"].ToString()}/{dthistory.Rows[0]["IFSC"].ToString()}/{dthistory.Rows[0]["OrderID"].ToString()}";
            string narration = $"{dthistory.Rows[0]["AccountNo"].ToString()}/{dthistory.Rows[0]["IFSC"].ToString()}/{dthistory.Rows[0]["OrderID"].ToString()}";

            if (jData["status"].ToString().ToUpper() == "SUCCESS")
            {
                Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Success',RRN='" + jData["rrn"].ToString() + "',Message='" + jData["statusDesc"].ToString() + "',Response='" + Data + "' where OrderID='" + dthistory.Rows[0]["OrderID"].ToString() + "'");
                Int32 UpdateCommission = await commissionService.GiveDMTCommission(dthistory.Rows[0]["OrderID"].ToString());

            }
            else if (jData["status"].ToString().ToUpper() == "FAILED" || jData["status"].ToString().ToUpper() == "REVERSAL")
            {
                Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Failed',Message='" + jData["statusDesc"].ToString() + "',Response='" + Data + "' where OrderID='" + dthistory.Rows[0]["OrderID"].ToString() + "'");
                if (ValUpdate == 1)
                {
                    DataTable dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + dthistory.Rows[0]["Amount"].ToString() + ", 'CR','Refund - " + narration + "',''," + ConstantsData.SMoneyTransfer + ",'" + dthistory.Rows[0]["OrderID"].ToString() + "',0,0,0,'" + dthistory.Rows[0]["Surcharge"].ToString() + "','" + dthistory.Rows[0]["GST"].ToString() + "'");
                    if (dtval.Rows.Count > 0)
                    {
                        return ErrorShow.ErrorMessageAPI("Transaction Failed");
                    }
                }
            }
        }
        return "";
    }


    public async Task<DataTable> GetDataByAccountNO(string AccountNO)
    {
        DataTable dthis = await cls.select_data_dtAsync("CheckAccountSearch '" + AccountNO + "'");
        return dthis;
    }

    public async Task<DataTable> GetBankList()
    {
        return await cls.select_data_dtAsync("select * from bank  where isactive=1");
    }
}