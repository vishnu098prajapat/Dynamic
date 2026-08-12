using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for CommissionService
/// </summary>
public class CommissionService : ICommissionService
{
    cls_connection cls;
    public CommissionService()
    {
        cls = new cls_connection();
    }

    public async Task<int> GiveCCPayCommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from CreditCardBillPay where orderid='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("CCPayCommission " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["OrderID"].ToString() + "'");
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

    public async Task<int> GiveBBPSCommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from tblRecharge_History where TransID='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("Recharge_DistributeCommissionUpline " + dthis.Rows[0]["Memberid"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["TransID"].ToString() + "'");
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

    public async Task<int> GiveDMTCommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from moneytransfer where orderid='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("DMTCommission " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["OrderID"].ToString() + "'");
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


    public async Task<int> GiveDMTPPICommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from DMTPPIHistory where orderid='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("DMTPPI_DistributeCommissionUpline " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["OrderID"].ToString() + "'");
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

    public async Task<int> GiveUPITransferCommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from UPITransfer where orderid='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("UPICommission " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["OrderID"].ToString() + "'");
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

    public async Task<int> GivePayCommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from PaySettlement where orderid='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("Pay_DistributeCommissionUpline " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["OrderID"].ToString() + "'");
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

    public async Task<int> GiveAEPSCommission(string TransID)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from Aeps_history where status='Success' and transcationtype in ('CW','MS') and  referenceno='" + TransID + "'");
        if (dthis.Rows.Count > 0)
        {
            Int32 Val = await cls.update_dataAsync("Proc_AEPS_DistributeCommission " + dthis.Rows[0]["Msrno"].ToString() + "," + Convert.ToDecimal(dthis.Rows[0]["Amount"].ToString()) + ",'" + dthis.Rows[0]["referenceno"].ToString() + "'");
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

    public async Task<int> GiveAEPSAmount(string TransID, string LoginID, string Msrno)
    {
        DataTable dthis = await cls.select_data_dtAsync("select * from aeps_history where transcationtype in ('CW') and Msrno=" + Msrno + " and referenceno='" + TransID + "' and Status='Success'");
        if (dthis.Rows.Count > 0)
        {
            string narration = "", Decs = "";
            narration = "Aeps TransID-" + TransID;
            Decs = $"AEPS Amount - {dthis.Rows[0]["Amount"].ToString()}. Done With Aadhar No {dthis.Rows[0]["adhaarnumber"].ToString()}. TransID - {dthis.Rows[0]["referenceno"].ToString()}";
            Int32 Val = await cls.update_dataAsync("exec PROC_AEPSWALLETTRANSACTION '" + LoginID + "','" + dthis.Rows[0]["Amount"].ToString() + "','CR','" + narration + "','" + Decs + "','" + dthis.Rows[0]["ServiceID"].ToString() + "','" + dthis.Rows[0]["referenceno"].ToString() + "'");
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
}