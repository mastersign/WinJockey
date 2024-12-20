using System.Collections.Concurrent;
using MQTTnet;
using MQTTnet.Client;

#nullable enable

namespace Mastersign.WinJockey;

public sealed class MqttConnection : IDisposable
{
    private const string DEBUG_PREFIX_CONNECTING = " ⇒ ";
    private const string DEBUG_PREFIX_CONNECTED = "✔ ";
    private const string DEBUG_PREFIX_DISCONNECTED = "❌ ";
    private const string DEBUG_PREFIX_SUBSCRIBE = " ⇌ ";
    private const string DEBUG_PREFIX_INPUT =  " ◀ ";
    private const string DEBUG_PREFIX_OUTPUT = " ▷ ";

    private MqttSetup? mqttSetup = null;

    public event EventHandler? Connected;

    public event EventHandler<MqttEventArgs>? Message;

    public event EventHandler<DebugMessageEventArgs>? DebugMessage;

    private readonly MqttFactory mqttFactory = new();
    private IMqttClient? mqttClient;
    private MqttClientOptions? mqttClientOptions;
    private string? lastKnownConnectionHash = null;

    private readonly CancellationTokenSource disposeTokenSource = new();
    private readonly PeriodicTimer connectTimer = new(TimeSpan.FromSeconds(5));

    private readonly BlockingCollection<MqttEventArgs?> mqttEvents = new(new ConcurrentQueue<MqttEventArgs?>());

    public MqttConnection()
    {
        _ = ConnectionWorker();
        ProcessMessageQueue();
    }

    private async Task ConnectionWorker()
    {
        var disposeToken = disposeTokenSource.Token;
        while (!disposeToken.IsCancellationRequested)
        {
            await connectTimer.WaitForNextTickAsync(disposeToken);
            if (mqttClient is null) continue;
            await TryConnect();
        }
    }

    private async Task TryConnect()
    {
        if (mqttClient is null) return;
        var disposeToken = disposeTokenSource.Token;
        if (await mqttClient.TryPingAsync(disposeToken)) return;
        try
        {
            await mqttClient.ConnectAsync(mqttClientOptions, disposeToken);
        }
        catch (InvalidOperationException)
        {
            // ignore concurrent connect attempts
        }
        catch (Exception e)
        {
            Debug(DEBUG_PREFIX_DISCONNECTED + "Connection failed: " + e.Message);
        }
    }

    public MqttSetup? Setup
    {
        get => mqttSetup;
        set
        {
            if (mqttSetup == value) return;
            if (mqttSetup != null)
            {
                mqttSetup.PropertyChanged -= MqttSetupPropertyChangedHandler;
            }
            mqttSetup = value;
            if (mqttSetup != null)
            {
                mqttSetup.PropertyChanged += MqttSetupPropertyChangedHandler;
            }
            UpdateConnection();
        }
    }

    private void MqttSetupPropertyChangedHandler(object? sender, EventArgs e) => UpdateConnection();

    private void UpdateConnection()
    {
        if (string.IsNullOrEmpty(mqttSetup?.Host) || string.IsNullOrEmpty(mqttSetup?.BaseTopic))
        {
            Disconnect();
            return;
        }

        var connectionHash = $"{mqttSetup.Host}:{mqttSetup.Port}/{mqttSetup.BaseTopic}";
        if (string.Equals(connectionHash, lastKnownConnectionHash)) return;

        mqttClient = mqttFactory.CreateMqttClient();
        mqttClient.ConnectingAsync += MqttClientConnectingHandler;
        mqttClient.ConnectedAsync += MqttClientConnectedHandler;
        mqttClient.DisconnectedAsync += MqttClientDisconnectedHandler;
        mqttClient.ApplicationMessageReceivedAsync += MqttClientApplicationMessageReceivedHandler;
        mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
            .WithTcpServer(mqttSetup.Host, mqttSetup.Port)
            .WithCleanSession()
            .Build();

        lastKnownConnectionHash = connectionHash;

        _ = TryConnect();
    }

    private void Disconnect()
    {
        if (mqttClient is null) return;
        if (mqttClient.IsConnected)
        {
            mqttClient.DisconnectAsync().Wait();
        }
        mqttClient.Dispose();
        mqttClient = null;
    }

    private Task MqttClientConnectingHandler(MqttClientConnectingEventArgs ea)
    {
        Debug(DEBUG_PREFIX_CONNECTING + "Connecting to MQTT server");
        return Task.CompletedTask;
    }

    private async Task MqttClientConnectedHandler(MqttClientConnectedEventArgs ea)
    {
        Debug(DEBUG_PREFIX_CONNECTED + "Connected to MQTT server");
        Connected?.Invoke(this, EventArgs.Empty);

        var topic = (mqttSetup?.ExpandedBaseTopic ?? string.Empty) + "#";
        Debug(DEBUG_PREFIX_SUBSCRIBE + "Subscribing to " + topic);

        var topicFilter = new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithExactlyOnceQoS()
            .Build();
        var options = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topicFilter)
            .Build();

        await mqttClient!.SubscribeAsync(options);
    }

    private Task MqttClientApplicationMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs arg)
    {
        Debug(DEBUG_PREFIX_INPUT + arg.ApplicationMessage.Topic);
        mqttEvents.Add(new MqttEventArgs(arg.ApplicationMessage.Topic));
        return Task.CompletedTask;
    }

    private Task MqttClientDisconnectedHandler(MqttClientDisconnectedEventArgs ea)
    {
        Debug(DEBUG_PREFIX_DISCONNECTED + "Disconnected from MQTT server");
        return Task.CompletedTask;
    }

    private void ProcessMessageQueue()
    {
        new Thread(() =>
        {
            foreach (var mqttEvent in mqttEvents.GetConsumingEnumerable())
            {
                if (mqttEvent is null) break;
                Message?.Invoke(this, mqttEvent);
            }
        }).Start();
    }

    // [Conditional("DEBUG")]
    private void Debug(string msg)
    {
        System.Diagnostics.Debug.WriteLine(msg);
        DebugMessage?.Invoke(this, new DebugMessageEventArgs(msg));
    }

    public async Task PublishMessage(string topic, string payload)
    {
        if (mqttClient is null || !mqttClient.IsConnected)
        {
            throw new InvalidOperationException("No MQTT connection");
        }
        Debug(DEBUG_PREFIX_OUTPUT + "Hello on " + topic);
        await mqttClient.PublishStringAsync(topic, payload,
            qualityOfServiceLevel: MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, 
            cancellationToken: disposeTokenSource.Token);
    }

    public void Dispose()
    {
        if (disposeTokenSource.IsCancellationRequested) return;
        if (mqttClient != null)
        {
            if (mqttClient.IsConnected)
            {
                mqttClient.DisconnectAsync().Wait();
            }
            mqttClient.Dispose();
            mqttClient = null;
        }
        disposeTokenSource.Cancel();
        mqttEvents.Add(null); // ends the the event delivery
        GC.SuppressFinalize(this);
    }
}

public class MqttEventArgs : EventArgs
{
    public string Topic { get; }

    public MqttEventArgs(string topic)
    {
        Topic = topic;
    }
}
