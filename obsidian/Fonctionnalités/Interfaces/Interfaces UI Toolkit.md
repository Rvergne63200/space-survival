---
tags: [jeu-colonie, fonctionnalite, interfaces]
---

# Interfaces UI Toolkit

## Menus
Mise en page dans `Assets/UI/Menus`, modifiable dans le UI Builder.
- Les scripts retrouvent les éléments par leur nom (`name` dans l'UXML). Un élément absent désactive seulement sa fonction, avec un avertissement dans la console. Style commun : `Menus.uss`.
- Menus : [[Menu principal]], [[Menu pause]], [[Écran de mort]], [[Messages au joueur]].

## Interfaces du jeu
Mise en page dans `Assets/UI/Game`, modifiable dans le UI Builder.
- Prefab `Prefabs/UI/UI` (instancié par le `UIManager`, racine `ParentUI`). Chaque interface est un UIDocument (PanelSettings commun, style `Game.uss`) :
  - **HUD** (`Hud.uxml`, ordre 0) : réticule, texte de l'objet pointé, barres de stats, ramassages, barre d'outils et cadre du slot en main. La [[Minimap]] est en haut à droite, l'[[Horloge]] dessous. Seuls les slots de la barre d'outils captent la souris.
  - **Inventaire** (`Inventory.uxml`), **Conteneur** (`Storage.uxml`) et **Fabrication** (`CraftingStation.uxml`), ordre 2 : chacun est un GameObject `Panel` activé à l'ouverture et enregistré comme interface (`InterfaceManager`). Fermé par Échap ou à la mort, il prévient son propriétaire (inventaire fermé, `Searcher.ClearCurrentSearchedStorage`, `Crafter.ClearStation`).
  - **Item au curseur** (`HeldItem.uxml`, ordre 4) : au-dessus des interfaces, ne capte pas la souris.
- Modèles instanciés par le code : `ItemSlot.uxml`, `StatBar.uxml`, `PickupInfo.uxml`, `RecipeSlot.uxml`, `RecipeIngredient.uxml`. Les scripts retrouvent les éléments par leur nom.
- Sans joueur (menu principal), le HUD et l'item au curseur sont masqués (`ParentUI.SetHudVisible`).

Voir aussi : [[Modes de jeu et interfaces]] · [[Clics de l'inventaire]] · [[Station de craft]]
