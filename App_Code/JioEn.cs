using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    // ============================================================
    // AES Encryption: AES/ECB/PKCS5Padding (Java) -> PKCS7 (.NET)
    // ============================================================
    public static string EncryptAES(string plainData, string aesKey)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(aesKey);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainData);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }

    // ============================================================
    // RSA Encryption (Java: RSA/ECB/PKCS1Padding)
    // Works on ALL .NET versions (using RSACryptoServiceProvider)
    // ============================================================
    public static string EncryptRSA(string plainData, string base64PublicKey)
    {
        byte[] publicKeyBytes = Convert.FromBase64String(base64PublicKey);

        RSAParameters rsaParams = DecodeX509PublicKey(publicKeyBytes);

        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(rsaParams);

            byte[] dataBytes = Encoding.UTF8.GetBytes(plainData);

            // PKCS1 Padding = true
            byte[] encrypted = rsa.Encrypt(dataBytes, false);

            return Convert.ToBase64String(encrypted);
        }
    }

    // ============================================================
    // Java X.509 Public Key Parser (Universal For All .NET Versions)
    // ============================================================
    private static RSAParameters DecodeX509PublicKey(byte[] x509Key)
    {
        using (MemoryStream mem = new MemoryStream(x509Key))
        using (BinaryReader reader = new BinaryReader(mem))
        {
            byte bt = 0;
            ushort twobytes = 0;

            twobytes = reader.ReadUInt16();
            if (twobytes == 0x8130) reader.ReadByte();
            else if (twobytes == 0x8230) reader.ReadInt16();
            else throw new Exception("Invalid X509 key format");

            twobytes = reader.ReadUInt16();
            if (twobytes != 0x300D) throw new Exception("Invalid X509 key format");

            reader.ReadBytes(15);

            twobytes = reader.ReadUInt16();
            if (twobytes == 0x8103) reader.ReadByte();
            else if (twobytes == 0x8203) reader.ReadInt16();
            else throw new Exception("Invalid X509 key format");

            bt = reader.ReadByte();
            if (bt != 0x00) throw new Exception("Invalid X509 key format");

            twobytes = reader.ReadUInt16();
            if (twobytes == 0x8130) reader.ReadByte();
            else if (twobytes == 0x8230) reader.ReadInt16();
            else throw new Exception("Invalid X509 key format");

            twobytes = reader.ReadUInt16();
            int modulusSize = twobytes;

            byte[] modulus = reader.ReadBytes(modulusSize);

            if (reader.ReadByte() != 0x02)
                throw new Exception("Invalid X509 key format");

            int exponentSize = reader.ReadByte();
            byte[] exponent = reader.ReadBytes(exponentSize);

            return new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            };
        }
    }

    // ============================================================
    // Secure Random String Generator
    // ============================================================
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        byte[] buffer = new byte[length];
        StringBuilder sb = new StringBuilder(length);

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }

        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[buffer[i] % chars.Length]);
        }

        return sb.ToString();
    }
}
