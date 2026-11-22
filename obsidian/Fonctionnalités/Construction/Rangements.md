---
tags: [jeu-colonie, fonctionnalite, construction, rangements]
---

# Rangements

`StorageContainer` + `ItemStorage`, modèles `Crate_Large`, `Crate_Small` et `Cabinet` animés par leur `StorageAnimator`.

- `8_Crate` grande caisse (prefab `Crate`, 20 slots, 1,6 × 0,9 × 1 m), `31_SmallCrate` petite caisse (`SmallCrate`, 10 slots), `32_Cabinet` armoire (`Cabinet`, 30 slots, 2 m de haut).
- **Ouverture** : interagir ouvre le rangement à côté de l'inventaire et ouvre le couvercle ou les portes (`Open`) ; fermer l'interface les referme (`Close`).
- **Empilage** (caisses, `stackable`) : une caisse en main visant une caisse de même taille ou plus grande (`stackSize`) et libre dessus se cale sur son `StackPoint` (sa position ; l'orientation est celle de la caisse du dessous), au fantôme et à la pose. Une caisse qui en porte une autre ne s'ouvre pas (`StorageAnimator.CanOpen`) et ne se détruit pas avant celle du dessus. Si celle du dessous disparaît quand même, celle du dessus est posée sur ce qu'il y a dessous. Le lien (id de la caisse du dessous) est sauvegardé.

Voir aussi : [[Objets posés]] · [[Pointeur et interactables]] (`Searchable`)
