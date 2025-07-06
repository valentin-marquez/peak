# BagsForEveryone

Automatically spawns backpacks for all players when starting a game. No more forgetting your bags!

## Installation

1. Install BepInEx 5.4.2100+
2. Place `BagsForEveryone.dll` in `BepInEx/plugins/`
3. Launch PEAK

## Features

- Automatic backpack for each player
- Works on game levels (Level_0 to Level_13)
- Mid-game joining players also get bags
- Multiplayer compatible
- No configuration needed

## Configuration

Optional config file: `BepInEx/config/com.nozz.bagsforeveryone.cfg`

- **ModEnabled** (default: true) - Enable/disable the mod

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
