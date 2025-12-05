using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

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
}

