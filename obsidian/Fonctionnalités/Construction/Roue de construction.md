---
tags: [jeu-colonie, fonctionnalite, construction, interfaces]
---

# Roue de construction

`BuildWheelUI`, dans `Bootstrap`, `BuildWheel.uxml` et `BuildWheelOption.uxml`, style dans `Menus.uss`.

- On maintient la touche Build (B) pour l'afficher, avec le curseur libre (mode `Menu`). Un clic sur une option la choisit et ferme la roue ; relâcher B la ferme aussi.
- Options : « Détruire », puis les structures du `StructureCatalog` (voir [[Structures et catalogue]]). Elles sont disposées en cercle (`radius`), et le centre affiche le nom et le coût de l'option survolée.

Voir aussi : [[Pose et destruction]]
