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

    /// <summary>
    /// Loadout scenarios with probabilities (sum should be 100)
    /// </summary>
    public List<LoadoutScenario> LoadoutScenarios { get; set; } = new List<LoadoutScenario>
    {
        new LoadoutScenario
        {
            Name = "full_buy",
            Probability = 70,
            TerroristPrimaryWeapon = "weapon_ak47",
            CTerroristPrimaryWeapon = "weapon_m4a1_silencer",
            SecondaryWeapon = "weapon_deagle",
            GiveKevlar = true,
            GiveHelmet = true,
            GiveHEGrenade = true,
            GiveFlashbang = true
        },
        new LoadoutScenario
        {
            Name = "half_buy",
            Probability = 20,
            TerroristPrimaryWeapon = "weapon_mac10",
            CTerroristPrimaryWeapon = "weapon_mp9",
            SecondaryWeapon = null,
            GiveKevlar = true,
            GiveHelmet = true,
            GiveHEGrenade = true,
            GiveFlashbang = false
        },
        new LoadoutScenario
        {
            Name = "gun",
            Probability = 10,
            TerroristPrimaryWeapon = null,
            CTerroristPrimaryWeapon = null,
            SecondaryWeapon = null, // Will use default team pistol
            GiveKevlar = true,
            GiveHelmet = false,
            GiveHEGrenade = false,
            GiveFlashbang = false
        }
    };
}

