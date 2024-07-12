using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XenoStealer
{
    public class ASN1DER
    {
        public enum ASN1DERTypeEnum
        {
            None = 0,
            Sequence = 0x30,
            Integer = 0x02,
            OctetString = 0x04,
            ObjectIdentifier = 0x06
        }

        public struct ASN1DERObject 
        {
            public ASN1DERTypeEnum type;
            public int length;
            public List<ASN1DERObject> objects;
            public byte[] data;

            public ASN1DERObject(ASN1DERTypeEnum _type, int _length, byte[] _data) 
            {
                type = _type;
                length = _length;
                data = _data;
                objects = new List<ASN1DERObject>();
            }
        }


        public static ASN1DERObject Parse(byte[] ASN1DERData) 
        {
            ASN1DERObject parsedData = new ASN1DERObject(ASN1DERTypeEnum.None, 0, null);

            for (int i = 0; i < (ASN1DERData.Length-1); i++) 
            {
                byte[] data;
                int dataLen = ASN1DERData[i + 1];
                ASN1DERTypeEnum type = (ASN1DERTypeEnum)ASN1DERData[i];
                if (type == ASN1DERTypeEnum.Sequence)
                {
                    if (parsedData.length == 0)
                    {
                        parsedData.type = ASN1DERTypeEnum.Sequence;
                        parsedData.length = ASN1DERData.Length;
                        data = new byte[ASN1DERData.Length];
                    }
                    else 
                    {
                        parsedData.objects.Add(new ASN1DERObject(ASN1DERTypeEnum.Sequence, dataLen, null));
                        data = new byte[dataLen];
                    }

                    int len = ASN1DERData.Length - (i + 2);
                    if (data.Length < len) 
                    {
                        len = data.Length;
                    }

                    Array.Copy(ASN1DERData, i + 2, data, 0, len);
                    parsedData.objects.Add(Parse(data));
                    i += dataLen + 1;
                }
                else if (type == ASN1DERTypeEnum.Integer || type == ASN1DERTypeEnum.OctetString || type == ASN1DERTypeEnum.ObjectIdentifier)
                {
                    data = new byte[dataLen];

                    int len = dataLen;
                    if ((dataLen + (i + 2)) > ASN1DERData.Length) 
                    {
                        len = ASN1DERData.Length - (i + 2);
                    }
                    Array.Copy(ASN1DERData, i + 2, data, 0, len);

                    parsedData.objects.Add(new ASN1DERObject(type, dataLen, data));
                    i+= dataLen + 1;

                }

            }
            return parsedData;

        }


    }
}
