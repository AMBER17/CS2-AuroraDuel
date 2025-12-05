using AuroraDuel.Commands;
using AuroraDuel.Managers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace AuroraDuel;

public class AuroraDuel : BasePlugin
{
    public override string ModuleName => "AuroraDuel Plugin";
    public override string ModuleVersion => "1.0.0";

    private ConfigManager? _configManager;
    private SettingsManager? _settingsManager;
    private DuelCommands? _duelCommands;
    private DuelGameManager? _duelGameManager;

    public override void Load(bool hotReload)
    {
        string configDirectory = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins");
        string pluginConfigPath = Path.Combine(configDirectory, "AuroraDuel");

        if (!Directory.Exists(pluginConfigPath))
        {
            Directory.CreateDirectory(pluginConfigPath);
        }

        // Charger les paramètres du plugin
        _settingsManager = new SettingsManager(pluginConfigPath);
        _settingsManager.LoadSettings();

        // Charger les combinaisons de duels
        _configManager = new ConfigManager(pluginConfigPath);
        _configManager.LoadConfig();

        // Initialiser le gestionnaire de jeu
        _duelGameManager = new DuelGameManager(_configManager, _settingsManager, this);
        _duelGameManager.RegisterEvents(this);

        // Initialiser les commandes
        _duelCommands = new DuelCommands(_configManager, _duelGameManager, _settingsManager);
        _duelCommands.RegisterCommands(this);

        Console.WriteLine("[AuroraDuel] Plugin chargé avec succès !");
    }

    public override void Unload(bool hotReload)
    {
        _configManager?.SaveConfig();
        base.Unload(hotReload);
    }
}