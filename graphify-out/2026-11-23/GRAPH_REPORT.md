# Graph Report - Space Survival  (2026-11-23)

## Corpus Check
- 366 files · ~172,194 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1499 file(s) not represented in the graph (top: .meta 930, .mat 186, .asset 90)
- WARNING: 1499 of 1944 files seen (77%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 3499 nodes · 8681 edges · 214 communities (148 shown, 66 thin omitted)
- Extraction: 85% EXTRACTED · 15% INFERRED · 0% AMBIGUOUS · INFERRED: 1280 edges (avg confidence: 0.89)
- Token cost: 497,038 input · 0 output

## Graph Freshness
- Built from commit: `8a76b4fe`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- 00 - Index À faire.md
- PlayerInventory
- Coal Generator Animator
- MonoBehaviour
- Builder Placement
- GameObject
- ItemSlot
- Vector3
- BuiltStructure
- Item
- Distribution Panel Core
- InteractableOutline
- Save System
- ISaveableComponent
- add_revolve
- Energy Network Manager
- Network Map UI
- system_collections
- oct_cyl
- Landing Capsule Blender Build
- Package Lock A
- Terrain procédural.md
- Interfaces UI Toolkit.md
- Map Utility & Minimap
- Package Lock B
- Package Lock C
- Tunnel Blender Build
- Electric Cables
- Package Lock D
- Input Actions
- Searcher
- ModelImportSetup
- IInterractable.GetInfo
- EnergyDevice
- ItemData
- Terrain Chunks
- CallbackContext
- Airlock Animator
- Distribution Panel UI
- st_build
- TextMeshProFloatingText
- Airlock Blender Build
- TMP Text Event Handler
- UI Manager
- EnergyDevice
- PlayingMode
- cp_build.py
- CraftingStationAnimator
- PlayerInputActions
- TMP Text Selectors
- SolarPanelAnimator
- Main Menu UI
- 00 - Index Pipeline 3D.md
- st_build (2)
- DrillAnimator
- SaveableObject
- CraftingStationUI
- dependencies
- bmesh
- TMP_UiFrameRateCounter
- TMP_FrameRateCounter
- PlayerContext
- DayNightCycle
- CameraMovement
- Pose et destruction.md
- BuildingPowerPanel
- PlayerMovement
- WeatherSystem
- ItemCollectionUI
- ws_build.py
- PlayerModeOptions
- RelayAntennaAnimator
- PlayerManager
- TerrainMask
- ParentedUI
- packages-lock (2)
- UnityEventRuntimeDebugger
- TMP_TextInfoDebugTool
- HeldItemUI
- WindTurbineAnimator
- CraftingStation
- cs_anim
- dr_build
- VertexJitter
- PlayerInputActions (3)
- ControlPanelUI
- EnergyConsumer
- StorageContainer
- ItemStorage
- Stat (2)
- 00 - Index DA.md
- packages-lock (3)
- packages-lock (4)
- packages-lock (5)
- SkewTextExample
- ControlPanelAnimator
- DeathScreenUI
- PlaceableItemData
- StructureData
- Historique - Refontes
- TerrainEditSaveData
- 00 - Index Fonctionnalités.md
- SubclassPicker
- PlayerInputActions (4)
- Déplacement du joueur.md
- EnergyStorage
- StatBarUI
- com.unity.performance.profile-analyzer@1.3.3
- packages-lock (7)
- InterfaceManager
- LandingCapsuleAnimator
- Intention et piliers du style
- EnergyValue
- Pickupable
- Technologie humaine
- wt_build
- TMPro
- FlashlightBeam
- StorageAnimator
- Label
- Fonctionnalités implémentées
- WallScreenAnimator
- com.unity.modules.adaptiveperformance@1.0.0
- dependencies
- packages-lock (8)
- al_anim
- EnergyGenerator
- StatName
- F65 - Intégration des modèles 3D
- PickupInfoUI
- ObjectSpin
- dependencies
- packages-lock (11)
- zfight (2)
- CameraController
- TerrainGenerator
- ToolData.cs
- NotificationUI
- PauseMenuUI
- À faire
- SaveData.cs
- Couleur et lumière
- À éviter et check-list
- Prompts IA
- Bible de direction artistique : [NOM DU JEU]
- DistributionPanel
- com.unity.settings-manager
- Menaces et lisibilité
- 14 - Production Unity.md
- PlayerInventory
- F54 - Cycle jour nuit et météo.md
- com.unity.modules.ai
- BenchmarkType
- com.unity.modules.androidjni
- com.coplaydev.unity-mcp@https://github.com/CoplayDev/unity-mc...
- com.unity.collab-proxy@2.12.4
- EnergyNetworkManager
- ItemGaugeUnit
- com.unity.modules.accessibility@1.0.0
- packages-lock (14)
- com.unity.modules.assetbundle@1.0.0
- Assembly-CSharp
- Direction
- com.unity.modules.imageconversion@1.0.0
- com.unity.modules.ui@1.0.0
- com.unity.modules.unitywebrequest@1.0.0
- manifest (3)
- manifest (4)
- manifest (5)
- com.unity.modules.unitywebrequestaudio@1.0.0
- manifest.json
- manifest (8)
- com.unity.modules.vr@1.0.0
- manifest (10)
- manifest (11)
- manifest (12)
- com.unity.timeline@1.8.12
- manifest (14)
- manifest (15)
- com.unity.modules.screencapture@1.0.0
- manifest (17)
- manifest (18)
- TMPro_InstructionOverlay
- manifest (20)
- IStorageMachine
- manifest (22)
- DeathBag
- manifest (24)
- manifest (25)
- PointedInterractorInformationUI
- manifest (27)
- manifest (28)
- InterfaceManager
- manifest (30)
- manifest (31)
- manifest (32)
- VertexShakeB
- VertexZoom
- Objets persistants
- Sommaire
- ChatController
- TMP_ExampleScript_01
- FpsCounterAnchorPositions
- CLAUDE

## God Nodes (most connected - your core abstractions)
1. `Builder` - 87 edges
2. `DistributionPanel` - 78 edges
3. `EnergyDevice` - 77 edges
4. `SaveManager` - 70 edges
5. `PlayerInventory` - 67 edges
6. `EnergyNetworkManager` - 58 edges
7. `Historique - Features` - 53 edges
8. `Item` - 52 edges
9. `CoalGeneratorAnimator` - 51 edges
10. `WeatherSystem` - 48 edges

## Surprising Connections (you probably didn't know these)
- `Inventaire du joueur` --references--> `PlayerInventory`  [INFERRED]
  obsidian/Fonctionnalités/Inventaire/Inventaire du joueur.md → Assets/Scripts/Behaviours/Entities/Player/PlayerInventory.cs
- `Textures` --references--> `ModelImportSetup`  [INFERRED]
  obsidian/Instructions/Pipeline 3D/09 - Textures.md → Assets/Scripts/Editor/ModelImportSetup.cs
- `DistributionPanelUI` --references--> `DistributionPanelSettingsView`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Tableau de distribution.md → Assets/Scripts/Behaviours/UI/Energy/DistributionPanelSettingsView.cs
- `ElectricCable` --references--> `Builder`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Câble électrique.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs
- `Modélisation Blender` --references--> `SolarPanel`  [INFERRED]
  obsidian/Instructions/Pipeline 3D/10 - Modélisation Blender.md → Assets/Scripts/Behaviours/Energy/SolarPanel.cs

## Import Cycles
- None detected.

## Communities (214 total, 66 thin omitted)

### Community 0 - "00 - Index À faire.md"
Cohesion: 0.10
Nodes (13): B26 - Flag identified des objets ramassables, F06 - Vrai glisser-déposer, F20 - Recettes à plusieurs résultats, F27 - Réseau d'oxygène, F38 - Position du sac de mort, F50 - Réseau électrique complet, F51 - Système d'oxygène, F52 - Machines consommatrices d'oxygène (+5 more)

### Community 2 - "PlayerInventory"
Cohesion: 0.09
Nodes (29): SubclassPicker, HeldAction, AbstractModifier, EnduranceModifier, LifeModifier, StatModifier, ConsummableData, ApplyModifiersAction (+21 more)

### Community 3 - "Coal Generator Animator"
Cohesion: 0.07
Nodes (20): CoalGeneratorAnimator, ChimneyHeight, CollarRotation, IsBusy, IsHatchOpen, IsRunning, MinChimneyHeight, SmokePoint (+12 more)

### Community 4 - "MonoBehaviour"
Cohesion: 0.10
Nodes (6): EnvMapAnimator, TeleType, TextConsoleSimulator, TMP_TextEventCheck, VertexColorCycler, Bootstrapper

### Community 5 - "Builder Placement"
Cohesion: 0.08
Nodes (13): Builder, CorridorStructure, Inventory, IsActive, Mode, Airlock, Building, Corridor (+5 more)

### Community 6 - "GameObject"
Cohesion: 0.11
Nodes (8): ShaderPropAnimator, BuildUtility, CorridorPieces, Piece, Historique - Refontes, Structures et catalogue, L'objet, Règles communes

### Community 7 - "ItemSlot"
Cohesion: 0.15
Nodes (8): ItemCollection, MaxSize, Slots, ItemSlot, ev_update, Item, Quantity, Historique - Bugs

### Community 8 - "Vector3"
Cohesion: 0.11
Nodes (11): BlueprintMaterial, Dome, ConnectorPrefab, Connectors, Model, Radius, DomeConnection, DomeConnector (+3 more)

### Community 9 - "BuiltStructure"
Cohesion: 0.06
Nodes (28): Airlock, All, DoorsBusy, IsBuilt, SaveKey, State, BuiltStructure, CostMultiplier (+20 more)

### Community 10 - "Item"
Cohesion: 0.13
Nodes (10): ItemSlotUI, Element, Item, Data, Gauge, GaugeRatio, GaugeSettings, HasGauge (+2 more)

### Community 11 - "Distribution Panel Core"
Cohesion: 0.04
Nodes (47): DistributionPanel, BatteryChargeSupplied, BatteryThreshold, Building, ChargeRequest, ChargeRoom, ChargeSupplied, ConsumedEnergy (+39 more)

### Community 12 - "InteractableOutline"
Cohesion: 0.13
Nodes (4): CameraContext, CameraManager, InteractableOutline, Contour de l'objet pointé

### Community 13 - "Save System"
Cohesion: 0.10
Nodes (10): SaveManager, HasSave, Instance, IsUnloading, LiveObjects, SavedTerrainEdits, SavesFolder, WorldLoaded (+2 more)

### Community 14 - "ISaveableComponent"
Cohesion: 0.24
Nodes (3): EnvironmentSpawner, F31 - Structures générées, Objets du décor

### Community 15 - "add_revolve"
Cohesion: 0.12
Nodes (29): add_box(), add_cyl(), add_revolve(), add_screw(), _copy_into(), finish(), zone_mats(), build_holo() (+21 more)

### Community 16 - "Energy Network Manager"
Cohesion: 0.16
Nodes (3): EnergyNetworkManager, HasInstance, Instance

### Community 19 - "oct_cyl"
Cohesion: 0.14
Nodes (25): beam(), pill(), animate(), arm_k(), mast_k(), build_arm(), build_base(), build_dish() (+17 more)

### Community 20 - "Landing Capsule Blender Build"
Cohesion: 0.13
Nodes (24): animate(), build_body(), build_door(), build_door_frames(), build_exterior(), q(), build_interior(), q() (+16 more)

### Community 22 - "Package Lock A"
Cohesion: 0.08
Nodes (28): dependencies, depth, source, version, dependencies, depth, source, version (+20 more)

### Community 23 - "Terrain procédural.md"
Cohesion: 0.14
Nodes (5): B10 - Pivot de ReplaceObject trop haut, F30 - Biomes, F32 - Ressources générées, O03 - Monde infini, R15 - Ids uniques des objets de scène

### Community 24 - "Interfaces UI Toolkit.md"
Cohesion: 0.11
Nodes (11): R01 - Refonte du chargement de scènes, R02 - Persistance entre scènes, R09 - Système d'interaction extensible, Domaines, Instructions, Règles communes, Code Unity, Point ouvert (+3 more)

### Community 25 - "Map Utility & Minimap"
Cohesion: 0.16
Nodes (4): MapUtility, ContourSegment, MinimapUI, Minimap

### Community 26 - "Package Lock B"
Cohesion: 0.07
Nodes (30): dependencies, depth, source, version, dependencies, depth, source, version (+22 more)

### Community 27 - "Package Lock C"
Cohesion: 0.07
Nodes (31): dependencies, depth, source, url, version, dependencies, depth, source (+23 more)

### Community 28 - "Tunnel Blender Build"
Cohesion: 0.18
Nodes (24): add_body(), add_flange(), add_rib(), f_dir(), glass_poly(), hollow_sweep(), pane(), path_frame() (+16 more)

### Community 29 - "Electric Cables"
Cohesion: 0.10
Nodes (12): ElectricCable, End, SaveKey, Start, State, CableItemData, Material, MetersPerItem (+4 more)

### Community 30 - "Package Lock D"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, version, dependencies, depth, source, version (+26 more)

### Community 31 - "Input Actions"
Cohesion: 0.08
Nodes (8): @PlayerInputActions, asset, bindingMask, bindings, controlSchemes, devices, ICameraActions, R06 - PlayerInputActions jamais libérés

### Community 32 - "Searcher"
Cohesion: 0.15
Nodes (6): AirlockDoor, Airlock, Outer, Searcher, CurrentSearchedStorage, Pointeur et interactables

### Community 35 - "EnergyDevice"
Cohesion: 0.19
Nodes (3): CablePlacer, Inventory, Source

### Community 36 - "ItemData"
Cohesion: 0.15
Nodes (7): ItemData, Description, Id, Model, Name, Sprite, StackMax

### Community 37 - "Terrain Chunks"
Cohesion: 0.19
Nodes (4): ChunkManager, Instance, Seed, TerrainEdits

### Community 39 - "Airlock Animator"
Cohesion: 0.19
Nodes (5): AirlockAnimator, InnerOpen, IsBusy, OuterOpen, Pressurized

### Community 40 - "Distribution Panel UI"
Cohesion: 0.13
Nodes (4): DistributionPanelSettingsView, Target, DistributionPanelUI, Instance

### Community 41 - "st_build"
Cohesion: 0.17
Nodes (18): animate(), build_body(), build_chimney_base(), build_collar(), build_fan(), build_hatch(), build_head(), build_hopper() (+10 more)

### Community 42 - "TextMeshProFloatingText"
Cohesion: 0.16
Nodes (6): Benchmark01, Benchmark02, Benchmark04, SimpleScript, TextMeshProFloatingText, TextMeshSpawner

### Community 44 - "Airlock Blender Build"
Cohesion: 0.16
Nodes (17): build_airlock_body(), build_connector(), build_fan(), build_leaf(), densify(), dome_surface_y(), door_leaf_poly(), finish2() (+9 more)

### Community 46 - "TMP Text Event Handler"
Cohesion: 0.12
Nodes (11): TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, onCharacterSelection, onLineSelection, onLinkSelection, onSpriteSelection (+3 more)

### Community 47 - "UI Manager"
Cohesion: 0.11
Nodes (15): UIManager, ev_updatePlayerContext, Instance, PlayerContext, PlayerInventory, PlayerStats, UI, ParentUI (+7 more)

### Community 48 - "EnergyDevice"
Cohesion: 0.11
Nodes (16): DisplayName, EnergyDevice, Cables, CanSwitch, ConnectionPoint, DeviceState, DisplayName, ForcedOff (+8 more)

### Community 49 - "PlayingMode"
Cohesion: 0.11
Nodes (15): ModeManager, Instance, Mode, PlayingMode, Build, Default, Interface, Menu (+7 more)

### Community 50 - "cp_build.py"
Cohesion: 0.14
Nodes (15): animate(), build_body(), build_hologram(), sbox(), sp(), build_projector(), cham(), clear() (+7 more)

### Community 51 - "CraftingStationAnimator"
Cohesion: 0.21
Nodes (4): CraftingStationAnimator, HologramScreen, IsPowered, IsPrinting

### Community 52 - "PlayerInputActions"
Cohesion: 0.14
Nodes (11): @Actions, ActionsActions, @Build, @ChangeHandedItem, @CloseInterfaces, @Drop, enabled, @Interract (+3 more)

### Community 54 - "SolarPanelAnimator"
Cohesion: 0.07
Nodes (14): SolarPanelAnimator, DeployProgress, HeadingAngle, IsDeployed, IsRotating, PanelFront, PanelUp, BuildingInterior (+6 more)

### Community 55 - "Main Menu UI"
Cohesion: 0.16
Nodes (3): MainMenuUI, SaveSlotInfo, Menu principal

### Community 56 - "00 - Index Pipeline 3D.md"
Cohesion: 0.15
Nodes (8): Façon de travailler, Palette des matériaux, Dimensions de référence, Vitres, Textures, Import Unity, Animations Blender, Animator Unity

### Community 57 - "st_build (2)"
Cohesion: 0.19
Nodes (16): _bevel_tmp(), cbox(), build(), cabinet_body(), cabinet_doors(), cham(), clear(), collisions() (+8 more)

### Community 58 - "DrillAnimator"
Cohesion: 0.20
Nodes (5): DrillAnimator, DustPoint, IsBusy, IsDrilling, Powered

### Community 59 - "SaveableObject"
Cohesion: 0.12
Nodes (9): SaveId, SaveableObject, Id, IsRegistered, IsRuntime, PrefabKey, ChunkManager (singleton, scène Monde), Terrain procédural (+1 more)

### Community 60 - "CraftingStationUI"
Cohesion: 0.19
Nodes (4): CraftingStationUI, RecipeSlotUI, Element, Recipe

### Community 61 - "dependencies"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, url (+18 more)

### Community 62 - "bmesh"
Cohesion: 0.22
Nodes (8): build_all(), build_size(), check(), check2(), faces_of(), islands_of(), overlap_area(), Erreurs à ne pas refaire

### Community 63 - "TMP_UiFrameRateCounter"
Cohesion: 0.17
Nodes (3): Benchmark01_UGUI, DropdownSample, TMP_UiFrameRateCounter

### Community 64 - "TMP_FrameRateCounter"
Cohesion: 0.24
Nodes (6): TMP_FrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

### Community 65 - "PlayerContext"
Cohesion: 0.12
Nodes (9): PlayerContext, Interractable, PlayerManager, PointedObject, PlayerInterractor, PlayerPointer, Source, IInterractable (+1 more)

### Community 66 - "DayNightCycle"
Cohesion: 0.11
Nodes (13): DayNightCycle, Day, Hour, Instance, IsDay, LightLevel, NightBrightness, NoonSunHeight (+5 more)

### Community 67 - "CameraMovement"
Cohesion: 0.18
Nodes (5): CameraMovement, Pitch, Target, Yaw, Caméra

### Community 68 - "Pose et destruction.md"
Cohesion: 0.18
Nodes (5): F47 - Relais radio, R12 - Intégration des constructions au terrain, Objets posés, Destruction, Pose et destruction

### Community 69 - "BuildingPowerPanel"
Cohesion: 0.10
Nodes (16): BuildingOutlet, Building, OptionalDemand, SaveKey, BuildingPanelTerminal, Panel, BuildingPowerPanel, Building (+8 more)

### Community 70 - "PlayerMovement"
Cohesion: 0.12
Nodes (5): PlayerMovement, forwardDirection, IsSprinting, Stats, Déplacement du joueur

### Community 71 - "WeatherSystem"
Cohesion: 0.07
Nodes (19): WeatherSystem, AmbientFactor, Instance, IsStorm, IsStormComing, Seed, SkyFactor, SolarFactor (+11 more)

### Community 72 - "ItemCollectionUI"
Cohesion: 0.14
Nodes (6): ItemCollectionUI, SlotsUI, ItemStorageUI, PlayerInventoryContentUI, PlayerToolBarUI, Clics de l'inventaire

### Community 73 - "ws_build.py"
Cohesion: 0.18
Nodes (12): arc_pt(), build_body(), build_screen(), clear(), rect(), rho(), screen_uv(), setup() (+4 more)

### Community 74 - "PlayerModeOptions"
Cohesion: 0.22
Nodes (8): PlayerModeOptions, Options, Commentaires, Composants, Conventions de code, Gameplay et modèles, Nommage, Robustesse

### Community 75 - "RelayAntennaAnimator"
Cohesion: 0.20
Nodes (4): RelayAntennaAnimator, IsAntennaExtended, IsDishDeployed, IsRotating

### Community 76 - "PlayerManager"
Cohesion: 0.11
Nodes (12): CameraManager, Camera, Instance, Target, PlayerManager, Instance, Inventory, IsDead (+4 more)

### Community 77 - "TerrainMask"
Cohesion: 0.12
Nodes (9): TerrainMask, Matrix, Shape, Box, Cylinder, MeshCutter, OwnedMeshes, Dômes (+1 more)

### Community 79 - "packages-lock (2)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 83 - "WindTurbineAnimator"
Cohesion: 0.18
Nodes (4): WindTurbineAnimator, IsDeployed, WindStrength, Scripts d'animation C# (runtime)

### Community 85 - "CraftingStation"
Cohesion: 0.09
Nodes (19): Crafter, CraftingStation, Current, IsUsable, Output, Power, Progress, Queue (+11 more)

### Community 86 - "cs_anim"
Cohesion: 0.33
Nodes (13): build_anim(), clear(), head_clearance(), ik(), k_arm(), k_head(), k_laser(), k_plate() (+5 more)

### Community 87 - "dr_build"
Cohesion: 0.19
Nodes (14): animate(), build_body(), build_carriage(), build_head(), build_hologram(), sbox(), sp(), collisions() (+6 more)

### Community 88 - "VertexJitter"
Cohesion: 0.12
Nodes (3): VertexJitter, VertexAnim, VertexShakeA

### Community 89 - "PlayerInputActions (3)"
Cohesion: 0.22
Nodes (6): @Movement, MovementActions, enabled, @Jump, @Move, @Sprint

### Community 90 - "ControlPanelUI"
Cohesion: 0.13
Nodes (12): ControlPanel, Consumer, DisplayName, IsPowered, ControlPanelUI, Instance, Carte, ControlPanelUI (+4 more)

### Community 91 - "EnergyConsumer"
Cohesion: 0.12
Nodes (9): EnergyConsumer, Consumption, Demand, IsPowered, State, TestMachine, Items du réseau, Objets de test (+1 more)

### Community 92 - "StorageContainer"
Cohesion: 0.15
Nodes (9): StorageContainer, Above, Below, SaveKey, Stackable, StackPoint, StackSize, State (+1 more)

### Community 93 - "ItemStorage"
Cohesion: 0.16
Nodes (10): ChargingStation, Consumption, Storage, Searchable, ItemStorage, Content, SaveKey, Rangements (+2 more)

### Community 94 - "Stat (2)"
Cohesion: 0.12
Nodes (10): PlayerStats, SerializableKeyValuePair, Key, Value, Stat, MaxValue, Value, StatMarker (+2 more)

### Community 95 - "00 - Index DA.md"
Cohesion: 0.17
Nodes (9): Fiche du jeu, Formes et silhouettes, Matériaux et textures, Environnement, Traces de vie ancienne, Colons, Direction artistique, La chaleur qui s'installe (+1 more)

### Community 96 - "packages-lock (3)"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, url, version, depth, source, version (+13 more)

### Community 97 - "packages-lock (4)"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 98 - "packages-lock (5)"
Cohesion: 0.12
Nodes (17): dependencies, depth, source, version, dependencies, depth, source, version (+9 more)

### Community 100 - "ControlPanelAnimator"
Cohesion: 0.23
Nodes (4): ControlPanelAnimator, HologramScreen, IsBusy, IsPowered

### Community 101 - "DeathScreenUI"
Cohesion: 0.08
Nodes (11): SceneLoadingManager, PlayerSpawnPoint, SceneLoader, DeathScreenUI, Instance, IsOpen, Fonctionnement actuel, Scènes (+3 more)

### Community 102 - "PlaceableItemData"
Cohesion: 0.15
Nodes (13): PlacedObject, Data, BuildModeUI, Instance, IPlacementPreview, PreviewModel, PlaceableItemData, NotInCorridors (+5 more)

### Community 103 - "StructureData"
Cohesion: 0.06
Nodes (30): BuildWheelUI, Instance, IsOpen, AirlockData, SnapDistance, CorridorData, ExtensionLength, ExtensionPiece (+22 more)

### Community 104 - "Historique - Refontes"
Cohesion: 0.10
Nodes (19): EquipmentData, Slot, EquipmentSlot, Back, Body, Feet, Hands, Head (+11 more)

### Community 107 - "00 - Index Fonctionnalités.md"
Cohesion: 0.17
Nodes (4): F11 - Données propres à chaque exemplaire, F17 - Contour masqué par les obstacles, F37 - Oxygène hors des zones respirables, Inventaire du joueur

### Community 109 - "PlayerInputActions (4)"
Cohesion: 0.26
Nodes (4): @Camera, CameraActions, enabled, @Rotation

### Community 110 - "Déplacement du joueur.md"
Cohesion: 0.14
Nodes (5): B03 - Décollage en haut des pentes, B07 - Ressenti des déplacements, B21 - Curseur visible après Échap, F03 - Accroupissement, R07 - CharacterController

### Community 111 - "EnergyStorage"
Cohesion: 0.15
Nodes (9): EnergyStorage, Capacity, Charge, Discharge, InputRate, OutputRate, SaveKey, State (+1 more)

### Community 112 - "StatBarUI"
Cohesion: 0.24
Nodes (5): PlayerStatsUI, StatBarUI, StatUIData, R05 - Prefab asset modifié au runtime, Stats du joueur

### Community 113 - "com.unity.performance.profile-analyzer@1.3.3"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.performance.profile-analyzer@1.3.3, com.unity.performance.profile-analyzer

### Community 114 - "packages-lock (7)"
Cohesion: 0.14
Nodes (14): dependencies, depth, source, version, dependencies, depth, source, url (+6 more)

### Community 115 - "InterfaceManager"
Cohesion: 0.29
Nodes (6): Modes de jeu et interfaces, Créer une interface, Interface du jeu (fenêtre qu'on ouvre), Mise en page, Textes, Éléments du HUD

### Community 116 - "LandingCapsuleAnimator"
Cohesion: 0.18
Nodes (4): LandingCapsuleAnimator, IsBusy, IsOpen, Capsule de départ

### Community 117 - "Intention et piliers du style"
Cohesion: 0.67
Nodes (3): Intention, Intention et piliers du style, Les piliers du style

### Community 119 - "Pickupable"
Cohesion: 0.15
Nodes (9): Pickuper, Pickupable, SaveKey, SaveState, ItemGauge, EmptyText, Label, Rechargeable (+1 more)

### Community 120 - "Technologie humaine"
Cohesion: 0.20
Nodes (10): Dômes de verre et tunnels, La capsule, Outils du joueur, Principes, Production, Rations, Robots et IA, Serres (+2 more)

### Community 121 - "wt_build"
Cohesion: 0.32
Nodes (11): animate(), fold(), mast(), build_base(), build_blade(), build_mast(), build_nacelle(), build_rotor() (+3 more)

### Community 122 - "TMPro"
Cohesion: 0.18
Nodes (3): TMP_DigitValidator, TMP_PhoneNumberValidator, TMPro

### Community 123 - "FlashlightBeam"
Cohesion: 0.27
Nodes (3): FlashlightBeam, FlashlightAction, Lampe torche

### Community 124 - "StorageAnimator"
Cohesion: 0.31
Nodes (5): StorageAnimator, CanOpen, IsBusy, IsOpen, StackPoint

### Community 125 - "Label"
Cohesion: 0.29
Nodes (4): ClockUI, ItemTooltip, F54 - Cycle jour nuit et météo, Horloge

### Community 126 - "Fonctionnalités implémentées"
Cohesion: 0.20
Nodes (10): Bootstrap, Construction, Crafting, Fonctionnalités implémentées, Interaction, Interfaces, Inventaire, Joueur (+2 more)

### Community 127 - "WallScreenAnimator"
Cohesion: 0.30
Nodes (3): WallScreenAnimator, IsPowered, ScreenSurface

### Community 129 - "dependencies"
Cohesion: 0.17
Nodes (12): dependencies, com.unity.modules.ai@1.0.0, com.unity.modules.cloth@1.0.0, com.unity.modules.tilemap@1.0.0, com.unity.modules.umbra@1.0.0, com.unity.modules.unitywebrequestassetbundle@1.0.0, com.unity.modules.vectorgraphics@1.0.0, com.unity.modules.unitywebrequestassetbundle (+4 more)

### Community 130 - "packages-lock (8)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 131 - "al_anim"
Cohesion: 0.62
Nodes (5): build(), doors(), fans(), key(), state()

### Community 132 - "EnergyGenerator"
Cohesion: 0.09
Nodes (18): EnergyDeviceInfo, EnergyGenerator, Generation, Output, Production, Network, SolarPanel, Generation (+10 more)

### Community 133 - "StatName"
Cohesion: 0.29
Nodes (6): StatName, Endurance, Health, Hunger, Oxygen, Thirst

### Community 134 - "F65 - Intégration des modèles 3D"
Cohesion: 0.18
Nodes (7): LandingCapsule, IsOpen, SaveKey, SaveState, LandingCapsuleDoor, Capsule, F65 - Intégration des modèles 3D

### Community 135 - "PickupInfoUI"
Cohesion: 0.31
Nodes (3): PickupInfoGroupUI, Group, PickupInfoUI

### Community 136 - "ObjectSpin"
Cohesion: 0.25
Nodes (5): ObjectSpin, MotionType, Rotation, SearchLight, Translation

### Community 137 - "dependencies"
Cohesion: 0.10
Nodes (20): dependencies, depth, hash, source, version, dependencies, depth, source (+12 more)

### Community 138 - "packages-lock (11)"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 139 - "zfight (2)"
Cohesion: 0.39
Nodes (5): geodesic_sphere(), hemisphere(), icosahedron(), orient_matrix(), stats()

### Community 140 - "CameraController"
Cohesion: 0.22
Nodes (5): CameraController, CameraModes, Follow, Free, Isometric

### Community 143 - "ToolData.cs"
Cohesion: 0.50
Nodes (3): ToolConsumptionMode, Continuous, PerUse

### Community 144 - "NotificationUI"
Cohesion: 0.20
Nodes (7): NotificationType, Info, Warning, NotificationUI, Instance, Message, Messages au joueur

### Community 145 - "PauseMenuUI"
Cohesion: 0.16
Nodes (5): Blocked, PauseMenuUI, Instance, IsOpen, Menu pause

### Community 146 - "À faire"
Cohesion: 0.25
Nodes (8): Bugs, Features, Historique, Identifiants, Optimisations, Prochains numéros libres, Refontes, À faire

### Community 147 - "SaveData.cs"
Cohesion: 0.29
Nodes (8): ComponentSaveData, ItemSlotSaveData, ObjectSaveData, PlayerSaveData, SaveData, StatSaveData, WorldTimeSaveData, SaveState

### Community 148 - "Couleur et lumière"
Cohesion: 0.50
Nodes (4): Couleur et lumière, Extérieur, par zone, Intérieur des dômes, Le cycle jour / nuit est un outil émotionnel

### Community 150 - "À éviter et check-list"
Cohesion: 0.50
Nodes (3): Check-list de validation pour chaque asset, À éviter, À éviter et check-list

### Community 151 - "Prompts IA"
Cohesion: 0.50
Nodes (3): Conseils, Prompt de base (image), Prompts IA

### Community 152 - "Bible de direction artistique : [NOM DU JEU]"
Cohesion: 0.67
Nodes (3): Bible de direction artistique : [NOM DU JEU], Légende, Sommaire

### Community 153 - "DistributionPanel"
Cohesion: 0.25
Nodes (8): PanelState, Fused, Loop, NoInput, Off, On, Shed, Starting

### Community 154 - "com.unity.settings-manager"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.settings-manager, com.unity.settings-manager

### Community 155 - "Menaces et lisibilité"
Cohesion: 0.67
Nodes (3): Les menaces, Menaces et lisibilité, Règles de lisibilité

### Community 157 - "PlayerInventory"
Cohesion: 0.12
Nodes (6): PlayerInventory, HandedIndex, HandedSlot, HasPlayer, IsOpen, ToolBar

### Community 159 - "com.unity.modules.ai"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.ai

### Community 160 - "BenchmarkType"
Cohesion: 0.22
Nodes (7): Benchmark03, BenchmarkType, TEXTMESH_BITMAP, TMP_BITMAP_MOBILE, TMP_SDF, TMP_SDF_MOBILE, TMP_SDF__MOBILE_SSD

### Community 161 - "com.unity.modules.androidjni"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.androidjni

### Community 164 - "EnergyNetworkManager"
Cohesion: 0.40
Nodes (5): NetworkState, NotLinked, Powered, SwitchedOff, Tripped

### Community 165 - "ItemGaugeUnit"
Cohesion: 0.50
Nodes (4): ItemGaugeUnit, Energy, Oxygen, Wear

### Community 167 - "packages-lock (14)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

### Community 170 - "Direction"
Cohesion: 0.50
Nodes (3): Direction, In, Out

### Community 180 - "manifest.json"
Cohesion: 0.17
Nodes (11): com.unity.modules.androidjni@1.0.0, com.unity.modules.audio@1.0.0, com.unity.modules.particlesystem@1.0.0, com.unity.modules.wind@1.0.0, com.unity.visualscripting@1.9.11, com.unity.modules.audio, com.unity.modules.particlesystem, com.unity.modules.androidjni (+3 more)

### Community 192 - "TMPro_InstructionOverlay"
Cohesion: 0.27
Nodes (6): TMPro_InstructionOverlay, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

### Community 194 - "IStorageMachine"
Cohesion: 0.20
Nodes (7): IStorageMachine, CanSwitchMachine, Gauge, GaugeText, MachineName, MachineOn, MachineState

### Community 202 - "InterfaceManager"
Cohesion: 0.32
Nodes (5): InterfaceManager, HasOpenInterface, Le joueur, Managers et cycle de vie, Modes

### Community 208 - "Objets persistants"
Cohesion: 0.33
Nodes (4): Root, SceneContext, Objets persistants, Outils de test

### Community 209 - "Sommaire"
Cohesion: 0.50
Nodes (4): Construction des modèles, Méthode technique, Pipeline 3D (Blender → Unity), Sommaire

### Community 211 - "TMP_ExampleScript_01"
Cohesion: 0.33
Nodes (4): TMP_ExampleScript_01, objectType, TextMeshPro, TextMeshProUGUI

### Community 212 - "FpsCounterAnchorPositions"
Cohesion: 0.40
Nodes (5): FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

## Knowledge Gaps
- **442 isolated node(s):** `Generation`, `Generation`, `Protecting`, `Wind`, `Instance` (+437 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 970 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **66 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Modélisation Blender` connect `Airlock Blender Build` to `EnergyGenerator`, `st_build`, `BuiltStructure`, `add_revolve`, `oct_cyl`, `00 - Index Pipeline 3D.md`, `Tunnel Blender Build`?**
  _High betweenness centrality (0.116) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `Builder` (e.g. with `Historique - Bugs` and `Historique - Optimisations`) actually correct?**
  _`Builder` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Generation`, `Generation`, `Protecting` to the rest of the system?**
  _442 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `00 - Index À faire.md` be split into smaller, more focused modules?**
  _Cohesion score 0.1028225806451613 - nodes in this community are weakly interconnected._
- **Why does `Airlock` connect `BuiltStructure` to `Searcher`, `IInterractable.GetInfo`, `MonoBehaviour`, `GameObject`, `Airlock Animator`, `Vector3`, `F65 - Intégration des modèles 3D`, `StructureData`, `Airlock Blender Build`, `system`, `PlayingMode`, `ConnectorPreview.Destroy`, `SolarPanelAnimator`, `EnergyValue`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.110) - this node is a cross-community bridge._
- **Should `system_collections_generic` be split into smaller, more focused modules?**
  _Cohesion score 0.06653225806451613 - nodes in this community are weakly interconnected._
- **Why does `add_revolve()` connect `add_revolve` to `wt_build`, `st_build`, `Airlock Blender Build`, `oct_cyl`, `Landing Capsule Blender Build`, `dr_build`, `st_build (2)`?**
  _High betweenness centrality (0.046) - this node is a cross-community bridge._