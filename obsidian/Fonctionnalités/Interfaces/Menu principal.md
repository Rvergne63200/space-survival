---
tags: [jeu-colonie, fonctionnalite, interfaces, menus]
---

# Menu principal

`MainMenuUI`, scène `0_MainMenu`, `MainMenu.uxml` : mode `PlayingMode.Menu`, avec un curseur libre et sans contrôle du joueur.

- **Nouvelle partie** : nom (pré-rempli « Partie N », doit être unique) et seed. La seed est pré-remplie au hasard, avec un bouton Aléatoire ; un texte non numérique est converti en nombre (`SaveManager.ParseSeed`).
- **Charger une partie** : liste des sauvegardes (`SaveSlot.uxml`), de la plus récente à la plus ancienne, avec le nom, la seed et la date. Chaque ligne a un bouton Charger et un bouton Supprimer (avec confirmation).
- **Quitter**.

Voir aussi : [[Sauvegarde]] · [[Interfaces UI Toolkit]] · [[Scènes et chargement]]
