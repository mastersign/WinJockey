﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Mastersign.WinJockey
{
    public static class WindowsPower
    {
        [DllImport("advapi32.dll")]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            uint BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InitiateSystemShutdownEx(
            string lpMachineName,
            string lpMessage,
            int dwTimeout,
            bool bForceAppsClosed,
            bool bRebootAfterShutdown,
            uint dwReason);

        [DllImport("Powrprof.dll", CharSet = CharSet.Auto)]
        private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;

            public uint HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;

            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [Flags]
        public enum ShutdownReason : uint
        {
            // Microsoft major reasons.
            SHTDN_REASON_MAJOR_OTHER = 0x00000000,
            SHTDN_REASON_MAJOR_NONE = 0x00000000,
            SHTDN_REASON_MAJOR_HARDWARE = 0x00010000,
            SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000,
            SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000,
            SHTDN_REASON_MAJOR_APPLICATION = 0x00040000,
            SHTDN_REASON_MAJOR_SYSTEM = 0x00050000,
            SHTDN_REASON_MAJOR_POWER = 0x00060000,
            SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000,

            // Microsoft minor reasons.
            SHTDN_REASON_MINOR_OTHER = 0x00000000,
            SHTDN_REASON_MINOR_NONE = 0x000000ff,
            SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001,
            SHTDN_REASON_MINOR_INSTALLATION = 0x00000002,
            SHTDN_REASON_MINOR_UPGRADE = 0x00000003,
            SHTDN_REASON_MINOR_RECONFIG = 0x00000004,
            SHTDN_REASON_MINOR_HUNG = 0x00000005,
            SHTDN_REASON_MINOR_UNSTABLE = 0x00000006,
            SHTDN_REASON_MINOR_DISK = 0x00000007,
            SHTDN_REASON_MINOR_PROCESSOR = 0x00000008,
            SHTDN_REASON_MINOR_NETWORKCARD = 0x00000000,
            SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a,
            SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b,
            SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c,
            SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d,
            SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e,
            SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F,
            SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010,
            SHTDN_REASON_MINOR_HOTFIX = 0x00000011,
            SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012,
            SHTDN_REASON_MINOR_SECURITY = 0x00000013,
            SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014,
            SHTDN_REASON_MINOR_WMI = 0x00000015,
            SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016,
            SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017,
            SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018,
            SHTDN_REASON_MINOR_MMC = 0x00000019,
            SHTDN_REASON_MINOR_TERMSRV = 0x00000020,

            // Flags that end up in the event log code.
            SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000,
            SHTDN_REASON_FLAG_PLANNED = 0x80000000,
            SHTDN_REASON_UNKNOWN = SHTDN_REASON_MINOR_NONE,
            SHTDN_REASON_LEGACY_API = (SHTDN_REASON_MAJOR_LEGACY_API | SHTDN_REASON_FLAG_PLANNED),

            // This mask cuts out UI flags.
            SHTDN_REASON_VALID_BIT_MASK = 0xc0ffffff
        }

        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        private static void EnableDisablePrivilege(string privilegeName, bool enable)
        {
            if (!LookupPrivilegeValue(null, privilegeName, out var luid))
            {
                throw new Exception("LookupPrivilegeValue failed",
                    Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
            }

            var processHandle = Process.GetCurrentProcess().SafeHandle;
            if (!OpenProcessToken(processHandle.DangerousGetHandle(), (uint)TokenAccessLevels.AdjustPrivileges, out var tokenHandle))
            {
                throw new Exception("OpenThreadToken failed",
                    Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
            }

            var tokenPrivileges = new TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Privileges = new[] 
                {
                    new LUID_AND_ATTRIBUTES
                    {
                        Luid = luid,
                        Attributes = (uint)(enable ? 2 : 0)
                    }
                },
            };

            if (!AdjustTokenPrivileges(tokenHandle, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero))
            {
                throw new Exception("AdjustTokenPrivileges failed",
                    Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
            }
        }

        public static bool InitiateShutdown(string message = null, int timeoutSeconds = 0, bool forceAppClose = false, bool reboot = false)
        {
            EnableDisablePrivilege(SE_SHUTDOWN_NAME, true);
            var reason = ShutdownReason.SHTDN_REASON_MAJOR_APPLICATION | ShutdownReason.SHTDN_REASON_MINOR_NONE;
            return InitiateSystemShutdownEx(null, message, timeoutSeconds, forceAppClose, reboot, (uint)reason);
        }

        public static bool Standby()
        {
            return SetSuspendState(false, true, true);
        }

        public static bool Hibernate()
        {
            return SetSuspendState(true, true, true);
        }
    }
}
