using RestSharp;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Net.Http;

public class FastSwiffAPI
{
    public static string Authorization = "Basic ";
    public static string BaseURL = "https://api.fastswiff.com";
    public static string ApiVersion = "2";

    public FastSwiffAPI()
    {
        // Constructor logic here if needed
    }

    // Internal response class
    public class CreateOrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ExternalReference { get; set; }
        public string RedirectUrl { get; set; }
        public string TransactionId { get; set; }
    }

    public class CreateOrderRequest
    {
        public string OrderReference { get; set; }
        public int MerchantReferenceId { get; set; }
        public bool NotifyBuyerSms { get; set; }
        public bool NotifyBuyerEmail { get; set; }
        public int BuyerSegmentId { get; set; }
        public string MerchantMid { get; set; }
        public string AccessKey { get; set; }
        public string RequestId { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionPurpose { get; set; }
        public PaymentOptions PaymentOptions { get; set; }
        public BuyerDetails BuyerDetails { get; set; }
        public IntegrationContext IntegrationContext { get; set; }
        public string CallbackUrl { get; set; }
        public string CreatedOn { get; set; }
        public object Metadata { get; set; }
        public string ExpiresAt { get; set; }
        public string RequestOrigin { get; set; }
        public string CreatedBy { get; set; }
        public string Origin { get; set; }
    }

    public class PaymentOptions
    {
        public object EnabledMethods { get; set; }
        public string Method { get; set; }
        public string Flow { get; set; }
        public string SurchargeMode { get; set; }
        public decimal SurchargeValue { get; set; }
        public UpiInfo UpiInfo { get; set; }
        public Card Card { get; set; }
    }

    public class UpiInfo
    {
        public string VpaAddress { get; set; }
        public object AccountNo { get; set; }
        public object Aadhaar { get; set; }
        public object Ifsc { get; set; }
        public object BankCode { get; set; }
    }

    public class Card
    {
        public string CardNo { get; set; }
        public string NameOnCard { get; set; }
        public string ExpiryDate { get; set; }
        public string Cvv { get; set; }
    }

    public class BuyerDetails
    {
        public string BuyerId { get; set; }
        public string Remarks { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
    }

    public class BillingAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Line { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
        public string Mobile { get; set; }
    }

    public class ShippingAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Line { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
        public string Mobile { get; set; }
    }

    public class IntegrationContext
    {
        public string Initiator { get; set; }
        public string SourceId { get; set; }
        public string IntegrationModel { get; set; }
        public string HashSignature { get; set; }
        public string IpAddress { get; set; }
    }

    public async static Task<CreateOrderResponse> CreateOrder(CreateOrderRequest orderRequest)
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseURL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", Authorization);
                client.DefaultRequestHeaders.Add("X-API-VERSION", ApiVersion);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);


                var json = Newtonsoft.Json.JsonConvert.SerializeObject(orderRequest, Newtonsoft.Json.Formatting.Indented);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                cls_connection cls = new cls_connection();
                cls.update_data("insert into logdata(data)values('" + json + "')");

                var response = await client.PostAsync("/payment/api/order/create", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                cls.update_data("insert into logdata(data)values('" + responseContent + "')");

                if (response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        try
                        {

                            JObject apiResponse = JObject.Parse(responseContent);


                            if (apiResponse["TransactionId"] != null)
                            {
                                string transactionId = apiResponse["TransactionId"]?.ToString();
                                string externalReference = apiResponse["ExternalReference"]?.ToString();
                                string redirectUrl = apiResponse["RedirectUrl"]?.ToString();


                                string paymentMessage = "";
                                if (apiResponse["Payment"] != null && apiResponse["Payment"]["ResponseMessage"] != null)
                                {
                                    paymentMessage = apiResponse["Payment"]["ResponseMessage"]?.ToString();
                                }

                                return new CreateOrderResponse
                                {
                                    Success = true,
                                    Message = paymentMessage ?? "Transaction created successfully",
                                    ExternalReference = externalReference,
                                    RedirectUrl = redirectUrl,
                                    TransactionId = transactionId
                                };
                            }
                            // Check for error case
                            else if (apiResponse["Success"] != null)
                            {
                                bool success = (bool)apiResponse["Success"];
                                string message = apiResponse["Message"]?.ToString();

                                return new CreateOrderResponse
                                {
                                    Success = success,
                                    Message = message ?? "API response received",
                                    RedirectUrl = null,
                                    ExternalReference = null,
                                    TransactionId = null
                                };
                            }
                            else
                            {
                                return new CreateOrderResponse
                                {
                                    Success = false,
                                    Message = "Unexpected response format from API",
                                    RedirectUrl = null,
                                    ExternalReference = null,
                                    TransactionId = null
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            return new CreateOrderResponse
                            {
                                Success = false,
                                Message = $"JSON Parse Error: {ex.Message}",
                                RedirectUrl = null,
                                ExternalReference = null,
                                TransactionId = null
                            };
                        }
                    }
                    else
                    {
                        return new CreateOrderResponse
                        {
                            Success = false,
                            Message = "Empty response from API",
                            RedirectUrl = null,
                            ExternalReference = null,
                            TransactionId = null
                        };
                    }
                }
                else
                {
                    string errorMessage = $"HTTP Error: {(int)response.StatusCode} - {response.StatusCode}";


                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        try
                        {
                            JObject errorResponse = JObject.Parse(responseContent);
                            if (errorResponse["message"] != null)
                            {
                                errorMessage = errorResponse["message"]?.ToString();
                            }
                            else if (errorResponse["error"] != null)
                            {
                                errorMessage = errorResponse["error"]?.ToString();
                            }
                            else if (errorResponse["Message"] != null)
                            {
                                errorMessage = errorResponse["Message"]?.ToString();
                            }
                            else
                            {
                                errorMessage = responseContent;
                            }
                        }
                        catch
                        {
                            errorMessage = responseContent;
                        }
                    }

                    return new CreateOrderResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        RedirectUrl = null,
                        ExternalReference = null,
                        TransactionId = null
                    };
                }
            }
        }
        catch (TaskCanceledException ex)
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = $"Request timeout: {ex.Message}",
                RedirectUrl = null,
                ExternalReference = null,
                TransactionId = null
            };
        }
        catch (HttpRequestException ex)
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = $"Network error: {ex.Message}",
                RedirectUrl = null,
                ExternalReference = null,
                TransactionId = null
            };
        }
        catch (Exception ex)
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = $"Unexpected error: {ex.Message}",
                RedirectUrl = null,
                ExternalReference = null,
                TransactionId = null
            };
        }
    }


    public async static Task<CreateOrderResponse> CreateCreditCardOrder(
        decimal amount,
        string cardNumber,
        string nameOnCard,
        string expiryDate,
        string cvv,
        string customerMobile,
        string customerEmail,
        string billingFirstName,
        string billingLastName,
        string billingAddress,
        string billingCity,
        string billingState,
        string billingPincode,
        string requestId)
    {
        var orderRequest = new CreateOrderRequest
        {
            OrderReference = requestId,
            MerchantReferenceId = 121229,
            NotifyBuyerSms = true,
            NotifyBuyerEmail = true,
            BuyerSegmentId = 0,
            MerchantMid = "",
            AccessKey = null,
            RequestId = requestId ?? Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10),
            TransactionAmount = amount,
            TransactionPurpose = "UPI",
            PaymentOptions = new PaymentOptions
            {
                EnabledMethods = null,
                Method = "UPI",
                Flow = "",
                SurchargeMode = "Amount",
                SurchargeValue = 0,
                UpiInfo = new UpiInfo
                {
                    VpaAddress = "",
                    AccountNo = null,
                    Aadhaar = null,
                    Ifsc = null,
                    BankCode = null
                },
                Card = new Card
                {
                    CardNo = cardNumber,
                    NameOnCard = nameOnCard,
                    ExpiryDate = expiryDate,
                    Cvv = cvv
                }
            },
            BuyerDetails = new BuyerDetails
            {
                BuyerId = "2626",
                Remarks = "",
                Mobile = customerMobile,
                Email = customerEmail,
                BillingAddress = new BillingAddress
                {
                    FirstName = billingFirstName,
                    LastName = billingLastName,
                    Line = billingAddress,
                    City = billingCity,
                    State = billingState,
                    Country = "IN",
                    Pincode = billingPincode,
                    Mobile = ""
                },
                ShippingAddress = new ShippingAddress
                {
                    FirstName = "",
                    LastName = "",
                    Line = "",
                    City = "",
                    State = "",
                    Country = "",
                    Pincode = "",
                    Mobile = ""
                }
            },
            IntegrationContext = new IntegrationContext
            {
                Initiator = "ravi",
                SourceId = "",
                IntegrationModel = "1",
                HashSignature = "",
                IpAddress = "183.82.97.220"
            },
            CallbackUrl = null,
            CreatedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Metadata = null,
            ExpiresAt = null,
            RequestOrigin = null,
            CreatedBy = null,
            Origin = null
        };

        return await CreateOrder(orderRequest);
    }


    public async static Task<PaymentResult> ProcessAndRedirect(
        decimal amount,
        string cardNumber,
        string nameOnCard,
        string expiryDate,
        string cvv,
        string customerMobile,
        string customerEmail,
        string billingFirstName,
        string billingLastName,
        string billingAddress,
        string billingCity,
        string billingState,
        string billingPincode,
        string OrderID)
    {
        var response = await CreateCreditCardOrder(
            amount, cardNumber, nameOnCard, expiryDate, cvv,
            customerMobile, customerEmail, billingFirstName, billingLastName,
            billingAddress, billingCity, billingState, billingPincode, OrderID
        );

        if (response.Success && !string.IsNullOrEmpty(response.ExternalReference))
        {

            string redirectUrl = response.RedirectUrl;

            //if (string.IsNullOrEmpty(redirectUrl))
            //{

            //    redirectUrl = $"https://fastswiff.com/pay/payment2?OrderKeyId={response.ExternalReference}";
            //}

            return new PaymentResult
            {
                Success = true,
                ExternalReference = response.ExternalReference,
                RedirectUrl = redirectUrl,
                Message = response.Message,
                TransactionId = response.TransactionId
            };
        }
        else
        {
            return new PaymentResult
            {
                Success = false,
                ExternalReference = response.ExternalReference,
                RedirectUrl = null,
                Message = response.Message,
                TransactionId = response.TransactionId
            };
        }
    }

    // Method to get order status
    public async static Task<string> GetOrderStatus(string orderId)
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseURL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", Authorization);
                client.DefaultRequestHeaders.Add("X-API-VERSION", ApiVersion);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.GetAsync($"/payment/api/order/status/{orderId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
    }
}

// Additional class to handle payment results
public class PaymentResult
{
    public bool Success { get; set; }
    public string ExternalReference { get; set; }
    public string RedirectUrl { get; set; }
    public string Message { get; set; }
    public string TransactionId { get; set; }
}