using System.Security.Cryptography;
using System.Text;

namespace AutomationAPI.Repositories.Helpers
{
    // Mirrors Selenium.BaseComponents.Utilities.EncryptDycrypt's exact algorithm (MD5-derived
    // key + TripleDES in ECB mode, same salt) for consistency with the existing codebase -
    // reused deliberately rather than switching to AES. This is a separate, self-contained
    // copy (not a shared reference) because AutomationAPI deliberately never references
    // AutomationTests/Selenium.BaseComponents, to avoid pulling Selenium/WebDriver
    // dependencies into the API's own deployable.
    public static class CredentialCipher
    {
        private static readonly string SaltKey = "sblw-3hn8-sqoy19";

#pragma warning disable SYSLIB0021 // MD5CryptoServiceProvider and TripleDESCryptoServiceProvider are obsolete
#pragma warning disable SYSLIB0022

        public static string Encrypt(string plainText)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(plainText);
            using var md5 = new MD5CryptoServiceProvider();
            byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(SaltKey));
            using var tripleDes = new TripleDESCryptoServiceProvider
            {
                Key = keyBytes,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            using var encryptor = tripleDes.CreateEncryptor();
            byte[] resultBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);
            return Convert.ToBase64String(resultBytes);
        }

        public static string? Decrypt(string cipherText)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using var md5 = new MD5CryptoServiceProvider();
                byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(SaltKey));
                using var tripleDes = new TripleDESCryptoServiceProvider
                {
                    Key = keyBytes,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };
                using var decryptor = tripleDes.CreateDecryptor();
                byte[] resultBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(resultBytes);
            }
            catch
            {
                return null;
            }
        }

#pragma warning restore SYSLIB0022
#pragma warning restore SYSLIB0021
    }
}
