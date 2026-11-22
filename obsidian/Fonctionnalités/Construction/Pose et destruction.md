---
tags: [jeu-colonie, fonctionnalite, construction]
---

# Pose et destruction

Deux façons de poser quelque chose, avec le même fantôme vert (valide) ou rouge (invalide) :
- **Placement** : un item plaçable de l'inventaire (une caisse, par exemple) est posé tel quel (voir [[Objets posés]]).
- **Build** : une structure choisie dans la [[Roue de construction]] est posée sous forme de **plan** (voir [[Plans de construction]]), auquel on apporte ensuite les ressources de son coût.

## Builder (sur le joueur)
Pendant le placement, le build ou la destruction, le mode `PlayingMode.Build` désactive l'usage de l'item en main (clics). Tout est masqué pendant qu'une interface est ouverte.
- **Fantôme** : copie visuelle du prefab (meshes seulement, sans script ni collider : `BuildUtility.CreateVisual`), posée là où vise la caméra, à **8 m** maximum (`range`). Pour une structure, la portée s'ajoute au rayon de son emprise, et son centre reste à au moins « rayon + 1 m » du joueur, qui ne peut donc pas se trouver dedans. Un objet qui doit apparaître tel qu'il sera posé (`IPlacementPreview` sur la racine de son prefab : générateur à charbon) a pour fantôme une copie de son modèle (`PreviewModel`, avec le script d'animation du modèle, sans les composants de jeu), mise à jour à chaque frame par `UpdatePreview`.
- **Commandes** : R le fait tourner (90°/s), Shift + R par pas de 45°. Clic gauche : poser. Clic droit ou Échap : annuler (Échap quitte d'abord le mode, puis ouvre le menu pause).
- **Bandeau d'indication** (`BuildModeUI`, dans `Bootstrap`, `BuildModeHint.uxml`) : en bas de l'écran, le mode en cours (Placement, Construction ou Destruction, avec le nom de l'objet) et ses commandes. Il ne capte pas la souris.
- **Validité** : surface d'appui assez horizontale, aucun objet dans la boîte englobante (le terrain excepté, avec une boîte réduite de 10 % pour ne pas compter le sol). Pour une structure, s'y ajoute un écart de hauteur du terrain sous les 4 coins ≤ `MaxSlope`.
- **Placement** : démarre tout seul quand l'item en main est un `PlaceableItemData`. La pose instancie son `placedPrefab` (`PlacedObject`) et retire un item du slot en main. Après un clic droit, le fantôme ne revient qu'au prochain changement d'item en main.
- **Build** : la pose crée le plan, après avoir aplani le terrain dessous (si `FlattenTerrain`, voir [[Terrain sous les constructions]]).
- Modes particuliers : pose du [[Sas]] et du [[Couloir]], pose des câbles ([[Câble électrique]]).

## Destruction
La structure, le plan ou l'objet posé visé est surligné en rouge, et un clic gauche le détruit. Clic droit pour quitter le mode. Les ressources rendues sont décrites dans [[Structures et catalogue]] et [[Objets posés]].

Voir aussi : [[Modes de jeu et interfaces]]
