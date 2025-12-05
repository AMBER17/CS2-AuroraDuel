using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using AuroraDuel.Models;

namespace AuroraDuel.Managers;

public class DuelGameManager
{
    private readonly ConfigManager _configManager;
    private readonly SettingsManager _settingsManager;
    private readonly Random _random = new Random();
    private readonly BasePlugin _pluginInstance;
    private DuelCombination? _currentDuelCombo = null;
    private List<CCSPlayerController>? _currentDuelPlayers = null;
    private bool _isDuelInProgress = false;
    private bool _isGameStarted = false;

    public static bool IsConfigModeActive { get; private set; } = false;

    // Raccourci pour accéder aux settings
    private PluginSettings Settings => _settingsManager.Settings;

    /// <summary>
    /// Vérifie si on est dans un vrai round de jeu (pas warmup).
    /// </summary>
    private bool IsValidGameRound()
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;
        if (gameRules == null) return true;
        return !gameRules.WarmupPeriod;
    }

    public DuelGameManager(ConfigManager configManager, SettingsManager settingsManager, BasePlugin pluginInstance)
    {
        _configManager = configManager;
        _settingsManager = settingsManager;
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
            Server.PrintToChatAll($"{ChatColors.Green}[DUEL CONFIG] {ChatColors.Default}Mode configuration ACTIF.");
        }
        else
        {
            Server.ExecuteCommand("sv_cheats 0");
            // Recharger la config des duels depuis le fichier
            _configManager.LoadConfig();
            Server.ExecuteCommand("mp_restartgame 1");
            _pluginInstance.AddTimer(1.0f, () => 
            {
                LoadDuelConfigs(_pluginInstance);
                // Réinitialiser l'état du jeu pour qu'il redémarre
                _isGameStarted = false;
                _isDuelInProgress = false;
                
                // Vérifier s'il y a déjà des joueurs en T/CT pour démarrer le jeu
                CheckAndStartGameIfPlayersPresent();
            });
            Server.PrintToChatAll($"{ChatColors.Red}[DUEL CONFIG] {ChatColors.Default}Mode configuration INACTIF.");
        }
    }

    public void RegisterEvents(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        // Mode Pre pour pouvoir bloquer le broadcast du message de changement d'équipe
        plugin.RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam, HookMode.Pre);
        plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        
        if (Settings.EnableDebugMessages)
            Console.WriteLine("[AuroraDuel] En attente d'un joueur en T ou CT...");
    }

    private void OnMapStart(string mapName)
    {
        _isGameStarted = false;
        _isDuelInProgress = false;
        _currentDuelCombo = null;
        _currentDuelPlayers = null;
        
        if (Settings.EnableDebugMessages)
            Console.WriteLine($"[AuroraDuel] Nouvelle carte : {mapName}. En attente d'un joueur en T ou CT...");
    }

    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        // Masquer le message de changement d'équipe (mode Pre requis)
        if (Settings.HideTeamChangeMessages)
        {
            info.DontBroadcast = true;
            @event.Silent = true;
        }

        if (IsConfigModeActive) return HookResult.Continue;

        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsHLTV) return HookResult.Continue;

        // Ignorer les bots pour la logique de démarrage
        if (player.IsBot) return HookResult.Continue;

        int newTeam = @event.Team;
        bool joinsPlayableTeam = newTeam == (int)CsTeam.Terrorist || newTeam == (int)CsTeam.CounterTerrorist;

        if (!_isGameStarted && joinsPlayableTeam)
        {
            _isGameStarted = true;
            
            if (Settings.EnableDebugMessages)
                Console.WriteLine($"[AuroraDuel] Joueur détecté en équipe : {player.PlayerName}");

            _pluginInstance.AddTimer(1.0f, () =>
            {
                LoadDuelConfigs(_pluginInstance);
                Server.ExecuteCommand("mp_restartgame 1");
            });
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
            
            if (Settings.EnableDebugMessages)
                Console.WriteLine("[AuroraDuel] Plus aucun joueur en T ou CT. En attente d'un joueur...");
        }
    }

    /// <summary>
    /// Vérifie s'il y a des joueurs en T/CT et démarre le jeu si nécessaire.
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
            
            if (Settings.EnableDebugMessages)
                Console.WriteLine($"[AuroraDuel] Joueurs détectés après sortie du mode config. Démarrage du jeu...");

            _pluginInstance.AddTimer(1.0f, () =>
            {
                LoadDuelConfigs(_pluginInstance);
                Server.ExecuteCommand("mp_restartgame 1");
            });
        }
    }

    public static void LoadDuelConfigs(BasePlugin plugin)
    {
        string configFilePath = Path.Combine(plugin.ModuleDirectory, "configs", "duel_settings.cfg");

        if (!File.Exists(configFilePath))
        {
            Console.WriteLine($"[AuroraDuel] Attention: Fichier de configuration non trouvé : {configFilePath}");
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
            Console.WriteLine($"[AuroraDuel] Erreur lors du chargement de la config : {ex.Message}");
        }
    }

    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (IsConfigModeActive || !_isGameStarted) return HookResult.Continue;

        if (!IsValidGameRound())
        {
            if (Settings.EnableDebugMessages)
                Console.WriteLine("[AuroraDuel] Round ignoré (warmup ou intro).");
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
            
            // Message au centre de l'écran
            string winMessage = Settings.DuelWinMessage.Replace("{winnerTeam}", winnerTeam);
            foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV))
            {
                player.PrintToCenter(winMessage);
            }
            
            _pluginInstance.AddTimer(Settings.DelayBeforeNextDuel, StartNextDuel);
        }
    }

    private void StartNextDuel()
    {
        if (!_isGameStarted) return;
        
        _isDuelInProgress = false;

        // Nettoyer les armes au sol
        RemoveDroppedWeapons();

        var currentMapName = Server.MapName;
        var availableCombos = _configManager.CurrentConfig.Combos
            .Where(c => c.MapName.Equals(currentMapName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (availableCombos.Count == 0)
        {
            Server.PrintToChatAll($"{ChatColors.Red}[AuroraDuel] Aucune combinaison de duel pour la carte {currentMapName}.");
            return;
        }

        _currentDuelCombo = availableCombos[_random.Next(availableCombos.Count)];
        _currentDuelPlayers = GetDuelPlayers(_currentDuelCombo);

        if (_currentDuelPlayers.Count == 0)
        {
            Server.PrintToChatAll($"{ChatColors.Red}[AuroraDuel] Aucun joueur disponible pour le duel.");
            return;
        }

        int tCount = _currentDuelPlayers.Count(p => p.Team == CsTeam.Terrorist);
        int ctCount = _currentDuelPlayers.Count(p => p.Team == CsTeam.CounterTerrorist);

        if (tCount == 0 || ctCount == 0)
        {
            Server.PrintToChatAll($"{ChatColors.Red}[AuroraDuel] Pas assez de joueurs (minimum 1 T et 1 CT requis).");
            return;
        }

        var spawnIndices = RespawnAndTeleportPlayers(_currentDuelPlayers, _currentDuelCombo);
        
        _isDuelInProgress = true;
        
        // Message dans le chat (général)
        string chatMessage = Settings.DuelStartChatMessage
            .Replace("{comboName}", _currentDuelCombo.ComboName)
            .Replace("{tCount}", tCount.ToString())
            .Replace("{ctCount}", ctCount.ToString());
        Server.PrintToChatAll(chatMessage);
        
        // Message personnalisé au centre de l'écran pour chaque joueur avec son index de spawn
        foreach (var kvp in spawnIndices)
        {
            var player = kvp.Key;
            var spawnIndex = kvp.Value;
            
            if (player.IsValid && !player.IsBot && !player.IsHLTV)
            {
                string team = player.Team == CsTeam.Terrorist ? "T" : "CT";
                string centerMessage = FormatCenterMessage(
                    Settings.DuelStartMessageWithSpawn,
                    _currentDuelCombo.ComboName,
                    team,
                    spawnIndex,
                    tCount,
                    ctCount
                );
                player.PrintToCenter(centerMessage);
            }
        }
        
        // Message au centre de l'écran pour les joueurs qui ne participent pas au duel (spectateurs)
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV && !spawnIndices.ContainsKey(p)))
        {
            string startMessage = FormatCenterMessage(
                Settings.DuelStartMessage,
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
    /// Formate un message pour le centre de l'écran en remplaçant les placeholders.
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
        
        // Obtenir les spawns valides avec leurs index originaux
        var tSpawnsWithIndex = combo.TSpawns
            .Select((spawn, index) => new { Spawn = spawn, OriginalIndex = index })
            .Where(x => x.Spawn != null && (x.Spawn.PosX != 0 || x.Spawn.PosY != 0 || x.Spawn.PosZ != 0))
            .OrderBy(_ => _random.Next())
            .ToList();

        var ctSpawnsWithIndex = combo.CTSpawns
            .Select((spawn, index) => new { Spawn = spawn, OriginalIndex = index })
            .Where(x => x.Spawn != null && (x.Spawn.PosX != 0 || x.Spawn.PosY != 0 || x.Spawn.PosZ != 0))
            .OrderBy(_ => _random.Next())
            .ToList();

        var tPlayers = players.Where(p => p.Team == CsTeam.Terrorist).OrderBy(_ => _random.Next()).ToList();
        var ctPlayers = players.Where(p => p.Team == CsTeam.CounterTerrorist).OrderBy(_ => _random.Next()).ToList();

        // Trouver l'index original dans la liste non filtrée pour les T
        for (int i = 0; i < tPlayers.Count && i < tSpawnsWithIndex.Count; i++)
        {
            var player = tPlayers[i];
            var spawnData = tSpawnsWithIndex[i];
            
            // Trouver l'index dans la liste complète (avec les spawns invalides)
            int originalIndex = combo.TSpawns.IndexOf(spawnData.Spawn);
            int displayIndex = combo.TSpawns
                .Take(originalIndex + 1)
                .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
            
            if (!player.PawnIsAlive) player.Respawn();
            GiveDuelEquipment(player);
            TeleportManager.TeleportPlayerToSpawn(player, spawnData.Spawn);
            spawnIndices[player] = displayIndex;
        }

        // Trouver l'index original dans la liste non filtrée pour les CT
        for (int i = 0; i < ctPlayers.Count && i < ctSpawnsWithIndex.Count; i++)
        {
            var player = ctPlayers[i];
            var spawnData = ctSpawnsWithIndex[i];
            
            // Trouver l'index dans la liste complète (avec les spawns invalides)
            int originalIndex = combo.CTSpawns.IndexOf(spawnData.Spawn);
            int displayIndex = combo.CTSpawns
                .Take(originalIndex + 1)
                .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
            
            if (!player.PawnIsAlive) player.Respawn();
            GiveDuelEquipment(player);
            TeleportManager.TeleportPlayerToSpawn(player, spawnData.Spawn);
            spawnIndices[player] = displayIndex;
        }
        
        return spawnIndices;
    }

    private void GiveDuelEquipment(CCSPlayerController player)
    {
        player.RemoveWeapons();

        // Arme principale selon l'équipe
        if (player.Team == CsTeam.Terrorist)
            player.GiveNamedItem(Settings.TerroristPrimaryWeapon);
        else if (player.Team == CsTeam.CounterTerrorist)
            player.GiveNamedItem(Settings.CTerroristPrimaryWeapon);

        // Équipement optionnel selon la config
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
    }

    /// <summary>
    /// Supprime toutes les armes au sol (dropped weapons).
    /// </summary>
    private void RemoveDroppedWeapons()
    {
        // Liste des préfixes d'armes à nettoyer
        var weaponPrefixes = new[] { "weapon_" };
        
        foreach (var prefix in weaponPrefixes)
        {
            var entities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(prefix);
            foreach (var entity in entities)
            {
                if (entity == null || !entity.IsValid) continue;
                
                // Vérifier si l'arme n'a pas de propriétaire (donc au sol)
                if (entity.OwnerEntity == null || !entity.OwnerEntity.IsValid)
                {
                    entity.Remove();
                }
            }
        }
        
        if (Settings.EnableDebugMessages)
            Console.WriteLine("[AuroraDuel] Armes au sol supprimées.");
    }

    private List<CCSPlayerController> GetDuelPlayers(DuelCombination combo)
    {
        var allPlayers = Utilities.GetPlayers()
            .Where(p => p.IsValid && !p.IsHLTV)
            .ToList();

        var tSpawns = combo.TSpawns
            .Where(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0))
            .ToList();
        var ctSpawns = combo.CTSpawns
            .Where(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0))
            .ToList();

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
