using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


/// <summary>
/// Summary description for CryptographyHelper
/// </summary>
public class CryptographyHelper
{
    private static bool _optimalAsymmetricEncryptionPadding = false;

    //These keys are of 2048byte
    private readonly static string PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAnD6sFeCJf9bR7PH/YisPwCePMUltELq/hp/Dm409XU/yEbgBTI/wMUnH+2juOTYeCirOB10bc/QrFVhaLV3gHWyvSL+Wv0ufdWeZiLXAnICNddiJCQvOU+5seNUoPw4QKvFAQgZLogsjWm2uajJSnSGskVCt48gaWVrIiO4OQZVkSTxXrUNknenXJ9b2bXam/Bt1ya1E1t1/b/ITZ/03WQjfGwXgctqYytvAHC3v541s9o53J2uWn/pOBJc2058Ad5WyzqPRHzA1gbl+XinKs85X5U5PAwJiVCsSyCFkNGwJmmuCPtrHipwZi5ax5jfrb71Plilj2VOXdrR2zU7FOQIDAQAB";
    public readonly static string PrivateKey = "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";

    public static string Encrypt(string plainText)
    {
        int keySize = 0;
        string publicKeyXml = "";

        GetKeyFromEncryptionString(PublicKey, out keySize, out publicKeyXml);

        var encrypted = Encrypt(Encoding.UTF8.GetBytes(plainText), keySize, publicKeyXml);

        return Convert.ToBase64String(encrypted);
    }

    private static byte[] Encrypt(byte[] data, int keySize, string publicKeyXml)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Data are empty", "data");
        int maxLength = GetMaxDataLength(keySize);
        if (data.Length > maxLength) throw new ArgumentException(String.Format("Maximum data length is {0}", maxLength), "data");
        if (!IsKeySizeValid(keySize)) throw new ArgumentException("Key size is not valid", "keySize");
        if (String.IsNullOrEmpty(publicKeyXml)) throw new ArgumentException("Key is null or empty", "publicKeyXml");

        using (var provider = new RSACryptoServiceProvider(keySize))
        {
            provider.FromXmlString(publicKeyXml);
            return provider.Encrypt(data, _optimalAsymmetricEncryptionPadding);
        }
    }

    public static string Decrypt(string encryptedText)
    {
        cls_connection Objss = new cls_connection();
        int keySize = 0;
        string publicAndPrivateKeyXml = "";
        GetKeyFromEncryptionString(PrivateKey, out keySize, out publicAndPrivateKeyXml);

        var decrypted = Decrypt(Convert.FromBase64String(encryptedText), keySize, publicAndPrivateKeyXml);

        return Encoding.UTF8.GetString(decrypted);
    }

    private static byte[] Decrypt(byte[] data, int keySize, string publicAndPrivateKeyXml)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Data are empty", "data");
        if (!IsKeySizeValid(keySize)) throw new ArgumentException("Key size is not valid", "keySize");
        if (String.IsNullOrEmpty(publicAndPrivateKeyXml)) throw new ArgumentException("Key is null or empty", "publicAndPrivateKeyXml");

        using (var provider = new RSACryptoServiceProvider(keySize))
        {
            provider.FromXmlString(publicAndPrivateKeyXml);
                return provider.Decrypt(data, _optimalAsymmetricEncryptionPadding);
        }
    }

    private static int GetMaxDataLength(int keySize)
    {
        if (_optimalAsymmetricEncryptionPadding)
        {
            return ((keySize - 384) / 8) + 7;
        }
        return ((keySize - 384) / 8) + 37;
    }

    private static bool IsKeySizeValid(int keySize)
    {
        return keySize >= 384 && keySize <= 16384 && keySize % 8 == 0;
    }

    private static void GetKeyFromEncryptionString(string rawkey, out int keySize, out string xmlKey)
    {
        keySize = 0;
        xmlKey = "";

        if (rawkey != null && rawkey.Length > 0)
        {

            byte[] keyBytes = Convert.FromBase64String(rawkey);
            var stringKey = Encoding.UTF8.GetString(keyBytes);

            if (stringKey.Contains("!"))
            {
                var splittedValues = stringKey.Split(new char[] { '!' }, 2);

            }
                
                keySize = 4096;
                xmlKey = "<RSAKeyValue><Modulus>1xIdnjgd0uSN1mrbPYK0Ug7deY4uU7W9z04Uz2lCfdEfOuopYJO6SlU1cjCRuJG5ObJBlwpQC48Msm9gOZNzdwNP9rjBjNOqDRwbT2CMY4eWTBA3DI1X18JIZ5uuKow080yzFxukuvRPhfNSfE3y0s0eJvXyBOSWo1bdfjzt+uRI1Xvx+4dVoUcqROEZAEIcXKqE+qqW6r6Pp40y9j1TSL4IKadsQB8nitqwIWex9x0EbEEOoB+9Y4yfVL2RldQshjamdsjbqwbx8zHs9ZDaMK7el0Qv8Ghg/a09NPEhsmC9Ugqsf8RLuAuYZEDdOT0vODNFAhx4KAfzZQwakYCCz0Z/hqJmzMXawT5W/gh+dvC9gXe31vlXRODiMXUUbPh/2foW2sG+cfwlQppg7+TPb4I2F/pvCwAa0cPUEllcHDSf7PA35uRFm5yCjNzVjbs1fHcpYyWcIirqlqkoOBRtxmjg4X/InbiHObNYNrjTmz8Q5H/Ba9NGda3bObE2Pki08RhzMkBCQFGKWS/udhRLMaJLPaGOOZMA6qpVCYue8qFgyzqsM4QgSWhpHsHGj8h4VFKi2WG6eLaj5BRbxwnyFriRQXW7iSP5PjSoQ976fvb61B32hllZThoPweMkgtdl2Tdqn6G0Yif47gQ+ogulipiPgRCQ7L6Gwssho2JCYU8=</Modulus><Exponent>AQAB</Exponent><P>9s16erzA++qPmEuBp1bybNwyWCekanTmuGh8XTPRyNNycEVpIF22mIDf3pL1gKQBuRMzlwNAce8ACKAGhD5Yyq/NLumyZIf0S1VqhLZb4WXFMuSVgVdSNtxmW0EUiCf32Qdgchg3OJ+34YIgwHp5gCKUeu3inTO8aDrewxjQsoyeGcB3/fSxMlNwAn6KlOKGuFd5K87Osmyw48Ue0s7g84rvImj1wQSYQLnHZub5WPUVX3pD15hL1uYhUX/NjTvmFBCZ80GHqp7rHUiFP4df8x1bW6wVowhA6FiukVt66GlY/+TgnR8hbcCipa0CQdwj3KlwJcE+5RtO4tJOAoDvjw==</P><Q>3xXpdXV13tkyFDm8qmgf4hODfGCh7O3+SQFLQ7CBfTt+8Y1GxPoRQfX+SulWY+5X9bLkrGyrZTfaXChYER7BTyAk3JYbs/yXHysaqHKqw0Y0nDaHKJ16ZkbCcIMwKobOTJy28XrV54QVs061ajHV7cZ6zayREmN1xiWiB2Qj+MrRINpWiy1DienXK/LZr+8zMzjF6/FB+lql2zGmKvs98NQO7o9P7pq6UBseEHwHscvg4Xc60Up19Erfyqg9KrxDqhafheUDW9vvvC+w7a+0aLkThyoZUy/bXudHn+pnOU8mYf4Rz3EspAfnjiVG73C/OTaOx60KTH4ZfbTvx7qSQQ==</Q><DP>2WYcynr0P7PZs1xnpAEZ4LyCeFbLJJ8/ERew3OSlmZJwjucFeGY0TtfO2592NE/D2MRKQG6z2OifjflwZBMLYddEFVv0MUv07FscTjTYcI6WC4OBEoLZv8ubOiz0qg8S6pDt6vLbybwfXLyknwgDeNR9GF/tq24EO4gH67EzPORS6/o3eHOPlvYah4fyCL4Vz9H1wZoVRuQpWSmtQiLL0pwCk7wDJngmqXU8aa9x22Nf5U+s7P+RZoYmuUSrywoFXI1XHwkNOUVtc8kK3vAfm+azL+hA0vCVRFXBx/beKFWL+TUiiiT+7E+wteHVSDBjQp/aUteL0IKilXiNEPWEjw==</DP><DQ>v94qsrlZrVcEuVyHJdD0Sw8ZcWnzGsH+iG+yef9e/4SiQ8KMcXHaUe7uzSMvCDfAMzOvdZTpPN7y0XxpciAPhqxQx9/a58yBdW8vOF2W/Ohu60QCry3yTfDh3r8WOB1FZaMLvVk2jC6QM22onjNKmXyxth+LjlM4wjkvyEeaoKpff3AKKx29DuquA8CU9bo3QQC9SZZwdpBsQb/L+P+pYG4igY+E4eT2BVutWeQzAfol6gUP812lYXzcSYK1SQIu+CUJArHbV7Ar2qDx/Ru3gLJdV00gBuoMNO2aLrH+Ic4vZuyMCOx/xnx97UWmYt2Huf9wgdAUIYH7DqEF8bRHgQ==</DQ><InverseQ>jZN1Wr2jBz9YWDjxCCcXv6FODbGT2hdKNL3qQcl2A6vTBjok3CcsL/Qf3CzXFzeuxfmj/kfi0IgaZ1cJ7cTISZSaITkSNlKPpC0cHdTtxAu9q+0HyS5gnYwc8y2GI673Ar5d/JnmExJoRcz7hBWmqdZTGQygiwdxvgXK/uIp2VUemGTnycYK9GJHTLex/Bdh1ot9zpIDkQnw5UfnQe+a2qCNr1CpAIEByNx2qKroVaSCjSDM5lR86K6Slmr5Vg7fXEyf5ky6YPklelnhDJCBljprdTD+NZ38InjUXoCMSNiMjQMNXTQJAtp1TvpbjJOgxjsepgzUcXYKCVo6nlf0hw==</InverseQ><D>UUv6wCUMOPzGFMf9qcmvadlOpLU3E9N+NU+ZlDGAvdh/yqBbFbOh1plRA6ISY0dKUeHs4zA1w7jGHuj04a64FcO9xVBfcWFNRXlbZWzkhHbrPENg27DOBLPe9LMnadPn7BQRGmoLodPZ/5NTmcNJR5/bhvSvEGRsAMBEJ4TWnGEQmwooFWE54CXrd2/zKGLVH74hWDOFqqRv3vED04qo4bVblos/4xkjtFwn+eidoABq/JS295Yn2+p7C+hAkPdMAUvFLt+MEaElcj1sCgNARujDIUDYMjLyftGuckW37BqfVdTNEdrcmGxhpXvTY+CZ6c+PhVXHobInsgmaPWfricbIbmEYeZcM8XxoYeStglebWB+OSki9/EOX28EmkYAfJ/zuv5jigesLZdI6yzd/37E2A95jn562YcqevMwYp+4Esh0k7dKPXxTYKZmhSMsfHbFy+/ef8DoQqZnnP0eykGHaJ7f0E31XD7aWi8wQVJtySCl55JBwnr4gYKZuYcJHZ2C5fC/r2EZOhMDlSwc3LiZ3luH2eQVVvHFbB+TpD0TB2pCp//u06BnjYKia64kROFDNzrJScEWd/ue9frEqHQcnb29tzoTNkrcUegi5CqIsZsur1PyWvqOmgqPwuKUUKwIJMFMHGUSt8/0EB645ltBSkYnAfTw3OFdVl5OY/YE=</D></RSAKeyValue>";
           
            //}
        }
    }
}
