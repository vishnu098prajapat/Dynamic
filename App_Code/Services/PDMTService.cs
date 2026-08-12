using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Threading.Tasks;

public class PDMTService : IPDMTService
{
    cls_connection cls;
    public static string ServiceID = "16";
    IServiceList serviceList;
    ICommissionService commissionService;
    ICommonService commonService;
    IWallet wallet;

    public PDMTService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commonService = new CommonService();
        commissionService = new CommissionService();
    }

    public async Task<DataTable> GetBankListAsync()
    {
        return await cls.select_data_dtAsync("select bank_id as BANKID,bank_name as [BANK NAME],bank_ifsc as IFSC,CONVERT(nvarchar(30),added_on,109)Adddate from ezBanks");
    }

    public async Task<string> GetCusResult(string No)
    {
        PaysPrintDMTCheckCustomer customerRegistration = new PaysPrintDMTCheckCustomer();
        customerRegistration.mobile = No;
        customerRegistration.MethodName = "customerdetails";
        return await ApiPostCall.PostCallAsync(customerRegistration.GetJson(), SONITechnoCredentrial.DMT);
    }

    public async Task<string> GetBeniList(string ID)
    {
        PaysPrintDMTBeniList paysPrintDMTBeniList = new PaysPrintDMTBeniList();
        paysPrintDMTBeniList.mobile = ID;
        paysPrintDMTBeniList.MethodName = "beneficiarylist";
        return await ApiPostCall.PostCallAsync(paysPrintDMTBeniList.GetJson(), SONITechnoCredentrial.DMT);
    }

    public async Task<string> Registration(PaysPrintDMTCustomerRegister paysPrintDMTCustomerRegister, string MemberID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PDMTService.ServiceID);
        if (serviceData.Status != "Success") return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold) return ErrorShow.ErrorMessageAPI("Contact To Admin");
        var surchargeData = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), 0, serviceData.APIID.ToString(), PDMTService.ServiceID, "SKYC");
        if (surchargeData.surcharge == 0) return ErrorShow.ErrorMessageAPI("Slab configuration not set.");
        if (_memberAllowDetails.MainBalance < (surchargeData.surcharge + _memberAllowDetails.HoldAMT)) return ErrorShow.ErrorMessageAPI("Low Balance");
        decimal GSTCalculation = (surchargeData.surcharge - (surchargeData.surcharge * 100 / 118));
        decimal PaySurcharge = (surchargeData.surcharge - GSTCalculation);
        string agentID = await wallet.GetOrderID();
        string RequestJson = paysPrintDMTCustomerRegister.GetJson();
        string Result = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.DMT);
        JObject jData = JObject.Parse(Result);
        string Status = jData["code"].ToString().ToUpper() == "ERR" ? "FAILED" : "Success";
        int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + ",0," + PaySurcharge + ",0,'" + Status + "','Verification','','" + agentID + "','','','" + Result + "',0,'EKYC','EKYC','','ALL','EKYC','" + agentID + "'," + serviceData.APIID + ",'" + _memberAllowDetails.ShopName + "','" + _memberAllowDetails.Mobile + "','" + _memberAllowDetails.ShopName + "','" + GSTCalculation + "','Web','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','','" + PDMTService.ServiceID + "','" + surchargeData.OpID + "','" + _memberAllowDetails.MainBalance + "','" + (_memberAllowDetails.MainBalance - surchargeData.surcharge) + "'");
        if (Val > 0 && Status.ToUpper() == "SUCCESS") await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','0','DR','EKYC/Bank1/" + agentID + "',''," + ConstantsData.SMoneyTransferEKYC + ",'" + agentID + "',0,0,0," + PaySurcharge + "," + GSTCalculation + "");
        return Result;
    }

    public async Task<string> EKYC(DMTRequestekyc paysPrintDMTCustomerRegister, string MemberID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PDMTService.ServiceID);
        if (serviceData.Status != "Success") return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold) return ErrorShow.ErrorMessageAPI("Contact To Admin");
        return await ApiPostCall.PostCallAsync(paysPrintDMTCustomerRegister.GetJson(), SONITechnoCredentrial.DMT);
    }

    public async Task<string> AddBeni(PaysPrintDMTBeniRegister paysPrintDMTBeniRegister)
    {
        return await ApiPostCall.PostCallAsync(paysPrintDMTBeniRegister.GetJson(), SONITechnoCredentrial.DMT);
    }

    public async Task<string> DeleteBeni(string No, string Number)
    {
        DeleteBeniPaysP deleteBeniPaysP = new DeleteBeniPaysP() { bene_id = No, mobile = Number, MethodName = "deletebeneficiary" };
        return await ApiPostCall.PostCallAsync(deleteBeniPaysP.GetJson(), SONITechnoCredentrial.DMT);
    }

    public async Task<string> GetBeniDetails(string No, string CusNo)
    {
        PaysBeniFetch paysBeniFetch = new PaysBeniFetch() { beneid = Convert.ToInt32(No), mobile = CusNo, MethodName = "beneficiarylistid" };
        return await ApiPostCall.PostCallAsync(paysBeniFetch.GetJson(), SONITechnoCredentrial.DMT);
    }

    public async Task<Decimal> GetSurchargeValue(String Msrno, decimal Amount)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PDMTService.ServiceID);
        if (serviceData.Status != "Success") return 0;
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(Msrno);
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(Amount), serviceData.APIID.ToString(), PDMTService.ServiceID, "MT");
        return item.surcharge;
    }

    public async Task<string> FundTransfer(DMTRequestPaysprint addAccount, string MemberID, string Source, string CusNo, string CusName)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PDMTService.ServiceID);
        if (serviceData.Status != "Success") return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold) return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        string hddGroupTransID = await wallet.GetOrderID();
        if (_memberAllowDetails.MainBalance >= (addAccount.Amount + _memberAllowDetails.HoldAMT))
        {
            if (serviceData.APIID == 1)
            {
                int factorAmount = 5000;
                int totAmount = addAccount.Amount.ToString().Replace(".00", "").ToInt32();
                int loop = totAmount / factorAmount;
                int remainingAmount = totAmount % factorAmount;
                string Result = "";
                for (int i = 0; i < loop; i++) Result = await DoTransaction(addAccount, _memberAllowDetails, factorAmount, hddGroupTransID, serviceData, Source, CusNo, CusName, "0");
                if (remainingAmount > 0) Result = await DoTransaction(addAccount, _memberAllowDetails, remainingAmount, hddGroupTransID, serviceData, Source, CusNo, CusName, "0");
                return Result;
            }
            return ErrorShow.ErrorMessageAPI("API Not Assign");
        }
        return ErrorShow.ErrorMessageAPI("Insufficient Balance");
    }

    public async Task<string> FundTransfer(DMTRequestPaysprint addAccount, string MemberID, string Source, string CusNo, string CusName, string TXNO, string GUID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PDMTService.ServiceID);
        if (serviceData.Status != "Success") return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold) return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(addAccount.Amount), serviceData.APIID.ToString(), PDMTService.ServiceID, "MT");
        if (item.surcharge == 0 && addAccount.Amount > 0) return ErrorShow.ErrorMessageAPI("Slab configuration not set.");
        if (_memberAllowDetails.MainBalance >= (addAccount.Amount + _memberAllowDetails.HoldAMT + item.surcharge))
        {
            if (serviceData.APIID == 1) return await DoTransaction(addAccount, _memberAllowDetails, (int)addAccount.Amount, GUID, serviceData, Source, CusNo, CusName, TXNO);
            return ErrorShow.ErrorMessageAPI("API Not Assign");
        }
        return ErrorShow.ErrorMessageAPI("Insufficient Balance");
    }

    public async Task<string> FundTransferSendOTP(DMTRequestPaysprint addAccount, string MemberID, string Source, string CusNo, string CusName)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PDMTService.ServiceID);
        if (serviceData.Status != "Success") return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold) return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        int factorAmount = 5000;
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(factorAmount), serviceData.APIID.ToString(), PDMTService.ServiceID, "MT");
        if (_memberAllowDetails.MainBalance >= (addAccount.Amount + _memberAllowDetails.HoldAMT + item.surcharge))
        {
            if (serviceData.APIID == 1)
            {
                addAccount.lat = _memberAllowDetails.latitude.ToString();
                addAccount.@long = _memberAllowDetails.longitude.ToString();
                addAccount.OrderID = await wallet.GetOrderID();
                addAccount.MethodName = "sendamountotp";
                string result = await ApiPostCall.PostCallAsync(addAccount.GetJson(), SONITechnoCredentrial.DMT);
                JObject TransferData = JObject.Parse(result);
                return TransferData["code"].ToString() != "ERR" ? result : ErrorShow.ErrorMessageAPI(TransferData["mess"].ToString());
            }
            return ErrorShow.ErrorMessageAPI("API Not Assign");
        }
        return ErrorShow.ErrorMessageAPI("Insufficient Balance");
    }

    public async Task<string> DoTransaction(DMTRequestPaysprint addAccount, MemberAllowData _memberAllowDetails, Int32 factorAmount, string hddGroupTransID, ServiceWithAPIIDStatus serviceData, string Source, string CusNo, string CusName, string TNO)
    {
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(factorAmount), serviceData.APIID.ToString(), PDMTService.ServiceID, "MT");
        decimal GSTCalculation = (item.surcharge - (item.surcharge * 100 / 118));
        decimal PaySurcharge = (item.surcharge - GSTCalculation);
        string agentID = await wallet.GetOrderID();
        addAccount.OrderID = agentID;
        string Dec = $"{addAccount.AccountNO}/{addAccount.IFSC}/{agentID}";
        string narration = $"{addAccount.AccountNO}/{addAccount.IFSC}/{agentID}";

        DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + factorAmount + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");

        if (dtval.Rows.Count > 0 && dtval.Rows[0]["msrno"].ToString() == "1")
        {
            int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + factorAmount + "," + PaySurcharge + ",'" + addAccount.BeniID + "','Pending','" + addAccount.Mode + "','','" + agentID + "','','','',0,'" + addAccount.BankName + "','" + addAccount.AccountNO + "','" + addAccount.IFSC + "','ALL','" + addAccount.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + CusName + "','" + CusNo + "','" + addAccount.Name + "','" + GSTCalculation + "','" + Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','','" + PDMTService.ServiceID + "'," + item.OpID + "," + _memberAllowDetails.MainBalance + "");

            if (Val > 0)
            {
                addAccount.Amount = factorAmount;
                addAccount.lat = _memberAllowDetails.latitude.ToString();
                addAccount.@long = _memberAllowDetails.longitude.ToString();

                string result = await ApiPostCall.PostCallAsync(addAccount.GetJson(), SONITechnoCredentrial.DMT);
                JObject TransferData = JObject.Parse(result);

                if (TransferData["code"].ToString().ToUpper() != "ERR")
                {
                    string Status = TransferData["data"]["Status"].ToString();
                    if (Status.ToUpper() == "SUCCESS" || Status.ToUpper() == "PENDING")
                    {
                        cls.update_data("Update MoneyTransfer set Status='" + Status + "',VendorID='" + TransferData["data"]["STID"].ToString() + "',Response='" + result + "',Message='" + TransferData["mess"].ToString() + "',RRN='" + TransferData["data"]["RRN"].ToString() + "' where OrderID='" + agentID + "'");
                        if (Status.ToUpper() == "SUCCESS") await commissionService.GiveDMTCommission(agentID);
                        object returnData = new { agentID = agentID, Message = "Transaction Process", status = Status, UTR = TransferData["data"]["RRN"].ToString() };
                        return ErrorShow.SuccessArrayAPI(Newtonsoft.Json.JsonConvert.SerializeObject(returnData));
                    }
                }

                await cls.update_dataAsync("Update MoneyTransfer set Status='Failed',RefundDate=getdate(),Message='" + TransferData["mess"].ToString() + "',Response='" + result + "' where OrderID='" + agentID + "'");
                await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + factorAmount + ", 'CR','Refund - " + narration + "',''," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                return ErrorShow.ErrorMessageAPI(TransferData["mess"].ToString());
            }
            return ErrorShow.ErrorMessageAPI("Try After Sometime");
        }
        return ErrorShow.ErrorMessageAPI("Low Balance");
    }

    public async Task<string> AccountVerificationNew(AccountVerificationNew accountVerifyST, string MemberID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(PDMTService.ServiceID);
        if (serviceData.Status != "Success") return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        string agentID = await wallet.GetOrderID();
        decimal mySurcharge = 2.54M;
        decimal GSTCalculationC = (mySurcharge * 18 / 100);
        accountVerifyST.OrderID = agentID;
        string result = await ApiPostCall.PostCallAsync(JsonConvert.SerializeObject(new {APIID=SONITechnoCredentrial.APIID,Token=SONITechnoCredentrial.Token, MethodName = "accountverify", OrderID = agentID, number = accountVerifyST.number, ifsc = accountVerifyST.ifsc }), SONITechnoCredentrial.DMT);
        JObject jObject = JObject.Parse(result);
        if (jObject["code"].ToString().ToUpper() == "TXN")
        {
            await cls.update_dataAsync("AddEditMoneyTransfer " + MemberID + ",1," + mySurcharge + ",0,'Success','IMPS','','" + agentID + "','','" + jObject["data"]["RRN"].ToString() + "','" + result + "'," + accountVerifyST.BankID + ",'" + accountVerifyST.BankName + "','" + accountVerifyST.number + "','" + accountVerifyST.ifsc + "','ALL','" + jObject["data"]["Name"].ToString() + "',''," + serviceData.APIID + ",'" + accountVerifyST.CName + "','" + accountVerifyST.CNumber + "','" + jObject["data"]["Name"].ToString() + "'," + GSTCalculationC + ",'" + accountVerifyST.Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + accountVerifyST.IP + "',0,0");
            await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','1','DR','" + accountVerifyST.number + "/" + accountVerifyST.ifsc + "/" + agentID + "',''," + ConstantsData.SAccountVerification + ",'" + agentID + "',0,0,0,'" + mySurcharge + "','" + GSTCalculationC + "'");
            return ErrorShow.SuccessMessageAPI(jObject["data"]["Name"].ToString());
        }
        return ErrorShow.ErrorMessageAPI(jObject["mess"].ToString());
    }
}