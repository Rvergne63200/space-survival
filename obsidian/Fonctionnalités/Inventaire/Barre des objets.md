---
tags: [jeu-colonie, fonctionnalite, inventaire, items]
---

# Barre des objets

Barre de charge, d'oxygène ou d'usure des outils et équipements (`UsableItemData.Gauge`, `ItemGauge`) : case `enabled` (décochée : pas de barre, quelle que soit la consommation), unité (`unit`) et capacité. L'unité ne change que l'affichage et la façon de remplir la barre :

- **Électricité** (`Energy`, Wh) : l'objet neuf est vide. À 0, il est inutilisable (ses actions s'éteignent, « déchargé ») jusqu'à sa recharge à la [[Station de recharge]].
- **Oxygène** (`Oxygen`, L) : l'objet neuf est vide. À 0, il est inutilisable (« vide »). Il se remplira dans un appareil à oxygène, qui reste à faire (rechargeur de bouteilles, [[F52 - Machines consommatrices d'oxygène|F52]]).
- **Usure** (`Wear`, unités) : l'objet neuf est plein et ne se recharge pas. À 0, il est détruit (« cassé »).
- **Consommation** : pour l'instant, seuls les outils vident leur barre (`ToolData`). Un outil est **actif** quand une action `Toggle` est allumée ou qu'une action `Hold` est en cours (`PlayerInventory.UpdateGauge`). Une utilisation est un clic dont l'action a été faite.
- Un objet avec une barre ne s'empile pas (`StackMax` vaut 1) : chaque slot en garde une copie (`Item.Copy` dans `ItemSlot.Add`), et la valeur suit l'objet quand il change de slot ou est jeté. Elle est sauvegardée avec les slots (`ItemSlotSaveData.gauge`) et les objets au sol (`Pickupable`).
- **Affichage** : une barre en bas du slot, bleue pour une charge, cyan clair pour l'oxygène, verte puis orange (sous 50 %) puis rouge (sous 20 %) pour une usure. L'infobulle affiche « Charge », « Oxygène » ou « Usure » avec la valeur dans son unité.
- Objets actuels : `22_Flashlight` (outil, 10 Wh, 5 W en continu quand elle est allumée, soit 2 h) et `18_Jetpack` (équipement, 50 Wh, 2000 W en vol, soit 90 s). La barre de `17_OxygenTank` est en oxygène mais désactivée, ses valeurs restent à définir.

Voir aussi : [[Données des items]] · [[Lampe torche]] · [[Équipement et jetpack]]
