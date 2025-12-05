using AuroraDuel.Managers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;

namespace AuroraDuel.Commands;

public class DuelCommands
{
    private readonly ConfigManager _configManager;
    private readonly DuelGameManager? _duelGameManager;
    private readonly SettingsManager? _settingsManager;

    public DuelCommands(ConfigManager configManager, DuelGameManager? duelGameManager = null, SettingsManager? settingsManager = null)
    {
        _configManager = configManager;
        _duelGameManager = duelGameManager;
        _settingsManager = settingsManager;
    }

    public void RegisterCommands(BasePlugin plugin)
    {
        plugin.AddCommand("duel_add_t", "duel_add_t <NomUniqueDuDuel>", Command_AddTSpawn);
        plugin.AddCommand("duel_add_ct", "duel_add_ct <NomUniqueDuDuel>", Command_AddCTSpawn);
        plugin.AddCommand("duel_remove_t_spawn", "duel_remove_t_spawn <NomDuel> <index>", Command_RemoveTSpawn);
        plugin.AddCommand("duel_remove_ct_spawn", "duel_remove_ct_spawn <NomDuel> <index>", Command_RemoveCTSpawn);
        plugin.AddCommand("duel_info", "duel_info <NomDuel>", Command_DuelInfo);
        plugin.AddCommand("duel_config", "Active/Désactive le mode configuration. Usage: !duel_config [on|off]", Command_ToggleConfigMode);
        plugin.AddCommand("duel_map", "Change la carte. Usage: !duel_map <nom_de_la_carte>", Command_ChangeMap);
        plugin.AddCommand("duel_reload", "Recharge les paramètres du plugin", Command_ReloadSettings);
        plugin.AddCommand("duel_list", "Liste tous les duels de la carte actuelle", Command_ListDuels);
        plugin.AddCommand("duel_delete", "Supprime un duel. Usage: !duel_delete <NomDuDuel>", Command_DeleteDuel);
        plugin.AddCommand("duel_help", "Affiche la liste de toutes les commandes disponibles", Command_Help);
    }

    [RequiresPermissions("@css/root")]
    public void Command_ReloadSettings(CCSPlayerController? player, CommandInfo info)
    {
        if (_settingsManager == null)
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Erreur: SettingsManager non disponible.";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        _settingsManager.ReloadSettings();
        string successMessage = $"{ChatColors.Green}[AuroraDuel] {ChatColors.Default}Paramètres {ChatColors.Green}rechargés avec succès{ChatColors.Default} !";
        if (player != null && player.IsValid)
            player.PrintToChat(successMessage);
        else
            info.ReplyToCommand(successMessage);
    }

    [RequiresPermissions("@css/root")] // Utilisez @duels/changemap si vous avez configuré vos permissions
    public void Command_ChangeMap(CCSPlayerController? player, CommandInfo info)
    {
        // 1. Vérification des arguments
        if (info.ArgCount < 2)
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Usage: {ChatColors.Yellow}!duel_map <nom_de_la_carte>";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        string newMapName = info.GetArg(1).Trim();

        // 2. Vérification que le nom de la carte n'est pas vide
        if (string.IsNullOrWhiteSpace(newMapName))
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Le nom de la carte ne peut pas être vide.";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        // 3. Exécution du changement de carte
        Server.PrintToChatAll($"{ChatColors.LightBlue}[AuroraDuel] {ChatColors.Default}Changement de carte imminent vers : {ChatColors.Yellow}{newMapName}{ChatColors.Default}...");

        // Utiliser CHANGELEVEL est souvent plus propre que MAP dans un plugin
        Server.ExecuteCommand($"changelevel {newMapName}");

        // Note : Si la carte n'existe pas, changelevel est plus tolérant et ne crashe pas.
    }

    /// <summary>
    /// Active ou désactive le mode configuration.
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_ToggleConfigMode(CCSPlayerController? player, CommandInfo info)
    {
        bool newState;

        // Déterminer l'état souhaité (on, off, ou toggle si aucun argument)
        if (info.ArgCount < 2)
        {
            // Basculer l'état actuel
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
                string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Argument invalide. Utilisation: {ChatColors.Yellow}!duel_config [on|off]";
                if (player != null && player.IsValid)
                    player.PrintToChat(message);
                else
                    info.ReplyToCommand(message);
                return;
            }
        }

        // Appliquer l'état
        if (_duelGameManager != null)
        {
            _duelGameManager.SetConfigMode(newState);
            string status = newState ? $"{ChatColors.Green}ACTIF" : $"{ChatColors.Red}INACTIF";
            string message = $"{ChatColors.LightBlue}[AuroraDuel] {ChatColors.Default}Mode configuration: {status}{ChatColors.Default}";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
        }
        else
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Erreur: DuelGameManager n'est pas disponible.";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
        }
    }

    private SpawnPoint GetCurrentSpawnPoint(CCSPlayerController player)
    {
        if (player.Pawn?.Value == null)
        {
            throw new InvalidOperationException("Impossible d'obtenir la position: Pawn est invalide.");
        }

        var pawn = player.Pawn.Value;
        var origin = pawn.AbsOrigin!;
        var angle = pawn.AbsRotation!;

        return new SpawnPoint
        {
            PosX = origin.X,
            PosY = origin.Y,
            PosZ = origin.Z,
            AngleYaw = angle.Y
        };
    }

    // --- Fonctions d'ajout de spawns ---

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
    /// Ajoute un spawn à la liste T ou CT d'un duel.
    /// </summary>
    private void AddSpawn(CCSPlayerController? player, CommandInfo info, bool isTerrorist)
    {
        if (info.ArgCount < 2)
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Usage: {ChatColors.Yellow}{info.GetArg(0)} <NomUniqueDuDuel>";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        if (player == null || !player.IsValid)
        {
            info.ReplyToCommand($"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Joueur invalide.");
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
                TSpawns = new List<SpawnPoint>(),
                CTSpawns = new List<SpawnPoint>()
            };
            _configManager.CurrentConfig.Combos.Add(combo);
            player.PrintToChat($"{ChatColors.Green}[AuroraDuel] {ChatColors.Default}Nouvelle combinaison {ChatColors.Yellow}'{comboName}' {ChatColors.Default}créée sur la carte {ChatColors.LightBlue}{mapName}{ChatColors.Default}.");
        }

        var spawnPoint = GetCurrentSpawnPoint(player);
        if (isTerrorist)
        {
            combo.TSpawns.Add(spawnPoint);
            player.PrintToChat($"{ChatColors.Green}[AuroraDuel] {ChatColors.Default}Spawn {ChatColors.Red}T {ChatColors.Default}ajouté pour {ChatColors.Yellow}'{comboName}' {ChatColors.Default}({ChatColors.LightBlue}{combo.TSpawns.Count} {ChatColors.Red}T {ChatColors.Default}au total).");
        }
        else
        {
            combo.CTSpawns.Add(spawnPoint);
            player.PrintToChat($"{ChatColors.Green}[AuroraDuel] {ChatColors.Default}Spawn {ChatColors.LightBlue}CT {ChatColors.Default}ajouté pour {ChatColors.Yellow}'{comboName}' {ChatColors.Default}({ChatColors.LightBlue}{combo.CTSpawns.Count} {ChatColors.LightBlue}CT {ChatColors.Default}au total).");
        }

        _configManager.SaveConfig();
        // Note: Les modifications sont directement dans _configManager.CurrentConfig qui est partagé avec DuelGameManager
        // donc elles sont immédiatement disponibles pour la logique de jeu
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
    /// Supprime un spawn à la liste T ou CT d'un duel.
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
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}L'index doit être un nombre entier positif (commence à 1).";
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
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Duel {ChatColors.Yellow}'{comboName}' {ChatColors.Default}introuvable sur la carte {ChatColors.LightBlue}{mapName}{ChatColors.Default}.";
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
            string teamNameColored = isTerrorist ? $"{ChatColors.Red}T" : $"{ChatColors.LightBlue}CT";
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Index {ChatColors.Yellow}{index} {ChatColors.Default}invalide. Il n'y a que {ChatColors.LightBlue}{validSpawns.Count} {ChatColors.Default}spawn(s) {teamNameColored}{ChatColors.Default} valide(s).";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        // Trouver le spawn dans la liste complète (avec les spawns invalides)
        var spawnToRemove = validSpawns[index - 1];
        spawns.Remove(spawnToRemove);

        string teamName = isTerrorist ? "T" : "CT";
        string successMessage = isTerrorist
            ? $"{ChatColors.Green}[AuroraDuel] {ChatColors.Default}Spawn {ChatColors.Red}{teamName} {ChatColors.Default}index {ChatColors.Yellow}{index} {ChatColors.Green}supprimé {ChatColors.Default}du duel {ChatColors.Yellow}'{comboName}'{ChatColors.Default}."
            : $"{ChatColors.Green}[AuroraDuel] {ChatColors.Default}Spawn {ChatColors.LightBlue}{teamName} {ChatColors.Default}index {ChatColors.Yellow}{index} {ChatColors.Green}supprimé {ChatColors.Default}du duel {ChatColors.Yellow}'{comboName}'{ChatColors.Default}.";
        if (player != null && player.IsValid)
            player.PrintToChat(successMessage);
        else
            info.ReplyToCommand(successMessage);

        _configManager.SaveConfig();
    }

    /// <summary>
    /// Affiche les détails d'un duel.
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_DuelInfo(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Usage: {ChatColors.Yellow}!duel_info <NomDuel>";
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
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Duel {ChatColors.Yellow}'{comboName}' {ChatColors.Default}introuvable sur la carte {ChatColors.LightBlue}{mapName}{ChatColors.Default}.";
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
            player.PrintToChat($" {ChatColors.LightBlue}[AuroraDuel] {ChatColors.Yellow}=== Informations du duel '{comboName}' ===");
            player.PrintToChat($" {ChatColors.Green}Carte: {ChatColors.LightBlue}{mapName}");
            player.PrintToChat($" {ChatColors.Green}Spawns {ChatColors.Red}T: {ChatColors.LightBlue}{validTSpawns.Count}");
            player.PrintToChat($" {ChatColors.Green}Spawns {ChatColors.LightBlue}CT: {ChatColors.LightBlue}{validCTSpawns.Count}");
            
            if (validTSpawns.Count > 0)
            {
                player.PrintToChat($" {ChatColors.Red}Spawns T:");
                for (int i = 0; i < validTSpawns.Count; i++)
                {
                    var spawn = validTSpawns[i];
                    player.PrintToChat($"   {ChatColors.Default}{i + 1}. {ChatColors.Yellow}X: {spawn.PosX:F1} {ChatColors.Default}Y: {ChatColors.Yellow}{spawn.PosY:F1} {ChatColors.Default}Z: {ChatColors.Yellow}{spawn.PosZ:F1}");
                }
            }
            
            if (validCTSpawns.Count > 0)
            {
                player.PrintToChat($" {ChatColors.LightBlue}Spawns CT:");
                for (int i = 0; i < validCTSpawns.Count; i++)
                {
                    var spawn = validCTSpawns[i];
                    player.PrintToChat($"   {ChatColors.Default}{i + 1}. {ChatColors.Yellow}X: {spawn.PosX:F1} {ChatColors.Default}Y: {ChatColors.Yellow}{spawn.PosY:F1} {ChatColors.Default}Z: {ChatColors.Yellow}{spawn.PosZ:F1}");
                }
            }
        }
        else
        {
            // Console output
            Console.WriteLine($"[AuroraDuel] === Informations du duel '{comboName}' ===");
            Console.WriteLine($"Carte: {mapName}");
            Console.WriteLine($"Spawns T: {validTSpawns.Count}");
            Console.WriteLine($"Spawns CT: {validCTSpawns.Count}");
            
            if (validTSpawns.Count > 0)
            {
                Console.WriteLine("Spawns T:");
                for (int i = 0; i < validTSpawns.Count; i++)
                {
                    var spawn = validTSpawns[i];
                    Console.WriteLine($"  {i + 1}. X: {spawn.PosX:F1} Y: {spawn.PosY:F1} Z: {spawn.PosZ:F1}");
                }
            }
            
            if (validCTSpawns.Count > 0)
            {
                Console.WriteLine("Spawns CT:");
                for (int i = 0; i < validCTSpawns.Count; i++)
                {
                    var spawn = validCTSpawns[i];
                    Console.WriteLine($"  {i + 1}. X: {spawn.PosX:F1} Y: {spawn.PosY:F1} Z: {spawn.PosZ:F1}");
                }
            }
        }
    }

    /// <summary>
    /// Liste tous les duels de la carte actuelle.
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
            string message = $"{ChatColors.Yellow}[AuroraDuel] {ChatColors.Default}Aucun duel configuré sur la carte {ChatColors.LightBlue}{currentMap}{ChatColors.Default}.";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        if (player != null && player.IsValid)
        {
            player.PrintToChat($" {ChatColors.LightBlue}[AuroraDuel] {ChatColors.Default}{ChatColors.Yellow}{duelsOnMap.Count} {ChatColors.Default}duel(s) sur {ChatColors.LightBlue}{currentMap}{ChatColors.Default}:");
            
            foreach (var duel in duelsOnMap)
            {
                // Compter les spawns configurés
                int tSpawns = duel.TSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                int ctSpawns = duel.CTSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                
                player.PrintToChat($"  {ChatColors.Green}• {ChatColors.Yellow}{duel.ComboName} {ChatColors.Default}({ChatColors.Red}{tSpawns} T {ChatColors.Default}/ {ChatColors.LightBlue}{ctSpawns} CT{ChatColors.Default})");
            }
        }
        else
        {
            // Console output
            foreach (var duel in duelsOnMap)
            {
                int tSpawns = duel.TSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                int ctSpawns = duel.CTSpawns
                    .Count(s => s != null && (s.PosX != 0 || s.PosY != 0 || s.PosZ != 0));
                
                Console.WriteLine($"  • {duel.ComboName} ({tSpawns} T / {ctSpawns} CT)");
            }
        }
    }

    /// <summary>
    /// Supprime un duel de la carte actuelle.
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_DeleteDuel(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Usage: {ChatColors.Yellow}!duel_delete <NomDuDuel>";
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
            string message = $"{ChatColors.Red}[AuroraDuel] {ChatColors.Default}Duel {ChatColors.Yellow}'{comboName}' {ChatColors.Default}introuvable sur la carte {ChatColors.LightBlue}{mapName}{ChatColors.Default}.";
            if (player != null && player.IsValid)
                player.PrintToChat(message);
            else
                info.ReplyToCommand(message);
            return;
        }

        _configManager.CurrentConfig.Combos.Remove(combo);
        _configManager.SaveConfig();

        string successMessage = $"{ChatColors.Green}[AuroraDuel] {ChatColors.Default}Duel {ChatColors.Yellow}'{comboName}' {ChatColors.Green}supprimé avec succès{ChatColors.Default}.";
        if (player != null && player.IsValid)
            player.PrintToChat(successMessage);
        else
            info.ReplyToCommand(successMessage);
    }

    /// <summary>
    /// Affiche l'aide avec toutes les commandes disponibles.
    /// </summary>
    [RequiresPermissions("@css/root")]
    public void Command_Help(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && player.IsValid)
        {
            player.PrintToChat($" {ChatColors.LightBlue}[AuroraDuel] {ChatColors.Yellow}=== Commandes disponibles ===");
            player.PrintToChat($" {ChatColors.Green}Configuration des spawns:");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.Red}!duel_add_t {ChatColors.Yellow}<NomDuel> {ChatColors.Default}- Ajoute un spawn T à votre position");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_add_ct {ChatColors.Yellow}<NomDuel> {ChatColors.Default}- Ajoute un spawn CT à votre position");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.Red}!duel_remove_t_spawn {ChatColors.Yellow}<NomDuel> <index> {ChatColors.Default}- Supprime un spawn T spécifique");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_remove_ct_spawn {ChatColors.Yellow}<NomDuel> <index> {ChatColors.Default}- Supprime un spawn CT spécifique");
            player.PrintToChat($" ");
            player.PrintToChat($" {ChatColors.Green}Gestion des duels:");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_list {ChatColors.Default}- Liste tous les duels de la carte actuelle");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_info {ChatColors.Yellow}<NomDuel> {ChatColors.Default}- Affiche les détails d'un duel");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_delete {ChatColors.Yellow}<NomDuel> {ChatColors.Default}- Supprime un duel de la carte");
            player.PrintToChat($" ");
            player.PrintToChat($" {ChatColors.Green}Autres commandes:");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_config {ChatColors.Yellow}[on|off] {ChatColors.Default}- Active/désactive le mode configuration");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_map {ChatColors.Yellow}<NomCarte> {ChatColors.Default}- Change la carte du serveur");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_reload {ChatColors.Default}- Recharge les paramètres du plugin");
            player.PrintToChat($"  {ChatColors.Default}• {ChatColors.LightBlue}!duel_help {ChatColors.Default}- Affiche cette aide");
            player.PrintToChat($" ");
            player.PrintToChat($" {ChatColors.Yellow}Note: {ChatColors.Default}Toutes les commandes nécessitent la permission {ChatColors.Red}@css/root");
        }
        else
        {
            // Console output
            Console.WriteLine("[AuroraDuel] === Commandes disponibles ===");
            Console.WriteLine("Configuration des spawns:");
            Console.WriteLine("  !duel_add_t <NomDuel> - Ajoute un spawn T à votre position");
            Console.WriteLine("  !duel_add_ct <NomDuel> - Ajoute un spawn CT à votre position");
            Console.WriteLine("  !duel_remove_t_spawn <NomDuel> <index> - Supprime un spawn T spécifique");
            Console.WriteLine("  !duel_remove_ct_spawn <NomDuel> <index> - Supprime un spawn CT spécifique");
            Console.WriteLine("");
            Console.WriteLine("Gestion des duels:");
            Console.WriteLine("  !duel_list - Liste tous les duels de la carte actuelle");
            Console.WriteLine("  !duel_info <NomDuel> - Affiche les détails d'un duel");
            Console.WriteLine("  !duel_delete <NomDuel> - Supprime un duel de la carte");
            Console.WriteLine("");
            Console.WriteLine("Autres commandes:");
            Console.WriteLine("  !duel_config [on|off] - Active/désactive le mode configuration");
            Console.WriteLine("  !duel_map <NomCarte> - Change la carte du serveur");
            Console.WriteLine("  !duel_reload - Recharge les paramètres du plugin");
            Console.WriteLine("  !duel_help - Affiche cette aide");
        }
    }
}