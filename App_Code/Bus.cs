using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Bus
/// </summary>
public class Bus
{
    public Bus()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}



public class GetBusLink
{
    public string APIID { get; set; }
    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }
    public string refid { get; set; }
    public string redirect_url { get; set; }

    public string GetJson()
    {
        GetBusLink ObjData = new GetBusLink();
        ObjData.refid = this.refid;
        ObjData.redirect_url = "https://sonitechno.in/";
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "buslink";

        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}
public class GetSourceCity : IValidatableObject
{
    public string APIID { get; set; }
    public string Token { get; set; }
    [Required]
    public string MethodName { get; set; }

    public string GetJson()
    {
        GetSourceCity ObjData = new GetSourceCity();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "getsource";
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(MethodName))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}



public class GetAvailable : IValidatableObject
{
    public string APIID { get; set; }
    public string Token { get; set; }
    public string MethodName { get; set; }
    public string source_id { get; set; }
    public string destination_id { get; set; }
    public string date_of_journey { get; set; }


    public string GetJson()
    {
        GetAvailable ObjData = new GetAvailable();
        ObjData.APIID = SONITechnoCredentrial.APIID;
        ObjData.Token = SONITechnoCredentrial.Token;
        ObjData.MethodName = "getavailable";
        ObjData.source_id = this.source_id;
        ObjData.destination_id = this.destination_id;
        ObjData.date_of_journey = this.date_of_journey;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(MethodName))
        {
            yield return new ValidationResult("Can't Null");
        }
    }
}
