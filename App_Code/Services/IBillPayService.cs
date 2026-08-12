using NewSoniTechno.App_Code.Model;
using System.Data;
using System.Threading.Tasks;

/// <summary>
/// Summary description for IBillPayService
/// </summary>
public interface IBillPayService
{
    Task<string> BillFetch(BillFtech billFtech);  
    Task<string> BillPay(BillFtech billFtech,string MemberID);
    Task<OpDetails> FetchOPDetailsWithAPI(string OPCode,string APIID);
    Task<OpDetails> FetchOPDetails(string OPCode);
}


public class OpDetails
{
    public bool Status { get; set; }
    public string Message { get; set; }
    public string OpName { get; set; }
    public string ServiceName { get; set; }
    public int OPID { get; set; }
    public int ServiceID { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public bool IsPending { get; set; }
    public string APIOPCode { get; set; }
    public string Optional1 { get; set; }
    public string Optional2 { get; set; }
}