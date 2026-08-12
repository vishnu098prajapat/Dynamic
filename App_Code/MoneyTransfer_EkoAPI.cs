using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Developed By: Harish Kr Patidar
/// Date: 02 Mar 2019
/// Description: Used to access money transfer api.
/// </summary>

public class MoneyTransfer_EkoAPI
{
    public static decimal Beneficiary_VerificationFee = Convert.ToDecimal(3.95);
    public static decimal UPI_VerificationFee = Convert.ToDecimal(2.95);
    public static string DMR_Limit = "50,000";

    public MoneyTransfer_EkoAPI()
    {
        DataTable dtAPI = new DataTable();
        cls_connection objConnection = new cls_connection();
        dtAPI = objConnection.select_data_dt("select * from MM_DMRCred");
        if (dtAPI.Rows.Count == 1)
        {
            //Beneficiary_VerificationFee = Convert.ToDecimal(dtAPI.Rows[0]["Beneficiary_VerificationFee"]);
            //  DMR_Limit = Convert.ToString(dtAPI.Rows[0]["DMR_Limit"]);
        }
    }

    public string RemitterDetails(string mobile)
    {
        return "";
    }

    public string RemitterRegistration(string mobile, string name, string pincode)
    {
        return "";
    }

    public static Tuple<decimal, Int32, decimal> GetSurchargeCommonCommisisonWithGST(string PackageID, decimal netAmount, string APIID, string ServiceID)
    {
        decimal surcharge_amt = 5; decimal surcharge_rate = 0, gst = 0; Boolean isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from CommonCommissionSetup where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' and Serviceid=" + ServiceID + " order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["Commission"].ToString());
            gst = Convert.ToDecimal(dtsr.Rows[0]["GstVal"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == false)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }
            return new Tuple<decimal, Int32, decimal>(surcharge_amt, dtsr.Rows[0]["OpId"].ToString().ToInt32(), gst);
        }
        else
        {
            return new Tuple<decimal, Int32, decimal>(surcharge_amt, 0, 0);
        }
    }

    public static Tuple<decimal, Int32, decimal, decimal> GetSurchargeCommonCommisison(string PackageID, decimal netAmount, string APIID, string ServiceID)
    {
        decimal surcharge_amt = 5; decimal surcharge_rate = 0; Boolean isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from CommonCommissionSetup where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' and Serviceid=" + ServiceID + " order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["Commission"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == false)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }
            return new Tuple<decimal, Int32, decimal, decimal>(surcharge_amt, dtsr.Rows[0]["OpId"].ToString().ToInt32(), Convert.ToDecimal(dtsr.Rows[0]["startval"].ToString()), Convert.ToDecimal(dtsr.Rows[0]["endval"].ToString()));
        }
        else
            return new Tuple<decimal, Int32, decimal, decimal>(surcharge_amt, 0, 0, 0);
    }


    public async static Task<SlabModel> GetSurchargeCommonCommisisonSlab(string PackageID, decimal netAmount, string APIID, string ServiceID)
    {
        // Default surcharge amount
        decimal surcharge_amt = 5;
        decimal surcharge_rate = 0;
        bool isFlat = false;

        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = await cls.select_data_dtAsync("Select top 1 * from CommonCommissionSetup where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' and Serviceid=" + ServiceID + " order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["Commission"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());

            // Calculate the surcharge amount
            surcharge_amt = isFlat ? surcharge_rate : (netAmount * surcharge_rate) / 100;

            return new SlabModel()
            {
                surcharge = surcharge_amt,
                startVal = Convert.ToDecimal(dtsr.Rows[0]["startval"].ToString()),
                endVal = Convert.ToDecimal(dtsr.Rows[0]["endVal"].ToString()),
                OpID = dtsr.Rows[0]["OpId"].ToString().ToInt32(),
                IsGst = dtsr.Rows[0]["IsGst"].ToString().ToBoolen(),
                Gst = Convert.ToDecimal(dtsr.Rows[0]["GstVal"].ToString()),
                IsTds = dtsr.Rows[0]["IsTds"].ToString().ToBoolen(),
                Tds = Convert.ToDecimal(dtsr.Rows[0]["Tds"].ToString())
            };
        }

        return new SlabModel()
        {
            surcharge = surcharge_amt,
            startVal = 0,
            endVal = 0,
            OpID = 0,
            IsGst = false,
            Gst = 0,
            IsTds = false,
            Tds = 0
        };
    }


    public async static Task<SlabModel> GetCSSlab(string PackageID, decimal netAmount, string APIID, string ServiceID)
    {
        // Default surcharge amount
        decimal surcharge_amt = 5;
        decimal surcharge_rate = 0;
        bool isFlat = false;

        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = await cls.select_data_dtAsync("Select top 1 * from CommonCommissionSetup where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' and Serviceid=" + ServiceID + " order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["Commission"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());

            // Calculate the surcharge amount
            surcharge_amt = isFlat ? surcharge_rate : (netAmount * surcharge_rate) / 100;

            return new SlabModel()
            {
                surcharge = surcharge_amt,
                startVal = Convert.ToDecimal(dtsr.Rows[0]["startval"].ToString()),
                endVal = Convert.ToDecimal(dtsr.Rows[0]["endVal"].ToString()),
                OpID = dtsr.Rows[0]["OpId"].ToString().ToInt32(),
                IsGst = dtsr.Rows[0]["IsGst"].ToString().ToBoolen(),
                Gst = Convert.ToDecimal(dtsr.Rows[0]["GstVal"].ToString()),
                IsTds = dtsr.Rows[0]["IsTds"].ToString().ToBoolen(),
                Tds = Convert.ToDecimal(dtsr.Rows[0]["Tds"].ToString())
            };
        }

        return new SlabModel()
        {
            surcharge = surcharge_amt,
            startVal = 0,
            endVal = 0,
            OpID = 0,
            IsGst = false,
            Gst = 0,
            IsTds = false,
            Tds = 0
        };
    }

    public async static Task<SlabModel> GetCommisisonSlab(string PackageID, decimal netAmount, string APIID, string ServiceID, string OPID)
    {
        // Default surcharge amount
        decimal surcharge_amt = 0;
        decimal surcharge_rate = 0;
        bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = await cls.select_data_dtAsync("GETSLAB " + PackageID + "," + netAmount + "," + APIID + ",'" + ServiceID + "','" + OPID + "'");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["SlabCharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["SlabIsPF"].ToString());

            // Calculate the surcharge amount
            surcharge_amt = isFlat ? surcharge_rate : (netAmount * surcharge_rate) / 100;

            return new SlabModel()
            {
                surcharge = surcharge_amt,
                startVal = Convert.ToDecimal(dtsr.Rows[0]["startval"].ToString()),
                endVal = Convert.ToDecimal(dtsr.Rows[0]["endVal"].ToString()),
                OpID = dtsr.Rows[0]["OpId"].ToString().ToInt32(),
                IsGst = Convert.ToBoolean(dtsr.Rows[0]["IsGst"].ToString()),
                IsPlusMinGst = Convert.ToBoolean(dtsr.Rows[0]["IsPlusMinGst"].ToString()),
                Gst = 18,
                IsTds = true,
                Tds = 2
            };
        }

        return new SlabModel()
        {
            surcharge = surcharge_amt,
            startVal = 0,
            endVal = 0,
            OpID = 0,
            IsGst = false,
            Gst = 0,
            IsPlusMinGst = false,
            IsTds = false,
            Tds = 0
        };
    }

    public async static Task<SlabModel> GetCommisisonSlabC(string PackageID, decimal netAmount, string APIID, string ServiceID, string OPID)
    {
        // Default surcharge amount
        decimal surcharge_amt = 0;
        decimal surcharge_rate = 0;
        bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = await cls.select_data_dtAsync("GETSLABComm " + PackageID + "," + netAmount + "," + APIID + ",'" + ServiceID + "','" + OPID + "'");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["SlabCharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["SlabIsPF"].ToString());

            // Calculate the surcharge amount
            surcharge_amt = isFlat ? surcharge_rate : (netAmount * surcharge_rate) / 100;

            return new SlabModel()
            {
                surcharge = surcharge_amt,
                startVal = Convert.ToDecimal(dtsr.Rows[0]["startval"].ToString()),
                endVal = Convert.ToDecimal(dtsr.Rows[0]["endVal"].ToString()),
                OpID = dtsr.Rows[0]["OpId"].ToString().ToInt32(),
                IsGst = Convert.ToBoolean(dtsr.Rows[0]["IsGst"].ToString()),
                IsPlusMinGst = Convert.ToBoolean(dtsr.Rows[0]["IsPlusMinGst"].ToString()),
                Gst = 18,
                IsTds = true,
                Tds = 2
            };
        }

        return new SlabModel()
        {
            surcharge = surcharge_amt,
            startVal = 0,
            endVal = 0,
            OpID = 0,
            IsGst = false,
            Gst = 0,
            IsPlusMinGst = false,
            IsTds = false,
            Tds = 0
        };
    }


    public static Tuple<decimal, Int32, decimal> GetSurchargeCommonCommisisonWithOp(string PackageID, decimal netAmount, string APIID, string ServiceID, string OPCode)
    {
        decimal surcharge_amt = 5, Cash_amt = 0; decimal surcharge_rate = 0; Boolean isFlat = false;
        decimal cashback = 0; Boolean iscashFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from CommonCommissionSetup where opid=(select id from Operator where OperatorCode='" + OPCode + "') and startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' and Serviceid=" + ServiceID + " order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["Commission"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());

            cashback = Convert.ToDecimal(dtsr.Rows[0]["CashBack"].ToString());
            iscashFlat = Convert.ToBoolean(dtsr.Rows[0]["IsCashBack"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == false)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }


            if (cashback > 0)
            {
                if (iscashFlat == false)
                    Cash_amt = ((netAmount) * cashback) / 100;
                else
                    Cash_amt = cashback;
            }

            return new Tuple<decimal, Int32, decimal>(surcharge_amt, dtsr.Rows[0]["OpId"].ToString().ToInt32(), Cash_amt);
        }
        else
        {
            return new Tuple<decimal, Int32, decimal>(surcharge_amt, 0, Cash_amt);
        }
    }

    public static decimal GetSurchargeDMT(string PackageID, decimal netAmount)
    {
        decimal surcharge_amt = 5; decimal surcharge_rate = 0; Boolean isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from DMTSurcharge where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == false)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }
            return surcharge_amt;
        }
        else
        {
            return surcharge_amt;
        }
    }
    public static decimal GetSurchargeDMTWithAPI(string PackageID, decimal netAmount, string APIID)
    {
        decimal surcharge_amt = 5; decimal surcharge_rate = 0; Boolean isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from DMTSurcharge where APIID=" + APIID + " and startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == false)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }
            return surcharge_amt;
        }
        else
        {
            return surcharge_amt;
        }
    }

    public static decimal GetSurchargeDMT2Special(string PackageID, decimal netAmount)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; int isFlat = 0;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        //string PackageID = cls.select_data_scalar_string("Select packageid from tblmlm_membermaster where PackageID='" + PackageID + "'");
        dtsr = cls.select_data_dt("Select top 1 * from tblSpecialDMTSurcharge where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToInt32(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == 0)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }
            return surcharge_amt;
        }
        else
        {
            return surcharge_amt;
        }
    }

    public string RemitterRegistration_Validate(string remitterid, string mobile, string otp)
    {
        return "";
    }

    public string RemitterRegistration_ResentOTP(string remittermobile)
    {
        return "";
    }

    public string BeneficiaryList(string remittermobile)
    {
        return "";
    }

    public string BeneficiaryRegistration_acc_ifsc(string remitterMobile, string name, string mobile, string ifsc, string account, string bank_id)
    {
        return "";
    }

    public string BeneficiaryRegistration_acc_bankcode(string remitterMobile, string name, string mobile, string bankcode, string account, string bank_id)
    {
        return "";
    }

    public string BeneficiaryDelete(string beneficiaryid, string remitterMobile)
    {
        return "";
    }

    public string Beneficiary_Verification(string remitterMobile, string accountNumber, string bankCode, string agentTransactionID)
    {
        return "";
    }

    public string FundTransfer(string remittermobile, string beneficiaryid, string amount, string channel, string agentTransactionID, string merchant_document_id_type, string merchant_document_id, string latlong, string pincode)
    {
        return "";
    }

    public string FundTransfer_Status(string client_id_or_Eko_id)
    {
        return "";
    }

    public string GetBankDetails_BankCode(string bank_code)
    {
        return "";
    }

    public string GetBankDetails_IFSC(string ifsc)
    {
        return "";
    }

    public string RefundTransaction(string eko_TID, string otp, string new_client_ref_id)
    {
        return "";
    }

    public string Refund_ResendOTP(string eko_TID)
    {
        return "";
    }

    public static decimal GetSurcharge(int PackageID, decimal Amount)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; int isFlat = 0;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from tblDMT_Surcharge where startval<=" + Amount + " and endval>=" + Amount + " and packageid='" + PackageID.ToString() + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToInt32(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == 0)
                    surcharge_amt = ((Amount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }

            // int roundoff = cls.select_data_scalar_int("SELECT CEILING(" + surcharge_amt + ")");
            return Convert.ToDecimal(surcharge_amt);
        }
        else
        {
            return surcharge_amt;
        }
    }

    public static decimal GetUPITransferSurcharge(int PackageID, decimal Amount)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from UPITransferSURCHARGE where startval<=" + Amount + " and endval>=" + Amount + " and packageid='" + PackageID.ToString() + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == false)
                    surcharge_amt = ((Amount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }

            // int roundoff = cls.select_data_scalar_int("SELECT CEILING(" + surcharge_amt + ")");
            return Convert.ToDecimal(surcharge_amt);
        }
        else
        {
            return surcharge_amt;
        }
    }
    public static decimal GetUPISurcharge(decimal Amount)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from UPISURCHARGE where startval<=" + Amount + " and endval>=" + Amount + " order by id asc");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == true)
                    surcharge_amt = surcharge_rate;
                else
                    surcharge_amt = ((Amount) * surcharge_rate) / 100;
            }

            // int roundoff = cls.select_data_scalar_int("SELECT CEILING(" + surcharge_amt + ")");
            return Convert.ToDecimal(surcharge_amt);
        }
        else
        {
            return surcharge_amt;
        }
    }

    public static decimal GetPGSurcharge(decimal Amount, string Mode)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from CommonSurchargeTable where Pgmode='" + Mode + "' order by pgid asc");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == true)
                    surcharge_amt = surcharge_rate;
                else
                    surcharge_amt = ((Amount) * surcharge_rate) / 100;
            }

            // int roundoff = cls.select_data_scalar_int("SELECT CEILING(" + surcharge_amt + ")");
            return Convert.ToDecimal(surcharge_amt);
        }
        else
        {
            return surcharge_amt;
        }
    }

    public static decimal GetVirtualSurcharge(decimal Amount)
    {
        decimal surcharge_amt = 1; decimal surcharge_rate = 0; bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from VirtualSURCHARGE where startval<=" + Amount + " and endval>=" + Amount + " order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == true)
                    surcharge_amt = surcharge_rate;
                else
                    surcharge_amt = ((Amount) * surcharge_rate) / 100;

            }

            // int roundoff = cls.select_data_scalar_int("SELECT CEILING(" + surcharge_amt + ")");
            return Convert.ToDecimal(surcharge_amt);
        }
        else
        {
            return surcharge_amt;
        }
    }
    public static decimal GetPayoutCharge(decimal Amount, string Type)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from payoutsurcharge where startval<=" + Amount + " and endval>=" + Amount + " and paymentType='" + Type + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == true)
                    surcharge_amt = surcharge_rate;
                else
                    surcharge_amt = ((Amount) * surcharge_rate) / 100;
            }

            return Convert.ToDecimal(surcharge_amt);
        }
        else
        {
            return surcharge_amt;
        }
    }
    public static decimal GetCabCommission(int PackageID, decimal Amount)
    {
        decimal Commission_amt = 0; decimal surcharge_rate = 0; int isFlat = 0;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = cls.select_data_dt("Select top 1 * from tblCab_Commission where startval<=" + Amount + " and endval>=" + Amount + " and packageid='" + PackageID.ToString() + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToInt32(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == 0)
                    Commission_amt = ((Amount) * surcharge_rate) / 100;
                else
                    Commission_amt = surcharge_rate;
            }

            // int roundoff = cls.select_data_scalar_int("SELECT CEILING(" + surcharge_amt + ")");
            return Convert.ToDecimal(Commission_amt);
        }
        else
        {
            return Commission_amt;
        }
    }

    public static decimal GetSurcharge_DMTMaha(string UserID, decimal netAmount)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; int isFlat = 0;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        string PackageID = cls.select_data_scalar_string("Select packageid from tblmlm_membermaster where MemberID='" + UserID + "'");
        dtsr = cls.select_data_dt("Select top 1 * from tblDMT_3_Surcharge where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToInt32(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == 0)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }
            return surcharge_amt;
        }
        else
        {
            return surcharge_amt;
        }
    }




    public static decimal GetSurcharge(string UserID, decimal netAmount)
    {
        decimal surcharge_amt = 0; decimal surcharge_rate = 0; int isFlat = 0;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        string PackageID = cls.select_data_scalar_string("Select packageid from tblmlm_membermaster where MemberID='" + UserID + "'");
        dtsr = cls.select_data_dt("Select top 1 * from tblMM_Surcharge where startval<=" + netAmount + " and endval>=" + netAmount + " and packageid='" + PackageID + "' order by id");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["surcharge"].ToString());
            isFlat = Convert.ToInt32(dtsr.Rows[0]["isflat"].ToString());
            if (surcharge_rate > 0)
            {
                if (isFlat == 0)
                    surcharge_amt = ((netAmount) * surcharge_rate) / 100;
                else
                    surcharge_amt = surcharge_rate;
            }
            return surcharge_amt;
        }
        else
        {
            return surcharge_amt;
        }
    }

    public static string GetUniqueSessionID()
    {
        CultureInfo ci = new CultureInfo("en-US");
        DateTime dt = DateTime.Parse(DateTime.Now.ToString());
        Random rnd = new Random();
        string Session = dt.ToString("ddMMyyyy") + dt.ToString("HHmmss") + rnd.Next(00000, 99999);
        return Session;
    }

    public static string GetTimeStamp()
    {
        CultureInfo ci = new CultureInfo("en-US");
        DateTime dt = DateTime.Parse(DateTime.Now.ToString());
        Random rnd = new Random();
        string Session = dt.ToString("HHmmss");
        return Session;
    }

    public static bool CheckMinimumBalance(int Msrno, decimal Amount, decimal CurrentWalletBalance)
    {
        cls_connection cls = new cls_connection();
        decimal minimumBalance = 0;
        try
        {
            string MemberTypeID = cls.select_data_scalar_string("select MemberTypeID from tblMLM_MemberMaster where Msrno=" + Msrno + "");
            DataTable dt = new DataTable();
            dt = cls.select_data_dt("select  * from tblMLM_Minimumbalance");
            if (MemberTypeID == "3")
            {
                minimumBalance = Convert.ToDecimal(dt.Rows[0]["MDMinimumBal"]);
            }
            if (MemberTypeID == "4")
            {
                minimumBalance = Convert.ToDecimal(dt.Rows[0]["DistributorMinimumBal"]);
            }
            if (MemberTypeID == "5")
            {
                minimumBalance = Convert.ToDecimal(dt.Rows[0]["RetailerMinimumBal"]);
            }
            if (MemberTypeID == "6")
            {
                try
                {
                    minimumBalance = Convert.ToDecimal(dt.Rows[0]["CustomerMinimumBal"]);
                }
                catch { minimumBalance = 0; }
            }
        }
        catch { minimumBalance = 0; }

        if (minimumBalance <= (CurrentWalletBalance - Amount))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public async static Task<SlabModel> GetCommisisonSlabWithName(string PackageID, decimal netAmount, string APIID, string ServiceID, string OPID, string OPName)
    {
        // Default surcharge amount
        decimal surcharge_amt = 0;
        decimal surcharge_rate = 0;
        bool isFlat = false;
        DataTable dtsr = new DataTable();
        cls_connection cls = new cls_connection();
        dtsr = await cls.select_data_dtAsync("GETSLABName " + PackageID + "," + netAmount + "," + APIID + ",'" + ServiceID + "','" + OPID + "','" + OPName + "'");
        if (dtsr.Rows.Count > 0)
        {
            surcharge_rate = Convert.ToDecimal(dtsr.Rows[0]["SlabCharge"].ToString());
            isFlat = Convert.ToBoolean(dtsr.Rows[0]["SlabIsPF"].ToString());

            // Calculate the surcharge amount
            surcharge_amt = isFlat ? surcharge_rate : (netAmount * surcharge_rate) / 100;

            return new SlabModel()
            {
                surcharge = surcharge_amt,
                startVal = Convert.ToDecimal(dtsr.Rows[0]["startval"].ToString()),
                endVal = Convert.ToDecimal(dtsr.Rows[0]["endVal"].ToString()),
                OpID = dtsr.Rows[0]["OpId"].ToString().ToInt32(),
                IsGst = true,
                Gst = 18,
                IsTds = true,
                Tds = 2
            };
        }


        surcharge_amt = (netAmount * 3) / 100;

        return new SlabModel()
        {
            surcharge = surcharge_amt,
            startVal = 1,
            endVal = 100000,
            OpID = 1168,
            IsGst = true,
            Gst = 18,
            IsTds = true,
            Tds = 2
        };
    }


}