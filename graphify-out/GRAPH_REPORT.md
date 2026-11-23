# Graph Report - Space Survival  (2026-11-23)

## Corpus Check
- 393 files · ~180,464 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1568 file(s) not represented in the graph (top: .meta 966, .mat 205, .asset 94)
- WARNING: 1568 of 2043 files seen (77%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 3675 nodes · 9091 edges · 205 communities (151 shown, 54 thin omitted)
- Extraction: 86% EXTRACTED · 14% INFERRED · 0% AMBIGUOUS · INFERRED: 1313 edges (avg confidence: 0.89)
- Token cost: 497,038 input · 0 output

## Graph Freshness
- Built from commit: `535d47c8`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- 00 - Index À faire.md
- PlayerInventory
- Coal Generator Animator
- TMP_Text
- Builder Placement
- Transform
- ItemSlot
- ConnectorPreview.Destroy
- Corridor
- ItemCollectionUI
- Distribution Panel Core
- CameraMovement
- Save System
- ISaveableComponent
- add_revolve
- Energy Network Manager
- Network Map UI
- system_collections
- oct_cyl
- Landing Capsule Blender Build
- CablePlacer
- Package Lock A
- Drill
- Interfaces UI Toolkit.md
- Map Utility & Minimap
- Package Lock B
- Package Lock C
- Tunnel Blender Build
- ElectricCable
- Package Lock D
- Input Actions
- GameObject
- ModelImportSetup
- IInterractable.GetInfo
- CraftingStationUI
- Terrain Chunks
- CallbackContext
- Airlock Animator
- Distribution Panel UI
- st_build
- TextMeshProFloatingText
- CraftingStationAnimator
- Airlock Blender Build
- system
- TMP Text Event Handler
- UI Manager
- EnergyDevice
- PlayingMode
- cp_build.py
- Vector3
- PlayerInputActions
- TMP Text Selectors
- SolarPanelAnimator
- Main Menu UI
- 00 - Index Pipeline 3D.md
- st_build (2)
- DrillAnimator
- SaveableObject
- Label
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
- UIDocument
- ws_build.py
- PlayerModeOptions
- RelayAntennaAnimator
- PlayerManager
- ControlPanel
- ParentedUI
- packages-lock (2)
- Scripts de référence
- TMP_TextInfoDebugTool
- BatteryAnimator
- WindTurbineAnimator
- CraftingStation
- cs_anim
- dr_build
- VertexJitter
- PlayerInputActions (3)
- PickupInfoGroupUI
- EnergyConsumer
- StorageContainer
- ItemStorage
- Stat (2)
- 00 - Index Game Design.md
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
- SubclassPicker.cs
- EnergyStorage
- StatBarUI
- com.unity.performance.profile-analyzer@1.3.3
- packages-lock (7)
- InterfaceManager
- LandingCapsuleAnimator
- Intention et piliers du style
- TerrainMask
- SceneLoadingManager
- Technologie humaine
- wt_build
- TMPro
- TerrainGenerator
- StorageAnimator
- BenchmarkType
- Fonctionnalités implémentées
- WallScreenAnimator
- com.unity.modules.adaptiveperformance@1.0.0
- dependencies
- packages-lock (8)
- al_anim
- SheddingMode
- StatName
- ItemStorageUI
- BuildingInterior
- dependencies
- packages-lock (11)
- zfight (2)
- CameraController
- Game design
- ItemGaugeUnit
- NotificationUI
- PauseMenuUI
- À faire
- Objets persistants
- À éviter et check-list
- Prompts IA
- Bible de direction artistique : [NOM DU JEU]
- DistributionPanel
- com.unity.settings-manager
- Menaces et lisibilité
- PlayerInventory
- com.unity.modules.ai
- VertexZoom
- com.unity.modules.androidjni
- com.coplaydev.unity-mcp@https://github.com/CoplayDev/unity-mc...
- com.unity.collab-proxy@2.12.4
- EnergyNetworkManager
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
- Ressources vitales
- manifest (22)
- Fiche du jeu
- manifest (24)
- manifest (25)
- Progression de la partie
- manifest (27)
- manifest (28)
- Mode
- manifest (30)
- manifest (31)
- manifest (32)
- Points ouverts du game design
- Space Survival (nom provisoire)
- TMP_ExampleScript_01
- Outils scientifiques
- Instructions
- CLAUDE

## God Nodes (most connected - your core abstractions)
1. `Builder` - 90 edges
2. `DistributionPanel` - 78 edges
3. `EnergyDevice` - 76 edges
4. `SaveManager` - 70 edges
5. `PlayerInventory` - 66 edges
6. `EnergyNetworkManager` - 58 edges
7. `Historique - Features` - 53 edges
8. `Item` - 52 edges
9. `CoalGeneratorAnimator` - 51 edges
10. `Dome` - 48 edges

## Surprising Connections (you probably didn't know these)
- `Inventaire du joueur` --references--> `PlayerInventory`  [INFERRED]
  obsidian/Fonctionnalités/Inventaire/Inventaire du joueur.md → Assets/Scripts/Behaviours/Entities/Player/PlayerInventory.cs
- `DistributionPanelUI` --references--> `DistributionPanelSettingsView`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Tableau de distribution.md → Assets/Scripts/Behaviours/UI/Energy/DistributionPanelSettingsView.cs
- `R13 - Snapping entre points de connexion` --references--> `Builder`  [INFERRED]
  obsidian/A faire/R13 - Snapping entre points de connexion.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs
- `ElectricCable` --references--> `Builder`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Câble électrique.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs
- `Contour de l'objet pointé` --references--> `InteractableOutline`  [INFERRED]
  obsidian/Fonctionnalités/Interaction/Contour de l'objet pointé.md → Assets/Scripts/Behaviours/Camera/InteractableOutline.cs

## Import Cycles
- None detected.

## Communities (205 total, 54 thin omitted)

### Community 0 - "00 - Index À faire.md"
Cohesion: 0.06
Nodes (23): B03 - Décollage en haut des pentes, B07 - Ressenti des déplacements, B10 - Pivot de ReplaceObject trop haut, B21 - Curseur visible après Échap, B26 - Flag identified des objets ramassables, F03 - Accroupissement, F06 - Vrai glisser-déposer, F17 - Contour masqué par les obstacles (+15 more)

### Community 2 - "PlayerInventory"
Cohesion: 0.12
Nodes (22): HeldAction, AbstractModifier, EnduranceModifier, LifeModifier, StatModifier, ConsummableData, ApplyModifiersAction, HoldItemAction (+14 more)

### Community 3 - "Coal Generator Animator"
Cohesion: 0.07
Nodes (21): CoalGeneratorAnimator, ChimneyHeight, CollarRotation, IsBusy, IsHatchOpen, IsRunning, MinChimneyHeight, SmokePoint (+13 more)

### Community 4 - "TMP_Text"
Cohesion: 0.08
Nodes (6): ChatController, EnvMapAnimator, TeleType, TextConsoleSimulator, TMP_TextEventCheck, VertexColorCycler

### Community 5 - "Builder Placement"
Cohesion: 0.08
Nodes (5): Builder, CorridorStructure, Inventory, IsActive, Source

### Community 6 - "Transform"
Cohesion: 0.11
Nodes (9): ShaderPropAnimator, BuildUtility, BlueprintMaterial, CorridorPieces, Piece, OwnedMeshes, Historique - Refontes, Couloir (+1 more)

### Community 7 - "ItemSlot"
Cohesion: 0.11
Nodes (14): ItemCollection, MaxSize, Slots, ItemData, Description, Id, Model, Name (+6 more)

### Community 8 - "ConnectorPreview.Destroy"
Cohesion: 0.10
Nodes (8): ConnectorPreview, DomeConnector, MaskMatrix, Owner, Socket, MeshCutter, Connecteurs de dôme, Dômes

### Community 9 - "Corridor"
Cohesion: 0.06
Nodes (20): Airlock, All, DoorsBusy, IsBuilt, SaveKey, State, Corridor, All (+12 more)

### Community 10 - "ItemCollectionUI"
Cohesion: 0.15
Nodes (7): ItemCollectionUI, SlotsUI, ItemSlotUI, Element, PlayerInventoryContentUI, PlayerToolBarUI, Clics de l'inventaire

### Community 11 - "Distribution Panel Core"
Cohesion: 0.04
Nodes (43): DistributionPanel, BatteryChargeSupplied, BatteryThreshold, Building, ChargeRequest, ChargeRoom, ChargeSupplied, ConsumedEnergy (+35 more)

### Community 12 - "CameraMovement"
Cohesion: 0.10
Nodes (7): CameraManager, Camera, Instance, Target, CameraContext, CameraManager, InteractableOutline

### Community 13 - "Save System"
Cohesion: 0.09
Nodes (13): PlayerSaveData, SaveData, StatSaveData, WorldTimeSaveData, SaveManager, HasSave, Instance, IsUnloading (+5 more)

### Community 14 - "ISaveableComponent"
Cohesion: 0.22
Nodes (4): EnvironmentSpawner, F31 - Structures générées, Sauvegarde, Objets du décor

### Community 15 - "add_revolve"
Cohesion: 0.12
Nodes (29): build_airlock_body(), densify(), add_box(), add_cyl(), add_revolve(), add_screw(), finish(), zone_mats() (+21 more)

### Community 16 - "Energy Network Manager"
Cohesion: 0.16
Nodes (3): EnergyNetworkManager, HasInstance, Instance

### Community 19 - "oct_cyl"
Cohesion: 0.14
Nodes (26): beam(), oct_cyl(), pill(), animate(), arm_k(), mast_k(), build_arm(), build_base() (+18 more)

### Community 20 - "Landing Capsule Blender Build"
Cohesion: 0.13
Nodes (24): animate(), build_body(), build_door(), build_door_frames(), build_exterior(), q(), build_interior(), q() (+16 more)

### Community 21 - "CablePlacer"
Cohesion: 0.12
Nodes (5): CablePlacer, Inventory, Source, BuildModeUI, Instance

### Community 22 - "Package Lock A"
Cohesion: 0.08
Nodes (28): dependencies, depth, source, version, dependencies, depth, source, version (+20 more)

### Community 23 - "Drill"
Cohesion: 0.13
Nodes (9): Drill, CanSwitchMachine, Gauge, GaugeText, IsFull, MachineName, MachineOn, Ore (+1 more)

### Community 24 - "Interfaces UI Toolkit.md"
Cohesion: 0.13
Nodes (7): R01 - Refonte du chargement de scènes, R02 - Persistance entre scènes, R09 - Système d'interaction extensible, R15 - Ids uniques des objets de scène, Code Unity, Point ouvert, Sommaire

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
Cohesion: 0.17
Nodes (25): offset_poly(), add_body(), add_flange(), add_rib(), f_dir(), glass_poly(), hollow_sweep(), pane() (+17 more)

### Community 29 - "ElectricCable"
Cohesion: 0.10
Nodes (12): ElectricCable, End, SaveKey, Start, State, CableItemData, Material, MetersPerItem (+4 more)

### Community 30 - "Package Lock D"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, version, dependencies, depth, source, version (+26 more)

### Community 31 - "Input Actions"
Cohesion: 0.08
Nodes (8): @PlayerInputActions, asset, bindingMask, bindings, controlSchemes, devices, ICameraActions, R06 - PlayerInputActions jamais libérés

### Community 32 - "GameObject"
Cohesion: 0.07
Nodes (14): AirlockDoor, Airlock, Outer, EnergyDeviceInfo, Interractable, Searcher, CurrentSearchedStorage, IInterractable (+6 more)

### Community 33 - "ModelImportSetup"
Cohesion: 0.17
Nodes (3): ModelImportSetup, Textures, Export FBX

### Community 36 - "CraftingStationUI"
Cohesion: 0.20
Nodes (4): CraftingStationUI, RecipeSlotUI, Element, Recipe

### Community 37 - "Terrain Chunks"
Cohesion: 0.18
Nodes (4): ChunkManager, Instance, Seed, TerrainEdits

### Community 39 - "Airlock Animator"
Cohesion: 0.19
Nodes (6): AirlockAnimator, InnerOpen, IsBusy, OuterOpen, Pressurized, Scripts d'animation C# (runtime)

### Community 40 - "Distribution Panel UI"
Cohesion: 0.14
Nodes (4): DistributionPanelSettingsView, Target, DistributionPanelUI, Instance

### Community 41 - "st_build"
Cohesion: 0.16
Nodes (17): animate(), build_body(), build_chimney_base(), build_collar(), build_fan(), build_hatch(), build_head(), build_hopper() (+9 more)

### Community 42 - "TextMeshProFloatingText"
Cohesion: 0.15
Nodes (7): Benchmark01, Benchmark02, Benchmark03, Benchmark04, SimpleScript, TextMeshProFloatingText, TextMeshSpawner

### Community 43 - "CraftingStationAnimator"
Cohesion: 0.23
Nodes (4): CraftingStationAnimator, HologramScreen, IsPowered, IsPrinting

### Community 44 - "Airlock Blender Build"
Cohesion: 0.17
Nodes (12): build_connector(), build_fan(), build_leaf(), dome_surface_y(), door_leaf_poly(), finish2(), fix_normals(), hollow_y() (+4 more)

### Community 45 - "system"
Cohesion: 0.08
Nodes (3): ToolConsumptionMode, Continuous, PerUse

### Community 46 - "TMP Text Event Handler"
Cohesion: 0.12
Nodes (11): TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, onCharacterSelection, onLineSelection, onLinkSelection, onSpriteSelection (+3 more)

### Community 47 - "UI Manager"
Cohesion: 0.12
Nodes (15): UIManager, ev_updatePlayerContext, Instance, PlayerContext, PlayerInventory, PlayerStats, UI, ParentUI (+7 more)

### Community 48 - "EnergyDevice"
Cohesion: 0.11
Nodes (13): DisplayName, EnergyDevice, Cables, CanSwitch, ConnectionPoint, DeviceState, DisplayName, ForcedOff (+5 more)

### Community 49 - "PlayingMode"
Cohesion: 0.15
Nodes (9): ModeManager, Instance, Mode, PlayingMode, Build, Default, Interface, Menu (+1 more)

### Community 50 - "cp_build.py"
Cohesion: 0.14
Nodes (15): animate(), build_body(), build_hologram(), sbox(), sp(), build_projector(), cham(), clear() (+7 more)

### Community 51 - "Vector3"
Cohesion: 0.11
Nodes (8): Dome, ConnectorPrefab, Connectors, InnerWallRadius, Model, Radius, Structure, DomeConnection

### Community 52 - "PlayerInputActions"
Cohesion: 0.14
Nodes (11): @Actions, ActionsActions, @Build, @ChangeHandedItem, @CloseInterfaces, @Drop, enabled, @Interract (+3 more)

### Community 54 - "SolarPanelAnimator"
Cohesion: 0.12
Nodes (8): SolarPanelAnimator, DeployProgress, HeadingAngle, IsDeployed, IsRotating, PanelFront, PanelUp, F65 - Intégration des modèles 3D

### Community 55 - "Main Menu UI"
Cohesion: 0.14
Nodes (7): SceneLoader, MainMenuUI, SaveSlotInfo, Fonctionnement actuel, Scènes, Scènes et chargement, Menu principal

### Community 56 - "00 - Index Pipeline 3D.md"
Cohesion: 0.11
Nodes (13): Construction des modèles, Méthode technique, Pipeline 3D (Blender → Unity), Sommaire, Façon de travailler, Palette des matériaux, Dimensions de référence, Stockage et empilage (+5 more)

### Community 57 - "st_build (2)"
Cohesion: 0.20
Nodes (13): build(), cabinet_body(), cabinet_doors(), cham(), clear(), collisions(), crate_body(), crate_lids() (+5 more)

### Community 58 - "DrillAnimator"
Cohesion: 0.18
Nodes (5): DrillAnimator, DustPoint, IsBusy, IsDrilling, Powered

### Community 59 - "SaveableObject"
Cohesion: 0.13
Nodes (10): SaveId, SaveableObject, Id, IsRegistered, IsRuntime, PrefabKey, ComponentSaveData, ChunkManager (singleton, scène Monde) (+2 more)

### Community 61 - "dependencies"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, url (+18 more)

### Community 62 - "bmesh"
Cohesion: 0.42
Nodes (5): check(), check2(), faces_of(), islands_of(), overlap_area()

### Community 63 - "TMP_UiFrameRateCounter"
Cohesion: 0.12
Nodes (8): Benchmark01_UGUI, DropdownSample, TMP_UiFrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

### Community 64 - "TMP_FrameRateCounter"
Cohesion: 0.13
Nodes (12): TMP_FrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight, TMPro_InstructionOverlay, FpsCounterAnchorPositions (+4 more)

### Community 65 - "PlayerContext"
Cohesion: 0.15
Nodes (8): Pickuper, PlayerContext, PlayerManager, PointedObject, PlayerInterractor, PlayerPointer, Source, Historique - Optimisations

### Community 66 - "DayNightCycle"
Cohesion: 0.06
Nodes (21): ObjectSpin, MotionType, Rotation, SearchLight, Translation, FlashlightBeam, DayNightCycle, Day (+13 more)

### Community 67 - "CameraMovement"
Cohesion: 0.18
Nodes (5): CameraMovement, Pitch, Target, Yaw, Caméra

### Community 68 - "Pose et destruction.md"
Cohesion: 0.23
Nodes (4): R12 - Intégration des constructions au terrain, R13 - Snapping entre points de connexion, Base de dômes, Dans le jeu aujourd'hui

### Community 69 - "BuildingPowerPanel"
Cohesion: 0.10
Nodes (16): BuildingOutlet, Building, OptionalDemand, SaveKey, BuildingPanelTerminal, Panel, BuildingPowerPanel, Building (+8 more)

### Community 70 - "PlayerMovement"
Cohesion: 0.13
Nodes (5): PlayerMovement, forwardDirection, IsSprinting, Stats, Déplacement du joueur

### Community 71 - "WeatherSystem"
Cohesion: 0.07
Nodes (20): WeatherSystem, AmbientFactor, Instance, IsStorm, IsStormComing, Seed, SkyFactor, SolarFactor (+12 more)

### Community 72 - "UIDocument"
Cohesion: 0.23
Nodes (5): ClockUI, HeldItemUI, HeldItem, F54 - Cycle jour nuit et météo, Horloge

### Community 73 - "ws_build.py"
Cohesion: 0.20
Nodes (11): arc_pt(), build_body(), build_screen(), clear(), rect(), rho(), screen_uv(), setup() (+3 more)

### Community 74 - "PlayerModeOptions"
Cohesion: 0.14
Nodes (12): SubclassPicker, PlayerModeOptions, Options, Commentaires, Composants, Conventions de code, Gameplay et modèles, Nommage (+4 more)

### Community 75 - "RelayAntennaAnimator"
Cohesion: 0.20
Nodes (4): RelayAntennaAnimator, IsAntennaExtended, IsDishDeployed, IsRotating

### Community 76 - "PlayerManager"
Cohesion: 0.12
Nodes (10): PlayerManager, Instance, Inventory, IsDead, Player, SpawnedFromSave, Stats, ViewSource (+2 more)

### Community 77 - "ControlPanel"
Cohesion: 0.09
Nodes (15): BuiltStructure, CostMultiplier, Data, ControlPanel, Consumer, DisplayName, IsPowered, Queue (+7 more)

### Community 78 - "ParentedUI"
Cohesion: 0.11
Nodes (3): PointedInterractorInformationUI, HandedCursorUI, ParentedUI

### Community 79 - "packages-lock (2)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 80 - "Scripts de référence"
Cohesion: 0.14
Nodes (22): prism_y(), aim(), bank_body(), belt(), boss(), build(), cabinet_body(), clear() (+14 more)

### Community 82 - "BatteryAnimator"
Cohesion: 0.20
Nodes (6): BatteryAnimator, IsCharging, IsPowered, Level, BatteryIndicator, Batteries

### Community 83 - "WindTurbineAnimator"
Cohesion: 0.09
Nodes (10): WindTurbineAnimator, IsDeployed, WindStrength, SolarPanel, Generation, WindTurbine, Generation, Protecting (+2 more)

### Community 85 - "CraftingStation"
Cohesion: 0.09
Nodes (18): Crafter, CraftingStation, Current, IsUsable, Output, Power, Progress, Recipes (+10 more)

### Community 86 - "cs_anim"
Cohesion: 0.33
Nodes (13): build_anim(), clear(), head_clearance(), ik(), k_arm(), k_head(), k_laser(), k_plate() (+5 more)

### Community 87 - "dr_build"
Cohesion: 0.19
Nodes (14): animate(), build_body(), build_carriage(), build_head(), build_hologram(), sbox(), sp(), collisions() (+6 more)

### Community 88 - "VertexJitter"
Cohesion: 0.09
Nodes (4): VertexJitter, VertexAnim, VertexShakeA, VertexShakeB

### Community 89 - "PlayerInputActions (3)"
Cohesion: 0.22
Nodes (6): @Movement, MovementActions, enabled, @Jump, @Move, @Sprint

### Community 90 - "PickupInfoGroupUI"
Cohesion: 0.31
Nodes (3): PickupInfoGroupUI, Group, PickupInfoUI

### Community 91 - "EnergyConsumer"
Cohesion: 0.11
Nodes (9): EnergyConsumer, Consumption, Demand, IsPowered, State, TestMachine, Items du réseau, Objets de test (+1 more)

### Community 92 - "StorageContainer"
Cohesion: 0.06
Nodes (16): State, StorageContainer, Above, Below, SaveKey, Stackable, StackPoint, StackSize (+8 more)

### Community 93 - "ItemStorage"
Cohesion: 0.07
Nodes (19): ChargingStation, Consumption, Storage, Equipment, DeathBag, ItemSlotSaveData, Item, Data (+11 more)

### Community 94 - "Stat (2)"
Cohesion: 0.12
Nodes (10): PlayerStats, SerializableKeyValuePair, Key, Value, Stat, MaxValue, Value, StatMarker (+2 more)

### Community 95 - "00 - Index Game Design.md"
Cohesion: 0.06
Nodes (42): F30 - Biomes, F47 - Relais radio, Formes et silhouettes, Couleur et lumière, Extérieur, par zone, Intérieur des dômes, Le cycle jour / nuit est un outil émotionnel, Matériaux et textures (+34 more)

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
Cohesion: 0.22
Nodes (5): ControlPanelAnimator, HologramScreen, IsBusy, IsPowered, Modèle

### Community 101 - "DeathScreenUI"
Cohesion: 0.23
Nodes (4): DeathScreenUI, Instance, IsOpen, Écran de mort

### Community 102 - "PlaceableItemData"
Cohesion: 0.14
Nodes (14): PlacedObject, Data, IPlacementPreview, PreviewModel, PlaceableItemData, NotInCorridors, OutdoorOnly, PlacedPrefab (+6 more)

### Community 103 - "StructureData"
Cohesion: 0.06
Nodes (32): BuildWheelUI, Instance, IsOpen, ObjectSaveData, AirlockData, SnapDistance, CorridorData, ExtensionLength (+24 more)

### Community 104 - "Historique - Refontes"
Cohesion: 0.07
Nodes (33): Pickupable, SaveKey, SaveState, EquipmentData, Slot, EquipmentSlot, Back, Body (+25 more)

### Community 105 - "TerrainEditSaveData"
Cohesion: 0.38
Nodes (3): TerrainEditSaveData, TerrainFlattening, Terrain sous les constructions

### Community 107 - "00 - Index Fonctionnalités.md"
Cohesion: 0.11
Nodes (6): F11 - Données propres à chaque exemplaire, F32 - Ressources générées, F37 - Oxygène hors des zones respirables, Objets posés, Contour de l'objet pointé, Inventaire du joueur

### Community 109 - "PlayerInputActions (4)"
Cohesion: 0.26
Nodes (4): @Camera, CameraActions, enabled, @Rotation

### Community 111 - "EnergyStorage"
Cohesion: 0.09
Nodes (19): EnergyGenerator, Generation, Output, Production, Network, EnergyStorage, Capacity, Charge (+11 more)

### Community 112 - "StatBarUI"
Cohesion: 0.26
Nodes (5): PlayerStatsUI, StatBarUI, StatUIData, R05 - Prefab asset modifié au runtime, Stats du joueur

### Community 113 - "com.unity.performance.profile-analyzer@1.3.3"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.performance.profile-analyzer@1.3.3, com.unity.performance.profile-analyzer

### Community 114 - "packages-lock (7)"
Cohesion: 0.14
Nodes (14): dependencies, depth, source, version, dependencies, depth, source, url (+6 more)

### Community 115 - "InterfaceManager"
Cohesion: 0.19
Nodes (8): InterfaceManager, HasOpenInterface, Modes de jeu et interfaces, Créer une interface, Interface du jeu (fenêtre qu'on ouvre), Mise en page, Textes, Éléments du HUD

### Community 116 - "LandingCapsuleAnimator"
Cohesion: 0.13
Nodes (10): LandingCapsuleAnimator, IsBusy, IsOpen, LandingCapsule, IsOpen, SaveKey, SaveState, LandingCapsuleDoor (+2 more)

### Community 117 - "Intention et piliers du style"
Cohesion: 0.67
Nodes (3): Intention, Intention et piliers du style, Les piliers du style

### Community 118 - "TerrainMask"
Cohesion: 0.29
Nodes (5): TerrainMask, Matrix, Shape, Box, Cylinder

### Community 120 - "Technologie humaine"
Cohesion: 0.20
Nodes (10): Dômes de verre et tunnels, La capsule, Outils du joueur, Principes, Production, Rations, Robots et IA, Serres (+2 more)

### Community 121 - "wt_build"
Cohesion: 0.32
Nodes (11): animate(), fold(), mast(), build_base(), build_blade(), build_mast(), build_nacelle(), build_rotor() (+3 more)

### Community 122 - "TMPro"
Cohesion: 0.20
Nodes (3): TMP_DigitValidator, TMP_PhoneNumberValidator, TMPro

### Community 124 - "StorageAnimator"
Cohesion: 0.16
Nodes (5): StorageAnimator, CanOpen, IsBusy, IsOpen, StackPoint

### Community 125 - "BenchmarkType"
Cohesion: 0.33
Nodes (6): BenchmarkType, TEXTMESH_BITMAP, TMP_BITMAP_MOBILE, TMP_SDF, TMP_SDF_MOBILE, TMP_SDF__MOBILE_SSD

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
Cohesion: 0.33
Nodes (7): build(), doors(), fans(), key(), state(), build_all(), build_size()

### Community 132 - "SheddingMode"
Cohesion: 0.50
Nodes (4): SheddingMode, BatteryBelow, None, Shortage

### Community 133 - "StatName"
Cohesion: 0.29
Nodes (6): StatName, Endurance, Health, Hunger, Oxygen, Thirst

### Community 135 - "ItemStorageUI"
Cohesion: 0.15
Nodes (9): ItemStorageUI, IStorageMachine, CanSwitchMachine, Gauge, GaugeText, MachineName, MachineOn, MachineState (+1 more)

### Community 136 - "BuildingInterior"
Cohesion: 0.18
Nodes (5): BuildingInterior, Radius, Zone, InteractionDistance, Intérieur des bâtiments

### Community 137 - "dependencies"
Cohesion: 0.10
Nodes (20): dependencies, depth, hash, source, version, dependencies, depth, source (+12 more)

### Community 138 - "packages-lock (11)"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 139 - "zfight (2)"
Cohesion: 0.33
Nodes (5): geodesic_sphere(), hemisphere(), icosahedron(), orient_matrix(), stats()

### Community 140 - "CameraController"
Cohesion: 0.22
Nodes (5): CameraController, CameraModes, Follow, Free, Isometric

### Community 141 - "Game design"
Cohesion: 0.25
Nodes (8): Colonie, Concept, Exploration et science, Game design, Légende, Monde, Survie, À décider

### Community 143 - "ItemGaugeUnit"
Cohesion: 0.40
Nodes (4): ItemGaugeUnit, Energy, Oxygen, Wear

### Community 144 - "NotificationUI"
Cohesion: 0.20
Nodes (7): NotificationType, Info, Warning, NotificationUI, Instance, Message, Messages au joueur

### Community 145 - "PauseMenuUI"
Cohesion: 0.14
Nodes (5): Blocked, PauseMenuUI, Instance, IsOpen, Menu pause

### Community 146 - "À faire"
Cohesion: 0.25
Nodes (8): Bugs, Features, Historique, Identifiants, Optimisations, Prochains numéros libres, Refontes, À faire

### Community 149 - "Objets persistants"
Cohesion: 0.15
Nodes (11): Bootstrapper, Root, SceneContext, Assets, Organisation des dossiers, Scripts (`Assets/Scripts/`), Objets persistants, Ce que l'IA vérifie (+3 more)

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

### Community 159 - "com.unity.modules.ai"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.ai

### Community 161 - "com.unity.modules.androidjni"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.androidjni

### Community 164 - "EnergyNetworkManager"
Cohesion: 0.40
Nodes (5): NetworkState, NotLinked, Powered, SwitchedOff, Tripped

### Community 167 - "packages-lock (14)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

### Community 170 - "Direction"
Cohesion: 0.50
Nodes (3): Direction, In, Out

### Community 180 - "manifest.json"
Cohesion: 0.17
Nodes (11): com.unity.modules.androidjni@1.0.0, com.unity.modules.audio@1.0.0, com.unity.modules.particlesystem@1.0.0, com.unity.modules.wind@1.0.0, com.unity.visualscripting@1.9.11, com.unity.modules.audio, com.unity.modules.particlesystem, com.unity.modules.androidjni (+3 more)

### Community 194 - "Ressources vitales"
Cohesion: 0.40
Nodes (5): Eau, Nourriture, Oxygène, Ressources vitales, Électricité

### Community 196 - "Fiche du jeu"
Cohesion: 0.40
Nodes (5): En bref, Fiche du jeu, Pitch, Production, Ton

### Community 199 - "Progression de la partie"
Cohesion: 0.40
Nodes (5): Au départ, Développement, Fin de partie, Progression de la partie, Ressenti

### Community 202 - "Mode"
Cohesion: 0.29
Nodes (7): Mode, Airlock, Building, Corridor, Destroying, None, Placing

### Community 207 - "Points ouverts du game design"
Cohesion: 0.50
Nodes (4): Décisions en attente, Points ouverts du game design, Questions relevées en documentant le concept *(proposition)*, Écarts entre le concept et le jeu actuel

### Community 208 - "Space Survival (nom provisoire)"
Cohesion: 0.50
Nodes (4): Décisions en attente, Les parties du coffre, Pour commencer, Space Survival (nom provisoire)

### Community 210 - "TMP_ExampleScript_01"
Cohesion: 0.33
Nodes (4): TMP_ExampleScript_01, objectType, TextMeshPro, TextMeshProUGUI

### Community 211 - "Outils scientifiques"
Cohesion: 0.67
Nodes (3): Outils scientifiques, Scanner, Table d'observation

### Community 212 - "Instructions"
Cohesion: 0.67
Nodes (3): Domaines, Instructions, Règles communes

## Knowledge Gaps
- **452 isolated node(s):** `Reserve`, `MachineName`, `Gauge`, `GaugeText`, `MachineOn` (+447 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1039 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **54 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Modélisation Blender` connect `add_revolve` to `Corridor`, `Airlock Blender Build`, `Scripts de référence`, `oct_cyl`, `WindTurbineAnimator`, `00 - Index Pipeline 3D.md`, `Tunnel Blender Build`?**
  _High betweenness centrality (0.113) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `Builder` (e.g. with `Historique - Bugs` and `Historique - Optimisations`) actually correct?**
  _`Builder` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Reserve`, `MachineName`, `Gauge` to the rest of the system?**
  _452 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `00 - Index À faire.md` be split into smaller, more focused modules?**
  _Cohesion score 0.05649717514124294 - nodes in this community are weakly interconnected._
- **Why does `Airlock` connect `Corridor` to `GameObject`, `IInterractable.GetInfo`, `Transform`, `Airlock Animator`, `ConnectorPreview.Destroy`, `StructureData`, `ItemCollectionUI`, `TextMeshProFloatingText`, `system`, `ControlPanel`, `add_revolve`, `Vector3`, `Objets persistants`, `SolarPanelAnimator`, `Map Utility & Minimap`, `StorageContainer`?**
  _High betweenness centrality (0.110) - this node is a cross-community bridge._
- **Should `system_collections_generic` be split into smaller, more focused modules?**
  _Cohesion score 0.0671602326811211 - nodes in this community are weakly interconnected._
- **Why does `@PlayerInputActions` connect `Input Actions` to `GameObject`, `CallbackContext`, `PlayerInputActions (4)`, `unityengine_inputsystem`, `PlayerInputActions`, `PlayerInputActions (3)`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.072) - this node is a cross-community bridge._