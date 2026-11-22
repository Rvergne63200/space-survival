---
tags: [jeu-colonie, fonctionnalite, interfaces, hud]
---

# Minimap

`MinimapUI`, sur le GameObject HUD, éléments `minimap` de `Hud.uxml`, dossier `Scripts/Behaviours/UI/Map`.

- Ronde, en haut à droite. Elle tourne avec la vue : ce qui est devant le joueur est en haut, et un « N » rouge suit le bord pour indiquer le nord.
- **Rendu** : stylisé comme la carte du [[Panneau de contrôle]] (`MapUtility`, partagé avec `NetworkMapElement`), sans le réseau électrique. Relief en teintes avec ombrage et courbes de niveau tous les 4 m (lignes vectorielles lissées, `MapUtility.DrawContours`), grille tous les 25 m, contours des bâtiments et couloirs (bleu) et des objets posés (orange), vus de dessus.
- **Tuile** : le relief est calculé depuis la seed sur un carré de `tileFactor` (3) × `radius` de côté, en `topographyResolution` (256) pixels. Il est recalculé quand le joueur s'approche de son bord. L'élément `minimap-image` est déplacé et tourné autour du joueur. Les contours sont recollectés toutes les `refreshInterval` (1 s).
- Une flèche jaune au centre représente le joueur. Le rayon affiché (`radius`) est fixe et réglable dans l'Inspector (50 m, sur le prefab `UI`). Légèrement transparente (`opacity` 0,8 dans `Game.uss`). Fond plus clair et plus transparent que la carte du panneau de contrôle (`lowColor`, `highColor`, `backgroundAlpha` 0,5, et fond du cercle à 25 %), lignes et contours inchangés.
- Elle est masquée sans joueur ou à la mort.

Voir aussi : [[Horloge]] · [[Interfaces UI Toolkit]] · [[Terrain procédural]]
