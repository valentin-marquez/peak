#!/usr/bin/env node
import fs from 'fs-extra';
import path from 'path';
import chalk from 'chalk';
import archiver from 'archiver';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * Enhanced mod packaging tool for PEAK mods
 * Creates optimized distribution-ready packages in dist directory
 */
class ModPackager {
    constructor() {
        this.projectRoot = path.resolve(__dirname, '../../../');
        this.distDir = path.join(this.projectRoot, 'dist');
    }

    async packageMod(modName, format = 'zip') {
        console.log(chalk.blue(`📦 Packaging mod: ${modName}`));

        const modDistPath = path.join(this.distDir, modName);

        if (!await fs.pathExists(modDistPath)) {
            throw new Error(`Mod build not found: ${modDistPath}. Run build first.`);
        }

        // Read manifest for version info
        const manifestPath = path.join(modDistPath, 'manifest.json');
        let version = '1.0.0';

        if (await fs.pathExists(manifestPath)) {
            const manifest = await fs.readJSON(manifestPath);
            version = manifest.version_number || version;
        }

        const packageName = `${modName}_v${version}.${format}`;
        const packagePath = path.join(this.distDir, packageName);

        // Remove existing package
        await fs.remove(packagePath);

        if (format === 'zip') {
            await this.createOptimizedZipPackage(modDistPath, packagePath);
        } else {
            throw new Error(`Unsupported package format: ${format}`);
        }

        console.log(chalk.green(`✅ Package created: ${packagePath}`));

        // Log detailed package info
        const packageStats = await fs.stat(packagePath);
        const originalSize = await this.calculateDirectorySize(modDistPath);
        const compressionRatio = ((1 - packageStats.size / originalSize) * 100).toFixed(1);

        console.log(chalk.cyan(`📊 Original size: ${(originalSize / 1024 / 1024).toFixed(2)} MB`));
        console.log(chalk.cyan(`📦 Package size: ${(packageStats.size / 1024 / 1024).toFixed(2)} MB`));
        console.log(chalk.cyan(`🗜️  Compression: ${compressionRatio}%`));

        return packagePath;
    }

    async createOptimizedZipPackage(sourcePath, outputPath) {
        return new Promise((resolve, reject) => {
            const output = fs.createWriteStream(outputPath);
            const archive = archiver('zip', {
                zlib: {
                    level: 9,           // Maximum compression
                    chunkSize: 1024 * 32, // 32KB chunks for better performance
                    windowBits: 15,     // Maximum window size
                    memLevel: 8         // Maximum memory usage
                },
                store: false // Never store uncompressed (always compress)
            });

            // Enhanced error handling
            output.on('close', () => {
                console.log(chalk.gray(`📁 Archive finalized with ${archive.pointer()} bytes`));
                resolve();
            });

            output.on('error', (err) => {
                console.error(chalk.red(`📦 Output stream error: ${err.message}`));
                reject(err);
            });

            archive.on('error', (err) => {
                console.error(chalk.red(`📦 Archive error: ${err.message}`));
                reject(err);
            });

            archive.on('warning', (err) => {
                if (err.code === 'ENOENT') {
                    console.warn(chalk.yellow(`⚠️  Archive warning: ${err.message}`));
                } else {
                    reject(err);
                }
            });

            // Progress reporting
            archive.on('progress', (progress) => {
                const percent = ((progress.fs.processedBytes / progress.fs.totalBytes) * 100).toFixed(1);
                if (progress.fs.processedBytes === progress.fs.totalBytes) {
                    console.log(chalk.gray(`📈 Compression complete: ${percent}%`));
                }
            });

            archive.pipe(output);

            // Add files with optimization
            archive.directory(sourcePath, false, (data) => {
                // Skip temporary files and system files
                if (data.name.startsWith('.') || data.name.includes('Thumbs.db') || data.name.includes('.DS_Store')) {
                    return false;
                }

                // Optimize compression based on file type
                if (data.name.endsWith('.dll') || data.name.endsWith('.exe')) {
                    data.store = false; // Always compress executables
                } else if (data.name.endsWith('.png') || data.name.endsWith('.jpg') || data.name.endsWith('.jpeg')) {
                    data.store = true; // Don't recompress images
                }

                return data;
            });

            archive.finalize();
        });
    }

    async calculateDirectorySize(dirPath) {
        let totalSize = 0;

        const calculateSize = async (currentPath) => {
            const stats = await fs.stat(currentPath);

            if (stats.isDirectory()) {
                const items = await fs.readdir(currentPath);
                for (const item of items) {
                    await calculateSize(path.join(currentPath, item));
                }
            } else {
                totalSize += stats.size;
            }
        };

        await calculateSize(dirPath);
        return totalSize;
    }

    async packageAllMods() {
        const distItems = await fs.readdir(this.distDir);
        const packages = [];

        // Filter only directories (mod folders), exclude ZIP files
        const modDirs = [];
        for (const item of distItems) {
            const itemPath = path.join(this.distDir, item);
            const stats = await fs.stat(itemPath);

            if (stats.isDirectory()) {
                modDirs.push(item);
            }
        }

        console.log(chalk.blue(`📦 Found ${modDirs.length} mod(s) to package`));

        for (const modName of modDirs) {
            try {
                const packagePath = await this.packageMod(modName);
                packages.push(packagePath);
            } catch (error) {
                console.warn(chalk.yellow(`⚠️  Skipped ${modName}: ${error.message}`));
            }
        }

        return packages;
    }

    async createRelease(modName) {
        console.log(chalk.blue(`🚀 Creating release for: ${modName}`));

        // Build -> Package workflow
        const { ModBuilder } = await import('./build-mod.js');
        const builder = new ModBuilder();

        await builder.buildMod(modName);
        const packagePath = await this.packageMod(modName);

        console.log(chalk.green(`🎉 Release ready: ${packagePath}`));
        return packagePath;
    }
}

// CLI execution
const packager = new ModPackager();
const args = process.argv.slice(2);
const command = args[0];
const modName = args[1];

if (!command) {
    console.error(chalk.red('❌ Please specify a command'));
    console.log(chalk.yellow('Commands:'));
    console.log(chalk.yellow('  package <mod-name>     # Package a built mod'));
    console.log(chalk.yellow('  package-all            # Package all built mods'));
    console.log(chalk.yellow('  release <mod-name>     # Full build + package workflow'));
    process.exit(1);
}

try {
    switch (command) {
        case 'package':
            if (!modName) {
                console.error(chalk.red('❌ Please specify a mod name'));
                process.exit(1);
            }
            await packager.packageMod(modName);
            break;

        case 'package-all':
            const packages = await packager.packageAllMods();
            console.log(chalk.green(`🎉 Packaged ${packages.length} mods successfully!`));
            break;

        case 'release':
            if (!modName) {
                console.error(chalk.red('❌ Please specify a mod name'));
                process.exit(1);
            }
            await packager.createRelease(modName);
            break;

        default:
            console.error(chalk.red(`❌ Unknown command: ${command}`));
            process.exit(1);
    }

    console.log(chalk.green('🎉 Packaging completed successfully!'));
} catch (error) {
    console.error(chalk.red('❌ Packaging failed:'), error.message);
    process.exit(1);
}
