namespace AuroraDuel.Models;

/// <summary>
/// General configuration for AuroraDuel plugin
/// </summary>
public class PluginSettings
{
    /// <summary>
    /// Delay in seconds before starting the next duel after a victory
    /// </summary>
    public float DelayBeforeNextDuel { get; set; } = 1.0f;

    /// <summary>
    /// Delay in seconds after round start before launching the first duel
    /// </summary>
    public float DelayAfterRoundStart { get; set; } = 2f;

    /// <summary>
    /// Enable/disable debug messages in console
    /// </summary>
    public bool EnableDebugMessages { get; set; } = true;

    /// <summary>
    /// Hide team change messages in chat
    /// </summary>
    public bool HideTeamChangeMessages { get; set; } = true;

    /// <summary>
    /// Give kevlar vest to players
    /// </summary>
    public bool GiveKevlar { get; set; } = true;

    /// <summary>
    /// Give helmet to players
    /// </summary>
    public bool GiveHelmet { get; set; } = true;

    /// <summary>
    /// Give Deagle to players
    /// </summary>
    public bool GiveDeagle { get; set; } = true;

    /// <summary>
    /// Give HE grenade to players
    /// </summary>
    public bool GiveHEGrenade { get; set; } = true;

    /// <summary>
    /// Give flashbang to players
    /// </summary>
    public bool GiveFlashbang { get; set; } = true;

    /// <summary>
    /// Primary weapon for Terrorists (e.g., "weapon_ak47")
    /// </summary>
    public string TerroristPrimaryWeapon { get; set; } = "weapon_ak47";

    /// <summary>
    /// Primary weapon for Counter-Terrorists (e.g., "weapon_m4a1_silencer")
    /// </summary>
    public string CTerroristPrimaryWeapon { get; set; } = "weapon_m4a1_silencer";
}

