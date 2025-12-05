# AuroraDuel

Plugin CounterStrikeSharp pour Counter-Strike 2 qui permet de créer et gérer des duels personnalisés avec des spawns configurables.

## 📋 Description

AuroraDuel est un plugin qui transforme votre serveur CS2 en une arène de duels personnalisés. Le plugin permet de :

- Configurer des duels avec des spawns T et CT flexibles (1v1, 2v4, etc.)
- Gérer automatiquement les rounds infinis (60 minutes)
- Téléporter automatiquement les joueurs aux positions configurées
- Équiper automatiquement les joueurs avec des armes personnalisables
- Afficher des messages personnalisés au début et à la fin de chaque duel

## 🚀 Prérequis

- Counter-Strike 2 Server
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (version 1.0.347 ou supérieure)
- .NET 8.0 SDK

## 📦 Installation

1. Clonez ce repository ou téléchargez les fichiers sources
2. Compilez le projet avec Visual Studio ou la ligne de commande :
   ```bash
   dotnet build
   ```
3. Copiez le fichier `AuroraDuel.dll` généré dans le dossier `bin/Debug/net8.0/` vers :
   ```
   csgo/addons/counterstrikesharp/plugins/
   ```
4. Copiez le dossier `configs/` à la racine de votre serveur CS2
5. Redémarrez votre serveur ou rechargez les plugins

## ⚙️ Configuration

### Fichier de configuration serveur (`configs/duel_settings.cfg`)

Ce fichier contient les paramètres du serveur pour les duels. Il est automatiquement exécuté au démarrage du plugin.

Les paramètres principaux incluent :
- Round de 60 minutes (infini)
- Désactivation du warmup
- Désactivation des conditions de fin de round automatiques
- Configuration des drops d'armes

### Fichier de paramètres du plugin (`configs/plugins/AuroraDuel/settings.json`)

Ce fichier est créé automatiquement au premier lancement. Il contient :

- **DelayBeforeNextDuel** : Délai avant le prochain duel (défaut: 1.0s)
- **DelayAfterRoundStart** : Délai après le début du round (défaut: 2.0s)
- **DuelStartMessage** : Message au centre de l'écran (spectateurs)
- **DuelStartMessageWithSpawn** : Message au centre de l'écran (participants)
- **DuelStartChatMessage** : Message dans le chat
- **DuelWinMessage** : Message de victoire
- **GiveKevlar** : Donner un gilet pare-balles (défaut: true)
- **GiveHelmet** : Donner un casque (défaut: true)
- **GiveDeagle** : Donner un Deagle (défaut: true)
- **GiveHEGrenade** : Donner une grenade HE (défaut: true)
- **GiveFlashbang** : Donner une flashbang (défaut: true)
- **TerroristPrimaryWeapon** : Arme principale T (défaut: "weapon_ak47")
- **CTerroristPrimaryWeapon** : Arme principale CT (défaut: "weapon_m4a1_silencer")

### Placeholders pour les messages

- `{comboName}` : Nom du duel
- `{team}` : Équipe du joueur (T ou CT)
- `{spawnIndex}` : Index du spawn du joueur
- `{tCount}` : Nombre de joueurs T
- `{ctCount}` : Nombre de joueurs CT
- `{winnerTeam}` : Équipe gagnante

## 🎮 Commandes

Toutes les commandes nécessitent la permission `@css/root`.

### Configuration des spawns

- `!duel_add_t <NomDuel>` - Ajoute un spawn T à votre position actuelle
- `!duel_add_ct <NomDuel>` - Ajoute un spawn CT à votre position actuelle
- `!duel_remove_t_spawn <NomDuel> <index>` - Supprime un spawn T spécifique (index commence à 1)
- `!duel_remove_ct_spawn <NomDuel> <index>` - Supprime un spawn CT spécifique (index commence à 1)

### Gestion des duels

- `!duel_list` - Liste tous les duels configurés sur la carte actuelle
- `!duel_info <NomDuel>` - Affiche les détails d'un duel (spawns, positions)
- `!duel_delete <NomDuel>` - Supprime un duel de la carte actuelle

### Mode configuration

- `!duel_config [on|off]` - Active/désactive le mode configuration
  - En mode `on` : Les duels ne démarrent pas automatiquement, vous pouvez configurer les spawns
  - En mode `off` : Les duels reprennent automatiquement si des joueurs sont présents

### Autres commandes

- `!duel_map <NomCarte>` - Change la carte du serveur
- `!duel_reload` - Recharge les paramètres du plugin
- `!duel_help` - Affiche la liste de toutes les commandes disponibles

## 🎯 Utilisation

### Configuration d'un nouveau duel

1. Activez le mode configuration : `!duel_config on`
2. Changez de carte si nécessaire : `!duel_map de_dust2`
3. Positionnez-vous à l'endroit où vous voulez un spawn T et tapez : `!duel_add_t long_A`
4. Répétez pour tous les spawns T du duel "long_A"
5. Positionnez-vous pour les spawns CT et tapez : `!duel_add_ct long_A`
6. Répétez pour tous les spawns CT
7. Désactivez le mode configuration : `!duel_config off`

### Exemple de configuration

Pour créer un duel 2v2 sur "long_A" :
```
!duel_config on
!duel_map de_dust2
[Se positionner à la position T1] !duel_add_t long_A
[Se positionner à la position T2] !duel_add_t long_A
[Se positionner à la position CT1] !duel_add_ct long_A
[Se positionner à la position CT2] !duel_add_ct long_A
!duel_config off
```

### Vérification des duels

- `!duel_list` - Voir tous les duels de la carte
- `!duel_info long_A` - Voir les détails du duel "long_A"

## 🔧 Fonctionnalités

### Gestion automatique des duels

- Sélection aléatoire d'un duel parmi ceux disponibles sur la carte
- Équilibrage automatique des équipes selon le nombre de spawns disponibles
- Téléportation automatique des joueurs aux positions configurées
- Attribution automatique d'équipement (armes, armure, grenades)
- Nettoyage automatique des armes au sol entre les duels

### Système de rounds infinis

- Rounds de 60 minutes
- Les rounds ne se terminent pas automatiquement quand une équipe est éliminée
- Nouveau duel automatique après chaque victoire
- Les joueurs en trop sont automatiquement mis en spectateur

### Messages personnalisés

- Message au centre de l'écran pour chaque joueur avec son index de spawn
- Message dans le chat pour tous les joueurs
- Message de victoire personnalisable

## 📁 Structure du projet

```
AuroraDuel/
├── AuroraDuel.cs              # Point d'entrée du plugin
├── Commands/
│   └── DuelCommands.cs        # Gestion de toutes les commandes
├── Managers/
│   ├── ConfigManager.cs       # Gestion de la configuration des duels
│   ├── DuelGameManager.cs    # Logique principale du jeu
│   ├── SettingsManager.cs    # Gestion des paramètres
│   └── TeleportManager.cs    # Gestion de la téléportation
├── Models/
│   ├── DuelSpawn.cs           # Modèles de données (DuelCombination, SpawnPoint)
│   └── PluginSettings.cs     # Modèle des paramètres du plugin
└── configs/
    └── duel_settings.cfg     # Configuration serveur
```

## 📝 Format des données

Les duels sont sauvegardés dans `configs/plugins/AuroraDuel/duels.json` :

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

## 🐛 Dépannage

### Les duels ne démarrent pas

- Vérifiez qu'au moins un duel est configuré sur la carte actuelle : `!duel_list`
- Vérifiez que le mode configuration est désactivé : `!duel_config off`
- Vérifiez qu'il y a au moins un joueur T et un joueur CT en jeu

### Les joueurs ne sont pas téléportés

- Vérifiez que les spawns sont valides : `!duel_info <NomDuel>`
- Vérifiez que les coordonnées des spawns ne sont pas (0, 0, 0)

### Les messages ne s'affichent pas

- Vérifiez les paramètres dans `settings.json`
- Rechargez les paramètres : `!duel_reload`

## 📄 Licence

Ce projet est sous licence libre. Vous êtes libre de l'utiliser, le modifier et le distribuer.

## 🤝 Contribution

Les contributions sont les bienvenues ! N'hésitez pas à ouvrir une issue ou une pull request.

## 📞 Support

Pour toute question ou problème, ouvrez une issue sur le repository GitHub.

---

**Version** : 1.0.0  
**Auteur** : AuroraDuel Team  
**Compatibilité** : Counter-Strike 2, CounterStrikeSharp 1.0.347+

