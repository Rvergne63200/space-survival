---
tags: [jeu-colonie, instructions, code, interfaces]
---

# Créer une interface

Toutes les interfaces sont en **UI Toolkit** (plus d'uGUI). L'organisation est décrite dans [[Interfaces UI Toolkit]].

## Mise en page
- UXML dans `Assets/UI/Game/` (interfaces du jeu) ou `Assets/UI/Menus/` (menus), style dans `Game.uss` ou `Menus.uss`. L'utilisateur doit pouvoir retoucher la mise en page dans le **UI Builder** : pas de style inline compliqué, des classes USS.
- Le script retrouve les éléments **par leur nom** (`root.Q<Label>("title")`). Un élément absent désactive seulement sa fonction, avec un avertissement.
- Éléments répétés (slots, lignes) : un UXML modèle instancié par le code (`ItemSlot.uxml`, `RecipeSlot.uxml`…), réutilisés d'une mise à jour à l'autre au lieu d'être recréés.

## Interface du jeu (fenêtre qu'on ouvre)
Sur le modèle de `DistributionPanelUI` :
- un UIDocument par interface, sur un GameObject `panel` enfant du prefab `Prefabs/UI/UI` (PanelSettings commun, ordre de tri : 2 pour les fenêtres) ; le script est un singleton `Instance` ;
- **ouvrir** : `panel.SetActive(true)`, puis `InterfaceManager.GetInstance().SetInterface(panel, true)`, puis lier les éléments (le UIDocument reconstruit ses éléments à chaque activation : relier une fois par arbre) ;
- **fermer** : `panel.SetActive(false)` et `SetInterface(panel, false)` ;
- dans `Update`, si le panel a été désactivé de l'extérieur (Échap ou mort : `InterfaceManager.CloseInterfaces`) ou si la cible a disparu, appeler sa fermeture pour libérer l'état ;
- rafraîchissement périodique (0,25 s) avec `Time.unscaledDeltaTime`, plutôt qu'à chaque frame.
- L'inventaire du joueur peut s'ouvrir avec l'interface : `PlayerInventory.OpenWith` / `CloseWith`.

## Éléments du HUD
- Dans `Hud.uxml` (ordre 0), sur le GameObject HUD, masqués sans joueur et à la mort. Ils ne captent pas la souris (`PickingMode.Ignore`), sauf les slots de la barre d'outils.
- Un élément au-dessus des fenêtres (item tenu au curseur) a un ordre plus élevé (4) et ne capte pas la souris.

## Textes
- Tous les textes affichés sont en français. Messages courts au joueur : `NotificationUI.Show(...)`.

Voir aussi : [[03 - Managers et cycle de vie]] · [[06 - Interactions]]
