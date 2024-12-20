using System.Windows.Input;
using Mastersign.WinJockey.Properties.Resources;

namespace Mastersign.WinJockey;

public class Commands
{
    public static RoutedUICommand EnableAutostart { get; }
        = new RoutedUICommand(Common.Command_EnableAutostart, nameof(EnableAutostart), typeof(Commands));

    public static RoutedUICommand DisableAutostart { get; }
        = new RoutedUICommand(Common.Command_DisableAutostart, nameof(DisableAutostart), typeof(Commands));

    public static RoutedUICommand SetupForVsCode { get; }
        = new RoutedUICommand(Common.Command_SetupForVsCode, nameof(SetupForVsCode), typeof(Commands));
}
