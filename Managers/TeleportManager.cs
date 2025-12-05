using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils; // Pour Vector et QAngle

namespace AuroraDuel.Managers;

public static class TeleportManager
{
    public static void TeleportPlayerToSpawn(CCSPlayerController player, SpawnPoint spawn)
    {
        if (player.PlayerPawn?.Value == null || !player.PlayerPawn.IsValid) return;

        // 1. Créer les objets de position et d'angle
        var position = new Vector(spawn.PosX, spawn.PosY, spawn.PosZ);
        var angles = new QAngle(0, spawn.AngleYaw, 0);

        // 2. Téléporter
        var pawn = player.PlayerPawn.Value;
        pawn.Teleport(position, angles, new Vector(0, 0, 0));

        // 3. (Optionnel) Réinitialiser le joueur si besoin (santé, armes, etc.)
        // ResetPlayerState(player);
    }

    // Ajoutez ici des fonctions utilitaires comme ResetPlayerState()
    // public static void ResetPlayerState(CCSPlayerController player) { /* ... */ }
}