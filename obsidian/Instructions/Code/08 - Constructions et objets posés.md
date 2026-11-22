---
tags: [jeu-colonie, instructions, code, construction]
---

# Constructions et objets posés

Le fonctionnement est décrit dans [[Pose et destruction]], [[Structures et catalogue]] et [[Objets posés]]. Deux façons d'ajouter quelque chose de constructible :

## Structure (bâtiment, depuis la roue)
- Un asset `StructureData` (Create > Construction > Structure) dans `ScriptableObjects/Structures` : id **unique et stable** (il sert de clé de sauvegarde), nom, icône, prefab, coût, remboursement, aplanissement, pente max. Une structure à pose particulière a sa sous-classe (`AirlockData`, `CorridorData`).
- Ajoute-le au `StructureCatalog` (roue de construction et sauvegarde).
- Prefab dans `Prefabs/Build/` : le modèle et le composant de la structure. À l'instanciation (`BuildUtility.InstantiateStructure`), ses parties passent sur le layer `Building`.
- Un bâtiment habitable a un enfant `Interior` (`BuildingInterior`) et le tableau intégré (`PowerSocket`, voir [[Électricité des bâtiments]]).
- Si la pose est particulière (accroche à un dôme…), elle a son mode dans le `Builder` : réutilise `DomeConnection` pour tout ce qui se fixe au mur d'un dôme.

## Objet posé (depuis l'inventaire)
- Un item `PlaceableItemData` avec son `placedPrefab` (voir [[05 - Ajouter un item]]), et si besoin `OutdoorOnly` / `NotInCorridors`, et son `Refund`.
- Instancie-le toujours par `BuildUtility.InstantiatePlaced` (pose du `PlacedObject` et de la clé de sauvegarde).
- Si son fantôme doit montrer l'objet tel qu'il sera posé (pièce réglée selon l'endroit), implémente `IPlacementPreview` sur la racine du prefab : `PreviewModel` (enfant copié comme fantôme) et `UpdatePreview(ghost)`, appelé à chaque frame.

## Règles communes
- Les aides partagées sont dans `BuildUtility` (`SetMaterial`, `LocalRendererBounds`, `FindChild`, `AddMeshColliders`, `DeconstructStructure`…) : ne les recopie pas dans un composant.
- Un objet détruit rend ses ressources et le contenu de ses conteneurs : passe par les chemins de destruction existants.
- Les colliders de modèles non convexes sont des `MeshCollider` (un par mesh, `AddMeshColliders`) ; les distances d'interaction en tiennent compte (`InteractionDistance`).

Voir aussi : [[04 - Rendre un objet sauvegardable]] · [[Terrain sous les constructions]]
