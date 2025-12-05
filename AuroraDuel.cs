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
    private LocalizationManager? _localizationManager;
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

        // Load localization
        _localizationManager = new LocalizationManager(pluginConfigPath);

        // Load plugin settings
        _settingsManager = new SettingsManager(pluginConfigPath, _localizationManager);
        _settingsManager.LoadSettings();

        // Load duel combinations
        _configManager = new ConfigManager(pluginConfigPath, _localizationManager);
        _configManager.LoadConfig();

        // Initialize game manager
        _duelGameManager = new DuelGameManager(_configManager, _settingsManager, _localizationManager, this);
        _duelGameManager.RegisterEvents(this);

        // Initialize commands
        _duelCommands = new DuelCommands(_configManager, _duelGameManager, _settingsManager, _localizationManager);
        _duelCommands.RegisterCommands(this);

        Console.WriteLine(_localizationManager.GetLocalization().PluginLoaded);
    }

    public override void Unload(bool hotReload)
    {
        _configManager?.SaveConfig();
        base.Unload(hotReload);
    }
}