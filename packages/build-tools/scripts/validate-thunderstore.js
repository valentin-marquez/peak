#!/usr/bin/env node
import fs from 'fs-extra';
import path from 'path';
import chalk from 'chalk';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * Thunderstore package validator
 * Validates that a package meets all Thunderstore requirements
 */
class ThunderstoreValidator {
    constructor() {
        this.projectRoot = path.resolve(__dirname, '../../../');
        this.requiredFiles = ['icon.png', 'README.md', 'manifest.json'];
        this.requiredManifestFields = ['name', 'version_number', 'website_url', 'description', 'dependencies'];
    }

    async validatePackage(distPath) {
        console.log(chalk.blue(`🔍 Validating Thunderstore package: ${path.basename(distPath)}`));

        const errors = [];
        const warnings = [];

        // Check required files exist
        for (const file of this.requiredFiles) {
            const filePath = path.join(distPath, file);
            if (!await fs.pathExists(filePath)) {
                errors.push(`Missing required file: ${file}`);
            } else {
                console.log(chalk.green(`✅ Found required file: ${file}`));
            }
        }

        // Validate icon.png
        const iconPath = path.join(distPath, 'icon.png');
        if (await fs.pathExists(iconPath)) {
            try {
                const sharp = (await import('sharp')).default;
                const metadata = await sharp(iconPath).metadata();

                if (metadata.width !== 256 || metadata.height !== 256) {
                    errors.push(`Icon must be exactly 256x256 pixels (found: ${metadata.width}x${metadata.height})`);
                } else {
                    console.log(chalk.green(`✅ Icon dimensions correct: 256x256`));
                }

                if (metadata.format !== 'png') {
                    errors.push(`Icon must be PNG format (found: ${metadata.format})`);
                } else {
                    console.log(chalk.green(`✅ Icon format correct: PNG`));
                }
            } catch (error) {
                errors.push(`Failed to validate icon: ${error.message}`);
            }
        }

        // Validate manifest.json
        const manifestPath = path.join(distPath, 'manifest.json');
        if (await fs.pathExists(manifestPath)) {
            try {
                const manifest = await fs.readJSON(manifestPath);

                // Check required fields
                for (const field of this.requiredManifestFields) {
                    if (!manifest.hasOwnProperty(field)) {
                        errors.push(`Missing required manifest field: ${field}`);
                    } else {
                        console.log(chalk.green(`✅ Found manifest field: ${field}`));
                    }
                }

                // Validate specific fields
                if (manifest.name && !/^[a-zA-Z0-9_]+$/.test(manifest.name)) {
                    errors.push(`Invalid mod name format. Only letters, numbers, and underscores allowed: ${manifest.name}`);
                }

                if (manifest.description && manifest.description.length > 250) {
                    errors.push(`Description too long (${manifest.description.length} chars, max 250)`);
                }

                if (manifest.version_number && !/^\d+\.\d+\.\d+$/.test(manifest.version_number)) {
                    warnings.push(`Version should follow semantic versioning (Major.Minor.Patch): ${manifest.version_number}`);
                }

                if (manifest.dependencies && !Array.isArray(manifest.dependencies)) {
                    errors.push(`Dependencies must be an array`);
                }

            } catch (error) {
                errors.push(`Failed to parse manifest.json: ${error.message}`);
            }
        }

        // Check for unnecessary files
        const allFiles = await fs.readdir(distPath);
        const unnecessaryFiles = allFiles.filter(file =>
            file.startsWith('System.') && file.endsWith('.dll')
        );

        if (unnecessaryFiles.length > 0) {
            warnings.push(`Found ${unnecessaryFiles.length} unnecessary system DLLs that may bloat the package`);
        }

        // Report results
        console.log(chalk.blue('\n📋 Validation Results:'));

        if (errors.length === 0) {
            console.log(chalk.green('✅ Package is Thunderstore compatible!'));
        } else {
            console.log(chalk.red(`❌ Found ${errors.length} error(s):`));
            errors.forEach(error => console.log(chalk.red(`   • ${error}`)));
        }

        if (warnings.length > 0) {
            console.log(chalk.yellow(`⚠️  Found ${warnings.length} warning(s):`));
            warnings.forEach(warning => console.log(chalk.yellow(`   • ${warning}`)));
        }

        return { valid: errors.length === 0, errors, warnings };
    }
}

// CLI execution
const distPath = process.argv[2];

if (!distPath) {
    console.error(chalk.red('❌ Please specify a dist path'));
    console.log(chalk.yellow('Usage: npm run validate-thunderstore <dist-path>'));
    process.exit(1);
}

const validator = new ThunderstoreValidator();

try {
    const result = await validator.validatePackage(distPath);

    if (!result.valid) {
        process.exit(1);
    }

    console.log(chalk.green('\n🎉 Validation completed successfully!'));
} catch (error) {
    console.error(chalk.red('❌ Validation failed:'), error.message);
    process.exit(1);
}
