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

