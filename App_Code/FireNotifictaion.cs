using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for FireNotifictaion
/// </summary>
public class FireNotifictaion
{
    public string to { get; set; }
    public Notification notification { get; set; }

    public string GetJson()
    {
        FireNotifictaion ObjData = new FireNotifictaion();
        ObjData.to = this.to;
        ObjData.notification = this.notification;
        string jsonString;
        jsonString = JsonConvert.SerializeObject(ObjData);
        return jsonString;
    }
}


public class Notification
{
    public string body { get; set; }
    public string title { get; set; }
    public string image { get; set; }
    public string icon { get; set; }
}


public class PostGetCallFireNotification
{
    public static string PostCall(string Jwson)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        var options = new RestClientOptions("https://fcm.googleapis.com/fcm/send")
        {
            MaxTimeout = -1,
        };

        var client = new RestClient();

        var request = new RestRequest("https://fcm.googleapis.com/fcm/send",Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "key=AAAAMzKPumA:APA91bEEcOngAlxu7FzrXbs-Eud6LBVxj1ovhKR3YtAmuMLOXxUklRO7vKSpbtivf3cslxV21g7sYH_mO2LkRqznaGSSJ9vvO6f4OGkmLqgUTds2g39qpF_CgkyGmRIGvLemp8ECnzAs");
        request.AddParameter("application/json", Jwson, ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        return response.Content;
    }

    public static void sendNotification(string Message, string Token,string Title)
    {                

        Notification notification = new Notification();
        notification.body = HttpUtility.HtmlDecode(Message);
        notification.title = Title;
        notification.icon = "https://www.onlygfx.com/wpcontent/uploads/2017/12/grunge-yes-no-icon-1-902x1024.png";
        FireNotifictaion fireNotifictaion = new FireNotifictaion();
        fireNotifictaion.to = Token;
        fireNotifictaion.notification = notification;
        PostGetCallFireNotification.PostCall(fireNotifictaion.GetJson());
    }
}