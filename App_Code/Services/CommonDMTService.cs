using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for CommonDMTService
/// </summary>
public class CommonDMTService : ICommonDMTService
{
    cls_connection cls;
    IServiceList serviceList;
    ICommissionService commissionService;
    ICommonService commonService;
    IWallet wallet;
    // Define once at class level
    private static readonly HashSet<string> supportedPipes = new HashSet<string>
    {
        "Payout",
        "DMTPayoutSettlement","SafeDMT","Bank1","Bank2"
    };




    public CommonDMTService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commonService = new CommonService();
        commissionService = new CommissionService();
    }

    public async Task<DataTable> GetBankList(string Pipe)
    {
        if (Pipe == "Bank1" || Pipe == "Bank2")
        {
            var requestPayload = new
            {
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token,
                MethodName = "banklist"
            };

            string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);

            string responseJson = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.DMT);
            var data = JObject.Parse(responseJson);

            return DMTAllProcess.ConvertJsonToDataTable(responseJson);
        }
        else
        {
            var requestPayload = new
            {
                APIID = SONITechnoCredentrial.APIID,
                Token = SONITechnoCredentrial.Token,
                MethodName = "banklist"
            };

            string RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);

            string responseJson = await ApiPostCall.PostCallAsync(RequestJson, SONITechnoCredentrial.MoneyURL);
            var data = JObject.Parse(responseJson);

            return DMTAllProcess.ConvertJsonToDataTable(responseJson);
        }
    }

    public async Task<string> GetCusResult(CommonSearch commonSearch)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonSearch.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        string Result = string.Empty;
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        IQueryRemitter queryRemitter = new QueryRemitter();

        if (commonSearch.Pipe == "Payout")
            commonSearchResult = await queryRemitter.PayoutDMTQueryRemitter(commonSearch);
        else if (commonSearch.Pipe == "DMTPayoutSettlement")
            commonSearchResult = await queryRemitter.DMTPayoutDMTQueryRemitter(commonSearch);
        else if (commonSearch.Pipe == "Bank1" || commonSearch.Pipe == "Bank2")
            commonSearchResult = await queryRemitter.FinoBankSoniTechnoQueryRemitter(commonSearch);
        else
            return ErrorShow.ErrorMessageAPI("Get Remitter is not supported by the selected bank pipe.");

        var successCodes = new[] { "0", "1", "2", "4" };

        // Check result and return accordingly
        if (!successCodes.Contains(commonSearchResult.RCode))
        {
            return ErrorShow.ErrorMessageAPI(commonSearchResult.Message);
        }

        string response = Newtonsoft.Json.JsonConvert.SerializeObject(commonSearchResult);


        return ErrorShow.SuccessArrayAPI(response);
    }
    public async Task<string> GetCusResultWithOTP(CommonSearchWithOTP commonSearch)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonSearch.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonSearch.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        string Result = string.Empty;
        CommonSearchResult commonSearchResult = new CommonSearchResult();


        IQueryRemitter queryRemitter = new QueryRemitter();

        if (commonSearch.Pipe == "Payout")
            commonSearchResult = await queryRemitter.PayoutDMTWithOTP(commonSearch);
        else
            return ErrorShow.ErrorMessageAPI("Get Remitter is not supported by the selected bank pipe.");

        var successCodes = new[] { "0", "1", "2", "4" };

        // Check result and return accordingly
        if (!successCodes.Contains(commonSearchResult.RCode))
        {
            return ErrorShow.ErrorMessageAPI(commonSearchResult.Message);
        }

        string response = Newtonsoft.Json.JsonConvert.SerializeObject(commonSearchResult);


        return ErrorShow.SuccessArrayAPI(response);
    }
    public async Task<string> EKYCPRrocess(CommonDMTEkycProcess commonEkycProcess)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonEkycProcess.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonEkycProcess.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
                _memberAllowDetails.PackgeID.ToString(),
                0,
                serviceData.APIID.ToString(),
                commonEkycProcess.ServiceID, "SKYC"
            );

        //if (item.surcharge == 0)
      //      return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


        string hddGroupTransID = await wallet.GetOrderID();
        decimal MyBalance = 0;
        decimal charge = 0M;
        decimal mySurcharge = item.surcharge;

        MyBalance = _memberAllowDetails.MainBalance;

        if (MyBalance < (charge + _memberAllowDetails.HoldAMT + mySurcharge))
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }



        string Result = string.Empty;
        CommonSearchResult commonSearchResult = new CommonSearchResult();

        if (commonEkycProcess.Pipe == "Bank1" || commonEkycProcess.Pipe == "Bank2")
            commonSearchResult = await DMTAllProcess.FinoEKYCSoniDMT(commonEkycProcess);
        else
            return ErrorShow.ErrorMessageAPI("EKYC Process is not supported by the selected bank pipe.");

        var successCodes = new[] { "0", "1", "2" };

        // Check result and return accordingly
        if (!successCodes.Contains(commonSearchResult.RCode))
        {
            return ErrorShow.ErrorMessageAPI(commonSearchResult.Message);
        }

        string response = Newtonsoft.Json.JsonConvert.SerializeObject(commonSearchResult);

        return ErrorShow.SuccessArrayAPI(response);
    }
    public async Task<string> Registration(CommonDMTRegistration commonDMTRegistration)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonDMTRegistration.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTRegistration.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
                _memberAllowDetails.PackgeID.ToString(),
                0,
                serviceData.APIID.ToString(),
                commonDMTRegistration.ServiceID, "SKYC"
            );

       // if (item.surcharge == 0)
        //    return ErrorShow.ErrorMessageAPI("Slab configuration not set.");


        string hddGroupTransID = await wallet.GetOrderID();
        decimal MyBalance = 0;
        decimal charge = 0M;
        decimal mySurcharge = item.surcharge;

        MyBalance = _memberAllowDetails.MainBalance;

        if (MyBalance < (charge + _memberAllowDetails.HoldAMT + mySurcharge))
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }

        const decimal gstRate = 18m;
        decimal gstMultiplier = gstRate / 100;
        decimal GSTCalculation = 0, PaySurcharge = 0;

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


        string Result = string.Empty;

        if (commonDMTRegistration.Pipe == "Bank1" || commonDMTRegistration.Pipe == "Bank2")
            Result = await DMTAllProcess.FinoRegisterRemitterSoniDMT(commonDMTRegistration);
        else
            return ErrorShow.ErrorMessageAPI("Register is not supported by the selected bank pipe.");

        JObject jData = JObject.Parse(Result);

        string Status = "Success";

        if (Convert.ToString(jData["code"]) == "ERR")
        {
            Status = "FAILED";
        }

        string maskedAadhar = new string('*', commonDMTRegistration.AadharNo.Length - 4)
                      + commonDMTRegistration.AadharNo.Substring(commonDMTRegistration.AadharNo.Length - 4);



        int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + ",0," + PaySurcharge + ",0,'" + Status + "','EKYC','','" + agentID + "','','','" + Result + "',0,'" + commonDMTRegistration.Pipe + "','" + maskedAadhar + "','" + commonDMTRegistration.Mobile + "','ALL','EKYC','" + agentID + "'," + serviceData.APIID + ",'" + _memberAllowDetails.ShopName + "','" + _memberAllowDetails.Mobile + "','" + _memberAllowDetails.ShopName + "','" + GSTCalculation + "','Web','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','','" + commonDMTRegistration.ServiceID + "','" + item.OpID + "','" + _memberAllowDetails.MainBalance + "','" + (_memberAllowDetails.MainBalance - item.surcharge) + "'");
        if (Val > 0)
        {
            string narration = "EKYC/" + commonDMTRegistration.Pipe + "/" + agentID;
            if (Status.ToUpper() == "SUCCESS")
            {
                await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','0','DR','" + narration + "',''," + item.OpID + ",'" + agentID + "',0,0,0," + PaySurcharge + "," + GSTCalculation + "");
                return ErrorShow.SuccessMessageAPI(jData["mess"].ToString());
            }
        }
        return Result;
    }
    public async Task<string> RegisterBeneficiary(CommonDMTRegisterBeneficiary commonDMTRegisterBeneficiary)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonDMTRegisterBeneficiary.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTRegisterBeneficiary.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        string Result = string.Empty;

        if (commonDMTRegisterBeneficiary.Pipe == "Payout")
            Result = await DMTAllProcess.PayoutAddBeni(commonDMTRegisterBeneficiary);
        else if (commonDMTRegisterBeneficiary.Pipe == "DMTPayoutSettlement")
            Result = await DMTAllProcess.DMTPayoutAddBeni(commonDMTRegisterBeneficiary);
        else if (commonDMTRegisterBeneficiary.Pipe == "Bank1" || commonDMTRegisterBeneficiary.Pipe == "Bank2")
            Result = await DMTAllProcess.FinoRegisterBeneficiarySoniDMT(commonDMTRegisterBeneficiary);
        else
            return ErrorShow.ErrorMessageAPI("Register Beneficiary is not supported by the selected bank pipe.");

        return Result;
    }
    public async Task<string> verification(CommonDMTBeneficiaryVerification commonDMTBeneficiaryVerification)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(AccountVerificationService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTBeneficiaryVerification.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
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

        if (MyBalance < (charge + _memberAllowDetails.HoldAMT + mySurcharge))
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }
        const decimal gstRate = 18m;
        decimal gstMultiplier = gstRate / 100;
        decimal GSTCalculation = 0, PaySurcharge = mySurcharge;

        
            GSTCalculation = item.IsPlusMinGst
       ? item.surcharge * gstMultiplier
       : item.surcharge - (item.surcharge / (1 + gstMultiplier));

            PaySurcharge = item.IsPlusMinGst
                ? item.surcharge
                : item.surcharge - GSTCalculation;
        

        string agentID = await wallet.GetOrderID();
        commonDMTBeneficiaryVerification.OrderID = agentID;

        string Dec = $"{commonDMTBeneficiaryVerification.AccountNo}/{commonDMTBeneficiaryVerification.IFSCCODE}/{agentID}";
        string narration = $"{commonDMTBeneficiaryVerification.AccountNo}/{commonDMTBeneficiaryVerification.IFSCCODE}/{agentID}";

        var Result = new PayoutCommonResponse();


        
            Result = await DMTAllProcess.SoniTechnoVerification(commonDMTBeneficiaryVerification);
        

        if (Result.Status.ToUpper() == "SUCCESS" || Result.Status.ToUpper() == "PENDING")
        {

            if (Result.IsOTP == false && Result.IsTPIN == false)
            {
                await cls.update_dataAsync("UPDATE  BeneficiaryDMT SET otpVerify=1 WHERE accountNumber='" + commonDMTBeneficiaryVerification.AccountNo + "' ");
                DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + charge + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SAccountVerification + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
            }

            var cres = new
            {
                RCode = "0",
                Message = "Verification Process",
                Name = Result.Name,
                IsOTP = Result.IsOTP,
                IsTpin = Result.IsTPIN,
                stateresp = Result.stateresp
            };

            string jsonString = JsonSerializer.Serialize(cres);
            return ErrorShow.SuccessArrayAPI(jsonString);

        }
        else
        {
            return ErrorShow.ErrorMessageAPI(Result.Message);
        }
    }

    public async Task<string> verificationWithOTP(CommonDMTBeneficiaryVerificationWithOTP commonDMTBeneficiaryVerification)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(AccountVerificationService.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTBeneficiaryVerification.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
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

        if (MyBalance < (charge + _memberAllowDetails.HoldAMT + mySurcharge))
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }
        const decimal gstRate = 18m;
        decimal gstMultiplier = gstRate / 100;
        decimal GSTCalculation = 0, PaySurcharge = 0;

       
            GSTCalculation = item.IsPlusMinGst
       ? item.surcharge * gstMultiplier
       : item.surcharge - (item.surcharge / (1 + gstMultiplier));

            PaySurcharge = item.IsPlusMinGst
                ? item.surcharge
                : item.surcharge - GSTCalculation;
        

        string agentID = await wallet.GetOrderID();
        commonDMTBeneficiaryVerification.OrderID = agentID;

        string Dec = $"{commonDMTBeneficiaryVerification.AccountNo}/{commonDMTBeneficiaryVerification.IFSCCODE}/{agentID}";
        string narration = $"{commonDMTBeneficiaryVerification.AccountNo}/{commonDMTBeneficiaryVerification.IFSCCODE}/{agentID}";

        var Result = new PayoutCommonResponse();

        
            Result = await DMTAllProcess.SoniTechnoVerificationWithOTP(commonDMTBeneficiaryVerification);
        

        if (Result.Status.ToUpper() == "SUCCESS" || Result.Status.ToUpper() == "PENDING")
        {
            int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + charge + "," + PaySurcharge + ",0,'Success','Verification','" + Result.Message + "','" + agentID + "','" + Result.VendorID + "','" + Result.RRN + "','" + Result.Result + "',0,'" + commonDMTBeneficiaryVerification.BankName + "','" + commonDMTBeneficiaryVerification.AccountNo + "','" + commonDMTBeneficiaryVerification.IFSCCODE + "','ALL','" + Result.Name + "','" + hddGroupTransID + "'," + serviceData.APIID + ",'" + commonDMTBeneficiaryVerification.CusName + "','" + commonDMTBeneficiaryVerification.CustomerMobile + "','" + Result.Name + "','" + GSTCalculation + "','" + commonDMTBeneficiaryVerification.Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + commonDMTBeneficiaryVerification.IP + "'");
            if (Val > 0)
            {
                await cls.update_dataAsync("UPDATE  BeneficiaryDMT SET otpVerify=1 WHERE accountNumber='" + commonDMTBeneficiaryVerification.AccountNo + "' ");
                DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + charge + "','DR','" + narration + "','" + Dec + "'," + ConstantsData.SAccountVerification + ",'" + agentID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");

                var cres = new
                {
                    RCode = "0",
                    Message = "Verification Process",
                    Name = Result.Name,
                    IsOTP = Result.IsOTP,
                    IsTpin = Result.IsTPIN,
                    stateresp = Result.stateresp
                };

                string jsonString = JsonSerializer.Serialize(cres);
                return ErrorShow.SuccessArrayAPI(jsonString);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI("Try After Sometime");
            }
        }
        else
        {
            return ErrorShow.ErrorMessageAPI(Result.Message);
        }
    }


    public async Task<string> DeleteBeneficiary(CommonDMTDeleteBeneficiary commonDMTDeleteBeneficiary)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonDMTDeleteBeneficiary.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTDeleteBeneficiary.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        string Result = string.Empty;

        if (commonDMTDeleteBeneficiary.Pipe == "Payout")
            Result = await DMTAllProcess.PayoutDeleteBeneficiaryDMT(commonDMTDeleteBeneficiary);
        else if (commonDMTDeleteBeneficiary.Pipe == "Bank1")
            Result = await DMTAllProcess.FinoDeleteBeneficiarySoniDMT(commonDMTDeleteBeneficiary);
        else
            return ErrorShow.ErrorMessageAPI("Delete Beneficiary is not supported by the selected bank pipe.");

        return Result;
    }
    public async Task<string> FetchBeneficiary(CommonDMTFetchListBeneficiary commonDMTFetchListBeneficiary)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonDMTFetchListBeneficiary.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTFetchListBeneficiary.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        string Result = string.Empty;


        if (commonDMTFetchListBeneficiary.Pipe == "Payout")
            Result = await DMTAllProcess.PayoutBeniList(commonDMTFetchListBeneficiary);
        else if (commonDMTFetchListBeneficiary.Pipe == "DMTPayoutSettlement")
            Result = await DMTAllProcess.DMTPayoutBeniList(commonDMTFetchListBeneficiary);
        else if (commonDMTFetchListBeneficiary.Pipe == "Bank1" || commonDMTFetchListBeneficiary.Pipe == "Bank2")
            Result = await DMTAllProcess.FinoFetchListBeneficiarySoniDMT(commonDMTFetchListBeneficiary);
        return Result;
    }
    public async Task<string> sendmoneyspecialsendotp(CommonSendMoneyOTP commonDMTFetchListBeneficiary)
    {

        // 1. Validate Pipe before wallet deduction
        if (string.IsNullOrEmpty(commonDMTFetchListBeneficiary.Pipe))
            return ErrorShow.ErrorMessageAPI("Invalid Bank Pipe.");


        if (!supportedPipes.Contains(commonDMTFetchListBeneficiary.Pipe))
            return ErrorShow.ErrorMessageAPI("DMT is not supported by the selected bank pipe.");


        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonDMTFetchListBeneficiary.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTFetchListBeneficiary.MemberID);


        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        decimal Amount = Convert.ToDecimal(commonDMTFetchListBeneficiary.amount);

        if (Amount <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Amount should be valid");
        }


        commonDMTFetchListBeneficiary.OrderID = await wallet.GetOrderID();
        decimal MyBalance = 0;
        MyBalance = _memberAllowDetails.MainBalance;

        if (MyBalance < (Amount + _memberAllowDetails.HoldAMT))
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }

        commonDMTFetchListBeneficiary.OrderID = await wallet.GetOrderID();

        string Result = string.Empty;

        if (commonDMTFetchListBeneficiary.Pipe == "Payout")
            Result = await DMTAllProcess.PayoutTransactionSendOtpPaysprintDMT(commonDMTFetchListBeneficiary);
        else if (commonDMTFetchListBeneficiary.Pipe == "DMTPayoutSettlement")
            Result = await DMTAllProcess.DMTPayoutTransactionSendOtp(commonDMTFetchListBeneficiary);
        else if (commonDMTFetchListBeneficiary.Pipe == "Bank1" || commonDMTFetchListBeneficiary.Pipe == "Bank2")
            Result = await DMTAllProcess.FinoTransactionSendOtpSoniDMT(commonDMTFetchListBeneficiary);
        else
            return ErrorShow.ErrorMessageAPI("OTP is not supported by the selected bank pipe.");


        return Result;
    }

    public async Task<string> sendmoneyspecial(CommonSendMoney commonSendMoney)
    {
        var errorMessage = ValidatorHelper.Validate(commonSendMoney);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            return ErrorShow.ErrorMessageAPI(errorMessage);
        }


        // 1. Validate Pipe before wallet deduction
        if (string.IsNullOrEmpty(commonSendMoney.Pipe))
            return ErrorShow.ErrorMessageAPI("Invalid Bank Pipe.");

        if (!supportedPipes.Contains(commonSendMoney.Pipe))
            return ErrorShow.ErrorMessageAPI("DMT is not supported by the selected bank pipe.");


        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonSendMoney.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonSendMoney.MemberID);


        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }


        if (commonSendMoney.Amount <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Amount should be valid");
        }


        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(_memberAllowDetails.PackgeID.ToString(), Convert.ToDecimal(commonSendMoney.Amount), serviceData.APIID.ToString(), commonSendMoney.ServiceID, "MTE");

        if (item.surcharge <= 0)
            return ErrorShow.ErrorMessageAPI("Slab configuration not set." + commonSendMoney.Amount);


        decimal MyBalance = 0;
        MyBalance = _memberAllowDetails.MainBalance;

        decimal mySurcharge = 0;
        mySurcharge = mySurcharge + item.surcharge;

        if (MyBalance < (commonSendMoney.Amount + _memberAllowDetails.HoldAMT + mySurcharge))
        {
            return ErrorShow.ErrorMessageAPI("Low Balance");
        }

        commonSendMoney.OrderID = await wallet.GetOrderID();




        const decimal gstRate = 18m;
        decimal gstMultiplier = gstRate / 100;
        decimal GSTCalculation = 0, PaySurcharge = mySurcharge;

        if (item.IsGst)
        {
            GSTCalculation = item.IsPlusMinGst
       ? item.surcharge * gstMultiplier
       : item.surcharge - (item.surcharge / (1 + gstMultiplier));

            PaySurcharge = item.IsPlusMinGst
                ? item.surcharge
                : item.surcharge - GSTCalculation;
        }


        string narration = $"{commonSendMoney.AccNumber}/{commonSendMoney.Isfc}/{commonSendMoney.OrderID}";
        string Dec = narration;

        DataTable dtval = await cls.select_data_dtAsync("exec PROC_EWALLETTRANSACTION '" + _memberAllowDetails.LoginID + "','" + commonSendMoney.Amount + "','DR','" + narration + "','" + Dec + "'," + commonSendMoney.ServiceID + ",'" + commonSendMoney.OrderID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");


        if (dtval.Rows.Count == 0)
            return ErrorShow.ErrorMessageAPI("Low Balance");

        if (dtval.Rows[0]["msrno"].ToString() != "1")
            return ErrorShow.ErrorMessageAPI("Low Balance");


        int Val = await cls.update_dataAsync("AddEditMoneyTransfer " + _memberAllowDetails.Msrno + "," + commonSendMoney.Amount + "," + PaySurcharge + ",'" + commonSendMoney.BeniID + "','Pending','" + commonSendMoney.txntype + "','','" + commonSendMoney.OrderID + "','','','',0,'" + commonSendMoney.BankName + "','" + commonSendMoney.AccNumber + "','" + commonSendMoney.Isfc + "','ALL','" + commonSendMoney.BeneName + "','" + commonSendMoney.GUID + "'," + serviceData.APIID + ",'" + commonSendMoney.CusName + "','" + commonSendMoney.CustomerMobile + "','" + commonSendMoney.BeneName + "','" + GSTCalculation + "','" + commonSendMoney.Source + "','" + _memberAllowDetails.latitude + "','" + _memberAllowDetails.longitude + "','" + commonSendMoney.IP + "','" + commonSendMoney.ServiceID + "'," + item.OpID + "," + _memberAllowDetails.MainBalance + "");
        if (Val <= 0)
            return ErrorShow.ErrorMessageAPI("Try After Sometime");

        PayoutCommonResponse payoutCommonResponse = new PayoutCommonResponse();


        if (commonSendMoney.Pipe == "Payout")
            payoutCommonResponse = await DMTAllProcess.PayoutTransaction(commonSendMoney);
        else if (commonSendMoney.Pipe == "DMTPayoutSettlement")
            payoutCommonResponse = await DMTAllProcess.DMTPayoutTransaction(commonSendMoney);
        else if (commonSendMoney.Pipe == "Bank1" || commonSendMoney.Pipe == "Bank2")
            payoutCommonResponse = await DMTAllProcess.FinoTransactionSendSoniDMT(commonSendMoney);
        else
        {
            Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Failed',RefundDate=getdate(),Message='" + payoutCommonResponse.Message + "' where OrderID='" + commonSendMoney.OrderID + "'");
            if (ValUpdate == 1)
            {
                dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + commonSendMoney.Amount + ", 'CR','Refund - " + narration + "',''," + commonSendMoney.ServiceID + ",'" + commonSendMoney.OrderID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                if (dtval.Rows.Count > 0)
                    return ErrorShow.ErrorMessageAPI("Transfer is not supported by the selected bank pipe.");
            }
        }

        // Log Transaction
        try
        {
            await commonService.SaveLogs(new LogsRequestModel()
            {
                ServiceID = commonSendMoney.ServiceID.ToInt32(),
                APIID = serviceData.APIID,
                TransID = commonSendMoney.OrderID,
                RequestData = payoutCommonResponse?.Request.Replace("'", "") ?? string.Empty,
                ResponseData = payoutCommonResponse?.Result.Replace("'", "") ?? string.Empty,
                CallBackData = ""
            });
        }
        catch (Exception logEx)
        {

        }

        string Status = payoutCommonResponse.Status.ToUpper();
        if (Status == "SUCCESS" || Status == "PENDING")
        {
            cls.update_data("Update MoneyTransfer set Status='" + Status + "',VendorID='" + payoutCommonResponse.VendorID + "',Response='',Message='" + payoutCommonResponse.Message + "',RRN='" + payoutCommonResponse.RRN + "' where OrderID='" + payoutCommonResponse.OrderID + "'");
            object returnData = new
            {
                agentID = payoutCommonResponse.OrderID,
                Message = "Transaction Process",
                status = Status,
                TXNO = payoutCommonResponse.TXNO,
                Status = Status,
                UTR = payoutCommonResponse.RRN,
                Reason = payoutCommonResponse.Message,
                GID = payoutCommonResponse.GUID,
            };

            if (Status.ToUpper() == "SUCCESS")
            {
                Int32 UpdateCommission = await commissionService.GiveDMTCommission(payoutCommonResponse.OrderID);
            }
            string Request = Newtonsoft.Json.JsonConvert.SerializeObject(returnData);
            return ErrorShow.SuccessArrayAPI(Request);
        }
        else
        {
            Int32 ValUpdate = await cls.update_dataAsync("Update MoneyTransfer set Status='Failed',RefundDate=getdate(),Message='" + payoutCommonResponse.Message + "' where OrderID='" + payoutCommonResponse.OrderID + "'");
            if (ValUpdate == 1)
            {
                dtval = await cls.select_data_dtAsync("Exec [PROC_EWALLETTRANSACTION] '" + _memberAllowDetails.LoginID + "'," + commonSendMoney.Amount + ", 'CR','Refund - " + narration + "',''," + commonSendMoney.ServiceID + ",'" + commonSendMoney.OrderID + "',0,0,0,'" + PaySurcharge + "','" + GSTCalculation + "'");
                if (dtval.Rows.Count > 0)
                {
                    return ErrorShow.ErrorMessageAPI(payoutCommonResponse.Message);
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
    public async Task<string> PayoutCustomerRegistrationRequest(PayoutCustomerRegistrationRequest commonDMTRegistration)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonDMTRegistration.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTRegistration.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        string Result = string.Empty;

        if (commonDMTRegistration.Pipe == "Payout")
            Result = await DMTAllProcess.PayoutCustomerRegistrationRequest(commonDMTRegistration);
        else if (commonDMTRegistration.Pipe == "DMTPayoutSettlement")
            Result = await DMTAllProcess.RegisterRemitterDMTPayoutSettlementSoniTechno(commonDMTRegistration);
        else
            return ErrorShow.ErrorMessageAPI("Customer Registration is not supported by the selected bank pipe.");

        return Result;
    }
    public async Task<string> RegisterBeneficiaryWithOTP(CommonDMTRegisterBeneficiaryWithOTP commonDMTRegisterBeneficiary)
    {
        // Check the service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(commonDMTRegisterBeneficiary.ServiceID);
        if (serviceData.Status != "Success")
        {
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
        }

        var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(commonDMTRegisterBeneficiary.MemberID);

        if (_memberAllowDetails.HoldUnHold)
        {
            return ErrorShow.ErrorMessageAPI("Contact To Admin");
        }

        string Result = string.Empty;

        if (commonDMTRegisterBeneficiary.Pipe == "Payout")
            Result = await DMTAllProcess.PayoutAddBeniWithOTP(commonDMTRegisterBeneficiary);
        else if (commonDMTRegisterBeneficiary.Pipe == "DMTPayoutSettlement")
            Result = await DMTAllProcess.DMTPayoutAddBeniWithOTP(commonDMTRegisterBeneficiary);
        else
            return ErrorShow.ErrorMessageAPI("Beneficiary addition with OTP is not supported by the selected bank pipe.");

        return Result;
    }
    private bool IsValidSlab(SlabModel slabDetails, decimal amount)
    {
        return slabDetails.startVal <= amount &&
               slabDetails.endVal >= amount &&
               slabDetails.startVal > 0 &&
               slabDetails.endVal > 0;
    }
}