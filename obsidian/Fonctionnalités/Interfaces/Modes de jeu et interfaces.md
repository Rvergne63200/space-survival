---
tags: [jeu-colonie, fonctionnalite, interfaces]
---

# Modes de jeu et interfaces

- **PlayingMode** (`[Flags]`) : `Default`, `Interface`, `Build`. Plusieurs modes peuvent être actifs en même temps.
- **ModeManager** (singleton C# statique) : `EnableMode` et `DisableMode` modifient le masque de modes et émettent `ev_OnUpdateOptions`. `IsEnabled(option)` renvoie vrai si un mode actif contient cette option.
- **PlayerModeOptions** : table statique mode → options. Le mode `Interface` active `player_movement_disabled`, `interface_manager_cursor_visible` et `camera_movement_locked`.
- **InterfaceManager** (singleton C# statique) : liste des interfaces ouvertes (`SetInterface(go, bool)`).
  - Dès qu'une interface est ouverte, il active le mode `Interface` : curseur visible et déverrouillé, mouvement et caméra bloqués. Il le désactive quand plus rien n'est ouvert.
  - `CloseInterfaces()`, appelé par la touche CloseInterfaces depuis `ParentUI`, ferme tout.
- **ParentUI** : racine de l'UI instanciée par UIManager. Elle référence les sous-UI (notifications de ramassage, conteneur, craft) et affiche le slot tenu au curseur.
- **ParentedUI** : classe de base des éléments d'UI. Elle reçoit `OnUpdateManager` et `OnUpdatePlayerContext` quand le manager ou le joueur change.

Voir aussi : [[Interfaces UI Toolkit]] · [[Menu pause]] · [[Pose et destruction]] (mode `Build`)
