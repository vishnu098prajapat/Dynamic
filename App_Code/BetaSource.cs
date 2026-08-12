using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for BetaSource
/// </summary>
public class BetaSource
{
    public static readonly string APIID = "";
    public static readonly string Token = "";
    //public static readonly string Token = "7f97fe34-013c-4d3d-8908-867be63d6929";
    public static readonly string URL = "https://betasourcesoftware.com/api/payoutapi";
    public BetaSource()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static async Task<string> PostCall(string body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(BetaSource.URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(BetaSource.URL, Method.Post);

        request.AddParameter("application/json", body, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return (response.Content);
    }



}
public class BetaMoneyTransferRequestST : IValidatableObject
{
    public string APIID { get; set; }

    public string Token { get; set; }

    [Required]
    public string MethodName { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    public string Amount { get; set; }

    [Required(ErrorMessage = "PaymentType is required")]
    public string Mode { get; set; }

    [Required(ErrorMessage = "IFSC is required")]
    public string IFSC { get; set; }

    [Required(ErrorMessage = "Account Number is required")]
    public string Accountno { get; set; }

    [Required(ErrorMessage = "OrderID is required")]
    public string OrderID { get; set; }

    [Required(ErrorMessage = "CustomerMobileNo is required")]
    public string Mobile { get; set; }

    public string Name { get; set; }

    public string BankName { get; set; }


    public string GetJson()
    {
        BetaMoneyTransferRequestST ObjData = new BetaMoneyTransferRequestST();
        ObjData.IFSC = this.IFSC;
        ObjData.Accountno = this.Accountno;
        ObjData.OrderID = this.OrderID;
        ObjData.Amount = this.Amount;
        ObjData.Mobile = this.Mobile;
        ObjData.Mode = this.Mode;
        ObjData.APIID = BetaSource.APIID;
        ObjData.Token = BetaSource.Token;
        ObjData.MethodName = this.MethodName;
        ObjData.Name = this.Name;
        ObjData.BankName = this.BankName;

        string jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Accountno))
        {
            yield return new ValidationResult("Account number cannot be null or empty", new[] { nameof(Accountno) });
        }

        if (!string.IsNullOrEmpty(Mobile) && Mobile.Length != 10)
        {
            yield return new ValidationResult("Mobile number must be 10 digits", new[] { nameof(Mobile) });
        }
        var validModes = new[] { "IMPS", "NEFT", "RTGS" };
        if (!string.IsNullOrEmpty(Mode) && !validModes.Contains(Mode.ToUpper()))
        {
            yield return new ValidationResult($"Mode must be one of: {string.Join(", ", validModes)}", new[] { nameof(Mode) });
        }
    }
}