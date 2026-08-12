using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

/// <summary>
/// Summary description for BillFtech
/// </summary>
namespace NewSoniTechno.App_Code.Model
{
    public class BillFtech
    {
        public string ServiceID { get; set; }
        public string ClientRefId { get; set; }
        public string Number { get; set; }
        public string SPKey { get; set; }
        public string Amount { get; set; }
        public string TelecomCircleID { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string Optional5 { get; set; }
        public string Optional6 { get; set; }
        public string Optional7 { get; set; }
        public string Optional8 { get; set; }
        public string Optional9 { get; set; }
        public string billValidationId { get; set; }
        public string ServiceCharge { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string DueDate { get; set; }
        public string BillDate { get; set; }
        public string IPAddress { get; set; }
    }



    public class BillFtechReturnResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string dueamount { get; set; }
        public string duedate { get; set; }
        public string customername { get; set; }
        public string billnumber { get; set; }
        public string minamount { get; set; }
        public string maxamount { get; set; }
        public string billdate { get; set; }
        public string acceptPartPay { get; set; }
        public string BBPSCharges { get; set; }
        public string BillUpdate { get; set; }
        public string RequestID { get; set; }
        public string ClientRefId { get; set; }
        public string BBPSComm { get; set; }
        public string billValidationId { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Result { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Request { get; set; }
    }

    public class BillPayReturnResponse
    {
        public bool status { get; set; }
        public string Rstatus { get; set; }
        public string message { get; set; }
        public string customername { get; set; }
        public string OpName { get; set; }
        public string ServiceName { get; set; }
        public string billnumber { get; set; }
        public string billdate { get; set; }
        public string BBPSCharges { get; set; }
        public string RequestID { get; set; }
        public string ClientRefId { get; set; }
        public string OefID { get; set; }
        public string Request { get; set; }
        public string Result { get; set; }
    }
}