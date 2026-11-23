# Graph Report - Space Survival  (2026-11-23)

## Corpus Check
- 397 files · ~189,178 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1615 file(s) not represented in the graph (top: .meta 1000, .mat 211, .asset 94)
- WARNING: 1615 of 2129 files seen (76%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 3754 nodes · 9290 edges · 214 communities (159 shown, 55 thin omitted)
- Extraction: 85% EXTRACTED · 15% INFERRED · 0% AMBIGUOUS · INFERRED: 1352 edges (avg confidence: 0.89)
- Token cost: 497,038 input · 0 output

## Graph Freshness
- Built from commit: `b73bb7ba`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- 00 - Index À faire.md
- ConnectorPreview.Destroy
- PlayerInventory
- Coal Generator Animator
- TMP_Text
- Builder Placement
- GameObject
- ItemSlot
- Corridor
- Airlock
- Label
- Distribution Panel Core
- CameraManager
- Save System
- ISaveableComponent
- add_revolve
- Energy Network Manager
- Network Map UI
- system_collections
- pill
- Landing Capsule Blender Build
- CablePlacer
- Package Lock A
- Drill
- 00 - Index Code.md
- Map Utility & Minimap
- Package Lock B
- Package Lock C
- Tunnel Blender Build
- ElectricCable
- Package Lock D
- Input Actions
- IInterractable
- ItemCollectionUI
- IInterractable.GetInfo
- cbox
- CraftingStationUI
- Terrain Chunks
- CallbackContext
- Airlock Animator
- Distribution Panel UI
- Scripts de référence
- TextMeshProFloatingText
- CraftingStationAnimator
- Airlock Blender Build
- TMP_TextEventHandler
- UI Manager
- EnergyDevice
- ModeManager
- cp_build.py
- Vector3
- PlayerInputActions
- Données des items.md
- SolarPanelAnimator
- Main Menu UI
- 00 - Index Pipeline 3D.md
- st_build (2)
- DrillAnimator
- SaveableObject
- Item
- dependencies
- bmesh
- TMP_TextSelector_B
- TMP_FrameRateCounter
- PlayerContext
- DayNightCycle
- CameraMovement
- Pose et destruction.md
- BuildingPowerPanel
- PlayerMovement
- WeatherSystem
- InteractableOutline
- ws_build.py
- ControlPanelUI
- RelayAntennaAnimator
- PlayerManager
- ControlPanel
- ParentedUI
- packages-lock (2)
- Scripts de référence
- TMP_TextInfoDebugTool
- BatteryAnimator
- WindTurbineAnimator
- InterfaceManager
- CraftingStation
- cs_anim
- dr_build
- ShaderPropAnimator
- PlayerInputActions (3)
- PickupInfoGroupUI
- EnergyConsumer
- StorageContainer
- Stat (2)
- 00 - Index Game Design.md
- packages-lock (3)
- packages-lock (4)
- packages-lock (5)
- TMP_UiFrameRateCounter
- ControlPanelAnimator
- DeathScreenUI
- PlaceableItemData
- StructureData
- Données des items
- TerrainEditSaveData
- 00 - Index Fonctionnalités.md
- SubclassPicker
- PlayerInputActions (4)
- system
- EnergyStorage
- StatBarUI
- com.unity.performance.profile-analyzer@1.3.3
- packages-lock (7)
- SolarPanel
- LandingCapsuleAnimator
- DomeConnector
- TerrainMask
- FlashlightBeam
- Technologie humaine
- wt_build.py
- TMPro
- VertexJitter
- Pickupable
- BenchmarkType
- Fonctionnalités implémentées
- WallScreenAnimator
- com.unity.modules.adaptiveperformance@1.0.0
- dependencies
- packages-lock (8)
- al_anim
- CorridorData
- PlayerStats
- PlayerModeOptions
- SaveManager.BeforeSceneChange
- ObjectSpin
- dependencies
- packages-lock (11)
- zfight (2)
- CameraController
- Game design
- MonoBehaviour
- NotificationUI
- PauseMenuUI
- À faire
- Objets persistants
- SceneLoadingManager
- Couleur et lumière
- ToolData.cs
- VertexShakeA
- DistributionPanel
- com.unity.settings-manager
- Menaces et lisibilité
- VertexShakeB
- PlayerInventory
- TMP_ExampleScript_01
- com.unity.modules.ai
- À éviter et check-list
- com.unity.modules.androidjni
- com.coplaydev.unity-mcp@https://github.com/CoplayDev/unity-mc...
- com.unity.collab-proxy@2.12.4
- EnergyNetworkManager
- Prompts IA
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
- Space Survival (nom provisoire)
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
- ItemGaugeUnit
- Points ouverts du game design
- Bible de direction artistique : [NOM DU JEU]
- Intention et piliers du style
- 14 - Production Unity.md
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
- `Stockage et empilage` --references--> `solid()`  [INFERRED]
  obsidian/Instructions/Pipeline 3D/06 - Stockage et empilage.md → ArtPipeline/Blender/st_build.py

## Import Cycles
- None detected.

## Communities (214 total, 55 thin omitted)

### Community 0 - "00 - Index À faire.md"
Cohesion: 0.07
Nodes (19): B03 - Décollage en haut des pentes, B07 - Ressenti des déplacements, B10 - Pivot de ReplaceObject trop haut, B21 - Curseur visible après Échap, B26 - Flag identified des objets ramassables, F03 - Accroupissement, F06 - Vrai glisser-déposer, F27 - Réseau d'oxygène (+11 more)

### Community 1 - "ConnectorPreview.Destroy"
Cohesion: 0.10
Nodes (3): MeshCutter, OwnedMeshes, Dômes

### Community 2 - "PlayerInventory"
Cohesion: 0.10
Nodes (26): SubclassPicker, AbstractModifier, EnduranceModifier, LifeModifier, StatModifier, ConsummableData, ApplyModifiersAction, HoldItemAction (+18 more)

### Community 3 - "Coal Generator Animator"
Cohesion: 0.05
Nodes (28): CoalGeneratorAnimator, ChimneyHeight, CollarRotation, IsBusy, IsHatchOpen, IsRunning, MinChimneyHeight, SmokePoint (+20 more)

### Community 4 - "TMP_Text"
Cohesion: 0.10
Nodes (5): EnvMapAnimator, TeleType, TextConsoleSimulator, TMP_TextEventCheck, VertexColorCycler

### Community 5 - "Builder Placement"
Cohesion: 0.07
Nodes (6): Structure, Builder, CorridorStructure, Inventory, IsActive, Source

### Community 6 - "GameObject"
Cohesion: 0.15
Nodes (7): BuildUtility, BlueprintMaterial, ObjectSaveData, Historique - Refontes, Structures et catalogue, L'objet, Règles communes

### Community 7 - "ItemSlot"
Cohesion: 0.08
Nodes (17): Pickuper, Equipment, ItemCollection, MaxSize, Slots, ItemData, Description, Id (+9 more)

### Community 8 - "Corridor"
Cohesion: 0.09
Nodes (16): IsBuilt, BuiltStructure, CostMultiplier, Data, Corridor, All, CostMultiplier, End (+8 more)

### Community 9 - "Airlock"
Cohesion: 0.09
Nodes (18): Airlock, All, DoorsBusy, SaveKey, State, StructureBlueprint, CostMultiplier, Data (+10 more)

### Community 10 - "Label"
Cohesion: 0.12
Nodes (8): ClockUI, HeldItemUI, HeldItem, ItemSlotUI, Element, ItemTooltip, F54 - Cycle jour nuit et météo, Horloge

### Community 11 - "Distribution Panel Core"
Cohesion: 0.05
Nodes (41): DistributionPanel, BatteryChargeSupplied, BatteryThreshold, Building, ChargeRequest, ChargeRoom, ChargeSupplied, ConsumedEnergy (+33 more)

### Community 12 - "CameraManager"
Cohesion: 0.19
Nodes (6): CameraManager, Camera, Instance, Target, CameraContext, CameraManager

### Community 13 - "Save System"
Cohesion: 0.10
Nodes (14): PlayerSaveData, SaveData, StatSaveData, WorldTimeSaveData, SaveManager, HasSave, Instance, IsUnloading (+6 more)

### Community 14 - "ISaveableComponent"
Cohesion: 0.24
Nodes (3): EnvironmentSpawner, F31 - Structures générées, Objets du décor

### Community 15 - "add_revolve"
Cohesion: 0.12
Nodes (27): add_box(), add_cyl(), add_revolve(), add_screw(), finish(), zone_mats(), build_holo(), sbox() (+19 more)

### Community 16 - "Energy Network Manager"
Cohesion: 0.15
Nodes (3): EnergyNetworkManager, HasInstance, Instance

### Community 19 - "pill"
Cohesion: 0.13
Nodes (26): beam(), pill(), prism_x(), animate(), arm_k(), mast_k(), build_arm(), build_base() (+18 more)

### Community 20 - "Landing Capsule Blender Build"
Cohesion: 0.13
Nodes (24): animate(), build_body(), build_door(), build_door_frames(), build_exterior(), q(), build_interior(), q() (+16 more)

### Community 21 - "CablePlacer"
Cohesion: 0.13
Nodes (6): CablePlacer, Blocked, Inventory, Source, BuildModeUI, Instance

### Community 22 - "Package Lock A"
Cohesion: 0.08
Nodes (28): dependencies, depth, source, version, dependencies, depth, source, version (+20 more)

### Community 23 - "Drill"
Cohesion: 0.13
Nodes (9): Drill, CanSwitchMachine, Gauge, GaugeText, IsFull, MachineName, MachineOn, Ore (+1 more)

### Community 24 - "00 - Index Code.md"
Cohesion: 0.11
Nodes (12): R01 - Refonte du chargement de scènes, R02 - Persistance entre scènes, R09 - Système d'interaction extensible, Code Unity, Point ouvert, Sommaire, Assets, Organisation des dossiers (+4 more)

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

### Community 29 - "ElectricCable"
Cohesion: 0.10
Nodes (12): ElectricCable, End, SaveKey, Start, State, CableItemData, Material, MetersPerItem (+4 more)

### Community 30 - "Package Lock D"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, version, dependencies, depth, source, version (+26 more)

### Community 31 - "Input Actions"
Cohesion: 0.08
Nodes (8): @PlayerInputActions, asset, bindingMask, bindings, controlSchemes, devices, ICameraActions, R06 - PlayerInputActions jamais libérés

### Community 32 - "IInterractable"
Cohesion: 0.09
Nodes (15): AirlockDoor, Airlock, Outer, EnergyDeviceInfo, Interractable, Searcher, CurrentSearchedStorage, IInterractable (+7 more)

### Community 33 - "ItemCollectionUI"
Cohesion: 0.14
Nodes (7): ItemCollectionUI, SlotsUI, ItemStorageUI, PlayerInventoryContentUI, PlayerToolBarUI, Clics de l'inventaire, Interface du jeu (fenêtre qu'on ouvre)

### Community 34 - "IInterractable.GetInfo"
Cohesion: 0.09
Nodes (8): MachineState, DistributionPanelOutput, CanSwitch, DisplayName, Generation, Panel, PortId, MachineState

### Community 35 - "cbox"
Cohesion: 0.17
Nodes (18): animate(), build_body(), build_chimney_base(), build_collar(), build_fan(), build_hatch(), build_head(), build_hopper() (+10 more)

### Community 36 - "CraftingStationUI"
Cohesion: 0.22
Nodes (4): CraftingStationUI, RecipeSlotUI, Element, Recipe

### Community 37 - "Terrain Chunks"
Cohesion: 0.13
Nodes (5): ChunkManager, Instance, Seed, TerrainEdits, TerrainGenerator

### Community 39 - "Airlock Animator"
Cohesion: 0.19
Nodes (5): AirlockAnimator, InnerOpen, IsBusy, OuterOpen, Pressurized

### Community 40 - "Distribution Panel UI"
Cohesion: 0.13
Nodes (4): DistributionPanelSettingsView, Target, DistributionPanelUI, Instance

### Community 41 - "Scripts de référence"
Cohesion: 0.06
Nodes (44): bake_positions(), bilinear(), blur(), build(), build_mesh(), build_textures(), fields(), lerp() (+36 more)

### Community 42 - "TextMeshProFloatingText"
Cohesion: 0.16
Nodes (6): Benchmark01, Benchmark02, Benchmark03, SimpleScript, TextMeshProFloatingText, TextMeshSpawner

### Community 43 - "CraftingStationAnimator"
Cohesion: 0.21
Nodes (4): CraftingStationAnimator, HologramScreen, IsPowered, IsPrinting

### Community 44 - "Airlock Blender Build"
Cohesion: 0.16
Nodes (15): build_airlock_body(), build_connector(), build_fan(), build_leaf(), densify(), dome_surface_y(), door_leaf_poly(), finish2() (+7 more)

### Community 46 - "TMP_TextEventHandler"
Cohesion: 0.12
Nodes (11): TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, onCharacterSelection, onLineSelection, onLinkSelection, onSpriteSelection (+3 more)

### Community 47 - "UI Manager"
Cohesion: 0.11
Nodes (15): UIManager, ev_updatePlayerContext, Instance, PlayerContext, PlayerInventory, PlayerStats, UI, ParentUI (+7 more)

### Community 48 - "EnergyDevice"
Cohesion: 0.10
Nodes (11): EnergyDevice, Cables, CanSwitch, ConnectionPoint, DeviceState, ForcedOff, IsOn, Panel (+3 more)

### Community 49 - "ModeManager"
Cohesion: 0.15
Nodes (9): ModeManager, Instance, Mode, PlayingMode, Build, Default, Interface, Menu (+1 more)

### Community 50 - "cp_build.py"
Cohesion: 0.14
Nodes (15): animate(), build_body(), build_hologram(), sbox(), sp(), build_projector(), cham(), clear() (+7 more)

### Community 51 - "Vector3"
Cohesion: 0.08
Nodes (13): BuildingInterior, Radius, Zone, Dome, ConnectorPrefab, Connectors, InnerWallRadius, Model (+5 more)

### Community 52 - "PlayerInputActions"
Cohesion: 0.14
Nodes (11): @Actions, ActionsActions, @Build, @ChangeHandedItem, @CloseInterfaces, @Drop, enabled, @Interract (+3 more)

### Community 53 - "Données des items.md"
Cohesion: 0.15
Nodes (5): F11 - Données propres à chaque exemplaire, F17 - Contour masqué par les obstacles, F20 - Recettes à plusieurs résultats, F56 - Horaires du tableau de distribution, R10 - Ids d'items générés

### Community 54 - "SolarPanelAnimator"
Cohesion: 0.13
Nodes (7): SolarPanelAnimator, DeployProgress, HeadingAngle, IsDeployed, IsRotating, PanelFront, PanelUp

### Community 55 - "Main Menu UI"
Cohesion: 0.17
Nodes (3): MainMenuUI, SaveSlotInfo, Menu principal

### Community 56 - "00 - Index Pipeline 3D.md"
Cohesion: 0.07
Nodes (18): ModelImportSetup, Construction des modèles, Méthode technique, Pipeline 3D (Blender → Unity), Sommaire, Façon de travailler, Palette des matériaux, Dimensions de référence (+10 more)

### Community 57 - "st_build (2)"
Cohesion: 0.20
Nodes (13): build(), cabinet_body(), cabinet_doors(), cham(), clear(), collisions(), crate_body(), crate_lids() (+5 more)

### Community 58 - "DrillAnimator"
Cohesion: 0.18
Nodes (5): DrillAnimator, DustPoint, IsBusy, IsDrilling, Powered

### Community 59 - "SaveableObject"
Cohesion: 0.13
Nodes (10): SaveId, SaveableObject, Id, IsRegistered, IsRuntime, PrefabKey, ComponentSaveData, ChunkManager (singleton, scène Monde) (+2 more)

### Community 60 - "Item"
Cohesion: 0.07
Nodes (18): ChargingStation, Consumption, Storage, DeathBag, ItemSlotSaveData, Item, Data, Gauge (+10 more)

### Community 61 - "dependencies"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, url (+18 more)

### Community 62 - "bmesh"
Cohesion: 0.22
Nodes (8): build_all(), build_size(), check(), check2(), faces_of(), islands_of(), overlap_area(), Erreurs à ne pas refaire

### Community 64 - "TMP_FrameRateCounter"
Cohesion: 0.13
Nodes (12): TMP_FrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight, TMPro_InstructionOverlay, FpsCounterAnchorPositions (+4 more)

### Community 65 - "PlayerContext"
Cohesion: 0.14
Nodes (7): PlayerContext, PlayerManager, PointedObject, PlayerInterractor, PlayerPointer, Source, Historique - Optimisations

### Community 66 - "DayNightCycle"
Cohesion: 0.11
Nodes (13): DayNightCycle, Day, Hour, Instance, IsDay, LightLevel, NightBrightness, NoonSunHeight (+5 more)

### Community 67 - "CameraMovement"
Cohesion: 0.18
Nodes (5): CameraMovement, Pitch, Target, Yaw, Caméra

### Community 68 - "Pose et destruction.md"
Cohesion: 0.15
Nodes (8): F32 - Ressources générées, R12 - Intégration des constructions au terrain, R13 - Snapping entre points de connexion, Objets posés, Destruction, Pose et destruction, Base de dômes, Dans le jeu aujourd'hui

### Community 69 - "BuildingPowerPanel"
Cohesion: 0.10
Nodes (16): BuildingOutlet, Building, OptionalDemand, SaveKey, BuildingPanelTerminal, Panel, BuildingPowerPanel, Building (+8 more)

### Community 70 - "PlayerMovement"
Cohesion: 0.13
Nodes (5): PlayerMovement, forwardDirection, IsSprinting, Stats, Déplacement du joueur

### Community 71 - "WeatherSystem"
Cohesion: 0.06
Nodes (20): WeatherSystem, AmbientFactor, Instance, IsStorm, IsStormComing, Seed, SkyFactor, SolarFactor (+12 more)

### Community 73 - "ws_build.py"
Cohesion: 0.20
Nodes (11): arc_pt(), build_body(), build_screen(), clear(), rect(), rho(), screen_uv(), setup() (+3 more)

### Community 75 - "RelayAntennaAnimator"
Cohesion: 0.20
Nodes (4): RelayAntennaAnimator, IsAntennaExtended, IsDishDeployed, IsRotating

### Community 76 - "PlayerManager"
Cohesion: 0.16
Nodes (8): PlayerManager, Instance, Inventory, IsDead, Player, SpawnedFromSave, Stats, ViewSource

### Community 77 - "ControlPanel"
Cohesion: 0.14
Nodes (10): ControlPanel, Consumer, DisplayName, IsPowered, Queue, Carte, ControlPanelUI, Panneau de contrôle (+2 more)

### Community 79 - "packages-lock (2)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 80 - "Scripts de référence"
Cohesion: 0.15
Nodes (20): prism_y(), aim(), bank_body(), belt(), boss(), build(), cabinet_body(), clear() (+12 more)

### Community 82 - "BatteryAnimator"
Cohesion: 0.20
Nodes (6): BatteryAnimator, IsCharging, IsPowered, Level, BatteryIndicator, Batteries

### Community 83 - "WindTurbineAnimator"
Cohesion: 0.18
Nodes (4): WindTurbineAnimator, IsDeployed, WindStrength, Scripts d'animation C# (runtime)

### Community 84 - "InterfaceManager"
Cohesion: 0.36
Nodes (3): InterfaceManager, HasOpenInterface, Modes de jeu et interfaces

### Community 85 - "CraftingStation"
Cohesion: 0.11
Nodes (18): Crafter, CraftingStation, Current, IsUsable, Output, Power, Progress, Recipes (+10 more)

### Community 86 - "cs_anim"
Cohesion: 0.33
Nodes (13): build_anim(), clear(), head_clearance(), ik(), k_arm(), k_head(), k_laser(), k_plate() (+5 more)

### Community 87 - "dr_build"
Cohesion: 0.19
Nodes (14): animate(), build_body(), build_carriage(), build_head(), build_hologram(), sbox(), sp(), collisions() (+6 more)

### Community 88 - "ShaderPropAnimator"
Cohesion: 0.20
Nodes (3): ShaderPropAnimator, SkewTextExample, WarpTextExample

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
Cohesion: 0.08
Nodes (14): StorageAnimator, CanOpen, IsBusy, IsOpen, StackPoint, StorageContainer, Above, Below (+6 more)

### Community 94 - "Stat (2)"
Cohesion: 0.15
Nodes (6): Stat, MaxValue, Value, StatMarker, Duration, Modifier

### Community 95 - "00 - Index Game Design.md"
Cohesion: 0.07
Nodes (36): F30 - Biomes, F47 - Relais radio, Formes et silhouettes, Matériaux et textures, Environnement, Traces de vie ancienne, Colons, Direction artistique (+28 more)

### Community 96 - "packages-lock (3)"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, url, version, depth, source, version (+13 more)

### Community 97 - "packages-lock (4)"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 98 - "packages-lock (5)"
Cohesion: 0.12
Nodes (17): dependencies, depth, source, version, dependencies, depth, source, version (+9 more)

### Community 99 - "TMP_UiFrameRateCounter"
Cohesion: 0.15
Nodes (7): Benchmark01_UGUI, TMP_UiFrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

### Community 100 - "ControlPanelAnimator"
Cohesion: 0.22
Nodes (5): ControlPanelAnimator, HologramScreen, IsBusy, IsPowered, Modèle

### Community 101 - "DeathScreenUI"
Cohesion: 0.23
Nodes (4): DeathScreenUI, Instance, IsOpen, Écran de mort

### Community 102 - "PlaceableItemData"
Cohesion: 0.16
Nodes (11): PlacedObject, Data, DisplayName, DisplayName, PlaceableItemData, NotInCorridors, OutdoorOnly, PlacedPrefab (+3 more)

### Community 103 - "StructureData"
Cohesion: 0.10
Nodes (22): BuildWheelUI, Instance, IsOpen, AirlockData, SnapDistance, StructureCatalog, Structures, StructureData (+14 more)

### Community 104 - "Données des items"
Cohesion: 0.08
Nodes (24): EquipmentData, Slot, EquipmentSlot, Back, Body, Feet, Hands, Head (+16 more)

### Community 105 - "TerrainEditSaveData"
Cohesion: 0.38
Nodes (3): TerrainEditSaveData, TerrainFlattening, Terrain sous les constructions

### Community 107 - "00 - Index Fonctionnalités.md"
Cohesion: 0.14
Nodes (3): F37 - Oxygène hors des zones respirables, R15 - Ids uniques des objets de scène, Inventaire du joueur

### Community 109 - "PlayerInputActions (4)"
Cohesion: 0.26
Nodes (4): @Camera, CameraActions, enabled, @Rotation

### Community 111 - "EnergyStorage"
Cohesion: 0.09
Nodes (19): EnergyGenerator, Generation, Output, Production, Network, EnergyStorage, Capacity, Charge (+11 more)

### Community 112 - "StatBarUI"
Cohesion: 0.23
Nodes (5): PlayerStatsUI, StatBarUI, StatUIData, R05 - Prefab asset modifié au runtime, Stats du joueur

### Community 113 - "com.unity.performance.profile-analyzer@1.3.3"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.performance.profile-analyzer@1.3.3, com.unity.performance.profile-analyzer

### Community 114 - "packages-lock (7)"
Cohesion: 0.14
Nodes (14): dependencies, depth, source, version, dependencies, depth, source, url (+6 more)

### Community 115 - "SolarPanel"
Cohesion: 0.14
Nodes (7): SolarPanel, Generation, WindTurbine, Generation, Protecting, Wind, F49 - Génération d'énergie

### Community 116 - "LandingCapsuleAnimator"
Cohesion: 0.25
Nodes (4): LandingCapsuleAnimator, IsBusy, IsOpen, Capsule de départ

### Community 117 - "DomeConnector"
Cohesion: 0.11
Nodes (11): DomeConnector, MaskMatrix, Owner, Socket, LandingCapsule, IsOpen, SaveKey, SaveState (+3 more)

### Community 118 - "TerrainMask"
Cohesion: 0.29
Nodes (5): TerrainMask, Matrix, Shape, Box, Cylinder

### Community 119 - "FlashlightBeam"
Cohesion: 0.27
Nodes (3): FlashlightBeam, FlashlightAction, Lampe torche

### Community 120 - "Technologie humaine"
Cohesion: 0.20
Nodes (10): Dômes de verre et tunnels, La capsule, Outils du joueur, Principes, Production, Rations, Robots et IA, Serres (+2 more)

### Community 121 - "wt_build.py"
Cohesion: 0.32
Nodes (11): animate(), fold(), mast(), build_base(), build_blade(), build_mast(), build_nacelle(), build_rotor() (+3 more)

### Community 122 - "TMPro"
Cohesion: 0.20
Nodes (3): TMP_DigitValidator, TMP_PhoneNumberValidator, TMPro

### Community 123 - "VertexJitter"
Cohesion: 0.12
Nodes (3): VertexJitter, VertexAnim, VertexZoom

### Community 124 - "Pickupable"
Cohesion: 0.18
Nodes (4): Pickupable, SaveKey, SaveState, Données des items

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
Cohesion: 0.62
Nodes (5): build(), doors(), fans(), key(), state()

### Community 132 - "CorridorData"
Cohesion: 0.15
Nodes (12): CorridorData, ExtensionLength, ExtensionPiece, MaxExtension, MaxLength, MinExtensionLength, MinLength, StraightLength (+4 more)

### Community 133 - "PlayerStats"
Cohesion: 0.16
Nodes (10): PlayerStats, StatName, Endurance, Health, Hunger, Oxygen, Thirst, SerializableKeyValuePair (+2 more)

### Community 134 - "PlayerModeOptions"
Cohesion: 0.17
Nodes (11): PlayerModeOptions, Options, Commentaires, Composants, Conventions de code, Gameplay et modèles, Nommage, Robustesse (+3 more)

### Community 135 - "SaveManager.BeforeSceneChange"
Cohesion: 0.21
Nodes (6): PlayerSpawnPoint, SceneLoader, Fonctionnement actuel, Scènes, Scènes et chargement, Mort et réapparition

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

### Community 141 - "Game design"
Cohesion: 0.25
Nodes (8): Colonie, Concept, Exploration et science, Game design, Légende, Monde, Survie, À décider

### Community 142 - "MonoBehaviour"
Cohesion: 0.14
Nodes (4): Benchmark04, ChatController, DropdownSample, Bootstrapper

### Community 144 - "NotificationUI"
Cohesion: 0.21
Nodes (7): NotificationType, Info, Warning, NotificationUI, Instance, Message, Messages au joueur

### Community 145 - "PauseMenuUI"
Cohesion: 0.21
Nodes (4): PauseMenuUI, Instance, IsOpen, Menu pause

### Community 146 - "À faire"
Cohesion: 0.25
Nodes (8): Bugs, Features, Historique, Identifiants, Optimisations, Prochains numéros libres, Refontes, À faire

### Community 147 - "Objets persistants"
Cohesion: 0.22
Nodes (7): Root, SceneContext, Objets persistants, Ce que l'IA vérifie, Ce que l'utilisateur teste, Outils de test, Vérifier son travail

### Community 150 - "Couleur et lumière"
Cohesion: 0.50
Nodes (4): Couleur et lumière, Extérieur, par zone, Intérieur des dômes, Le cycle jour / nuit est un outil émotionnel

### Community 151 - "ToolData.cs"
Cohesion: 0.50
Nodes (3): ToolConsumptionMode, Continuous, PerUse

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
Cohesion: 0.10
Nodes (8): PlayerInventory, HandedIndex, HandedSlot, HasPlayer, HeldAction, IsOpen, ToolBar, ItemUseContext

### Community 158 - "TMP_ExampleScript_01"
Cohesion: 0.33
Nodes (4): TMP_ExampleScript_01, objectType, TextMeshPro, TextMeshProUGUI

### Community 159 - "com.unity.modules.ai"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.ai

### Community 160 - "À éviter et check-list"
Cohesion: 0.50
Nodes (3): Check-list de validation pour chaque asset, À éviter, À éviter et check-list

### Community 161 - "com.unity.modules.androidjni"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.androidjni

### Community 164 - "EnergyNetworkManager"
Cohesion: 0.40
Nodes (5): NetworkState, NotLinked, Powered, SwitchedOff, Tripped

### Community 165 - "Prompts IA"
Cohesion: 0.50
Nodes (3): Conseils, Prompt de base (image), Prompts IA

### Community 167 - "packages-lock (14)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

### Community 170 - "Direction"
Cohesion: 0.50
Nodes (3): Direction, In, Out

### Community 180 - "manifest.json"
Cohesion: 0.17
Nodes (11): com.unity.modules.androidjni@1.0.0, com.unity.modules.audio@1.0.0, com.unity.modules.particlesystem@1.0.0, com.unity.modules.wind@1.0.0, com.unity.visualscripting@1.9.11, com.unity.modules.audio, com.unity.modules.particlesystem, com.unity.modules.androidjni (+3 more)

### Community 192 - "Space Survival (nom provisoire)"
Cohesion: 0.50
Nodes (4): Décisions en attente, Les parties du coffre, Pour commencer, Space Survival (nom provisoire)

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

### Community 206 - "ItemGaugeUnit"
Cohesion: 0.40
Nodes (4): ItemGaugeUnit, Energy, Oxygen, Wear

### Community 207 - "Points ouverts du game design"
Cohesion: 0.50
Nodes (4): Décisions en attente, Points ouverts du game design, Questions relevées en documentant le concept *(proposition)*, Écarts entre le concept et le jeu actuel

### Community 208 - "Bible de direction artistique : [NOM DU JEU]"
Cohesion: 0.67
Nodes (3): Bible de direction artistique : [NOM DU JEU], Légende, Sommaire

### Community 209 - "Intention et piliers du style"
Cohesion: 0.67
Nodes (3): Intention, Intention et piliers du style, Les piliers du style

### Community 211 - "Outils scientifiques"
Cohesion: 0.67
Nodes (3): Outils scientifiques, Scanner, Table d'observation

### Community 212 - "Instructions"
Cohesion: 0.67
Nodes (3): Domaines, Instructions, Règles communes

## Knowledge Gaps
- **452 isolated node(s):** `Element`, `HologramScreen`, `IsBusy`, `IsPowered`, `Instance` (+447 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1065 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **55 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Airlock` connect `Airlock` to `IInterractable`, `ConnectorPreview.Destroy`, `IInterractable.GetInfo`, `Builder Placement`, `GameObject`, `Airlock Animator`, `Corridor`, `StructureData`, `system`, `MonoBehaviour`, `add_revolve`, `Vector3`, `DomeConnector`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.140) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `Builder` (e.g. with `Historique - Bugs` and `Historique - Optimisations`) actually correct?**
  _`Builder` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Element`, `HologramScreen`, `IsBusy` to the rest of the system?**
  _452 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `00 - Index À faire.md` be split into smaller, more focused modules?**
  _Cohesion score 0.06887755102040816 - nodes in this community are weakly interconnected._
- **Why does `Modélisation Blender` connect `add_revolve` to `cbox`, `Airlock`, `Airlock Blender Build`, `Scripts de référence`, `pill`, `SolarPanel`, `00 - Index Pipeline 3D.md`, `Tunnel Blender Build`?**
  _High betweenness centrality (0.131) - this node is a cross-community bridge._
- **Should `ConnectorPreview.Destroy` be split into smaller, more focused modules?**
  _Cohesion score 0.0962566844919786 - nodes in this community are weakly interconnected._
- **Why does `Scripts de référence` connect `Scripts de référence` to `cbox`, `Scripts de référence`, `ws_build.py`, `Airlock Blender Build`, `add_revolve`, `pill`, `Landing Capsule Blender Build`, `00 - Index Pipeline 3D.md`, `st_build (2)`, `bmesh`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._