# ClimbMap

**Where the hell are my friends?**

A vertical minimap that shows you who's winning the race to the top. Because nothing's more frustrating than climbing solo only to discover your buddy is already taking selfies at the summit.

Simple, useful, and saves you from yelling "WHERE IS EVERYONE?" into your mic.

*Know where you are. Know where you shouldn't be. Keep climbing.*

## 🚀 Features

- **Vertical Minimap**: Displays altitude in a heartbeat-style vertical layout
- **Player Tracking**: Shows all players with their custom colors
- **Campfire Indicators**: Displays campfires with different states (unlit, lit, spent)
- **Real-time Updates**: Live position tracking and status updates
- **Configurable**: Customize size, opacity, and display options

## How it Works

The minimap appears in the top-right corner of your screen and shows:

- **Players**: Colored dots representing each player's position on the mountain
- **Campfires**: Square markers showing campfire locations and states
- **Altitude**: Vertical position represents height on the mountain

## Campfire States

- **White**: Unlit campfire
- **Red**: Lit campfire
- **Gray**: Spent campfire

## Player States

- **Custom Color**: Normal player
- **Yellow**: Passed out player
- **Red**: Dead player

## Configuration

The mod can be configured through the BepInEx configuration file:

- `ModEnabled`: Enable/disable the minimap
- `MapWidth`: Width of the minimap (default: 200)
- `MapHeight`: Height of the minimap (default: 400)
- `MapOpacity`: Background opacity (default: 0.8)
- `ShowPlayerNames`: Show player names on dots (default: true)

## PEAK Systems Used

This mod utilizes the following PEAK systems:

- **Core Game Features**: Essential game functionality and basic systems
- **Multiplayer & Networking**: Photon networking, multiplayer systems, and online features
- **User Interface & UI Systems**: Unity UI, TextMeshPro, and custom UI components

## 📦 Installation

### Automatic (Mod Manager)

1. Install using r2modman or Thunderstore Mod Manager
2. Launch PEAK

### Manual

1. Make sure BepInEx is installed in your PEAK folder
2. Download the latest version of the mod
3. Extract `ClimbMap.dll` to `BepInEx/plugins/`
4. Launch PEAK

## ⚙️ Configuration

The mod can be configured in `BepInEx/config/com.nozz.climbmap.cfg`:

- **ModEnabled** (default: true) - Enable/disable the mod

## 🎮 How It Works

[Describe how your mod works here]

## 🔧 Development

This project is part of the PEAK mods monorepo.

### Build Locally

```bash
bun run build
```

### Create a Release

```bash
bun run release
```

## 🤝 Contributing

Feel free to submit issues or pull requests on GitHub.

## 📄 License

MIT License - See LICENSE file for details.
