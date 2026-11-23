---
tags: [jeu-colonie, pipeline-3d, direction-artistique]
---

# Objets muraux

- Le mur intérieur des dômes est un **cylindre lisse** de rayon `R_OUT - 0.25` (3,75 / 5,75 / 7,75 m). Sa partie pleine va du sol (0.215) à 0.95 m, puis vient la ceinture jusqu'à 1.20.
- **Origine** au point de contact avec le mur (dos à y = 0), au milieu de la hauteur de l'objet. Face avant vers -Y.
- Un objet courbe a un rayon **plus serré que le plus petit dôme** (`WallScreen` : 3,6 m au dos), et il est posé sur une **platine graphite** (2,5 cm). Ses bords ne recoupent ainsi aucun des trois murs : vérifier avec `wall_clearance()` (`ws_build.py`).
- **Courbe lisse** (`sweep()` de `ws_build.py`, 48 segments). Une façade d'un seul tenant vaut mieux qu'un cadre en 4 morceaux, qui laisse des raccords visibles aux coins.
- Un objet **plat** peut rester plat s'il est posé sur une platine graphite plus étroite que lui : ses bords restent devant le mur (`Battery_Wall` : 0,60 m de large, platine de 0,36 m et 2,5 cm, 1,6 cm de marge sur le dôme de 8 m). Les coins de la platine rentrent de quelques millimètres dans le mur, sans que ça se voie.
- En jeu, un objet mural se pose avec `PlaceableItemData.WallMounted` : centre à 0,58 m au-dessus de l'origine du dôme, sur la partie pleine du mur.
- Pour les rendus, un mur temporaire `_Wall` (préfixe `_`, jamais exporté).

[[00 - Index Pipeline 3D|Index]] · ← [[06 - Stockage et empilage]] · [[08 - Vitres]] →
