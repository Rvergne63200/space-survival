---
tags: [jeu-colonie, pipeline-3d, formes]
---

# Géométrie des modèles

> Les formes voulues (courbes nettes, facettes discrètes pour la technologie et formes douces pour la nature, chanfreins, pas d'arête à 90°, détail par éléments rapportés, socles) relèvent de la DA, qui fait foi : voir [[03 - Formes et silhouettes]]. Cette note donne seulement leur mise en œuvre.

- **Chanfreins :** large chanfrein à 45° de 2 à 80 cm selon la taille de la pièce (paramètre `ch` de `cbox`), et un **edge break** de 6 mm (`EB = 0.006`) sur toutes les arêtes restantes.
- **Nombre de côtés des cylindres et pièces de révolution** (`oct_cyl(..., sides=N)`, `add_cyl` / `add_revolve` avec `segs`) :
  - **machines et bâtiments** : facettes discrètes, **12 à 16 côtés** (16 pour les grandes pièces). Les anciens modèles à 8 côtés sont plus facettés que la règle actuelle ;
  - **éléments naturels** : lisses, 24 côtés ou plus, sans facettes visibles.
- Dômes avec `geodesic.py`.
- **Section standard des modules habitables :** profil à 6 côtés (`prof6`), avec fond plat, flancs verticaux et deux grands chanfreins à 45° en haut.
- **Éléments rapportés** posés 1 à 6 mm au-dessus de la surface (jamais dans le même plan, voir [[16 - Erreurs à ne pas refaire]]) :
  - panneaux inset `_Dark`, grilles d'aération (lamelles `_Metal`) ;
  - vis (`add_screw`) sur les brides ;
  - filets néon (`pill`), bandes `_Dark` autour des caissons ;
  - balises `_Orange` sur le toit, petites consoles (écran `_Glow` et boutons).
- **Socles :** pieds ou jupe qui descendent légèrement sous le sol (z < 0).
- **Shading :** `shade_smooth()` puis `set_sharp_from_angle(angle)`. Une arête est vive quand l'angle entre ses deux faces dépasse `angle` ; entre deux côtés d'un cylindre à N côtés, il vaut 360° / N (30° à 12 côtés, 22,5° à 16).
  - **machines et bâtiments** : **20°**, pour que les facettes de 12 à 16 côtés restent lisibles et les chanfreins nets ;
  - **éléments naturels** : 35° ou plus, pour un rendu doux ;
  - **vitres** : 5°.
  - Valeurs de départ à valider sur les rendus avec l'utilisateur.

[[00 - Index Pipeline 3D|Index]] · ← [[02 - Palette des matériaux]] · [[04 - Dimensions de référence]] →
