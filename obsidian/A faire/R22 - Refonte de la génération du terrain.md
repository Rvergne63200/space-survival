---
tags: [jeu-colonie, a-faire, refonte, terrain]
id: R22
categorie: Refonte
feature: Terrain
statut: en cours
---

# R22 - Refonte de la génération du terrain

Remplacer la génération actuelle (quelques couches de Perlin, rendu jugé simpliste) par un relief réaliste, et revoir la conception du code (une responsabilité par classe).

- Choix de l'utilisateur (2026-11-23) : plaines praticables avec des reliefs forts par endroits ; cratères, dunes et plaines de sable, falaises / canyons / mesas, anciennes rivières et lacs ; prototype visuel dans l'éditeur avant de brancher en jeu ; les sauvegardes actuelles peuvent être cassées.
- Fait : bruit déterministe avec seed (`TerrainNoise`), outil d'aperçu `Tools > Terrain Preview` (carte de 4 km et rendus 3D dans `ArtPipeline/Renders/Terrain_*.png`, avec la part de sable, sédiments et roche).
- Fait : conception, une responsabilité par classe, dans `Assets/Scripts/Utilities/Terrain/` :
  - `TerrainShape` applique dans l'ordre les paysages (`ITerrainFeature`) à un `TerrainPoint` et rend un `TerrainSample` (hauteur, sable, sédiments, roche) ;
  - un fichier par paysage dans `Features/` (réglages + règle) : `TerrainRegions`, `TerrainPlains`, `TerrainPlateaus` (avec les canyons), `TerrainDunes` (avec les bassins), `TerrainDryLakes`, `TerrainDryRivers`, `TerrainCraters` ;
  - réglages dans le ScriptableObject `TerrainShapeSettings`, asset `Assets/Resources/Terrain/TerrainShape.asset`, utilisé par l'aperçu et par le jeu.
- Fait : branché en jeu. `ChunkManager` crée le `TerrainShape` avec la seed entière (plus de `perlinOffset`), `TerrainGenerator` ne fait plus que calculer et appliquer les hauteurs d'un chunk, et la carte et la minimap échantillonnent le relief en parallèle.
- Contrainte : avec un point par mètre, une pente de plus de 60 à 70° se dessine en dents de scie (vu en jeu sur les paliers des falaises, 2026-11-23). Parois des paliers, bord des plateaux (`edgeWidth` 0,15) et parois des canyons élargis : pentes de plus de 60° sur 0,008 % du sol, 69° au maximum. Toute nouvelle forme raide doit respecter cette limite (vérifier la pente par le calcul) ; les vraies parois verticales seront des modèles ([[F67 - Éléments naturels 3D|F67]]).
- Reste : itérer sur le relief avec l'utilisateur (plaines lisses, paliers des falaises trop réguliers, dunes peu visibles), répartir les sols selon le relief ([[F30 - Biomes|F30]]), carte et aplanissements.
- Les formes qu'un relief en grille ne peut pas faire (arches, surplombs) : [[F67 - Éléments naturels 3D|F67]].

Voir aussi : [[Terrain procédural]] · [[Paysages de la planète]]
