# PEAK Mods

<div align="center">

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Bun](https://img.shields.io/badge/Bun-1.0+-black?logo=bun&logoColor=white)](https://bun.sh)
[![TurboRepo](https://img.shields.io/badge/TurboRepo-Monorepo-blue?logo=turborepo)](https://turbo.build/repo)
[![BepInEx](https://img.shields.io/badge/BepInEx-5.x-green)](https://github.com/BepInEx/BepInEx)

*Modern monorepo for BepInEx mods for [PEAK](https://store.steampowered.com/app/3527290/PEAK/)*

</div>

## 🎮 Available Mods

| Mod | Version | Description | Downloads |
|-----|---------|-------------|-----------|
| [**BagsForEveryone**](./mods/BagsForEveryone/) | `1.0.0` | Automatically generates backpacks for all players | [Download](https://github.com/valentin-marquez/peak/releases) |

### Features Overview

| Feature | BagsForEveryone |
|---------|----------------|
| � Auto Backpack Generation | ✅ |
| 🎯 Smart Scene Detection | ✅ |
| 🔄 Mid-Game Support | ✅ |
| 🌐 Multiplayer Compatible | ✅ |
| 🛡️ Duplicate Prevention | ✅ |
| ⚡ Zero Configuration | ✅ |

## 🚀 Quick Start

### For Players

1. **Install BepInEx** in your PEAK folder
2. **Download mod** from [Releases](https://github.com/valentin-marquez/peak/releases)
3. **Extract** to `BepInEx/plugins/`
4. **Launch** PEAK

### For Developers

```bash
# Create a new mod
bun run peak:init MyNewMod

# Build and release
bun run peak:release MyNewMod
```

## �️ Development

This monorepo uses modern tooling for efficient mod development:

- **[Bun](https://bun.sh)** - Fast JavaScript runtime
- **[TurboRepo](https://turbo.build/repo)** - Monorepo management
- **[@peak/build](./packages/build-tools/)** - Custom build tools
- **[BepInEx](https://github.com/BepInEx/BepInEx)** - Modding framework

### Commands

| Command | Description |
|---------|-------------|
| `bun run peak:init <name>` | Create new mod with interactive setup |
| `bun run peak:build <name>` | Build specific mod |
| `bun run peak:release <name>` | Complete release workflow |
| `bun run peak:list` | List all available mods |

## 📄 License

MIT License - See [LICENSE](./LICENSE) file for details.

---

<div align="center">

**Happy Modding!** 🎮✨

</div>
