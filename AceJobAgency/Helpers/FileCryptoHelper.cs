using System.Security.Cryptography;
using System.Text;

public static class FileCryptoHelper
{
    public static byte[] Encrypt(byte[] data, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var cipher = encryptor.TransformFinalBlock(data, 0, data.Length);

        return aes.IV.Concat(cipher).ToArray();
    }

    public static byte[] Decrypt(byte[] encryptedData, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);

        var iv = encryptedData.Take(16).ToArray();
        var cipher = encryptedData.Skip(16).ToArray();

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
    }
}
