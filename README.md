# BagsForEveryone

A minimalist BepInEx mod for Peak that automatically spawns backpacks for all players when starting a game.

## 🎒 Features

- **Automatic backpack spawning** - Every player gets a backpack when the game starts
- **Smart scene detection** - Only works in actual game levels (Level_0 to Level_13), not in lobby
- **Mid-game support** - New players joining mid-game also receive backpacks
- **Multiplayer compatible** - Works seamlessly with Photon networking
- **Duplicate prevention** - Won't give multiple backpacks to the same player
- **Zero configuration** - Works out of the box with sensible defaults

## 🚀 Installation

### Automatic (Mod Manager)
1. Install using r2modman or Thunderstore Mod Manager
2. Launch Peak

### Manual Installation
1. Make sure BepInEx is installed in your Peak game folder
2. Download the latest release from GitHub
3. Extract `BagsForEveryone.dll` to `BepInEx/plugins/`
4. Launch Peak

## ⚙️ Configuration

The mod has only one setting:
- **ModEnabled** (default: true) - Enable/disable automatic backpack spawning

Configuration file: `BepInEx/config/com.nozz.bagsforeveryone.cfg`

## 🎮 How It Works

The mod automatically detects when players spawn into game levels and ensures everyone gets exactly one backpack. It uses Peak's official item spawning system and Photon networking for seamless multiplayer compatibility.

Fixed settings for optimal experience:
- **Spawn delay**: 2 seconds after game start
- **Backpack amount**: 1 per player
- **Mid-game spawning**: Always enabled

## 🔧 Development

This project uses GitHub Actions for automated building and releasing.

### Building Locally
```bash
dotnet build --configuration Release
```

### Creating a Release
```bash
git tag v1.0.0
git push origin v1.0.0
```

## 🤝 Contributing

Feel free to submit issues or pull requests on GitHub.

## 📄 License

MIT License - See LICENSE file for details.