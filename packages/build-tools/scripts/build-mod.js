#!/usr/bin/env node
import fs from 'fs-extra';
import path from 'path';
import chalk from 'chalk';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * Enhanced build script for PEAK mods with dynamic release generation
 */
class ModBuilder {
    constructor() {
        this.projectRoot = path.resolve(__dirname, '../../../');
        this.modsDir = path.join(this.projectRoot, 'mods');
        this.distDir = path.join(this.projectRoot, 'dist');
    }

    async buildMod(modName) {
        console.log(chalk.blue(`🔨 Building mod: ${modName}`));

        const modPath = path.join(this.modsDir, modName);

        if (!await fs.pathExists(modPath)) {
            throw new Error(`Mod directory not found: ${modPath}`);
        }

        // Ensure dist directory exists
        await fs.ensureDir(this.distDir);

        // Clean previous build
        const modDistPath = path.join(this.distDir, modName);
        await fs.remove(modDistPath);
        await fs.ensureDir(modDistPath);

        // Read mod configuration
        const manifestPath = path.join(modPath, 'manifest.json');
        const packagePath = path.join(modPath, 'package.json');

        let manifest, packageJson;

        if (await fs.pathExists(manifestPath)) {
            manifest = await fs.readJSON(manifestPath);
        }

        if (await fs.pathExists(packagePath)) {
            packageJson = await fs.readJSON(packagePath);
        }

        // Build the C# project
        console.log(chalk.yellow('📦 Building C# project...'));
        try {
            execSync('dotnet build --configuration Release', {
                cwd: modPath,
                stdio: 'inherit'
            });
        } catch (error) {
            throw new Error(`Failed to build C# project: ${error.message}`);
        }

        // Copy build outputs (only the main mod DLL and essential dependencies)
        const binPath = path.join(modPath, 'bin', 'Release');
        const dllFiles = await fs.readdir(binPath);

        // Filter to only include the main mod DLL and essential BepInEx dependencies
        const essentialFiles = [
            `${modName}.dll`, // Main mod DLL
            'BepInEx.dll',
            'BepInEx.Core.dll',
            'BepInEx.Harmony.dll'
        ];

        for (const file of dllFiles) {
            if (file.endsWith('.dll')) {
                // Only copy essential DLLs
                if (essentialFiles.some(essential => file === essential) || file === `${modName}.dll`) {
                    await fs.copy(
                        path.join(binPath, file),
                        path.join(modDistPath, file)
                    );
                    console.log(chalk.green(`✅ Copied essential: ${file}`));
                } else {
                    console.log(chalk.yellow(`⚠️  Skipped system dependency: ${file}`));
                }
            }
        }

        // Copy additional assets
        await this.copyAssets(modPath, modDistPath);

        // Generate dynamic manifest (README is now copied in copyAssets)
        await this.generateManifest(modDistPath, manifest, packageJson);

        console.log(chalk.green(`✅ Mod built successfully: ${modDistPath}`));
        return modDistPath;
    }

    async copyAssets(modPath, distPath) {
        // Copy and optimize icon.png to root (required by Thunderstore at 256x256)
        const iconPath = path.join(modPath, 'assets', 'icon.png');
        if (await fs.pathExists(iconPath)) {
            const outputIconPath = path.join(distPath, 'icon.png');

            // Use sharp to resize and optimize the icon for Thunderstore
            const sharp = (await import('sharp')).default;
            await sharp(iconPath)
                .resize(256, 256, {
                    fit: 'cover',
                    position: 'center'
                })
                .png({
                    quality: 90,
                    compressionLevel: 6
                })
                .toFile(outputIconPath);

            console.log(chalk.cyan(`📁 Optimized and copied icon.png to root (256x256)`));
        }

        // Copy README.md from mod directory if it exists, otherwise skip auto-generation
        const readmePath = path.join(modPath, 'README.md');
        if (await fs.pathExists(readmePath)) {
            await fs.copy(readmePath, path.join(distPath, 'README.md'));
            console.log(chalk.cyan(`📁 Copied README.md to root`));
        }

        // Copy other optional files to root if they exist
        const rootAssets = ['LICENSE', 'CHANGELOG.md'];
        for (const asset of rootAssets) {
            const assetPath = path.join(modPath, asset);
            if (await fs.pathExists(assetPath)) {
                await fs.copy(assetPath, path.join(distPath, asset));
                console.log(chalk.cyan(`📁 Copied ${asset} to root`));
            }
        }

        // Note: assets folder is NOT copied to dist as it contains source files
        // Only the optimized icon.png is copied to root above
        console.log(chalk.gray(`� Assets folder excluded from distribution (source files only)`));
    }

    async generateManifest(distPath, manifest, packageJson) {
        const dynamicManifest = {
            name: manifest?.name || packageJson?.name || 'UnknownMod',
            version_number: manifest?.version_number || packageJson?.version || '1.0.0',
            website_url: manifest?.website_url || packageJson?.homepage || '',
            description: manifest?.description || packageJson?.description || '',
            dependencies: manifest?.dependencies || []
        };

        await fs.writeJSON(
            path.join(distPath, 'manifest.json'),
            dynamicManifest,
            { spaces: 2 }
        );

        console.log(chalk.cyan('📝 Generated dynamic manifest.json'));
    }

    async generateReadme(distPath, manifest, packageJson) {
        const modName = manifest?.name || packageJson?.name || 'UnknownMod';
        const version = manifest?.version_number || packageJson?.version || '1.0.0';
        const description = manifest?.description || packageJson?.description || '';

        const readmeContent = `# ${modName}

**Version:** ${version}

## Description
${description}

## Installation
1. Make sure you have BepInEx installed
2. Copy the DLL file to your BepInEx/plugins folder
3. Launch the game

## Generated on
${new Date().toISOString()}

---
*This README was automatically generated during the build process.*
`;

        await fs.writeFile(path.join(distPath, 'README.md'), readmeContent);
        console.log(chalk.cyan('📝 Generated dynamic README.md'));
    }
}

// CLI execution
const modName = process.argv[2];

if (!modName) {
    console.error(chalk.red('❌ Please specify a mod name'));
    console.log(chalk.yellow('Usage: npm run build-mod <mod-name>'));
    process.exit(1);
}

const builder = new ModBuilder();

try {
    await builder.buildMod(modName);
    console.log(chalk.green('🎉 Build completed successfully!'));
} catch (error) {
    console.error(chalk.red('❌ Build failed:'), error.message);
    process.exit(1);
}
