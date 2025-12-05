using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization; // Pour la sérialisation JSON

// Classe pour stocker la position et l'angle d'un joueur
public class SpawnPoint
{
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }

    // Angle Yaw (rotation horizontale) est souvent le plus important
    public float AngleYaw { get; set; }
}

// Classe pour stocker la combinaison complète avec listes de spawns
public class DuelCombination
{
    public string MapName { get; set; } = string.Empty; // La carte sur laquelle la combinaison est définie
    public string ComboName { get; set; } = string.Empty; // Nom unique (ex: "long_A_v_mid")
    public List<SpawnPoint> TSpawns { get; set; } = new List<SpawnPoint>();
    public List<SpawnPoint> CTSpawns { get; set; } = new List<SpawnPoint>();
}

// Classe conteneur pour la sauvegarde
public class DuelConfig
{
    public List<DuelCombination> Combos { get; set; } = new List<DuelCombination>();
}