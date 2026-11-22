---
tags: [jeu-colonie, fonctionnalite, construction, items]
---

# Objets posés

**PlaceableItemData** (ItemData, Item/Placeable) : `placedPrefab` et `Refund`, rendu à la destruction (vide : l'item lui-même).

- `OutdoorOnly` : se pose uniquement dehors, pas dans un dôme, un sas ou un couloir (fantôme rouge et « Se pose uniquement dehors » dans le bandeau ; panneau solaire, plus tard l'éolienne).
- `NotInCorridors` : pas dans un couloir ni un sas, trop bas (générateur à charbon, « Ne rentre pas dans un couloir »).
- Le contenu des conteneurs d'un objet détruit va aussi dans l'inventaire.
- Objets actuels : les [[Rangements]], la [[Station de craft]], les machines et générateurs (voir [[Objets du réseau électrique]]).
- Sauvegarde : `placed:<itemId>`, avec le contenu de leurs conteneurs (voir [[Structures et catalogue]]).

Voir aussi : [[Pose et destruction]] · [[Données des items]]
