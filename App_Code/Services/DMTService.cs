using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Summary description for DMTService
/// </summary>
public class DMTService : IDMTService
{
    cls_connection cls;
    IServiceList serviceList;
    IWallet wallet;
    private static string ServiceID = "16";

    public DMTService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
    }

    public async Task<Decimal> GetSurchargeValue(String Msrno, decimal Amount)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(DMTService.ServiceID);
        if (serviceData.Status == "Success")
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(Msrno);
            var item = MoneyTransfer_EkoAPI.GetSurchargeCommonCommisison(_memberAllowDetails.PackgeID.ToString(), Amount, serviceData.APIID.ToString(), DMTService.ServiceID);
            return item.Item1;
        }
        else
            return 0;
    }

    public async Task<string> AccountVerification(AccountVerifyST accountVerifyST, string MemberID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(DMTService.ServiceID);
        if (serviceData.Status == "Success")
        {
            decimal MyBalance = 0;
            string agentID = await wallet.GetOrderID();
            string hddGroupTransID = await wallet.GetOrderID();

            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
            if (!_memberAllowDetails.HoldUnHold)
            {
                MyBalance = _memberAllowDetails.MainBalance;
                if (MyBalance >= Convert.ToDecimal(MoneyTransfer_EkoAPI.Beneficiary_VerificationFee)) // Verification fee
                {
                    if (MoneyTransfer_EkoAPI.CheckMinimumBalance(Convert.ToInt32(MemberID), Convert.ToDecimal(MoneyTransfer_EkoAPI.Beneficiary_VerificationFee), MyBalance))
                    {
                        string Dec = $"{accountVerifyST.number}/{accountVerifyST.ifsc}/{agentID}";
                        string narration = $"{accountVerifyST.number}/{accountVerifyST.ifsc}/{agentID}";
                        decimal deductAmount = (Convert.ToDecimal(MoneyTransfer_EkoAPI.Beneficiary_VerificationFee));
                        accountVerifyST.OrderID = agentID;
                        string result = await ApiPostCall.PostCallAsync(accountVerifyST.GetJson(), SONITechnoCredentrial.MoneyURL);
                        JObject jObject = JObject.Parse(result);
                        if (jObject["code"].ToString().ToUpper() == "TXN")
                        {
                            int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + MemberID + "," + MoneyTransfer_EkoAPI.Beneficiary_VerificationFee + ",0,0,'Success','IMPS','','" + agentID + "','','" + jObject["data"]["RRN"].ToString() + "','" + result + "'");
                            if (Val > 0)
                            {
                                DataTable dtewal = await cls.select_data_dtAsync("exec [PROC_EWALLETTRANSACTION]  '" + _memberAllowDetails.LoginID + "', " + deductAmount + ", 'Dr','" + narration + "','" + Dec + "'," + ConstantsData.SAccountVerification + ",'" + agentID + "'");
                                return ErrorShow.SuccessMessageAPI(jObject["data"]["Name"].ToString());
                            }
                            else
                            {
                                return ErrorShow.ErrorMessageAPI("Try After Sometime");
                            }
                        }
                        else
                        {
                            return ErrorShow.ErrorMessageAPI(jObject["mess"].ToString());
                        }

                    }
                    else
                    {
                        return ErrorShow.ErrorMessageAPI("Low Balance! Minimum balance limit exceed.");
                    }
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI("You have Insufficient Balance.");
                }
            }
            else
                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }

    public async Task<string> AddBeni(AddAccount addAccount, string MemberID, string CusID, string VerifyOTPAccount, string BankName)
    {
        string Result = await ApiPostCall.PostCallAsync(addAccount.GetJson(), SONITechnoCredentrial.MoneyURL);
        JObject Data = JObject.Parse(Result);
        if (Data["code"].ToString().ToUpper() == "ERR")
        {
            return ErrorShow.ErrorMessageAPI(Data["mess"].ToString());
        }
        else
        {

            Int32 Val = 0;
            Val = cls.update_data("AddEditBeneficiaryDMT " + CusID + ",'" + addAccount.Name + "','" + addAccount.Mobile + "','" + addAccount.accountNumber + "','" + addAccount.IFSC + "','" + VerifyOTPAccount + "','" + BankName + "'");
            if (Val > 0)
            {
                return ErrorShow.SuccessMessageAPI("Add Beneficiary Successfully");
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Already exists");
            }
        }

    }

    public async Task<Int32> DeleteBeni(string No)
    {

        return await cls.update_dataAsync("ManageBeneficiaryDMT 'Delete','" + No + "'");
    }

    public async Task<string> FundTransfer(JObject addAccount, string MemberID,string Source)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(DMTService.ServiceID);
        if (serviceData.Status == "Success")
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
            if (!_memberAllowDetails.HoldUnHold)
            {
                if (addAccount["Amount"].ToString() != "" && Convert.ToDouble(addAccount["Amount"].ToString()) > 0)
                {
                    DataTable dtCUS = await GetCustomerAsync(addAccount["CusID"].ToString());
                    if (dtCUS.Rows.Count > 0)
                    {
                        double remianinSendLimit = Convert.ToDouble(dtCUS.Rows[0]["Limit"].ToString());
                        if (remianinSendLimit >= Convert.ToDouble(addAccount["Amount"].ToString()))
                        {

                            if (Convert.ToDouble(addAccount["Amount"].ToString()) <= Convert.ToDouble(dtCUS.Rows[0]["SingleSlot"].ToString()))
                            {
                                string hddGroupTransID = await wallet.GetOrderID();

                                decimal MyBalance = 0;
                                MyBalance = _memberAllowDetails.MainBalance;
                                if (MyBalance >= Convert.ToDecimal(addAccount["Amount"].ToString()))
                                {
                                    if (Convert.ToInt32(addAccount["BeniID"].ToString()) > 0)
                                    {
                                        DataTable dtBeniList = await cls.select_data_dtAsync("select * from BeneficiaryDMT where id='" + addAccount["BeniID"].ToString() + "' and isactive=1");
                                        if (dtBeniList.Rows.Count > 0)
                                        {
                                            decimal mySurcharge = 0;
                                            int factorAmount = Convert.ToInt32(addAccount["Amount"].ToString());
                                            int totAmount = Convert.ToInt32(addAccount["Amount"].ToString());
                                            int remainingAmount = factorAmount;
                                            int loop = totAmount / factorAmount;

                                            mySurcharge = 0;

                                            var item = MoneyTransfer_EkoAPI.GetSurchargeCommonCommisison(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(factorAmount), serviceData.APIID.ToString(), DMTService.ServiceID);

                                            mySurcharge = item.Item1;

                                            Decimal GSTCalculation = (mySurcharge - (mySurcharge * 100 / 118));
                                            Decimal PaySurcharge = (mySurcharge - GSTCalculation);

                                            string agentID = System.Guid.NewGuid().ToString().Replace('-', '0').Substring(1, 20);


                                            string Dec = $"{dtBeniList.Rows[0]["accountNumber"].ToString()}/{dtBeniList.Rows[0]["IFSC"].ToString()}/{agentID}";
                                            string narration = $"{dtBeniList.Rows[0]["accountNumber"].ToString()}/{dtBeniList.Rows[0]["IFSC"].ToString()}/{agentID}";

                                            DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + addAccount["Amount"].ToString() + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                                            if (dtval.Rows.Count > 0)
                                            {
                                                if (dtval.Rows[0]["msrno"].ToString() == "1")
                                                {
                                                    int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + addAccount["Amount"].ToString() + "," + PaySurcharge + "," + addAccount["BeniID"].ToString() + ",'Pending','" + addAccount["TransType"].ToString() + "','','" + agentID + "','','','',0,'" + dtBeniList.Rows[0]["BankName"].ToString() + "','" + dtBeniList.Rows[0]["accountNumber"].ToString() + "','" + dtBeniList.Rows[0]["IFSC"].ToString() + "','ALL','" + dtBeniList.Rows[0]["Name"].ToString() + "','" + hddGroupTransID + "',"+serviceData.APIID+",'" + dtCUS.Rows[0]["Name"].ToString() + "','" + dtCUS.Rows[0]["Mobile"].ToString() + "','" + dtBeniList.Rows[0]["Name"].ToString() + "','" + GSTCalculation + "','"+Source+ "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','','" + DMTService.ServiceID + "'," + item.Item2 + "");
                                                    if (Val > 0)
                                                    {
                                                        MoneyTransferRequestST moneyTransferRequestST = new MoneyTransferRequestST();
                                                        moneyTransferRequestST.Amount = Convert.ToInt32(addAccount["Amount"].ToString());
                                                        moneyTransferRequestST.ifsc = dtBeniList.Rows[0]["IFSC"].ToString();
                                                        moneyTransferRequestST.number = dtBeniList.Rows[0]["accountNumber"].ToString();
                                                        moneyTransferRequestST.PaymentType = addAccount["TransType"].ToString();
                                                        moneyTransferRequestST.OrderID = agentID;
                                                        moneyTransferRequestST.CustomerMobileNo = dtCUS.Rows[0]["mobile"].ToString();
                                                        moneyTransferRequestST.MethodName = "sendamount";

                                                        string result = await ApiPostCall.PostCallAsync(moneyTransferRequestST.GetJson(), SONITechnoCredentrial.MoneyURL);
                                                        JObject TransferData = JObject.Parse(result);
                                                        if (TransferData["code"].ToString().ToUpper() != "ERR")
                                                        {
                                                            if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS" || TransferData["data"]["Status"].ToString().ToUpper() == "PENDING")
                                                            {
                                                                cls.update_data("Update MoneyTransfer set Status='" + TransferData["data"]["Status"].ToString() + "',VendorID='" + TransferData["data"]["STID"].ToString() + "',Response='" + result + "',Message='" + TransferData["mess"].ToString() + "',RRN='" + TransferData["data"]["RRN"].ToString() + "' where OrderID='" + agentID + "'");
                                                                object returnData = new
                                                                {
                                                                    agentID = agentID,
                                                                    Message = "Transaction Process",
                                                                    status = TransferData["data"]["Status"].ToString()
                                                                };
                                                                if (TransferData["data"]["Status"].ToString().ToUpper() == "SUCCESS")
                                                                {
                                                                    Int32 UpdateCommission = await GiveCommission(agentID);
                                                                }

                                                                string Request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);
                                                                return ErrorShow.SuccessArrayAPI(Request);
                                                            }
                                                            else
                                                            {
                                                                Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Refund',Message='" + TransferData["mess"].ToString() + "',Response='" + result + "' where OrderID='" + agentID + "'");
                                                                if (ValUpdate == 1)
                                                                {
                                                                    dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + Convert.ToDecimal(addAccount["Amount"]) + ", 'CR','Refund - " + narration + "',''," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                                                                    if (dtval.Rows.Count > 0)
                                                                    {
                                                                        return ErrorShow.ErrorMessageAPI("Transaction Failed");
                                                                    }
                                                                    else
                                                                    {
                                                                        return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
                                                                }

                                                            }
                                                        }
                                                        else
                                                        {
                                                            Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Refund',Message='" + TransferData["mess"].ToString() + "',Response='" + result + "' where OrderID='" + agentID + "'");
                                                            if (ValUpdate == 1)
                                                            {
                                                                dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + Convert.ToDecimal(addAccount["Amount"]) + ", 'CR','Refund - " + narration + "',''," + ConstantsData.SMoneyTransfer + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                                                                if (dtval.Rows.Count > 0)
                                                                {
                                                                    return ErrorShow.ErrorMessageAPI(TransferData["mess"].ToString());
                                                                }
                                                                else
                                                                {
                                                                    return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                return ErrorShow.ErrorMessageAPI("Transaction Under Process Please Check and again try");
                                                            }

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
                                            return ErrorShow.ErrorMessageAPI("Beni not found");

                                    }
                                    else
                                    {
                                        return ErrorShow.ErrorMessageAPI("Beni not found");
                                    }
                                }
                                else
                                {
                                    return ErrorShow.ErrorMessageAPI("Insufficient Balance");
                                }


                            }
                            else
                            {
                                return ErrorShow.ErrorMessageAPI("You Can only send " + dtCUS.Rows[0]["SingleSlot"].ToString() + " in one time");
                            }

                        }
                        else
                        {
                            return ErrorShow.ErrorMessageAPI("Monthly Limit Completed");
                        }
                    }
                    else
                    {
                        return ErrorShow.ErrorMessageAPI("Customer Not Found");
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

    public async Task<DataTable> GetBankListAsync()
    {

        return await cls.select_data_dtAsync("select * from Bank where IsActive=1");
    }

    public async Task<DataTable> GetBeniList(string ID)
    {

        return await cls.select_data_dtAsync("ManageBeneficiaryDMT 'GetAllActive','" + ID + "'");
    }

    public async Task<string> GetCusResult(string No)
    {
        CustomerDetails customerRegistration = new CustomerDetails();
        customerRegistration.Mobile = No;
        customerRegistration.MethodName = "customerdetails";
        return await ApiPostCall.PostCallAsync(customerRegistration.GetJson(), SONITechnoCredentrial.MoneyURL);
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

    public async Task<string> Registration(CustomerRegistration _obj, string MemberID)
    {
        CustomerRegistration customerRegistration = new CustomerRegistration();
        customerRegistration.Name = _obj.Name;
        customerRegistration.Mobile = _obj.Mobile;
        customerRegistration.ADDRESS = _obj.ADDRESS;
        customerRegistration.Pan = _obj.Pan;
        customerRegistration.AADHAR = _obj.AADHAR;
        customerRegistration.IsVerify = true;
        customerRegistration.MethodName = "customerregistration";
        string Result = await ApiPostCall.PostCallAsync(customerRegistration.GetJson(), SONITechnoCredentrial.MoneyURL);
        JObject Data = JObject.Parse(Result);
        if (Data["code"].ToString().ToUpper() == "ERR")
        {
            return ErrorShow.ErrorMessageAPI(Data["mess"].ToString());
        }
        else
        {

            DataTable dt = new DataTable();
            dt = await cls.select_data_dtAsync("AddEditCustomerDMT '" + customerRegistration.Name + "','" + customerRegistration.Mobile + "','" + customerRegistration.ADDRESS + "','" + customerRegistration.Pan + "','" + customerRegistration.AADHAR + "'," + MemberID + "");
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["status"].ToString() == "1")
                {
                    return ErrorShow.SuccessMessageAPI(Data["mess"].ToString());
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI(dt.Rows[0]["MESS"].ToString());
                }
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Try After Sometime");
            }
        }

    }
}