---
tags: [jeu-colonie, fonctionnalite, inventaire, equipement]
---

# Équipement et jetpack

- **Équipement** (`Equipment`, 7 slots, sauvegardé avec le joueur et lâché dans le sac à la mort) : tête, corps, dos, mains, pieds et deux bouteilles d'oxygène (`EquipmentLayout`).
  - Chaque slot n'accepte que les `EquipmentData` de son emplacement, un seul à la fois (`ItemSlot.SetRestriction` : `Accepts`, `Limit`).
  - Affiché sous l'inventaire avec le nom de chaque emplacement (`PlayerInventoryContentUI`), les mêmes clics que les autres slots.
  - Shift + clic : un équipement de l'inventaire ou de la barre d'outils va dans son emplacement libre, un équipement porté revient dans l'inventaire (puis la barre d'outils).
  - `WornSlot(emplacement)` donne le slot porté.
- **Jetpack** (`PlayerMovement.ApplyJetpack`) : porté sur le dos, la touche Saut maintenue en l'air annule la gravité du joueur et pousse vers le haut jusqu'à `MaxRiseSpeed`, en vidant sa barre (`Consumption`). Vide, il ne pousse plus (notification) jusqu'à la recharge à la station.
- La consommation d'oxygène des bouteilles attend le réseau d'oxygène ([[F27 - Réseau d'oxygène|F27]], [[F51 - Système d'oxygène|F51]]).

Voir aussi : [[Données des items]] · [[Barre des objets]] · [[Inventaire du joueur]] · [[Station de recharge]]
