using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Freebie.Libs
{
    public static class FreebieCrypt
    {
        private static byte[] _salt = Encoding.ASCII.GetBytes("hNj046yIkl3Zoo2011sMpjRV");
       
        public static string EncryptStringAES(string email, string account_no)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");
            if (string.IsNullOrEmpty(account_no))
                throw new ArgumentNullException("account_no");

            string outStr = null;                      
            RijndaelManaged aesAlg = null;

            try
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(account_no, _salt);
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(email);
                        }
                    }
                    outStr = HttpUtility.UrlEncode(Convert.ToBase64String(msEncrypt.ToArray()));
                }
            }
            finally
            {
                
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            return outStr;
        }

        public static string DecryptStringAES(string http_enc, string account_no)
        {
            if (string.IsNullOrEmpty(http_enc))
                throw new ArgumentNullException("http_enc");
            if (string.IsNullOrEmpty(account_no))
                throw new ArgumentNullException("account_no");
            RijndaelManaged aesAlg = null;
            string email = null;
            
            try
            {
                string enc = HttpUtility.UrlDecode(http_enc);
                enc = enc.Replace(" ", "+");
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(account_no, _salt);         
                byte[] bytes = Convert.FromBase64String(enc);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        { email = srDecrypt.ReadToEnd(); }
                    }
                }
            }
            finally
            {
              
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return email;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}