using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for EDMTService
/// </summary>
public class EDMTService : IEDMTService
{
    cls_connection cls;
    private static string ServiceID = "37";
    IServiceList serviceList;
    ICommissionService commissionService;
    IWallet wallet;
    IPayoutService payoutService;
    ICommonService commonService;
    public EDMTService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commissionService = new CommissionService();
        payoutService = new PayoutService();
        commonService = new CommonService();
    }

    public async Task<string> AccountVerification(CommonAccountVerificationRequest accountVerifyST, string MemberID)
    {

        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(EDMTService.ServiceID);
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
            EDMTService.ServiceID, "DMTV"
        );

        if (item.surcharge == 0)
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


        string hddGroupTransID = await wallet.GetOrderID();
        decimal MyBalance = 0;
        decimal charge = 0M;
        decimal mySurcharge = item.surcharge;
        MyBalance = _memberAllowDetails.MainBalance;


        Decimal GSTCalculation = 0;// (mySurcharge - (mySurcharge * 100 / 118));
        Decimal PaySurcharge = 0;// (mySurcharge - GSTCalculation);
        const decimal gstRate = 18m;
        decimal gstMultiplier = gstRate / 100;

        PaySurcharge = mySurcharge;
        if (item.IsGst)
        {
            GSTCalculation = item.IsPlusMinGst
       ? item.surcharge * gstMultiplier
       : item.surcharge - (item.surcharge / (1 + gstMultiplier));

            PaySurcharge = item.IsPlusMinGst
                ? item.surcharge
                : item.surcharge - GSTCalculation;
        }


        if (MyBalance >= (charge + _memberAllowDetails.HoldAMT + mySurcharge))
        {


            string agentID = await wallet.GetOrderID();
            accountVerifyST.OrderID = agentID;

            string Dec = $"{accountVerifyST.AccountNO}/{accountVerifyST.IFSC}/{agentID}";
            string narration = $"{accountVerifyST.AccountNO}/{accountVerifyST.IFSC}/{agentID}";

            //Iserver
            AccountVerificationIserve request = new AccountVerificationIserve();
            request.beneName = "Soni";
            request.beneAccountNo = accountVerifyST.AccountNO;
            request.beneifsc = accountVerifyST.IFSC;
            request.benePhoneNo = Convert.ToInt64(_memberAllowDetails.Mobile);
            request.clientReferenceNo = agentID;
            Location location = new Location();
            location = location.GetLocationDataCurrent(accountVerifyST.IP);
            request.latlong = $"{location.Latitude},{location.Longitude}";
            request.pincode = 302020;
            request.custMobNo = Convert.ToInt64(accountVerifyST.CusMobile);
            request.custName = "Vipin";
            request.custIpAddress = accountVerifyST.IP;
            request.beneBankName = accountVerifyST.BankName;
            request.paramA = "";
            request.paramB = "";
            PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
            // if (serviceData.APIID == 1)
            payoutCommonResponse = await VSoniTechno(request);


            try
            {
                await commonService.SaveLogs(new LogsRequestModel() { ServiceID = AccountVerificationService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = payoutCommonResponse.Request, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
            }
            catch { };



            if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
            {
                int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + charge + "," + PaySurcharge + ",0,'Success','Verification','" + payoutCommonResponse.Message + "','" + agentID + "','" + payoutCommonResponse.VendorID + "','" + payoutCommonResponse.RRN + "','" + payoutCommonResponse.Result + "',0,'" + accountVerifyST.BankName + "','" + accountVerifyST.AccountNO + "','" + accountVerifyST.IFSC + "','ALL','" + payoutCommonResponse.Name + "','" + hddGroupTransID + "',1,'" + accountVerifyST.CusName + "','" + accountVerifyST.CusMobile + "','" + payoutCommonResponse.Name + "','" + GSTCalculation + "','" + accountVerifyST.Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + accountVerifyST.IP + "','" + EDMTService.ServiceID + "','" + item.OpID + "','" + _memberAllowDetails.MainBalance + "','" + (_memberAllowDetails.MainBalance - item.surcharge) + "'");
                if (Val > 0)
                {
                    DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + charge + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SAccountVerification + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
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

    public async Task<string> CheckRemitterRegistration(string No, string ServiceID)
    {
        object Requst = new
        {
            requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
            token = ezypay.token,
            service = ServiceID,
            sender = No
        };


        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Dmt/checkRemitterRegistration");
        JObject jData = JObject.Parse(Result);
        if (jData["Resp_code"].ToString() != "RCS")
        {
            return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
        }
        else
        {
            return ErrorShow.SuccessArrayAPI(jData["data"].ToString());
        }

    }

    public async Task<string> EKYC(object Requst, string MemberID, string ServiceID)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(EDMTService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }


        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (!_memberAllowDetails.HoldUnHold)
        {
            //// Step 5: Get surcharge and commission details
            var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
                _memberAllowDetails.PackgeID.ToString(),
                0,
                serviceData.APIID.ToString(),
                EDMTService.ServiceID, "SKYC"
            );

            if (item.surcharge == 0)
                return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


            if (_memberAllowDetails.MainBalance > (item.surcharge + _memberAllowDetails.HoldAMT))
            {
                Decimal GSTCalculation = (item.surcharge - (item.surcharge * 100 / 118));
                Decimal PaySurcharge = (item.surcharge - GSTCalculation);
                string agentID = await wallet.GetOrderID();

                string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
                string Result = await ezypay.Postcall(RequestJson, "Dmt/remitter_ekyc");
                Result = Result.Replace("'", "");
                try
                {
                    await commonService.SaveLogs(new LogsRequestModel() { ServiceID = EDMTService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = RequestJson, ResponseData = Result, CallBackData = "" });
                }
                catch { };

                JObject jData = JObject.Parse(Result);
                if (jData["Resp_code"].ToString() == "RCS")
                {
                    string Status = "Success";
                    if (jData["data"]["txn_status"].ToString().ToUpper() == "FAILED")
                    {
                        Status = "FAILED";
                    }
                    int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + ",0," + PaySurcharge + ",0,'" + Status + "','Verification','" + jData["data"]["txn_desc"].ToString() + "','" + agentID + "','" + jData["data"]["txnid"].ToString() + "','','" + Result + "',0,'EKYC','EKYC','','ALL','EKYC','" + agentID + "'," + serviceData.APIID + ",'" + _memberAllowDetails.ShopName + "','" + _memberAllowDetails.Mobile + "','" + _memberAllowDetails.ShopName + "','" + GSTCalculation + "','Web','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','','" + EDMTService.ServiceID + "','" + item.OpID + "','" + _memberAllowDetails.MainBalance + "','" + (_memberAllowDetails.MainBalance - item.surcharge) + "'");
                    if (Val > 0)
                    {
                        string narration = "EKYC/Bank1/" + agentID;
                        if (Status.ToUpper() == "SUCCESS")
                        {
                            await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','0','DR','" + narration + "',''," + ConstantsData.SMoneyTransferEKYC + ",'" + agentID + "',0,0,0," + PaySurcharge + "," + GSTCalculation + "");
                            return ErrorShow.SuccessMessageAPI(jData["data"]["txn_desc"].ToString());

                        }
                        else
                        {
                            return ErrorShow.ErrorMessageAPI(jData["data"]["txn_desc"].ToString());
                        }
                    }
                    else
                    {
                        return ErrorShow.ErrorMessageAPI("Try After Sometime");
                    }

                }
                else
                {
                    return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Low Balance");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }
    }

    public async Task<Decimal> GetSurchargeValue(String Msrno, decimal Amount, string ServiceID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(EDMTService.ServiceID);
        if (serviceData.Status == "Success")
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(Msrno);
            var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Amount, serviceData.APIID.ToString(), EDMTService.ServiceID, "MT");
            return item.surcharge;
        }
        else
            return 0;
    }
    public async Task<string> VerifyRemitterAadhaar(string No, string Aadhar, string MemberID, string ServiceID)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(EDMTService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (!_memberAllowDetails.HoldUnHold)
        {
            //// Step 5: Get surcharge and commission details
            var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
                _memberAllowDetails.PackgeID.ToString(),
                0,
                serviceData.APIID.ToString(),
                EDMTService.ServiceID, "SKYC"
            );

            if (item.surcharge == 0)
                return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


            if (_memberAllowDetails.MainBalance > (item.surcharge + _memberAllowDetails.HoldAMT))
            {
                object Requst = new
                {
                    requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
                    token = ezypay.token,
                    service = ServiceID,
                    sender = No,
                    aadhaar = Aadhar
                };
                string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
                string Result = await ezypay.Postcall(RequestJson, "Dmt/verify_remitter_aadhaar");
                JObject jData = JObject.Parse(Result);
                if (jData["Resp_code"].ToString() == "RCS")
                {
                    return ErrorShow.SuccessArrayAPI(jData["data"].ToString());
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Low Balance");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

    }

    public async Task<string> VerifyRemitterOtp(object Requst, string Msrno, string ServiceID)
    {
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Dmt/verify_remitter_otp");
        JObject jData = JObject.Parse(Result);
        if (jData["Resp_code"].ToString() == "RCS")
        {
            return ErrorShow.SuccessArrayAPI(jData["data"].ToString());
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
        }

    }

    public async Task<string> GetBeneficiaryList(string No, string ServiceID)
    {
        object Requst = new
        {
            requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
            token = ezypay.token,
            service = ServiceID,
            sender = No
        };
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Dmt/beneficiaryList");
        JObject jData = JObject.Parse(Result);
        if (jData["Resp_code"].ToString() == "RNFVAD")
        {
            return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
        }
        else
        {
            return ErrorShow.SuccessArrayAPI(jData["data"].ToString());
        }

    }

    public async Task<string> GetBankList()
    {
        object Requst = new
        {
            requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
            token = ezypay.token
        };
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Dmt/bankList");
        JObject jData = JObject.Parse(Result);
        if (jData["Resp_code"].ToString() != "RCS")
        {
            return ErrorShow.ErrorMessageAPI(jData["data"].ToString());
        }
        else
        {
            return ErrorShow.SuccessArrayAPI(jData["data"].ToString());
        }

    }


    public async Task<string> AddBeneficiary(object Requst, string ServiceID)
    {
        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Dmt/addBeneficiary");
        JObject jData = JObject.Parse(Result);
        if (jData["Resp_code"].ToString() == "RCS")
        {
            return ErrorShow.SuccessMessageAPI(jData["Resp_desc"].ToString());
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
        }
    }


    public async Task<string> DeleteBeneficiaryRequest(string No, string beneficiaryid, string ServiceID)
    {
        object Requst = new
        {
            requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
            token = ezypay.token,
            service = ServiceID,
            sender = No,
            beneficiaryid = beneficiaryid,
        };

        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Dmt/deleteBeneficiaryRequest");
        JObject jData = JObject.Parse(Result);
        if (jData["Resp_code"].ToString() == "RCS")
        {
            return ErrorShow.SuccessMessageAPI(jData["Resp_desc"].ToString());
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
        }

    }


    public async Task<string> InititateAfaRequest(string No, string ServiceID)
    {
        object Requst = new
        {
            requestid = System.Guid.NewGuid().ToString().Replace("-", ""),
            token = ezypay.token,
            service = ServiceID,
            sender = No
        };

        string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(Requst);
        string Result = await ezypay.Postcall(RequestJson, "Dmt/inititate_afa_request");
        JObject jData = JObject.Parse(Result);
        if (jData["Resp_code"].ToString() == "VRF")
        {
            return ErrorShow.SuccessArrayAPI(jData["data"].ToString());
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(jData["Resp_desc"].ToString());
        }

    }

    private bool IsValidSlab(SlabModel slabDetails, decimal amount)
    {
        return slabDetails.startVal <= amount &&
               slabDetails.endVal >= amount &&
               slabDetails.startVal > 0 &&
               slabDetails.endVal > 0;
    }


    public async Task<string> FundTransfer(DMTRequestCommon addAccount, string MemberID, string Source, string OutLetID, string ServiceID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(EDMTService.ServiceID);
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
                        decimal mySurcharge = 0;
                        int factorAmount = addAccount.Amount;
                        int totAmount = addAccount.Amount;
                        int remainingAmount = factorAmount;
                        int loop = totAmount / factorAmount;
                        mySurcharge = 0;
                        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(remainingAmount), serviceData.APIID.ToString(), EDMTService.ServiceID, "MT");

                        if (!IsValidSlab(item, factorAmount))
                            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");

                        mySurcharge = mySurcharge + item.surcharge;

                        Decimal GSTCalculation = 0;// (mySurcharge - (mySurcharge * 100 / 118));
                        Decimal PaySurcharge = 0;// (mySurcharge - GSTCalculation);
                        const decimal gstRate = 18m;
                        decimal gstMultiplier = gstRate / 100;

                        PaySurcharge = mySurcharge;
                        if (item.IsGst)
                        {
                            GSTCalculation = item.IsPlusMinGst
                       ? item.surcharge * gstMultiplier
                       : item.surcharge - (item.surcharge / (1 + gstMultiplier));

                            PaySurcharge = item.IsPlusMinGst
                                ? item.surcharge
                                : item.surcharge - GSTCalculation;
                        }


                        string agentID = await wallet.GetOrderID();
                        addAccount.OrderID = agentID;
                        string Dec = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";
                        string narration = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";

                        DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + addAccount.Amount + "','DR','" + narration + "','" + Dec + "'," + EDMTService.ServiceID + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                        if (dtval.Rows.Count > 0)
                        {
                            if (dtval.Rows[0]["msrno"].ToString() == "1")
                            {
                                int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + addAccount.Amount + "," + PaySurcharge + "," + addAccount.BeneID + ",'Pending','" + addAccount.Mode + "','','" + agentID + "','','','',0,'" + addAccount.BankName + "','" + addAccount.Accountno + "','" + addAccount.IFSC + "','ALL','" + addAccount.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + addAccount.CusName + "','" + addAccount.CusMobile + "','" + addAccount.Name + "','" + GSTCalculation + "','" + Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + addAccount.IP + "','" + EDMTService.ServiceID + "'," + item.OpID + "");
                                if (Val > 0)
                                {
                                    PayoutCommon payoutCommon = new PayoutCommon();
                                    PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
                                    payoutCommon.Amount = addAccount.Amount;
                                    payoutCommon.OrderID = addAccount.OrderID;
                                    payoutCommon.AccountID = ServiceID;
                                    payoutCommon.Mobile = addAccount.RemitterMobile;
                                    payoutCommon.BankName = addAccount.BankName;
                                    payoutCommon.Name = addAccount.Name;
                                    payoutCommon.AccountNO = addAccount.Accountno;
                                    payoutCommon.IFSC = addAccount.IFSC;
                                    payoutCommon.Mode = addAccount.Mode;
                                    payoutCommon.identifier = addAccount.identifier;
                                    payoutCommon.AgentMobile = addAccount.RemitterMobile;
                                    payoutCommon.otp = addAccount.OTP;
                                    payoutCommon.BeneID = addAccount.BeneID;
                                    payoutCommon.MailID = _memberAllowDetails.ShopName.ToString();

                                    payoutCommonResponse = await payoutService.FundTransferezypay(payoutCommon);

                                    try
                                    {
                                        await commonService.SaveLogs(new LogsRequestModel() { ServiceID = EDMTService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = payoutCommonResponse.Request, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
                                    }
                                    catch { };


                                    if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
                                    {
                                        cls.update_data("Update MoneyTransfer set Status='" + payoutCommonResponse.Status + "',VendorID='" + payoutCommonResponse.VendorID + "',Response='" + payoutCommonResponse.Result + "',Message='" + payoutCommonResponse.Message + "',RRN='" + payoutCommonResponse.RRN + "' where OrderID='" + agentID + "'");
                                        object returnData = new
                                        {
                                            agentID = agentID,
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
                                            dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + addAccount.Amount + ", 'CR','Refund - " + narration + "',''," + EDMTService.ServiceID + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
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

    public async Task<string> FundTransferWithStep(DMTRequestCommon addAccount, string MemberID, string Source, string OutLetID, string ServiceID, string TNO, string GID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(EDMTService.ServiceID);
        if (serviceData.Status == "Success")
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
            if (!_memberAllowDetails.HoldUnHold)
            {
                if (addAccount.Amount.ToString() != "" && addAccount.Amount > 0)
                {
                    string hddGroupTransID = GID;
                    decimal MyBalance = 0;
                    MyBalance = _memberAllowDetails.MainBalance;
                    if (MyBalance >= (addAccount.Amount + _memberAllowDetails.HoldAMT))
                    {
                        decimal mySurcharge = 0;
                        int factorAmount = addAccount.Amount;
                        int totAmount = addAccount.Amount;
                        int remainingAmount = factorAmount;
                        int loop = totAmount / factorAmount;
                        mySurcharge = 0;
                        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(remainingAmount), serviceData.APIID.ToString(), EDMTService.ServiceID, "MT");

                        if (!IsValidSlab(item, factorAmount))
                            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");

                        mySurcharge = mySurcharge + item.surcharge;

                        Decimal GSTCalculation = (mySurcharge - (mySurcharge * 100 / 118));
                        Decimal PaySurcharge = (mySurcharge - GSTCalculation);
                        string agentID = await wallet.GetOrderID();
                        addAccount.OrderID = agentID;
                        string Dec = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";
                        string narration = $"{addAccount.Accountno}/{addAccount.IFSC}/{agentID}";

                        DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + addAccount.Amount + "','DR','" + narration + "','" + Dec + "'," + EDMTService.ServiceID + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                        if (dtval.Rows.Count > 0)
                        {
                            if (dtval.Rows[0]["msrno"].ToString() == "1")
                            {
                                int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + addAccount.Amount + "," + PaySurcharge + "," + addAccount.BeneID + ",'Pending','" + addAccount.Mode + "','','" + agentID + "','','','',0,'" + addAccount.BankName + "','" + addAccount.Accountno + "','" + addAccount.IFSC + "','ALL','" + addAccount.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + addAccount.CusName + "','" + addAccount.CusMobile + "','" + addAccount.Name + "','" + GSTCalculation + "','" + Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + addAccount.IP + "','" + EDMTService.ServiceID + "'," + item.OpID + "," + _memberAllowDetails.MainBalance + "," + (_memberAllowDetails.MainBalance - addAccount.Amount - GSTCalculation - PaySurcharge) + "");
                                if (Val > 0)
                                {
                                    PayoutCommon payoutCommon = new PayoutCommon();
                                    PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();
                                    payoutCommon.Amount = addAccount.Amount;
                                    payoutCommon.OrderID = addAccount.OrderID;
                                    payoutCommon.AccountID = ServiceID;
                                    payoutCommon.Mobile = addAccount.RemitterMobile;
                                    payoutCommon.BankName = addAccount.BankName;
                                    payoutCommon.Name = addAccount.Name;
                                    payoutCommon.AccountNO = addAccount.Accountno;
                                    payoutCommon.IFSC = addAccount.IFSC;
                                    payoutCommon.Mode = addAccount.Mode;
                                    payoutCommon.identifier = addAccount.identifier;
                                    payoutCommon.AgentMobile = addAccount.RemitterMobile;
                                    payoutCommon.otp = addAccount.OTP;
                                    payoutCommon.BeneID = addAccount.BeneID;
                                    payoutCommon.MailID = _memberAllowDetails.ShopName.ToString();

                                    payoutCommonResponse = await payoutService.FundTransferezypay(payoutCommon);

                                    try
                                    {
                                        await commonService.SaveLogs(new LogsRequestModel() { ServiceID = EDMTService.ServiceID.ToInt32(), APIID = serviceData.APIID, TransID = agentID, RequestData = payoutCommonResponse.Result, ResponseData = payoutCommonResponse.Result, CallBackData = "" });
                                    }
                                    catch { };


                                    if (payoutCommonResponse.Status.ToUpper() == "SUCCESS" || payoutCommonResponse.Status.ToUpper() == "PENDING")
                                    {
                                        cls.update_data("Update MoneyTransfer set Status='" + payoutCommonResponse.Status + "',VendorID='" + payoutCommonResponse.VendorID + "',Response='" + payoutCommonResponse.Result + "',Message='" + payoutCommonResponse.Message + "',RRN='" + payoutCommonResponse.RRN + "' where OrderID='" + agentID + "'");
                                        object returnData = new
                                        {
                                            agentID = agentID,
                                            Message = "Transaction Process",
                                            status = payoutCommonResponse.Status,
                                            TXNO = TNO,
                                            Status = payoutCommonResponse.Status,
                                            UTR = payoutCommonResponse.RRN,
                                            Reason = payoutCommonResponse.Message,
                                            GID = GID,
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
                                            dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + addAccount.Amount + ", 'CR','Refund - " + narration + "',''," + EDMTService.ServiceID + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
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
}