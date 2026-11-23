---
tags: [jeu-colonie, fonctionnalite, monde, terrain]
---

# Terrain procédural

## ChunkManager (singleton, scène Monde)
- Grille de chunks de `chunkSize` (64 m). Les chunks situés à moins de `generationDistance` (5 dans `0_World`) de celui du joueur sont chargés, soit une grille de 11×11. Ce calcul n'est refait que quand le joueur change de chunk.
- Les chunks éloignés sont **désactivés**, et réactivés au retour. Au-delà de `generationDistance + destroyMargin` (2 chunks de plus), ils sont **détruits** avec leur `TerrainData` pour libérer la mémoire. Au retour, ils sont régénérés à l'identique : relief tiré de la seed, aplanissements rejoués, et objets restaurés par le `SaveManager`, qui garde leur état (`UnloadObjects`).
- Les chunks chargés sont reliés à leurs voisins (`Terrain.SetNeighbors`, vérifié à chaque frame dans `LateUpdate`, auto-connexion Unity désactivée), ce qui évite les fissures de LOD vues de loin.
- La seed (entière) et les réglages `TerrainShapeSettings` (champ `shapeSettings`, vide = l'asset `Resources/Terrain/TerrainShape`) donnent un `TerrainShape` (voir plus bas), créé au `Start`. Le terrain est continu et déterministe. La seed de la scène ne sert qu'aux nouvelles parties : une partie sauvegardée garde sa seed. Les sauvegardes faites avec l'ancienne génération (Perlin) n'ont plus le même relief.
- `ToReplace` : le joueur et les transforms configurés sont replacés sur la surface quand leur chunk a fini de se générer. Le joueur est fourni par `PlayerManager` (`ev_spawnPlayer`), et la référence est mise à jour à chaque respawn.
- `AddTerrainEdit` : aplanissement sous une construction (voir [[Terrain sous les constructions]]), appliqué aux chunks chargés, enregistré et rejoué à la génération des autres.
- Gizmos de debug : chunks générés et chargés.

## TerrainGenerator (sur le prefab `Chunk`)
- Duplique le `TerrainData`. Le terrain utilise le shader `Terrain/World UV Standard` : les textures sont positionnées depuis la position monde, donc continues entre les chunks.
- Le `TerrainData` dupliqué est `Assets/Terrain/0_TestTerrain.asset` (prefab `Chunk`). Il n'a pour l'instant **qu'un calque**, `Ground_Rust` (rouille, le sol des gisements), peint à 100 %. Trois autres sols sont prêts, pour des zones placées selon le relief (refonte du terrain à venir) : `Ground_Ochre` (ocre-sable), `Ground_Dust` (rouille sombre, poussière de basalte), `Ground_Sediment` (sédiments pâles). Textures raccordables de 16 m, sols presque uniformes à nuances douces (réglages `PATCH` et `NUANCE`), couleur et normal map dans `Assets/Terrain/Textures/`, générées par `ArtPipeline/Blender/ground_build.py` (`GROUNDS`).
- **Zones** (`GenerateZones`, en asynchrone avec le relief ; inactif tant que le `TerrainData` n'a qu'un calque, l'utilisateur a jugé la répartition au hasard peu naturelle) : chaque calque a son propre bruit de Perlin en coordonnées monde (avec la seed), le plus fort l'emporte avec des transitions douces (`zoneSharpness` 8). Zones de 150 à 300 m (`zoneSize` 400), fréquence réglée par `zoneWeights` (1,25 / 1 / 0,92 / 0,85 : environ 50 / 21 / 20 / 9 % du sol). Carte de 64 × 64 texels par chunk (`zoneResolution`, 1 m), dont les bords coïncident avec ceux des voisins. Rien n'est sauvegardé : les zones sont recalculées à l'identique.
- **Anti-répétition** (`TerrainWorldUVSplatmapCommon.cginc`, `HexSetup` / `HexDiffuse` / `HexNormal`) : chaque calque est lu en *hex tiling*. Le plan est découpé en cellules hexagonales (`HEX_CELL` = 0,5 cellule par tuile, soit environ 9 m pour `Ground_Rust`), chacune avec un décalage et une rotation aléatoires, fondues entre elles (`HEX_EXPONENT` 4) en favorisant l'échantillon le plus contrasté (`HEX_CONTRAST`). La normal map est tournée avec sa cellule. Coût : 3 lectures au lieu d'une par texture.
- **Projection triplanaire** (`TriplanarSetup` / `TriplanarDiffuse` / `TriplanarNormal`) : sur les pentes, chaque calque est aussi projeté depuis les côtés (plans ZY et XY, à la même échelle), pour que la texture ne s'étire plus sur les falaises. Les trois projections, chacune en hex tiling, sont fondues selon la normale de la surface (`TRIPLANAR_SHARPNESS` 4). La normale vient de la normal map du terrain, donc seulement en mode instancié (`drawInstanced`, le cas des chunks), sinon projection du dessus seule. La normal map est appliquée dans l'espace monde, puis ramenée dans le repère de la surface. Une projection n'est lue que là où son poids n'est pas nul, et un calque absent du pixel n'est pas lu : sur un sol plat, le coût est celui d'avant.
- Le rendu lointain (au-delà de `basemapDistance`, 1000 m) utilise la base map d'Unity, sans ces deux corrections.
- Il calcule la heightmap (résolution `size+1`, soit 1 m, hauteur max 1024) **en asynchrone** (`Task.Run`) avec le `TerrainShape` reçu de `ChunkManager` (`Initialize(coord, shape, size)`), rejoue les aplanissements, puis émet `ev_OnTerminateGeneration`.
- `ChunkManager.GeneratedHeightAt(x, z)` donne la hauteur générée en une position du monde, chargée ou non (sans les aplanissements). La carte du [[Panneau de contrôle]] et la [[Minimap]] s'en servent pour le relief ; `MapUtility` échantillonne les colonnes en parallèle (environ 50 ms pour une tuile de minimap de 256 × 256, contre 0,4 s sur un seul cœur).

## Forme du relief (`Assets/Scripts/Utilities/Terrain/`)
- `TerrainShape` : fonction pure de la position, de la seed et des réglages (déterministe, utilisable depuis plusieurs threads). Elle applique dans l'ordre les paysages (`ITerrainFeature`) à un `TerrainPoint` et rend un `TerrainSample` (hauteur, sable, sédiments, roche). Environ 6 µs par point.
- Un fichier par paysage dans `Features/`, avec sa section de réglages :
  - `TerrainRegions` : carte des régions sur environ 3 km (bas = bassins de sable, milieu = plaines, haut = plateaux) et position déformée pour les grandes formes ;
  - `TerrainPlains` : plaines à 120 m, ondulation douce et groupes de collines ;
  - `TerrainPlateaus` : plateaux de 60 m, falaises en 4 paliers, coupés par des canyons ;
  - `TerrainDunes` : bassins de sable 16 m plus bas, dunes dans le sens du vent (face douce au vent, face raide sous le vent) ;
  - `TerrainDryLakes` : fonds de lacs plats ; `TerrainDryRivers` : lits de rivières sinueux dans certaines zones de plaine ;
  - `TerrainCraters` : cratères sur 3 échelles de cellules (900, 260, 70 m), cuvette à fond plat, rebord et éjectas.
- `TerrainNoise` : bruit de gradient avec seed (16 directions tirées par hachage), fBm et bruit en crêtes.
- Les masques sable / sédiments / roche ne sont pas encore utilisés pour les calques de sol ([[F30 - Biomes|F30]]).
- Aperçus dans l'éditeur : `Tools > Terrain Preview` (carte de 4 km, rendus 3D d'1 km : vue haute, vue à hauteur d'homme, et vue à 15 m de la pente la plus raide, dans `ArtPipeline/Renders/Terrain_*.png`), avec les réglages de l'asset.

Voir aussi : [[Objets du décor]] · [[Sauvegarde]]
