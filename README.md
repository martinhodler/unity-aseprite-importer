# Aseprite-Importer for Unity
![AsepriteImporter Thumbnail](GitHub/AsepriteImporterUnity.png)


[![openupm](https://img.shields.io/npm/v/io.tinu.asepriteimporter?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.tinu.asepriteimporter/)

This package helps you importing [.ase files](https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md) from [aseprite](https://www.aseprite.org/). The reader is fully written in C# which reads the compressed binary file and creates spritesheets, sprites and animations out of it.

## Features
* Creates a sprite sheet file (not dynamic)
* Editable meta data like custom physics shapes with the 'Sprite Editor'
* Generates tilemap with extended-padding (solves lines tearing issue between tiles)
* Makes animation files (not dynamic)
* Creates an animation controller
* New tilemap name rule support (row-col)

![Demo GIF of AsepriteImporter](GitHub/aseprite-importer-demo.gif)

![image](https://user-images.githubusercontent.com/22926212/100529665-2cb66480-322d-11eb-82fa-5729572a75d9.png)
![image](https://user-images.githubusercontent.com/22926212/100529680-57a0b880-322d-11eb-8e8a-e0b48ff0495b.png)
![image](https://user-images.githubusercontent.com/22926212/100529693-7e5eef00-322d-11eb-8d46-5c7e03e958ce.png)


## Install
### OpenUPM
#### Unity Package
OpenUPM lets you install packages more easily by providing a downloadable unity package which will setup the package automatically.
You can find the download on the following link:

[![openupm](https://img.shields.io/npm/v/io.tinu.asepriteimporter?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.tinu.asepriteimporter/) https://openupm.com/packages/io.tinu.asepriteimporter/


#### CLI
##### Prerequisites
- [Node.js 12](https://nodejs.org/en/)
- [openupm-cli](https://github.com/openupm/openupm-cli#openupm-cli).

See: https://openupm.com/docs/getting-started.html#installing-openupm-cli

##### OpenUPM CLI Install

```sh
# Go to your Unity project directory
cd YOUR_UNITY_PROJECT_DIR

# Install package: io.tinu.asepriteimporter
openupm add io.tinu.asepriteimporter
```
### Unity
* In Unity open the Package Manager (`Window > Package Manager`).
* In the Package Manager click on the Plus-Icon in the top-left and select `Add package from git URL...`
* Enter the URL of this Repository (`https://github.com/martinhodler/unity-aseprite-importer.git`) and press <kbd>Enter</kbd>

## License

See LICENSE file.

Note: As of Unity doesn't include any editor scripts in the game, you don't have to license your game under GPL.
