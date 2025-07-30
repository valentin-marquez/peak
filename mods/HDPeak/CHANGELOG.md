# Changelog

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