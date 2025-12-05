using System.Text.Json;

namespace AuroraDuel.Managers;

public class ConfigManager
{
    private readonly string _configPath;

    // Le stockage de la configuration est accessible publiquement
    public DuelConfig CurrentConfig { get; private set; } = new DuelConfig();

    public ConfigManager(string configDirectory)
    {
        // Définit le chemin complet du fichier JSON
        _configPath = Path.Combine(configDirectory, "duels.json");
    }

    public void LoadConfig()
    {
        // Logique de chargement JSON (comme défini précédemment)
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            CurrentConfig = JsonSerializer.Deserialize<DuelConfig>(json) ?? new DuelConfig();
            Console.WriteLine($"[AuroraDuel] {CurrentConfig.Combos.Count} combinaisons chargées.");
        }
        else
        {
            CurrentConfig = new DuelConfig();
            Console.WriteLine("[AuroraDuel] Nouvelle configuration de duels créée.");
        }
    }

    public void SaveConfig()
    {
        // Logique de sauvegarde JSON (comme défini précédemment)
        var json = JsonSerializer.Serialize(CurrentConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
        Console.WriteLine($"[AuroraDuel] {CurrentConfig.Combos.Count} combinaisons sauvegardées.");
    }
}