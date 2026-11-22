---
tags: [jeu-colonie, a-faire, refonte, ui]
id: R05
categorie: Refonte
feature: UI
statut: à faire
---

# R05 - Prefab asset modifié au runtime

`ItemCollectionUI` et `PlayerStatsUI` modifient le **prefab asset** au runtime (`slotPrefab.GetComponent<...>().parentUI = parentUI`). En éditeur, la modification persiste dans l'asset, avec une référence vers un objet de scène. Assigner `parentUI` sur l'instance après l'`Instantiate`.

Voir aussi : [[Interfaces UI Toolkit]]
