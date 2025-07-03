#!/usr/bin/env node
import { program } from 'commander';
import chalk from 'chalk';
import { execSync } from 'child_process';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ASCII Art Banner
const banner = `
██████╗ ███████╗ █████╗ ██╗  ██╗    ██████╗ ██╗   ██╗██╗██╗     ██████╗ 
██╔══██╗██╔════╝██╔══██╗██║ ██╔╝    ██╔══██╗██║   ██║██║██║     ██╔══██╗
██████╔╝█████╗  ███████║█████╔╝     ██████╔╝██║   ██║██║██║     ██║  ██║
██╔═══╝ ██╔══╝  ██╔══██║██╔═██╗     ██╔══██╗██║   ██║██║██║     ██║  ██║
██║     ███████╗██║  ██║██║  ██╗    ██████╔╝╚██████╔╝██║███████╗██████╔╝
╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝    ╚═════╝  ╚═════╝ ╚═╝╚══════╝╚═════╝ 
                                                                          
`;

console.log(chalk.cyan(banner));
console.log(chalk.gray('Peak Mod Build Tools - v1.0.0\n'));

program
    .name('@peak/build')
    .description('Build tools for PEAK BepInEx mods')
    .version('1.0.0');

// Build command
program
    .command('build <modName>')
    .description('Build a PEAK mod for distribution')
    .option('-v, --validate', 'Validate against Thunderstore requirements after build')
    .action(async (modName, options) => {
        try {
            console.log(chalk.blue(`🚀 Building mod: ${modName}`));

            // Run build script
            execSync(`node ${path.join(__dirname, 'scripts/build-mod.js')} ${modName}`, {
                stdio: 'inherit',
                cwd: __dirname
            });

            if (options.validate) {
                console.log(chalk.blue(`🔍 Validating against Thunderstore requirements...`));
                execSync(`node ${path.join(__dirname, 'scripts/validate-thunderstore.js')} dist/${modName}`, {
                    stdio: 'inherit',
                    cwd: path.resolve(__dirname, '../..')
                });
            }

            console.log(chalk.green(`✅ Build completed successfully!`));
        } catch (error) {
            console.error(chalk.red(`❌ Build failed: ${error.message}`));
            process.exit(1);
        }
    });

// Package command
program
    .command('package <modName>')
    .description('Package a built mod into a Thunderstore-compatible ZIP')
    .action(async (modName) => {
        try {
            console.log(chalk.blue(`📦 Packaging mod: ${modName}`));

            execSync(`node ${path.join(__dirname, 'scripts/package-mod.js')} package ${modName}`, {
                stdio: 'inherit',
                cwd: __dirname
            });

            console.log(chalk.green(`✅ Package completed successfully!`));
        } catch (error) {
            console.error(chalk.red(`❌ Packaging failed: ${error.message}`));
            process.exit(1);
        }
    });

// Release command (build + validate + package)
program
    .command('release <modName>')
    .description('Complete release workflow: build, validate and package')
    .action(async (modName) => {
        try {
            console.log(chalk.blue(`🚀 Starting release workflow for: ${modName}`));

            // Build
            console.log(chalk.cyan(`\n🔨 Step 1: Building...`));
            execSync(`node ${path.join(__dirname, 'scripts/build-mod.js')} ${modName}`, {
                stdio: 'inherit',
                cwd: __dirname
            });

            // Validate
            console.log(chalk.cyan(`\n🔍 Step 2: Validating...`));
            execSync(`node ${path.join(__dirname, 'scripts/validate-thunderstore.js')} dist/${modName}`, {
                stdio: 'inherit',
                cwd: path.resolve(__dirname, '../..')
            });

            // Package
            console.log(chalk.cyan(`\n📦 Step 3: Packaging...`));
            execSync(`node ${path.join(__dirname, 'scripts/package-mod.js')} package ${modName}`, {
                stdio: 'inherit',
                cwd: __dirname
            });

            console.log(chalk.green(`\n🎉 Release workflow completed successfully!`));
            console.log(chalk.gray(`Package ready for Thunderstore upload.`));
        } catch (error) {
            console.error(chalk.red(`❌ Release failed: ${error.message}`));
            process.exit(1);
        }
    });

// Validate command
program
    .command('validate <modName>')
    .description('Validate a built mod against Thunderstore requirements')
    .action(async (modName) => {
        try {
            console.log(chalk.blue(`🔍 Validating mod: ${modName}`));

            execSync(`node ${path.join(__dirname, 'scripts/validate-thunderstore.js')} dist/${modName}`, {
                stdio: 'inherit',
                cwd: path.resolve(__dirname, '../..')
            });

            console.log(chalk.green(`✅ Validation completed successfully!`));
        } catch (error) {
            console.error(chalk.red(`❌ Validation failed: ${error.message}`));
            process.exit(1);
        }
    });

// Optimize icon command
program
    .command('optimize-icon <modName>')
    .description('Optimize mod icon for Thunderstore (256x256)')
    .action(async (modName) => {
        try {
            console.log(chalk.blue(`🖼️ Optimizing icon for: ${modName}`));

            execSync(`node ${path.join(__dirname, 'scripts/optimize-icon.js')} ${modName}`, {
                stdio: 'inherit',
                cwd: __dirname
            });

            console.log(chalk.green(`✅ Icon optimization completed successfully!`));
        } catch (error) {
            console.error(chalk.red(`❌ Icon optimization failed: ${error.message}`));
            process.exit(1);
        }
    });

// List available mods
program
    .command('list')
    .description('List all available mods in the workspace')
    .action(async () => {
        try {
            const fs = (await import('fs-extra')).default;
            const modsDir = path.resolve(__dirname, '../../mods');
            const mods = await fs.readdir(modsDir);

            console.log(chalk.blue('📁 Available mods:'));
            for (const mod of mods) {
                const modPath = path.join(modsDir, mod);
                const stats = await fs.stat(modPath);
                if (stats.isDirectory()) {
                    console.log(chalk.green(`  • ${mod}`));
                }
            }
        } catch (error) {
            console.error(chalk.red(`❌ Failed to list mods: ${error.message}`));
            process.exit(1);
        }
    });

// Clean dist directory
program
    .command('clean')
    .description('Clean dist directory (removes build artifacts and ZIP files)')
    .option('-z, --zip-only', 'Only remove ZIP files, keep built mod folders')
    .action(async (options) => {
        try {
            const fs = (await import('fs-extra')).default;
            const distDir = path.resolve(__dirname, '../../dist');

            if (!await fs.pathExists(distDir)) {
                console.log(chalk.yellow('📁 Dist directory does not exist, nothing to clean'));
                return;
            }

            const items = await fs.readdir(distDir);
            let removedCount = 0;

            console.log(chalk.blue('🧹 Cleaning dist directory...'));

            for (const item of items) {
                const itemPath = path.join(distDir, item);
                const stats = await fs.stat(itemPath);

                if (options.zipOnly) {
                    // Only remove ZIP files
                    if (stats.isFile() && item.endsWith('.zip')) {
                        await fs.remove(itemPath);
                        console.log(chalk.red(`🗑️  Removed: ${item}`));
                        removedCount++;
                    }
                } else {
                    // Remove everything
                    await fs.remove(itemPath);
                    console.log(chalk.red(`🗑️  Removed: ${item}`));
                    removedCount++;
                }
            }

            if (removedCount === 0) {
                console.log(chalk.yellow('📁 Nothing to clean'));
            } else {
                console.log(chalk.green(`✅ Cleaned ${removedCount} item(s) from dist directory`));
            }
        } catch (error) {
            console.error(chalk.red(`❌ Clean failed: ${error.message}`));
            process.exit(1);
        }
    });

// Initialize new mod project
program
    .command('init <modName>')
    .description('Initialize a new mod project with interactive setup')
    .option('-d, --description <description>', 'Mod description')
    .option('-a, --author <author>', 'Author name')
    .option('--skip-features', 'Skip feature selection (minimal setup)')
    .action(async (modName, options) => {
        try {
            console.log(chalk.blue(`🚀 Initializing new mod: ${modName}`));

            // Import and run the init script
            const { initializeMod } = await import('./scripts/init-mod.js');
            await initializeMod(modName, options);

            console.log(chalk.green(`✅ Mod initialization completed successfully!`));
        } catch (error) {
            console.error(chalk.red(`❌ Initialization failed: ${error.message}`));
            process.exit(1);
        }
    });

program.parse();
