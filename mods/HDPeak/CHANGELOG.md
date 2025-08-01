# Changelog

## [1.2.1] - 2025-08-31

### Added

- Added a setting for far plane culling distance, letting users control how far objects are rendered ([#4](https://github.com/valentin-marquez/peak/pull/4), by [@someramsey](https://github.com/someramsey)).

### Technical changes

- Implemented the culling distance as a `FloatSetting` due to UI issues with `IntSetting`.
- Added a Harmony instance field to the plugin class.
- Created and registered a custom setting class for culling distance.
- Patched the `MainCamera` class with Harmony to update the value in real time.

### Notes

- The setting displays as a float, but only integer values affect the game.
- Thanks to [@someramsey](https://github.com/someramsey) for troubleshooting and implementing this feature.

## [1.2.0] - 2025-07-30

### Changed

- Migrated all mod settings to use the SettingsExtender library for improved compatibility and stability.
- Replaced the custom `SettingsRegistry` implementation with the SettingsExtender API.
- Refactored settings classes (`AnisotropicFilteringSetting`, `TextureQualitySetting`, `ShadowResolutionSetting`, `OpaqueTextureSetting`, `MaxLightsSetting`, `DynamicBatchingSetting`, and `LODBiasSetting`) to use the new API.
- Cleaned up redundant boilerplate code related to settings management.

### Fixed

- Resolved issues where menu names and option labels were not displaying correctly in the settings UI.
- Updated the default game path in `Directory.Build.props` to match the standard Steam installation directory for easier contributor setup.

### Notes

- Contributors should verify the game path in their local environment or consider implementing an automatic path detection mechanism for future improvements.
- Special thanks to [@someramsey](https://github.com/someramsey) for contributing these changes in [pull request #3](https://github.com/valentin-marquez/peak/pull/3).

## [1.1.0] - 2025-07-11

### Added

- Full multi-language support for all mod texts, using the native PEAK game localization system.
- Embedded CSV localization file with translations for 13 languages.
- Automatic integration with the language selected by the user in the game.
- Documentation for adding new translations and contributing.

### Improved

- All setting names and options are now translated dynamically.
- The settings tab name is also translatable.

### Technical changes

- The localization CSV file is embedded as a resource in the DLL.
- The mod uses reflection to integrate with the game's localization system.
- Translations are applied in real time, no need to restart the game.

---

## [1.0.0] - 2025-07-10

- Initial release of HDPeak mod with advanced graphics settings for PEAK.
