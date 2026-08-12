using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Permission
/// </summary>
public class Permission
{
    public bool TwoWay { set; get; }
    public bool OTP { set; get; }
    public bool TPIN { set; get; }
    public bool RegistratioWIthOTPVerification { set; get; }
    public bool AddBalanceOTP { set; get; }
    public bool PanelDisable { set; get; }
    public bool BankAddOTP { set; get; }
    public bool BankAddTPIN { set; get; }
    public bool CashoutTransactionOTP { set; get; }
    public bool CashoutTransactionTPIN { set; get; }


    public bool WorkingWithoutKYC { set; get; }
    public bool AddBalanceTPIN { set; get; }
    public bool DeductBalanceOTP { set; get; }
    public bool DeductBalanceTPIN { set; get; }
    public bool MailSendRegistration { set; get; }
    public bool OTPSendRegistration { set; get; }
    public bool MailSendRegistrationOTP { set; get; }
    public bool OTPSendRegistrationOTP { set; get; }
    public bool MailSendPasswordChangeOTP { set; get; }
    public bool OTPSendPasswordChangeOTP { set; get; }
    public bool MailSendTPINChangeOTP { set; get; }
    public bool OTPSendTPINChangeOTP { set; get; }
    public bool SystemDown { set; get; }
    public bool GoogleAuth { set; get; }
    public bool PanVerify { set; get; }
    public bool AadharVerify { set; get; }

    public static Permission GetPermission()
    {
        cls_connection cls = new cls_connection();
        string Result = cls.select_data_scalar_string("SELECT top  1 *  FROM AdminAccountSetting where msrno=1 FOR JSON AUTO").Replace("[", "").Replace("]", "");
        Permission myDeserializedClass = JsonConvert.DeserializeObject<Permission>(Result);
        return myDeserializedClass;
    }
}
