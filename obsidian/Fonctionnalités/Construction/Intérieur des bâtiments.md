---
tags: [jeu-colonie, fonctionnalite, construction, batiments]
---

# Intérieur des bâtiments

`BuildingInterior`, enfant `Interior` des prefabs de dôme.

- L'intérieur du bâtiment : un collider trigger (sphère du rayon du dôme) sur le layer `Ignore Raycast` pour ne gêner ni la visée ni la pose.
- `Contains(point)` dit si un point est dedans.
- Sert à l'électricité du bâtiment ([[Électricité des bâtiments]]) et à la pose des couloirs (bord du bâtiment), plus tard à l'oxygène et au joueur à l'intérieur ([[F51 - Système d'oxygène|F51]], [[F55 - Informations des réseaux|F55]]).
- Le dôme lui-même est désactivé pour pouvoir entrer, en attendant un vrai modèle.

Voir aussi : [[Dômes]] · [[Couloir]]
