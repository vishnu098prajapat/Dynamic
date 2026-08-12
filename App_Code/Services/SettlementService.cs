using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for SettlementService
/// </summary>
public class SettlementService : ISettlementService
{
    cls_connection cls;
    IServiceList serviceList;
    IPayoutService payoutService;
    IWallet wallet;
    private static string ServiceID = "23";
    public SettlementService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        payoutService = new PayoutService();
        serviceList = new ServiceList();
    }

    public async Task<DataTable> GetBankDetails(string MemberID)
    {
        return await cls.select_data_dtAsync("select * from VIEW_MEMBERBANKDETAILS where msrno = " + MemberID + "");
    }

    public async Task<String> Settlement(string MemberID, string BankID, string SType, string Mode, string Amount)
    {
        // Check service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(SettlementService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        // Retrieve member permissions
        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        }

        // Validate amount
        if (Amount.ToInt32() <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Enter a valid amount");
        }

        // Retrieve member balance
        decimal MyBalance = _memberAllowDetails.AEPSBalance;
        string agentID = await wallet.GetOrderID();


        //var item =await MoneyTransfer_EkoAPI.GetSurchargeCommonCommisisonSlab(
        //    _memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(Amount), serviceData.APIID.ToString(), SettlementService.ServiceID
        //);

        // Step 5: Get surcharge and commission details
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
            _memberAllowDetails.PackgeID.ToString(),
            Convert.ToDecimal(Amount),
            serviceData.APIID.ToString(),
            SettlementService.ServiceID, Mode
        );


        if (!IsValidSlab(item, Convert.ToDecimal(Amount)) && SType.Equals("BANK", StringComparison.OrdinalIgnoreCase))
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");

        if (MyBalance < (Convert.ToDecimal(Amount)+ item.surcharge + _memberAllowDetails.HoldAMT))
        {
            return ErrorShow.ErrorMessageAPI("Insufficient Balance");
        }

        // Process based on SType (BANK or Wallet)
        if (SType.Equals("BANK", StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessBankSettlement(_memberAllowDetails, BankID, Mode, Convert.ToDecimal(Amount), agentID, serviceData.APIID.ToString(), MyBalance, item);
        }
        else
        {
            if (_memberAllowDetails.AEPSBalance > (Convert.ToDecimal(Amount)+_memberAllowDetails.HoldAMT))
            {
                string guid = agentID;
                string Narration = $"AEPS/TO/Main Wallet/{guid}/Settlement";
                DataTable dtWall = new DataTable();
                dtWall = await cls.select_data_dtAsync("Exec PROC_AEPSWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + (Convert.ToDecimal(Amount)) + "','DR','" + Narration + "',''," + ConstantsData.SSettlement + ",'" + guid + "',0,0,0,0,0");
                if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                {
                    DataTable dtValue = cls.select_data_dt("EXEC PROC_AEPS_WITHDRWALREQUEST '" + _memberAllowDetails.LoginID + "','" + Amount + "','0','','" + SType + "','',0,'" + guid + "',0,0,"+ MyBalance + ",0,"+SettlementService.ServiceID+"");
                    if (Convert.ToInt32(dtValue.Rows[0]["ID"]) > 0)
                    {
                        cls.update_data("EXEC PROC_AEPS2WITHDRAW_REQUEST_ACTION 'AEPS2WITHDRAW_REQUEST_APPROVE'," + Convert.ToInt32(dtValue.Rows[0]["ID"]) + ",'','Success','Wallet','Aeps To Wallet Success','',''," + ConstantsData.SSettlement + "");
                        return ErrorShow.SuccessMessageAPI("Transaction Successfully Trasnfer Aeps to Main Wallet");
                    }
                    else
                        return ErrorShow.ErrorMessageAPI("Try After Sometime");
                }
                else
                    return ErrorShow.ErrorMessageAPI(dtWall.Rows[0]["STATUS"].ToString());
            }
            else
                return ErrorShow.ErrorMessageAPI("AMOUNT MUST BE LESS OR EQUAL TO YOUR WALLET BALANCE");
        }

    }


    private bool IsValidSlab(SlabModel slabDetails, decimal amount)
    {
        return slabDetails.startVal <= amount &&
               slabDetails.endVal >= amount &&
               slabDetails.startVal > 0 &&
               slabDetails.endVal > 0;
    }


    // Method to handle Bank Settlement
    private async Task<string> ProcessBankSettlement(MemberAllowData memberAllowDetails, string bankId, string mode, decimal amount, string guid, string apiId,decimal Balance, SlabModel item)
    {
        if (string.IsNullOrEmpty(bankId))
        {
            return ErrorShow.ErrorMessageAPI("Select one account details.");
        }

        // Get bank details securely using parameterized query
        var dtBankId = await cls.select_data_dtAsync("select * from View_MemberBankList where msrno = '" + memberAllowDetails.Msrno + "' and ID='" + bankId + "'");

        if (dtBankId.Rows.Count == 0)
        {
            return ErrorShow.ErrorMessageAPI("You must have a bank account to submit a withdraw request.");
        }


        decimal surcharge = item.surcharge;
        decimal gst = item.IsGst ? 0 : (surcharge * item.Gst) / 100;// surcharge * item.Gst / 100;
        decimal totalAmount = surcharge + amount + gst;


        // Proceed with the withdrawal request
        var bankRow = dtBankId.Rows[0];
        string narration = $"{bankRow["AccountNumber"]}/{bankRow["IFSCCode"]}/{guid}/{mode}/Settlement";

        // Wallet transaction
        var dtWallet = await cls.select_data_dtAsync("Exec PROC_AEPSWALLETTRANSACTION '" + memberAllowDetails.LoginID + "','" + (amount) + "','DR','" + narration + "','','" + ConstantsData.SSettlement + "','" + guid + "',0,0,0," + surcharge + "," + gst + "");
        if (dtWallet.Rows.Count > 0)
        {
            if (dtWallet.Rows[0]["Msrno"].ToString() == "1")
            {
                return await HandleBankTransfer(memberAllowDetails, bankRow, amount, mode, guid, apiId, surcharge, gst, Balance, item);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Amount must be less or equal to your wallet balance.");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI("Insufficient Balance Due to commission and Minimum wallet Balance is "+ memberAllowDetails.HoldAMT+ "");
        }
    }


    // Helper Method to Handle Bank Transfer
    private async Task<string> HandleBankTransfer(MemberAllowData memberAllowDetails, DataRow bankRow, decimal amount, string mode, string guid, string apiId, decimal surcharge, decimal GST, decimal Balance, SlabModel item)
    {
        DataTable dtValue = await cls.select_data_dtAsync("EXEC PROC_AEPS_WITHDRWALREQUEST '" + memberAllowDetails.LoginID + "','" + amount + "','" + bankRow["id"].ToString() + "','','Bank','" + mode + "','" + surcharge + "','" + guid + "'," + apiId + ","+ GST + ","+ Balance + ","+ item.OpID+ ","+ SettlementService.ServiceID + "");
        if (Convert.ToInt32(dtValue.Rows[0]["ID"]) > 0)
        {
            // Prepare transfer request
            var moneyTransferRequest = new PayoutCommon
            {
                Amount = amount,
                IFSC = bankRow["IFSCCode"].ToString(),
                AccountNO = bankRow["AccountNumber"].ToString(),
                Mode = mode,
                OrderID = guid,
                BankName = bankRow["Name"].ToString(),
                BeneID = bankRow["Bene_ID"].ToString(),
                Mobile = memberAllowDetails.Mobile,
                MailID = memberAllowDetails.Email,
                Name = bankRow["AccountHolderName"].ToString(),
                identifier=(cls.select_data_scalar_string("select top 1 aepsid from AEPSOnBoardStatus where name='bank6' and msrno='"+ memberAllowDetails.Msrno+ "'"))
            };

            var result = await payoutService.FundTransfer(moneyTransferRequest, memberAllowDetails.Msrno.ToString());
            var transferData = JObject.Parse(result);

            string vendorId = transferData["data"]["VendorID"].ToString();
            string txnCode = transferData["data"]["TXNCode"].ToString();
            string rrn = transferData["data"]["RRN"].ToString();
            string message = transferData["data"]["Message"].ToString();
            string status = transferData["data"]["Status"].ToString().ToUpper();

            if (status == "SUCCESS" || status == "PENDING" || status == "INPROGRESS")
            {
                await cls.update_dataAsync("EXEC PROC_AEPS2WITHDRAW_REQUEST_ACTION 'AEPS2WITHDRAW_REQUEST_APPROVE'," + Convert.ToInt32(dtValue.Rows[0]["ID"]) + ",'" + vendorId + "','"+ status + "','" + txnCode + "','" + message + "','" + rrn + "','" + result + "'," + ConstantsData.SSettlement + "");
                return ErrorShow.SuccessMessageAPI(moneyTransferRequest.OrderID);
            }
            else if (status == "FAILED")
            {
                await cls.update_dataAsync("EXEC PROC_AEPS2WITHDRAW_REQUEST_ACTION 'AEPS2WITHDRAW_REQUEST_DELETE'," + Convert.ToInt32(dtValue.Rows[0]["ID"]) + ",'" + vendorId + "','Pending','" + txnCode + "','" + message + "','" + rrn + "','" + result + "'," + ConstantsData.SSettlement + "");
                return ErrorShow.ErrorMessageAPI(message);
            }
            else
            {
                await cls.update_dataAsync("EXEC PROC_AEPS2WITHDRAW_REQUEST_ACTION 'AEPS2WITHDRAW_REQUEST_DELETE'," + Convert.ToInt32(dtValue.Rows[0]["ID"]) + ",'" + vendorId + "','Pending','" + txnCode + "','" + message + "','" + rrn + "','" + result + "'," + ConstantsData.SSettlement + "");
                return ErrorShow.ErrorMessageAPI(message);
            }
        }
        else
            return ErrorShow.ErrorMessageAPI("Try After Sometime");
    }
}