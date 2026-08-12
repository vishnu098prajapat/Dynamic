using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for KYCService
/// </summary>
public class KYCService : IKYCService
{
    cls_connection cls;
    IWallet wallet;
    IServiceList serviceList;
    private static string ServiceID = "50";

    public KYCService()
    {
        cls = new cls_connection();
        serviceList = new ServiceList();
        wallet = new Wallet();
    }

    public async Task<string> SendOTP(PaysAadharSendOTP paysAadharSendOTP, string MemberID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(KYCService.ServiceID);
        if (serviceData.Status == "Success")
        {
            paysAadharSendOTP.refid = await wallet.GetOrderID();
            paysAadharSendOTP.MethodName = "sendotp";
            string Result = await ApiPostCall.PostCallAsync(paysAadharSendOTP.GetJson(), SONITechnoCredentrial.KYCURL);
            JObject jdata = JObject.Parse(Result);
            if (jdata["code"].ToString().ToUpper() != "ERR")
            {
                object req = new
                {
                    OrderID = paysAadharSendOTP.refid,
                    Message = jdata["mess"].ToString()
                };
                string jsonString = JsonSerializer.Serialize(req);

                return ErrorShow.SuccessArrayAPI(jsonString);
            }
            else
            {
                return ErrorShow.ErrorMessageAPI(jdata["mess"].ToString());
            }
        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }

    public async Task<string> VerifyOTP(OTPVerifyAadhar oTPVerifyAadhar, string MemberID)
    {
        var serviceData = await serviceList.CheckServiceStatusWithAPIID(KYCService.ServiceID);
        if (serviceData.Status == "Success")
        {
            var _memberAllowDetails = await wallet.GetMemberAllowPermissionDetails(MemberID);
            if (!_memberAllowDetails.HoldUnHold)
            {
                PaysAadharSendOTPVerify paysAadharSendOTPVerify = new PaysAadharSendOTPVerify();
                paysAadharSendOTPVerify.refid = oTPVerifyAadhar.OrderID;
                paysAadharSendOTPVerify.MethodName = "verifyOTP";
                paysAadharSendOTPVerify.otp = oTPVerifyAadhar.OTP;

                string Result = await ApiPostCall.PostCallAsync(paysAadharSendOTPVerify.GetJson(), SONITechnoCredentrial.KYCURL);
                JObject jdata = JObject.Parse(Result);
                if (jdata["code"].ToString().ToUpper() != "ERR")
                {
                    Int32 Val = 0;
                    Val = await cls.update_dataAsync("AddEditAadharVerification '" + _memberAllowDetails.Msrno + "','" + oTPVerifyAadhar.AadharNo + "','" + paysAadharSendOTPVerify.refid + "','" + jdata["data"]["full_name"].ToString() + "','" + Result + "'");
                    if (Val > 0)
                    {
                        await cls.update_dataAsync("update CustomerDMT set IsKYC=1,Limit=500000,SingleSlot=49999 where Mobile='" + oTPVerifyAadhar.CustomerNo + "'");
                        return ErrorShow.SuccessMessageAPI(jdata["mess"].ToString());
                    }
                    else
                        return ErrorShow.ErrorMessageAPI("Try After Sometime");
                }
                else
                {
                    return ErrorShow.ErrorMessageAPI(jdata["mess"].ToString());
                }
            }
            else
                return ErrorShow.ErrorMessageAPI("Not Allow Contact to Admin");
        }
        else
            return ErrorShow.ErrorMessageAPI(serviceData.Reason);
    }
}