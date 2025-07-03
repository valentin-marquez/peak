#!/usr/bin/env node
import fs from 'fs-extra';
import path from 'path';
import chalk from 'chalk';
import sharp from 'sharp';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * Icon optimization tool for PEAK mods
 * Converts any image to a 256x256 PNG optimized icon
 */
class IconOptimizer {
    constructor() {
        this.projectRoot = path.resolve(__dirname, '../../../');
        this.targetSize = 256;
    }

    async optimizeIcon(inputPath, outputPath) {
        console.log(chalk.blue(`🖼️  Optimizing icon: ${inputPath}`));

        if (!await fs.pathExists(inputPath)) {
            throw new Error(`Input image not found: ${inputPath}`);
        }

        // Ensure output directory exists
        await fs.ensureDir(path.dirname(outputPath));

        try {
            await sharp(inputPath)
                .resize(this.targetSize, this.targetSize, {
                    fit: 'cover',
                    position: 'center'
                })
                .png({
                    quality: 90,
                    compressionLevel: 6,
                    palette: true
                })
                .toFile(outputPath);

            console.log(chalk.green(`✅ Icon optimized successfully: ${outputPath}`));

            // Log file size info
            const inputStats = await fs.stat(inputPath);
            const outputStats = await fs.stat(outputPath);

            console.log(chalk.cyan(`📊 Size: ${inputStats.size} bytes → ${outputStats.size} bytes`));
            console.log(chalk.cyan(`📏 Dimensions: ${this.targetSize}x${this.targetSize} PNG`));

        } catch (error) {
            throw new Error(`Failed to optimize icon: ${error.message}`);
        }
    }

    async optimizeModIcon(modName) {
        const modPath = path.join(this.projectRoot, 'mods', modName);
        const assetsPath = path.join(modPath, 'assets');

        if (!await fs.pathExists(assetsPath)) {
            throw new Error(`Assets directory not found: ${assetsPath}`);
        }

        // Look for existing icon files
        const iconFiles = await fs.readdir(assetsPath);
        const imageExtensions = ['.png', '.jpg', '.jpeg', '.gif', '.bmp', '.webp'];

        const possibleIcons = iconFiles.filter(file => {
            const ext = path.extname(file).toLowerCase();
            return imageExtensions.includes(ext) &&
                (file.toLowerCase().includes('icon') || file.toLowerCase().includes('logo'));
        });

        if (possibleIcons.length === 0) {
            throw new Error(`No icon files found in ${assetsPath}. Looking for files containing 'icon' or 'logo'.`);
        }

        const inputIcon = path.join(assetsPath, possibleIcons[0]);
        const outputIcon = path.join(assetsPath, 'icon.png');

        await this.optimizeIcon(inputIcon, outputIcon);

        // If we used a different file as input, optionally remove it
        if (possibleIcons[0] !== 'icon.png') {
            console.log(chalk.yellow(`💡 Original icon kept as backup: ${possibleIcons[0]}`));
        }

        return outputIcon;
    }

    async optimizeAllModIcons() {
        const modsPath = path.join(this.projectRoot, 'mods');
        const modDirs = await fs.readdir(modsPath);

        for (const modDir of modDirs) {
            const modPath = path.join(modsPath, modDir);
            const stats = await fs.stat(modPath);

            if (stats.isDirectory()) {
                try {
                    await this.optimizeModIcon(modDir);
                } catch (error) {
                    console.warn(chalk.yellow(`⚠️  Skipped ${modDir}: ${error.message}`));
                }
            }
        }
    }
}

// CLI execution
const optimizer = new IconOptimizer();
const args = process.argv.slice(2);

if (args.length === 0) {
    console.log(chalk.blue('🖼️  Optimizing icons for all mods...'));
    try {
        await optimizer.optimizeAllModIcons();
        console.log(chalk.green('🎉 All icons optimized successfully!'));
    } catch (error) {
        console.error(chalk.red('❌ Optimization failed:'), error.message);
        process.exit(1);
    }
} else if (args.length === 1) {
    // Single mod
    const modName = args[0];
    console.log(chalk.blue(`🖼️  Optimizing icon for mod: ${modName}`));
    try {
        await optimizer.optimizeModIcon(modName);
        console.log(chalk.green('🎉 Icon optimized successfully!'));
    } catch (error) {
        console.error(chalk.red('❌ Optimization failed:'), error.message);
        process.exit(1);
    }
} else if (args.length === 2) {
    // Custom input/output
    const [inputPath, outputPath] = args;
    try {
        await optimizer.optimizeIcon(inputPath, outputPath);
        console.log(chalk.green('🎉 Icon optimized successfully!'));
    } catch (error) {
        console.error(chalk.red('❌ Optimization failed:'), error.message);
        process.exit(1);
    }
} else {
    console.error(chalk.red('❌ Invalid arguments'));
    console.log(chalk.yellow('Usage:'));
    console.log(chalk.yellow('  npm run optimize-icon                    # Optimize all mod icons'));
    console.log(chalk.yellow('  npm run optimize-icon <mod-name>        # Optimize specific mod icon'));
    console.log(chalk.yellow('  npm run optimize-icon <input> <output>  # Custom input/output'));
    process.exit(1);
}
