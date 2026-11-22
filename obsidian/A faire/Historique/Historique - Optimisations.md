---
tags: [jeu-colonie, a-faire, historique, optimisation]
---

# Historique - Optimisations

Tâches de la catégorie Optimisation terminées (ou abandonnées), reprises telles quelles de l'ancien TODO.md. Retour à [[00 - Index À faire]].

- [x] **O01** **[Terrain]** Les chunks n'étaient jamais détruits, donc la mémoire augmentait sans limite en explorant. Les chunks au-delà de `generationDistance + destroyMargin` (2) sont maintenant détruits, avec leur `TerrainData`, quand le joueur change de chunk. Les plus proches sont seulement désactivés, pour ne pas les régénérer en faisant des allers-retours près d'une frontière. Pas besoin de les écrire sur disque : un chunk détruit est régénéré à l'identique (relief tiré de la seed, aplanissements rejoués), et l'état de ses objets est conservé par le `SaveManager` (`UnloadObjects`). Les objets hors chunk (objets jetés) avec un Rigidbody sont figés jusqu'à la régénération du terrain sous eux. ^O01
- [x] **O02** **[Terrain]** `ChunkManager.Update` ne recalcule plus les chunks nécessaires à chaque frame, seulement quand le joueur change de chunk (`lastPlayerChunk`). ^O02
- [x] **O04** **[Inventaire]** `ItemCollectionUI.UpdateItemCollectionUI` réutilise les slots UI existants au lieu de tous les détruire et recréer à chaque `ev_Update`. Il n'en crée ou n'en supprime que pour suivre la taille de la collection. ^O04
  - [x] `InventorySlotUI.setSlot` ne se réabonne à `ItemSlot.ev_update` que si le slot change (sinon, doublons de listeners à chaque mise à jour).
  - [x] Un slot `null` garde son UI (affichée vide), donc l'index UI correspond toujours à l'index du slot. C'est ce qu'utilise `HandedCursorUI`.
- [x] **O05** **[Interaction]** Le `PlayerContext` est mis en cache dans `Awake` au lieu d'un `GetComponent` à chaque accès : `PlayerPointer`, `PlayerInterractor` et `Builder`, ainsi que `Crafter` et `Pickuper`, qui faisaient de même. ^O05
