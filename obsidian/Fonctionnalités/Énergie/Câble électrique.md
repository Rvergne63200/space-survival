---
tags: [jeu-colonie, fonctionnalite, energie]
---

# Câble électrique

## ElectricCable
Objet « runtime » sauvegardé (`cable:<id de l'item>`), avec les ids de ses deux appareils, ses points de passage et le nombre de câbles utilisés.
- **Tracé** : une courbe de Catmull-Rom qui pend légèrement entre deux points (`LineRenderer`).
- **Ciblage** : des colliders triggers le long du câble, visés par le mode destruction (`Builder`). Ils ne gênent pas le joueur.
- **Destruction** : il rend ses câbles au joueur.

## CablePlacer
Sur le joueur, actif avec un `CableItemData` en main (`Item/Cable` : mètres par item, largeur, matériau) :
- **Clic gauche** : sur un appareil, il commence le câble. Ailleurs, il ajoute un point de passage. Sur un 2e appareil, il termine le câble.
- **Clic droit** : il retire le dernier point, ou annule le câble.
- **Aperçu** : vert si le câble peut être terminé sur l'appareil visé, rouge s'il manque des câbles. Le bandeau indique le nombre de câbles nécessaires et possédés.
- Deux appareils ne peuvent être reliés qu'une fois.
- Tableaux et bâtiments : le câble se branche sur le port le plus proche de la visée ; deux ports d'un même bâtiment ne se relient pas, ni les entrées de deux bâtiments (voir [[Tableau de distribution]], [[Électricité des bâtiments]]).

Voir aussi : [[Appareils électriques]] · [[Réseau électrique]]
