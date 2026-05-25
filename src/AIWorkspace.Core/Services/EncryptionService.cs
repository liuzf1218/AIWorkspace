using System.Security.Cryptography;

namespace AIWorkspace.Core.Services;

public static class EncryptionService
{
    public static byte[] Encrypt(string plainText)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
    }

    public static string Decrypt(byte[] encryptedData)
    {
        var bytes = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
