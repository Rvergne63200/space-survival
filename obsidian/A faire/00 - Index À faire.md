---
tags: [jeu-colonie, a-faire, index]
---

# À faire

Une note par tâche ouverte. Chaque note a ses propriétés : `id`, `categorie`, `feature` (le domaine du jeu concerné, voir [[00 - Index Fonctionnalités]]) et `statut` (`à faire` ou `en cours`).

## Identifiants

- `B` = Bug, `F` = Feature, `O` = Optimisation, `R` = Refonte.
- Ils sont **stables** : une nouvelle tâche prend le numéro suivant de sa catégorie, et un numéro n'est jamais réattribué, même si la tâche est supprimée ou terminée.
- Nom de la note : `ID - titre court` (par exemple `F51 - Système d'oxygène`).
- Une tâche terminée quitte ce dossier et rejoint la note historique de sa catégorie, avec un identifiant de bloc (`^F45`) pour pouvoir y faire un lien.

## Prochains numéros libres

- Bug : **B68** · Feature : **F68** · Optimisation : **O06** · Refonte : **R23**

## Bugs

- [[B03 - Décollage en haut des pentes]] · Mouvement
- [[B07 - Ressenti des déplacements]] · Mouvement
- [[B10 - Pivot de ReplaceObject trop haut]] · Terrain
- [[B21 - Curseur visible après Échap]] · Interfaces
- [[B26 - Flag identified des objets ramassables]] · Interaction

## Features

- [[F03 - Accroupissement]] · Mouvement
- [[F37 - Oxygène hors des zones respirables]] · Stats
- [[F38 - Position du sac de mort]] · Stats
- [[F06 - Vrai glisser-déposer]] · Inventaire
- [[F11 - Données propres à chaque exemplaire]] · Inventaire
- [[F17 - Contour masqué par les obstacles]] · Interaction
- [[F20 - Recettes à plusieurs résultats]] · Crafting
- [[F22 - Catalogue de constructions]] · Construction · *en cours*
- [[F47 - Relais radio]] · Construction
- [[F48 - Crafter]] · Crafting
- [[F49 - Génération d'énergie]] · Énergie · *en cours*
- [[F50 - Réseau électrique complet]] · Énergie · *en cours*
- [[F51 - Système d'oxygène]] · Oxygène
- [[F52 - Machines consommatrices d'oxygène]] · Oxygène
- [[F53 - Système de plantation]] · Plantations
- [[F54 - Cycle jour nuit et météo]] · Monde · *en cours*
- [[F66 - Chutes de météorites]] · Monde
- [[F55 - Informations des réseaux]] · Interfaces
- [[F56 - Horaires du tableau de distribution]] · Énergie · *en cours*
- [[F27 - Réseau d'oxygène]] · Construction
- [[F30 - Biomes]] · Terrain
- [[F31 - Structures générées]] · Terrain
- [[F32 - Ressources générées]] · Terrain
- [[F67 - Éléments naturels 3D]] · Terrain
- [[F65 - Intégration des modèles 3D]] · Modèles · *en cours*

## Optimisations

- [[O03 - Monde infini]] · Terrain

## Refontes

- [[R01 - Refonte du chargement de scènes]] · Bootstrap
- [[R02 - Persistance entre scènes]] · Bootstrap
- [[R05 - Prefab asset modifié au runtime]] · UI
- [[R06 - PlayerInputActions jamais libérés]] · Global
- [[R07 - CharacterController]] · Mouvement
- [[R08 - Refonte du HUD des stats]] · Stats
- [[R09 - Système d'interaction extensible]] · Interaction
- [[R10 - Ids d'items générés]] · Inventaire
- [[R11 - Direction.In de TryMerge]] · Inventaire
- [[R12 - Intégration des constructions au terrain]] · Construction
- [[R13 - Snapping entre points de connexion]] · Construction
- [[R15 - Ids uniques des objets de scène]] · Global
- [[R22 - Refonte de la génération du terrain]] · Terrain · *en cours*

## Historique

- [[Historique - Bugs]]
- [[Historique - Features]]
- [[Historique - Optimisations]]
- [[Historique - Refontes]]
