---
tags: [jeu-colonie, fonctionnalite, joueur, stats]
---

# Mort et réapparition

- **Mort** (vie à 0, `PlayerManager.KillPlayer`) : l'inventaire, la barre d'outils et l'objet tenu au curseur sont placés dans un **sac** (prefab `DeathBag`, conteneur fouillable) posé au sol à l'endroit de la mort. Le sac est sauvegardé (`prefab:DeathBag`) et disparaît une fois vidé (vérifié en fin de frame, à chaque changement de l'un de ses slots).
  - L'équipement porté est aussi lâché dans le sac (voir [[Équipement et jetpack]]).
  - Les interfaces se ferment et l'[[Écran de mort]] s'affiche. Les stats sont figées (`PlayerManager.IsDead`).
- **Réapparaître** (`PlayerManager.Respawn`) : toutes les stats remontent au maximum, et le joueur réapparaît sur le `PlayerSpawnPoint`.

Voir aussi : [[Stats du joueur]] · [[Scènes et chargement]]
