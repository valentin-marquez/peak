# ClimbTunes 🎵

**Portable radio for mountaineers. Play your YouTube music while conquering peaks... or while rolling down the mountain. The perfect soundtrack for your epic falls!**

## 🚀 Features

ClimbTunes introduces a fully functional portable radio system to PEAK, allowing you to:

- **🎧 Play YouTube Music**: Stream your favorite YouTube videos and playlists as audio
- **📻 Portable Radio**: Physical radio objects that can be placed anywhere in the world
- **🎵 3D Audio**: Spatial audio that gets louder as you get closer to the radio
- **🎨 Audio Visualization**: Visual effects that respond to the music (pulsing lights, particles)
- **🌐 Multiplayer Sync**: Share your music with other players in real-time
- **🎛️ Full Controls**: Play, pause, stop, skip tracks, and adjust volume
- **📱 Intuitive UI**: Easy-to-use interface for managing your music
- **🎪 Auto-Spawn**: Automatically gives every player a radio when the game starts

This mod utilizes the following PEAK systems:

- **Core Game Features**: Essential game functionality and basic systems
- **Multiplayer & Networking**: Photon networking, multiplayer systems, and online features
- **User Interface & UI Systems**: Unity UI, TextMeshPro, and custom UI components
- **Audio Systems**: Audio processing, voice chat, and sound systems

## 📦 Installation

### Automatic (Mod Manager)

1. Install using r2modman or Thunderstore Mod Manager
2. Launch PEAK

### Manual

1. Make sure BepInEx is installed in your PEAK folder
2. Download the latest version of the mod
3. Extract `ClimbTunes.dll` to `BepInEx/plugins/`
4. Launch PEAK

## 🎮 How to Use

### Getting Started

1. **Auto-Spawn**: When you start a game, a radio will automatically spawn near you
2. **Manual Spawn**: You can also spawn radios by using the mod's commands
3. **Pick up and Place**: Grab your radio and place it wherever you want music

### Controls

- **Press `R`** to open the radio control UI
- **Enter YouTube URL**: Paste any YouTube video or playlist URL
- **Control Playback**: Use Play, Pause, Stop, and Skip buttons
- **Volume Control**: Adjust the radio volume to your preference

### Supported URLs

- YouTube videos: `https://www.youtube.com/watch?v=VIDEO_ID`
- YouTube playlists: `https://www.youtube.com/playlist?list=PLAYLIST_ID`
- Direct audio URLs: Any direct link to audio files (MP3, OGG, etc.)

### Radio Features

- **3D Spatial Audio**: The radio gets louder as you get closer
- **Visual Effects**: Pulsing lights and particle effects that react to the music
- **Multiplayer Sync**: Other players can hear your music and see the effects
- **Persistent**: Radios stay in the world and continue playing

## ⚙️ Configuration

The mod can be configured in `BepInEx/config/com.nozz.climbtunes.cfg`:

```ini
[General]
ModEnabled = true

[Audio]
RadioVolume = 0.7
DefaultPlaylist = https://www.youtube.com/playlist?list=YOUR_PLAYLIST_ID

[Gameplay]
AutoSpawnRadio = true

[Controls]
RadioToggleKey = R
```

### Configuration Options

- **ModEnabled**: Enable or disable the entire mod
- **RadioVolume**: Default volume for new radios (0.0 to 1.0)
- **DefaultPlaylist**: Default YouTube playlist to load
- **AutoSpawnRadio**: Automatically spawn radios for players at game start
- **RadioToggleKey**: Key to open/close the radio control UI

## � Music Recommendations

Here are some great playlists to get you started:

- **Epic Climbing**: High-energy tracks for challenging climbs
- **Chill Vibes**: Relaxing music for scenic mountain views
- **Rock Anthems**: Classic rock for when you're feeling unstoppable
- **Electronic Beats**: EDM and electronic music for modern mountaineers

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

## 🐛 Known Issues

- **YouTube Audio**: Due to YouTube's restrictions, actual audio extraction requires additional setup
- **Large Files**: Very long audio files may cause memory issues
- **Network Lag**: Audio synchronization may have slight delays on high-latency connections

## 🤝 Contributing

Feel free to submit issues or pull requests on GitHub.

## 📝 Changelog

### v1.0.0

- Initial release
- Basic radio functionality
- YouTube URL support
- 3D spatial audio
- Visual effects and audio visualization
- Multiplayer synchronization
- Auto-spawn feature
- Configuration system

## 🙏 Credits

- **PEAK Game**: For creating an amazing climbing experience
- **BepInEx**: For the excellent modding framework
- **Unity**: For the powerful game engine
- **Photon**: For networking capabilities

## 📄 License

MIT License - See LICENSE file for details.

---

**Made with ❤️ for the PEAK community**

*Climb high, play loud! 🏔️🎵*
