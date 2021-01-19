# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.4] - 2021-01-19
### Added
- Dependency for the Unity 2D Sprite in package.json
- Support for OpenUPM

## [1.0.3] - 2020-12-28
### Added
- Support for different import implementations
- Bundled Importer to keep the imported sprites and animations in one file
- Support for the sprite-editor with the Bundled Importer

### Changed
- The regular import was labelled "Generated" because it generates the content and creates new assets in a subfolder 

## [1.0.2] - 2020-12-28
### Added
- Creates a sprite sheet file (not dynamic)
- Editable meta data like custom physics shapes with the 'Sprite Editor'
- Generates tilemap with extended-padding (solves lines tearing issue between tiles)
- Makes animation files (not dynamic)
- Animation controller creation
- New tilemap name rule support (row-col)

### Changed
- Creates multiple assets instead of one single bundle

## [1.0.1] - 2020-02-04
### Added
- Transparent color option
- Tilemap empty behaviour

### Changed
- Support for Unity package manager was fixed

## [1.0.0] - 2018-11-05
### Added
- Different Import Modes (Sprite, Tilemap)
- Auto-Sprite creation