# Aseprite-Importer for Unity
![AsepriteImporter Thumbnail](GitHub/AsepriteImporterUnity.png)

This package helps you importing [.ase files](https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md) from [aseprite](https://www.aseprite.org/). The reader is fully written in C# which reads the compressed binary file and creates spritesheets, sprites and animations out of it.

## Difference with origin Aseprite-Importer
The 'unity-aseprite-importer' is a good plugin but it only generate not-editable sprites and animations.
This one genetate editable resources.
* It makes a sprite sheet file (not dynamic)
* You can edit the meta like custom physics shapes with the 'Sprite Editor'
* It generates tilemap with extended-padding, it solves lines tearing issue between tiles.
* It makes animation files (not dynamic)
* You can add a custom event to each animation
* It makes an animation controller
* New tilemap name rule support (row-col) which makes you find a certain tile fast.
* Some features are removed (Transparent color, Tilemap empty behavior) 

![image](https://user-images.githubusercontent.com/22926212/100529665-2cb66480-322d-11eb-82fa-5729572a75d9.png)
![image](https://user-images.githubusercontent.com/22926212/100529680-57a0b880-322d-11eb-8e8a-e0b48ff0495b.png)
![image](https://user-images.githubusercontent.com/22926212/100529693-7e5eef00-322d-11eb-8d46-5c7e03e958ce.png)


## Usage
Import the ```unity-aseprite-importer``` folder into your project's Packages folder. Now it should import all newly added .ase-files automatically. Already imported assets do need a manual re-import.

![Demo GIF of AsepriteImporter](GitHub/aseprite-importer-demo.gif)


## License

See LICENSE file.

Note: As of Unity doesn't include any editor scripts in the game, you don't have to license your game under GPL.
