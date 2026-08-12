using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MemberPermission
/// </summary>
public class MemberPermission
{
    public bool TwoWay { set; get; } = false;
    public bool IsOTP { set; get; } = false;
    public bool IsTpin { set; get; } = false;
    public bool IsPattern { set; get; } = false;
    public bool IsGoogleAuth { set; get; } = false;
    public string AuthKey { set; get; } = "";
    public MemberPermission()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static MemberPermission GetPermission(string Msrno)
    {
        cls_connection cls = new cls_connection();
        string Result = cls.select_data_scalar_string("SELECT *  FROM MemberSecurity where msrno="+ Msrno + "  FOR JSON AUTO").Replace("[", "").Replace("]", "");
        MemberPermission myDeserializedClass = JsonConvert.DeserializeObject<MemberPermission>(Result);
        return myDeserializedClass;
    }
}