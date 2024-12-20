using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using WindowsDesktop;

namespace Mastersign.WinMan;

internal class WindowManagement
{
    public static void SwitchToVirtualDesktop(int no)
    {
        var desktops = VirtualDesktop.GetDesktops();
        if (no < 0 || no >= desktops.Length) return;
        desktops[no].Switch();
    }

    public static void PinCurrentWindow()
    {
        if (!VirtualDesktop.IsSupported) return;
        var hWnd = WinApi.GetForegroundWindow();
        if (!WinApi.IsWindow(hWnd)) return;
        var wrapper = WindowWrapper.ForHandle(hWnd);
        if (!wrapper.IsStandardWindow) return;
        if (VirtualDesktop.IsPinnedWindow(hWnd)) return;
        VirtualDesktop.PinWindow(hWnd);
    }

    public static void UnpinCurrentWindow()
    {
        if (!VirtualDesktop.IsSupported) return;
        var hWnd = WinApi.GetForegroundWindow();
        if (!WinApi.IsWindow(hWnd)) return;
        var wrapper = WindowWrapper.ForHandle(hWnd);
        if (!wrapper.IsStandardWindow) return;
        if (!VirtualDesktop.IsPinnedWindow(hWnd)) return;
        VirtualDesktop.UnpinWindow(hWnd);
    }

    public static void MoveCurrentWindowToVirtualDesktop(int no)
    {
        if (!VirtualDesktop.IsSupported) return;
        var hWnd = WinApi.GetForegroundWindow();
        if (!WinApi.IsWindow(hWnd)) return;
        var wrapper = WindowWrapper.ForHandle(hWnd);
        if (!wrapper.IsStandardWindow) return;
        var desktops = VirtualDesktop.GetDesktops();
        if (no < 0 || no >= desktops.Length) return;
        desktops[no].MoveWindowHere(hWnd);
    }

    public static void MaximizeWindow()
    {
        var wrapper = WindowWrapper.ForHandle(WinApi.GetForegroundWindow(), clearCache: true);
        if (!wrapper.IsStandardWindow) return;
        var r = wrapper.NormalPosition;
        wrapper.SetPlacement(r, ShowWindowCommands.Maximize);
    }

    public static void NormalWindow()
    {
        var wrapper = WindowWrapper.ForHandle(WinApi.GetForegroundWindow(), clearCache: true);
        if (!wrapper.IsStandardWindow) return;
        var r = wrapper.NormalPosition;
        wrapper.SetPlacement(r, ShowWindowCommands.Normal);
    }

    public static void MinimizeWindow()
    {
        var wrapper = WindowWrapper.ForHandle(WinApi.GetForegroundWindow(), clearCache: true);
        if (!wrapper.IsStandardWindow) return;
        wrapper.ShowCommand = ShowWindowCommands.Minimize;
    }

    public static void MoveCurrentWindowToScreen(int no)
    {
        var wrapper = WindowWrapper.ForHandle(WinApi.GetForegroundWindow(), clearCache: true);
        if (!wrapper.IsStandardWindow) return;

        var currentShowCmd = wrapper.ShowCommand;
        if (currentShowCmd != ShowWindowCommands.Normal)
        {
            wrapper.ShowCommand = ShowWindowCommands.Restore;
        }

        var screens = Screen.AllScreens.OrderBy(s => (s.Bounds.X, s.Bounds.Y)).ToList();
        if (no < 0 || no >= screens.Count) return;
        var s2 = screens[no];
        var s2B = s2.WorkingArea;
        var s = wrapper.Screen;
        var sB = s.WorkingArea;
        var r = wrapper.NormalPosition;
        var relL = (double)(r.X - sB.X) / sB.Width;
        var relT = (double)(r.Y - sB.Y) / sB.Height;
        var relW = (double)r.Width / sB.Width;
        var relH = (double)r.Height / sB.Height;
        var left = (int)Math.Round(relL * s2B.Width) + s2B.X;
        var top = (int)Math.Round(relT * s2B.Height) + s2B.Y;
        var width = (int)Math.Round(relW * s2B.Width);
        var height = (int)Math.Round(relH * s2B.Height);
        var r2 = new RECT(left, top, left + width, top + height);

        wrapper.SetPlacement(r2, currentShowCmd);
    }

    public static void CloseWindow()
    {
        var wrapper = WindowWrapper.ForHandle(WinApi.GetForegroundWindow(), clearCache: true);
        if (!wrapper.IsStandardWindow) return;
        wrapper.Close();
    }

    public static void MoveCurrentWindow(double left, double top, double width, double height)
    {
        var wrapper = WindowWrapper.ForHandle(WinApi.GetForegroundWindow(), clearCache: true);
        if (!wrapper.IsStandardWindow) return;

        left = Math.Max(0, Math.Min(1, left));
        top = Math.Max(0, Math.Min(1, top));
        width = Math.Max(0.001, Math.Min(1 - left, width));
        height = Math.Max(0.001, Math.Min(1 - top, height));
        var s = wrapper.Screen;
        var sB = s.WorkingArea;
        var r = new RECT(
            sB.Left + (int)Math.Round(left * sB.Width),
            sB.Top + (int)Math.Round(top * sB.Height),
            sB.Left + (int)Math.Round((left + width) * sB.Width),
            sB.Top + (int)Math.Round((top + height) * sB.Height));
        wrapper.MoveTo(r);
    }
}

internal class WindowWrapper
{
    private static readonly Dictionary<IntPtr, WindowWrapper> _instances = new Dictionary<IntPtr, WindowWrapper>();

    public static WindowWrapper ForHandle(IntPtr hWnd) => ForHandle(hWnd, clearCache: false);

    public static WindowWrapper ForHandle(IntPtr hWnd, bool clearCache)
    {
        if (!_instances.TryGetValue(hWnd, out WindowWrapper wrapper))
        {
            wrapper = new WindowWrapper(hWnd);
            _instances[hWnd] = wrapper;
        }
        else if (clearCache)
        {
            wrapper.ClearCache();
        }
        return wrapper;
    }

    public static WindowWrapper Find(string windowClass, string windowName)
    {
        var hWnd = WinApi.FindWindow(windowClass, windowName);
        return hWnd != IntPtr.Zero ? ForHandle(hWnd, clearCache: false) : null;
    }

    public static void ClearCaches()
    {
        var handles = _instances.Keys.ToArray();
        foreach (var hWnd in handles)
        {
            if (!WinApi.IsWindow(hWnd))
            {
                _instances.Remove(hWnd);
            }
            else
            {
                _instances[hWnd].ClearCache();
            }
        }
    }

    public static WindowWrapper[] AllWindows()
    {
        List<IntPtr> result = new List<IntPtr>();
        WinApi.EnumWindows((hWnd, lParam) =>
        {
            if (WinApi.IsWindowVisible(hWnd) &&
                (VirtualDesktop.FromHwnd(hWnd) != null ||
                 VirtualDesktop.IsPinnedWindow(hWnd) ||
                 VirtualDesktop.IsPinnedApplication(hWnd.GetAppId())))
            {
                result.Add(hWnd);
            }
            return true;
        }, IntPtr.Zero);
        return result.Select(ForHandle).ToArray();
    }

    public IntPtr Handle { get; private set; }
    private WindowWrapper(IntPtr hWnd)
    {
        Handle = hWnd;
        ClearCache();
    }

    public void ClearCache()
    {
        _titleLoaded = false;
        _windowClassLoaded = false;
        _placementLoaded = false;
        _virtualDesktopLoaded = false;
        _screenLoaded = false;
        _processLoaded = false;
    }

    public bool IsDesktop => WinApi.GetDesktopWindow() == Handle;

    public bool IsShell => WinApi.GetShellWindow() == Handle;

    public bool IsValid => WinApi.IsWindow(Handle);

    public bool IsVisible => WinApi.IsWindowVisible(Handle);

    private static readonly string[] _systemWindowClassNames = new[]
    {
        "SysListView32", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "Progman"
    };

    public bool IsStandardWindow
    {
        get
        {
            // make sure the window is top level
            if (WinApi.GetAncestor(Handle, GetAncestorFlags.GetRoot) != Handle) return false;
            // make sure the window is visible
            if (!IsVisible) return false;

            // make sure the window has a border is no hovering tool window
            var style = WinApi.GetWindowLong(Handle, GetWindowLongIndex.GWL_STYLE);
            if ((style & WindowStyles.WS_POPUP) == WindowStyles.WS_POPUP &&
                (style & WindowStyles.WS_THICKFRAME) == 0 &&
                (style & WindowStyles.WS_MINIMIZEBOX) == 0 &&
                (style & WindowStyles.WS_MAXIMIZEBOX) == 0)
            {
                return false;
            }
            // make sure the window is not a child, disabled, a toolwindow or unable to be activcated
            var exStyle = WinApi.GetWindowLong(Handle, GetWindowLongIndex.GWL_EXSTYLE);
            if ((style & WindowStyles.WS_CHILD) == WindowStyles.WS_CHILD ||
                (style & WindowStyles.WS_DISABLED) == WindowStyles.WS_DISABLED ||
                (exStyle & WindowStyles.WS_EX_TOOLWINDOW) == WindowStyles.WS_EX_TOOLWINDOW ||
                (exStyle & WindowStyles.WS_EX_NOACTIVATE) == WindowStyles.WS_EX_NOACTIVATE)
            {
                return false;
            }

            // make sure the window is no system window
            if (IsDesktop) return false;
            if (IsShell) return false;
            if (_systemWindowClassNames.Contains(WindowClass)) return false;
            if (WindowClass == "Windows.UI.Core.CoreWindow" &&
                ProcessFileName.EndsWith("SearchUI.exe"))
            {
                return false;
            }

            return true;
        }
    }

    private string _title;
    private bool _titleLoaded;

    private string ReadTitle()
    {
        // Get the size of the string required to hold the window title. 
        var size = WinApi.SendMessage((int)Handle, WinApi.WM_GETTEXTLENGTH, 0, 0).ToInt32();

        // If the return is 0, there is no title. 
        if (size > 0)
        {
            var title = new StringBuilder(size + 1);
            WinApi.SendMessage(Handle, WinApi.WM_GETTEXT, title.Capacity, title);
            return title.ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    public string Title
    {
        get
        {
            if (!_titleLoaded)
            {
                _title = ReadTitle();
                _titleLoaded = true;
            }
            return _title;
        }
    }

    private string _windowClass;
    private bool _windowClassLoaded;

    private string ReadWindowClass()
    {
        var sb = new StringBuilder(256);
        WinApi.GetClassName(Handle, sb, sb.Capacity);
        return sb.ToString();
    }

    public string WindowClass
    {
        get
        {
            if (!_windowClassLoaded)
            {
                _windowClass = ReadWindowClass();
                _windowClassLoaded = true;
            }
            return _windowClass;
        }
    }

    public string AppId => Handle.GetAppId();

    public bool IsModernAppWindow
        => ProcessFileName != null
           && Path.GetFileName(ProcessFileName).ToLower() == "applicationframehost.exe"
           && WindowClass == "ApplicationFrameWindow";

    private WINDOWPLACEMENT _placement = WINDOWPLACEMENT.Default;
    private RECT _extendedFrameBounds;
    private bool _placementLoaded;

    private void ReadPlacement()
    {
        WinApi.GetWindowPlacement(Handle, ref _placement);
        WinApi.DwmGetWindowAttribute(Handle, DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out _extendedFrameBounds, Marshal.SizeOf(typeof(RECT)));
        _placementLoaded = true;
    }

    private void WritePlacement()
    {
        WinApi.SetWindowPlacement(Handle, ref _placement);
    }

    public RECT NormalPosition
    {
        get
        {
            if (!_placementLoaded) ReadPlacement();
            return _placement.NormalPosition;
        }
        set
        {
            ReadPlacement();
            _placement.NormalPosition = value;
            WritePlacement();
        }
    }

    public POINT MaximizedPosition
    {
        get
        {
            if (!_placementLoaded) ReadPlacement();
            return _placement.MaxPosition;
        }
    }

    public POINT MinimizedPosition
    {
        get
        {
            if (!_placementLoaded) ReadPlacement();
            return _placement.MinPosition;
        }
    }

    public ShowWindowCommands ShowCommand
    {
        get
        {
            if (!_placementLoaded) ReadPlacement();
            return _placement.ShowCmd;
        }
        set
        {
            ReadPlacement();
            _placement.ShowCmd = value;
            WritePlacement();
        }
    }

    public RECT CompensateBorder(RECT windowPosition)
    {
        ReadPlacement();
        return new RECT(
            windowPosition.Left + (_placement.NormalPosition.Left - _extendedFrameBounds.Left),
            windowPosition.Top + (_placement.NormalPosition.Top - _extendedFrameBounds.Top),
            windowPosition.Right + (_placement.NormalPosition.Right - _extendedFrameBounds.Right),
            windowPosition.Bottom + (_placement.NormalPosition.Bottom - _extendedFrameBounds.Bottom));
    }

    public void SetPlacement(RECT normalPosition, ShowWindowCommands showCmd)
    {
        ReadPlacement();
        _placement.NormalPosition = normalPosition;
        _placement.ShowCmd = showCmd;
        _placement.Flags = WindowPlacementFlags.WPF_ASYNCWINDOWPLACEMENT;
        WritePlacement();
    }

    public void MoveTo(RECT position)
    {
        ReadPlacement();
        _placement.ShowCmd = ShowWindowCommands.Restore;
        WritePlacement();
        ReadPlacement();
        position = CompensateBorder(position);
        _placement.NormalPosition = position;
        _placement.ShowCmd = ShowWindowCommands.Normal;
        _placement.Flags = WindowPlacementFlags.WPF_ASYNCWINDOWPLACEMENT;
        WritePlacement();
    }

    private VirtualDesktop _virtualDesktop;
    private bool _virtualDesktopLoaded;

    public VirtualDesktop VirtualDesktop
    {
        get
        {
            if (!_virtualDesktopLoaded)
            {
                _virtualDesktop = VirtualDesktop.FromHwnd(Handle);
                _virtualDesktopLoaded = true;
            }
            return _virtualDesktop;
        }
    }

    private Screen _screen;
    private bool _screenLoaded;

    public Screen Screen
    {
        get
        {
            if (!_screenLoaded)
            {
                _screen = Screen.FromHandle(Handle);
                _screenLoaded = true;
            }
            return _screen;
        }
    }

    private Process _process;
    private bool _processLoaded;

    public Process Process
    {
        get
        {
            if (!_processLoaded)
            {
                WinApi.GetWindowThreadProcessId(Handle, out uint pId);
                _process = Process.GetProcessById((int)pId);
                _processLoaded = true;
            }
            return _process;
        }
    }

    public string ProcessFileName
    {
        get
        {
            try
            {
                return Process?.MainModule?.FileName;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public List<WindowWrapper> GetChildWindows()
    {
        List<IntPtr> result = new List<IntPtr>();
        WinApi.EnumChildWindows(Handle, (hWnd, lParam) =>
        {
            result.Add(hWnd);
            return true;
        }, IntPtr.Zero);
        return result.Select(ForHandle).ToList();
    }

    public void Unpin()
    {
        VirtualDesktop.UnpinWindow(Handle);
        var appId = Handle.GetAppId();
        if (appId != null) VirtualDesktop.UnpinApplication(appId);
    }

    public void Pin()
    {
        VirtualDesktop.PinWindow(Handle);
    }

    public void Close()
    {
        WinApi.PostMessage(Handle, WinApi.WM_CLOSE, 0, 0);
    }

    public override string ToString()
    {
        return $"{Title} [{Handle}] {ShowCommand} ({Path.GetFileName(Process.MainModule.FileName)}) ";
    }
}

internal static class WinApi
{
    public const int WM_GETTEXT = 0x000D;
    public const int WM_GETTEXTLENGTH = 0x000E;
    public const uint WM_CLOSE = 0x0010;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam, int lparam);

    [DllImport("user32.Dll")]
    public static extern int PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = false)]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", SetLastError = false)]
    public static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = false)]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowLong(IntPtr hWnd, GetWindowLongIndex index);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindowStations(EnumWindowStationsDelegate lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr SetCapture(IntPtr hWnd);

    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out RECT pvAttribute, int cbAttribute);

    [DllImport("coredll.dll", SetLastError = true)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private delegate bool EnumWindowStationsDelegate(string windowsStation, IntPtr lParam);
}

[Serializable]
internal struct POINT
{
    public int x;
    public int y;
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int Left, Top, Right, Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
        get { return Left; }
        set { Right -= (Left - value); Left = value; }
    }

    public int Y
    {
        get { return Top; }
        set { Bottom -= (Top - value); Top = value; }
    }

    public int Height
    {
        get { return Bottom - Top; }
        set { Bottom = value + Top; }
    }

    public int Width
    {
        get { return Right - Left; }
        set { Right = value + Left; }
    }

    public System.Drawing.Point Location
    {
        get { return new System.Drawing.Point(Left, Top); }
        set { X = value.X; Y = value.Y; }
    }

    public System.Drawing.Size Size
    {
        get { return new System.Drawing.Size(Width, Height); }
        set { Width = value.Width; Height = value.Height; }
    }

    public static implicit operator System.Drawing.Rectangle(RECT r)
    {
        return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
    }

    public static implicit operator RECT(System.Drawing.Rectangle r)
    {
        return new RECT(r);
    }

    public static bool operator ==(RECT r1, RECT r2)
    {
        return r1.Equals(r2);
    }

    public static bool operator !=(RECT r1, RECT r2)
    {
        return !r1.Equals(r2);
    }

    public bool Equals(RECT r)
    {
        return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
    }

    public override bool Equals(object obj)
    {
        if (obj is RECT)
            return Equals((RECT)obj);
        else if (obj is System.Drawing.Rectangle)
            return Equals(new RECT((System.Drawing.Rectangle)obj));
        return false;
    }

    public override int GetHashCode()
    {
        return ((System.Drawing.Rectangle)this).GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, "({0}, {1}) - ({2}, {3})", Left, Top, Right, Bottom);
    }
}

internal enum GetWindowCmd : uint
{
    GW_HWNDFIRST = 0,
    GW_HWNDLAST = 1,
    GW_HWNDNEXT = 2,
    GW_HWNDPREV = 3,
    GW_OWNER = 4,
    GW_CHILD = 5,
    GW_ENABLEDPOPUP = 6
}

internal enum GetWindowLongIndex : int
{
    GWL_WNDPROC = -4,
    GWL_HINSTANCE = -6,
    GWL_IDENTIFIER = -12,
    GWL_STYLE = -16,
    GWL_EXSTYLE = -20,
    GWL_USERDATA = -21,
}

internal enum GetAncestorFlags : int
{
    /// <summary>
    /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
    /// </summary>
    GetParent = 1,
    /// <summary>
    /// Retrieves the root window by walking the chain of parent windows.
    /// </summary>
    GetRoot = 2,
    /// <summary>
    /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
    /// </summary>
    GetRootOwner = 3
}

public static class WindowStyles
{
    public const uint WS_OVERLAPPED = 0x00000000;
    public const uint WS_POPUP = 0x80000000;
    public const uint WS_CHILD = 0x40000000;
    public const uint WS_MINIMIZE = 0x20000000;
    public const uint WS_VISIBLE = 0x10000000;
    public const uint WS_DISABLED = 0x08000000;
    public const uint WS_CLIPSIBLINGS = 0x04000000;
    public const uint WS_CLIPCHILDREN = 0x02000000;
    public const uint WS_MAXIMIZE = 0x01000000;
    public const uint WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
    public const uint WS_BORDER = 0x00800000;
    public const uint WS_DLGFRAME = 0x00400000;
    public const uint WS_VSCROLL = 0x00200000;
    public const uint WS_HSCROLL = 0x00100000;
    public const uint WS_SYSMENU = 0x00080000;
    public const uint WS_THICKFRAME = 0x00040000;
    public const uint WS_GROUP = 0x00020000;
    public const uint WS_TABSTOP = 0x00010000;

    public const uint WS_MINIMIZEBOX = 0x00020000;
    public const uint WS_MAXIMIZEBOX = 0x00010000;

    public const uint WS_TILED = WS_OVERLAPPED;
    public const uint WS_ICONIC = WS_MINIMIZE;
    public const uint WS_SIZEBOX = WS_THICKFRAME;
    public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

    // Common Window Styles

    public const uint WS_OVERLAPPEDWINDOW =
        (WS_OVERLAPPED |
          WS_CAPTION |
          WS_SYSMENU |
          WS_THICKFRAME |
          WS_MINIMIZEBOX |
          WS_MAXIMIZEBOX);

    public const uint WS_POPUPWINDOW =
        (WS_POPUP |
          WS_BORDER |
          WS_SYSMENU);

    public const uint WS_CHILDWINDOW = WS_CHILD;

    //Extended Window Styles

    public const uint WS_EX_DLGMODALFRAME = 0x00000001;
    public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
    public const uint WS_EX_TOPMOST = 0x00000008;
    public const uint WS_EX_ACCEPTFILES = 0x00000010;
    public const uint WS_EX_TRANSPARENT = 0x00000020;

    public const uint WS_EX_MDICHILD = 0x00000040;
    public const uint WS_EX_TOOLWINDOW = 0x00000080;
    public const uint WS_EX_WINDOWEDGE = 0x00000100;
    public const uint WS_EX_CLIENTEDGE = 0x00000200;
    public const uint WS_EX_CONTEXTHELP = 0x00000400;

    public const uint WS_EX_RIGHT = 0x00001000;
    public const uint WS_EX_LEFT = 0x00000000;
    public const uint WS_EX_RTLREADING = 0x00002000;
    public const uint WS_EX_LTRREADING = 0x00000000;
    public const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
    public const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;

    public const uint WS_EX_CONTROLPARENT = 0x00010000;
    public const uint WS_EX_STATICEDGE = 0x00020000;
    public const uint WS_EX_APPWINDOW = 0x00040000;

    public const uint WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
    public const uint WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);

    public const uint WS_EX_LAYERED = 0x00080000;

    public const uint WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
    public const uint WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
                                                    //#endif /* WINVER >= 0x0500 */
    public const uint WS_EX_COMPOSITED = 0x02000000;
    public const uint WS_EX_NOACTIVATE = 0x08000000;
}

[Flags]
internal enum WindowPlacementFlags : uint
{
    WPF_NONE = 0x0000,
    WPF_ASYNCWINDOWPLACEMENT = 0x0004,
    WPF_RESTORETOMAXIMIZED = 0x0002,
    WPF_SETMINPOSITION = 0x0001,
}

internal enum ShowWindowCommands
{
    /// <summary>
    /// Hides the window and activates another window.
    /// </summary>
    Hide = 0,
    /// <summary>
    /// Activates and displays a window. If the window is minimized or
    /// maximized, the system restores it to its original size and position.
    /// An application should specify this flag when displaying the window
    /// for the first time.
    /// </summary>
    Normal = 1,
    /// <summary>
    /// Activates the window and displays it as a minimized window.
    /// </summary>
    ShowMinimized = 2,
    /// <summary>
    /// Maximizes the specified window.
    /// </summary>
    Maximize = 3, // is this the right value?
    /// <summary>
    /// Activates the window and displays it as a maximized window.
    /// </summary>      
    ShowMaximized = 3,
    /// <summary>
    /// Displays a window in its most recent size and position. This value
    /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
    /// the window is not activated.
    /// </summary>
    ShowNoActivate = 4,
    /// <summary>
    /// Activates the window and displays it in its current size and position.
    /// </summary>
    Show = 5,
    /// <summary>
    /// Minimizes the specified window and activates the next top-level
    /// window in the Z order.
    /// </summary>
    Minimize = 6,
    /// <summary>
    /// Displays the window as a minimized window. This value is similar to
    /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
    /// window is not activated.
    /// </summary>
    ShowMinNoActive = 7,
    /// <summary>
    /// Displays the window in its current size and position. This value is
    /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
    /// window is not activated.
    /// </summary>
    ShowNA = 8,
    /// <summary>
    /// Activates and displays the window. If the window is minimized or
    /// maximized, the system restores it to its original size and position.
    /// An application should specify this flag when restoring a minimized window.
    /// </summary>
    Restore = 9,
    /// <summary>
    /// Sets the show state based on the SW_* value specified in the
    /// STARTUPINFO structure passed to the CreateProcess function by the
    /// program that started the application.
    /// </summary>
    ShowDefault = 10,
    /// <summary>
    ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
    /// that owns the window is not responding. This flag should only be
    /// used when minimizing windows from a different thread.
    /// </summary>
    ForceMinimize = 11
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
internal struct WINDOWPLACEMENT
{
    /// <summary>
    /// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
    /// <para>
    /// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
    /// </para>
    /// </summary>
    public uint Length;

    /// <summary>
    /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
    /// </summary>
    public WindowPlacementFlags Flags;

    /// <summary>
    /// The current show state of the window.
    /// </summary>
    public ShowWindowCommands ShowCmd;

    /// <summary>
    /// The coordinates of the window's upper-left corner when the window is minimized.
    /// </summary>
    public POINT MinPosition;

    /// <summary>
    /// The coordinates of the window's upper-left corner when the window is maximized.
    /// </summary>
    public POINT MaxPosition;

    /// <summary>
    /// The window's coordinates when the window is in the restored position.
    /// </summary>
    public RECT NormalPosition;

    /// <summary>
    /// Gets the default (empty) value.
    /// </summary>
    public static WINDOWPLACEMENT Default
    {
        get
        {
            WINDOWPLACEMENT result = new WINDOWPLACEMENT();
            result.Length = (uint)Marshal.SizeOf(result);
            return result;
        }
    }
}

[Flags]
public enum DwmWindowAttribute : uint
{
    DWMWA_NCRENDERING_ENABLED = 1,
    DWMWA_NCRENDERING_POLICY,
    DWMWA_TRANSITIONS_FORCEDISABLED,
    DWMWA_ALLOW_NCPAINT,
    DWMWA_CAPTION_BUTTON_BOUNDS,
    DWMWA_NONCLIENT_RTL_LAYOUT,
    DWMWA_FORCE_ICONIC_REPRESENTATION,
    DWMWA_FLIP3D_POLICY,
    DWMWA_EXTENDED_FRAME_BOUNDS,
    DWMWA_HAS_ICONIC_BITMAP,
    DWMWA_DISALLOW_PEEK,
    DWMWA_EXCLUDED_FROM_PEEK,
    DWMWA_CLOAK,
    DWMWA_CLOAKED,
    DWMWA_FREEZE_REPRESENTATION,
    DWMWA_LAST
}
