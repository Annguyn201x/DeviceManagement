using System.Text.Json;

namespace manegment_device;

public sealed class WaterMonitorSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManagementDevice", "water-monitor-settings.json");

    public string BrokerHost { get; set; } = Environment.GetEnvironmentVariable("WATER_MQTT_HOST") ?? "192.168.1.150";
    public int BrokerPort { get; set; } = int.TryParse(Environment.GetEnvironmentVariable("WATER_MQTT_PORT"), out var port) ? port : 1883;
    public string BrokerUsername { get; set; } = Environment.GetEnvironmentVariable("WATER_MQTT_USERNAME") ?? "";
    public string BrokerPassword { get; set; } = Environment.GetEnvironmentVariable("WATER_MQTT_PASSWORD") ?? "";
    public string DatabaseConnectionString { get; set; } = Environment.GetEnvironmentVariable("WATER_DB_CONNECTION")
        ?? "Server=localhost;Database=device;Uid=root;Pwd=hoangduc2006b@;";

    public static WaterMonitorSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var loaded = JsonSerializer.Deserialize<WaterMonitorSettings>(File.ReadAllText(SettingsPath));
                if (loaded is not null)
                {
                    loaded.DatabaseConnectionString = Environment.GetEnvironmentVariable("WATER_DB_CONNECTION")
                        ?? loaded.DatabaseConnectionString;
                    return loaded;
                }
            }
        }
        catch (IOException) { }
        catch (JsonException) { }
        return new WaterMonitorSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
