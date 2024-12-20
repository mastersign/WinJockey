#nullable enable

using System.Windows.Threading;
using Mastersign.Tools;

namespace Mastersign.WinJockey;

partial class WinJockeyRuntime : IDisposable
{
    private const string DEBUG_PREFIX_ACTION = " ↯ ";

    public Dispatcher Dispatcher { get; set; }

    private void Initialize()
    {
        Actions = new() { Runtime = this };

        InitializePropertyBindings();
    }

    public void Dispose()
    {
        DisposePropertyBindings();

        GC.SuppressFinalize(this);
    }

    ~WinJockeyRuntime()
    {
        Dispose();
    }

    #region Debug Messages

    public event EventHandler<DebugMessageEventArgs>? DebugMessage;

    // [Conditional("DEBUG")]
    internal void Debug(string msg)
    {
        System.Diagnostics.Debug.WriteLine(msg);
        DebugMessage?.Invoke(this, new DebugMessageEventArgs(msg));
    }

    public void ShowDebugMessage(string msg) => Dispatcher.BeginInvoke(AddDebugMessageToMessageList, msg);

    private void AddDebugMessageToMessageList(string msg)
    {
        DebugMessages.Insert(0, msg.Ellipsis(100, 9, " ⋯ "));
        while (DebugMessages.Count > 1000)
        {
            DebugMessages.RemoveAt(DebugMessages.Count - 1);
        }
    }

    #endregion

    #region Child Binding

    private PropertyObserver<WinJockeyRuntime, WinJockeyConfiguration>? configObserver;
    private PropertyObserver<WinJockeyConfiguration, WinJockeySetup>? setupObserver;
    private PropertyObserver<WinJockeySetup, MqttSetup>? mqttSetupObserver;
    private PropertyObserver<WinJockeyRuntime, MqttConnection>? mqttConnectionObserver;

    private void InitializePropertyBindings()
    {
        configObserver = new(this, nameof(Config), BindConfig, UnbindConfig);
        mqttConnectionObserver = new(this, nameof(MqttConnection), BindMqttConnection, UnbindMqttConnection);
    }

    private void DisposePropertyBindings()
    {
        if (mqttConnectionObserver != null)
        {
            mqttConnectionObserver.Dispose();
            mqttConnectionObserver = null;
        }
        if (mqttSetupObserver != null)
        {
            mqttSetupObserver.Dispose();
            mqttSetupObserver = null;
        }
        if (setupObserver != null)
        {
            setupObserver.Dispose();
            setupObserver = null;
        }
        if (configObserver != null)
        {
            configObserver.Dispose();
            configObserver = null;
        }
    }

    private void BindConfig(WinJockeyConfiguration configuration)
    {
        setupObserver = new(configuration, nameof(WinJockeyConfiguration.Setup), BindSetup, UnbindSetup);
    }

    private void UnbindConfig(WinJockeyConfiguration configuration)
    {
        if (mqttSetupObserver != null)
        {
            mqttSetupObserver.Dispose();
            mqttSetupObserver = null;
        }
    }
    private void BindSetup(WinJockeySetup setup)
    {
        mqttSetupObserver = new(setup, nameof(WinJockeySetup.MqttServer), BindMqttSetup, UnbindMqttSetup);
    }

    private void UnbindSetup(WinJockeySetup setup)
    {
        if (mqttSetupObserver != null)
        {
            mqttSetupObserver.Dispose();
            mqttSetupObserver = null;
        }
    }

    private void BindMqttSetup(MqttSetup setup)
    {
        if (MqttConnection != null)
        {
            MqttConnection.Setup = setup;
        }
    }

    private void UnbindMqttSetup(MqttSetup setup)
    {
        if (MqttConnection != null)
        {
            MqttConnection.Setup = null;
        }
    }

    private void BindMqttConnection(MqttConnection connection)
    {
        connection.Connected += MqttConnectedHandler;
        connection.Message += MqttMessageHandler;
        if (Config?.Setup?.MqttServer != null)
        {
            connection.Setup = Config.Setup.MqttServer;
        }
    }

    private void UnbindMqttConnection(MqttConnection connection)
    {
        connection.Setup = null;
        connection.Message -= MqttMessageHandler;
        connection.Connected -= MqttConnectedHandler;
    }

    #endregion

    private void MqttConnectedHandler(object? sender, EventArgs e)
    {
        var topic = Config.Setup.MqttServer.ExpandedBaseTopic + "HELLO";
        var payload = DateTime.UtcNow.ToString("u");
        _ = MqttConnection.PublishMessage(topic, payload);
    }

    private void MqttMessageHandler(object? sender, MqttEventArgs e)
    {
        foreach (var command in MatchMqttMessage(e))
        {
            Debug(DEBUG_PREFIX_ACTION + command.CommandName);
            Dispatcher.BeginInvoke(Actions.Trigger, command);
        }
    }

    private IEnumerable<CommandConfiguration> MatchMqttMessage(MqttEventArgs e)
    {
        foreach (var command in Config.Commands)
        {
            if (command.MatchesMqttTopic(Config.Setup.MqttServer.ExpandedBaseTopic ?? string.Empty, e.Topic))
            {
                yield return command;
            }
        }
    }
}