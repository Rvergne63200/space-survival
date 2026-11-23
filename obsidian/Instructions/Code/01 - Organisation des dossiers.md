---
tags: [jeu-colonie, instructions, code]
---

# Organisation des dossiers

## Scripts (`Assets/Scripts/`)

| Dossier | Contenu |
|---|---|
| `Behaviours/Animations/` | Scripts d'animation des modèles (`<Model>Animator.cs`), produits par le pipeline 3D |
| `Behaviours/Bootstrap/` | `Root`, `SceneContext`, `Bootstrapper` ; `Manager/` : `PlayerManager`, `UIManager`, `CameraManager`, `SceneLoadingManager` |
| `Behaviours/Build/` | Constructions : `Builder` est sur le joueur, ici les structures (`Dome`, `Airlock`, `Corridor`, `BuiltStructure`, `StructureBlueprint`…) et `BuildUtility` |
| `Behaviours/Camera/` | Caméra du joueur, contour de l'objet pointé |
| `Behaviours/Energy/` | Réseau électrique : appareils, câbles, tableaux, `EnergyNetworkManager` |
| `Behaviours/Entities/Player/` | Composants du joueur (déplacement, pointeur, inventaire, `Builder`, `Crafter`, `Searcher`…) |
| `Behaviours/Environment/` | Monde : `Interractable/` (objets avec lesquels on interagit), cycle jour / nuit, météo |
| `Behaviours/Scene/` | Chargement de scènes, point de spawn |
| `Behaviours/UI/<Domaine>/` | Interfaces UI Toolkit, un sous-dossier par domaine (`Inventory`, `Crafting`, `Energy`, `Map`, `Menu`, `Stat`, `Interraction`) ; `ParentUI` et `ParentedUI` à la racine |
| `Manager/` | Singletons C# statiques (pas des MonoBehaviour) : `ModeManager`, `InterfaceManager`, `PlayingMode`, `PlayerModeOptions` |
| `Save/` | `SaveManager`, `SaveableObject`, `ISaveableComponent`, `SaveData` |
| `Utilities/<Domaine>/` | Données et logique sans MonoBehaviour de scène : `Item/` (ItemData et ses sous-classes, `ItemSlot`, `ItemCollection`), `Build/` (`StructureData`, `IPlacementPreview`, aplanissement), `Crafting/`, `Effect/` (modifiers), `Stat/`, `Serialization/` (`SubclassPicker`) ; exception : `Terrain/` regroupe tout le terrain, MonoBehaviour compris (`ChunkManager`, `TerrainGenerator`, `EnvironmentSpawner`), avec la forme du relief (`TerrainShape`, `TerrainNoise`, réglages) et un paysage par fichier dans `Features/` |
| `Editor/` | Scripts d'éditeur (`ModelImportSetup`) |
| `Enum/`, `Debug/` | Énumérations partagées, outils de debug |

Un nouveau script va dans le dossier de son domaine. Un nouveau domaine prend un sous-dossier dans `Behaviours/` (et `Behaviours/UI/` pour son interface). Les noms de dossiers existants restent tels quels, même mal orthographiés (`Interractable`, `Interraction`) : les renommer casserait les références.

## Assets

| Dossier | Contenu |
|---|---|
| `Assets/Prefabs/<Domaine>/` | `Build/` (structures, connecteurs), `Environment/` (objets du monde ; `Energy/` pour les machines électriques), `Entities/`, `Camera/`, `UI/` |
| `Assets/ScriptableObjects/` | `Items/` (items du jeu), `TestItems/` (items de test `T…`), `Recipes/`, `Structures/` |
| `Assets/UI/` | `Menus/` et `Game/` : UXML et USS (`Menus.uss`, `Game.uss`), modifiables dans le UI Builder |
| `Assets/Resources/` | `Models/<Model>/` (modèles exportés, voir [[11 - Export FBX]]), matériaux, shaders et sprites chargés par le code |
| `Assets/Scenes/` | `Bootstrap`, `0_MainMenu`, `0_World`, `1_TestScene` |

Voir aussi : [[02 - Conventions de code]] · [[00 - Index Code]]
