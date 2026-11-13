using System.Buffers;
using System.Text.Json;
using System.Text;
using MQTTnet;
using MQTTnet.Protocol;

namespace manegment_device;

public sealed record WaterTelemetry(string DeviceId, float? Ph, float? Tds, float? Temperature, float? Battery,
    double? Latitude, double? Longitude, int? Rssi);

public sealed class WaterMqttService : IAsyncDisposable
{
    private readonly MqttClientFactory _factory = new();
    private readonly IMqttClient _client;
    private readonly WaterMonitorSettings _settings;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public event Action<WaterTelemetry>? TelemetryReceived;
    public event Action<string, bool>? DeviceStatusChanged;
    public event Action<bool, string>? ConnectionChanged;
    public bool IsConnected => _client.IsConnected;

    public WaterMqttService(WaterMonitorSettings settings)
    {
        _settings = settings;
        _client = _factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _client.DisconnectedAsync += args =>
        {
            ConnectionChanged?.Invoke(false, "Mất kết nối MQTT");
            return Task.CompletedTask;
        };
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client.IsConnected) return;
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_client.IsConnected) return;
            var options = new MqttClientOptionsBuilder()
                .WithClientId($"water-monitor-{Environment.MachineName}-{Guid.NewGuid():N}"[..40])
                .WithTcpServer(_settings.BrokerHost, _settings.BrokerPort)
                .WithTimeout(TimeSpan.FromSeconds(6));
            if (!string.IsNullOrWhiteSpace(_settings.BrokerUsername))
                options.WithCredentials(_settings.BrokerUsername, _settings.BrokerPassword);
            await _client.ConnectAsync(options.Build(), cancellationToken);
            var subscribe = _factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("water/device/+/telemetry").WithAtMostOnceQoS())
                .WithTopicFilter(f => f.WithTopic("water/device/+/status").WithAtLeastOnceQoS())
                .Build();
            await _client.SubscribeAsync(subscribe, cancellationToken);
            ConnectionChanged?.Invoke(true, $"Đã kết nối {_settings.BrokerHost}:{_settings.BrokerPort}");
        }
        finally { _connectionLock.Release(); }
    }

    public async Task PublishCalibrationAsync(string deviceId, float phOffset, float tdsFactor, float tempOffset,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsConnected) throw new InvalidOperationException("Chưa kết nối MQTT broker.");
        var payload = JsonSerializer.Serialize(new { phOffset, tdsFactor, tempOffset });
        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"water/device/{deviceId}/calibrate")
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build();
        await _client.PublishAsync(message, cancellationToken);
    }

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var levels = args.ApplicationMessage.Topic.Split('/');
            if (levels.Length != 4 || levels[0] != "water" || levels[1] != "device") return Task.CompletedTask;
            var segments = args.ApplicationMessage.Payload.ToArray();
            var deviceId = levels[2];
            if (levels[3] == "status")
            {
                var online = string.Equals(Encoding.UTF8.GetString(segments).Trim().Trim('"'), "online", StringComparison.OrdinalIgnoreCase);
                DeviceStatusChanged?.Invoke(deviceId, online);
                return Task.CompletedTask;
            }
            var payload = JsonDocument.Parse(segments);
            using (payload)
            {
                if (levels[3] == "telemetry")
                {
                    var root = payload.RootElement;
                    TelemetryReceived?.Invoke(new WaterTelemetry(
                        ReadString(root, "deviceId") ?? deviceId,
                        ReadFloat(root, "ph"), ReadFloat(root, "tds"), ReadFloat(root, "temperature"),
                        ReadFloat(root, "battery"), ReadDouble(root, "lat"), ReadDouble(root, "lon"), ReadInt(root, "rssi")));
                }
            }
        }
        catch (JsonException) { }
        catch (InvalidOperationException) { }
        return Task.CompletedTask;
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static float? ReadFloat(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetSingle(out var parsed) ? parsed : null;
    private static double? ReadDouble(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var parsed) ? parsed : null;
    private static int? ReadInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsed) ? parsed : null;

    public async Task DisconnectAsync()
    {
        if (_client.IsConnected) await _client.DisconnectAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try { await DisconnectAsync(); }
        finally { _client.Dispose(); _connectionLock.Dispose(); }
    }
}
