using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static XenoStealer.InternalStructs;

namespace XenoStealer
{
    public class InternalStructs32
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct LIST_ENTRY32
        {
            public uint Flink;
            public uint Blink;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PEB_LDR_DATA32
        {
            public uint Length;
            public bool Initialized;
            public uint SsHandle;
            public LIST_ENTRY32 InLoadOrderModuleList;
            public LIST_ENTRY32 InMemoryOrderModuleList;
            public LIST_ENTRY32 InInitializationOrderModuleList;
            public uint EntryInProgress;
            public bool ShutdownInProgress;
            public uint ShutdownThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING32
        {
            public ushort Length;
            public ushort MaximumLength;
            public uint Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LDR_DATA_TABLE_ENTRY32_SNAP
        {
            public LIST_ENTRY32 InLoadOrderLinks;
            public LIST_ENTRY32 InMemoryOrderLinks;
            public LIST_ENTRY32 InInitializationOrderLinks;
            public uint DllBase;
            public uint EntryPoint;
            public uint SizeOfImage;
            public UNICODE_STRING32 FullDllName;
            public UNICODE_STRING32 BaseDllName;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;

            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_NT_HEADERS32
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }


        public static IntPtr GetLdr32(IntPtr addr)
        {
            return addr + 0xc;
        }


    }
}
