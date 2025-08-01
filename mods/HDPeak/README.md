# HDPeak

Advanced graphics settings mod for PEAK. Provides enhanced visual quality and performance optimization options.

## Features

### Graphics Quality Settings

| Setting                   | Description                                 | Options                        | Performance Impact                                 |
|---------------------------|---------------------------------------------|--------------------------------|----------------------------------------------------|
| **Anti-Aliasing**         | Reduces jagged edges on 3D objects          | Off, 2x, 4x, 8x MSAA           | Higher = smoother edges, lower performance         |
| **Anisotropic Filtering** | Improves texture quality at distance        | Disable, Enable, Force Enable  | Higher = sharper distant textures                  |
| **Texture Quality**       | Controls texture resolution                 | Very Low → Very High           | Higher = sharper textures, more VRAM usage         |
| **Shadow Resolution**     | Controls shadow map quality                 | Very Low (256px) → Ultra (8192px) | Higher = sharper shadows, more GPU usage      |
| **LOD Bias**              | Controls model detail distance              | 0.5 → 2.0                      | Higher = more detail at distance, lower performance|
| **Far Plane Culling Distance** | Sets the camera's far clipping plane distance | 100.0 → 2000.0 (float)         | Higher = renders more distant objects, lower performance |

### Performance Settings

| Setting                   | Description                                 | Options                        | Performance Impact                                 |
|---------------------------|---------------------------------------------|--------------------------------|----------------------------------------------------|
| **Opaque Texture**        | Camera opaque texture for effects           | Disabled, Enabled              | Disabled = Better performance                      |
| **Max Additional Lights** | Maximum real-time lights                    | Very Low (1) → Very High (8)   | Higher = more dynamic lights, lower performance    |
| **Dynamic Batching**      | Optimizes rendering of small objects        | Disabled, Enabled              | Disabled = Better for complex scenes               |

### Implementation Notes

- **Far Plane Culling Distance** uses a `FloatSetting` for user adjustment due to UI limitations with `IntSetting`. This allows decimal input, but the value is treated as an integer internally.
- A field for the Harmony instance is added to the plugin class for patch management.
- A custom setting class for the culling distance is registered in the `SettingsRegistry`.
- Harmony is used to patch the game's `MainCamera` class, ensuring the culling distance updates in real time.

## Installation

1. Install BepInEx
2. Place `HDPeak.dll` in `BepInEx/plugins/`
3. Launch game and access settings via Options menu

## Requirements

- BepInEx 5.x
- PEAK game
