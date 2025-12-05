using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using AuroraDuel.Models;

namespace AuroraDuel.Managers;

/// <summary>
/// Handles player teleportation to spawn points
/// </summary>
public static class TeleportManager
{
    /// <summary>
    /// Teleports a player to the specified spawn point
    /// </summary>
    public static void TeleportPlayerToSpawn(CCSPlayerController player, Models.SpawnPoint spawn)
    {
        if (player.PlayerPawn?.Value == null || !player.PlayerPawn.IsValid) return;

        var position = new Vector(spawn.PosX, spawn.PosY, spawn.PosZ);
        var angles = new QAngle(0, spawn.AngleYaw, 0);

        var pawn = player.PlayerPawn.Value;
        pawn.Teleport(position, angles, new Vector(0, 0, 0));
    }
}