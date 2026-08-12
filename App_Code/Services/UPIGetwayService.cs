using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for UPIGetwayService
/// </summary>
public class UPIGetwayService : IUPIGetway
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    ICommonService commonService;
    private static string ServiceID = "19";

    public UPIGetwayService()
    {
        cls = new cls_connection();
        wallet = new Wallet();
        serviceList = new ServiceList();
        commonService = new CommonService();
    }

    private bool IsValidSlab(SlabModel slabDetails, decimal amount)
    {
        return slabDetails.startVal <= amount &&
               slabDetails.endVal >= amount &&
               slabDetails.startVal > 0 &&
               slabDetails.endVal > 0;
    }

    public async Task<string> UPIRequest(string MemberID, Int32 Amount, string Source)
    {
        // Check service status
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(UPIGetwayService.ServiceID);
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

        if (!_memberAllowDetails.KYC)
        {
            return ErrorShow.ErrorMessageAPI("Fisrt Complete your kyc");
        }

        if (Common.CheckServiceAssignOrNot(Convert.ToString(_memberAllowDetails.Msrno), "19", cls) != 1)
        {
            return ErrorShow.ErrorMessageAPI("Service Not Active,Contact to Customer Care");
        }

        // Validate amount
        if (Amount <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Enter a valid amount");
        }

        // Retrieve member balance
        string agentID = await wallet.GetOrderID();

        // Step 5: Get surcharge and commission details
        var item = await MoneyTransfer_EkoAPI.GetCommisisonSlab(
            _memberAllowDetails.PackgeID.ToString(),
            Amount,
            serviceData.APIID.ToString(),
            UPIGetwayService.ServiceID, "UP"
        );

        if (!IsValidSlab(item, Amount))
            return ErrorShow.ErrorMessageAPI("Slab configuration not set.");

        decimal surcharge = item.surcharge;

        Decimal GSTCalculation = 0;
        Decimal PaySurcharge = 0;

        const decimal gstRate = 18m;
        decimal gstMultiplier = gstRate / 100;

        PaySurcharge = item.surcharge;
        if (item.IsGst)
        {
            GSTCalculation = item.IsPlusMinGst
                ? item.surcharge * gstMultiplier
                : item.surcharge - (item.surcharge / (1 + gstMultiplier));

            PaySurcharge = item.IsPlusMinGst
                ? item.surcharge
                : item.surcharge - GSTCalculation;
        }

        // Database insertion
        int val = await cls.update_dataAsync($"PROC_FundAdd_UPI_Virtaul_Transaction " + _memberAllowDetails.Msrno + ",'','" + agentID + "','','" + Source + "','" + _memberAllowDetails.ShopName + "','" + _memberAllowDetails.Mobile + "','" + Amount + "','0','" + Amount + "','','UPI','','','" + PaySurcharge + "','" + GSTCalculation + "','" + UPIGetwayService.ServiceID + "','" + item.OpID + "','" + serviceData.APIID + "'");

        if (val <= 0)
        {
            return ErrorShow.ErrorMessageAPI("Try After Sometime");
        }

        string result = "";

        if (serviceData.APIID == 1)
        {
            // APIID 1 specific implementation
            string request = "{\"key\": \"" + UPIGetway.token + "\",\"client_txn_id\": \"" + agentID + "\",\"amount\": \"" + Amount + "\",\"p_info\": \"Product Name\",\"customer_name\": \"" + _memberAllowDetails.ShopName + "\",\"customer_email\": \"" + _memberAllowDetails.Email + "\",\"customer_mobile\": \"" + _memberAllowDetails.Mobile + "\",\"redirect_url\": \"" + UPIGetway.Redirect + "\",\"udf1\": \"user defined field 1 (max 25 char)\",\"udf2\": \"user defined field 2 (max 25 char)\",\"udf3\": \"user defined field 3 (max 25 char)\"\r\n}";

            result = await tezindia.PostCall(Amount.ToString(), agentID);

            try
            {
                await commonService.SaveLogs(new LogsRequestModel()
                {
                    ServiceID = UPIGetwayService.ServiceID.ToInt32(),
                    APIID = serviceData.APIID,
                    TransID = agentID,
                    RequestData = request,
                    ResponseData = result,
                    CallBackData = ""
                });
            }
            catch { }

            JObject jData = JObject.Parse(result);

            if (Convert.ToBoolean(jData["status"].ToString()))
            {
                jData["result"]["data"]["payment_url"] = jData["result"]["payment_url"].ToString();
                return ErrorShow.SuccessArrayAPI(jData["result"]["data"].ToString());
            }
            else
            {
                await cls.update_dataAsync("update FundAdd_UPI_Virtaul_Transaction set status='Failed',Response='" + result + "' where order_id='" + agentID + "'");
                return ErrorShow.ErrorMessageAPI(jData["message"].ToString());
            }
        }
        else if (serviceData.APIID == 1032) // FastSwiff API Condition
        {
            try
            {
                var fastSwiffResult = await FastSwiffAPI.ProcessAndRedirect(
                    amount: Convert.ToDecimal(Amount),
                    cardNumber: "",
                    nameOnCard: _memberAllowDetails.ShopName ?? "Customer",
                    expiryDate: "",
                    cvv: "",
                    customerMobile: _memberAllowDetails.Mobile,
                    customerEmail: _memberAllowDetails.Email ?? "customer@example.com",
                    billingFirstName: _memberAllowDetails.ShopName ?? "Customer",
                    billingLastName: "",
                    billingAddress: _memberAllowDetails.Address,
                    billingCity: "",
                    billingState: "Maharashtra",
                    billingPincode: _memberAllowDetails.Pincode,
                    OrderID: agentID
                );

                // Log the request
                try
                {
                    await commonService.SaveLogs(new LogsRequestModel()
                    {
                        ServiceID = UPIGetwayService.ServiceID.ToInt32(),
                        APIID = serviceData.APIID,
                        TransID = agentID,
                        RequestData = $"FastSwiff Request - Amount: {Amount}, Mobile: {_memberAllowDetails.Mobile}",
                        ResponseData = Newtonsoft.Json.JsonConvert.SerializeObject(fastSwiffResult),
                        CallBackData = ""
                    });
                }
                catch { }

                if (fastSwiffResult.Success)
                {
                    var responseData = new
                    {
                        intPermit = "Allowd",
                        bhim_link = "",
                        phonepe_link = "",
                        paytm_link = "",
                        gpay_link = "",
                        amazonpay_link = "",
                        cred_link = "",
                        qr_image = "",
                        payment_url = fastSwiffResult.RedirectUrl,
                        external_reference = fastSwiffResult.ExternalReference,
                        transaction_id = fastSwiffResult.TransactionId,
                        message = fastSwiffResult.Message
                    };

                    string jsonStringData = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                    return ErrorShow.SuccessArrayAPI(jsonStringData);
                }
                else
                {
                    await cls.update_dataAsync("update FundAdd_UPI_Virtaul_Transaction set status='Failed',Response='" + fastSwiffResult.Message + "' where order_id='" + agentID + "'");
                    return ErrorShow.ErrorMessageAPI(fastSwiffResult.Message ?? "FastSwiff API Error");
                }
            }
            catch (Exception ex)
            {
                await cls.update_dataAsync("update FundAdd_UPI_Virtaul_Transaction set status='Failed',Response='" + ex.Message + "' where order_id='" + agentID + "'");
                return ErrorShow.ErrorMessageAPI("FastSwiff API Error: " + ex.Message);
            }
        }
        else
        {
            // Default implementation for other API IDs
            string request = "{\"key\": \"" + UPIGetway.token + "\",\"client_txn_id\": \"" + agentID + "\",\"amount\": \"" + Amount + "\",\"p_info\": \"Product Name\",\"customer_name\": \"" + _memberAllowDetails.ShopName + "\",\"customer_email\": \"" + _memberAllowDetails.Email + "\",\"customer_mobile\": \"" + _memberAllowDetails.Mobile + "\",\"redirect_url\": \"" + UPIGetway.Redirect + "\",\"udf1\": \"user defined field 1 (max 25 char)\",\"udf2\": \"user defined field 2 (max 25 char)\",\"udf3\": \"user defined field 3 (max 25 char)\"\r\n}";

            result = await UPIGetway.PostCall(request, "create_order");

            try
            {
                await commonService.SaveLogs(new LogsRequestModel()
                {
                    ServiceID = UPIGetwayService.ServiceID.ToInt32(),
                    APIID = serviceData.APIID,
                    TransID = agentID,
                    RequestData = request,
                    ResponseData = result,
                    CallBackData = ""
                });
            }
            catch { }

            JObject jData = JObject.Parse(result);

            if (Convert.ToBoolean(jData["status"].ToString()))
            {
                return ErrorShow.SuccessMessageAPI(jData["data"]["payment_url"].ToString());
            }
            else
            {
                await cls.update_dataAsync("update FundAdd_UPI_Virtaul_Transaction set status='Failed',Response='" + result + "' where order_id='" + agentID + "'");
                return ErrorShow.ErrorMessageAPI(jData["msg"].ToString());
            }
        }
    }
}