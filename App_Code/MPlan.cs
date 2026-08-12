using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MPlan
/// </summary>
public class MPlan
{
    public static string Roffer(string Opr, string Mobileno)
    {
        string Result = PostGetMPlan.Roffer(Opr, Mobileno);
        return Result;
    }

    public static string DTHDetails(string Opr, string Mobileno)
    {
        string Result = PostGetMPlan.DTHDetails(Opr.Replace(" ",""), Mobileno);
        return Result;
    }

    public static string DTHDetailsPHN(string Opr, string Mobileno)
    {
        string Result = PostGetMPlan.DTHDetailsPHN(Opr.Replace(" ", ""), Mobileno);
        return Result;
    }

    public static string DTHPlan(string Opr)
    {
        string Result = PostGetMPlan.DTHPlan(Opr);
        return Result;
    }


    
    public static string Plan(string Opr)
    {
        string Result = PostGetMPlan.Plan(Opr);
        return Result;
    }

    public static string PlanNew(string Opr,string cir)
    {
        string Result = PostGetMPlan.Plan(Opr);
        return Result;
    }

    public static string OperatorCheck(string tel)
    {
        string Result = PostGetMPlan.operatorinfo(tel);
        return Result;
    }

    public static string DTHOperatorCheck(string tel)
    {
        string Result = PostGetMPlan.DTHoperatorinfo(tel);
        return Result;
    }
    public static string Bill(string Opr, string Mobileno)
    {
        string Result = PostGetMPlan.Bill(Opr.Replace(" ", ""), Mobileno);
        return Result;
    }

    public static string PostPaid(string Opr, string Mobileno)
    {
        string Result = PostGetMPlan.PostPaid(Opr.Replace(" ", ""), Mobileno);
        return Result;
    }

    public static string WaterBill(string Opr, string Mobileno)
    {
        string Result = PostGetMPlan.WaterBill(Opr.Replace(" ", ""), Mobileno);
        return Result;
    }
}


    


public class PostGetMPlan
{
    public static string Roffer(string oprerator, string mobile)
    {
        string URL = "https://www.mplan.in/api/plans.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&offer=roffer&tel=" + mobile + "&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);

       
        var request = new RestRequest(URL,Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
       return (response.Content);
    }

    public static string DTHDetails(string oprerator, string mobile)
    {
        string URL = "https://www.mplan.in/api/Dthinfo.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&offer=roffer&tel=" + mobile + "&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);

      
        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }

    public static string DTHDetailsPHN(string oprerator, string mobile)
    {
        string URL = "https://www.mplan.in/api/DthinfoMobile.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&offer=roffer&tel=" + mobile + "&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);

        
        var request = new RestRequest(URL,Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }

    public static string DTHPlan(string oprerator)
    {
        string URL = "https://www.mplan.in/api/dthplans.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }

    public static string Plan(string oprerator)
    {
        string URL = "https://www.mplan.in/api/plans.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&cricle=Rajasthan&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }

    public static string PlanNew(string oprerator, string Cir)
    {
        string URL = "https://rapi.mobikwik.com/recharge/v1/rechargePlansAPI/" + oprerator + "/" + Cir + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }

    public static string PostPaid(string oprerator, string tel)
    {
        string URL = "https://www.mplan.in/api/Bsnl.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&offer=roffer&tel=" + tel + "&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }

    public static string operatorinfo(string oprerator)
    {
        string URL = "http://operatorcheck.mplan.in/api/operatorinfo.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&tel=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }
    public static string DTHoperatorinfo(string oprerator)
    {
        string URL = "http://operatorcheck.mplan.in/api/dthoperatorinfo.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&tel=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }
    public static string Bill(string oprerator, string mobile)
    {
        string URL = "https://www.mplan.in/api/electricinfo.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&offer=roffer&tel=" + mobile + "&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }

    public static string WaterBill(string oprerator, string mobile)
    {
        string URL = "https://www.mplan.in/api/Water.php?apikey=f3f4e67ca66eb3c7e6044cabc834b052&offer=roffer&tel=" + mobile + "&operator=" + oprerator + "";
        var options = new RestClientOptions(URL)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);


        var request = new RestRequest(URL, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Basic cnpwX2xpdmVfdllJaDNPOVVnQ2hpT2c6WnVrZ0trUkZ6SjZHdlJSMWFNejFDUk40");
        request.AddHeader("Cookie", "mplansession=gef167np5k0o8sam6feeo2c5o3");
        RestResponse response = client.Execute(request);
        return (response.Content);
    }
}