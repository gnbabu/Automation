using System;
using System.Security.Cryptography;
using System.Text;

namespace Selenium.BaseComponents.Utilities
{
    public class EncryptDycrypt
    {
        private static string SaltKey = "sblw-3hn8-sqoy19";
        public EncryptDycrypt()
        {
        }

#pragma warning disable SYSLIB0021 // MD5CryptoServiceProvider and TripleDESCryptoServiceProvider are obsolete
#pragma warning disable SYSLIB0022

        public static string ReturnEncryptedPassword(string TextToEncrypt)
        {
            byte[] MyEncryptedArray = UTF8Encoding.UTF8.GetBytes(TextToEncrypt);
            MD5CryptoServiceProvider MyMD5CryptoService = new MD5CryptoServiceProvider();
            byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SaltKey));
            MyMD5CryptoService.Clear();
            var MyTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            MyTripleDESCryptoService.Key = MysecurityKeyArray;
            MyTripleDESCryptoService.Mode = CipherMode.ECB;
            MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;
            var MyCrytpoTransform = MyTripleDESCryptoService.CreateEncryptor();
            byte[] MyresultArray = MyCrytpoTransform.TransformFinalBlock(MyEncryptedArray, 0, MyEncryptedArray.Length);
            MyTripleDESCryptoService.Clear();
            return Convert.ToBase64String(MyresultArray, 0, MyresultArray.Length);
        }

        public static string GetPasswordText(string TextToDecrypt)
        {
            try
            {
                byte[] MyDecryptArray = Convert.FromBase64String(TextToDecrypt);
                MD5CryptoServiceProvider MyMD5CryptoService = new MD5CryptoServiceProvider();
                byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SaltKey));
                MyMD5CryptoService.Clear();
                var MyTripleDESCryptoService = new TripleDESCryptoServiceProvider();
                MyTripleDESCryptoService.Key = MysecurityKeyArray;
                MyTripleDESCryptoService.Mode = CipherMode.ECB;
                MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;
                var MyCrytpoTransform = MyTripleDESCryptoService.CreateDecryptor();
                byte[] MyresultArray = MyCrytpoTransform.TransformFinalBlock(MyDecryptArray, 0, MyDecryptArray.Length);
                MyTripleDESCryptoService.Clear();
                return UTF8Encoding.UTF8.GetString(MyresultArray);
            }
            catch { return null; }
        }

#pragma warning restore SYSLIB0022
#pragma warning restore SYSLIB0021

    }
}
