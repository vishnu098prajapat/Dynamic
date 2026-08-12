using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for ICreditCard
/// </summary>
public interface ICreditCard
{
    Task<string> CreditCardPay(CreditCardRequest uPIVerfication, string MemberID,string Source);
    Task<string> CreditCardBillFetch(dynamic Request);
    Task<string> BillerList();

}


public class CreditCardRequest
{
    public string Name { get; set; }
    public string CardNo { get; set; }
    public string Mobileno { get; set; }
    public string Amount { get; set; }
    public string CardType { get; set; }
    public string FetchID { get; set; }
}