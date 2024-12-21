#nullable enable

using System.Runtime.InteropServices;

namespace Mastersign.WinJockey;

public static class WindowsSession
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool LockWorkStation();

    public static void Lock()
    {
        if (!LockWorkStation())
        {
            throw new Exception("LockWorkstation failed",
                Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
        }
    }

    public static void RegisterSessionLockHandler(Action? lockHandler = null, Action? unlockHandler = null)
    {
        Microsoft.Win32.SystemEvents.SessionSwitch += (sender, ea) => {
            switch (ea.Reason)
            {
                case Microsoft.Win32.SessionSwitchReason.SessionLock:
                    lockHandler?.Invoke();
                    break;
                case Microsoft.Win32.SessionSwitchReason.SessionUnlock:
                    unlockHandler?.Invoke();
                    break;
            }
        };
    }
}
