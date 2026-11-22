---
tags: [jeu-colonie, pipeline-3d, direction-artistique, dimensions]
---

# Dimensions de référence

- Le joueur mesure environ 1,80 m. Le sol de marche intérieur est à **0.215 m** : sol du dôme à 0.20, plus 1,5 cm de seuil.
- **Murs pleins :** 1.20 m de haut maximum, soit à peu près à mi-hauteur du personnage. Au-dessus, c'est **vitré**.
- **Bride standard**, la même pour le connecteur de dôme, le sas et tous les tunnels. Profils `(x, z)` :
  - Passage libre : `PASS = prof6(0.90, 0.215, 2.60, 0.40)`, soit 1,80 m de large et 2,60 m de haut.
  - Coque : `SHELL = prof6(1.30, 0.00, 2.95, 0.55)`.
  - Bride : `offset_poly(SHELL, 0.15)`, épaisseur 0.12, avec des vis tous les 55 cm.
- Tout nouveau module qui se raccorde au réseau **doit réutiliser cette bride**. Il se chaîne par des empties enfants nommés **`Socket`** (axe de sortie = -Y Blender).

- **Passages praticables :** le collider du joueur fait environ **0,85 m de diamètre et 1,70 m de haut** (CapsuleCollider de rayon 0,5 et de hauteur 2, à l'échelle 0,85 dans `Player.prefab`). Une porte de module autonome doit faire au moins 1,1 m de large et 1,9 m de haut.

[[00 - Index Pipeline 3D|Index]] · ← [[03 - Géométrie des modèles]] · [[05 - Modèles habitables]] →
