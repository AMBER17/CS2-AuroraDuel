using System.Text.Json;
using AuroraDuel.Models;

namespace AuroraDuel.Managers;

/// <summary>
/// Manages plugin settings
/// </summary>
public class SettingsManager
{
    private readonly string _settingsPath;
    private readonly LocalizationManager? _localizationManager;

    public PluginSettings Settings { get; private set; } = new PluginSettings();

    public SettingsManager(string configDirectory, LocalizationManager? localizationManager = null)
    {
        _settingsPath = Path.Combine(configDirectory, "settings.json");
        _localizationManager = localizationManager;
    }

    public void LoadSettings()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                Settings = JsonSerializer.Deserialize<PluginSettings>(json) ?? new PluginSettings();
                var loc = _localizationManager?.GetLocalization();
                Console.WriteLine(loc?.SettingsLoaded ?? "[AuroraDuel] Settings loaded.");
            }
            catch (Exception ex)
            {
                var loc = _localizationManager?.GetLocalization();
                Console.WriteLine(loc != null 
                    ? string.Format(loc.SettingsLoadError, ex.Message)
                    : $"[AuroraDuel] Error loading settings: {ex.Message}");
                Settings = new PluginSettings();
                SaveSettings();
            }
        }
        else
        {
            Settings = new PluginSettings();
            SaveSettings();
            var loc = _localizationManager?.GetLocalization();
            Console.WriteLine(loc?.SettingsCreated ?? "[AuroraDuel] Settings file created with default values.");
        }
    }

    public void SaveSettings()
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            var json = JsonSerializer.Serialize(Settings, options);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            var loc = _localizationManager?.GetLocalization();
            Console.WriteLine(loc != null
                ? string.Format(loc.SettingsSaveError, ex.Message)
                : $"[AuroraDuel] Error saving settings: {ex.Message}");
        }
    }

    public void ReloadSettings()
    {
        LoadSettings();
        var loc = _localizationManager?.GetLocalization();
        Console.WriteLine(loc?.SettingsReloaded ?? "[AuroraDuel] Settings reloaded.");
    }
}

