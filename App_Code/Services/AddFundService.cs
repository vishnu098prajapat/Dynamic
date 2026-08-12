using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for AddFundService
/// </summary>
public class AddFundService : IAddFundService
{
    cls_connection cls;
    IWallet wallet;
    public AddFundService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
    }

    public async Task<string> AddFund(string FromMeberID, string ToLoginID, decimal Amount, string Remark)
    {
        decimal MyBalance = 0;
        var _pmemberAllowDetails = await wallet.GetMemberAllowPermissionDetails(FromMeberID);
        if (_pmemberAllowDetails.IsActive)
        {
            if (!_pmemberAllowDetails.HoldUnHold)
            {
                var _cmemberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(ToLoginID);
                if (_cmemberAllowDetails.IsActive)
                {
                    if (!_cmemberAllowDetails.HoldUnHold)
                    {
                        MyBalance = _pmemberAllowDetails.MainBalance;
                        string OrderID = await wallet.GetOrderID();
                        if (MyBalance >= Amount)
                        {
                            Int32 Val = cls.update_data("AddEditFundTransfer " + _pmemberAllowDetails.Msrno + "," + _cmemberAllowDetails.Msrno + ",'" + OrderID + "','" + Amount + "','0','Cr','Main','Success','" + Remark + "'");
                            if (Val > 0)
                            {
                                string Narration = $"Send/{OrderID}/{_cmemberAllowDetails.LoginID}({_cmemberAllowDetails.ShopName})";
                                string NarrationRecevier = $"Recevied/{OrderID}/{_pmemberAllowDetails.LoginID}";
                                DataTable dtWall = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _pmemberAllowDetails.LoginID + "','" + Amount + "','Dr','" + Narration + "','" + Remark + "','" + ConstantsData.SFundTransfer + "','" + OrderID + "'");
                                if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                {
                                    dtWall = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _cmemberAllowDetails.LoginID + "','" + Amount + "','Cr','" + NarrationRecevier + "','" + Remark + "','" + ConstantsData.SFundRecevied + "','" + OrderID + "'," + _pmemberAllowDetails.Msrno + ",0");
                                    if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                    {
                                        try
                                        {
                                            FirebaseSender firebaseSender = new FirebaseSender();
                                            string message = $"₹{Amount} has been successfully Recevied.";
                                            await firebaseSender.SendNotificationAsync(_cmemberAllowDetails.Token, "Fund Recevied", message, "Fund Recevied", "");


                                            string[] ValueArray = new string[4];
                                            ValueArray[0] = _pmemberAllowDetails.ShopName;
                                            ValueArray[1] = _cmemberAllowDetails.Mobile;
                                            ValueArray[2] = Amount.ToString();
                                            ValueArray[3] = _cmemberAllowDetails.ShopName;
                                            string str = SMS.SendWithV(_pmemberAllowDetails.Mobile, 24, ValueArray, "1");

                                            string[] ValueArrays = new string[5];
                                            ValueArrays[0] = Amount.ToString();
                                            ValueArrays[1] = _pmemberAllowDetails.ShopName;
                                            ValueArrays[2] = _pmemberAllowDetails.Mobile;
                                            ValueArrays[3] = _cmemberAllowDetails.MainBalance.ToString();
                                            ValueArrays[4] = (_cmemberAllowDetails.MainBalance + Amount).ToString();
                                            string strs = SMS.SendWithV(_cmemberAllowDetails.Mobile, 23, ValueArrays, "1");

                                        }
                                        catch (Exception ex)
                                        {
                                        }

                                        return ErrorShow.SuccessMessageAPI("Fund Transfer Successfully");
                                    }
                                    else
                                        return ErrorShow.ErrorMessageAPI("Try After Sometime");
                                }
                                else
                                    return ErrorShow.ErrorMessageAPI("Try After Sometime");
                            }
                            else
                                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
                        }
                        else
                        {
                            return ErrorShow.ErrorMessageAPI("You have Insufficient Balance.");
                        }
                    }
                    else
                        return ErrorShow.ErrorMessageAPI("Recevier Member Account On Hold");
                }
                else
                    return ErrorShow.ErrorMessageAPI("Recevier Member In Deactive Mode");
            }
            else
                return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");
        }
        else
            return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");
    }


    public async Task<string> AddFundAEPSToMain(string FromMeberID, string ToLoginID, decimal Amount, string Remark)
    {
        decimal MyBalance = 0;
        var _pmemberAllowDetails = await wallet.GetMemberAllowPermissionDetails(FromMeberID);
        if (_pmemberAllowDetails.IsActive)
        {
            if (!_pmemberAllowDetails.HoldUnHold)
            {
                var _cmemberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(ToLoginID);
                if (_cmemberAllowDetails.IsActive)
                {
                    if (!_cmemberAllowDetails.HoldUnHold)
                    {
                        MyBalance = _pmemberAllowDetails.AEPSBalance;
                        string OrderID = await wallet.GetOrderID();
                        if (MyBalance >= Amount)
                        {
                            Int32 Val = cls.update_data("AddEditFundTransfer " + _pmemberAllowDetails.Msrno + "," + _cmemberAllowDetails.Msrno + ",'" + OrderID + "','" + Amount + "','0','Cr','AEPS','Success','" + Remark + "'");
                            if (Val > 0)
                            {
                                string Narration = $"Send/{OrderID}/{_cmemberAllowDetails.LoginID}({_cmemberAllowDetails.ShopName})";
                                string NarrationRecevier = $"Recevied/{OrderID}/{_pmemberAllowDetails.LoginID}";
                                DataTable dtWall = await cls.select_data_dtAsync("exec PROC_AEPSWALLETTRANSACTION '" + _pmemberAllowDetails.LoginID + "','" + Amount + "','Dr','" + Narration + "','" + Remark + "','" + ConstantsData.SFundTransfer + "','" + OrderID + "'");
                                if (dtWall.Rows.Count > 0)
                                {
                                    if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                    {
                                        dtWall = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _cmemberAllowDetails.LoginID + "','" + Amount + "','Cr','" + NarrationRecevier + "','" + Remark + "','" + ConstantsData.SFundRecevied + "','" + OrderID + "'," + _pmemberAllowDetails.Msrno + ",0");
                                        if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                        {
                                            try
                                            {
                                                FirebaseSender firebaseSender = new FirebaseSender();
                                                string message = $"₹{Amount} has been successfully Recevied.";
                                                await firebaseSender.SendNotificationAsync(_cmemberAllowDetails.Token, "Fund Recevied", message, "Fund Recevied", "");

                                                string[] ValueArray = new string[4];
                                                ValueArray[0] = _pmemberAllowDetails.ShopName;
                                                ValueArray[1] = _cmemberAllowDetails.Mobile;
                                                ValueArray[2] = Amount.ToString();
                                                ValueArray[3] = _cmemberAllowDetails.ShopName;
                                                string str = SMS.SendWithV(_pmemberAllowDetails.Mobile, 24, ValueArray, "1");

                                                string[] ValueArrays = new string[5];
                                                ValueArrays[0] = Amount.ToString();
                                                ValueArrays[1] = _pmemberAllowDetails.ShopName;
                                                ValueArrays[2] = _pmemberAllowDetails.Mobile;
                                                ValueArrays[3] = _cmemberAllowDetails.MainBalance.ToString();
                                                ValueArrays[4] = (_cmemberAllowDetails.MainBalance + Amount).ToString();
                                                string strs = SMS.SendWithV(_cmemberAllowDetails.Mobile, 23, ValueArrays, "1");

                                            }
                                            catch (Exception ex)
                                            {
                                            }


                                            return ErrorShow.SuccessMessageAPI("Fund Transfer Successfully");
                                        }
                                        else
                                            return ErrorShow.ErrorMessageAPI("Low Balance");
                                    }
                                    else
                                        return ErrorShow.ErrorMessageAPI("Low Balance");
                                }
                                else
                                {
                                    return ErrorShow.ErrorMessageAPI("Low Balance");
                                }
                            }
                            else
                                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
                        }
                        else
                        {
                            return ErrorShow.ErrorMessageAPI("You have Insufficient Balance.");
                        }
                    }
                    else
                        return ErrorShow.ErrorMessageAPI("Recevier Member Account On Hold");
                }
                else
                    return ErrorShow.ErrorMessageAPI("Recevier Member In Deactive Mode");
            }
            else
                return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");
        }
        else
            return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");
    }

    public async Task<string> DeductFund(string FromMeberID, string ToLoginID, decimal Amount, string Remark)
    {
        decimal chkBalance = 0;
        var _pmemberAllowDetails = await wallet.GetMemberAllowPermissionDetails(FromMeberID);
        if (_pmemberAllowDetails.IsActive)
        {
            if (!_pmemberAllowDetails.HoldUnHold)
            {
                var _cmemberAllowDetails = await wallet.GetMemberAllowPermissionDetailsByLoginID(ToLoginID);
                if (_cmemberAllowDetails.IsActive)
                {
                    if (!_cmemberAllowDetails.HoldUnHold)
                    {
                        chkBalance = _cmemberAllowDetails.MainBalance;
                        string OrderID = await wallet.GetOrderID();

                        if (chkBalance >= Amount)
                        {
                            Int32 Val = cls.update_data("AddEditFundTransfer " + _pmemberAllowDetails.Msrno + "," + _cmemberAllowDetails.Msrno + ",'" + OrderID + "','" + Amount + "','0','Cr','Main','Success','" + Remark + "'");
                            if (Val > 0)
                            {
                                string Narration = $"Deduct/{OrderID}/{_cmemberAllowDetails.LoginID}({_cmemberAllowDetails.ShopName})";
                                string NarrationRecevier = $"Recevied/{OrderID}/{_pmemberAllowDetails.LoginID}/{_cmemberAllowDetails.LoginID}";
                                DataTable dtWall = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _cmemberAllowDetails.LoginID + "','" + Amount + "','Dr','" + Narration + "','" + Remark + "','" + ConstantsData.SDeductFund + "','" + OrderID + "'");
                                if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                {
                                    dtWall = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _pmemberAllowDetails.LoginID + "','" + Amount + "','Cr','" + NarrationRecevier + "','" + Remark + "','" + ConstantsData.SFundRecevied + "','" + OrderID + "'," + _pmemberAllowDetails.Msrno + ",0");
                                    if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                    {
                                        try
                                        {
                                            FirebaseSender firebaseSender = new FirebaseSender();
                                            string message = $"₹{Amount} has been successfully Recevied.";
                                            await firebaseSender.SendNotificationAsync(_cmemberAllowDetails.Token, "Fund Recevied", message, "Fund Recevied", "");


                                            string[] ValueArray = new string[4];
                                            ValueArray[0] = _pmemberAllowDetails.ShopName;
                                            ValueArray[1] = _cmemberAllowDetails.Mobile;
                                            ValueArray[2] = Amount.ToString();
                                            ValueArray[3] = _cmemberAllowDetails.ShopName;
                                            string str = SMS.SendWithV(_pmemberAllowDetails.Mobile, 24, ValueArray, "1");

                                            string[] ValueArrays = new string[5];
                                            ValueArrays[0] = Amount.ToString();
                                            ValueArrays[1] = _pmemberAllowDetails.ShopName;
                                            ValueArrays[2] = _pmemberAllowDetails.Mobile;
                                            ValueArrays[3] = _cmemberAllowDetails.MainBalance.ToString();
                                            ValueArrays[4] = (_cmemberAllowDetails.MainBalance + Amount).ToString();
                                            string strs = SMS.SendWithV(_cmemberAllowDetails.Mobile, 23, ValueArrays, "1");

                                        }
                                        catch (Exception ex)
                                        {
                                        }

                                        return ErrorShow.SuccessMessageAPI("Fund Deduct Successfully");
                                    }
                                    else
                                        return ErrorShow.ErrorMessageAPI("Try After Sometime");
                                }
                                else
                                    return ErrorShow.ErrorMessageAPI("Try After Sometime");
                            }
                            else
                                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");

                            //Int32 Val = cls.update_data("AddEditFundTransfer " + _pmemberAllowDetails.Msrno + "," + _cmemberAllowDetails.Msrno + ",'" + OrderID + "','" + Amount + "','0','DR','Main','Pending','" + Remark + "'");
                            //if (Val > 0)
                            //{
                            //    Val = await cls.update_dataAsync("exec AddEditMemberHoldAmount 0,'" + _cmemberAllowDetails.Msrno + "','" + Amount + "','" + Remark + "','" + OrderID + "'");
                            //    if (Val > 0)
                            //    {
                            //        try
                            //        {
                            //            FirebaseSender firebaseSender = new FirebaseSender();
                            //            string message = $"We've placed a ₹{Amount} hold on your account for verification. This is a temporary hold and will be released automatically once the process is complete.Hold by {_pmemberAllowDetails.ShopName}({_pmemberAllowDetails.Mobile})";
                            //            await firebaseSender.SendNotificationAsync(_cmemberAllowDetails.Token, "Hold Amount", message, "Hold Amount", "");
                            //        }
                            //        catch (Exception ex)
                            //        {
                            //        }

                            //        return ErrorShow.SuccessMessageAPI("Request Submitted Successfully!");
                            //    }
                            //    else
                            //    {
                            //        return ErrorShow.ErrorMessageAPI("Try After Sometime");
                            //    }
                            //}
                            //else
                            //    return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");

                        }
                        else
                        {
                            return ErrorShow.ErrorMessageAPI("You have Insufficient Balance.");
                        }
                    }
                    else
                        return ErrorShow.ErrorMessageAPI("Recevier Member Account On Hold");
                }
                else
                    return ErrorShow.ErrorMessageAPI("Recevier Member In Deactive Mode");
            }
            else
                return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");
        }
        else
            return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");
    }


    public async Task<string> DeductSettlement(string OrderID)
    {
        DataTable dtsett = await cls.select_data_dtAsync("select * from FundTransfer where OrderID='" + OrderID + "' and Factor='DR'");
        if (dtsett.Rows.Count > 0)
        {
            decimal chkBalance = 0;
            var _pmemberAllowDetails = await wallet.GetMemberAllowPermissionDetails(dtsett.Rows[0]["FMsrno"].ToString());
            if (_pmemberAllowDetails.IsActive)
            {
                if (!_pmemberAllowDetails.HoldUnHold)
                {
                    var _cmemberAllowDetails = await wallet.GetMemberAllowPermissionDetails(dtsett.Rows[0]["TMsrno"].ToString());
                    if (_cmemberAllowDetails.IsActive)
                    {
                        if (!_cmemberAllowDetails.HoldUnHold)
                        {
                            decimal Amount = Convert.ToDecimal(dtsett.Rows[0]["Amount"].ToString());
                            chkBalance = _cmemberAllowDetails.MainBalance;
                            if (chkBalance >= Amount)
                            {
                                string Narration = $"Deduct/{OrderID}/{_pmemberAllowDetails.LoginID}({_pmemberAllowDetails.ShopName})";
                                string NarrationRecevier = $"Recevied/{OrderID}/{_cmemberAllowDetails.LoginID}";

                                DataTable dtmh = await cls.select_data_dtAsync("SELECT * FROM MemberHoldAmount WHERE OrderID='" + dtsett.Rows[0]["OrderID"].ToString() + "'");
                                string Status = dtmh.Rows.Count > 0 ? dtmh.Rows[0]["Status"].ToString() : "";

                                if (Status == "Success")
                                {
                                    await cls.select_data_dtAsync("UPDATE member SET holdamt=(holdamt-" + Amount + ") WHERE msrno='" + _cmemberAllowDetails.Msrno + "'");

                                    DataTable dtWall = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _cmemberAllowDetails.LoginID + "','" + Amount + "','Dr','" + Narration + "','" + dtsett.Rows[0]["Reason"].ToString() + "','" + ConstantsData.SDeductFund + "','" + OrderID + "'");
                                    if (dtWall.Rows.Count > 0)
                                    {
                                        if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                        {
                                            dtWall = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _pmemberAllowDetails.LoginID + "','" + Amount + "','Cr','" + NarrationRecevier + "','" + dtsett.Rows[0]["Reason"].ToString() + "','" + ConstantsData.SFundRecevied + "','" + OrderID + "'," + _pmemberAllowDetails.Msrno + ",0");
                                            if (dtWall.Rows[0]["MSRNO"].ToString() == "1")
                                            {
                                                await cls.select_data_dtAsync("UPDATE FundTransfer SET Status='Success' WHERE OrderID='" + OrderID + "'");
                                                await cls.select_data_dtAsync("UPDATE MemberHoldAmount SET Status='Resolved' WHERE OrderID='" + OrderID + "'");

                                                try
                                                {
                                                    FirebaseSender firebaseSender = new FirebaseSender();
                                                    string message = $"Your temporary ₹{Amount} hold has been successfully reversed. You can now use the full amount.";
                                                    await firebaseSender.SendNotificationAsync(_cmemberAllowDetails.Token, "Hold Reversed", message, "Fund Request", "");
                                                }
                                                catch (Exception ex)
                                                {
                                                }


                                                try
                                                {
                                                    FirebaseSender firebaseSender = new FirebaseSender();
                                                    string message = $"temporary ₹{Amount} hold has been successfully reversed.";
                                                    await firebaseSender.SendNotificationAsync(_pmemberAllowDetails.Token, "Hold Reversed", message, "Fund Request", "");
                                                }
                                                catch (Exception ex)
                                                {
                                                }

                                                return ErrorShow.SuccessMessageAPI("Fund Reversal Successfully");
                                            }
                                            else
                                                return ErrorShow.ErrorMessageAPI("Try After Sometime");
                                        }
                                        else
                                        {
                                            return ErrorShow.ErrorMessageAPI("Low Balance");
                                        }
                                    }
                                    else
                                    {
                                        await cls.select_data_dtAsync("UPDATE member SET holdamt=(holdamt+" + Amount + ") WHERE msrno='" + _cmemberAllowDetails.Msrno + "'");

                                        return ErrorShow.ErrorMessageAPI("Please Remove Hold Amount Condition");
                                    }
                                }
                                else
                                    return ErrorShow.ErrorMessageAPI("Something went wrong");
                            }
                            else
                            {
                                return ErrorShow.ErrorMessageAPI("You have Insufficient Balance.");
                            }
                        }
                        else
                            return ErrorShow.ErrorMessageAPI("Recevier Member Account On Hold");
                    }
                    else
                        return ErrorShow.ErrorMessageAPI("Recevier Member In Deactive Mode");
                }
                else
                    return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");
            }
            else
                return ErrorShow.ErrorMessageAPI("Member In Deactive Mode");

        }
        else
            return ErrorShow.ErrorMessageAPI("Record Not Found");
    }

}