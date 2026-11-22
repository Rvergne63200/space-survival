---
tags: [jeu-colonie, pipeline-3d, direction-artistique]
---

# Objets muraux

- Le mur intérieur des dômes est un **cylindre lisse** de rayon `R_OUT - 0.25` (3,75 / 5,75 / 7,75 m). Sa partie pleine va du sol (0.215) à 0.95 m, puis vient la ceinture jusqu'à 1.20.
- **Origine** au point de contact avec le mur (dos à y = 0), au milieu de la hauteur de l'objet. Face avant vers -Y.
- Un objet courbe a un rayon **plus serré que le plus petit dôme** (`WallScreen` : 3,6 m au dos), et il est posé sur une **platine graphite** (2,5 cm). Ses bords ne recoupent ainsi aucun des trois murs : vérifier avec `wall_clearance()` (`ws_build.py`).
- **Courbe lisse** (`sweep()` de `ws_build.py`, 48 segments). Une façade d'un seul tenant vaut mieux qu'un cadre en 4 morceaux, qui laisse des raccords visibles aux coins.
- Pour les rendus, un mur temporaire `_Wall` (préfixe `_`, jamais exporté).

[[00 - Index Pipeline 3D|Index]] · ← [[06 - Stockage et empilage]] · [[08 - Vitres]] →
