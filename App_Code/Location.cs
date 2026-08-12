using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for Location
/// </summary>
public class Location
{
    public string IPAddress { get; set; }
    public string CountryName { get; set; }
    public string CountryCode { get; set; }
    public string CityName { get; set; }
    public string RegionName { get; set; }
    public string ZipCode { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string TimeZone { get; set; }


    public Location GetLocationData()
    {
        cls_connection Objconnection = new cls_connection();

        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        string ipAddress = "103.73.190.82";
        Location location;
        string APIKey = "3444b35770997f8c65cba44c97fac8e27ffbab9c161b23d0950d2b15e8900ace";
        string url = string.Format("http://api.ipinfodb.com/v3/ip-city/?key={0}&ip={1}&format=json", APIKey, ipAddress);
        

        using (WebClient client = new WebClient())
        {
            string json = client.DownloadString(url);
            location = new JavaScriptSerializer().Deserialize<Location>(json);
            List<Location> locations = new List<Location>();
            locations.Add(location);

        }
        return location;
    }

    public Location GetLocationDataCurrent(string IP)
    {
        cls_connection Objconnection = new cls_connection();

        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        string ipAddress = IP;
        Location location;
        string APIKey = "3444b35770997f8c65cba44c97fac8e27ffbab9c161b23d0950d2b15e8900ace";
        string url = string.Format("http://api.ipinfodb.com/v3/ip-city/?key={0}&ip={1}&format=json", APIKey, ipAddress);
        

        using (WebClient client = new WebClient())
        {
            string json = client.DownloadString(url);
            location = new JavaScriptSerializer().Deserialize<Location>(json);
            List<Location> locations = new List<Location>();
            locations.Add(location);

        }
        return location;
    }

  
}

