using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for INSDLService
/// </summary>
public interface INSDLService
{
    Task<int> CheckAgentRegisterOrNot(String MemberID);
    Task<string> MERCHANTREGISTRATION(NSDLMerchantRegistration nSDLMerchantRegistration, string MemberID);
    Task<string> ApplyNewPanCard(newPanApplySoft newPanApplySoft,String MemberID,String Source);
}