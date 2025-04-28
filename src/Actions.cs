using System;
using System.Diagnostics;
using System.IO;
using Mastersign.WinMan;

namespace Mastersign.WinJockey
{
    public class Actions
    {
        private const string DEBUG_ERROR_PREFIX = " !! ";

        public WinJockeyRuntime Runtime { get; set; } = null;

        private WinJockeyConfiguration Config => Runtime.Config;

        private void Debug(string message) => Runtime.Debug(message);
    
        private void RunAction(
            Action<CommandConfiguration> action,
            Func<CommandConfiguration, string> errorMessageSource,
            CommandConfiguration command)
        {
            try
            {
                action(command);
            }
            catch (Exception ex)
            {
                Runtime.Debug(DEBUG_ERROR_PREFIX + ex.GetType().FullName);
                Runtime.Debug("    " + ex.Message);
                //System.Windows.MessageBox.Show(
                //    errorMessageSource(command) +
                //    $"\n\n{ex.GetType().FullName}:\n{ex.Message}",
                //    $"WinJockey {command.Action.ToString().ToUpperInvariant()} Action Error",
                //    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RunAction(Action action, Func<string> errorMessageSource, CommandConfiguration command)
            => RunAction(_ => action(), _ => errorMessageSource(), command);

        private void RunAction(Action action, string errorMessage, CommandConfiguration command)
            => RunAction(action, () => errorMessage, command);

        public void Trigger(CommandConfiguration command)
        {
            int? screen;
            int? vDesktop;
            string shutdownMessage;
            int? shutdownTimeoutSeconds;
            bool shutdownForce;
            string mac;
            string subnet;
            if (command.Action == ActionType.None) return;
            switch (command.Action)
            {
                case ActionType.URL:
                    if (string.IsNullOrWhiteSpace(command.Url)) return;
                    RunAction(OpenUrl, cmd => $"Error opening URL '{cmd.Url}'.", command);
                    break;
                case ActionType.Exec:
                case ActionType.Shell:
                    if (string.IsNullOrWhiteSpace(command.Cmd)) return;
                    RunAction(
                        cmd => FocusWindow.ExecuteWithFocus(IntPtr.Zero, () => ShellCommand(cmd)),
                        cmd => $"Error executing command '{cmd.Cmd} {cmd.CmdArgs}'.",
                        command);
                    break;
                case ActionType.PinWindow:
                    RunAction(WindowManagement.PinCurrentWindow,
                        "Error pinning current window to all virtual desktops.",
                        command);
                    break;
                case ActionType.UnpinWindow:
                    RunAction(WindowManagement.UnpinCurrentWindow,
                        "Error unpinning current window from all virtual desktops.",
                        command);
                    break;
                case ActionType.MaximizeWindow:
                    RunAction(WindowManagement.MaximizeWindow,
                        "Error maximizing current window.",
                        command);
                    break;
                case ActionType.NormalWindow:
                    RunAction(WindowManagement.NormalWindow,
                        "Error restoring normal state for current window.",
                        command);
                    break;
                case ActionType.MinimizeWindow:
                    RunAction(WindowManagement.MinimizeWindow,
                        "Error minimizing current window.",
                        command);
                    break;
                case ActionType.CloseWindow:
                    RunAction(WindowManagement.CloseWindow,
                        "Error closing current window.",
                        command);
                    break;
                case ActionType.MoveWindow:
                    screen = command.Screen;
                    if (screen.HasValue)
                    {
                        RunAction(
                            () => WindowManagement.MoveCurrentWindowToScreen(screen.Value),
                            () => $"Error moving current window to screen {screen.Value}.",
                            command);
                    }
                    var winPos = command.WindowPosition;
                    if (winPos != null)
                    {
                        RunAction(
                            () => WindowManagement.MoveCurrentWindow(
                                winPos.X / 100.0, winPos.Y / 100.0,
                                winPos.W / 100.0, winPos.H / 100.0),
                            () => $"Error moving current window to " +
                                $"left={winPos.X}%, top={winPos.Y}%, width={winPos.W}%, height={winPos.H}%.",
                            command);
                    }
                    switch (command.WindowStyle)
                    {
                        case ProcessWindowStyle.Maximized:
                            RunAction(WindowManagement.MaximizeWindow,
                                "Error maximizing current window.",
                                command);
                            break;
                        case ProcessWindowStyle.Normal:
                            RunAction(WindowManagement.NormalWindow,
                                "Error restoring normal state for current window.",
                                command);
                            break;
                        case ProcessWindowStyle.Minimized:
                            RunAction(WindowManagement.MinimizeWindow,
                                "Error minimizing current window.",
                                command);
                            break;
                    }
                    vDesktop = command.VirtualDesktop;
                    if (vDesktop.HasValue)
                    {
                        RunAction(
                            () =>
                            {
                                WindowManagement.MoveCurrentWindowToVirtualDesktop(vDesktop.Value);
                                WindowManagement.SwitchToVirtualDesktop(vDesktop.Value);
                            },
                            () => $"Error moving current window to virtual desktop {vDesktop.Value}.",
                            command);
                    }
                    break;
                case ActionType.SwitchVirtualDesktop:
                    vDesktop = command.VirtualDesktop;
                    RunAction(
                        () => WindowManagement.SwitchToVirtualDesktop(vDesktop ?? 0),
                        () => $"Error switching virtual desktop {vDesktop ?? 0}.",
                        command);
                    break;

                case ActionType.LockSession:
                    RunAction(
                        () => WindowsSession.Lock(),
                        () => $"Error locking windows session.",
                        command);
                    break;
                case ActionType.StandbySystem:
                    RunAction(
                        () => WindowsPower.Standby(),
                        () => $"Error initiating system standby.",
                        command);
                    break;
                case ActionType.HibernateSystem:
                    RunAction(
                        () => WindowsPower.Hibernate(),
                        () => $"Error initiating system hibernation.",
                        command);
                    break;
                case ActionType.ShutdownSystem:
                    shutdownMessage = command.ShutdownMessage;
                    shutdownTimeoutSeconds = command.ShutdownTimeout;
                    shutdownForce = command.ShutdownForce;
                    RunAction(
                        () => WindowsPower.InitiateShutdown(shutdownMessage,
                            shutdownTimeoutSeconds ?? 0,
                            forceAppClose: shutdownForce,
                            reboot: false),
                        () => $"Error initiating system shutdown.",
                        command);
                    break;
                case ActionType.RebootSystem:
                    shutdownMessage = command.ShutdownMessage;
                    shutdownTimeoutSeconds = command.ShutdownTimeout;
                    shutdownForce = command.ShutdownForce;
                    RunAction(
                        () => WindowsPower.InitiateShutdown(shutdownMessage,
                            shutdownTimeoutSeconds ?? 0,
                            forceAppClose: shutdownForce,
                            reboot: true),
                        () => $"Error initiating system reboot.",
                        command);
                    break;
                case ActionType.WakeOnLan:
                    mac = command.Mac;
                    subnet = command.Subnet;
                    RunAction(
                        () => WakeOnLan.SendMagicPacket(mac, subnet),
                        () => $"Error sending magic packet.",
                        command);
                    break;
            }
        }

        private void OpenUrl(CommandConfiguration command)
        {
            var setup = Config.Setup;
            var url = new Uri(command.Url, UriKind.Absolute).AbsoluteUri;
            ProcessStart browser = null;
            if (setup.Browsers != null &&
                setup.Browsers.TryGetValue("Default", out var defBrowser)) {
                browser = defBrowser;
            }
            if (!string.IsNullOrEmpty(command.Browser) &&
                setup.Browsers != null &&
                setup.Browsers.TryGetValue(command.Browser, out var altBrowser))
            {
                browser = altBrowser;
            }
            var browserExe = browser != null && !string.IsNullOrWhiteSpace(browser.Exe)
                ? Environment.ExpandEnvironmentVariables(browser.Exe)
                : null;
            if (browserExe != null && File.Exists(browserExe))
            {
                Process.Start(browserExe,
                    string.IsNullOrWhiteSpace(browser.Args)
                        ? url
                        : Environment.ExpandEnvironmentVariables(browser.Args).Replace("$URL$", url));
            }
            else
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                });
            }
        }

        private void ShellCommand(CommandConfiguration command)
        {
            if (string.IsNullOrWhiteSpace(command.Cmd)) return;
            var cmd = Environment.ExpandEnvironmentVariables(command.Cmd);
            var args = !string.IsNullOrWhiteSpace(command.CmdArgs)
                ? Environment.ExpandEnvironmentVariables(command.CmdArgs)
                : string.Empty;
            var workingDir = !string.IsNullOrWhiteSpace(command.WorkingDir)
                ? Environment.ExpandEnvironmentVariables(command.WorkingDir)
                : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var hasWindowsShellDirectoryPlaceholder =
                StringExpander.HasWindowsShellDirectoryPlaceholder(cmd) ||
                StringExpander.HasWindowsShellDirectoryPlaceholder(args) ||
                StringExpander.HasWindowsShellDirectoryPlaceholder(workingDir);
            var hasWindowsShellSelectionPlaceholder =
                StringExpander.HasWindowsShellSelectionPlaceholder(args);
            if (hasWindowsShellDirectoryPlaceholder ||
                hasWindowsShellSelectionPlaceholder)
            {
                var se = StringExpander.Instance;
                if (!se.UpdateWindowsShellState(hasWindowsShellSelectionPlaceholder))
                {
                    Debug("No Windows Shell location available");
                    return;
                }
                if (hasWindowsShellSelectionPlaceholder && !se.IsShellSelectionAvailable)
                {
                    Debug("No Window Shell selection available");
                    return;
                }
                cmd = se.ExpandWindowsShellDir(cmd);
                args = se.ExpandWindowsShellDir(args);
                if (hasWindowsShellSelectionPlaceholder)
                {
                    args = se.ExpandWindowsShellSelection(args);
                }
                workingDir = se.ExpandWindowsShellDir(workingDir);
            }

            cmd = StringExpander.ExpandWinJockeyConfigLocation(cmd, Config.RealPath);
            args = StringExpander.ExpandWinJockeyConfigLocation(args, Config.RealPath);
            workingDir = StringExpander.ExpandWinJockeyConfigLocation(workingDir, Config.RealPath);

            Process.Start(new ProcessStartInfo(cmd, args)
            {
                WorkingDirectory = workingDir,
                UseShellExecute = command.Action == ActionType.Shell,
                WindowStyle = command.WindowStyle ?? ProcessWindowStyle.Normal,
            });
        }

    }
}
