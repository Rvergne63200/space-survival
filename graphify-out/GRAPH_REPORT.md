# Graph Report - Space Survival  (2026-11-23)

## Corpus Check
- 361 files · ~168,183 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1493 file(s) not represented in the graph (top: .meta 926, .mat 186, .asset 89)
- WARNING: 1493 of 1933 files seen (77%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 3435 nodes · 8551 edges · 204 communities (140 shown, 64 thin omitted)
- Extraction: 85% EXTRACTED · 15% INFERRED · 0% AMBIGUOUS · INFERRED: 1262 edges (avg confidence: 0.89)
- Token cost: 497,038 input · 0 output

## Graph Freshness
- Built from commit: `7dc96162`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- 00 - Index À faire.md
- PlayerInventory
- Coal Generator Animator
- TMP_Text
- Builder Placement
- Historique - Refontes
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
- ConnectorPreview.Destroy
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
- ItemStorage
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
- Appareils électriques.md
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
- dome_build.py
- ItemCollectionUI
- ws_build.py
- PlayerModeOptions
- RelayAntennaAnimator
- PlayerManager
- Material
- ParentedUI
- packages-lock (2)
- UnityEventRuntimeDebugger
- TMP_TextInfoDebugTool
- VisualElement
- WindTurbineAnimator
- Crafter
- cs_anim
- dr_build
- SaveManager.RestoreWorld
- PlayerInputActions (3)
- ControlPanelUI
- EnergyConsumer
- StorageContainer
- CraftingStation
- Stat (2)
- 00 - Index DA.md
- packages-lock (3)
- packages-lock (4)
- packages-lock (5)
- SkewTextExample
- ControlPanelAnimator
- DeathScreenUI
- UIDocument
- StructureData
- Historique - Refontes
- TerrainEditSaveData
- 00 - Index Fonctionnalités.md
- SubclassPicker
- PlayerInputActions (4)
- Déplacement du joueur.md
- EnergyGenerator
- StatBarUI
- com.unity.performance.profile-analyzer@1.3.3
- packages-lock (7)
- InterfaceManager
- LandingCapsuleAnimator
- Intention et piliers du style
- PlayerSpawnPoint
- Technologie humaine
- wt_build
- TMPro
- FlashlightBeam
- StorageAnimator
- Label
- Fonctionnalités implémentées
- BuildingInterior
- com.unity.modules.adaptiveperformance@1.0.0
- dependencies
- packages-lock (8)
- al_anim
- SolarPanel
- PlayerStats
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
- ItemStorageUI
- Couleur et lumière
- À éviter et check-list
- Prompts IA
- Bible de direction artistique : [NOM DU JEU]
- DistributionPanel
- com.unity.settings-manager
- Menaces et lisibilité
- 14 - Production Unity.md
- PlayerInventory
- Builder
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
- manifest (20)
- manifest (22)
- manifest (24)
- manifest (25)
- manifest (27)
- manifest (28)
- manifest (30)
- manifest (31)
- manifest (32)
- SheddingMode
- Sommaire
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
10. `BuiltStructure` - 47 edges

## Surprising Connections (you probably didn't know these)
- `Inventaire du joueur` --references--> `PlayerInventory`  [INFERRED]
  obsidian/Fonctionnalités/Inventaire/Inventaire du joueur.md → Assets/Scripts/Behaviours/Entities/Player/PlayerInventory.cs
- `Textures` --references--> `ModelImportSetup`  [INFERRED]
  obsidian/Instructions/Pipeline 3D/09 - Textures.md → Assets/Scripts/Editor/ModelImportSetup.cs
- `DistributionPanelUI` --references--> `DistributionPanelSettingsView`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Tableau de distribution.md → Assets/Scripts/Behaviours/UI/Energy/DistributionPanelSettingsView.cs
- `R13 - Snapping entre points de connexion` --references--> `Builder`  [INFERRED]
  obsidian/A faire/R13 - Snapping entre points de connexion.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs
- `ElectricCable` --references--> `Builder`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Câble électrique.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs

## Import Cycles
- None detected.

## Communities (204 total, 64 thin omitted)

### Community 0 - "00 - Index À faire.md"
Cohesion: 0.10
Nodes (13): B26 - Flag identified des objets ramassables, F06 - Vrai glisser-déposer, F27 - Réseau d'oxygène, F38 - Position du sac de mort, F50 - Réseau électrique complet, F51 - Système d'oxygène, F52 - Machines consommatrices d'oxygène, F53 - Système de plantation (+5 more)

### Community 2 - "PlayerInventory"
Cohesion: 0.09
Nodes (29): SubclassPicker, HeldAction, AbstractModifier, EnduranceModifier, LifeModifier, StatModifier, ConsummableData, ApplyModifiersAction (+21 more)

### Community 3 - "Coal Generator Animator"
Cohesion: 0.05
Nodes (27): CoalGeneratorAnimator, ChimneyHeight, CollarRotation, IsBusy, IsHatchOpen, IsRunning, MinChimneyHeight, SmokePoint (+19 more)

### Community 4 - "TMP_Text"
Cohesion: 0.04
Nodes (11): ChatController, EnvMapAnimator, TeleType, TextConsoleSimulator, TMP_TextEventCheck, VertexColorCycler, VertexJitter, VertexAnim (+3 more)

### Community 5 - "Builder Placement"
Cohesion: 0.10
Nodes (7): Builder, CorridorStructure, Inventory, IsActive, Source, BuildModeUI, Instance

### Community 6 - "Historique - Refontes"
Cohesion: 0.06
Nodes (23): Socket, TerrainMask, Matrix, Shape, Box, Cylinder, InteractionDistance, CorridorData (+15 more)

### Community 7 - "ItemSlot"
Cohesion: 0.21
Nodes (5): Slots, ItemSlot, ev_update, Item, Quantity

### Community 8 - "Vector3"
Cohesion: 0.10
Nodes (12): Dome, ConnectorPrefab, Connectors, Model, Radius, ConnectorPreview, DomeConnection, DomeConnector (+4 more)

### Community 9 - "BuiltStructure"
Cohesion: 0.06
Nodes (28): Airlock, All, DoorsBusy, IsBuilt, SaveKey, State, BuiltStructure, CostMultiplier (+20 more)

### Community 10 - "Item"
Cohesion: 0.08
Nodes (18): ChargingStation, Consumption, Storage, DeathBag, ItemSlotSaveData, Item, Data, Gauge (+10 more)

### Community 11 - "Distribution Panel Core"
Cohesion: 0.05
Nodes (43): DistributionPanel, BatteryChargeSupplied, BatteryThreshold, Building, ChargeRequest, ChargeRoom, ChargeSupplied, ConsumedEnergy (+35 more)

### Community 12 - "InteractableOutline"
Cohesion: 0.10
Nodes (8): CameraManager, Camera, Instance, Target, CameraContext, CameraManager, InteractableOutline, Contour de l'objet pointé

### Community 13 - "Save System"
Cohesion: 0.08
Nodes (14): PlayerSaveData, SaveData, SaveSlotInfo, WorldTimeSaveData, SaveManager, HasSave, Instance, IsUnloading (+6 more)

### Community 14 - "ISaveableComponent"
Cohesion: 0.24
Nodes (3): EnvironmentSpawner, F31 - Structures générées, Objets du décor

### Community 15 - "add_revolve"
Cohesion: 0.16
Nodes (20): prism_y(), prof6(), add_box(), add_cyl(), add_revolve(), add_screw(), _copy_into(), finish() (+12 more)

### Community 16 - "Energy Network Manager"
Cohesion: 0.12
Nodes (4): EnergyNetworkManager, HasInstance, Instance, Network

### Community 19 - "oct_cyl"
Cohesion: 0.14
Nodes (26): beam(), oct_cyl(), pill(), animate(), arm_k(), mast_k(), build_arm(), build_base() (+18 more)

### Community 20 - "Landing Capsule Blender Build"
Cohesion: 0.12
Nodes (25): _bevel_tmp(), animate(), build_body(), build_door(), build_door_frames(), build_exterior(), q(), build_interior() (+17 more)

### Community 22 - "Package Lock A"
Cohesion: 0.08
Nodes (28): dependencies, depth, source, version, dependencies, depth, source, version (+20 more)

### Community 23 - "Terrain procédural.md"
Cohesion: 0.15
Nodes (5): B10 - Pivot de ReplaceObject trop haut, F30 - Biomes, F32 - Ressources générées, O03 - Monde infini, R15 - Ids uniques des objets de scène

### Community 24 - "Interfaces UI Toolkit.md"
Cohesion: 0.11
Nodes (11): R01 - Refonte du chargement de scènes, R02 - Persistance entre scènes, R09 - Système d'interaction extensible, Domaines, Instructions, Règles communes, Code Unity, Point ouvert (+3 more)

### Community 25 - "Map Utility & Minimap"
Cohesion: 0.17
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

### Community 32 - "ItemStorage"
Cohesion: 0.14
Nodes (13): AirlockDoor, Airlock, Outer, EnergyDeviceInfo, Interractable, Searcher, CurrentSearchedStorage, IInterractable (+5 more)

### Community 35 - "EnergyDevice"
Cohesion: 0.18
Nodes (3): CablePlacer, Inventory, Source

### Community 36 - "ItemData"
Cohesion: 0.11
Nodes (10): Equipment, ItemCollection, MaxSize, ItemData, Description, Id, Model, Name (+2 more)

### Community 37 - "Terrain Chunks"
Cohesion: 0.19
Nodes (4): ChunkManager, Instance, Seed, TerrainEdits

### Community 39 - "Airlock Animator"
Cohesion: 0.19
Nodes (6): AirlockAnimator, InnerOpen, IsBusy, OuterOpen, Pressurized, Scripts d'animation C# (runtime)

### Community 41 - "st_build"
Cohesion: 0.18
Nodes (18): animate(), build_body(), build_chimney_base(), build_collar(), build_fan(), build_hatch(), build_head(), build_hopper() (+10 more)

### Community 42 - "TextMeshProFloatingText"
Cohesion: 0.12
Nodes (9): Benchmark01, Benchmark02, Benchmark03, Benchmark04, ShaderPropAnimator, SimpleScript, TextMeshProFloatingText, TextMeshSpawner (+1 more)

### Community 44 - "Airlock Blender Build"
Cohesion: 0.19
Nodes (14): build_airlock_body(), build_connector(), build_fan(), build_leaf(), densify(), dome_surface_y(), door_leaf_poly(), finish2() (+6 more)

### Community 46 - "TMP Text Event Handler"
Cohesion: 0.12
Nodes (11): TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, onCharacterSelection, onLineSelection, onLinkSelection, onSpriteSelection (+3 more)

### Community 47 - "UI Manager"
Cohesion: 0.12
Nodes (14): UIManager, ev_updatePlayerContext, Instance, PlayerInventory, PlayerStats, UI, ParentUI, CraftingStationUI (+6 more)

### Community 48 - "EnergyDevice"
Cohesion: 0.12
Nodes (11): EnergyDevice, Cables, CanSwitch, ConnectionPoint, DeviceState, ForcedOff, IsOn, Panel (+3 more)

### Community 49 - "PlayingMode"
Cohesion: 0.14
Nodes (9): ModeManager, Instance, Mode, PlayingMode, Build, Default, Interface, Menu (+1 more)

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
Cohesion: 0.13
Nodes (7): SolarPanelAnimator, DeployProgress, HeadingAngle, IsDeployed, IsRotating, PanelFront, PanelUp

### Community 56 - "00 - Index Pipeline 3D.md"
Cohesion: 0.15
Nodes (8): Façon de travailler, Palette des matériaux, Dimensions de référence, Vitres, Textures, Import Unity, Animations Blender, Animator Unity

### Community 57 - "st_build (2)"
Cohesion: 0.19
Nodes (14): build(), cabinet_body(), cabinet_doors(), cham(), clear(), collisions(), crate_body(), crate_lids() (+6 more)

### Community 58 - "DrillAnimator"
Cohesion: 0.20
Nodes (5): DrillAnimator, DustPoint, IsBusy, IsDrilling, Powered

### Community 59 - "SaveableObject"
Cohesion: 0.12
Nodes (11): SaveId, SaveableObject, Id, IsRegistered, IsRuntime, PrefabKey, ComponentSaveData, ObjectSaveData (+3 more)

### Community 60 - "CraftingStationUI"
Cohesion: 0.20
Nodes (4): CraftingStationUI, RecipeSlotUI, Element, Recipe

### Community 61 - "dependencies"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, url (+18 more)

### Community 62 - "bmesh"
Cohesion: 0.22
Nodes (8): build_all(), build_size(), check(), check2(), faces_of(), islands_of(), overlap_area(), Erreurs à ne pas refaire

### Community 63 - "TMP_UiFrameRateCounter"
Cohesion: 0.09
Nodes (12): Benchmark01_UGUI, DropdownSample, TMP_ExampleScript_01, objectType, TextMeshPro, TextMeshProUGUI, TMP_UiFrameRateCounter, FpsCounterAnchorPositions (+4 more)

### Community 64 - "TMP_FrameRateCounter"
Cohesion: 0.13
Nodes (12): TMP_FrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight, TMPro_InstructionOverlay, FpsCounterAnchorPositions (+4 more)

### Community 65 - "PlayerContext"
Cohesion: 0.11
Nodes (9): PlayerContext, Pickuper, PlayerContext, PlayerManager, PointedObject, PlayerInterractor, PlayerPointer, Source (+1 more)

### Community 66 - "DayNightCycle"
Cohesion: 0.09
Nodes (18): Root, SceneContext, DayNightCycle, Day, Hour, Instance, IsDay, NightBrightness (+10 more)

### Community 67 - "CameraMovement"
Cohesion: 0.18
Nodes (5): CameraMovement, Pitch, Target, Yaw, Caméra

### Community 68 - "Pose et destruction.md"
Cohesion: 0.17
Nodes (6): F47 - Relais radio, R12 - Intégration des constructions au terrain, R13 - Snapping entre points de connexion, Objets posés, Destruction, Pose et destruction

### Community 69 - "BuildingPowerPanel"
Cohesion: 0.10
Nodes (16): BuildingOutlet, Building, OptionalDemand, SaveKey, BuildingPanelTerminal, Panel, BuildingPowerPanel, Building (+8 more)

### Community 70 - "PlayerMovement"
Cohesion: 0.16
Nodes (5): PlayerMovement, forwardDirection, IsSprinting, Stats, Déplacement du joueur

### Community 71 - "dome_build.py"
Cohesion: 0.26
Nodes (12): build_base(), build_frame(), build_glass(), build_wall(), curved_slab(), dome_sphere(), make_mats(), node() (+4 more)

### Community 72 - "ItemCollectionUI"
Cohesion: 0.15
Nodes (8): ItemCollectionUI, SlotsUI, ItemSlotUI, Element, PlayerInventoryContentUI, PlayerToolBarUI, R05 - Prefab asset modifié au runtime, Clics de l'inventaire

### Community 73 - "ws_build.py"
Cohesion: 0.18
Nodes (12): arc_pt(), build_body(), build_screen(), clear(), rect(), rho(), screen_uv(), setup() (+4 more)

### Community 74 - "PlayerModeOptions"
Cohesion: 0.17
Nodes (11): PlayerModeOptions, Options, Commentaires, Composants, Conventions de code, Gameplay et modèles, Nommage, Robustesse (+3 more)

### Community 75 - "RelayAntennaAnimator"
Cohesion: 0.18
Nodes (4): RelayAntennaAnimator, IsAntennaExtended, IsDishDeployed, IsRotating

### Community 76 - "PlayerManager"
Cohesion: 0.16
Nodes (8): PlayerManager, Instance, Inventory, IsDead, Player, SpawnedFromSave, Stats, ViewSource

### Community 77 - "Material"
Cohesion: 0.24
Nodes (4): BuildUtility, BlueprintMaterial, Structures et catalogue, L'objet

### Community 79 - "packages-lock (2)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 82 - "VisualElement"
Cohesion: 0.21
Nodes (4): DistributionPanelUI, Instance, HeldItemUI, HeldItem

### Community 83 - "WindTurbineAnimator"
Cohesion: 0.17
Nodes (3): WindTurbineAnimator, IsDeployed, WindStrength

### Community 85 - "Crafter"
Cohesion: 0.24
Nodes (8): Crafter, RecipeData, Ingredients, ItemBlock, Item, Quantity, Results, Station de craft

### Community 86 - "cs_anim"
Cohesion: 0.33
Nodes (13): build_anim(), clear(), head_clearance(), ik(), k_arm(), k_head(), k_laser(), k_plate() (+5 more)

### Community 87 - "dr_build"
Cohesion: 0.19
Nodes (14): animate(), build_body(), build_carriage(), build_head(), build_hologram(), sbox(), sp(), collisions() (+6 more)

### Community 89 - "PlayerInputActions (3)"
Cohesion: 0.22
Nodes (6): @Movement, MovementActions, enabled, @Jump, @Move, @Sprint

### Community 90 - "ControlPanelUI"
Cohesion: 0.14
Nodes (12): ControlPanel, Consumer, DisplayName, IsPowered, ControlPanelUI, Instance, Carte, ControlPanelUI (+4 more)

### Community 91 - "EnergyConsumer"
Cohesion: 0.12
Nodes (9): EnergyConsumer, Consumption, Demand, IsPowered, State, TestMachine, Items du réseau, Objets de test (+1 more)

### Community 92 - "StorageContainer"
Cohesion: 0.12
Nodes (12): StorageContainer, Above, Below, SaveKey, Stackable, StackPoint, StackSize, State (+4 more)

### Community 93 - "CraftingStation"
Cohesion: 0.15
Nodes (11): CraftingStation, Current, IsUsable, Output, Power, Progress, Queue, Recipes (+3 more)

### Community 94 - "Stat (2)"
Cohesion: 0.15
Nodes (6): Stat, MaxValue, Value, StatMarker, Duration, Modifier

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
Cohesion: 0.13
Nodes (7): ControlPanelAnimator, HologramScreen, IsBusy, IsPowered, WallScreenAnimator, IsPowered, ScreenSurface

### Community 101 - "DeathScreenUI"
Cohesion: 0.23
Nodes (4): DeathScreenUI, Instance, IsOpen, Écran de mort

### Community 102 - "UIDocument"
Cohesion: 0.18
Nodes (4): ClockUI, HandedCursorUI, F54 - Cycle jour nuit et météo, Horloge

### Community 103 - "StructureData"
Cohesion: 0.06
Nodes (34): PlacedObject, Data, DisplayName, DisplayName, BuildWheelUI, Instance, IsOpen, AirlockData (+26 more)

### Community 104 - "Historique - Refontes"
Cohesion: 0.07
Nodes (27): Pickupable, SaveKey, SaveState, EquipmentData, Slot, EquipmentSlot, Back, Body (+19 more)

### Community 105 - "TerrainEditSaveData"
Cohesion: 0.33
Nodes (3): TerrainEditSaveData, TerrainFlattening, Terrain sous les constructions

### Community 107 - "00 - Index Fonctionnalités.md"
Cohesion: 0.16
Nodes (4): B21 - Curseur visible après Échap, F11 - Données propres à chaque exemplaire, F37 - Oxygène hors des zones respirables, Inventaire du joueur

### Community 109 - "PlayerInputActions (4)"
Cohesion: 0.26
Nodes (4): @Camera, CameraActions, enabled, @Rotation

### Community 110 - "Déplacement du joueur.md"
Cohesion: 0.20
Nodes (4): B03 - Décollage en haut des pentes, B07 - Ressenti des déplacements, F03 - Accroupissement, R07 - CharacterController

### Community 111 - "EnergyGenerator"
Cohesion: 0.10
Nodes (18): EnergyGenerator, Generation, Output, Production, EnergyStorage, Capacity, Charge, Discharge (+10 more)

### Community 112 - "StatBarUI"
Cohesion: 0.25
Nodes (4): PlayerStatsUI, StatBarUI, StatUIData, Stats du joueur

### Community 113 - "com.unity.performance.profile-analyzer@1.3.3"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.performance.profile-analyzer@1.3.3, com.unity.performance.profile-analyzer

### Community 114 - "packages-lock (7)"
Cohesion: 0.14
Nodes (14): dependencies, depth, source, version, dependencies, depth, source, url (+6 more)

### Community 115 - "InterfaceManager"
Cohesion: 0.20
Nodes (8): InterfaceManager, HasOpenInterface, Modes de jeu et interfaces, Créer une interface, Interface du jeu (fenêtre qu'on ouvre), Mise en page, Textes, Éléments du HUD

### Community 116 - "LandingCapsuleAnimator"
Cohesion: 0.13
Nodes (10): LandingCapsuleAnimator, IsBusy, IsOpen, LandingCapsule, IsOpen, SaveKey, SaveState, LandingCapsuleDoor (+2 more)

### Community 117 - "Intention et piliers du style"
Cohesion: 0.67
Nodes (3): Intention, Intention et piliers du style, Les piliers du style

### Community 119 - "PlayerSpawnPoint"
Cohesion: 0.24
Nodes (6): PlayerSpawnPoint, SceneLoader, Fonctionnement actuel, Scènes, Scènes et chargement, Mort et réapparition

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
Cohesion: 0.31
Nodes (3): FlashlightBeam, FlashlightAction, Lampe torche

### Community 124 - "StorageAnimator"
Cohesion: 0.17
Nodes (5): StorageAnimator, CanOpen, IsBusy, IsOpen, StackPoint

### Community 126 - "Fonctionnalités implémentées"
Cohesion: 0.20
Nodes (10): Bootstrap, Construction, Crafting, Fonctionnalités implémentées, Interaction, Interfaces, Inventaire, Joueur (+2 more)

### Community 127 - "BuildingInterior"
Cohesion: 0.25
Nodes (4): BuildingInterior, Radius, Zone, Intérieur des bâtiments

### Community 129 - "dependencies"
Cohesion: 0.17
Nodes (12): dependencies, com.unity.modules.ai@1.0.0, com.unity.modules.cloth@1.0.0, com.unity.modules.tilemap@1.0.0, com.unity.modules.umbra@1.0.0, com.unity.modules.unitywebrequestassetbundle@1.0.0, com.unity.modules.vectorgraphics@1.0.0, com.unity.modules.unitywebrequestassetbundle (+4 more)

### Community 130 - "packages-lock (8)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 131 - "al_anim"
Cohesion: 0.62
Nodes (5): build(), doors(), fans(), key(), state()

### Community 132 - "SolarPanel"
Cohesion: 0.20
Nodes (3): SolarPanel, Generation, F49 - Génération d'énergie

### Community 133 - "PlayerStats"
Cohesion: 0.14
Nodes (11): PlayerStats, StatName, Endurance, Health, Hunger, Oxygen, Thirst, StatSaveData (+3 more)

### Community 135 - "PickupInfoUI"
Cohesion: 0.31
Nodes (3): PickupInfoGroupUI, Group, PickupInfoUI

### Community 136 - "ObjectSpin"
Cohesion: 0.22
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
Cohesion: 0.11
Nodes (7): PlayerInventory, HandedIndex, HandedSlot, HasPlayer, IsOpen, ToolBar, Historique - Bugs

### Community 158 - "Builder"
Cohesion: 0.29
Nodes (7): Mode, Airlock, Building, Corridor, Destroying, None, Placing

### Community 159 - "com.unity.modules.ai"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.ai

### Community 160 - "BenchmarkType"
Cohesion: 0.33
Nodes (6): BenchmarkType, TEXTMESH_BITMAP, TMP_BITMAP_MOBILE, TMP_SDF, TMP_SDF_MOBILE, TMP_SDF__MOBILE_SSD

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

### Community 207 - "SheddingMode"
Cohesion: 0.50
Nodes (4): SheddingMode, BatteryBelow, None, Shortage

### Community 209 - "Sommaire"
Cohesion: 0.50
Nodes (4): Construction des modèles, Méthode technique, Pipeline 3D (Blender → Unity), Sommaire

## Knowledge Gaps
- **421 isolated node(s):** `Data`, `Gauge`, `GaugeRatio`, `GaugeSettings`, `HasGauge` (+416 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 941 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **64 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Modélisation Blender` connect `add_revolve` to `SolarPanel`, `BuiltStructure`, `Airlock Blender Build`, `oct_cyl`, `00 - Index Pipeline 3D.md`, `Tunnel Blender Build`?**
  _High betweenness centrality (0.130) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `Builder` (e.g. with `Historique - Bugs` and `Historique - Optimisations`) actually correct?**
  _`Builder` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Data`, `Gauge`, `GaugeRatio` to the rest of the system?**
  _421 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `00 - Index À faire.md` be split into smaller, more focused modules?**
  _Cohesion score 0.10317460317460317 - nodes in this community are weakly interconnected._
- **Why does `Airlock` connect `BuiltStructure` to `ItemStorage`, `IInterractable.GetInfo`, `Builder Placement`, `Historique - Refontes`, `Airlock Animator`, `Vector3`, `StructureData`, `TextMeshProFloatingText`, `system`, `Material`, `add_revolve`, `ConnectorPreview.Destroy`, `ISaveableComponent.RestoreState`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.122) - this node is a cross-community bridge._
- **Should `system_collections_generic` be split into smaller, more focused modules?**
  _Cohesion score 0.06653225806451613 - nodes in this community are weakly interconnected._
- **Why does `add_revolve()` connect `add_revolve` to `wt_build`, `dome_build.py`, `st_build`, `Airlock Blender Build`, `oct_cyl`, `Landing Capsule Blender Build`, `dr_build`, `st_build (2)`?**
  _High betweenness centrality (0.056) - this node is a cross-community bridge._