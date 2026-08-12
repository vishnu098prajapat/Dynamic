using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for VanST
/// </summary>
public class VanST : IValidatableObject
{
    [Required]
    public string APIID { get; set; }
    [Required]
    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    [Required(ErrorMessage = "Mobile is required")]
    [MinLength(10, ErrorMessage = "Please Enter Valid Mobile")]
    [MaxLength(10, ErrorMessage = "Please Enter Valid Mobile")]
    public string mobile { get; set; }

    [Required(ErrorMessage = "name is required")]
    [MinLength(4, ErrorMessage = "Please Enter Valid name")]
    [MaxLength(20, ErrorMessage = "Please Enter Valid name")]
    public string name { get; set; }

    [Required(ErrorMessage = "referenceno is required")]
    [MinLength(11, ErrorMessage = "Please Enter Valid referenceno")]
    [MaxLength(11, ErrorMessage = "Please Enter Valid referenceno")]
    public string partnerId { get; set; }

    public string GetJson()
    {
        VanST ObjData = new VanST();
        ObjData.mobile = this.mobile;
        ObjData.name = this.name;
        ObjData.partnerId = this.partnerId;
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = this.MethodName;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(partnerId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(mobile))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}



public class Van
{
    public static string PostCall(string Request, string URL)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        var options = new RestClientOptions("")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient();
        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        var body = Request;
        request.AddParameter("application/json", body, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }
}