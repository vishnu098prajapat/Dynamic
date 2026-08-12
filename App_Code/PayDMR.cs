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
/// Summary description for PayDMR
/// </summary>
public class PayDMR
{
    public static readonly string ApiToken = "";
    public static readonly string URL = "";
    public static readonly string BalanceURL = "";

    public PayDMR()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static async Task<string> PostCall(string body)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(PayDMR.URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(PayDMR.URL, Method.Post);

        request.AddParameter("application/json", body, ParameterType.RequestBody);
        RestResponse response = await client.ExecuteAsync(request);
        return (response.Content);
    }
    public static async Task<string> GetBalance()
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions(BalanceURL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest(BalanceURL, Method.Get);

        RestResponse response = await client.ExecuteAsync(request);
        return response.Content;
    }

}

public class PayDMRTransferRequest : IValidatableObject
{
    public string api_token { get; set; }
    public string mobile_number { get; set; }
    public string email { get; set; }
    public string beneficiary_name { get; set; }
    public string ifsc_code { get; set; }
    public string account_number { get; set; }
    public decimal amount { get; set; }
    public string channel_id { get; set; }  // 1 = NEFT, 2 = IMPS
    public string client_id { get; set; }

    public string GetJson()
    {
        PayDMRTransferRequest ObjData = new PayDMRTransferRequest();
        ObjData.api_token = PayDMR.ApiToken;
        ObjData.mobile_number = this.mobile_number;
        ObjData.email = this.email;
        ObjData.beneficiary_name = this.beneficiary_name;
        ObjData.ifsc_code = this.ifsc_code;
        ObjData.account_number = this.account_number;
        ObjData.amount = this.amount;
        ObjData.channel_id = this.channel_id;
        ObjData.client_id = this.client_id;

        string jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(mobile_number))
        {
            yield return new ValidationResult("Mobile number cannot be null or empty", new[] { nameof(mobile_number) });
        }

        if (!string.IsNullOrEmpty(mobile_number) && mobile_number.Length != 10)
        {
            yield return new ValidationResult("Mobile number must be 10 digits", new[] { nameof(mobile_number) });
        }

        if (string.IsNullOrEmpty(email))
        {
            yield return new ValidationResult("Email cannot be null or empty", new[] { nameof(email) });
        }

        if (!string.IsNullOrEmpty(email) && !new EmailAddressAttribute().IsValid(email))
        {
            yield return new ValidationResult("Invalid email format", new[] { nameof(email) });
        }

        if (string.IsNullOrEmpty(beneficiary_name))
        {
            yield return new ValidationResult("Beneficiary name cannot be null or empty", new[] { nameof(beneficiary_name) });
        }

        if (string.IsNullOrEmpty(ifsc_code))
        {
            yield return new ValidationResult("IFSC code cannot be null or empty", new[] { nameof(ifsc_code) });
        }

        if (!string.IsNullOrEmpty(ifsc_code) && ifsc_code.Length != 11)
        {
            yield return new ValidationResult("IFSC code must be exactly 11 characters", new[] { nameof(ifsc_code) });
        }

        if (string.IsNullOrEmpty(account_number))
        {
            yield return new ValidationResult("Account number cannot be null or empty", new[] { nameof(account_number) });
        }

        if (amount <= 0)
        {
            yield return new ValidationResult("Amount must be greater than 0", new[] { nameof(amount) });
        }

        var validChannels = new[] { "1", "2" };
        if (!string.IsNullOrEmpty(channel_id) && !validChannels.Contains(channel_id))
        {
            yield return new ValidationResult($"Channel ID must be 1 (NEFT) or 2 (IMPS)", new[] { nameof(channel_id) });
        }

        if (string.IsNullOrEmpty(client_id))
        {
            yield return new ValidationResult("Client ID cannot be null or empty", new[] { nameof(client_id) });
        }
    }
}

public class PayDMRTransferResponse
{
    public string status { get; set; } 
    public string message { get; set; }
    public string utr { get; set; }
    public string payid { get; set; }
}