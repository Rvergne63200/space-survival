---
tags: [jeu-colonie, pipeline-3d, direction-artistique, materiaux]
---

# Textures

- **Technologie** (machines, bâtiments) : matériaux unis, pas de texture (voir [[02 - Palette des matériaux]]).
- **Éléments naturels** (gisements, roches) : textures cuites. Le rendu visé (textures peintes plutôt que scannées) est décrit dans la DA, voir [[05 - Matériaux et textures]].

## Fichiers et import

- Un jeu de textures = `<Prefix>_BaseColor.png`, `<Prefix>_Normal.png`, `<Prefix>_MetallicSmoothness.png` (et en option `_Occlusion`, `_Emission`), dans le dossier du modèle.
- `ModelImportSetup` crée `M_<Prefix>.mat` (Standard) pour **chaque** jeu de textures du dossier, et le FBX doit utiliser un matériau Blender du même nom `M_<Prefix>`. Exemple : `Deposit_Coal/` contient `Deposit_Coal_*` (la dalle, `M_Deposit_Coal`) et `Deposit_Coal_Ore_*` (le minerai, `M_Deposit_Coal_Ore`).
- `MetallicSmoothness` : R = metallic, A = smoothness (1 − roughness).

## Méthode qui marche (gisements, 2026-11-23)

1. **Modèle du jeu** : relief à 2 cm, flouté (pas d'escalier sur les bords raides), décimé en gardant le détail dans les fissures, puis **retriangulé en Delaunay 2D** avec des sommets ajoutés tous les 25 cm (aucun repli, triangles de 40 cm au plus), UV dépliées en une seule île.
2. **Modèle détaillé** (1 cm, ~280 000 sommets) : le même relief avec le détail fin (arêtes, petits creux).
3. **Matériau en nœuds Cycles** sur le modèle détaillé : rampes de couleur sur des bruits, saleté dans les creux (nœud Ambient Occlusion en `only_local`), arêtes éclaircies (nœud Bevel), assombrissement avec la profondeur **calculé au pixel** (hauteur de référence en attribut − hauteur réelle).
4. **Cuisson** du modèle détaillé vers le modèle du jeu : couleur (`DIFFUSE`, passe couleur), normal map (tangente), rugosité. Cage 8 cm, distance 20 cm.
5. Les éléments durs (minerai) sont de la **vraie géométrie** à facettes avec une petite texture raccordable, pas une couleur peinte sur la roche.

- Le sol orange existe aussi en texture raccordable pour le terrain : `Assets/Terrain/Ground_Rust.terrainlayer` (`ground_rust.py`, tuile de 8 m).

[[00 - Index Pipeline 3D|Index]] · ← [[08 - Vitres]] · [[10 - Modélisation Blender]] →
