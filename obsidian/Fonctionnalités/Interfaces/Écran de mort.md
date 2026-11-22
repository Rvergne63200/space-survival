---
tags: [jeu-colonie, fonctionnalite, interfaces, menus]
---

# Écran de mort

`DeathScreenUI`, dans `Bootstrap`, `DeathScreen.uxml`.

- Filtre rouge (classe `death-root`) et deux boutons.
- « Réapparaître » est grisé avec un compte à rebours pendant `respawnDelay` secondes (3).
- « Menu principal » fait réapparaître le joueur, sauvegarde, puis charge le menu.
- Ce n'est pas une interface : Échap ne le ferme pas et n'ouvre pas le menu pause. Le curseur est libre (mode `Menu`).

Voir aussi : [[Mort et réapparition]] · [[Interfaces UI Toolkit]]
