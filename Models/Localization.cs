namespace AuroraDuel.Models;

/// <summary>
/// Localization model containing all translatable strings
/// </summary>
public class Localization
{
    // Plugin messages
    public string PluginLoaded { get; set; } = "[AuroraDuel] Plugin loaded successfully!";
    public string SettingsLoaded { get; set; } = "[AuroraDuel] Settings loaded.";
    public string SettingsReloaded { get; set; } = "[AuroraDuel] Settings reloaded.";
    public string SettingsCreated { get; set; } = "[AuroraDuel] Settings file created with default values.";
    public string SettingsLoadError { get; set; } = "[AuroraDuel] Error loading settings: {0}";
    public string SettingsSaveError { get; set; } = "[AuroraDuel] Error saving settings: {0}";
    public string ConfigLoaded { get; set; } = "[AuroraDuel] {0} combinations loaded.";
    public string ConfigSaved { get; set; } = "[AuroraDuel] {0} combinations saved.";
    public string ConfigCreated { get; set; } = "[AuroraDuel] New duel configuration created.";

    // Command errors
    public string ErrorSettingsManagerNotAvailable { get; set; } = "[AuroraDuel] Error: SettingsManager not available.";
    public string ErrorDuelGameManagerNotAvailable { get; set; } = "[AuroraDuel] Error: DuelGameManager is not available.";
    public string ErrorInvalidPlayer { get; set; } = "[AuroraDuel] Invalid player.";
    public string ErrorInvalidArgument { get; set; } = "[AuroraDuel] Invalid argument. Usage: {0}";
    public string ErrorMapNameEmpty { get; set; } = "[AuroraDuel] Map name cannot be empty.";
    public string ErrorDuelNotFound { get; set; } = "[AuroraDuel] Duel '{0}' not found on map {1}.";
    public string ErrorInvalidIndex { get; set; } = "[AuroraDuel] Index must be a positive integer (starts at 1).";
    public string ErrorIndexOutOfRange { get; set; } = "[AuroraDuel] Index {0} invalid. There are only {1} valid {2} spawn(s).";
    public string ErrorNoDuelsOnMap { get; set; } = "[AuroraDuel] No duels configured on map {0}.";
    public string ErrorNoPlayersAvailable { get; set; } = "[AuroraDuel] No players available for the duel.";
    public string ErrorNotEnoughPlayers { get; set; } = "[AuroraDuel] Not enough players (minimum 1 T and 1 CT required).";
    public string ErrorNoCombinationsForMap { get; set; } = "[AuroraDuel] No duel combinations for map {0}.";

    // Command success messages
    public string ConfigModeActive { get; set; } = "[DUEL CONFIG] Configuration mode ACTIVE.";
    public string ConfigModeInactive { get; set; } = "[DUEL CONFIG] Configuration mode INACTIVE.";
    public string SettingsReloadedSuccess { get; set; } = "[AuroraDuel] Settings reloaded successfully!";
    public string MapChangeImminent { get; set; } = "[AuroraDuel] Map change imminent to: {0}...";
    public string DuelCreated { get; set; } = "[AuroraDuel] New combination '{0}' created on map {1}.";
    public string TSpawnAdded { get; set; } = "[AuroraDuel] T spawn added for '{0}' ({1} T total).";
    public string CTSpawnAdded { get; set; } = "[AuroraDuel] CT spawn added for '{0}' ({1} CT total).";
    public string TSpawnRemoved { get; set; } = "[AuroraDuel] T spawn index {0} removed from duel '{1}'.";
    public string CTSpawnRemoved { get; set; } = "[AuroraDuel] CT spawn index {0} removed from duel '{1}'.";
    public string DuelDeleted { get; set; } = "[AuroraDuel] Duel '{0}' deleted successfully.";

    // Command usage
    public string UsageAddTSpawn { get; set; } = "Usage: {0} <UniqueDuelName>";
    public string UsageAddCTSpawn { get; set; } = "Usage: {0} <UniqueDuelName>";
    public string UsageRemoveTSpawn { get; set; } = "Usage: {0} <DuelName> <index>";
    public string UsageRemoveCTSpawn { get; set; } = "Usage: {0} <DuelName> <index>";
    public string UsageDuelInfo { get; set; } = "Usage: {0} <DuelName>";
    public string UsageDuelDelete { get; set; } = "Usage: {0} <DuelName>";
    public string UsageDuelMap { get; set; } = "Usage: {0} <map_name>";
    public string UsageDuelConfig { get; set; } = "Usage: {0} [on|off]";

    // Duel info
    public string DuelInfoHeader { get; set; } = "=== Duel '{0}' Information ===";
    public string DuelInfoMap { get; set; } = "Map: {0}";
    public string DuelInfoTSpawns { get; set; } = "T Spawns: {0}";
    public string DuelInfoCTSpawns { get; set; } = "CT Spawns: {0}";
    public string DuelInfoTSpawnsList { get; set; } = "T Spawns:";
    public string DuelInfoCTSpawnsList { get; set; } = "CT Spawns:";
    public string DuelInfoSpawnPosition { get; set; } = "  {0}. X: {1:F1} Y: {2:F1} Z: {3:F1}";

    // Duel list
    public string DuelListHeader { get; set; } = "{0} duel(s) on {1}:";
    public string DuelListItem { get; set; } = "  • {0} ({1} T / {2} CT)";

    // Help command
    public string HelpHeader { get; set; } = "=== Available Commands ===";
    public string HelpSpawnConfig { get; set; } = "Spawn Configuration:";
    public string HelpAddTSpawn { get; set; } = "  • {0} <DuelName> - Add a T spawn at your position";
    public string HelpAddCTSpawn { get; set; } = "  • {0} <DuelName> - Add a CT spawn at your position";
    public string HelpRemoveTSpawn { get; set; } = "  • {0} <DuelName> <index> - Remove a specific T spawn";
    public string HelpRemoveCTSpawn { get; set; } = "  • {0} <DuelName> <index> - Remove a specific CT spawn";
    public string HelpDuelManagement { get; set; } = "Duel Management:";
    public string HelpDuelList { get; set; } = "  • {0} - List all duels on current map";
    public string HelpDuelInfo { get; set; } = "  • {0} <DuelName> - Display duel details";
    public string HelpDuelDelete { get; set; } = "  • {0} <DuelName> - Delete a duel from the map";
    public string HelpOtherCommands { get; set; } = "Other Commands:";
    public string HelpDuelConfig { get; set; } = "  • {0} [on|off] - Enable/disable configuration mode";
    public string HelpDuelMap { get; set; } = "  • {0} <MapName> - Change server map";
    public string HelpDuelReload { get; set; } = "  • {0} - Reload plugin settings";
    public string HelpDuelHelp { get; set; } = "  • {0} - Display this help";
    public string HelpNote { get; set; } = "Note: All commands require {0} permission";

    // In-game messages
    public string YouAreSpawnT { get; set; } = "[AuroraDuel] You are spawn T #{0}.";
    public string YouAreSpawnCT { get; set; } = "[AuroraDuel] You are spawn CT #{0}.";
    
    // Config mode status
    public string ConfigModeStatusActive { get; set; } = "[AuroraDuel] Configuration mode: ACTIVE";
    public string ConfigModeStatusInactive { get; set; } = "[AuroraDuel] Configuration mode: INACTIVE";
    
    // Duel messages (moved from PluginSettings)
    /// <summary>
    /// Message displayed at center of screen at the start of a duel (for spectators)
    /// Placeholders: {comboName}, {tCount}, {ctCount}
    /// </summary>
    public string DuelStartMessage { get; set; } = "{comboName}\n{tCount} T vs {ctCount} CT";
    
    /// <summary>
    /// Message displayed at center of screen at the start of a duel for participating players
    /// Placeholders: {comboName}, {team}, {spawnIndex}, {tCount}, {ctCount}
    /// </summary>
    public string DuelStartMessageWithSpawn { get; set; } = "{comboName} - Spawn {team} #{spawnIndex}\n{tCount} T vs {ctCount} CT";
    
    /// <summary>
    /// Message displayed in chat at the start of a duel
    /// Placeholders: {comboName}, {tCount}, {ctCount}
    /// </summary>
    public string DuelStartChatMessage { get; set; } = " \u0004[AuroraDuel] \u0001{comboName} \u0001- \u000F{tCount} T \u0001vs \u000B{ctCount} CT";
    
    /// <summary>
    /// Message displayed at center of screen when a team wins
    /// Placeholders: {winnerTeam}
    /// </summary>
    public string DuelWinMessage { get; set; } = "The {winnerTeam} have won!";
    
}

