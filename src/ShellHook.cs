using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SHDocVw;
using Shell32;

namespace Mastersign.WindowsShell
{
    public sealed class ShellHook : IDisposable
    {
        private static class WinApi
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

            [DllImport("user32.dll", SetLastError = false)]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll", SetLastError = false)]
            public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter,
                string className, string windowTitle);

            public static IEnumerable<IntPtr> GetDesktopWindowsInZOrder()
            {
                var desktop = GetDesktopWindow();
                var current = GetWindow(desktop, GetWindowCmd.GW_CHILD);
                while (current != IntPtr.Zero)
                {
                    yield return current;
                    current = GetWindow(current, GetWindowCmd.GW_HWNDNEXT);
                }
            }
        }

        private enum GetWindowCmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService([MarshalAs(UnmanagedType.LPStruct)] Guid service, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        }

        // note: for the following interfaces, not all methods are defined as we don't use them here
        [Guid("000214E2-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellBrowser
        {
            IntPtr GetWindow();
        }

        public long LastWindowHWnd { get; private set; }

        public long LastTabHWnd { get; private set; }

        public string LastLocationUrl { get; private set; }

        public string LastLocationName { get; private set; }

        private readonly List<string> lastSelection = new List<string>();

        public string[] GetLastSelection() => lastSelection.ToArray();
    
    
        private IShellWindows shellWindows;

        public ShellHook()
        {
            shellWindows = new ShellWindows();
        }

        private IEnumerable<InternetExplorer> GetShellViews()
        {
            if (shellWindows == null) throw new ObjectDisposedException(nameof(ShellHook));
            var n = shellWindows.Count;
            if (n == 0) yield break;

            var windows = new List<InternetExplorer>();
            for (var i = 0; i < n; i++)
            {
                dynamic view = null;
                try
                {
                    view = shellWindows.Item(i);
                }
                catch (Exception) { }
                if (view == null) continue;
                string fullName = null;
                try
                {
                    fullName = view.FullName;
                }
                catch (Exception) { }
                if (fullName != null && Path.GetFileNameWithoutExtension(fullName).ToLowerInvariant() == "explorer")
                {
                    yield return view;
                }
                else
                {
                    Marshal.ReleaseComObject(view);
                }
            }
        }

        /// <summary>
        /// Returns all shell views of the top most Explorer window.
        /// Returns only one view if the top most Explorer window has no tabs.
        /// </summary>
        private IEnumerable<InternetExplorer> GetTopMostShellViews()
        {
            var explorerViews = GetShellViews().ToList();

            if (explorerViews.Count == 0) yield break;
            if (explorerViews.Count == 1) yield return explorerViews[0];

            var matched = new List<InternetExplorer>();
            foreach (var desktopWindowHandle in WinApi.GetDesktopWindowsInZOrder())
            {
                var hWnd = desktopWindowHandle.ToInt64();
                foreach (var view in explorerViews)
                {
                    var isTopMost = false;
                    try
                    {
                        isTopMost = view.HWND == hWnd;
                    }
                    catch (Exception) { }
                    if (isTopMost)
                    {
                        matched.Add(view);
                        yield return view;
                    }
                }
                if (matched.Count > 0) break;
            }
            // release unmatched shell views
            foreach (var view in explorerViews)
            {
                if (!matched.Contains(view))
                {
                    Marshal.ReleaseComObject(view);
                }
            }
        }

        private static IntPtr GetTabHandle(InternetExplorer shellView)
        {
            var SID_STopLevelBrowser = new Guid("4c96be40-915c-11cf-99d3-00aa004ae837");
            var IID_IShellBrowser = new Guid("000214E2-0000-0000-C000-000000000046");

            var sp = (IServiceProvider)shellView;
            IShellBrowser browser = null;
            try
            {
                browser = (IShellBrowser)sp.QueryService(SID_STopLevelBrowser, IID_IShellBrowser);
                if (browser != null) return browser.GetWindow();
            }
            finally
            {
                if (browser != null)
                {
                    Marshal.ReleaseComObject(browser);
                }
            }
            return IntPtr.Zero;
        }

        private (InternetExplorer, IntPtr) GetTopMostShellView()
        {
            var topMostShellViews = GetTopMostShellViews().ToList();
            if (topMostShellViews.Count == 0) return (null, IntPtr.Zero);

            var topMostShellWindowHWnd = topMostShellViews[0].HWND;
            // Get the handle of the first control of class ShellTabWindowClass in Z-order,
            // which is the tab in the foreground
            var activeTabHWnd = WinApi.FindWindowEx(new IntPtr(topMostShellWindowHWnd), IntPtr.Zero, "ShellTabWindowClass", null);

            InternetExplorer result = null;
            IntPtr tabHandle = IntPtr.Zero;

            if (activeTabHWnd == IntPtr.Zero)
            {
                // shell window has no tabs
                result = topMostShellViews[0];
                for (var i  = 1; i < topMostShellViews.Count; i++)
                {
                    Marshal.ReleaseComObject(topMostShellViews[i]);
                }
            }
            else
            {
                // shell window has tabs, pick the one with the handle of the foreground tab control
                for (var i = 0; i < topMostShellViews.Count; i++)
                {
                    var view = topMostShellViews[i];
                    var tabHWnd = GetTabHandle(view);
                    if (tabHWnd == activeTabHWnd)
                    {
                        result = view;
                        tabHandle = tabHWnd;
                    }
                    else
                    {
                        Marshal.ReleaseComObject(view);
                    }
                }
            }
            return (result, tabHandle);
        }

        public void Update(bool updateSelection)
        {
            var (topMostShellView, tabHandle) = GetTopMostShellView();

            if (topMostShellView != null)
            {
                // Update state
                if (LastWindowHWnd != topMostShellView.HWND)
                {
                    LastWindowHWnd = topMostShellView.HWND;
                    LastTabHWnd = tabHandle.ToInt64();
                }
                else if (LastTabHWnd != tabHandle.ToInt64())
                {
                    LastTabHWnd = tabHandle.ToInt64();
                }
                if (LastLocationUrl != topMostShellView.LocationURL
                    || LastLocationName != topMostShellView.LocationName)
                {
                    LastLocationUrl = topMostShellView.LocationURL;
                    LastLocationName = topMostShellView.LocationName;
                }

                // only determine selection, if required
                if (updateSelection)
                {
                    var doc = (ShellFolderView)topMostShellView.Document;
                    var items = doc.SelectedItems();
                    bool changed = false;
                    var selection = new List<string>();

                    // iterate through selected items
                    foreach (FolderItem item in items)
                    {
                        var path = item.Path;
                        selection.Add(path);

                        // is more selected?
                        if (!lastSelection.Contains(path))
                        {
                            changed = true;
                        }
                        // release COM item
                        Marshal.ReleaseComObject(item);
                    }

                    // release COM item list
                    Marshal.ReleaseComObject(items);
                    items = null;

                    // release COM document
                    Marshal.ReleaseComObject(doc);
                    doc = null;

                    if (!changed)
                    {
                        // iterate through as of yet selected items
                        foreach (var path in lastSelection)
                        {
                            // is less selected?
                            if (!selection.Contains(path))
                            {
                                changed = true;
                                break;
                            }
                        }
                    }
                    if (changed)
                    {
                        lastSelection.Clear();
                        lastSelection.AddRange(selection);
                    }
                }   

                Marshal.ReleaseComObject(topMostShellView);
            }
            else
            {
                // if there is no Explorer window
                if (LastWindowHWnd != 0 || LastTabHWnd != 0)
                {
                    LastTabHWnd = 0;
                    LastWindowHWnd = 0;
                }
                if (LastLocationUrl != null || LastLocationName != null)
                {
                    LastLocationUrl = null;
                    LastLocationName = null;
                }
                if (lastSelection.Count > 0)
                {
                    lastSelection.Clear();
                }
            }
        }

        public string GetCurrentLocation()
        {
            Update(updateSelection: false);
            return LastLocationUrl;
        }

        public void GetCurrentLocation(out string url, out string name)
        {
            Update(updateSelection: false);
            url = LastLocationUrl;
            name = LastLocationName;
        }

        public string[] GetCurrentSelection()
        {
            Update(updateSelection: true);
            return GetLastSelection();
        }

        public void Dispose()
        {
            if (shellWindows != null)
            {
                Marshal.ReleaseComObject(shellWindows);
                shellWindows = null;
            }
            GC.SuppressFinalize(this);
        }

        ~ShellHook()
        {
            Dispose();
        }
    }
}
