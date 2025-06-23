# Bags For Everyone

A minimalist mod that automatically spawns backpacks for all players when starting a game. No more forgetting your bags!

## Features

- 🎒 **Automatic backpack spawning** - Every player gets a backpack when the game starts
- 🔄 **Mid-game support** - New players joining mid-game also receive backpacks
- 🎯 **Smart scene detection** - Only works in actual game levels (Level_0 to Level_13), not in lobby
- 🌐 **Multiplayer compatible** - Works seamlessly with Photon networking
- 🛡️ **Duplicate prevention** - Won't give multiple backpacks to the same player
- ⚡ **Zero configuration** - Works out of the box with sensible defaults

## Installation

### Automatic (Thunderstore Mod Manager)
1. Install using your preferred mod manager (r2modman, Thunderstore Mod Manager)
2. Launch the game

### Manual Installation
1. Make sure BepInEx is installed in your Peak game folder
2. Download the latest release
3. Extract `BagsForEveryone.dll` to `BepInEx/plugins/`
4. Launch the game

## Configuration

The mod is designed to be minimalist with only one setting:
- **ModEnabled** (default: true) - Enable/disable automatic backpack spawning

Configuration file location: `BepInEx/config/com.nozz.bagsforeveryone.cfg`

## How It Works

The mod automatically detects when players spawn into game levels and ensures everyone gets exactly one backpack. It uses Peak's official item spawning system and Photon networking for seamless multiplayer compatibility.

**Fixed settings for optimal experience:**
- Spawn delay: 2 seconds after game start
- Backpack amount: 1 per player
- Mid-game spawning: Always enabled

Only the master client spawns items to prevent duplication, and all spawning is synchronized across all clients.

## Compatibility

- ✅ **Peak** - Primary game support
- ✅ **BepInEx 5.x** - Required framework
- ✅ **Multiplayer** - Full Photon networking support
- ✅ **All game modes** - Works in any level scenario

## Changelog

### v1.0.0
- Initial release
- Minimalist automatic backpack spawning for all players
- Smart scene detection (Level_0 to Level_13)
- Mid-game player support
- Zero configuration required

## Support

If you encounter any issues:
1. Check the BepInEx console for error messages
2. Verify the mod is enabled in the configuration file
3. Report issues on the GitHub repository

## Credits

Created by nozz for the Peak community.

## License

MIT License - Feel free to modify and redistribute.