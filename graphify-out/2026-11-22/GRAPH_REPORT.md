# Graph Report - Space Survival  (2026-11-22)

## Corpus Check
- 349 files · ~163,986 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1493 file(s) not represented in the graph (top: .meta 926, .mat 186, .asset 89)
- WARNING: 1493 of 1921 files seen (78%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 3366 nodes · 8315 edges · 207 communities (148 shown, 59 thin omitted)
- Extraction: 86% EXTRACTED · 14% INFERRED · 0% AMBIGUOUS · INFERRED: 1143 edges (avg confidence: 0.88)
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
- GameObject
- ItemSlot
- Vector3
- Airlock & Corridor Structures
- Historique - Features
- Distribution Panel Core
- InteractableOutline
- Save System
- Item
- add_revolve
- Energy Network Manager
- Network Map UI
- system_collections
- Blender Build Scripts
- Landing Capsule Blender Build
- StructureBlueprint
- Package Lock A
- Sauvegarde.md
- Équipement et jetpack.md
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
- CablePlacer
- Airlock Animator
- Distribution Panel UI
- st_build
- TextMeshProFloatingText
- 00 - Index Fonctionnalités.md
- Airlock Blender Build
- system
- TMP Text Event Handler
- UI Manager
- PlaceableItemData
- PlayingMode
- cp_build.py
- 00 - Index Pipeline 3D.md
- PlayerInputActions
- TMP Text Selectors
- SolarPanelAnimator
- Main Menu UI
- Interfaces UI Toolkit.md
- st_build (2)
- DrillAnimator
- Saveable Objects
- CraftingStationUI
- packages-lock
- bmesh
- TMP_UiFrameRateCounter
- TMP_FrameRateCounter
- PlayerContext
- DayNightCycle
- PlayerInputActions (2)
- Pose et destruction.md
- BuildingPowerPanel
- PlayerMovement
- StatName
- ItemCollectionUI
- ws_build.py
- MonoBehaviour
- RelayAntennaAnimator
- PlayerManager
- Button
- ParentedUI
- packages-lock (2)
- UnityEventRuntimeDebugger
- TMP_TextInfoDebugTool
- CraftingStationAnimator
- WindTurbineAnimator
- StructureData
- cs_anim
- dr_build
- VertexShakeA
- PlayerInputActions (3)
- ControlPanelUI
- EnergyConsumer
- StorageContainer
- Crafter
- Stat (2)
- 00 - Index DA.md
- packages-lock (3)
- packages-lock (4)
- packages-lock (5)
- SkewTextExample
- ControlPanelAnimator
- DeathScreenUI
- CameraManager
- dome_build.py
- Historique - Refontes
- ObjectSpin
- CraftingStation
- SubclassPicker
- PlayerInputActions (4)
- Déplacement du joueur.md
- EnergyStorage (2)
- StatBarUI
- packages-lock (6)
- packages-lock (7)
- CorridorData
- LandingCapsuleAnimator
- Intention et piliers du style
- Pickupable
- FlashlightBeam
- Technologie humaine
- wt_build
- TMPro
- InteractableOutline
- CameraMovement
- Label
- Fonctionnalités implémentées
- BuildingInterior
- manifest
- manifest (2)
- packages-lock (8)
- al_anim
- TerrainFlattening
- SolarPanel
- LandingCapsule
- PickupInfoUI
- packages-lock (9)
- packages-lock (10)
- packages-lock (11)
- zfight (2)
- CameraController
- TerrainMask
- TerrainGenerator
- NotificationUI
- InterfaceManager.GetInstance
- À faire
- EnergyGenerator
- Couleur et lumière
- À éviter et check-list
- Prompts IA
- Bible de direction artistique : [NOM DU JEU]
- DistributionPanel
- EnvironmentSpawner.Spawn
- Menaces et lisibilité
- 09 - Textures.md
- Historique - Bugs
- Builder
- VertexJitter
- BenchmarkType
- MonoBehaviour (3)
- packages-lock (12)
- VertexZoom
- EnergyNetworkManager
- ItemGaugeUnit
- packages-lock (13)
- packages-lock (14)
- ToolData.cs
- Assembly-CSharp
- Direction
- Sommaire
- ChunkManager (singleton, scène Monde)
- manifest (3)
- manifest (4)
- manifest (5)
- manifest (6)
- manifest (7)
- manifest (8)
- manifest (9)
- manifest (10)
- manifest (11)
- manifest (12)
- manifest (13)
- manifest (14)
- manifest (15)
- manifest (16)
- manifest (17)
- manifest (18)
- manifest (19)
- manifest (20)
- manifest (21)
- manifest (22)
- manifest (23)
- manifest (24)
- manifest (25)
- manifest (26)
- manifest (27)
- manifest (28)
- manifest (29)
- manifest (30)
- manifest (31)
- manifest (32)
- manifest (33)
- CLAUDE

## God Nodes (most connected - your core abstractions)
1. `Builder` - 84 edges
2. `DistributionPanel` - 77 edges
3. `EnergyDevice` - 75 edges
4. `SaveManager` - 66 edges
5. `PlayerInventory` - 66 edges
6. `EnergyNetworkManager` - 55 edges
7. `Historique - Features` - 53 edges
8. `Item` - 52 edges
9. `CoalGeneratorAnimator` - 51 edges
10. `NetworkMapElement` - 46 edges

## Surprising Connections (you probably didn't know these)
- `Textures` --references--> `ModelImportSetup`  [INFERRED]
  obsidian/Pipeline 3D/09 - Textures.md → Assets/Scripts/Editor/ModelImportSetup.cs
- `Inventaire du joueur` --references--> `PlayerInventory`  [INFERRED]
  obsidian/Fonctionnalités/Inventaire/Inventaire du joueur.md → Assets/Scripts/Behaviours/Entities/Player/PlayerInventory.cs
- `DistributionPanelUI` --references--> `DistributionPanelSettingsView`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Tableau de distribution.md → Assets/Scripts/Behaviours/UI/Energy/DistributionPanelSettingsView.cs
- `R13 - Snapping entre points de connexion` --references--> `Builder`  [INFERRED]
  obsidian/A faire/R13 - Snapping entre points de connexion.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs
- `ElectricCable` --references--> `Builder`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Câble électrique.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs

## Import Cycles
- None detected.

## Communities (207 total, 59 thin omitted)

### Community 0 - "00 - Index À faire.md"
Cohesion: 0.16
Nodes (8): B26 - Flag identified des objets ramassables, F27 - Réseau d'oxygène, F50 - Réseau électrique complet, F51 - Système d'oxygène, F52 - Machines consommatrices d'oxygène, F53 - Système de plantation, F56 - Horaires du tableau de distribution, R09 - Système d'interaction extensible

### Community 2 - "PlayerInventory"
Cohesion: 0.08
Nodes (20): PlayerInventory, HandedIndex, HandedSlot, HasPlayer, HeldAction, IsOpen, ToolBar, HoldItemAction (+12 more)

### Community 3 - "Coal Generator Animator"
Cohesion: 0.07
Nodes (20): CoalGeneratorAnimator, ChimneyHeight, CollarRotation, IsBusy, IsHatchOpen, IsRunning, MinChimneyHeight, SmokePoint (+12 more)

### Community 4 - "TMP_Text"
Cohesion: 0.10
Nodes (5): EnvMapAnimator, TeleType, TextConsoleSimulator, TMP_TextEventCheck, VertexColorCycler

### Community 5 - "Builder Placement"
Cohesion: 0.09
Nodes (7): Builder, CorridorStructure, Inventory, IsActive, Source, IPlacementPreview, PreviewModel

### Community 6 - "GameObject"
Cohesion: 0.12
Nodes (8): AirlockDoor, Airlock, Outer, BuildUtility, BlueprintMaterial, OwnedMeshes, Historique - Refontes, Structures et catalogue

### Community 7 - "ItemSlot"
Cohesion: 0.20
Nodes (5): Slots, ItemSlot, ev_update, Item, Quantity

### Community 8 - "Vector3"
Cohesion: 0.09
Nodes (15): Dome, ConnectorPrefab, Connectors, Model, Radius, ConnectorPreview, DomeConnection, DomeConnector (+7 more)

### Community 9 - "Airlock & Corridor Structures"
Cohesion: 0.07
Nodes (19): Airlock, All, DoorsBusy, IsBuilt, SaveKey, State, BuiltStructure, CostMultiplier (+11 more)

### Community 10 - "Historique - Features"
Cohesion: 0.21
Nodes (10): PickupInfoGroupUI, AbstractModifier, EnduranceModifier, LifeModifier, StatModifier, ConsummableData, ApplyModifiersAction, Historique - Features (+2 more)

### Community 11 - "Distribution Panel Core"
Cohesion: 0.05
Nodes (41): DistributionPanel, BatteryChargeSupplied, BatteryThreshold, Building, ChargeRequest, ChargeRoom, ChargeSupplied, ConsumedEnergy (+33 more)

### Community 13 - "Save System"
Cohesion: 0.10
Nodes (12): PlayerSaveData, SaveData, StatSaveData, SaveManager, HasSave, Instance, IsUnloading, LiveObjects (+4 more)

### Community 14 - "Item"
Cohesion: 0.12
Nodes (12): HeldItemUI, HeldItem, ItemSlotUI, Element, Item, Data, Gauge, GaugeRatio (+4 more)

### Community 15 - "add_revolve"
Cohesion: 0.16
Nodes (20): prism_y(), prof6(), add_box(), add_cyl(), add_revolve(), add_screw(), _copy_into(), finish() (+12 more)

### Community 16 - "Energy Network Manager"
Cohesion: 0.12
Nodes (5): EnergyNetworkManager, HasInstance, Instance, Network, Appareils électriques

### Community 19 - "Blender Build Scripts"
Cohesion: 0.14
Nodes (26): beam(), oct_cyl(), pill(), animate(), arm_k(), mast_k(), build_arm(), build_base() (+18 more)

### Community 20 - "Landing Capsule Blender Build"
Cohesion: 0.12
Nodes (25): _bevel_tmp(), animate(), build_body(), build_door(), build_door_frames(), build_exterior(), q(), build_interior() (+17 more)

### Community 21 - "StructureBlueprint"
Cohesion: 0.12
Nodes (5): StructureBlueprint, CostMultiplier, Data, SaveKey, SaveState

### Community 22 - "Package Lock A"
Cohesion: 0.06
Nodes (33): dependencies, depth, source, version, dependencies, depth, source, version (+25 more)

### Community 23 - "Sauvegarde.md"
Cohesion: 0.11
Nodes (7): B10 - Pivot de ReplaceObject trop haut, F30 - Biomes, F32 - Ressources générées, O03 - Monde infini, R01 - Refonte du chargement de scènes, R02 - Persistance entre scènes, R15 - Ids uniques des objets de scène

### Community 24 - "Équipement et jetpack.md"
Cohesion: 0.12
Nodes (8): F06 - Vrai glisser-déposer, F11 - Données propres à chaque exemplaire, F37 - Oxygène hors des zones respirables, F38 - Position du sac de mort, R08 - Refonte du HUD des stats, R10 - Ids d'items générés, R11 - Direction.In de TryMerge, Inventaire du joueur

### Community 25 - "Map Utility & Minimap"
Cohesion: 0.17
Nodes (4): MapUtility, ContourSegment, MinimapUI, Minimap

### Community 26 - "Package Lock B"
Cohesion: 0.08
Nodes (30): dependencies, depth, source, version, dependencies, depth, source, version (+22 more)

### Community 27 - "Package Lock C"
Cohesion: 0.07
Nodes (31): dependencies, depth, source, version, dependencies, depth, source, url (+23 more)

### Community 28 - "Tunnel Blender Build"
Cohesion: 0.18
Nodes (24): add_body(), add_flange(), add_rib(), f_dir(), glass_poly(), hollow_sweep(), pane(), path_frame() (+16 more)

### Community 29 - "Electric Cables"
Cohesion: 0.10
Nodes (12): ElectricCable, End, SaveKey, Start, State, CableItemData, Material, MetersPerItem (+4 more)

### Community 30 - "Package Lock D"
Cohesion: 0.08
Nodes (29): dependencies, depth, source, version, dependencies, depth, source, version (+21 more)

### Community 31 - "Input Actions"
Cohesion: 0.08
Nodes (8): @PlayerInputActions, asset, bindingMask, bindings, controlSchemes, devices, ICameraActions, R06 - PlayerInputActions jamais libérés

### Community 32 - "ItemStorage"
Cohesion: 0.08
Nodes (14): ChargingStation, Consumption, Storage, Searcher, CurrentSearchedStorage, DeathBag, Searchable, ItemSlotSaveData (+6 more)

### Community 35 - "EnergyDevice"
Cohesion: 0.08
Nodes (13): DisplayName, EnergyDevice, Cables, CanSwitch, ConnectionPoint, DeviceState, DisplayName, ForcedOff (+5 more)

### Community 36 - "ItemData"
Cohesion: 0.09
Nodes (11): Pickuper, Equipment, ItemCollection, MaxSize, ItemData, Description, Id, Model (+3 more)

### Community 37 - "Terrain Chunks"
Cohesion: 0.16
Nodes (4): ChunkManager, Instance, Seed, TerrainEdits

### Community 38 - "CablePlacer"
Cohesion: 0.20
Nodes (3): CablePlacer, Inventory, Source

### Community 39 - "Airlock Animator"
Cohesion: 0.19
Nodes (5): AirlockAnimator, InnerOpen, IsBusy, OuterOpen, Pressurized

### Community 40 - "Distribution Panel UI"
Cohesion: 0.12
Nodes (4): DistributionPanelSettingsView, Target, DistributionPanelUI, Instance

### Community 41 - "st_build"
Cohesion: 0.18
Nodes (18): animate(), build_body(), build_chimney_base(), build_collar(), build_fan(), build_hatch(), build_head(), build_hopper() (+10 more)

### Community 42 - "TextMeshProFloatingText"
Cohesion: 0.19
Nodes (5): Benchmark01, Benchmark02, SimpleScript, TextMeshProFloatingText, TextMeshSpawner

### Community 43 - "00 - Index Fonctionnalités.md"
Cohesion: 0.22
Nodes (3): F17 - Contour masqué par les obstacles, F20 - Recettes à plusieurs résultats, F55 - Informations des réseaux

### Community 44 - "Airlock Blender Build"
Cohesion: 0.19
Nodes (14): build_airlock_body(), build_connector(), build_fan(), build_leaf(), densify(), dome_surface_y(), door_leaf_poly(), finish2() (+6 more)

### Community 46 - "TMP Text Event Handler"
Cohesion: 0.12
Nodes (11): TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, onCharacterSelection, onLineSelection, onLinkSelection, onSpriteSelection (+3 more)

### Community 47 - "UI Manager"
Cohesion: 0.12
Nodes (14): UIManager, ev_updatePlayerContext, Instance, PlayerInventory, PlayerStats, UI, ParentUI, CraftingStationUI (+6 more)

### Community 48 - "PlaceableItemData"
Cohesion: 0.12
Nodes (12): PlacedObject, Data, BuildModeUI, Instance, PlaceableItemData, NotInCorridors, OutdoorOnly, PlacedPrefab (+4 more)

### Community 49 - "PlayingMode"
Cohesion: 0.13
Nodes (11): ModeManager, Instance, Mode, PlayerModeOptions, Options, PlayingMode, Build, Default (+3 more)

### Community 50 - "cp_build.py"
Cohesion: 0.14
Nodes (15): animate(), build_body(), build_hologram(), sbox(), sp(), build_projector(), cham(), clear() (+7 more)

### Community 51 - "00 - Index Pipeline 3D.md"
Cohesion: 0.18
Nodes (6): Façon de travailler, Palette des matériaux, Dimensions de référence, Import Unity, Animations Blender, Animator Unity

### Community 52 - "PlayerInputActions"
Cohesion: 0.14
Nodes (11): @Actions, ActionsActions, @Build, @ChangeHandedItem, @CloseInterfaces, @Drop, enabled, @Interract (+3 more)

### Community 54 - "SolarPanelAnimator"
Cohesion: 0.14
Nodes (7): SolarPanelAnimator, DeployProgress, HeadingAngle, IsDeployed, IsRotating, PanelFront, PanelUp

### Community 55 - "Main Menu UI"
Cohesion: 0.17
Nodes (3): MainMenuUI, SaveSlotInfo, Menu principal

### Community 57 - "st_build (2)"
Cohesion: 0.19
Nodes (14): build(), cabinet_body(), cabinet_doors(), cham(), clear(), collisions(), crate_body(), crate_lids() (+6 more)

### Community 58 - "DrillAnimator"
Cohesion: 0.19
Nodes (5): DrillAnimator, DustPoint, IsBusy, IsDrilling, Powered

### Community 59 - "Saveable Objects"
Cohesion: 0.15
Nodes (8): SaveId, SaveableObject, Id, IsRegistered, IsRuntime, PrefabKey, ComponentSaveData, ObjectSaveData

### Community 60 - "CraftingStationUI"
Cohesion: 0.20
Nodes (4): CraftingStationUI, RecipeSlotUI, Element, Recipe

### Community 61 - "packages-lock"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 62 - "bmesh"
Cohesion: 0.22
Nodes (8): build_all(), build_size(), check(), check2(), faces_of(), islands_of(), overlap_area(), Erreurs à ne pas refaire

### Community 63 - "TMP_UiFrameRateCounter"
Cohesion: 0.15
Nodes (7): Benchmark01_UGUI, TMP_UiFrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

### Community 64 - "TMP_FrameRateCounter"
Cohesion: 0.13
Nodes (12): TMP_FrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight, TMPro_InstructionOverlay, FpsCounterAnchorPositions (+4 more)

### Community 65 - "PlayerContext"
Cohesion: 0.10
Nodes (10): PlayerContext, PlayerContext, Interractable, PlayerManager, PointedObject, PlayerInterractor, PlayerPointer, Source (+2 more)

### Community 66 - "DayNightCycle"
Cohesion: 0.12
Nodes (12): DayNightCycle, Day, Hour, Instance, IsDay, NightBrightness, NoonSunHeight, Sun (+4 more)

### Community 68 - "Pose et destruction.md"
Cohesion: 0.19
Nodes (4): F47 - Relais radio, R12 - Intégration des constructions au terrain, R13 - Snapping entre points de connexion, Objets posés

### Community 69 - "BuildingPowerPanel"
Cohesion: 0.11
Nodes (15): BuildingOutlet, Building, OptionalDemand, SaveKey, BuildingPanelTerminal, Panel, BuildingPowerPanel, Building (+7 more)

### Community 70 - "PlayerMovement"
Cohesion: 0.16
Nodes (5): PlayerMovement, forwardDirection, IsSprinting, Stats, Déplacement du joueur

### Community 71 - "StatName"
Cohesion: 0.29
Nodes (6): StatName, Endurance, Health, Hunger, Oxygen, Thirst

### Community 72 - "ItemCollectionUI"
Cohesion: 0.15
Nodes (6): ItemCollectionUI, SlotsUI, ItemStorageUI, PlayerInventoryContentUI, PlayerToolBarUI, Clics de l'inventaire

### Community 73 - "ws_build.py"
Cohesion: 0.18
Nodes (12): arc_pt(), build_body(), build_screen(), clear(), rect(), rho(), screen_uv(), setup() (+4 more)

### Community 74 - "MonoBehaviour"
Cohesion: 0.11
Nodes (6): Benchmark04, ChatController, DropdownSample, Bootstrapper, Root, SceneContext

### Community 75 - "RelayAntennaAnimator"
Cohesion: 0.18
Nodes (4): RelayAntennaAnimator, IsAntennaExtended, IsDishDeployed, IsRotating

### Community 76 - "PlayerManager"
Cohesion: 0.10
Nodes (14): PlayerManager, Instance, Inventory, IsDead, Player, SpawnedFromSave, Stats, ViewSource (+6 more)

### Community 77 - "Button"
Cohesion: 0.20
Nodes (7): IStorageMachine, CanSwitchMachine, Gauge, GaugeText, MachineName, MachineOn, MachineState

### Community 78 - "ParentedUI"
Cohesion: 0.12
Nodes (3): PointedInterractorInformationUI, HandedCursorUI, ParentedUI

### Community 79 - "packages-lock (2)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 82 - "CraftingStationAnimator"
Cohesion: 0.21
Nodes (4): CraftingStationAnimator, HologramScreen, IsPowered, IsPrinting

### Community 83 - "WindTurbineAnimator"
Cohesion: 0.18
Nodes (4): WindTurbineAnimator, IsDeployed, WindStrength, Scripts d'animation C# (runtime)

### Community 85 - "StructureData"
Cohesion: 0.10
Nodes (19): BuildWheelUI, Instance, IsOpen, AirlockData, SnapDistance, StructureCatalog, Structures, StructureData (+11 more)

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
Cohesion: 0.13
Nodes (12): ControlPanel, Consumer, DisplayName, IsPowered, ControlPanelUI, Instance, Carte, ControlPanelUI (+4 more)

### Community 91 - "EnergyConsumer"
Cohesion: 0.12
Nodes (9): EnergyConsumer, Consumption, Demand, IsPowered, State, TestMachine, Items du réseau, Objets de test (+1 more)

### Community 92 - "StorageContainer"
Cohesion: 0.09
Nodes (14): StorageAnimator, CanOpen, IsBusy, IsOpen, StackPoint, StorageContainer, Above, Below (+6 more)

### Community 93 - "Crafter"
Cohesion: 0.24
Nodes (8): Crafter, RecipeData, Ingredients, ItemBlock, Item, Quantity, Results, Station de craft

### Community 94 - "Stat (2)"
Cohesion: 0.12
Nodes (10): PlayerStats, SerializableKeyValuePair, Key, Value, Stat, MaxValue, Value, StatMarker (+2 more)

### Community 95 - "00 - Index DA.md"
Cohesion: 0.17
Nodes (9): Fiche du jeu, Formes et silhouettes, Matériaux et textures, Environnement, Traces de vie ancienne, Colons, Direction artistique, La chaleur qui s'installe (+1 more)

### Community 96 - "packages-lock (3)"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, depth, source, version (+8 more)

### Community 97 - "packages-lock (4)"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 98 - "packages-lock (5)"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 100 - "ControlPanelAnimator"
Cohesion: 0.25
Nodes (4): ControlPanelAnimator, HologramScreen, IsBusy, IsPowered

### Community 101 - "DeathScreenUI"
Cohesion: 0.14
Nodes (5): SceneLoadingManager, DeathScreenUI, Instance, IsOpen, Écran de mort

### Community 102 - "CameraManager"
Cohesion: 0.19
Nodes (6): CameraManager, Camera, Instance, Target, CameraContext, CameraManager

### Community 103 - "dome_build.py"
Cohesion: 0.26
Nodes (12): build_base(), build_frame(), build_glass(), build_wall(), curved_slab(), dome_sphere(), make_mats(), node() (+4 more)

### Community 104 - "Historique - Refontes"
Cohesion: 0.07
Nodes (29): EquipmentData, Slot, EquipmentSlot, Back, Body, Feet, Hands, Head (+21 more)

### Community 105 - "ObjectSpin"
Cohesion: 0.17
Nodes (5): ObjectSpin, MotionType, Rotation, SearchLight, Translation

### Community 107 - "CraftingStation"
Cohesion: 0.15
Nodes (11): CraftingStation, Current, IsUsable, Output, Power, Progress, Queue, Recipes (+3 more)

### Community 109 - "PlayerInputActions (4)"
Cohesion: 0.26
Nodes (4): @Camera, CameraActions, enabled, @Rotation

### Community 110 - "Déplacement du joueur.md"
Cohesion: 0.20
Nodes (4): B03 - Décollage en haut des pentes, B07 - Ressenti des déplacements, F03 - Accroupissement, R07 - CharacterController

### Community 111 - "EnergyStorage (2)"
Cohesion: 0.15
Nodes (11): EnergyStorage, Capacity, Charge, Discharge, InputRate, OutputRate, SaveKey, State (+3 more)

### Community 112 - "StatBarUI"
Cohesion: 0.24
Nodes (4): PlayerStatsUI, StatBarUI, StatUIData, Stats du joueur

### Community 113 - "packages-lock (6)"
Cohesion: 0.13
Nodes (14): dependencies, depth, source, url, version, dependencies, depth, source (+6 more)

### Community 114 - "packages-lock (7)"
Cohesion: 0.14
Nodes (14): dependencies, depth, source, version, dependencies, depth, source, url (+6 more)

### Community 115 - "CorridorData"
Cohesion: 0.15
Nodes (12): CorridorData, ExtensionLength, ExtensionPiece, MaxExtension, MaxLength, MinExtensionLength, MinLength, StraightLength (+4 more)

### Community 116 - "LandingCapsuleAnimator"
Cohesion: 0.18
Nodes (4): LandingCapsuleAnimator, IsBusy, IsOpen, Capsule de départ

### Community 117 - "Intention et piliers du style"
Cohesion: 0.67
Nodes (3): Intention, Intention et piliers du style, Les piliers du style

### Community 118 - "Pickupable"
Cohesion: 0.18
Nodes (6): Pickupable, SaveKey, SaveState, ISaveableComponent, SaveKey, Sauvegarde

### Community 119 - "FlashlightBeam"
Cohesion: 0.33
Nodes (3): FlashlightBeam, FlashlightAction, Lampe torche

### Community 120 - "Technologie humaine"
Cohesion: 0.20
Nodes (10): Dômes de verre et tunnels, La capsule, Outils du joueur, Principes, Production, Rations, Robots et IA, Serres (+2 more)

### Community 121 - "wt_build"
Cohesion: 0.32
Nodes (11): animate(), fold(), mast(), build_base(), build_blade(), build_mast(), build_nacelle(), build_rotor() (+3 more)

### Community 122 - "TMPro"
Cohesion: 0.18
Nodes (3): TMP_DigitValidator, TMP_PhoneNumberValidator, TMPro

### Community 123 - "InteractableOutline"
Cohesion: 0.21
Nodes (4): ShaderPropAnimator, WallScreenAnimator, IsPowered, ScreenSurface

### Community 124 - "CameraMovement"
Cohesion: 0.18
Nodes (5): CameraMovement, Pitch, Target, Yaw, Caméra

### Community 125 - "Label"
Cohesion: 0.28
Nodes (4): ClockUI, ItemTooltip, F54 - Cycle jour nuit et météo, Horloge

### Community 126 - "Fonctionnalités implémentées"
Cohesion: 0.20
Nodes (10): Bootstrap, Construction, Crafting, Fonctionnalités implémentées, Interaction, Interfaces, Inventaire, Joueur (+2 more)

### Community 127 - "BuildingInterior"
Cohesion: 0.19
Nodes (5): BuildingInterior, Radius, Zone, Roof, Intérieur des bâtiments

### Community 128 - "manifest"
Cohesion: 0.17
Nodes (11): com.unity.modules.adaptiveperformance@1.0.0, com.unity.modules.assetbundle@1.0.0, com.unity.modules.imageconversion@1.0.0, com.unity.modules.vr@1.0.0, com.unity.timeline@1.8.12, com.unity.modules.assetbundle, com.unity.modules.imageconversion, com.unity.collab-proxy (+3 more)

### Community 129 - "manifest (2)"
Cohesion: 0.17
Nodes (12): dependencies, com.unity.collab-proxy@2.12.4, com.unity.modules.accessibility@1.0.0, com.unity.modules.cloth@1.0.0, com.unity.modules.ui@1.0.0, com.unity.modules.unitywebrequest@1.0.0, com.unity.modules.unitywebrequestaudio@1.0.0, com.unity.modules.ui (+4 more)

### Community 130 - "packages-lock (8)"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 131 - "al_anim"
Cohesion: 0.62
Nodes (5): build(), doors(), fans(), key(), state()

### Community 132 - "TerrainFlattening"
Cohesion: 0.38
Nodes (3): TerrainEditSaveData, TerrainFlattening, Terrain sous les constructions

### Community 133 - "SolarPanel"
Cohesion: 0.14
Nodes (5): EnergyDeviceInfo, SolarPanel, Generation, F49 - Génération d'énergie, Panneau solaire

### Community 134 - "LandingCapsule"
Cohesion: 0.18
Nodes (7): LandingCapsule, IsOpen, SaveKey, SaveState, LandingCapsuleDoor, Capsule, F65 - Intégration des modèles 3D

### Community 136 - "packages-lock (9)"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 137 - "packages-lock (10)"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 138 - "packages-lock (11)"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 139 - "zfight (2)"
Cohesion: 0.39
Nodes (5): geodesic_sphere(), hemisphere(), icosahedron(), orient_matrix(), stats()

### Community 140 - "CameraController"
Cohesion: 0.22
Nodes (5): CameraController, CameraModes, Follow, Free, Isometric

### Community 141 - "TerrainMask"
Cohesion: 0.19
Nodes (5): TerrainMask, Matrix, Shape, Box, Cylinder

### Community 144 - "NotificationUI"
Cohesion: 0.20
Nodes (7): NotificationType, Info, Warning, NotificationUI, Instance, Message, Messages au joueur

### Community 145 - "InterfaceManager.GetInstance"
Cohesion: 0.13
Nodes (8): Blocked, PauseMenuUI, Instance, IsOpen, InterfaceManager, HasOpenInterface, Menu pause, Modes de jeu et interfaces

### Community 146 - "À faire"
Cohesion: 0.25
Nodes (8): Bugs, Features, Historique, Identifiants, Optimisations, Prochains numéros libres, Refontes, À faire

### Community 147 - "EnergyGenerator"
Cohesion: 0.17
Nodes (10): DistributionPanelOutput, CanSwitch, DisplayName, Generation, Panel, PortId, EnergyGenerator, Generation (+2 more)

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

### Community 154 - "EnvironmentSpawner.Spawn"
Cohesion: 0.24
Nodes (3): EnvironmentSpawner, F31 - Structures générées, Objets du décor

### Community 155 - "Menaces et lisibilité"
Cohesion: 0.67
Nodes (3): Les menaces, Menaces et lisibilité, Règles de lisibilité

### Community 156 - "09 - Textures.md"
Cohesion: 0.29
Nodes (4): Livrables par asset, Production (Unity, développeur solo), Vitres, Textures

### Community 158 - "Builder"
Cohesion: 0.29
Nodes (7): Mode, Airlock, Building, Corridor, Destroying, None, Placing

### Community 160 - "BenchmarkType"
Cohesion: 0.22
Nodes (7): Benchmark03, BenchmarkType, TEXTMESH_BITMAP, TMP_BITMAP_MOBILE, TMP_SDF, TMP_SDF_MOBILE, TMP_SDF__MOBILE_SSD

### Community 161 - "MonoBehaviour (3)"
Cohesion: 0.33
Nodes (4): TMP_ExampleScript_01, objectType, TextMeshPro, TextMeshProUGUI

### Community 162 - "packages-lock (12)"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.collab-proxy

### Community 164 - "EnergyNetworkManager"
Cohesion: 0.40
Nodes (5): NetworkState, NotLinked, Powered, SwitchedOff, Tripped

### Community 165 - "ItemGaugeUnit"
Cohesion: 0.50
Nodes (4): ItemGaugeUnit, Energy, Oxygen, Wear

### Community 166 - "packages-lock (13)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.umbra

### Community 167 - "packages-lock (14)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

### Community 168 - "ToolData.cs"
Cohesion: 0.50
Nodes (3): ToolConsumptionMode, Continuous, PerUse

### Community 170 - "Direction"
Cohesion: 0.50
Nodes (3): Direction, In, Out

### Community 171 - "Sommaire"
Cohesion: 0.50
Nodes (4): Construction des modèles, Méthode technique, Pipeline 3D (Blender → Unity), Sommaire

### Community 172 - "ChunkManager (singleton, scène Monde)"
Cohesion: 0.67
Nodes (3): ChunkManager (singleton, scène Monde), Terrain procédural, TerrainGenerator (sur le prefab `Chunk`)

## Knowledge Gaps
- **421 isolated node(s):** `Mode`, `Mode`, `Hold`, `Press`, `Toggle` (+416 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 919 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **59 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Airlock` connect `Airlock & Corridor Structures` to `IInterractable.GetInfo`, `Builder Placement`, `GameObject`, `Airlock Animator`, `Vector3`, `LandingCapsule`, `MonoBehaviour`, `system`, `add_revolve`, `StructureBlueprint`, `Pickupable`, `StructureData`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.133) - this node is a cross-community bridge._
- **Are the 7 inferred relationships involving `Builder` (e.g. with `Historique - Bugs` and `Historique - Optimisations`) actually correct?**
  _`Builder` has 7 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Mode`, `Mode`, `Hold` to the rest of the system?**
  _421 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `system_collections_generic` be split into smaller, more focused modules?**
  _Cohesion score 0.06881720430107527 - nodes in this community are weakly interconnected._
- **Why does `Modélisation Blender` connect `add_revolve` to `SolarPanel`, `Airlock & Corridor Structures`, `Airlock Blender Build`, `Blender Build Scripts`, `00 - Index Pipeline 3D.md`, `Tunnel Blender Build`?**
  _High betweenness centrality (0.126) - this node is a cross-community bridge._
- **Should `PlayerInventory` be split into smaller, more focused modules?**
  _Cohesion score 0.08345428156748912 - nodes in this community are weakly interconnected._
- **Why does `add_revolve()` connect `add_revolve` to `wt_build`, `dome_build.py`, `st_build`, `Airlock Blender Build`, `Blender Build Scripts`, `Landing Capsule Blender Build`, `dr_build`, `st_build (2)`?**
  _High betweenness centrality (0.052) - this node is a cross-community bridge._