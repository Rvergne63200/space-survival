---
tags: [jeu-colonie, fonctionnalite, interfaces, menus]
---

# Menu pause

`PauseMenuUI`, dans `Bootstrap`, `PauseMenu.uxml`.

- Échap ferme les interfaces ouvertes ; s'il n'y en a aucune, il ouvre le menu pause (en jeu). Le menu pause est lui-même une interface, donc Échap le ferme.
- Pendant la pause, `timeScale` vaut 0.
- Boutons : Reprendre, Retour au menu principal (sauvegarde puis chargement du menu) et Quitter.
- **Dans l'éditeur Unity**, la touche est ² à la place d'Échap (`ParentUI`, override `#if UNITY_EDITOR`) : Échap y libère toujours le curseur. Les builds gardent Échap.

Voir aussi : [[Modes de jeu et interfaces]] · [[Interfaces UI Toolkit]]
