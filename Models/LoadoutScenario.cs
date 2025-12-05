namespace AuroraDuel.Models;

/// <summary>
/// Represents a loadout scenario with weapons and equipment
/// </summary>
public class LoadoutScenario
{
    /// <summary>
    /// Name of the scenario (e.g., "full_buy", "half_buy", "gun")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Probability percentage (0-100) for this scenario to be selected
    /// </summary>
    public int Probability { get; set; } = 0;

    /// <summary>
    /// Primary weapon for Terrorists (e.g., "weapon_ak47", "weapon_galil", null for no primary)
    /// </summary>
    public string? TerroristPrimaryWeapon { get; set; }

    /// <summary>
    /// Primary weapon for Counter-Terrorists (e.g., "weapon_m4a1_silencer", "weapon_famas", null for no primary)
    /// </summary>
    public string? CTerroristPrimaryWeapon { get; set; }

    /// <summary>
    /// Secondary weapon (e.g., "weapon_deagle", "weapon_p250", "weapon_glock", "weapon_usp_silencer")
    /// </summary>
    public string? SecondaryWeapon { get; set; }

    /// <summary>
    /// Whether to give kevlar vest
    /// </summary>
    public bool GiveKevlar { get; set; } = true;

    /// <summary>
    /// Whether to give helmet
    /// </summary>
    public bool GiveHelmet { get; set; } = true;

    /// <summary>
    /// Whether to give HE grenade
    /// </summary>
    public bool GiveHEGrenade { get; set; } = false;

    /// <summary>
    /// Whether to give flashbang
    /// </summary>
    public bool GiveFlashbang { get; set; } = false;

    /// <summary>
    /// Whether to give smoke grenade
    /// </summary>
    public bool GiveSmoke { get; set; } = false;

    /// <summary>
    /// Whether to give molotov/incendiary grenade
    /// </summary>
    public bool GiveMolotov { get; set; } = false;
}

