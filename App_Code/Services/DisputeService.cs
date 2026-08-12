using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dispute;

/// <summary>
/// Summary description for DisputeService
/// </summary>
public class DisputeService : IDisputeService
{
    cls_connection cls;
    ICommonService commonService;
    public DisputeService()
    {
        cls = new cls_connection();
        commonService = new CommonService();
    }

    public async Task<ResponseBack> ApplyDispute(ApplyDispute applyDispute, string MemberID)
    {
        if (applyDispute.ServiceID == 1)
        {
            return await DisputeForRecharge(applyDispute);
        }
        else
        {
            return new ResponseBack
            {
                Status = "Error",
                Message = "Can not apply dispute on this service"
            };
        }
    }


    public async Task<ResponseBack> DisputeForRecharge(ApplyDispute applyDispute)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(applyDispute?.TransID))
        {
            return new ResponseBack
            {
                Status = "Error",
                Message = "Invalid Transaction ID"
            };
        }

        // Fetch recharge history
        DataTable rechargeHistory = await commonService.GetRechargeHistory(applyDispute.TransID);

        // Check if record exists
        if (rechargeHistory.Rows.Count == 0)
        {
            return new ResponseBack
            {
                Status = "Error",
                Message = "Record Not Found"
            };
        }

        // Retrieve transaction details
        var status = rechargeHistory.Rows[0]["Status"]?.ToString()?.ToUpper();
        var isDisputed = Convert.ToBoolean(rechargeHistory.Rows[0]["Dispute"]?.ToString());

        // Validate transaction status
        if (status != "SUCCESS")
        {
            return new ResponseBack
            {
                Status = "Error",
                Message = "Only successful transactions can be disputed"
            };
        }

        // Check if already disputed
        if (isDisputed)
        {
            return new ResponseBack
            {
                Status = "Error",
                Message = "Dispute already applied"
            };
        }

        // Apply dispute
        int updateResult =cls.update_data("UPDATE tblRecharge_History SET Dispute = 1 WHERE TransID = '"+ applyDispute.TransID + "'");
        if (updateResult > 0)
        {
            return new ResponseBack
            {
                Status = "Success",
                Message = "Dispute applied successfully"
            };
        }

        return new ResponseBack
        {
            Status = "Error",
            Message = "Failed to apply dispute"
        };
    }
}