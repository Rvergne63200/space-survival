---
tags: [jeu-colonie, fonctionnalite, joueur, stats]
---

# Stats du joueur

- **StatName** : `Health`, `Endurance`, `Hunger`, `Thirst` et `Oxygen` (sérialisé par valeur : ajouter les nouvelles stats à la fin).
- **PlayerStats** : liste de paires (StatName → Stat), sérialisée sur le PlayerManager, donc persistante. À chaque frame en jeu (pas dans le menu), il appelle `Actualize(deltaTime)` sur chaque stat.
  - **Faim** et **soif** baissent en continu (`baseRegeneration` négatif) : une jauge pleine tient environ 40 min pour la faim et 25 min pour la soif.
  - **Oxygène** : il reste au maximum pour l'instant. Il dépendra plus tard du réseau d'oxygène et de bouteilles d'oxygène (système d'équipements, voir [[F37 - Oxygène hors des zones respirables|F37]]).
  - **Stats vitales** (`vitalStats` : faim, soif, oxygène) : tant que l'une d'elles est à 0, la vie baisse de `emptyVitalDamage` (1) par seconde, et ces pertes s'additionnent.
- **Mort** : voir [[Mort et réapparition]].
- **Stat** : `Value` est bornée entre 0 et `MaxValue`. Events `ev_updateValue` et `ev_updateMaxValue`. `Consume(x)` retire une valeur fixe. À chaque frame, la stat gagne `baseRegeneration` plus la somme des markers actifs, multipliée par deltaTime.
- **StatMarker** : modificateur de régénération identifié par une clé. Il vaut un montant par seconde (positif ou négatif) pendant une durée (infinie par défaut), puis il est supprimé.
- **Effets et consommables** : voir [[Modificateurs et consommables]].
- **UI** : `PlayerStatsUI` crée une barre (`StatBarUI`, `StatBar.uxml`) par stat de `statUIDataCollection` (`StatUIData` : stat, icône, couleur), en bas à gauche du HUD. Chaque barre affiche « valeur/max » et un remplissage proportionnel.

Voir aussi : [[Déplacement du joueur]] (sprint et endurance)
