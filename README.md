# MapGenerator

The Procedural 3D Map Generator is a versatile tool for real-time procedural map generation, mainly created to meet the need for consecutively generating large 3D maps. It provides features including terrain generation, civilization generation (such as towns, roads, and bridges), and decorations. The Map Generator does not work with fixed models or generate terrain with a fixed height range. Users can utilize any prefabs, models, or terrain properties as they wish. Example visuals are created with the default dataset included in the repository.

Seed: 15151515<br>
![S_15151515](https://github.com/keremaydogan/MapGenerator/assets/62666688/9bd61aac-48bb-486c-8f3f-9b9b0871b379)
<br>

Seed: 12345678<br>
![S_12345678](https://github.com/keremaydogan/MapGenerator/assets/62666688/45a12253-80c5-408d-ab35-7d30c0a10717)
<br>

Seed: -140133298<br>
![S_-140133298](https://github.com/keremaydogan/MapGenerator/assets/62666688/cbb7f2d3-d82a-42d0-88f2-760faa1802ab)
<br>

Seed: -387517110<br>
![S_-387517110](https://github.com/keremaydogan/MapGenerator/assets/62666688/de993802-a161-4661-b795-08e11bc80401)
<br>

Seed: -2114975292<br>
![S_-2114975292](https://github.com/keremaydogan/MapGenerator/assets/62666688/c5b487a3-b335-4baf-b04b-7fedeaed87d8)
<br>

Seed: 1123581321<br>
![S_1123581321](https://github.com/keremaydogan/MapGenerator/assets/62666688/32a2b262-881f-4443-a455-8dd5d4d62d61)
<br>

Seed: 1123581321 (3000x3000)<br>
![S_1123581321_3000x3000](https://github.com/keremaydogan/MapGenerator/assets/62666688/44c14d44-cf72-495b-acb6-b4f447fd9bd2)
<br>

Seed: -1900468417 (upper view)<br>
![S_-1900468417](https://github.com/keremaydogan/MapGenerator/assets/62666688/3a00fc24-f428-441e-9887-4800a84fae36)
<br>

Seed: -265657411 (Height Offset: 0)<br>
![S_-265657411](https://github.com/keremaydogan/MapGenerator/assets/62666688/d8abe285-d848-4cd3-8534-9a849ddd1c5a)
<br>

Seed: -26565741 (Height Offset: 75)<br>
![S_-265657411_HO75](https://github.com/keremaydogan/MapGenerator/assets/62666688/b9c5e539-cb5c-4f89-a21f-b9e133321f97)
<br>

Seed: 223439984<br>
![S_223439984](https://github.com/keremaydogan/MapGenerator/assets/62666688/39456929-ff16-4f36-b1ca-52a9a68e3a8a)
<br>

Seed: 223439984_(close view 1)<br>
![S_223439984_V1](https://github.com/keremaydogan/MapGenerator/assets/62666688/50f845d0-4989-4a52-8064-af3d436f275a)
<br>

Seed: 223439984_(close view 2)<br>
![S_223439984_V2](https://github.com/keremaydogan/MapGenerator/assets/62666688/c46aa629-f83c-4de8-8770-53dfd7916e9d)
<br>


## How it works?

Map Generator Component<br>
![MapGeneratorComponent](https://github.com/keremaydogan/MapGenerator/assets/62666688/42cb2dd4-5006-46cb-b76e-7563d69f3120)
<br>

Map Generator has 3 main module: Terrain Generator, Path Generator, and Map Decorator.

### Terrain Generator
Terrain Generator Data<br>
![TerraGenData](https://github.com/keremaydogan/MapGenerator/assets/62666688/0d6a477b-f416-4231-9483-022905506068)
<br>
Terrain generation works by applying multiple Perlin noise levels with different cell sizes and amplitude coefficients.

### Path Generator
Path Generator Data<br>
![PathGenData](https://github.com/keremaydogan/MapGenerator/assets/62666688/aa4dd8ca-3a87-4f5b-9503-717abe205d99)
<br>
Bridge Generator Data<br>
![BridgeGenData](https://github.com/keremaydogan/MapGenerator/assets/62666688/5e69531d-ba1e-4e95-b5f9-82dd46c8af49)
<br>
Road Generator Data<br>
![RoadGenData](https://github.com/keremaydogan/MapGenerator/assets/62666688/8d97254d-1556-4e4a-8eca-57ecfc3d09b0)
<br>
The Path Generator controls the building of constructions, determining how to connect settlements, alter terrain to create walkable roads, and where to place bridges, among other functions. Generating roads as separate game objects will become obsolete after the texture generation update.

### Map Decorator
Map Decorator Data<br>
![MapDecoratorData](https://github.com/keremaydogan/MapGenerator/assets/62666688/38373993-bffe-47e4-aab4-0dcf341806ca)
<br>

The Map Decorator controls the placement and quantity of decorations, such as trees, rocks, statues, and water towers.

## License
Shield: [![CC BY-ND 4.0][cc-by-nd-shield]][cc-by-nd]

This work is licensed under a
[Creative Commons Attribution-NoDerivs 4.0 International License][cc-by-nd].

[![CC BY-ND 4.0][cc-by-nd-image]][cc-by-nd]

[cc-by-nd]: https://creativecommons.org/licenses/by-nd/4.0/

[cc-by-nd-image]: https://licensebuttons.net/l/by-nd/4.0/88x31.png
[cc-by-nd-shield]: https://img.shields.io/badge/License-CC%20BY--ND%204.0-lightgrey.svg
