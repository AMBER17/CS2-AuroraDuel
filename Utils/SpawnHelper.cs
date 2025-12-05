using AuroraDuel.Models;

namespace AuroraDuel.Utils;

/// <summary>
/// Utility class for spawn point operations
/// </summary>
public static class SpawnHelper
{
    /// <summary>
    /// Checks if a spawn point is valid (not null and not at origin)
    /// </summary>
    public static bool IsValidSpawn(SpawnPoint? spawn)
    {
        return spawn != null && (spawn.PosX != 0 || spawn.PosY != 0 || spawn.PosZ != 0);
    }

    /// <summary>
    /// Gets all valid spawns from a list
    /// </summary>
    public static List<SpawnPoint> GetValidSpawns(List<SpawnPoint> spawns)
    {
        return spawns.Where(IsValidSpawn).ToList();
    }

    /// <summary>
    /// Gets the display index of a spawn (1-based, counting only valid spawns)
    /// </summary>
    public static int GetDisplayIndex(SpawnPoint spawn, List<SpawnPoint> allSpawns)
    {
        int originalIndex = allSpawns.IndexOf(spawn);
        return allSpawns
            .Take(originalIndex + 1)
            .Count(IsValidSpawn);
    }
}

