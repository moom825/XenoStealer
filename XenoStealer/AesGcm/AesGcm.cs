using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class AesGcm
    {
        private static uint SUCCESS = 0x00000000;
        
        private static uint BCRYPT_KEY_DATA_BLOB_MAGIC = 0x4d42444b;
        
        private static string BCRYPT_OBJECT_LENGTH = "ObjectLength";
        private static string BCRYPT_CHAIN_MODE_GCM = "ChainingModeGCM";
        private static string BCRYPT_AUTH_TAG_LENGTH = "AuthTagLength";
        private static string BCRYPT_CHAINING_MODE = "ChainingMode";
        private static string BCRYPT_AES_ALGORITHM = "AES";
        
        private static string MS_PRIMITIVE_PROVIDER = "Microsoft Primitive Provider";

        public static byte[] Decrypt(byte[] key, byte[] iv, byte[] aad, byte[] cipherText, byte[] authTag)
        {
            if (!OpenAlgorithmProvider(BCRYPT_AES_ALGORITHM, MS_PRIMITIVE_PROVIDER, BCRYPT_CHAIN_MODE_GCM, out IntPtr hAlg)) 
            {
                return null;
            }

            if (!ImportKey(hAlg, key, out IntPtr hKey, out IntPtr keyDataBuffer)) 
            {
                BCryptNativeMethods.BCryptCloseAlgorithmProvider(hAlg, 0x0);
                return null;
            }

            if (!GetMaxAuthTagSize(hAlg, out uint MaxAuthTagSize))
            {
                BCryptNativeMethods.BCryptDestroyKey(hKey);
                Marshal.FreeHGlobal(keyDataBuffer);
                BCryptNativeMethods.BCryptCloseAlgorithmProvider(hAlg, 0x0);
                return null;
            }

            BCryptInternalStructs.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO authInfo = new BCryptInternalStructs.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO(iv, aad, authTag);
            byte[] ivData = new byte[MaxAuthTagSize];
            int plainTextSize = 0;
            
            uint status = BCryptNativeMethods.BCryptDecrypt(hKey, cipherText, (uint)cipherText.Length, ref authInfo, ivData, (uint)ivData.Length, null, 0, ref plainTextSize, 0x0);

            if (status != SUCCESS) 
            {
                BCryptNativeMethods.BCryptDestroyKey(hKey);
                Marshal.FreeHGlobal(keyDataBuffer);
                BCryptNativeMethods.BCryptCloseAlgorithmProvider(hAlg, 0x0);
                authInfo.Dispose();
                return null;
            }

            byte[] plainText = new byte[plainTextSize];

            status = BCryptNativeMethods.BCryptDecrypt(hKey, cipherText, (uint)cipherText.Length, ref authInfo, ivData, (uint)ivData.Length, plainText, (uint)plainText.Length, ref plainTextSize, 0x0);

            BCryptNativeMethods.BCryptDestroyKey(hKey);
            Marshal.FreeHGlobal(keyDataBuffer);
            BCryptNativeMethods.BCryptCloseAlgorithmProvider(hAlg, 0x0);
            authInfo.Dispose();

            if (status != SUCCESS) 
            {
                return null;
            }

            return plainText;
        }

        private static bool GetMaxAuthTagSize(IntPtr hAlg, out uint MaxAuthTagSize)
        {
            if (GetProperty(hAlg, BCRYPT_AUTH_TAG_LENGTH, out BCryptInternalStructs.BCRYPT_KEY_LENGTHS_STRUCT tagLengthsValue)) 
            {
                MaxAuthTagSize = tagLengthsValue.dwMaxLength;
                return true;
            }
            MaxAuthTagSize = 0;
            return false;
        }

        private static bool OpenAlgorithmProvider(string alg, string provider, string chainingMode, out IntPtr hAlg)
        {
            uint status = BCryptNativeMethods.BCryptOpenAlgorithmProvider(out hAlg, alg, provider, 0x0);

            if (status != SUCCESS) 
            {
                return false;
            }
               

            byte[] chainMode = Encoding.Unicode.GetBytes(chainingMode);
            status = BCryptNativeMethods.BCryptSetProperty(hAlg, BCRYPT_CHAINING_MODE, chainMode, (uint)chainMode.Length, 0x0);

            if (status != SUCCESS) 
            {
                BCryptNativeMethods.BCryptCloseAlgorithmProvider(hAlg, 0x0);
                return false;
            }
                

            return true;
        }

        private static bool ImportKey(IntPtr hAlg, byte[] key, out IntPtr hKey, out IntPtr keyDataBuffer)
        {
            hKey= IntPtr.Zero;
            keyDataBuffer= IntPtr.Zero;
            if (!GetProperty(hAlg, BCRYPT_OBJECT_LENGTH, out InternalStructs.UINTRESULT outData)) 
            {
                return false;
            }

            uint keyDataSize = outData.Value;

            keyDataBuffer = Marshal.AllocHGlobal((int)keyDataSize);


            BCryptInternalStructs.BCRYPT_KEY_DATA_BLOB_HEADER KeyDataBlobHeader = new BCryptInternalStructs.BCRYPT_KEY_DATA_BLOB_HEADER();
            KeyDataBlobHeader.dwMagic = BCRYPT_KEY_DATA_BLOB_MAGIC;
            KeyDataBlobHeader.dwVersion = 0x1;
            KeyDataBlobHeader.cbKeyData = (uint)key.Length;


            uint KeyBlobSize = (uint)(Marshal.SizeOf(KeyDataBlobHeader) + key.Length);
            IntPtr KeyBlob = Marshal.AllocHGlobal((int)KeyBlobSize);

            Marshal.StructureToPtr(KeyDataBlobHeader, KeyBlob, false);
            Marshal.Copy(key, 0, KeyBlob + Marshal.SizeOf(KeyDataBlobHeader), key.Length);

            uint status = BCryptNativeMethods.BCryptImportKey_KeyDataBlob(hAlg, IntPtr.Zero, out hKey, keyDataBuffer, keyDataSize, KeyBlob, KeyBlobSize);

            Marshal.FreeHGlobal(KeyBlob);

            if (status != SUCCESS) 
            {
                Marshal.FreeHGlobal(keyDataBuffer);
                return false;
            }

            return true;
        }

        private static bool GetProperty<T>(IntPtr hAlg, string name, out T outData)
        {
            outData = default(T);
            uint size = 0;
            
            uint pOutDataLength = (uint)Marshal.SizeOf<T>();
            IntPtr pOutData=Marshal.AllocHGlobal((int)pOutDataLength);

            uint status = BCryptNativeMethods.BCryptGetProperty(hAlg, name, pOutData, pOutDataLength, ref size, 0x0);

            if (status != SUCCESS) 
            {
                Marshal.FreeHGlobal(pOutData);
                return false;
            }

            outData=Marshal.PtrToStructure<T>(pOutData);
            Marshal.FreeHGlobal(pOutData);
            return true;
        }
    }
}
