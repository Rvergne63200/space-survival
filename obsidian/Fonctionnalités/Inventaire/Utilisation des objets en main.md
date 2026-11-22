---
tags: [jeu-colonie, fonctionnalite, inventaire, outils]
---

# Utilisation des objets en main

- Pour un outil, `Use1` (clic gauche) exécute ses actions principales et `Use2` (clic droit) ses actions secondaires (`ToolData.PrimaryActions` / `SecondaryActions`, des `ItemAction` comme `ApplyModifiersAction`). Sans action secondaire, un `ConsummableData` est consommé au clic droit. Les deux sont ignorés quand une interface est ouverte ou en mode construction (`OPTION_USE_DISABLED`).
- **Modes des actions** (`ItemAction.Mode`) : les outils s'utilisent au clic gauche.
  - `Press` (`ItemAction`) : `Execute` une fois par clic (arme qui tire, consommable).
  - `Toggle` (`ToggleItemAction`) : chaque clic alterne allumé / éteint (`SetOn`). L'action s'éteint quand l'item quitte la main.
  - `Hold` (`HoldItemAction`) : `Execute` démarre, `Hold(context, heldTime)` est appelé à chaque frame jusqu'à ce qu'il renvoie false, puis `End(context, completed)`. Le relâchement du bouton, l'ouverture d'une interface ou le changement d'item arrêtent l'action (`completed` = false).
- **Item en main** : `PlayerInventory` vérifie à chaque frame si l'item en main change (autre slot, slot vidé, joueur absent). Il appelle alors `OnUnequip` sur les actions de l'ancien outil (`ToolData.Unequip`).
- `ItemUseContext.View` : le transform de la caméra du joueur.
- L'état d'une action (allumée, maintenue) est porté par l'asset ToolData : il est partagé par tous les items de ce type.

Voir aussi : [[Lampe torche]] · [[Barre des objets]] · [[Modificateurs et consommables]] · [[Inventaire du joueur]]
