using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    private const string keyPhrase = "*Gm9QYyrrFP@TUhatQv@SSFcHf^kbzpr"; // 32 bytes for AES-256
    private static readonly byte[] key = Encoding.UTF8.GetBytes(keyPhrase);
    
    private const CipherMode aesMode = CipherMode.CBC;
    private const PaddingMode aesPadding = PaddingMode.PKCS7;

    public static byte[] EncryptStringAesCbc(string plainText)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.Mode = aesMode;
        aes.Padding = aesPadding;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV

        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(plainBytes, 0, plainBytes.Length);
        cs.FlushFinalBlock();

        return ms.ToArray();
    }

    public static string DecryptStringAesCbc(byte[] data)
    {
        byte[] iv = new byte[16]; // AES block size
        Array.Copy(data, 0, iv, 0, iv.Length);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.Mode = aesMode;
        aes.Padding = aesPadding;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(data, iv.Length, data.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);

        return reader.ReadToEnd();
    }
}
