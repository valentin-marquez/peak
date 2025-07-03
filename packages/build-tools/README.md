# @peak/build - Peak Mod Build Tools

Build tools for BepInEx mods for PEAK.

## 🚀 Quick Usage

```bash
# View all available commands
bun run @peak/build --help

# Initialize a new mod with interactive setup
bun run @peak/build init MyNewMod

# List available mods
bun run @peak/build list

# Build a mod with validation
bun run @peak/build build BagsForEveryone --validate

# Package a mod
bun run @peak/build package BagsForEveryone

# Complete release workflow (build + validate + package)
bun run @peak/build release BagsForEveryone

# Validate a built mod
bun run @peak/build validate BagsForEveryone
```

## 📋 Available Commands

### `init <modName> [options]`

Initialize a new mod project with interactive feature selection.

**Options:**

- `--description "description"` - Set mod description
- `--author "author"` - Set author name  
- `--skip-features` - Skip feature selection for minimal setup

**Features you can select:**

- **Core Game Features** - Essential game functionality
- **Multiplayer & Networking** - Photon networking and online features
- **User Interface & UI Systems** - Unity UI and custom components
- **Graphics & Rendering** - Rendering pipelines and visual effects
- **Animation & Rigging** - Animation systems and movement
- **Audio Systems** - Audio processing and voice chat
- **Input & Controller Support** - Input systems and controllers
- **Physics & Collision** - Physics and collision detection
- **AI & Navigation** - AI systems and pathfinding
- **Steam Integration** - Steam API and platform features
- **Performance & Optimization** - LOD systems and performance tools
- **Utilities & Tools** - Development tools and libraries

This command creates a complete mod structure with:

- C# project file with selected DLL references
- Plugin boilerplate code with feature initialization
- Manifest and package.json files
- README documentation
- Placeholder icon (replace with your 256x256 PNG)

### `build <modName> [--validate]`

Builds a mod for distribution.

- Compiles the C# project
- Copies essential files (excludes system DLLs)
- Automatically optimizes icon (256x256)
- Generates manifest.json and README.md dynamically
- `--validate` option to validate against Thunderstore requirements

### `package <modName>`

Packages a built mod into a Thunderstore-compatible ZIP.

### `release <modName>`

Complete release workflow: build → validate → package.

### `validate <modName>`

Validates a built mod against Thunderstore requirements:

- ✅ Required files: `icon.png`, `README.md`, `manifest.json`
- ✅ Icon: 256x256 PNG
- ✅ Manifest: all required fields
- ✅ Thunderstore compatibility

### `list`

Lists all available mods in the workspace.

### `optimize-icon <modName>`

Optimizes mod icon to 256x256 (Thunderstore requirement).

### `clean [--zip-only]`

Cleans build outputs from the dist folder.

- Without flags: removes all dist/ contents
- `--zip-only`: removes only ZIP files, keeps built mods

## 🎯 Convenient Aliases

```bash
# Short aliases available
bun run peak              # View help
bun run peak:init         # Initialize new mod
bun run peak:build        # Build mod
bun run peak:package      # Package mod  
bun run peak:release      # Complete release
bun run peak:validate     # Validate mod
bun run peak:list         # List mods
bun run peak:clean        # Clean dist files
```

## 📦 Thunderstore Requirements

The system ensures all packages comply with Thunderstore requirements:

### Required Files

- `icon.png` - PNG icon 256x256 pixels (at ZIP root)
- `README.md` - Documentation in markdown
- `manifest.json` - Package metadata

### Manifest Structure

```json
{
  "name": "ModName",
  "version_number": "1.0.0",
  "website_url": "https://github.com/...",
  "description": "Mod description (max 250 characters)",
  "dependencies": ["BepInEx-BepInExPack-5.4.2100"]
}
```

## 🔧 Configuration

The package configures automatically as part of the workspace. You only need:

1. Have your mod in the `mods/` folder
2. Ensure it has:
   - `manifest.json` with basic metadata
   - `assets/icon.png` (any size, optimized automatically)
   - C# project that compiles correctly

## 📁 Output Structure (dist folder only)

```text
dist/
├── ModName/                    # Uncompressed mod (clean structure)
│   ├── icon.png (256x256)      # Optimized icon for Thunderstore
│   ├── README.md               # Documentation
│   ├── manifest.json           # Package metadata
│   └── ModName.dll             # Main mod file
└── ModName_v1.0.0.zip         # Compressed package for Thunderstore
```

**Note**: The `assets/` folder is NOT included in final distribution as it contains source files not needed for Thunderstore.

## 🗜️ Advanced Compression

The system includes optimized compression:

- **Maximum compression level** (level 9)
- **Smart compression** by file type
- **Automatic exclusion** of temporary files
- **Detailed compression reports**
- **Performance optimization** with 32KB chunks

## ✅ Automatic Validation

The system includes automatic validation that verifies:

- Icon dimensions (256x256)
- Icon format (PNG)
- Presence of required files
- manifest.json structure
- Description character limits
- Semantic version format

## 🧹 File Management

```bash
# Clean only ZIP files (keep built mods)
bun run peak:clean --zip-only

# Clean entire dist directory
bun run peak:clean
```

## 🎉 Complete Example

```bash
# 1. Initialize a new mod with interactive setup
bun run peak:init MyAwesomeMod

# Follow the prompts:
# - Enter description: "An amazing mod for PEAK"
# - Enter author: "YourName"
# - Select features (y/N for each):
#   📦 Core Game Features: y
#   🌐 Multiplayer & Networking: y
#   🎨 User Interface & UI Systems: n
#   🎮 Input & Controller Support: y
#   ... (continue selecting what you need)

# 2. Navigate to your mod and customize
cd mods/MyAwesomeMod
# Replace assets/icon.png with your 256x256 icon
# Edit src/MyAwesomeModPlugin.cs to implement your logic

# 3. Do complete release with one command
bun run peak:release MyAwesomeMod

# 4. ZIP will be ready in dist/
# 5. Ready to upload to Thunderstore!
```

### Quick Setup Examples

```bash
# Minimal mod with no extra features
bun run peak:init QuickMod --skip-features

# Pre-configured mod with all info
bun run peak:init MyMod --description "Amazing PEAK mod" --author "YourName"

# UI-focused mod (you'll be prompted for features)
bun run peak:init UIEnhancer --description "Improves PEAK UI"
```

## 🐛 Troubleshooting

If you encounter errors, the system will show clear messages about what needs to be fixed:

- Missing files
- Icon dimension issues
- manifest.json errors
- C# compilation problems

## 🔄 Integration with Monorepo

This build system integrates seamlessly with the PEAK mods monorepo:

- **Automatic discovery** of mods in the workspace
- **Consistent build outputs** across all mods
- **Shared configuration** through workspace settings
- **TurboRepo compatibility** for caching and parallel builds

## 📊 Build Reports

The system provides detailed reports for each build:

- **File sizes** before and after compression
- **Compression ratios** achieved
- **Validation results** with specific issues
- **Build time** and performance metrics

---

**Happy modding with modern build tools!** 🎮✨
