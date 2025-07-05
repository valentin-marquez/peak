# HDPeak - Advanced Graphics Optimization

Advanced graphics and performance optimization mod for PEAK - Customize resolution, shadows, fog, anti-aliasing, LOD, and texture quality for optimal performance and visual quality.

Based on HDLethalCompany but designed specifically for PEAK's Unity engine and rendering pipeline using PEAK's native graphics systems.

## 📦 Installation

### Automatic (Mod Manager)

1. Install using r2modman or Thunderstore Mod Manager
2. Launch PEAK

### Manual

1. Make sure BepInEx is installed in your PEAK folder
2. Download the latest version of the mod
3. Extract `HDPeak.dll` to `BepInEx/plugins/`
4. Launch PEAK

## ⚙️ Configuration

The mod creates a comprehensive configuration file at `BepInEx/config/com.nozz.hdpeak.cfg`:

### Resolution Settings

```ini
[RESOLUTION]
# Resolution scale multiplier
# 0.5 = 50% (better performance)
# 1.0 = 100% (native resolution)
# 1.5 = 150% (better quality)
ResolutionMultiplier = 1.0

# Enable custom resolution scaling
EnableResolutionFix = true
```

### Graphics Effects

```ini
[EFFECTS]
# Enable post-processing effects
EnablePostProcessing = true

# Enable volumetric fog
EnableFog = true

# Enable anti-aliasing
EnableAntiAliasing = false

# Enable foliage rendering
EnableFoliage = true
```

### Quality Settings

```ini
[QUALITY]
# Texture Quality: 0=Very Low, 1=Low, 2=Medium, 3=High
TextureQuality = 3

# Shadow Quality: 0=Disabled, 1=Low, 2=Medium, 3=High
ShadowQuality = 3

# LOD Quality: 0=Low, 1=Medium, 2=High
LODQuality = 1

# Fog Quality: 0=Very Low, 1=Low, 2=Medium, 3=High
FogQuality = 1
```

## 🎯 Recommended Settings

### 🚀 Performance Focus (Low-End Hardware)

- ResolutionMultiplier: `0.8`
- TextureQuality: `1`
- ShadowQuality: `1`
- LODQuality: `0`
- EnableFog: `false`
- EnableFoliage: `false`

### ⚖️ Balanced (Mid-Range Hardware)

- ResolutionMultiplier: `1.0`
- TextureQuality: `2`
- ShadowQuality: `2`
- LODQuality: `1`
- EnableFog: `true`
- FogQuality: `1`

### 🎨 Visual Quality (High-End Hardware)

- ResolutionMultiplier: `1.2`
- TextureQuality: `3`
- ShadowQuality: `3`
- LODQuality: `2`
- EnableAntiAliasing: `true`
- FogQuality: `3`

## 🔧 Advanced Features

### Automatic Hardware Detection

The mod automatically detects your hardware and suggests optimal settings:

- **6GB+ VRAM & 8+ CPU cores**: High settings
- **4GB+ VRAM & 6+ CPU cores**: Medium settings
- **2GB+ VRAM & 4+ CPU cores**: Low settings
- **Below 2GB VRAM**: Very low settings

### Performance Monitoring

- Real-time FPS tracking
- Automatic quality adjustment when performance drops
- Memory usage optimization
- Frame time analysis

## 📊 Performance Impact

| Setting | Performance Impact | Visual Impact |
|---------|-------------------|---------------|
| Resolution 0.8x | +20-30% FPS | Slightly softer image |
| Shadows Low | +10-15% FPS | Reduced shadow quality |
| Fog Disabled | +5-10% FPS | No volumetric fog |
| Foliage Disabled | +5-15% FPS | Less vegetation detail |
| Texture Low | +5-10% FPS | Reduced texture detail |

## 🐛 Troubleshooting

### Common Issues

**Game crashes on startup**:

- Verify BepInEx installation
- Check mod compatibility
- Try default settings first

**No visual changes**:

- Restart the game after config changes
- Check if settings are being overridden
- Verify mod is loading in BepInEx console

**Performance worse than before**:

- Try performance-focused settings
- Disable features gradually
- Check system specifications

## 🤝 Compatibility

### Compatible Mods

- Most BepInEx mods
- UI enhancement mods
- Gameplay modification mods

### Incompatible Mods

- Other graphics modification mods
- Resolution override mods
- Conflicting post-processing mods

## 🆘 Support

- **GitHub Issues**: [Report bugs and request features](https://github.com/valentin-marquez/peak/issues)
- **Discord**: Join the PEAK modding community
- **Documentation**: Check the wiki for detailed guides

## 📝 Changelog

### Version 1.0.0

- Initial release
- Basic graphics optimization
- Resolution scaling
- Quality presets
- Performance monitoring

## 🙏 Credits

- **Inspired by**: HDLethalCompany by Sligili
- **Built with**: BepInEx, Harmony
- **Tested by**: PEAK modding community

## 📄 License

This mod is licensed under the MIT License - see the LICENSE file for details.

---

**⚠️ Note**: This mod modifies rendering settings and may impact game performance. Always backup your save files before installing mods.

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
