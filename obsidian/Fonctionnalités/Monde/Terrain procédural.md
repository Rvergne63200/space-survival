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
- Il calcule la heightmap (résolution `size+1`, hauteur max 1024) **en asynchrone** (`Task.Run`). Le bruit combine plusieurs couches de Perlin : variation locale, variation globale, montagnes et creux.
- Émet `ev_OnTerminateGeneration` à la fin.
- `HeightAt` donne la hauteur générée en un point du bruit, et `ChunkManager.GeneratedHeightAt(x, z)` en une position du monde, chargée ou non (sans les aplanissements). La carte du [[Panneau de contrôle]] et la [[Minimap]] s'en servent pour le relief.

Voir aussi : [[Objets du décor]] · [[Sauvegarde]]
