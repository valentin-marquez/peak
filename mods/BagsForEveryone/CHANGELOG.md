# Changelog

## [1.1.0] - 2025-07-30

### Changed

- Updated game level validation logic: now accepts any scene whose name starts with `Level_`, instead of restricting to `Level_0` through `Level_13`.
- Simplified `IsInValidGameLevel()` method for improved maintainability and flexibility.

### Technical changes

- Removed numeric parsing and range checks from the level validation code.
- The method now returns `true` for all scenes with names beginning with `Level_`.
