# BagsForEveryone

Minimalist mod that automatically generates backpacks for all players when starting a game in PEAK.

## 🎒 Features

- **Automatic backpack generation** - Each player receives a backpack when starting the game
- **Smart scene detection** - Only works on real game levels (Level_0 to Level_13), not in lobby
- **Support for mid-game joining players** - New players also receive backpacks
- **Multiplayer compatible** - Works perfectly with Photon networking system
- **Duplicate prevention** - Won't give multiple backpacks to the same player
- **Zero configuration** - Works from the first moment without configuration

## 🚀 Installation

### Automatic (Mod Manager)

1. Install using r2modman or Thunderstore Mod Manager
2. Launch Peak

### Manual

1. Make sure BepInEx is installed in your Peak folder
2. Download the latest version of the mod
3. Extract `BagsForEveryone.dll` to `BepInEx/plugins/`
4. Launch Peak

## ⚙️ Configuration

The mod has only one configuration:

- **ModEnabled** (default: true) - Enable/disable automatic backpack generation

Configuration file: `BepInEx/config/com.nozz.bagsforeveryone.cfg`

## 🎮 How It Works

The mod automatically detects when players spawn in game levels and ensures all players get exactly one backpack. It uses Peak's official object generation system and Photon networking for perfect multiplayer compatibility.

Fixed settings for optimal experience:

- **Spawn delay**: 2 seconds after game start
- **Backpack quantity**: 1 per player
- **Mid-game spawning**: Always enabled

## 🔧 Development

This project is part of the PEAK mods monorepo.

### Build Locally

```bash
dotnet build --configuration Release
```

### Create a Release

```bash
git tag BagsForEveryone-v1.0.0
git push origin BagsForEveryone-v1.0.0
```

## 🤝 Contributing

Feel free to submit issues or pull requests on GitHub.

## 📄 License

MIT License - See LICENSE file for details.
