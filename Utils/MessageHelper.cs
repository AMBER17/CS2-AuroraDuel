using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace AuroraDuel.Utils;

/// <summary>
/// Utility class for sending messages to players or console
/// </summary>
public static class MessageHelper
{
    /// <summary>
    /// Sends a message to a player (if valid) or to console
    /// </summary>
    public static void SendMessage(CCSPlayerController? player, CommandInfo? info, string message)
    {
        if (player != null && player.IsValid)
        {
            player.PrintToChat(message);
        }
        else if (info != null)
        {
            info.ReplyToCommand(message);
        }
    }

    /// <summary>
    /// Sends multiple messages to a player (if valid) or to console
    /// </summary>
    public static void SendMessages(CCSPlayerController? player, CommandInfo? info, IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            if (player != null && player.IsValid)
            {
                player.PrintToChat(message);
            }
            else if (info != null)
            {
                info.ReplyToCommand(message);
            }
        }
    }

    /// <summary>
    /// Checks if a player is valid and not a bot/HLTV
    /// </summary>
    public static bool IsValidPlayer(CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
    }

    /// <summary>
    /// Removes chat color codes from a message for console output
    /// </summary>
    public static string StripChatColors(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;
        
        // Remove common chat color codes
        var colorCodes = new[] { "\u0001", "\u0004", "\u000B", "\u000F" };
        string result = message;
        foreach (var code in colorCodes)
        {
            result = result.Replace(code, "");
        }
        
        // Remove ANSI-style color codes if present
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\u001B\[[0-9;]*m", "");
        
        return result.Trim();
    }
}

