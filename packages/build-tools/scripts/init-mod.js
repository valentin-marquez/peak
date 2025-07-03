#!/usr/bin/env node

import { readdir, mkdir, writeFile, copyFile, access } from 'fs/promises';
import { join, basename, dirname } from 'path';
import { fileURLToPath } from 'url';
import { createInterface } from 'readline';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// PEAK DLL Categories for feature selection
const PEAK_DLL_CATEGORIES = {
    core: {
        name: "Core Game Features",
        description: "Essential game functionality and basic systems",
        dlls: [
            "Assembly-CSharp.dll",
            "Assembly-CSharp-firstpass.dll",
            "pworld.dll",
            "Zorro.Core.Runtime.dll"
        ]
    },
    networking: {
        name: "Multiplayer & Networking",
        description: "Photon networking, multiplayer systems, and online features",
        dlls: [
            "Photon3Unity3D.dll",
            "PhotonChat.dll",
            "PhotonRealtime.dll",
            "PhotonUnityNetworking.dll",
            "PhotonUnityNetworking.Demos.dll",
            "PhotonUnityNetworking.Utilities.dll",
            "PhotonVoice.API.dll",
            "PhotonVoice.dll",
            "PhotonVoice.PUN.dll",
            "Unity.Multiplayer.Center.Common.dll",
            "Unity.Multiplayer.Playmode.Common.Runtime.dll",
            "Unity.Multiplayer.Playmode.dll",
            "Unity.Networking.Transport.dll",
            "Unity.Services.Multiplayer.dll",
            "Zorro.PhotonUtility.dll"
        ]
    },
    ui: {
        name: "User Interface & UI Systems",
        description: "Unity UI, TextMeshPro, and custom UI components",
        dlls: [
            "UnityEngine.UI.dll",
            "UnityEngine.UIElementsModule.dll",
            "UnityEngine.UIModule.dll",
            "Unity.TextMeshPro.dll",
            "UnityUIExtensions.dll",
            "Zorro.UI.Runtime.dll"
        ]
    },
    graphics: {
        name: "Graphics & Rendering",
        description: "Rendering pipelines, shaders, visual effects, and graphics systems",
        dlls: [
            "Unity.RenderPipelines.Core.Runtime.dll",
            "Unity.RenderPipelines.Universal.Runtime.dll",
            "Unity.RenderPipelines.Universal.2D.Runtime.dll",
            "Unity.RenderPipelines.Universal.Config.Runtime.dll",
            "Unity.RenderPipelines.Universal.Shaders.dll",
            "Unity.VisualEffectGraph.Runtime.dll",
            "UnityEngine.VFXModule.dll",
            "HBAO.Runtime.dll",
            "HBAO.Universal.Runtime.dll",
            "sc.posteffects.runtime.dll",
            "Tayx.Graphy.dll"
        ]
    },
    animation: {
        name: "Animation & Rigging",
        description: "Animation systems, rigging, timelines, and movement",
        dlls: [
            "UnityEngine.AnimationModule.dll",
            "Unity.Animation.Rigging.dll",
            "Unity.Animation.Rigging.DocCodeExamples.dll",
            "Unity.Timeline.dll",
            "DOTween.dll",
            "DOTweenPro.dll",
            "DemiLib.dll",
            "Zorro.JiggleBones.dll"
        ]
    },
    audio: {
        name: "Audio Systems",
        description: "Audio processing, voice chat, and sound systems",
        dlls: [
            "UnityEngine.AudioModule.dll",
            "UnityEngine.DSPGraphModule.dll",
            "PhotonVoice.API.dll",
            "PhotonVoice.dll",
            "PhotonVoice.PUN.dll"
        ]
    },
    input: {
        name: "Input & Controller Support",
        description: "Input systems, controller support, and user interaction",
        dlls: [
            "Unity.InputSystem.dll",
            "Unity.InputSystem.ForUI.dll",
            "UnityEngine.InputModule.dll",
            "UnityEngine.InputLegacyModule.dll",
            "UnityEngine.InputForUIModule.dll",
            "Zorro.ControllerSupport.dll"
        ]
    },
    physics: {
        name: "Physics & Collision",
        description: "Physics systems, collision detection, and spatial queries",
        dlls: [
            "UnityEngine.PhysicsModule.dll",
            "UnityEngine.Physics2DModule.dll",
            "UnityEngine.TerrainPhysicsModule.dll"
        ]
    },
    ai: {
        name: "AI & Navigation",
        description: "AI systems, pathfinding, and navigation mesh",
        dlls: [
            "UnityEngine.AIModule.dll",
            "Unity.AI.Navigation.dll"
        ]
    },
    utilities: {
        name: "Utilities & Tools",
        description: "Development tools, serialization, and utility libraries",
        dlls: [
            "Sirenix.OdinInspector.Attributes.dll",
            "Sirenix.OdinInspector.Modules.Unity.Addressables.dll",
            "Sirenix.OdinInspector.Modules.UnityLocalization.dll",
            "Sirenix.Serialization.Config.dll",
            "Sirenix.Serialization.dll",
            "Sirenix.Utilities.dll",
            "Newtonsoft.Json.dll",
            "Unity.Mathematics.dll",
            "Unity.Collections.dll",
            "Unity.Burst.dll",
            "Unity.Burst.Unsafe.dll"
        ]
    },
    steam: {
        name: "Steam Integration",
        description: "Steam API, achievements, and platform features",
        dlls: [
            "com.rlabrecque.steamworks.net.dll"
        ]
    },
    optimization: {
        name: "Performance & Optimization",
        description: "LOD systems, mesh optimization, and performance tools",
        dlls: [
            "Ashley.MeshSplitter.dll",
            "Whinarn.UnityMeshSimplifier.Runtime.dll",
            "Zorro.AutoLOD.dll",
            "Unity.Profiling.Core.dll",
            "Unity.MemoryProfiler.dll"
        ]
    }
};

// Templates for different file types
const TEMPLATES = {
    csproj: (modName, selectedFeatures) => {
        const references = generateReferences(selectedFeatures);
        return `<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net471</TargetFramework>
    <AssemblyTitle>${modName}</AssemblyTitle>
    <AssemblyDescription>A BepInEx mod for PEAK</AssemblyDescription>
    <AssemblyVersion>1.0.0</AssemblyVersion>
    <FileVersion>1.0.0</FileVersion>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <RestoreAdditionalProjectSources>
      https://api.nuget.org/v3/index.json;
      https://nuget.bepinex.dev/v3/index.json
    </RestoreAdditionalProjectSources>
    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <PropertyGroup>
    <!-- Plugin Info for BepInEx -->
    <BepInExPluginGuid>com.nozz.${modName.toLowerCase()}</BepInExPluginGuid>
    <BepInExPluginName>${modName}</BepInExPluginName>
    <BepInExPluginVersion>\$(AssemblyVersion)</BepInExPluginVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BepInEx.Analyzers" Version="1.*" PrivateAssets="all" />
    <PackageReference Include="BepInEx.Core" Version="5.*" />
  </ItemGroup>

  <ItemGroup Condition="'\$(TargetFramework.TrimEnd(\`0123456789\`))' == 'net'">
    <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.2" PrivateAssets="all" />
  </ItemGroup>

  <!-- Essential PEAK game references -->
  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>\$(PeakGamePath)\\PEAK_Data\\Managed\\Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine">
      <HintPath>\$(PeakGamePath)\\PEAK_Data\\Managed\\UnityEngine.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>\$(PeakGamePath)\\PEAK_Data\\Managed\\UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

${references}

  <!-- PostBuild: Copy DLL to plugins folder for testing -->
  <Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'\$(Configuration)' == 'Release'">
    <ItemGroup>
      <OutputFiles Include="\$(TargetPath)" />
    </ItemGroup>
    
    <!-- Copy to plugins folder for testing -->
    <Copy SourceFiles="@(OutputFiles)" 
          DestinationFolder="\$(PeakGamePath)\\BepInEx\\plugins\\${modName}\\" 
          ContinueOnError="true" />
    
    <Message Text="${modName}.dll copied to:" Importance="high" />
    <Message Text="  - \$(PeakGamePath)\\BepInEx\\plugins\\${modName}\\" Importance="high" />
  </Target>

</Project>`;
    },

    plugin: (modName, selectedFeatures) => {
        const usings = generateUsings(selectedFeatures);
        const features = generateFeatureCode(selectedFeatures);

        return `using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
${usings}

namespace ${modName}
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ${modName}Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        
        // Configuration
        private ConfigEntry<bool> modEnabled;
        
        private void Awake()
        {
            Logger = base.Logger;
            
            // Configuration setup
            modEnabled = Config.Bind("General", "ModEnabled", true, "Enable/disable the mod");
            
            if (!modEnabled.Value)
            {
                Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is disabled!");
                return;
            }
            
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            
            // Initialize selected features
${features}
        }
        
        private void Start()
        {
            if (!modEnabled.Value) return;
            
            // Start logic here
        }
        
        private void Update()
        {
            if (!modEnabled.Value) return;
            
            // Update logic here
        }
        
        private void OnDisable()
        {
            // Cleanup logic here
        }
    }
}`;
    },

    // Remove pluginInfo since it's auto-generated by BepInEx.PluginInfoProps

    pluginInfo: (modName) => `namespace ${modName}
{
    internal static class MyPluginInfo
    {
        internal const string PLUGIN_GUID = "com.nozz.${modName.toLowerCase()}";
        internal const string PLUGIN_NAME = "${modName}";
        internal const string PLUGIN_VERSION = "1.0.0";
    }
}`,

    manifest: (modName, description, author) => ({
        name: modName,
        version_number: "1.0.0",
        website_url: "https://github.com/your-username/peak-mods",
        description: description || `A PEAK mod that enhances the game experience`,
        dependencies: ["BepInEx-BepInExPack-5.4.2100"]
    }),

    packageJson: (modName) => ({
        name: `@peak-mods/${modName.toLowerCase()}`,
        version: "1.0.0",
        description: `PEAK mod: ${modName}`,
        private: true,
        scripts: {
            build: "@peak/build build " + modName,
            package: "@peak/build package " + modName,
            release: "@peak/build release " + modName,
            validate: "@peak/build validate " + modName,
            clean: "@peak/build clean"
        },
        dependencies: {
            "@peak/build": "workspace:*"
        }
    }),

    readme: (modName, description, selectedFeatures) => {
        const featuresText = Object.keys(selectedFeatures)
            .filter(key => selectedFeatures[key])
            .map(key => `- **${PEAK_DLL_CATEGORIES[key].name}**: ${PEAK_DLL_CATEGORIES[key].description}`)
            .join('\n');

        return `# ${modName}

${description || 'A minimalist mod for PEAK that enhances the game experience.'}

## 🚀 Features

This mod utilizes the following PEAK systems:

${featuresText}

## 📦 Installation

### Automatic (Mod Manager)
1. Install using r2modman or Thunderstore Mod Manager
2. Launch PEAK

### Manual
1. Make sure BepInEx is installed in your PEAK folder
2. Download the latest version of the mod
3. Extract \`${modName}.dll\` to \`BepInEx/plugins/\`
4. Launch PEAK

## ⚙️ Configuration

The mod can be configured in \`BepInEx/config/com.nozz.${modName.toLowerCase()}.cfg\`:

- **ModEnabled** (default: true) - Enable/disable the mod

## 🎮 How It Works

[Describe how your mod works here]

## 🔧 Development

This project is part of the PEAK mods monorepo.

### Build Locally
\`\`\`bash
bun run build
\`\`\`

### Create a Release
\`\`\`bash
bun run release
\`\`\`

## 🤝 Contributing

Feel free to submit issues or pull requests on GitHub.

## 📄 License

MIT License - See LICENSE file for details.
`;
    }
};

function generateReferences(selectedFeatures) {
    const selectedDlls = new Set();

    Object.keys(selectedFeatures).forEach(category => {
        if (selectedFeatures[category]) {
            PEAK_DLL_CATEGORIES[category].dlls.forEach(dll => {
                selectedDlls.add(dll);
            });
        }
    });

    if (selectedDlls.size === 0) return '';

    const references = Array.from(selectedDlls)
        .map(dll => `    <Reference Include="${dll.replace('.dll', '')}">\n      <HintPath>$(PeakGamePath)\\PEAK_Data\\Managed\\${dll}</HintPath>\n    </Reference>`)
        .join('\n');

    return `  <ItemGroup>\n${references}\n  </ItemGroup>`;
}

function generateUsings(selectedFeatures) {
    const usings = new Set();

    if (selectedFeatures.networking) {
        usings.add('using Photon.Pun;');
        usings.add('using Photon.Realtime;');
    }

    if (selectedFeatures.ui) {
        usings.add('using UnityEngine.UI;');
        usings.add('using TMPro;');
    }

    if (selectedFeatures.input) {
        usings.add('using UnityEngine.InputSystem;');
    }

    if (selectedFeatures.physics) {
        usings.add('using UnityEngine;');
    }

    if (selectedFeatures.audio) {
        usings.add('using UnityEngine;');
    }

    return Array.from(usings).join('\n');
}

function generateFeatureCode(selectedFeatures) {
    const codeBlocks = [];

    if (selectedFeatures.networking) {
        codeBlocks.push('            // Networking features initialized');
        codeBlocks.push('            Logger.LogInfo("Photon networking support enabled");');
    }

    if (selectedFeatures.ui) {
        codeBlocks.push('            // UI features initialized');
        codeBlocks.push('            Logger.LogInfo("UI system support enabled");');
    }

    if (selectedFeatures.input) {
        codeBlocks.push('            // Input features initialized');
        codeBlocks.push('            Logger.LogInfo("Input system support enabled");');
    }

    if (selectedFeatures.physics) {
        codeBlocks.push('            // Physics features initialized');
        codeBlocks.push('            Logger.LogInfo("Physics system support enabled");');
    }

    if (selectedFeatures.ai) {
        codeBlocks.push('            // AI features initialized');
        codeBlocks.push('            Logger.LogInfo("AI and navigation support enabled");');
    }

    if (selectedFeatures.audio) {
        codeBlocks.push('            // Audio features initialized');
        codeBlocks.push('            Logger.LogInfo("Audio system support enabled");');
    }

    if (selectedFeatures.steam) {
        codeBlocks.push('            // Steam integration initialized');
        codeBlocks.push('            Logger.LogInfo("Steam API support enabled");');
    }

    return codeBlocks.length > 0 ? codeBlocks.join('\n') : '            // No additional features selected';
}

async function promptUser(question) {
    const rl = createInterface({
        input: process.stdin,
        output: process.stdout
    });

    return new Promise((resolve) => {
        rl.question(question, (answer) => {
            rl.close();
            resolve(answer);
        });
    });
}

async function promptFeatureSelection() {
    console.log('\n🎯 Select the PEAK features your mod will use:\n');

    const features = {};

    for (const [key, category] of Object.entries(PEAK_DLL_CATEGORIES)) {
        console.log(`📦 ${category.name}`);
        console.log(`   ${category.description}`);
        console.log(`   DLLs: ${category.dlls.slice(0, 3).join(', ')}${category.dlls.length > 3 ? '...' : ''}`);

        const answer = await promptUser(`   Include this feature? (y/N): `);
        features[key] = answer.toLowerCase() === 'y' || answer.toLowerCase() === 'yes';
        console.log('');
    }

    return features;
}

async function createDefaultIcon(assetsDir) {
    const iconPath = join(assetsDir, 'icon.png');

    try {
        // Copy the icon from BagsForEveryone as a template
        const templateIconPath = join(process.cwd(), 'mods', 'BagsForEveryone', 'assets', 'icon.png');
        await copyFile(templateIconPath, iconPath);
        console.log(`📱 Created placeholder icon at ${iconPath}`);
        console.log(`   ✅ Using BagsForEveryone icon as template`);
        console.log(`   ⚠️  Replace this with your actual 256x256 PNG icon`);
    } catch (error) {
        // If template icon doesn't exist, create a minimal valid PNG
        const minimalPng = Buffer.from([
            // PNG signature
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            // IHDR chunk for 1x1 image
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89,
            // IDAT chunk
            0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54,
            0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01,
            0x0D, 0x0A, 0x2D, 0xB4,
            // IEND chunk
            0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        ]);

        await writeFile(iconPath, minimalPng);
        console.log(`📱 Created minimal placeholder icon at ${iconPath}`);
        console.log(`   ⚠️  Replace this with your actual 256x256 PNG icon`);
    }
}

export async function initializeMod(modName, options = {}) {
    try {
        console.log(`🚀 Initializing new PEAK mod: ${modName}\n`);

        // Get basic info
        const description = options.description || await promptUser('📝 Mod description: ');
        const author = options.author || await promptUser('👤 Author name: ');

        // Feature selection
        const selectedFeatures = options.skipFeatures ? {} : await promptFeatureSelection();

        // Create directory structure
        const modsDir = join(process.cwd(), 'mods');
        const modDir = join(modsDir, modName);

        // Check if mod already exists
        try {
            await access(modDir);
            throw new Error(`Mod '${modName}' already exists!`);
        } catch (error) {
            if (error.code !== 'ENOENT') throw error;
        }

        console.log('📁 Creating directory structure...');
        await mkdir(modDir, { recursive: true });
        await mkdir(join(modDir, 'src'), { recursive: true });
        await mkdir(join(modDir, 'assets'), { recursive: true });

        // Generate files
        console.log('📄 Generating project files...');

        // .csproj file
        await writeFile(
            join(modDir, `${modName}.csproj`),
            TEMPLATES.csproj(modName, selectedFeatures)
        );

        // Main plugin file
        await writeFile(
            join(modDir, 'src', `${modName}Plugin.cs`),
            TEMPLATES.plugin(modName, selectedFeatures)
        );

        // Plugin info file
        await writeFile(
            join(modDir, 'src', 'MyPluginInfo.cs'),
            TEMPLATES.pluginInfo(modName)
        );

        // manifest.json
        await writeFile(
            join(modDir, 'manifest.json'),
            JSON.stringify(TEMPLATES.manifest(modName, description, author), null, 2)
        );

        // package.json
        await writeFile(
            join(modDir, 'package.json'),
            JSON.stringify(TEMPLATES.packageJson(modName), null, 2)
        );

        // README.md
        await writeFile(
            join(modDir, 'README.md'),
            TEMPLATES.readme(modName, description, selectedFeatures)
        );

        // Create placeholder icon
        await createDefaultIcon(join(modDir, 'assets'));

        // Summary
        console.log('\n✅ Mod initialization complete!\n');
        console.log(`📦 Mod Name: ${modName}`);
        console.log(`📁 Location: ${modDir}`);
        console.log(`🎯 Features: ${Object.keys(selectedFeatures).filter(k => selectedFeatures[k]).length} selected`);

        const selectedCount = Object.values(selectedFeatures).filter(Boolean).length;
        if (selectedCount > 0) {
            console.log('\n🔧 Selected Features:');
            Object.keys(selectedFeatures)
                .filter(key => selectedFeatures[key])
                .forEach(key => {
                    console.log(`   ✓ ${PEAK_DLL_CATEGORIES[key].name}`);
                });
        }

        console.log('\n🚀 Next Steps:');
        console.log(`   1. cd mods/${modName}`);
        console.log(`   2. Replace assets/icon.png with your 256x256 PNG icon`);
        console.log(`   3. Edit src/${modName}Plugin.cs to implement your mod logic`);
        console.log(`   4. Run 'bun run build' to build your mod`);
        console.log(`   5. Run 'bun run release' to create a Thunderstore package`);

    } catch (error) {
        console.error('❌ Error initializing mod:', error.message);
        process.exit(1);
    }
}

// CLI usage
if (import.meta.url === `file://${process.argv[1]}`) {
    const modName = process.argv[2];

    if (!modName) {
        console.error('Usage: init-mod.js <ModName> [--skip-features]');
        process.exit(1);
    }

    const options = {
        skipFeatures: process.argv.includes('--skip-features')
    };

    initializeMod(modName, options);
}
