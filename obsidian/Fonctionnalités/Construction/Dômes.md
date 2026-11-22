---
tags: [jeu-colonie, fonctionnalite, construction, batiments]
---

# Dômes

Prefabs `Prefabs/Build/Dome_8m`, `Dome_12m`, `Dome_16m`, modèles `Resources/Models/Dome_<taille>`, assets `ScriptableObjects/Structures/Dome_<taille>`, dans le catalogue.

- Rayon 4, 6 et 8 m. Coût 10 fer + 15 bois, 20 + 30, 35 + 50 ; remboursement 5, 10 et 17 fer.
- Chacun a son intérieur (`Interior`, voir [[Intérieur des bâtiments]]), sa prise électrique (`PowerSocket`, juste dehors, à +Z, voir [[Électricité des bâtiments]]), sa borne intérieure (`PanelTerminal`) et le composant `Dome` :
  - `radius` (mur extérieur) et `connectorPrefab` (le connecteur de sa taille, `Prefabs/Build/Connectors/DomeConnector_<taille>`, voir [[Connecteurs de dôme]]).
  - **Matériaux masqués** : à l'Awake, les matériaux du modèle sont remplacés par des copies (une par matériau, partagée) avec les shaders `SpaceSurvival/Masked Standard` et `Masked Transparent` (`Resources/Shaders`), qui ne dessinent pas ce qui est dans le masque d'un connecteur (ombres comprises). Les assets des modèles ne sont pas modifiés.
  - **Colliders** : un `MeshCollider` par mesh du modèle, découpé par les masques de ses connecteurs (`MeshCutter` : triangles coupés plan par plan, la partie dans la boîte est retirée) à chaque ajout ou retrait de connecteur.
- L'ancien dôme (`BaseDom`) est supprimé : les sauvegardes qui en contiennent les perdent.

Voir aussi : [[Structures et catalogue]] · [[Terrain sous les constructions]] · [[Sas]] · [[Couloir]]
