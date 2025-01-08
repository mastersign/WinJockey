using System.Reflection;
using Microsoft.Win32;

namespace Mastersign.WinJockey;

internal static class AutostartManager
{
    private const string RUN_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RUN_VALUE_NAME = "WinJockey";

    private static string ExecutablePath => System.IO.Path.ChangeExtension(
        Assembly.GetExecutingAssembly().Location, ".exe");

    private static string CommandLine => $"\"{ExecutablePath}\" --minimized";

    public static bool IsAutostartEnabled()
    {
        var runKey = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH)
            ?? throw new InvalidOperationException($"Could not find registry key {RUN_KEY_PATH} in user hive.");
        return string.Equals(
            runKey.GetValue(RUN_VALUE_NAME) as string,
            CommandLine);
    }

    public static void EnableAutostart()
    {
        var runKey = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, writable: true)
            ?? throw new InvalidOperationException($"Could not find writable registry key {RUN_KEY_PATH} in user hive.");
        runKey.SetValue(RUN_VALUE_NAME, CommandLine, RegistryValueKind.String);
        runKey.Flush();
    }

    public static void DisableAutostart()
    {
        var runKey = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, writable: true)
            ?? throw new InvalidOperationException($"Could not find writable registry key {RUN_KEY_PATH} in user hive.");
        if (runKey.GetValueKind(RUN_VALUE_NAME) != RegistryValueKind.None)
        {
            runKey.DeleteValue(RUN_VALUE_NAME);
            runKey.Flush();
        }
    }
}
