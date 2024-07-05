using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class InternalStructs
    {
        public enum PROCESSINFOCLASS
        {
            ProcessBasicInformation, // q: PROCESS_BASIC_INFORMATION, PROCESS_EXTENDED_BASIC_INFORMATION
            ProcessQuotaLimits, // qs: QUOTA_LIMITS, QUOTA_LIMITS_EX
            ProcessIoCounters, // q: IO_COUNTERS
            ProcessVmCounters, // q: VM_COUNTERS, VM_COUNTERS_EX, VM_COUNTERS_EX2
            ProcessTimes, // q: KERNEL_USER_TIMES
            ProcessBasePriority, // s: KPRIORITY
            ProcessRaisePriority, // s: ULONG
            ProcessDebugPort, // q: HANDLE
            ProcessExceptionPort, // s: PROCESS_EXCEPTION_PORT
            ProcessAccessToken, // s: PROCESS_ACCESS_TOKEN
            ProcessLdtInformation, // qs: PROCESS_LDT_INFORMATION // 10
            ProcessLdtSize, // s: PROCESS_LDT_SIZE
            ProcessDefaultHardErrorMode, // qs: ULONG
            ProcessIoPortHandlers, // (kernel-mode only)
            ProcessPooledUsageAndLimits, // q: POOLED_USAGE_AND_LIMITS
            ProcessWorkingSetWatch, // q: PROCESS_WS_WATCH_INFORMATION[]; s: void
            ProcessUserModeIOPL,
            ProcessEnableAlignmentFaultFixup, // s: BOOLEAN
            ProcessPriorityClass, // qs: PROCESS_PRIORITY_CLASS
            ProcessWx86Information,
            ProcessHandleCount, // q: ULONG, PROCESS_HANDLE_INFORMATION // 20
            ProcessAffinityMask, // s: KAFFINITY
            ProcessPriorityBoost, // qs: ULONG
            ProcessDeviceMap, // qs: PROCESS_DEVICEMAP_INFORMATION, PROCESS_DEVICEMAP_INFORMATION_EX
            ProcessSessionInformation, // q: PROCESS_SESSION_INFORMATION
            ProcessForegroundInformation, // s: PROCESS_FOREGROUND_BACKGROUND
            ProcessWow64Information, // q: ULONG_PTR
            ProcessImageFileName, // q: UNICODE_STRING
            ProcessLUIDDeviceMapsEnabled, // q: ULONG
            ProcessBreakOnTermination, // qs: ULONG
            ProcessDebugObjectHandle, // q: HANDLE // 30
            ProcessDebugFlags, // qs: ULONG
            ProcessHandleTracing, // q: PROCESS_HANDLE_TRACING_QUERY; s: size 0 disables, otherwise enables
            ProcessIoPriority, // qs: IO_PRIORITY_HINT
            ProcessExecuteFlags, // qs: ULONG
            ProcessResourceManagement, // ProcessTlsInformation // PROCESS_TLS_INFORMATION
            ProcessCookie, // q: ULONG
            ProcessImageInformation, // q: SECTION_IMAGE_INFORMATION
            ProcessCycleTime, // q: PROCESS_CYCLE_TIME_INFORMATION // since VISTA
            ProcessPagePriority, // q: PAGE_PRIORITY_INFORMATION
            ProcessInstrumentationCallback, // qs: PROCESS_INSTRUMENTATION_CALLBACK_INFORMATION // 40
            ProcessThreadStackAllocation, // s: PROCESS_STACK_ALLOCATION_INFORMATION, PROCESS_STACK_ALLOCATION_INFORMATION_EX
            ProcessWorkingSetWatchEx, // q: PROCESS_WS_WATCH_INFORMATION_EX[]
            ProcessImageFileNameWin32, // q: UNICODE_STRING
            ProcessImageFileMapping, // q: HANDLE (input)
            ProcessAffinityUpdateMode, // qs: PROCESS_AFFINITY_UPDATE_MODE
            ProcessMemoryAllocationMode, // qs: PROCESS_MEMORY_ALLOCATION_MODE
            ProcessGroupInformation, // q: USHORT[]
            ProcessTokenVirtualizationEnabled, // s: ULONG
            ProcessConsoleHostProcess, // q: ULONG_PTR // ProcessOwnerInformation
            ProcessWindowInformation, // q: PROCESS_WINDOW_INFORMATION // 50
            ProcessHandleInformation, // q: PROCESS_HANDLE_SNAPSHOT_INFORMATION // since WIN8
            ProcessMitigationPolicy, // s: PROCESS_MITIGATION_POLICY_INFORMATION
            ProcessDynamicFunctionTableInformation,
            ProcessHandleCheckingMode, // qs: ULONG; s: 0 disables, otherwise enables
            ProcessKeepAliveCount, // q: PROCESS_KEEPALIVE_COUNT_INFORMATION
            ProcessRevokeFileHandles, // s: PROCESS_REVOKE_FILE_HANDLES_INFORMATION
            ProcessWorkingSetControl, // s: PROCESS_WORKING_SET_CONTROL
            ProcessHandleTable, // q: ULONG[] // since WINBLUE
            ProcessCheckStackExtentsMode,
            ProcessCommandLineInformation, // q: UNICODE_STRING // 60
            ProcessProtectionInformation, // q: PS_PROTECTION
            ProcessMemoryExhaustion, // PROCESS_MEMORY_EXHAUSTION_INFO // since THRESHOLD
            ProcessFaultInformation, // PROCESS_FAULT_INFORMATION
            ProcessTelemetryIdInformation, // PROCESS_TELEMETRY_ID_INFORMATION
            ProcessCommitReleaseInformation, // PROCESS_COMMIT_RELEASE_INFORMATION
            ProcessDefaultCpuSetsInformation,
            ProcessAllowedCpuSetsInformation,
            ProcessSubsystemProcess,
            ProcessJobMemoryInformation, // PROCESS_JOB_MEMORY_INFO
            ProcessInPrivate, // since THRESHOLD2 // 70
            ProcessRaiseUMExceptionOnInvalidHandleClose, // qs: ULONG; s: 0 disables, otherwise enables
            ProcessIumChallengeResponse,
            ProcessChildProcessInformation, // PROCESS_CHILD_PROCESS_INFORMATION
            ProcessHighGraphicsPriorityInformation,
            ProcessSubsystemInformation, // q: SUBSYSTEM_INFORMATION_TYPE // since REDSTONE2
            ProcessEnergyValues, // PROCESS_ENERGY_VALUES, PROCESS_EXTENDED_ENERGY_VALUES
            ProcessActivityThrottleState, // PROCESS_ACTIVITY_THROTTLE_STATE
            ProcessActivityThrottlePolicy, // PROCESS_ACTIVITY_THROTTLE_POLICY
            ProcessWin32kSyscallFilterInformation,
            ProcessDisableSystemAllowedCpuSets, // 80
            ProcessWakeInformation, // PROCESS_WAKE_INFORMATION
            ProcessEnergyTrackingState, // PROCESS_ENERGY_TRACKING_STATE
            ProcessManageWritesToExecutableMemory, // MANAGE_WRITES_TO_EXECUTABLE_MEMORY // since REDSTONE3
            ProcessCaptureTrustletLiveDump,
            ProcessTelemetryCoverage,
            ProcessEnclaveInformation,
            ProcessEnableReadWriteVmLogging, // PROCESS_READWRITEVM_LOGGING_INFORMATION
            ProcessUptimeInformation, // PROCESS_UPTIME_INFORMATION
            ProcessImageSection, // q: HANDLE
            ProcessDebugAuthInformation, // since REDSTONE4 // 90
            ProcessSystemResourceManagement, // PROCESS_SYSTEM_RESOURCE_MANAGEMENT
            ProcessSequenceNumber, // q: ULONGLONG
            ProcessLoaderDetour, // since REDSTONE5
            ProcessSecurityDomainInformation, // PROCESS_SECURITY_DOMAIN_INFORMATION
            ProcessCombineSecurityDomainsInformation, // PROCESS_COMBINE_SECURITY_DOMAINS_INFORMATION
            ProcessEnableLogging, // PROCESS_LOGGING_INFORMATION
            ProcessLeapSecondInformation, // PROCESS_LEAP_SECOND_INFORMATION
            ProcessFiberShadowStackAllocation, // PROCESS_FIBER_SHADOW_STACK_ALLOCATION_INFORMATION // since 19H1
            ProcessFreeFiberShadowStackAllocation, // PROCESS_FREE_FIBER_SHADOW_STACK_ALLOCATION_INFORMATION
            MaxProcessInfoClass
        };


        public struct UINTRESULT
        {
            public uint Value;
        }
        public struct USHORTRESULT
        {
            public ushort Value;
        }

        public struct ULONGRESULT
        {
            public ulong Value;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public uint VirtualAddress;
            public uint Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DOS_HEADER
        {
            public ushort e_magic;
            public ushort e_cblp;
            public ushort e_cp;
            public ushort e_crlc;
            public ushort e_cparhdr;
            public ushort e_minalloc;
            public ushort e_maxalloc;
            public ushort e_ss;
            public ushort e_sp;
            public ushort e_csum;
            public ushort e_ip;
            public ushort e_cs;
            public ushort e_lfarlc;
            public ushort e_ovno;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] e_res;
            public ushort e_oemid;
            public ushort e_oeminfo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public ushort[] e_res2;
            public int e_lfanew;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_EXPORT_DIRECTORY
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Name;
            public uint Base;
            public uint NumberOfFunctions;
            public uint NumberOfNames;
            public uint AddressOfFunctions;     // RVA from base of image
            public uint AddressOfNames;         // RVA from base of image
            public uint AddressOfNameOrdinals;  // RVA from base of image
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public int ExitStatus;
            public IntPtr PebBaseAddress;
            public UIntPtr AffinityMask;
            public uint BasePriority;
            public UIntPtr UniqueProcessId;
            public UIntPtr InheritedFromUniqueProcessId;
        }

        public enum FileType
        {
            FILE_TYPE_UNKNOWN = 0x0000, // The specified file type is unknown.
            FILE_TYPE_DISK = 0x0001, // The specified file is a disk file.
            FILE_TYPE_CHAR = 0x0002, // The specified file is a character file, typically an LPT device or a console.
            FILE_TYPE_PIPE = 0x0003, // The specified file is a socket, a named pipe, or an anonymous pipe.
            FILE_TYPE_REMOTE = 0x8000, // Unused.
        }

    }
}
