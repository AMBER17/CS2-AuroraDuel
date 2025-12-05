using System.Text.Json;
using AuroraDuel.Models;

namespace AuroraDuel.Managers;

/// <summary>
/// Manages localization and translations for the plugin
/// </summary>
public class LocalizationManager
{
    private readonly string _localizationPath;
    private Localization _localization;

    public LocalizationManager(string configDirectory)
    {
        _localizationPath = Path.Combine(configDirectory, "localization.json");
        _localization = new Localization();
        LoadLocalization();
    }

    public Localization GetLocalization()
    {
        return _localization;
    }

    private void LoadLocalization()
    {
        if (File.Exists(_localizationPath))
        {
            try
            {
                var json = File.ReadAllText(_localizationPath);
                var loaded = JsonSerializer.Deserialize<Localization>(json);
                if (loaded != null)
                {
                    _localization = loaded;
                    Console.WriteLine("[AuroraDuel] Localization loaded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuroraDuel] Error loading localization: {ex.Message}. Using default (English).");
                SaveLocalization(); // Create default file
            }
        }
        else
        {
            SaveLocalization(); // Create default file
            Console.WriteLine("[AuroraDuel] Localization file created with default values (English).");
        }
    }

    public void SaveLocalization()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_localization, options);
            File.WriteAllText(_localizationPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuroraDuel] Error saving localization: {ex.Message}");
        }
    }

    public void ReloadLocalization()
    {
        LoadLocalization();
        Console.WriteLine("[AuroraDuel] Localization reloaded.");
    }
}

