using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using AuroraDuel.Models;

namespace AuroraDuel.Managers;

/// <summary>
/// Manages player loadouts based on configured scenarios and probabilities
/// </summary>
public class LoadoutManager
{
    private readonly Random _random = new Random();
    private readonly List<LoadoutScenario> _scenarios;
    private readonly int[] _cumulativeProbabilities;

    public LoadoutManager(List<LoadoutScenario> scenarios)
    {
        // Normalize probabilities to ensure they sum to 100
        _scenarios = NormalizeProbabilities(scenarios);
        _cumulativeProbabilities = CalculateCumulativeProbabilities(_scenarios);
    }

    /// <summary>
    /// Normalizes scenario probabilities to sum to 100
    /// </summary>
    private List<LoadoutScenario> NormalizeProbabilities(List<LoadoutScenario> scenarios)
    {
        if (scenarios.Count == 0)
        {
            return new List<LoadoutScenario>();
        }

        int total = scenarios.Sum(s => s.Probability);
        
        if (total == 0)
        {
            // If all probabilities are 0, distribute equally
            int equalProb = 100 / scenarios.Count;
            int remainder = 100 % scenarios.Count;
            for (int i = 0; i < scenarios.Count; i++)
            {
                scenarios[i].Probability = equalProb + (i < remainder ? 1 : 0);
            }
        }
        else if (total != 100)
        {
            // Normalize to 100
            int normalizedSum = 0;
            for (int i = 0; i < scenarios.Count - 1; i++)
            {
                int normalized = (int)Math.Round((double)scenarios[i].Probability * 100 / total);
                scenarios[i].Probability = normalized;
                normalizedSum += normalized;
            }
            // Last scenario gets the remainder to ensure sum is exactly 100
            scenarios[^1].Probability = 100 - normalizedSum;
        }

        // Verify final sum and adjust if needed
        int finalSum = scenarios.Sum(s => s.Probability);
        if (finalSum != 100)
        {
            int diff = 100 - finalSum;
            scenarios[^1].Probability += diff;
        }

        return scenarios;
    }

    /// <summary>
    /// Calculates cumulative probabilities for weighted random selection
    /// </summary>
    private int[] CalculateCumulativeProbabilities(List<LoadoutScenario> scenarios)
    {
        var cumulative = new int[scenarios.Count];
        int sum = 0;
        for (int i = 0; i < scenarios.Count; i++)
        {
            sum += scenarios[i].Probability;
            cumulative[i] = sum;
        }
        return cumulative;
    }

    /// <summary>
    /// Selects a random loadout scenario based on probabilities
    /// </summary>
    public LoadoutScenario SelectRandomScenario()
    {
        if (_scenarios.Count == 0)
        {
            throw new InvalidOperationException("No loadout scenarios configured");
        }

        int randomValue = _random.Next(1, 101); // 1-100

        for (int i = 0; i < _cumulativeProbabilities.Length; i++)
        {
            if (randomValue <= _cumulativeProbabilities[i])
            {
                return _scenarios[i];
            }
        }

        // Fallback to last scenario (should not happen if probabilities are correct)
        return _scenarios[^1];
    }

    /// <summary>
    /// Applies the selected loadout scenario to a player
    /// </summary>
    public void ApplyLoadout(CCSPlayerController player, LoadoutScenario scenario, bool silent = false)
    {
        if (player == null || !player.IsValid) return;

        player.RemoveWeapons();
        
        // Remove armor (kevlar/helmet) before applying new loadout
        RemovePlayerArmor(player);

        // Primary weapon based on team
        string? primaryWeapon = player.Team == CsTeam.Terrorist 
            ? scenario.TerroristPrimaryWeapon 
            : scenario.CTerroristPrimaryWeapon;

        if (!string.IsNullOrWhiteSpace(primaryWeapon))
        {
            player.GiveNamedItem(primaryWeapon);
        }

        // Secondary weapon
        if (!string.IsNullOrWhiteSpace(scenario.SecondaryWeapon))
        {
            player.GiveNamedItem(scenario.SecondaryWeapon);
        }
        else
        {
            // If no secondary specified, always give default team pistol
            if (player.Team == CsTeam.Terrorist)
            {
                player.GiveNamedItem(CsItem.Glock);
            }
            else if (player.Team == CsTeam.CounterTerrorist)
            {
                player.GiveNamedItem(CsItem.USP);
            }
        }

        // Always give knife
        player.GiveNamedItem(CsItem.Knife);

        // Armor - Apply after a frame update to ensure proper state synchronization
        Server.NextFrame(() =>
        {
            if (player == null || !player.IsValid || player.PlayerPawn?.Value == null) return;

            var pawn = player.PlayerPawn.Value;
            if (!pawn.IsValid) return;

            if (scenario.GiveKevlar)
            {
                // Set armor value
                pawn.ArmorValue = 100;
                Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");

                // Set helmet state via ItemServices
                if (pawn.ItemServices != null)
                {
                    var itemService = new CCSPlayer_ItemServices(pawn.ItemServices.Handle)
                    {
                        HasHelmet = scenario.GiveHelmet
                    };
                    // Notify the game that ItemServices has changed (this will sync HasHelmet)
                    Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pItemServices");
                }
            }
        });

        // Grenades
        if (scenario.GiveHEGrenade)
        {
            player.GiveNamedItem(CsItem.HE);
        }
        if (scenario.GiveFlashbang)
        {
            player.GiveNamedItem(CsItem.Flashbang);
        }
        if (scenario.GiveSmoke)
        {
            player.GiveNamedItem(CsItem.Smoke);
        }
        if (scenario.GiveMolotov)
        {
            if (player.Team == CsTeam.Terrorist)
            {
                player.GiveNamedItem(CsItem.Molotov);
            }
            else
            {
                player.GiveNamedItem(CsItem.Incendiary);
            }
        }

    }

    /// <summary>
    /// Removes all armor (kevlar and helmet) from a player
    /// </summary>
    private void RemovePlayerArmor(CCSPlayerController player)
    {
        // Reset armor values using NextFrame to ensure proper synchronization
        Server.NextFrame(() =>
        {
            if (player == null || !player.IsValid || player.PlayerPawn?.Value == null) return;

            var pawn = player.PlayerPawn.Value;
            if (!pawn.IsValid) return;

            // Reset armor value
            pawn.ArmorValue = 0;
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");

            // Remove helmet via ItemServices
            if (pawn.ItemServices != null)
            {
                var itemService = new CCSPlayer_ItemServices(pawn.ItemServices.Handle)
                {
                    HasHelmet = false
                };
                // Notify the game that ItemServices has changed (this will sync HasHelmet)
                Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pItemServices");
            }
        });
    }
}

