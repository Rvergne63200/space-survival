---
tags: [jeu-colonie, fonctionnalite, energie, items]
---

# Station de recharge

`ChargingStation`, un `EnergyConsumer`, et un `ItemStorage` d'un slot, prefab `Prefabs/Environment/Energy/ChargingStation`, item `28_ChargingStation`.

- Interagir avec elle ouvre son slot, comme un conteneur. Le texte au pointeur indique la charge de l'objet déposé.
- Alimentée, elle recharge l'objet électrique (barre en Wh, voir [[Barre des objets]]) déposé avec `chargePower` (100 W). Elle ne demande cette puissance à son réseau que tant que l'objet n'est pas plein, et rien sinon.

Voir aussi : [[Appareils électriques]] · [[Lampe torche]] · [[Équipement et jetpack]]
