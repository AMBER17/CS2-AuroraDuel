using System.Text.Json;
using AuroraDuel.Models;

namespace AuroraDuel.Managers;

/// <summary>
/// Manages duel configuration (spawns, combinations)
/// </summary>
public class ConfigManager
{
    private readonly string _configPath;
    private readonly LocalizationManager? _localizationManager;

    public DuelConfig CurrentConfig { get; private set; } = new DuelConfig();

    public ConfigManager(string configDirectory, LocalizationManager? localizationManager = null)
    {
        _configPath = Path.Combine(configDirectory, "duels.json");
        _localizationManager = localizationManager;
    }

    public void LoadConfig()
    {
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            CurrentConfig = JsonSerializer.Deserialize<DuelConfig>(json) ?? new DuelConfig();
            var loc = _localizationManager?.GetLocalization();
            Console.WriteLine(loc != null
                ? string.Format(loc.ConfigLoaded, CurrentConfig.Combos.Count)
                : $"[AuroraDuel] {CurrentConfig.Combos.Count} combinations loaded.");
        }
        else
        {
            CurrentConfig = new DuelConfig();
            var loc = _localizationManager?.GetLocalization();
            Console.WriteLine(loc?.ConfigCreated ?? "[AuroraDuel] New duel configuration created.");
        }
    }

    public void SaveConfig()
    {
        var json = JsonSerializer.Serialize(CurrentConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
        var loc = _localizationManager?.GetLocalization();
        Console.WriteLine(loc != null
            ? string.Format(loc.ConfigSaved, CurrentConfig.Combos.Count)
            : $"[AuroraDuel] {CurrentConfig.Combos.Count} combinations saved.");
    }
}