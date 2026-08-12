using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Summary description for HashGenerator
/// </summary>
public class HashGenerator
{
    public HashGenerator()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static string GenerateHashKey(Dictionary<string, string> parameters, string salt, string hashingMethod = "SHA512")
    {
        string secureHash = null;
        var sortedParameters = parameters.OrderBy(p => p.Key);
        var hashData = new StringBuilder();

        foreach (var param in sortedParameters)
        {
            if (!string.IsNullOrEmpty(param.Value))
            {
                hashData.Append("~").Append(param.Value.Trim());
            }
        }
        if (hashData.Length > 0)
        {
            //
            //secureHash = ComputeHash(hashData.ToString()+"~"+ salt, hashingMethod);
            //secureHash = ComputeHash("AMOUNT=100~CURRENCY_CODE=356~CUST_EMAIL=vipinsoni1995@gmail.com~CUST_NAME=Vipin~CUST_PHONE=8005575599~CUST_STREET_ADDRESS1=Gurgaon~CUST_ZIP=302020~MERCHANTNAME=MAVYFINTECH~ORDER_ID=ORD12345667754343~PAY_ID=5283540415172919~PRODUCT_DESC=CD Player~RETURN_URL=https://mavyfintech.com/letspgcallback.aspx~TXNTYPE=SALEb7bba7e2872c4ffb", hashingMethod);
            secureHash = ComputeHash("AMOUNT=1000~CURRENCY_CODE=356~CUST_EMAIL=vipinsoni1995@gmail.com~CUST_NAME=Vipin~CUST_PHONE=8005575599~CUST_STREET_ADDRESS1=Gurgaon~CUST_ZIP=302020~MERCHANTNAME=MAVYFINTECH~ORDER_ID=2405311531297579521~PAY_ID=5283540415172919~PRODUCT_DESC=CD Player~RETURN_URL=https://mavyfintech.com/letspgcallback.aspx~TXNTYPE=SALEb7bba7e2872c4ffb", hashingMethod);
            //secureHash = ComputeHash(dd, hashingMethod);
        }

        return secureHash;
    }


    public static string GenerateHashKey(pgRequest pgRequest,string hashingMethod = "SHA512")
    {
        string secureHash = null;
        //secureHash = ComputeHash(hashData.ToString()+"~"+ salt, hashingMethod);
        secureHash = ComputeHash("AMOUNT="+ pgRequest.AMOUNT + "~CURRENCY_CODE=356~CUST_EMAIL="+ pgRequest.CUST_EMAIL + "~CUST_NAME="+ pgRequest.CUST_NAME + "~CUST_PHONE="+ pgRequest.CUST_PHONE + "~CUST_STREET_ADDRESS1="+ pgRequest.CUST_STREET_ADDRESS1 + "~CUST_ZIP="+ pgRequest.CUST_ZIP + "~MERCHANTNAME="+ pgRequest.MERCHANTNAME + "~ORDER_ID="+ pgRequest.ORDER_ID + "~PAY_ID="+ pgRequest.PAY_ID + "~PRODUCT_DESC="+ pgRequest.PRODUCT_DESC+ "~RETURN_URL="+ pgRequest.RETURN_URL+ "~TXNTYPE=SALE"+ pgRequest.Salt+ "", hashingMethod);
        //secureHash = ComputeHash(dd, hashingMethod);
        return secureHash;
    }

    private static string ComputeHash(string input, string hashingMethod)
    {
        HashAlgorithm algorithm;

        switch (hashingMethod.ToUpper())
        {
            case "SHA256":
                algorithm = SHA256.Create();
                break;
            case "SHA512":
            default:
                algorithm = SHA512.Create();
                break;
        }

        byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
    }




}