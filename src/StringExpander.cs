using System;
using System.IO;
using System.Linq;
using Mastersign.WindowsShell;

namespace Mastersign.WinJockey
{
    internal class StringExpander: IDisposable
    {
        private static StringExpander instance;
        public static StringExpander Instance
        {
            get
            {
                if (instance == null) instance = new StringExpander();
                return instance;
            }
        }

        public static void DisposeInstance() => instance?.Dispose();

        private ShellHook shellHook = new ShellHook();

        public void Dispose()
        {
            if (shellHook != null)
            {
                shellHook.Dispose();
                shellHook = null;
            }
            GC.SuppressFinalize(this);
        }

        ~StringExpander() => Dispose();

        private string windowsShellDir;
        private string[] windowsShellSelection = Array.Empty<string>();

        public bool IsShellDirectoryAvailable => windowsShellDir != null;
        public bool IsShellSelectionAvailable => windowsShellSelection.Length > 0;
        public bool IsOneShellItemSelected => windowsShellSelection.Length == 1;

        public static bool HasWindowsShellDirectoryPlaceholder(string s)
            => !string.IsNullOrWhiteSpace(s) && s.Contains("$WSD$");

        public string ExpandWindowsShellDir(string s)
            => s.Replace("$WSD$", windowsShellDir 
                ?? throw new InvalidOperationException("UpdateWindowsShellState() needs to be called first"));

        public static bool HasWindowsShellSelectionPlaceholder(string s)
            => !string.IsNullOrWhiteSpace(s) && (s.Contains("$WSS$") || s.Contains("$WSSS$"));

        public string ExpandWindowsShellSelection(string s, bool doubleQuoted = true) => s
                .Replace("$WSS$",
                    doubleQuoted
                        ? '"' + windowsShellSelection[0] + '"'
                        : windowsShellSelection[0])
                .Replace("$WSSS$",
                    string.Join(" ", doubleQuoted
                        ? windowsShellSelection.Select(p => '"' + p + '"')
                        : windowsShellSelection));

        private static string GetDirectoryPathFromShellLocation(string wsl)
        {
            if (wsl == null) return null;
            var wslUri = new Uri(wsl);
            if (wslUri.Scheme != "file") return null;
            var wsd = wslUri.LocalPath;
            if (string.IsNullOrWhiteSpace(wsd)) return null;
            if (!Directory.Exists(wsd)) return null;
            return wsd;
        }

        public bool UpdateWindowsShellState(bool updateSelection)
        {
            windowsShellDir = null;
            windowsShellSelection = Array.Empty<string>();

            if (shellHook == null) return false;
            shellHook.Update(updateSelection);
            var cwsd = GetDirectoryPathFromShellLocation(shellHook.LastLocationUrl);
            if (cwsd is null) return false;
            windowsShellDir = cwsd;
            if (updateSelection)
            {
                windowsShellSelection = shellHook.GetLastSelection();
            }
            return true;
        }

        public string GetLastWindowsShellDirectory()
            => GetDirectoryPathFromShellLocation(shellHook?.GetCurrentLocation());

        public static string ExpandWinJockeyConfigLocation(string s, string winJockeyConfigRoot)
            => s.Replace("$WJC$", winJockeyConfigRoot);

    }
}
