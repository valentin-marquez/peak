# Peak Mods Monorepo

Modern monorepo for all my BepInEx mods for the game [PEAK](https://store.steampowered.com/app/3527290/PEAK/), managed with **TurboRepo** and **Bun**.

## 🎮 Included Mods

### [BagsForEveryone](./mods/BagsForEveryone/)

A minimalist mod that automatically generates backpacks for all players when starting a game.

**Features:**

- 🎒 Automatic backpack generation for all players
- 🎯 Smart scene detection (only works on levels Level_0 to Level_13)
- 🔄 Support for players joining mid-game
- 🌐 Multiplayer compatible (Photon networking)
- 🛡️ Duplicate prevention
- ⚡ Zero configuration - works from the first moment

## 🏗️ Monorepo Architecture

```
peak/
├── mods/                          # Individual mods (workspaces)
│   ├── BagsForEveryone/          # Automatic backpack mod
│   │   ├── src/                  # C# source code
│   │   ├── Release/              # Release files
│   │   ├── package.json          # Bun configuration
│   │   └── *.csproj              # .NET project
│   └── [Future mods]/         
├── scripts/                      # TypeScript automation scripts
│   ├── create-mod.ts            # Create new mods
│   └── release.ts               # Release management
├── .github/workflows/           # GitHub Actions CI/CD
├── package.json                 # Main monorepo configuration
├── turbo.json                   # TurboRepo configuration
├── tsconfig.json               # TypeScript configuration
├── Directory.Build.props       # Global MSBuild properties
├── NuGet.config               # NuGet configuration
└── Peak.sln                   # Visual Studio solution
```

## 🚀 Quick Start

### Prerequisites

- **Bun** >= 1.0.0 ([Install Bun](https://bun.sh/docs/installation))
- **.NET SDK** >= 6.0
- **Visual Studio 2022** or **VS Code** with C# extension
- **BepInEx** installed in your PEAK folder

### Initial Setup

1. **Clone and install:**

   ```bash
   git clone https://github.com/valentin-marquez/peak.git
   cd peak
   bun install
   ```

2. **Configure PEAK paths:**

   Edit `Directory.Build.props` and update the path to your PEAK installation:

   ```xml
   <PeakGamePath>D:\SteamLibrary\steamapps\common\PEAK</PeakGamePath>
   ```

3. **Build all mods:**

   ```bash
   bun run build
   ```

## 🛠️ Available Commands

### General Development

```bash
# Install dependencies
bun install

# Build all mods
bun run build

# Build specific mod
bun run build --filter=@peak-mods/bagsforeveryone

# Development mode with watch
bun run dev

# Clean builds
bun run clean

# Run tests
bun run test
```

### Mod Management

```bash
# Create a new mod
bun run create-mod MyNewMod

# Create mod release
bun run release BagsForEveryone 1.1.0

# Create release with Git tag
bun run release BagsForEveryone 1.1.0 --tag
```

### Using TurboRepo

```bash
# See what will be executed
bunx turbo run build --dry-run

# Execute with detailed cache
bunx turbo run build --verbosity=2

# Clear Turbo cache
bunx turbo prune
```

## 📦 Release Management

### Automatic Release

Releases are created automatically with GitHub Actions:

```bash
# 1. Commit your changes
git add .
git commit -m "feat: new functionality in BagsForEveryone"
git push origin main

# 2. Create and push the tag
git tag BagsForEveryone-v1.1.0
git push origin BagsForEveryone-v1.1.0

# 3. GitHub Actions handles the rest automatically
```

### Manual Release

```bash
# Prepare release locally
bun run release BagsForEveryone 1.1.0 --tag

# The script will create the ZIP and tag automatically
git push origin BagsForEveryone-v1.1.0
```

## 🆕 Creating a New Mod

### Quick Start with Interactive Setup

1. **Use the new init command:**

   ```bash
   bun run peak:init MyNewMod
   ```

2. **Follow the interactive prompts:**
   - Enter mod description and author
   - Select PEAK features your mod will use:
     - Core Game Features
     - Multiplayer & Networking  
     - UI Systems
     - Graphics & Rendering
     - Animation & Physics
     - And many more...

3. **Complete the setup:**

   ```bash
   cd mods/MyNewMod
   # Replace assets/icon.png with your 256x256 icon
   # Edit src/MyNewModPlugin.cs to implement your functionality
   bun run build
   ```

### Advanced Options

```bash
# Quick setup with minimal features
bun run peak:init MyMod --skip-features

# Pre-fill information
bun run peak:init MyMod --description "Amazing mod" --author "YourName"
```

### Manual Creation (Legacy)

1. **Use the generator:**

   ```bash
   bun run create-mod MyNewMod --description "My amazing mod for PEAK"
   ```

2. **Implement functionality:**

   ```bash
   cd mods/MyNewMod
   # Edit src/MyNewModPlugin.cs
   bun run build
   ```

3. **Configure release:**

   ```bash
   # Update Release/README.md and manifest.json
   bun run release
   ```

## 🔧 Development Configuration

### VS Code (Recommended)

The project includes pre-defined configuration for VS Code:

- **Tasks**: Build, clean, dev, create-mod, release
- **IntelliSense**: For C# and TypeScript
- **Problem Matchers**: For compilation errors

Quick commands:

- `Ctrl+Shift+P` → "Tasks: Run Task" → "Build All Mods"
- `Ctrl+Shift+P` → "Tasks: Run Task" → "Create New Mod"

### Visual Studio

- Open `Peak.sln` for full access to all mods
- Pre-configured Debug/Release configurations
- Full IntelliSense and debugging for C#

### Useful Environment Variables

```bash
# Windows
set PEAK_GAME_PATH=D:\SteamLibrary\steamapps\common\PEAK
set BEPINEX_PLUGIN_PATH=%PEAK_GAME_PATH%\BepInEx\plugins

# PowerShell
$env:PEAK_GAME_PATH="D:\SteamLibrary\steamapps\common\PEAK"
$env:BEPINEX_PLUGIN_PATH="$env:PEAK_GAME_PATH\BepInEx\plugins"
```

## 📋 Project Conventions

### Naming

- **Mods:** PascalCase (e.g: `BagsForEveryone`)
- **Packages:** `@peak-mods/{mod-name}` (e.g: `@peak-mods/bagsforeveryone`)
- **C# Namespaces:** `{ModName}` (e.g: `BagsForEveryone`)
- **BepInEx GUID:** `com.nozz.{modname}` (e.g: `com.nozz.bagsforeveryone`)

### Workspace Structure

Each mod is an independent workspace with:

- `package.json` - Bun configuration and scripts
- `src/` - C# source code
- `Release/` - Files ready for distribution
- `README.md` - Developer documentation

### Versioning

- **Semantic Versioning (semver)**: `MAJOR.MINOR.PATCH`
- **Git Tags**: `{ModName}-v{Version}` (e.g: `BagsForEveryone-v1.0.0`)
- **Independent releases**: Each mod has its own lifecycle

## 🚀 CI/CD with GitHub Actions

### Automatic Build

- **Trigger**: Push to `main` or `develop`, Pull Requests
- **Actions**: Install dependencies, build, test, upload artifacts

### Automated Release Pipeline

- **Trigger**: Push tags with format `*-v*`
- **Actions**:
  - Parse tag to extract mod and version
  - Build specific mod
  - Create ZIP package with BepInEx structure
  - Create GitHub Release with attached files
  - Automatic icon generation if missing

## 🤝 Contributing

1. Fork the repository
2. Create a branch: `git checkout -b feature/new-functionality`
3. Make your changes and tests: `bun run build && bun run test`
4. Commit: `git commit -am 'feat: add new functionality'`
5. Push: `git push origin feature/new-functionality`
6. Create a Pull Request

## 🔗 Useful Links

- **Bun Documentation**: <https://bun.sh/docs>
- **TurboRepo**: <https://turbo.build/repo/docs>
- **BepInEx**: <https://github.com/BepInEx/BepInEx>
- **PEAK on Steam**: <https://store.steampowered.com/app/3527290/PEAK/>

## 📄 License

MIT License - See [LICENSE](./LICENSE) file for details.

## 🙏 Acknowledgments

- **Peak Community** - For feedback and testing
- **BepInEx Team** - For the excellent modding framework
- **Thunderstore** - For the distribution platform
- **Bun & TurboRepo Teams** - For modern development tools

---

**Happy Modding with Modern Tools!** 🎮✨
