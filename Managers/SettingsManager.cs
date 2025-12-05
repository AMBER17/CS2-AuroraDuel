using System.Text.Json;
using AuroraDuel.Models;

namespace AuroraDuel.Managers;

public class SettingsManager
{
    private readonly string _settingsPath;

    public PluginSettings Settings { get; private set; } = new PluginSettings();

    public SettingsManager(string configDirectory)
    {
        _settingsPath = Path.Combine(configDirectory, "settings.json");
    }

    public void LoadSettings()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                Settings = JsonSerializer.Deserialize<PluginSettings>(json) ?? new PluginSettings();
                Console.WriteLine("[AuroraDuel] Paramètres chargés.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuroraDuel] Erreur lors du chargement des paramètres : {ex.Message}");
                Settings = new PluginSettings();
                SaveSettings(); // Créer un fichier par défaut
            }
        }
        else
        {
            Settings = new PluginSettings();
            SaveSettings(); // Créer un fichier par défaut
            Console.WriteLine("[AuroraDuel] Fichier de paramètres créé avec les valeurs par défaut.");
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
            Console.WriteLine($"[AuroraDuel] Erreur lors de la sauvegarde des paramètres : {ex.Message}");
        }
    }

    public void ReloadSettings()
    {
        LoadSettings();
        Console.WriteLine("[AuroraDuel] Paramètres rechargés.");
    }
}

