using System;
using System.Linq;
using System.Windows;

namespace Mastersign.WinJockey
{
    public partial class App : Application
    {
        private readonly MqttConnection mqttConnection;

        private readonly WinJockeyConfiguration config;

        private readonly WinJockeyRuntime runtime;

        public WinJockeyRuntime Runtime => runtime;

        public App()
        {
            Exit += ExitHandler;

            config = new WinJockeyConfiguration();
            config.Synchronize();

            runtime = new WinJockeyRuntime
            {
                Dispatcher = Dispatcher,
                Config = config,
            };

            mqttConnection = new MqttConnection();
            mqttConnection.DebugMessage += DebugMessageHandler;

            runtime.MqttConnection = mqttConnection;
            runtime.DebugMessage += DebugMessageHandler;
        }

        private void ExitHandler(object sender, ExitEventArgs e)
        {
            runtime.Dispose();
            mqttConnection.Dispose();
            StringExpander.DisposeInstance();
        }

        private void DebugMessageHandler(object sender, DebugMessageEventArgs e)
        {
            runtime.ShowDebugMessage(e.Message);
        }

        public bool StartMinimized => Environment.GetCommandLineArgs().Contains("--minimized");

    }
}
