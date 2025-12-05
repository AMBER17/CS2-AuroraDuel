using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AuroraDuel.Models;

/// <summary>
/// Represents a spawn point with position and angle
/// </summary>
public class SpawnPoint
{
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float AngleYaw { get; set; }
}

/// <summary>
/// Represents a complete duel combination with T and CT spawn lists
/// </summary>
public class DuelCombination
{
    public string MapName { get; set; } = string.Empty;
    public string ComboName { get; set; } = string.Empty;
    public List<SpawnPoint> TSpawns { get; set; } = new List<SpawnPoint>();
    public List<SpawnPoint> CTSpawns { get; set; } = new List<SpawnPoint>();
}

/// <summary>
/// Container class for duel configuration storage
/// </summary>
public class DuelConfig
{
    public List<DuelCombination> Combos { get; set; } = new List<DuelCombination>();
}