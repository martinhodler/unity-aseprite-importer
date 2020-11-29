# Aseprite-Importer for Unity (FORK!)
![AsepriteImporter Thumbnail](GitHub/AsepriteImporterUnity.png)

This package helps you importing [.ase files](https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md) from [aseprite](https://www.aseprite.org/). The reader is fully written in C# which reads the compressed binary file and creates spritesheets, sprites and animations out of it.

https://github.com/mjshin82/unity-aseprite-importer

## Difference with origin Aseprite-Importer
* It makes a sprite sheet file (not dynamic)
* You can edit the meta like custom physics shapes with the 'Sprite Editor'
* It generates tilemap with extended-padding, it solves lines tearing issue between tiles.
* It makes animation files (not dynamic)
* You can add a custom event to each animation
* It makes an animation controller
* New tilemap name rule support (row-col) which makes you find a certain tile fast.
* Some features are removed (Transparent color, Tilemap empty behavior) 


## Usage
Import the ```unity-aseprite-importer``` folder into your project's Packages folder. Now it should import all newly added .ase-files automatically. Already imported assets do need a manual re-import.

![Demo GIF of AsepriteImporter](GitHub/aseprite-importer-demo.gif)


## License

See LICENSE file.

Note: As of Unity doesn't include any editor scripts in the game, you don't have to license your game under GPL.
