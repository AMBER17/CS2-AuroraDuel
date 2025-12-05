# AuroraDuel

CounterStrikeSharp plugin for Counter-Strike 2 that allows you to create and manage custom duels with configurable spawns.

## 📋 Description

AuroraDuel is a plugin that transforms your CS2 server into a custom duel arena. The plugin allows you to:

- Configure duels with flexible T and CT spawns (1v1, 2v4, etc.)
- Automatically manage infinite rounds (60 minutes)
- Automatically teleport players to configured positions
- Automatically equip players with customizable weapons
- Display customizable messages at the start and end of each duel

## 🚀 Prerequisites

- Counter-Strike 2 Server
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (version 1.0.347 or higher)
- .NET 8.0 SDK

## 📦 Installation

1. Clone this repository or download the source files
2. Build the project with Visual Studio or command line:
   ```bash
   dotnet build
   ```
3. Copy the generated `AuroraDuel.dll` file from the `bin/Debug/net8.0/` folder to:
   ```
   csgo/addons/counterstrikesharp/plugins/
   ```
4. Copy the `configs/` folder to the root of your CS2 server
5. Restart your server or reload plugins

## ⚙️ Configuration

### Server Configuration File (`configs/duel_settings.cfg`)

This file contains server settings for duels. It is automatically executed when the plugin starts.

Main settings include:
- 60-minute rounds (infinite)
- Warmup disabled
- Automatic round end conditions disabled
- Weapon drop configuration

### Plugin Settings File (`configs/plugins/AuroraDuel/settings.json`)

This file is automatically created on first launch. It contains only gameplay configuration:

- **DelayBeforeNextDuel**: Delay before next duel (default: 1.0s)
- **DelayAfterRoundStart**: Delay after round start (default: 2.0s)
- **EnableDebugMessages**: Enable/disable debug messages in console (default: true)
- **HideTeamChangeMessages**: Hide team change messages in chat (default: true)
- **GiveKevlar**: Give kevlar vest (default: true)
- **GiveHelmet**: Give helmet (default: true)
- **GiveDeagle**: Give Deagle (default: true)
- **GiveHEGrenade**: Give HE grenade (default: true)
- **GiveFlashbang**: Give flashbang (default: true)
- **TerroristPrimaryWeapon**: T primary weapon (default: "weapon_ak47")
- **CTerroristPrimaryWeapon**: CT primary weapon (default: "weapon_m4a1_silencer")

### Localization File (`configs/plugins/AuroraDuel/localization.json`)

**This is the main file for all plugin messages and text.** It is automatically created with English defaults on first launch. All user-facing messages are stored here, making it easy to translate the entire plugin to any language.

The localization file contains:

- **Plugin messages**: Loading, saving, error messages
- **Command messages**: Success, error, and usage messages for all commands
- **Duel messages**: Start messages, win messages, chat messages
- **In-game messages**: Spawn notifications, team information

#### Message Placeholders

All messages support placeholders that are automatically replaced:

- `{comboName}`: Duel name
- `{team}`: Player's team (T or CT)
- `{spawnIndex}`: Player's spawn index
- `{tCount}`: Number of T players
- `{ctCount}`: Number of CT players
- `{winnerTeam}`: Winning team
- `{0}`, `{1}`, etc.: Format arguments for string.Format

#### Duel Messages in Localization

The following duel-related messages are configurable in `localization.json`:

- **DuelStartMessage**: Center screen message for spectators
- **DuelStartMessageWithSpawn**: Center screen message for participating players (includes spawn index)
- **DuelStartChatMessage**: Chat message displayed to all players at duel start
- **DuelWinMessage**: Victory message displayed when a team wins

## 🎮 Commands

All commands require `@css/root` permission.

### Spawn Configuration

- `!duel_add_t <DuelName>` - Add a T spawn at your current position
- `!duel_add_ct <DuelName>` - Add a CT spawn at your current position
- `!duel_remove_t_spawn <DuelName> <index>` - Remove a specific T spawn (index starts at 1)
- `!duel_remove_ct_spawn <DuelName> <index>` - Remove a specific CT spawn (index starts at 1)

### Duel Management

- `!duel_list` - List all duels configured on the current map
- `!duel_info <DuelName>` - Display duel details (spawns, positions)
- `!duel_delete <DuelName>` - Delete a duel from the current map

### Configuration Mode

- `!duel_config [on|off]` - Enable/disable configuration mode
  - In `on` mode: Duels don't start automatically, you can configure spawns
  - In `off` mode: Duels automatically resume if players are present

### Other Commands

- `!duel_map <MapName>` - Change server map
- `!duel_reload` - Reload plugin settings
- `!duel_help` - Display list of all available commands

## 🎯 Usage

### Configuring a New Duel

1. Enable configuration mode: `!duel_config on`
2. Change map if needed: `!duel_map de_dust2`
3. Position yourself where you want a T spawn and type: `!duel_add_t long_A`
4. Repeat for all T spawns for the "long_A" duel
5. Position yourself for CT spawns and type: `!duel_add_ct long_A`
6. Repeat for all CT spawns
7. Disable configuration mode: `!duel_config off`

### Example Configuration

To create a 2v2 duel on "long_A":
```
!duel_config on
!duel_map de_dust2
[Position yourself at T1 position] !duel_add_t long_A
[Position yourself at T2 position] !duel_add_t long_A
[Position yourself at CT1 position] !duel_add_ct long_A
[Position yourself at CT2 position] !duel_add_ct long_A
!duel_config off
```

### Verifying Duels

- `!duel_list` - See all duels on the map
- `!duel_info long_A` - See details of the "long_A" duel

## 🔧 Features

### Automatic Duel Management

- Random selection of a duel among those available on the map
- Automatic team balancing based on available spawns
- Automatic player teleportation to configured positions
- Automatic equipment assignment (weapons, armor, grenades)
- Automatic cleanup of weapons on the ground between duels

### Infinite Round System

- 60-minute rounds
- Rounds don't automatically end when a team is eliminated
- Automatic new duel after each victory
- Extra players are automatically moved to spectator

### Complete Internationalization

- **All messages centralized** in `localization.json`
- Default language: English
- Easy to translate to any language by modifying a single file
- All user-facing text is translatable (commands, messages, notifications)
- Placeholder system for dynamic content

## 📁 Project Structure

```
AuroraDuel/
├── AuroraDuel.cs              # Plugin entry point
├── Commands/
│   └── DuelCommands.cs       # Handles all commands
├── Managers/
│   ├── ConfigManager.cs       # Manages duel configuration
│   ├── DuelGameManager.cs    # Main game logic
│   ├── SettingsManager.cs    # Manages settings
│   ├── LocalizationManager.cs # Manages translations
│   └── TeleportManager.cs    # Handles teleportation
├── Models/
│   ├── DuelSpawn.cs           # Data models (DuelCombination, SpawnPoint)
│   ├── PluginSettings.cs     # Plugin settings model
│   └── Localization.cs       # Localization model
└── configs/
    └── duel_settings.cfg     # Server configuration
```

## 📝 Data Format

Duels are saved in `configs/plugins/AuroraDuel/duels.json`:

```json
{
  "Combos": [
    {
      "MapName": "de_dust2",
      "ComboName": "long_A",
      "TSpawns": [
        {
          "PosX": 100.0,
          "PosY": 200.0,
          "PosZ": 50.0,
          "AngleYaw": 90.0
        }
      ],
      "CTSpawns": [
        {
          "PosX": 300.0,
          "PosY": 400.0,
          "PosZ": 50.0,
          "AngleYaw": 270.0
        }
      ]
    }
  ]
}
```

## 🌍 Localization

The plugin supports full internationalization. **All user-facing messages are stored in the localization file**, making translation simple and centralized.

### Translating the Plugin

To translate the plugin to your language:

1. Edit `configs/plugins/AuroraDuel/localization.json`
2. Translate all string values to your language
3. Keep all placeholders intact (e.g., `{comboName}`, `{tCount}`, `{0}`, etc.)
4. Save the file
5. Reload the plugin or use `!duel_reload`

### Message Categories

The localization file is organized into categories:

- **Plugin messages**: System messages (loading, saving, errors)
- **Command errors**: Error messages for invalid commands
- **Command success messages**: Success confirmations
- **Command usage**: Usage instructions for commands
- **Duel info**: Information display messages
- **Duel list**: List formatting messages
- **Help command**: Help text for all commands
- **In-game messages**: Player notifications
- **Duel messages**: Start, win, and chat messages

### Example Translation

To translate to French, you would change:

```json
{
  "DuelStartMessage": "{comboName}\n{tCount} T vs {ctCount} CT",
  "DuelWinMessage": "The {winnerTeam} have won!"
}
```

To:

```json
{
  "DuelStartMessage": "{comboName}\n{tCount} T vs {ctCount} CT",
  "DuelWinMessage": "Les {winnerTeam} ont gagné !"
}
```

**Note**: All placeholders must remain unchanged for the plugin to work correctly.

## 🐛 Troubleshooting

### Duels Don't Start

- Check that at least one duel is configured on the current map: `!duel_list`
- Check that configuration mode is disabled: `!duel_config off`
- Check that there is at least one T player and one CT player in the game

### Players Are Not Teleported

- Check that spawns are valid: `!duel_info <DuelName>`
- Check that spawn coordinates are not (0, 0, 0)

### Messages Don't Display

- Check settings in `settings.json`
- Reload settings: `!duel_reload`

## 📄 License

This project is under a free license. You are free to use, modify, and distribute it.

## 🤝 Contributing

Contributions are welcome! Feel free to open an issue or pull request.

## 📞 Support

For any questions or issues, open an issue on the GitHub repository.

---

**Version**: 1.0.0  
**Author**: AuroraDuel Team  
**Compatibility**: Counter-Strike 2, CounterStrikeSharp 1.0.347+
