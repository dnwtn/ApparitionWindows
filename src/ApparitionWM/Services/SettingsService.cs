using System.IO;
using System.Text.Json;
using ApparitionWM.Models;

namespace ApparitionWM.Services;

/// <summary>
/// Persists application settings to a JSON file in the user's AppData folder.
/// </summary>
public sealed class SettingsService
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Apparition");

    private static readonly string SettingsPath =
        Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AppSettings _settings = new();

    /// <summary>The current in-memory settings.</summary>
    public AppSettings Settings => _settings;

    /// <summary>Loads settings from disk, or returns defaults if no file exists.</summary>
    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            _settings = new AppSettings();
        }

        return _settings;
    }

    /// <summary>Saves current settings to disk.</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail — settings are not critical
        }
    }

    /// <summary>Updates the settings object and saves to disk.</summary>
    public void Update(Action<AppSettings> configure)
    {
        configure(_settings);
        Save();
    }
}
