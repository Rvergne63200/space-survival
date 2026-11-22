---
tags: [jeu-colonie, fonctionnalite, construction, batiments]
---

# Couloir

`CorridorData` : hérite de `StructureData`, longueurs min et max ; `Corridor` ; prefab `Prefabs/Build/Corridor`, asset `ScriptableObjects/Structures/Corridor`, dans le catalogue.

- **Pose** (`Builder`, mode couloir) : choisi dans la roue. En visant un premier dôme (structure finie avec un `Dome`, visé jusqu'à 30 m), le fantôme de son connecteur suit le point visé sur le mur (rouge si un autre connecteur ou la borne électrique est trop près). Un clic choisit ce dôme : le couloir part alors en rouge de ce connecteur vers le point visé (au moins un morceau droit, au plus la longueur max). En visant un autre dôme, ou en approchant le point visé à moins de 3 m de son mur, il s'y fixe : il est recalé sur la ligne entre leurs centres (un couloir droit ne peut relier deux connecteurs que là), le premier connecteur s'y déplace, le second apparaît, et tout passe au vert si la pose est valide (`Corridor.ComputePlacement` : entre les brides des connecteurs, à la hauteur du sol des dômes).
- **Validité** : bâtiments pas déjà reliés, place libre pour les deux connecteurs, longueur entre 2 et 20 m, dénivelé entre les deux bâtiments ≤ `MaxSlope` (2 m), rien sur le passage (le terrain et les deux bâtiments exceptés). Le terrain est aplani sous tout le couloir, en pente d'un dôme à l'autre.
- **Connecteurs** : fini, il crée le connecteur de chaque dôme (ils ouvrent les murs) et les retire avec lui.
- **Modèle** (`CorridorPieces`, enfant `Pieces`, le même pour le fantôme, le plan et le couloir fini) : autant de `Tunnel_Straight` (2 m) que possible bout à bout, centrés, et à chaque bout un `Tunnel_Extension` (1 m, étiré selon Z) jusqu'à la bride du connecteur. Chaque extension fait entre 0,25 et 0,75 morceau droit (0,5 à 1,5 m, `MaxExtension` 0,75) : s'il reste moins de chaque côté, un morceau droit de moins et des extensions plus longues (une extension courte se voit). Pièces et longueurs sur l'asset `CorridorData`. Le prefab `Corridor` est une racine vide (pivot au centre du sol). Fini : un `MeshCollider` sur chaque mesh. Plan : matériau du plan, et son collider autour des morceaux.
- **Pente** : entre deux dômes de hauteurs différentes, le couloir n'est pas incliné mais cisaillé : il reste à plat (rotation horizontale) et chaque sommet de chaque morceau monte de pente × sa position le long du couloir (copies des meshes, détruites avec le couloir, `OwnedMeshes` ; Read/Write activé sur `Tunnel_Straight` et `Tunnel_Extension` par `ModelImportSetup`). Les sections restent verticales : les bouts épousent les brides verticales des connecteurs, et le sol, le toit et les côtés sont des lignes droites d'un dôme à l'autre. Longueur et pente sont horizontales (`Corridor.Length`, `Corridor.Slope`, sauvegardées) ; les morceaux sont un peu plus longs le long de la pente. Le masque du terrain et les collisions suivent le cisaillement.
- **Coût et remboursement par mètre** (arrondi au-dessus) : 2 fer et 1 bois par mètre, rend 1 fer par mètre (`CostMultiplier` sur `StructureBlueprint` et `BuiltStructure`).
- **Sauvegarde** : comme les autres structures (`build:Corridor`, `blueprint:Corridor`), avec les ids de ses deux bâtiments et sa longueur (composant `Corridor`).
- **Destruction** : directement, ou avec un de ses deux bâtiments (plan compris) : il rend ses ressources.
- **Électricité** : un couloir relie les bâtiments sans câble, voir [[Électricité des bâtiments]].

Voir aussi : [[Dômes]] · [[Connecteurs de dôme]] · [[Terrain sous les constructions]]
