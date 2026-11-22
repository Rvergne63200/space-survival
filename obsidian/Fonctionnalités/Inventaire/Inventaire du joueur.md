---
tags: [jeu-colonie, fonctionnalite, inventaire]
---

# Inventaire du joueur

`PlayerInventory`, sur le PlayerManager, donc persistant.

- Inventaire de **32 slots** et barre d'outils de **6 slots**.
- `Add` : un slot qui contient déjà le même item est prioritaire (barre d'outils, puis inventaire), puis les slots vides de la barre d'outils, puis ceux de l'inventaire. `Remove` retire de l'inventaire, puis de la barre d'outils.
- Touche Inventaire (I ou Tab) : ouvre ou ferme l'inventaire, avec l'équipement sous la grille (`ev_UpdateIsOpen`). Voir [[Équipement et jetpack]].
- Molette : change le slot en main (`HandedIndex`, avec bouclage). Sans effet quand une interface (inventaire, panneau de contrôle…) ou un menu est ouvert.
- **Usage de l'item en main** : voir [[Utilisation des objets en main]].
- Touche Drop (inventaire ouvert) : jette devant le joueur tout le contenu du slot tenu au curseur. Le slot du curseur est aussi jeté automatiquement quand la dernière interface se ferme.
- Clics dans l'inventaire : voir [[Clics de l'inventaire]].

Voir aussi : [[Données des items]] · [[Interfaces UI Toolkit]]
