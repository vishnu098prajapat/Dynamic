using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
/// <summary>
/// Summary description for CypherExample
/// </summary>
public class CypherExample
{
    protected RijndaelManaged myRijndael;
    public static string encryptionKey = "";
    public static string initialisationVector = "Orfkh4XwRubCeo+WZ+PTVQsL5V2V8MiS30e6tYJBRdSSEg2z8z1hupxYYaSSW3ZTjRRodOt3J/4YnjHhsFR6XvKLxejibnKVfs92UdQlgs03PwYEcmeAOtoRGiV1B5BxxzJrgUqXBz7AR93VgHPSiKh/yO0PjKEvMlu1njyZ0IxKatIlj9Wyt+l17+BrFd/hul/RGWKLYURrwYgC76JaMLQK3HcgGkNhcrjz2StpKd01XUS86cwPn3OlvBVexRnSQzgBBpSu4H2rhjwxENRH6FEVLQNiXg/4R7LozVSGCFFsbmwGQ9/ex9NN3mURGrcaRsNQmot94yS9/pGuGtyOWdUhNqxUsYwU9MLCMs4PyGbyUBlSfrMB6IbJSyfYr5TqMNNoFXSzUPo/VK8N77BCO9uKG5YLLlFY53NtEvY1iKe70G2huOy96eedJYJkFWDfBCXEK6Ndpbi+qUzZiHDEehiXK4hR2MlOqsf4eVG+RbVM0BzBE7Jj0RJSSQhfNINtKVCt+lOHCH7iRfcx9G8U50DtE/HhwPbsDHaJTnKt8ko=";
    public string DecryptText(string encryptedString)
    {
        

        string ourDec = "";
        using (myRijndael = new RijndaelManaged())

        {
            myRijndael.KeySize = 128;
            myRijndael.Key = System.Text.Encoding.UTF8.GetBytes(encryptionKey); //Convert.FromBase64String(Base64Decode(encryptionKey.Replace("-", "")));// HexStringToByte(encryptionKey);
            myRijndael.IV = new byte[16];// HexStringToByte(initialisationVector);
            myRijndael.Mode = CipherMode.CBC;
            //  myRijndael.Padding = PaddingMode.Zeros;
            myRijndael.Padding = PaddingMode.PKCS7;
            Byte[] ourEnc = Convert.FromBase64String(encryptedString);
            byte[] newArray = new byte[ourEnc.Length - 16];
            Array.Copy(ourEnc, 16, newArray, 0, newArray.Length);
            ourDec = DecryptStringFromBytes(ourEnc, myRijndael.Key, myRijndael.IV);
            return ourDec;

        }

    }


    public string DecryptDataNew(string data, string decryptedkey)
    {
        byte[] databyte1 = Encoding.UTF8.GetBytes(data);
        string ss = Convert.ToBase64String(databyte1);
        byte[] resbyte = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            resbyte[i] = databyte1[i];
        }
        string resp = AESDecryptNS(data, decryptedkey, resbyte);
        return resp;
    }

    public static string AESDecryptNS(string data, string pKey, byte[] piv)
    {
        string plaintext = string.Empty;
        byte[] inputBytes = Convert.FromBase64String(data);
        byte[] Key = Encoding.UTF8.GetBytes(pKey);
        byte[] iv = piv;// string.IsNullOrEmpty(piv) ? null : Encoding.UTF8.GetBytes(piv);

        byte[] plainText = GetCryptoAlgorithm().CreateDecryptor(Key, (iv == null ? Key : iv)).TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        plaintext = ASCIIEncoding.UTF8.GetString(plainText, 16, plainText.Length - 16);
        return plaintext.Replace("\u000e", string.Empty).Replace("\u0002", string.Empty).Replace("\u0006", string.Empty);
    }

    private static RijndaelManaged GetCryptoAlgorithm()
    {
        RijndaelManaged algorithm = new RijndaelManaged();
        //set the mode, padding and block size
        algorithm.Padding = PaddingMode.None;
        algorithm.Mode = CipherMode.CBC;
        algorithm.KeySize = 128;
        algorithm.BlockSize = 128;
        return algorithm;
    }


    private static AesManaged CreateAes()
    {
        var aes = new AesManaged();
        aes.Key =  System.Text.Encoding.UTF8.GetBytes(encryptionKey); //UTF8-Encoding
        aes.IV = new byte[16]; ;// System.Text.Encoding.UTF8.GetBytes(initialisationVector);//UT8-Encoding
        return aes;
    }

    public static string decrypt(string text)
    {
        using (var aes = CreateAes())
        {
            ICryptoTransform decryptor = aes.CreateDecryptor();
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

        }
    }


    public string EncryptText(string plainText,string Key,string IV)
    {
        using (myRijndael = new RijndaelManaged())
        {
            myRijndael.Key = System.Text.Encoding.UTF8.GetBytes(Key);
            myRijndael.IV = new byte[16]; 
            myRijndael.Mode = CipherMode.CBC;
            myRijndael.Padding = PaddingMode.PKCS7;
            byte[] encrypted = EncryptStringToBytes(plainText, myRijndael.Key, myRijndael.IV);
            string encString = Convert.ToBase64String(encrypted);
            return encString;
        }
    }
    protected byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)

    {

        // Check arguments.

        if (plainText == null || plainText.Length <= 0)

            throw new ArgumentNullException("plainText");

        if (Key == null || Key.Length <= 0)

            throw new ArgumentNullException("Key");

        if (IV == null || IV.Length <= 0)

            throw new ArgumentNullException("Key");

        byte[] encrypted;

        // Create an RijndaelManaged object

        // with the specified key and IV.

        using (RijndaelManaged rijAlg = new RijndaelManaged())

        {

            rijAlg.Key = Key;

            rijAlg.IV = IV;



            // Create a decrytor to perform the stream transform.

            ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);



            // Create the streams used for encryption.

            using (MemoryStream msEncrypt = new MemoryStream())

            {

                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))

                {

                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))

                    {



                        //Write all data to the stream.

                        swEncrypt.Write(plainText);

                    }

                    encrypted = msEncrypt.ToArray();

                }

            }

        }





        // Return the encrypted bytes from the memory stream.

        return encrypted;



    }



    protected string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)

    {

        // Check arguments.

        if (cipherText == null || cipherText.Length <= 0)

            throw new ArgumentNullException("cipherText");

        if (Key == null || Key.Length <= 0)

            throw new ArgumentNullException("Key");

        if (IV == null || IV.Length <= 0)

            throw new ArgumentNullException("Key");



        // Declare the string used to hold

        // the decrypted text.

        string plaintext = null;



        // Create an RijndaelManaged object
        // with the specified key and IV.

        using (RijndaelManaged rijAlg = new RijndaelManaged())

        {

            rijAlg.Key = Key;

            rijAlg.IV = IV;



            // Create a decrytor to perform the stream transform.

            ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);



            // Create the streams used for decryption.

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))

            {

                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))

                {

                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                    {



                        // Read the decrypted bytes from the decrypting stream

                        // and place them in a string.

                        plaintext = srDecrypt.ReadToEnd();

                    }

                }

            }



        }



        return plaintext;



    }



    public static void GenerateKeyAndIV()

    {

        // This code is only here for an example

        RijndaelManaged myRijndaelManaged = new RijndaelManaged();

        myRijndaelManaged.Mode = CipherMode.CBC;

        myRijndaelManaged.Padding = PaddingMode.PKCS7;


        myRijndaelManaged.GenerateIV();

        myRijndaelManaged.GenerateKey();

        string newKey = ByteArrayToHexString(myRijndaelManaged.Key);

        string newinitVector = ByteArrayToHexString(myRijndaelManaged.IV);

    }



    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static byte[] HexStringToByte(string hexString)

    {

        try

        {
            string base64Encoded = hexString;
            byte[] data = System.Convert.FromBase64String(base64Encoded);

            return data;
        }

        catch

        {

            throw;

        }

    }



    public static string ByteArrayToHexString(byte[] ba)

    {

        StringBuilder hex = new StringBuilder(ba.Length * 2);

        foreach (byte b in ba)

            hex.AppendFormat("{0:x2}", b);

        return hex.ToString();

    }

}


