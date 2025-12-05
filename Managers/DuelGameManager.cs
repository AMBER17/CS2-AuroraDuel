using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using AuroraDuel.Models;
using AuroraDuel.Utils;

namespace AuroraDuel.Managers;

/// <summary>
/// Manages duel game logic, player handling, and game events
/// </summary>
public class DuelGameManager
{
    private readonly ConfigManager _configManager;
    private readonly SettingsManager _settingsManager;
    private readonly LocalizationManager? _localizationManager;
    private readonly Random _random = new Random();
    private readonly BasePlugin _pluginInstance;
    private DuelCombination? _currentDuelCombo = null;
    private List<CCSPlayerController>? _currentDuelPlayers = null;
    private bool _isDuelInProgress = false;
    private bool _isGameStarted = false;
    private bool _isSettingUpDuel = false; // Flag to prevent OnPlayerTeam from triggering during duel setup

    public static bool IsConfigModeActive { get; private set; } = false;

    private PluginSettings Settings => _settingsManager.Settings;
    private Localization Localization => _localizationManager?.GetLocalization() ?? new Localization();

    /// <summary>
    /// Checks if we are in a valid game round (not warmup)
    /// </summary>
    private bool IsValidGameRound()
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;
        if (gameRules == null) return true;
        return !gameRules.WarmupPeriod;
    }

    public DuelGameManager(ConfigManager configManager, SettingsManager settingsManager, LocalizationManager? localizationManager, BasePlugin pluginInstance)
    {
        _configManager = configManager;
        _settingsManager = settingsManager;
        _localizationManager = localizationManager;
        _pluginInstance = pluginInstance;
    }

    public void SetConfigMode(bool isActive)
    {
        IsConfigModeActive = isActive;

        if (isActive)
        {
            _isDuelInProgress = false;
            _isGameStarted = false;
            Server.ExecuteCommand("sv_cheats 1");
            Server.ExecuteCommand("mp_restartgame 1");
            Server.PrintToChatAll($"{ChatColors.Green}{Localization.ConfigModeActive}");
        }
        else
        {
            Server.ExecuteCommand("sv_cheats 0");
            // Reload duel config from file
            _configManager.LoadConfig();
            Server.ExecuteCommand("mp_restartgame 1");
            _pluginInstance.AddTimer(1.0f, () => 
            {
                LoadDuelConfigs(_pluginInstance);
                // Reset game state to restart
                _isGameStarted = false;
                _isDuelInProgress = false;
                
                // Check if there are already players in T/CT to start the game
                CheckAndStartGameIfPlayersPresent();
            });
            Server.PrintToChatAll($"{ChatColors.Red}{Localization.ConfigModeInactive}");
        }
    }

    public void RegisterEvents(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        // Pre mode to block team change message broadcast
        plugin.RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam, HookMode.Pre);
        plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    private void OnMapStart(string mapName)
    {
        _isGameStarted = false;
        _isDuelInProgress = false;
        _currentDuelCombo = null;
        _currentDuelPlayers = null;
    }

    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        // Hide team change message (Pre mode required)
        if (Settings.HideTeamChangeMessages)
        {
            info.DontBroadcast = true;
            @event.Silent = true;
        }

        if (IsConfigModeActive) return HookResult.Continue;

        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsHLTV) return HookResult.Continue;

        // Ignore bots for startup logic
        if (player.IsBot) return HookResult.Continue;

        int newTeam = @event.Team;
        bool joinsPlayableTeam = newTeam == (int)CsTeam.Terrorist || newTeam == (int)CsTeam.CounterTerrorist;

        if (!_isGameStarted && joinsPlayableTeam)
        {
            _isGameStarted = true;

            _pluginInstance.AddTimer(1.0f, () =>
            {
                LoadDuelConfigs(_pluginInstance);
                Server.ExecuteCommand("mp_restartgame 1");
            });
        }
        else if (_isGameStarted && joinsPlayableTeam)
        {
            // Ignore team changes during duel setup to prevent infinite loops
            if (_isSettingUpDuel) return HookResult.Continue;
            
            // Player joined a team while game is already started
            // Check if we need to start a duel (no duel in progress and valid round)
            if (!_isDuelInProgress && IsValidGameRound())
            {
                _pluginInstance.AddTimer(1.0f, () =>
                {
                    // Respawn all dead players in T/CT teams
                    foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV && 
                        (p.Team == CsTeam.Terrorist || p.Team == CsTeam.CounterTerrorist)))
                    {
                        if (!p.PawnIsAlive)
                        {
                            p.Respawn();
                        }
                    }
                    
                    // Start a duel after respawning
                    _pluginInstance.AddTimer(0.5f, StartNextDuel);
                });
            }
        }
        else if (_isGameStarted && !joinsPlayableTeam)
        {
            _pluginInstance.AddTimer(0.5f, CheckRemainingPlayers);
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (IsConfigModeActive || !_isGameStarted) return HookResult.Continue;
        _pluginInstance.AddTimer(0.5f, CheckRemainingPlayers);
        return HookResult.Continue;
    }

    private void CheckRemainingPlayers()
    {
        var playersInTeam = Utilities.GetPlayers()
            .Count(p => p.IsValid && !p.IsBot && !p.IsHLTV && 
                   (p.Team == CsTeam.Terrorist || p.Team == CsTeam.CounterTerrorist));

        if (playersInTeam == 0)
        {
            _isGameStarted = false;
            _isDuelInProgress = false;
        }
    }

    /// <summary>
    /// Checks if there are players in T/CT and starts the game if needed
    /// </summary>
    private void CheckAndStartGameIfPlayersPresent()
    {
        if (_isGameStarted || IsConfigModeActive) return;

        var playersInTeam = Utilities.GetPlayers()
            .Where(p => p.IsValid && !p.IsBot && !p.IsHLTV && 
                   (p.Team == CsTeam.Terrorist || p.Team == CsTeam.CounterTerrorist))
            .ToList();

        if (playersInTeam.Count > 0)
        {
            _isGameStarted = true;

            _pluginInstance.AddTimer(1.0f, () =>
            {
                LoadDuelConfigs(_pluginInstance);
                Server.ExecuteCommand("mp_restartgame 1");
            });
        }
    }

    /// <summary>
    /// Loads server configuration from duel_settings.cfg file
    /// </summary>
    public static void LoadDuelConfigs(BasePlugin plugin)
    {
        string configFilePath = Path.Combine(plugin.ModuleDirectory, "configs", "duel_settings.cfg");

        if (!File.Exists(configFilePath))
        {
            Console.WriteLine($"[AuroraDuel] Warning: Configuration file not found: {configFilePath}");
            return;
        }

        try
        {
            foreach (var line in File.ReadAllLines(configFilePath))
            {
                var trimmedLine = line.Trim();
                if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//"))
                {
                    Server.ExecuteCommand(trimmedLine);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuroraDuel] Error loading config: {ex.Message}");
        }
    }

    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (IsConfigModeActive || !_isGameStarted) return HookResult.Continue;

        if (!IsValidGameRound())
        {
            return HookResult.Continue;
        }

        _pluginInstance.AddTimer(Settings.DelayAfterRoundStart, StartNextDuel);
        return HookResult.Continue;
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (IsConfigModeActive || !_isDuelInProgress || !_isGameStarted) return HookResult.Continue;
        _pluginInstance.AddTimer(0.1f, CheckTeamElimination);
        return HookResult.Continue;
    }

    /// <summary>
    /// Checks if a team has been eliminated and starts the next duel
    /// </summary>
    private void CheckTeamElimination()
    {
        if (!_isDuelInProgress) return;

        var aliveTerrorists = Utilities.GetPlayers()
            .Count(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive);

        var aliveCTs = Utilities.GetPlayers()
            .Count(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive);

        if (aliveTerrorists == 0 || aliveCTs == 0)
        {
            _isDuelInProgress = false;
            string winnerTeam = aliveTerrorists == 0 ? "CT" : "T";
            
            string winMessage = Localization.DuelWinMessage.Replace("{winnerTeam}", winnerTeam);
            foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV))
            {
                player.PrintToCenter(winMessage);
            }
            
            _pluginInstance.AddTimer(Settings.DelayBeforeNextDuel, StartNextDuel);
        }
    }

    /// <summary>
    /// Starts the next duel by selecting a random combination and teleporting players
    /// </summary>
    private void StartNextDuel()
    {
        if (!_isGameStarted) return;
        
        _isDuelInProgress = false;
        _isSettingUpDuel = true; // Set flag to prevent OnPlayerTeam from triggering during setup

        RemoveDroppedWeapons();

        var currentMapName = Server.MapName;
        var availableCombos = _configManager.CurrentConfig.Combos
            .Where(c => c.MapName.Equals(currentMapName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (availableCombos.Count == 0)
        {
            Server.PrintToChatAll($"{ChatColors.Red}{string.Format(Localization.ErrorNoCombinationsForMap, currentMapName)}");
            _isSettingUpDuel = false;
            return;
        }

        _currentDuelCombo = availableCombos[_random.Next(availableCombos.Count)];
        _currentDuelPlayers = GetDuelPlayers(_currentDuelCombo);

        if (_currentDuelPlayers.Count == 0)
        {
            Server.PrintToChatAll($"{ChatColors.Red}{Localization.ErrorNoPlayersAvailable}");
            _isSettingUpDuel = false;
            return;
        }

        int tCount = _currentDuelPlayers.Count(p => p.Team == CsTeam.Terrorist);
        int ctCount = _currentDuelPlayers.Count(p => p.Team == CsTeam.CounterTerrorist);

        if (tCount == 0 || ctCount == 0)
        {
            Server.PrintToChatAll($"{ChatColors.Red}{Localization.ErrorNotEnoughPlayers}");
            _isSettingUpDuel = false;
            return;
        }

        var spawnIndices = RespawnAndTeleportPlayers(_currentDuelPlayers, _currentDuelCombo);
        
        _isDuelInProgress = true;
        _isSettingUpDuel = false; // Clear flag after duel setup is complete
        
        // General chat message
        string chatMessage = Localization.DuelStartChatMessage
            .Replace("{comboName}", _currentDuelCombo.ComboName)
            .Replace("{tCount}", tCount.ToString())
            .Replace("{ctCount}", ctCount.ToString());
        Server.PrintToChatAll(chatMessage);
        
        // Personalized center screen message for each player with their spawn index
        foreach (var kvp in spawnIndices)
        {
            var player = kvp.Key;
            var spawnIndex = kvp.Value;
            
            if (player.IsValid && !player.IsBot && !player.IsHLTV)
            {
                string team = player.Team == CsTeam.Terrorist ? "T" : "CT";
                string centerMessage = FormatCenterMessage(
                    Localization.DuelStartMessageWithSpawn,
                    _currentDuelCombo.ComboName,
                    team,
                    spawnIndex,
                    tCount,
                    ctCount
                );
                player.PrintToCenter(centerMessage);
                
                if (player.Team == CsTeam.Terrorist)
                {
                    player.PrintToChat($"{ChatColors.LightBlue}{string.Format(Localization.YouAreSpawnT, spawnIndex)}");
                }
                else
                {
                    player.PrintToChat($"{ChatColors.LightBlue}{string.Format(Localization.YouAreSpawnCT, spawnIndex)}");
                }
            }
        }
        
        // Center screen message for players not participating in the duel (spectators)
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV && !spawnIndices.ContainsKey(p)))
        {
            string startMessage = FormatCenterMessage(
                Localization.DuelStartMessage,
                _currentDuelCombo.ComboName,
                null,
                null,
                tCount,
                ctCount
            );
            player.PrintToCenter(startMessage);
        }
    }

    /// <summary>
    /// Formats a message for the center of the screen by replacing placeholders
    /// </summary>
    private string FormatCenterMessage(string template, string comboName, string? team, int? spawnIndex, int tCount, int ctCount)
    {
        string message = template
            .Replace("{comboName}", comboName)
            .Replace("{tCount}", tCount.ToString())
            .Replace("{ctCount}", ctCount.ToString());
        
        if (team != null)
        {
            message = message.Replace("{team}", team);
        }
        
        if (spawnIndex.HasValue)
        {
            message = message.Replace("{spawnIndex}", spawnIndex.Value.ToString());
        }
        
        return message;
    }

    private Dictionary<CCSPlayerController, int> RespawnAndTeleportPlayers(List<CCSPlayerController> players, DuelCombination combo)
    {
        var spawnIndices = new Dictionary<CCSPlayerController, int>();
    
        // Get valid spawns shuffled
        var validTSpawns = SpawnHelper.GetValidSpawns(combo.TSpawns).OrderBy(_ => _random.Next()).ToList();
        var validCTSpawns = SpawnHelper.GetValidSpawns(combo.CTSpawns).OrderBy(_ => _random.Next()).ToList();

        var tPlayers = players.Where(p => p.Team == CsTeam.Terrorist).OrderBy(_ => _random.Next()).ToList();
        var ctPlayers = players.Where(p => p.Team == CsTeam.CounterTerrorist).OrderBy(_ => _random.Next()).ToList();

        // Process T players
        ProcessTeamPlayers(tPlayers, validTSpawns, combo.TSpawns, spawnIndices);
        
        // Process CT players
        ProcessTeamPlayers(ctPlayers, validCTSpawns, combo.CTSpawns, spawnIndices);
            
        // Teleport all players to their spawns
        TeleportPlayersToSpawns(spawnIndices, combo);
        
        return spawnIndices;
    }

    /// <summary>
    /// Processes players for a team (T or CT) - respawns and assigns spawn indices
    /// </summary>
    private void ProcessTeamPlayers(
        List<CCSPlayerController> teamPlayers, 
        List<Models.SpawnPoint> validSpawns, 
        List<Models.SpawnPoint> allSpawns,
        Dictionary<CCSPlayerController, int> spawnIndices)
    {
        for (int i = 0; i < teamPlayers.Count && i < validSpawns.Count; i++)
        {
            var player = teamPlayers[i];
            var spawn = validSpawns[i];
            
            int displayIndex = SpawnHelper.GetDisplayIndex(spawn, allSpawns);
            
            if (!player.PawnIsAlive) 
            {
                player.Respawn();
                _pluginInstance.AddTimer(0.05f, () =>
                {
                    if (player.IsValid && !player.IsBot && !player.IsHLTV)
                    {
                        RestorePlayerHealth(player);
                        GiveDuelEquipment(player);
                    }
                });
            }
            else
            {
                UpdatePlayerModel(player);
            }
            spawnIndices[player] = displayIndex;
        }
    }

    /// <summary>
    /// Teleports players to their assigned spawn points
    /// </summary>
    private void TeleportPlayersToSpawns(Dictionary<CCSPlayerController, int> spawnIndices, DuelCombination combo)
    {
        foreach (var kvp in spawnIndices)
        {
            var player = kvp.Key;
            var spawnIndex = kvp.Value;
            
            if (!player.IsValid || player.IsBot || player.IsHLTV) continue;
            
            Models.SpawnPoint? targetSpawn = null;
            var spawnsToSearch = player.Team == CsTeam.Terrorist ? combo.TSpawns : combo.CTSpawns;
            var validSpawns = SpawnHelper.GetValidSpawns(spawnsToSearch);
            
            if (spawnIndex > 0 && spawnIndex <= validSpawns.Count)
            {
                targetSpawn = validSpawns[spawnIndex - 1];
            }
            
            if (targetSpawn != null)
            {
                TeleportManager.TeleportPlayerToSpawn(player, targetSpawn);
            }
        }
    }

    /// <summary>
    /// Restores player health and armor to full
    /// </summary>
    private void RestorePlayerHealth(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.PlayerPawn?.Value == null) return;

        var pawn = player.PlayerPawn.Value;
        if (!pawn.IsValid) return;

        // Set health to 100 (restore even if player is already alive)
        pawn.Health = 100;
        pawn.MaxHealth = 100;

        // Restore armor if kevlar is given
        if (Settings.GiveKevlar)
        {
            pawn.ArmorValue = 100; // Full armor
        }
        else
        {
            pawn.ArmorValue = 0;
        }
        
        // Ensure CT players never have the bomb (in case game gives it automatically)
        RemoveBombFromPlayer(player);
    }

    private void GiveDuelEquipment(CCSPlayerController player)
    {
        player.RemoveWeapons();

        // Primary weapon based on team
        if (player.Team == CsTeam.Terrorist)
            player.GiveNamedItem(Settings.TerroristPrimaryWeapon);
        else if (player.Team == CsTeam.CounterTerrorist)
            player.GiveNamedItem(Settings.CTerroristPrimaryWeapon);

        // Optional equipment based on config
        if (Settings.GiveHelmet)
            player.GiveNamedItem(CsItem.KevlarHelmet);
        if (Settings.GiveKevlar)
            player.GiveNamedItem(CsItem.Kevlar);
        
        player.GiveNamedItem(CsItem.Knife);
        
        if (Settings.GiveDeagle)
            player.GiveNamedItem(CsItem.Deagle);
        if (Settings.GiveHEGrenade)
            player.GiveNamedItem(CsItem.HE);
        if (Settings.GiveFlashbang)
            player.GiveNamedItem(CsItem.Flashbang);
        
        // Ensure CT players never have the bomb
        RemoveBombFromPlayer(player);
    }
    
    /// <summary>
    /// Removes C4 bomb from CT players (bomb should only be on T players)
    /// </summary>
    private void RemoveBombFromPlayer(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.PlayerPawn?.Value == null) return;
        
        var pawn = player.PlayerPawn.Value;
        if (!pawn.IsValid || pawn.WeaponServices == null) return;
        
        // Remove C4 bomb if player has it
        var weapons = pawn.WeaponServices.MyWeapons;
        if (weapons == null) return;
        
        foreach (var weaponHandle in weapons)
        {
            if (weaponHandle == null || !weaponHandle.IsValid) continue;
            
            var weapon = weaponHandle.Value;
            if (weapon == null || !weapon.IsValid) continue;
            
            // Check if it's a C4 bomb
            if (weapon.DesignerName.Contains("c4", StringComparison.OrdinalIgnoreCase))
            {
                weapon.Remove();
            }
        }
    }

    /// <summary>
    /// Updates player model to match their current team by forcing a model refresh
    /// For alive players, we use ChangeTeam which kills and respawns them instantly to update the model
    /// </summary>
    private void UpdatePlayerModel(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.PlayerPawn?.Value == null) return;

        // If player is alive and on a playable team, we need to force model update
        // The only reliable way is to use ChangeTeam which will kill and respawn the player
        // This is almost instantaneous and the player won't notice
        if (player.PawnIsAlive && (player.Team == CsTeam.Terrorist || player.Team == CsTeam.CounterTerrorist))
        {
            var currentTeam = player.Team;
            // ChangeTeam kills the player, then we respawn immediately
            player.ChangeTeam(currentTeam);
            if (player.IsValid && !player.IsBot && !player.IsHLTV)
            {
                player.Respawn();
                if (player.IsValid && !player.IsBot && !player.IsHLTV)
                {
                    RestorePlayerHealth(player);
                }
            }
        }

        GiveDuelEquipment(player);
    }

    /// <summary>
    /// Removes all weapons on the ground (dropped weapons)
    /// </summary>
    private void RemoveDroppedWeapons()
    {
        var entities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("weapon_");
        foreach (var entity in entities)
        {
            if (entity == null || !entity.IsValid) continue;
            
            // Check if weapon has no owner (on the ground)
            if (entity.OwnerEntity == null || !entity.OwnerEntity.IsValid)
            {
                entity.Remove();
            }
        }
    }

    /// <summary>
    /// Selects players for the duel based on available spawns and balances teams
    /// </summary>
    private List<CCSPlayerController> GetDuelPlayers(DuelCombination combo)
    {
        var allPlayers = Utilities.GetPlayers()
            .Where(p => p.IsValid && !p.IsHLTV)
            .ToList();

        var tSpawns = SpawnHelper.GetValidSpawns(combo.TSpawns);
        var ctSpawns = SpawnHelper.GetValidSpawns(combo.CTSpawns);

        int maxTSpawns = tSpawns.Count;
        int maxCTSpawns = ctSpawns.Count;
        int maxTotalPlayers = maxTSpawns + maxCTSpawns;

        var shuffledPlayers = allPlayers.OrderBy(_ => _random.Next()).ToList();
        var selectedPlayers = shuffledPlayers.Take(maxTotalPlayers).ToList();

        BalanceTeams(selectedPlayers, maxTSpawns, maxCTSpawns);

        foreach (var player in shuffledPlayers.Skip(maxTotalPlayers))
        {
            if (player.Team != CsTeam.Spectator)
                player.ChangeTeam(CsTeam.Spectator);
        }

        return selectedPlayers;
    }

    /// <summary>
    /// Balances teams by assigning players to T and CT based on available spawns
    /// </summary>
    private void BalanceTeams(List<CCSPlayerController> players, int maxTSpawns, int maxCTSpawns)
    {
        if (players.Count == 0) return;

        var shuffledPlayers = players.OrderBy(_ => _random.Next()).ToList();
        int maxAssignable = Math.Min(shuffledPlayers.Count, maxTSpawns + maxCTSpawns);

        if (shuffledPlayers.Count < 2)
        {
            if (shuffledPlayers.Count == 1 && maxTSpawns > 0)
                shuffledPlayers[0].SwitchTeam(CsTeam.Terrorist);
            return;
        }

        shuffledPlayers[0].SwitchTeam(CsTeam.Terrorist);
        shuffledPlayers[1].SwitchTeam(CsTeam.CounterTerrorist);

        int tCount = 1, ctCount = 1;

        for (int i = 2; i < maxAssignable; i++)
        {
            bool assignToT = _random.Next(2) == 0;
            
            if (assignToT && tCount < maxTSpawns)
            {
                shuffledPlayers[i].SwitchTeam(CsTeam.Terrorist);
                tCount++;
            }
            else if (!assignToT && ctCount < maxCTSpawns)
            {
                shuffledPlayers[i].SwitchTeam(CsTeam.CounterTerrorist);
                ctCount++;
            }
            else if (tCount < maxTSpawns)
            {
                shuffledPlayers[i].SwitchTeam(CsTeam.Terrorist);
                tCount++;
            }
            else if (ctCount < maxCTSpawns)
            {
                shuffledPlayers[i].SwitchTeam(CsTeam.CounterTerrorist);
                ctCount++;
            }
            else
            {
                shuffledPlayers[i].ChangeTeam(CsTeam.Spectator);
            }
        }

        for (int i = maxAssignable; i < shuffledPlayers.Count; i++)
            shuffledPlayers[i].ChangeTeam(CsTeam.Spectator);
    }
}
