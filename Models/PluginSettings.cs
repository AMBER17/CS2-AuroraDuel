namespace AuroraDuel.Models;

/// <summary>
/// Configuration générale du plugin AuroraDuel
/// </summary>
public class PluginSettings
{
    /// <summary>
    /// Délai en secondes avant de lancer le prochain duel après une victoire
    /// </summary>
    public float DelayBeforeNextDuel { get; set; } = 1.0f;

    /// <summary>
    /// Délai en secondes après le début du round avant de lancer le premier duel
    /// </summary>
    public float DelayAfterRoundStart { get; set; } = 2f;

    /// <summary>
    /// Message affiché au centre de l'écran au début d'un duel
    /// Placeholders: {comboName}, {tCount}, {ctCount}
    /// </summary>
    public string DuelStartMessage { get; set; } = "{comboName}\n{tCount} T vs {ctCount} CT";

    /// <summary>
    /// Message affiché au centre de l'écran au début d'un duel pour les joueurs participants
    /// Placeholders: {comboName}, {team}, {spawnIndex}, {tCount}, {ctCount}
    /// </summary>
    public string DuelStartMessageWithSpawn { get; set; } = "{comboName} - Spawn {team} #{spawnIndex}\n{tCount} T vs {ctCount} CT";

    /// <summary>
    /// Message affiché dans le chat au début d'un duel
    /// Placeholders: {comboName}, {tCount}, {ctCount}
    /// </summary>
    public string DuelStartChatMessage { get; set; } = " \u0004[AuroraDuel] \u0001{comboName} \u0001- \u000F{tCount} T \u0001vs \u000B{ctCount} CT";

    /// <summary>
    /// Message affiché au centre de l'écran quand une équipe gagne
    /// Placeholders: {winnerTeam}
    /// </summary>
    public string DuelWinMessage { get; set; } = "Les {winnerTeam} ont gagne !";

    /// <summary>
    /// Activer/désactiver les messages de debug dans la console
    /// </summary>
    public bool EnableDebugMessages { get; set; } = true;

    /// <summary>
    /// Masquer les messages de changement d'équipe dans le chat
    /// </summary>
    public bool HideTeamChangeMessages { get; set; } = true;

    /// <summary>
    /// Donner un gilet pare-balles aux joueurs
    /// </summary>
    public bool GiveKevlar { get; set; } = true;

    /// <summary>
    /// Donner un casque aux joueurs
    /// </summary>
    public bool GiveHelmet { get; set; } = true;

    /// <summary>
    /// Donner un Deagle aux joueurs
    /// </summary>
    public bool GiveDeagle { get; set; } = true;

    /// <summary>
    /// Donner une grenade HE aux joueurs
    /// </summary>
    public bool GiveHEGrenade { get; set; } = true;

    /// <summary>
    /// Donner une flashbang aux joueurs
    /// </summary>
    public bool GiveFlashbang { get; set; } = true;

    /// <summary>
    /// Arme principale pour les Terroristes (ex: "weapon_ak47")
    /// </summary>
    public string TerroristPrimaryWeapon { get; set; } = "weapon_ak47";

    /// <summary>
    /// Arme principale pour les Anti-Terroristes (ex: "weapon_m4a1_silencer")
    /// </summary>
    public string CTerroristPrimaryWeapon { get; set; } = "weapon_m4a1_silencer";
}

