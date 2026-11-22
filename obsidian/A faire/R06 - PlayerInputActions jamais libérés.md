---
tags: [jeu-colonie, a-faire, refonte, global]
id: R06
categorie: Refonte
feature: Global
statut: à faire
---

# R06 - PlayerInputActions jamais libérés

Les `PlayerInputActions` créés par chaque composant ne sont jamais `Dispose()` (un asset d'input est perdu à chaque respawn du joueur).

Voir aussi : [[Déplacement du joueur]] · [[Caméra]]
