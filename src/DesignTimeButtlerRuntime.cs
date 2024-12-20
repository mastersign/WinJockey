using System.Windows.Threading;

namespace Mastersign.WinJockey;

public class DesignTimeWinJockeyRuntime : WinJockeyRuntime
{
    private readonly System.Timers.Timer debugTimer = new(500) { AutoReset = true };
    private long n = 0;

    public DesignTimeWinJockeyRuntime()
    {
        Config = new WinJockeyConfiguration();
        MqttConnection = new MqttConnection();
        Dispatcher = Dispatcher.CurrentDispatcher;

        debugTimer.Elapsed += delegate
        {
            n++;
            var msg = $"MSG {n:000000} {DateTime.Now:F}";
            ShowDebugMessage(msg);
        };
        debugTimer.Start();
    }
}
