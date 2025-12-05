using AuroraDuel.Managers;
using AuroraDuel.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;

namespace AuroraDuel.Commands;

/// <summary>
/// Handles all chat and console commands for the plugin
/// </summary>
public class DuelCommands
{
    private readonly ConfigManager _configManager;
    private readonly DuelGameManager? _duelGameManager;
    private readonly SettingsManager? _settingsManager;
    private readonly LocalizationManager? _localizationManager;

    private Localization Localization => _localizationManager?.GetLocalization() ?? new Localization();

    public DuelCommands(ConfigManager configManager, DuelGameManager? duelGameManager = null, SettingsManager? settingsManager = null, LocalizationManager? localizationManager = null)
    {
        _configManager = configManager;
        _duelGameManager = duelGameManager;
        _settingsManager = settingsManager;
        _localizationManager = localizationManager;
    }

    public void RegisterCommands(BasePlugin plugin)
    {
        plugin.AddCommand("duel_add_t", "duel_add_t <UniqueDuelName>", Command_AddTSpawn);
        plugin.AddCommand("duel_add_ct", "duel_add_ct <UniqueDuelName>", Command_AddCTSpawn);
        plugin.AddCommand("duel_remove_t_spawn", "duel_remove_t_spawn <DuelName> <index>", Command_RemoveTSpawn);
        plugin.AddCommand("duel_remove_ct_spawn", "duel_remove_ct_spawn <DuelName> <index>", Command_RemoveCTSpawn);
        plugin.AddCommand("duel_info", "duel_info <DuelName>", Command_DuelInfo);
        plugin.AddCommand("duel_config", "Enable/disable configuration mode. Usage: !duel_config [on|off]", Command_ToggleConfigMode);
        plugin.AddCommand("duel_map", "Change the map. Usage: !duel_map <map_name>", Command_ChangeMap);
        plugin.AddCommand("duel_reload", "Reload plugin settings", Command_ReloadSettings);
        plugin.AddCommand("duel_list", "List all duels on current map", Command_ListDuels);
        plugin.AddCommand("duel_delete", "Delete a duel. Usage: !duel_delete <DuelName>", Command_DeleteDuel);
        plugin.AddCommand("duel_help", "Display list of all available commands", Command_Help);
    }

    [RequiresPermissions("@css/root")]
    public void Command_ReloadSettings(CCSPlayerController? player, CommandInfo info)
    {
        if (_settingsManager == null)
        {
            string message = $"{ChatColors.Red}{Localization.ErrorSettingsManagerNotAvailable}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        _settingsManager.ReloadSettings();
        string successMessage = $"{ChatColors.Green}{Localization.SettingsReloadedSuccess}";
        if (player != null && player.IsValid)
            player.PrintToChat(successMessage);
        else
            info.ReplyToCommand(successMessage);
    }

    /// <summary>
    /// Changes the server map
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_ChangeMap(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            string message = $"{ChatColors.Red}{string.Format(Localization.UsageDuelMap, "!duel_map")}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        string newMapName = info.GetArg(1).Trim();

        if (string.IsNullOrWhiteSpace(newMapName))
        {
            string message = $"{ChatColors.Red}{Localization.ErrorMapNameEmpty}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        Server.PrintToChatAll($"{ChatColors.LightBlue}{string.Format(Localization.MapChangeImminent, newMapName)}");
        Server.ExecuteCommand($"changelevel {newMapName}");
    }

    /// <summary>
    /// Enables or disables configuration mode
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_ToggleConfigMode(CCSPlayerController? player, CommandInfo info)
    {
        bool newState;

        if (info.ArgCount < 2)
        {
            newState = !DuelGameManager.IsConfigModeActive;
        }
        else
        {
            string arg = info.GetArg(1).ToLower();
            if (arg == "on")
            {
                newState = true;
            }
            else if (arg == "off")
            {
                newState = false;
            }
            else
            {
                string message = $"{ChatColors.Red}{string.Format(Localization.ErrorInvalidArgument, "!duel_config [on|off]")}";
                if (player != null && player.IsValid)
                    player.PrintToChat(message);
                else
                    info.ReplyToCommand(message);
                return;
            }
        }

        if (_duelGameManager != null)
        {
            _duelGameManager.SetConfigMode(newState);
            string message = newState 
                ? $"{ChatColors.LightBlue}{Localization.ConfigModeStatusActive}"
                : $"{ChatColors.LightBlue}{Localization.ConfigModeStatusInactive}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
        }
        else
        {
            string message = $"{ChatColors.Red}{Localization.ErrorDuelGameManagerNotAvailable}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
        }
    }

    /// <summary>
    /// Gets the current spawn point from player position
    /// </summary>
    private Models.SpawnPoint GetCurrentSpawnPoint(CCSPlayerController player)
    {
        if (player.Pawn?.Value == null)
        {
            throw new InvalidOperationException("Cannot get position: Pawn is invalid.");
        }

        var pawn = player.Pawn.Value;
        var origin = pawn.AbsOrigin!;
        var angle = pawn.AbsRotation!;

        return new Models.SpawnPoint
        {
            PosX = origin.X,
            PosY = origin.Y,
            PosZ = origin.Z,
            AngleYaw = angle.Y
        };
    }

    [RequiresPermissions("@css/root")]
    public void Command_AddTSpawn(CCSPlayerController? player, CommandInfo info)
    {
        AddSpawn(player, info, true);
    }

    [RequiresPermissions("@css/root")]
    public void Command_AddCTSpawn(CCSPlayerController? player, CommandInfo info)
    {
        AddSpawn(player, info, false);
    }

    /// <summary>
    /// Adds a spawn to the T or CT list of a duel
    /// </summary>
    private void AddSpawn(CCSPlayerController? player, CommandInfo info, bool isTerrorist)
    {
        if (info.ArgCount < 2)
        {
            string usage = isTerrorist ? Localization.UsageAddTSpawn : Localization.UsageAddCTSpawn;
            string message = $"{ChatColors.Red}{string.Format(usage, info.GetArg(0))}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        if (player == null || !player.IsValid)
        {
            info.ReplyToCommand($"{ChatColors.Red}{Localization.ErrorInvalidPlayer}");
            return;
        }

        string comboName = info.GetArg(1).Trim();
        string mapName = Server.MapName;

        var combo = _configManager.CurrentConfig.Combos.FirstOrDefault(c =>
            c.ComboName.Equals(comboName, StringComparison.OrdinalIgnoreCase) &&
            c.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase));

        if (combo == null)
        {
            combo = new DuelCombination
            {
                MapName = mapName,
                ComboName = comboName,
                TSpawns = new List<Models.SpawnPoint>(),
                CTSpawns = new List<Models.SpawnPoint>()
            };
            _configManager.CurrentConfig.Combos.Add(combo);
            player.PrintToChat($"{ChatColors.Green}{string.Format(Localization.DuelCreated, comboName, mapName)}");
        }

        var spawnPoint = GetCurrentSpawnPoint(player);
        if (isTerrorist)
        {
            combo.TSpawns.Add(spawnPoint);
            player.PrintToChat($"{ChatColors.Green}{string.Format(Localization.TSpawnAdded, comboName, combo.TSpawns.Count)}");
        }
        else
        {
            combo.CTSpawns.Add(spawnPoint);
            player.PrintToChat($"{ChatColors.Green}{string.Format(Localization.CTSpawnAdded, comboName, combo.CTSpawns.Count)}");
        }

        _configManager.SaveConfig();
    }

    /// <summary>
    /// Supprime un spawn T d'un duel.
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_RemoveTSpawn(CCSPlayerController? player, CommandInfo info)
    {
        RemoveSpawn(player, info, true);
    }

    /// <summary>
    /// Supprime un spawn CT d'un duel.
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_RemoveCTSpawn(CCSPlayerController? player, CommandInfo info)
    {
        RemoveSpawn(player, info, false);
    }

    /// <summary>
    /// Removes a spawn from the T or CT list of a duel
    /// </summary>
    private void RemoveSpawn(CCSPlayerController? player, CommandInfo info, bool isTerrorist)
    {
        if (info.ArgCount < 3)
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Usage: {ChatColors.Yellow}{info.GetArg(0)} <NomDuel> <index>";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        string comboName = info.GetArg(1).Trim();
        string mapName = Server.MapName;

        if (!int.TryParse(info.GetArg(2), out int index) || index < 1)
        {
            string message = $"{ChatColors.Red}{Localization.ErrorInvalidIndex}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        var combo = _configManager.CurrentConfig.Combos.FirstOrDefault(c =>
            c.ComboName.Equals(comboName, StringComparison.OrdinalIgnoreCase) &&
            c.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase));

        if (combo == null)
        {
            string message = $"{ChatColors.Red}{string.Format(Localization.ErrorDuelNotFound, comboName, mapName)}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        var spawns = isTerrorist ? combo.TSpawns : combo.CTSpawns;
        var validSpawns = spawns
            .Where(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0))
            .ToList();

        if (index > validSpawns.Count)
        {
            string teamName = isTerrorist ? "T" : "CT";
            string message = $"{ChatColors.Red}{string.Format(Localization.ErrorIndexOutOfRange, index, validSpawns.Count, teamName)}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        var spawnToRemove = validSpawns[index - 1];
        spawns.Remove(spawnToRemove);

        string successMessage = isTerrorist
            ? $"{ChatColors.Green}{string.Format(Localization.TSpawnRemoved, index, comboName)}"
            : $"{ChatColors.Green}{string.Format(Localization.CTSpawnRemoved, index, comboName)}";
        if (player != null && player.IsValid)
            player.PrintToChat(successMessage);
        else
            info.ReplyToCommand(successMessage);

        _configManager.SaveConfig();
    }

    /// <summary>
    /// Displays duel details
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_DuelInfo(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            string message = $"{ChatColors.Red}{string.Format(Localization.UsageDuelInfo, "!duel_info")}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        string comboName = info.GetArg(1).Trim();
        string mapName = Server.MapName;

        var combo = _configManager.CurrentConfig.Combos.FirstOrDefault(c =>
            c.ComboName.Equals(comboName, StringComparison.OrdinalIgnoreCase) &&
            c.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase));

        if (combo == null)
        {
            string message = $"{ChatColors.Red}{string.Format(Localization.ErrorDuelNotFound, comboName, mapName)}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        var validTSpawns = combo.TSpawns
            .Where(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0))
            .ToList();
        var validCTSpawns = combo.CTSpawns
            .Where(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0))
            .ToList();

        if (player != null && player.IsValid)
        {
            player.PrintToChat($" {ChatColors.LightBlue}[AuroraDuel] {ChatColors.Yellow}{string.Format(Localization.DuelInfoHeader, comboName)}");
            player.PrintToChat($" {ChatColors.Green}{string.Format(Localization.DuelInfoMap, mapName)}");
            player.PrintToChat($" {ChatColors.Green}{string.Format(Localization.DuelInfoTSpawns, validTSpawns.Count)}");
            player.PrintToChat($" {ChatColors.Green}{string.Format(Localization.DuelInfoCTSpawns, validCTSpawns.Count)}");
            
            if (validTSpawns.Count > 0)
            {
                player.PrintToChat($" {ChatColors.Red}{Localization.DuelInfoTSpawnsList}");
                for (int i = 0; i < validTSpawns.Count; i++)
                {
                    var spawn = validTSpawns[i];
                    player.PrintToChat($"   {ChatColors.Default}{string.Format(Localization.DuelInfoSpawnPosition, i + 1, spawn.PosX, spawn.PosY, spawn.PosZ)}");
                }
            }
            
            if (validCTSpawns.Count > 0)
            {
                player.PrintToChat($" {ChatColors.LightBlue}{Localization.DuelInfoCTSpawnsList}");
                for (int i = 0; i < validCTSpawns.Count; i++)
                {
                    var spawn = validCTSpawns[i];
                    player.PrintToChat($"   {ChatColors.Default}{string.Format(Localization.DuelInfoSpawnPosition, i + 1, spawn.PosX, spawn.PosY, spawn.PosZ)}");
                }
            }
        }
        else
        {
            Console.WriteLine($"[AuroraDuel] {string.Format(Localization.DuelInfoHeader, comboName)}");
            Console.WriteLine(string.Format(Localization.DuelInfoMap, mapName));
            Console.WriteLine(string.Format(Localization.DuelInfoTSpawns, validTSpawns.Count));
            Console.WriteLine(string.Format(Localization.DuelInfoCTSpawns, validCTSpawns.Count));
            
            if (validTSpawns.Count > 0)
            {
                Console.WriteLine(Localization.DuelInfoTSpawnsList);
                for (int i = 0; i < validTSpawns.Count; i++)
                {
                    var spawn = validTSpawns[i];
                    Console.WriteLine(string.Format(Localization.DuelInfoSpawnPosition, i + 1, spawn.PosX, spawn.PosY, spawn.PosZ));
                }
            }
            
            if (validCTSpawns.Count > 0)
            {
                Console.WriteLine(Localization.DuelInfoCTSpawnsList);
                for (int i = 0; i < validCTSpawns.Count; i++)
                {
                    var spawn = validCTSpawns[i];
                    Console.WriteLine(string.Format(Localization.DuelInfoSpawnPosition, i + 1, spawn.PosX, spawn.PosY, spawn.PosZ));
                }
            }
        }
    }

    /// <summary>
    /// Lists all duels on the current map
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_ListDuels(CCSPlayerController? player, CommandInfo info)
    {
        string currentMap = Server.MapName;
        var duelsOnMap = _configManager.CurrentConfig.Combos
            .Where(c => c.MapName.Equals(currentMap, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.ComboName)
            .ToList();

        if (duelsOnMap.Count == 0)
        {
            string message = $"{ChatColors.Yellow}{string.Format(Localization.ErrorNoDuelsOnMap, currentMap)}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        if (player != null && player.IsValid)
        {
            player.PrintToChat($" {ChatColors.LightBlue}[AuroraDuel] {ChatColors.Default}{ChatColors.Yellow}{string.Format(Localization.DuelListHeader, duelsOnMap.Count, currentMap)}");
            
            foreach (var duel in duelsOnMap)
            {
                int tSpawns = duel.TSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                int ctSpawns = duel.CTSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                
                player.PrintToChat($"  {ChatColors.Green}• {ChatColors.Yellow}{string.Format(Localization.DuelListItem, duel.ComboName, tSpawns, ctSpawns)}");
            }
        }
        else
        {
            foreach (var duel in duelsOnMap)
            {
                int tSpawns = duel.TSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                int ctSpawns = duel.CTSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                
                Console.WriteLine(string.Format(Localization.DuelListItem, duel.ComboName, tSpawns, ctSpawns));
            }
        }
    }

    /// <summary>
    /// Deletes a duel from the current map
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_DeleteDuel(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            string message = $"{ChatColors.Red}{string.Format(Localization.UsageDuelDelete, "!duel_delete")}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        string comboName = info.GetArg(1).Trim();
        string mapName = Server.MapName;

        var combo = _configManager.CurrentConfig.Combos.FirstOrDefault(c =>
            c.ComboName.Equals(comboName, StringComparison.OrdinalIgnoreCase) &&
            c.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase));

        if (combo == null)
        {
            string message = $"{ChatColors.Red}{string.Format(Localization.ErrorDuelNotFound, comboName, mapName)}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        _configManager.CurrentConfig.Combos.Remove(combo);
        _configManager.SaveConfig();

        string successMessage = $"{ChatColors.Green}{string.Format(Localization.DuelDeleted, comboName)}";
        if (player != null && player.IsValid)
            player.PrintToChat(successMessage);
        else
            info.ReplyToCommand(successMessage);
    }

    /// <summary>
    /// Displays help with all available commands
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_Help(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && player.IsValid)
        {
            player.PrintToChat($" {ChatColors.LightBlue}[AuroraDuel] {ChatColors.Yellow}{Localization.HelpHeader}");
            player.PrintToChat($" {ChatColors.Green}{Localization.HelpSpawnConfig}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.Red}{string.Format(Localization.HelpAddTSpawn, "!duel_add_t")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpAddCTSpawn, "!duel_add_ct")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.Red}{string.Format(Localization.HelpRemoveTSpawn, "!duel_remove_t_spawn")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpRemoveCTSpawn, "!duel_remove_ct_spawn")}");
            player.PrintToChat($" ");
            player.PrintToChat($" {ChatColors.Green}{Localization.HelpDuelManagement}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpDuelList, "!duel_list")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpDuelInfo, "!duel_info")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpDuelDelete, "!duel_delete")}");
            player.PrintToChat($" ");
            player.PrintToChat($" {ChatColors.Green}{Localization.HelpOtherCommands}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpDuelConfig, "!duel_config")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpDuelMap, "!duel_map")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpDuelReload, "!duel_reload")}");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}{string.Format(Localization.HelpDuelHelp, "!duel_help")}");
            player.PrintToChat($" ");
            player.PrintToChat($" {ChatColors.Yellow}{string.Format(Localization.HelpNote, "@css/root")}");
        }
        else
        {
            Console.WriteLine($"[AuroraDuel] {Localization.HelpHeader}");
            Console.WriteLine(Localization.HelpSpawnConfig);
            Console.WriteLine(string.Format(Localization.HelpAddTSpawn, "!duel_add_t"));
            Console.WriteLine(string.Format(Localization.HelpAddCTSpawn, "!duel_add_ct"));
            Console.WriteLine(string.Format(Localization.HelpRemoveTSpawn, "!duel_remove_t_spawn"));
            Console.WriteLine(string.Format(Localization.HelpRemoveCTSpawn, "!duel_remove_ct_spawn"));
            Console.WriteLine("");
            Console.WriteLine(Localization.HelpDuelManagement);
            Console.WriteLine(string.Format(Localization.HelpDuelList, "!duel_list"));
            Console.WriteLine(string.Format(Localization.HelpDuelInfo, "!duel_info"));
            Console.WriteLine(string.Format(Localization.HelpDuelDelete, "!duel_delete"));
            Console.WriteLine("");
            Console.WriteLine(Localization.HelpOtherCommands);
            Console.WriteLine(string.Format(Localization.HelpDuelConfig, "!duel_config"));
            Console.WriteLine(string.Format(Localization.HelpDuelMap, "!duel_map"));
            Console.WriteLine(string.Format(Localization.HelpDuelReload, "!duel_reload"));
            Console.WriteLine(string.Format(Localization.HelpDuelHelp, "!duel_help"));
        }
    }
}