---
tags: [jeu-colonie, fonctionnalite, monde, terrain]
---

# Terrain procédural

## ChunkManager (singleton, scène Monde)
- Grille de chunks de `chunkSize` (64 m). Les chunks situés à moins de `generationDistance` (5 dans `0_World`) de celui du joueur sont chargés, soit une grille de 11×11. Ce calcul n'est refait que quand le joueur change de chunk.
- Les chunks éloignés sont **désactivés**, et réactivés au retour. Au-delà de `generationDistance + destroyMargin` (2 chunks de plus), ils sont **détruits** avec leur `TerrainData` pour libérer la mémoire. Au retour, ils sont régénérés à l'identique : relief tiré de la seed, aplanissements rejoués, et objets restaurés par le `SaveManager`, qui garde leur état (`UnloadObjects`).
- Les chunks chargés sont reliés à leurs voisins (`Terrain.SetNeighbors`, vérifié à chaque frame dans `LateUpdate`, auto-connexion Unity désactivée), ce qui évite les fissures de LOD vues de loin.
- Seed → offset Perlin global, ce qui donne un terrain continu et déterministe. La seed de la scène ne sert qu'aux nouvelles parties : une partie sauvegardée garde sa seed.
- `ToReplace` : le joueur et les transforms configurés sont replacés sur la surface quand leur chunk a fini de se générer. Le joueur est fourni par `PlayerManager` (`ev_spawnPlayer`), et la référence est mise à jour à chaque respawn.
- `AddTerrainEdit` : aplanissement sous une construction (voir [[Terrain sous les constructions]]), appliqué aux chunks chargés, enregistré et rejoué à la génération des autres.
- Gizmos de debug : chunks générés et chargés.

## TerrainGenerator (sur le prefab `Chunk`)
- Duplique le `TerrainData`. Le terrain utilise le shader `Terrain/World UV Standard` : les textures sont positionnées depuis la position monde, donc continues entre les chunks.
- Le `TerrainData` dupliqué est `Assets/Terrain/0_TestTerrain.asset` (prefab `Chunk`). Il n'a pour l'instant **qu'un calque**, `Ground_Rust` (rouille, le sol des gisements), peint à 100 %. Trois autres sols sont prêts, pour des zones placées selon le relief (refonte du terrain à venir) : `Ground_Ochre` (ocre-sable), `Ground_Dust` (rouille sombre, poussière de basalte), `Ground_Sediment` (sédiments pâles). Textures raccordables de 16 m, sols presque uniformes à nuances douces (réglages `PATCH` et `NUANCE`), couleur et normal map dans `Assets/Terrain/Textures/`, générées par `ArtPipeline/Blender/ground_build.py` (`GROUNDS`).
- **Zones** (`GenerateZones`, en asynchrone avec le relief ; inactif tant que le `TerrainData` n'a qu'un calque, l'utilisateur a jugé la répartition au hasard peu naturelle) : chaque calque a son propre bruit de Perlin en coordonnées monde (avec la seed), le plus fort l'emporte avec des transitions douces (`zoneSharpness` 8). Zones de 150 à 300 m (`zoneSize` 400), fréquence réglée par `zoneWeights` (1,25 / 1 / 0,92 / 0,85 : environ 50 / 21 / 20 / 9 % du sol). Carte de 64 × 64 texels par chunk (`zoneResolution`, 1 m), dont les bords coïncident avec ceux des voisins. Rien n'est sauvegardé : les zones sont recalculées à l'identique.
- **Anti-répétition** (`TerrainWorldUVSplatmapCommon.cginc`, `HexSetup` / `HexDiffuse` / `HexNormal`) : chaque calque est lu en *hex tiling*. Le plan est découpé en cellules hexagonales (`HEX_CELL` = 0,5 cellule par tuile, soit environ 9 m pour `Ground_Rust`), chacune avec un décalage et une rotation aléatoires, fondues entre elles (`HEX_EXPONENT` 4) en favorisant l'échantillon le plus contrasté (`HEX_CONTRAST`). La normal map est tournée avec sa cellule. Coût : 3 lectures au lieu d'une par texture.
- Il calcule la heightmap (résolution `size+1`, hauteur max 1024) **en asynchrone** (`Task.Run`). Le bruit combine plusieurs couches de Perlin : variation locale, variation globale, montagnes et creux.
- Émet `ev_OnTerminateGeneration` à la fin.
- `HeightAt` donne la hauteur générée en un point du bruit, et `ChunkManager.GeneratedHeightAt(x, z)` en une position du monde, chargée ou non (sans les aplanissements). La carte du [[Panneau de contrôle]] et la [[Minimap]] s'en servent pour le relief.

Voir aussi : [[Objets du décor]] · [[Sauvegarde]]
