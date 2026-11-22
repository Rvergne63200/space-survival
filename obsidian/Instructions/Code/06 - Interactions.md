---
tags: [jeu-colonie, instructions, code, interaction]
---

# Interactions

Le système est décrit dans [[Pointeur et interactables]]. Pour rendre un objet interactif :

- Implémente `IInterractable` (orthographe du projet) sur un composant de l'objet :
  - `Interract(GameObject interractor)` : ce que fait la touche Interagir ;
  - `GetInfo()` : le texte affiché au-dessus du réticule, en français (« Ouvrir la porte »). Il est relu à chaque frame : il peut refléter l'état courant.
- Le pointeur trouve l'`IInterractable` dans les **parents** du collider touché : l'objet a besoin d'un collider (non trigger) sur lui ou sur un enfant. Les triggers et le layer du joueur sont ignorés.
- Portée de visée : 3 m (`PlayerPointer.range`). Pour une interface qui doit se fermer quand le joueur s'éloigne, mesure la distance avec `InteractionDistance.To(composant, position)` (jusqu'aux colliders de l'objet, compatible avec les MeshCollider non convexes), comme `Searcher` et `Crafter` (4,5 m).
- **Conteneur** : `Searchable` + `ItemStorage`. Une machine dont l'interface est son conteneur implémente aussi `IStorageMachine` (nom, état, jauge, bouton Allumer / Éteindre), comme le générateur à charbon.
- **Appareil électrique** : `EnergyDeviceInfo` affiche l'état et allume / éteint ; ne le refais pas à la main (voir [[09 - Ajouter un appareil électrique]]).
- Une **porte ou pièce mobile** de modèle : l'interaction appelle l'API du `<Model>Animator` (`Toggle()`, `Open()`…), jamais un mouvement codé dans le composant d'interaction.
- Le système doit devenir plus extensible : voir [[R09 - Système d'interaction extensible|R09]] avant d'ajouter un nouveau type d'interaction complexe (maintenue, conditionnelle…).

Voir aussi : [[07 - Créer une interface]]
