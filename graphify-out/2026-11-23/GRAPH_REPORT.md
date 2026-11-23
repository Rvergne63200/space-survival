# Graph Report - Space Survival  (2026-11-23)

## Corpus Check
- 413 files · ~195,365 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1645 file(s) not represented in the graph (top: .meta 1026, .mat 211, .asset 95)
- WARNING: 1645 of 2189 files seen (75%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 3888 nodes · 9554 edges · 216 communities (157 shown, 59 thin omitted)
- Extraction: 85% EXTRACTED · 15% INFERRED · 0% AMBIGUOUS · INFERRED: 1396 edges (avg confidence: 0.89)
- Token cost: 497,038 input · 0 output

## Graph Freshness
- Built from commit: `de958b30`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- 00 - Index À faire.md
- Pose et destruction.md
- PlayerInventory
- CoalGeneratorAnimator
- TMP_Text
- Builder Placement
- Material
- ItemData
- Corridor
- ConnectorPreview.Destroy
- Label
- Distribution Panel Core
- InteractableOutline
- Save System
- TerrainGenerator
- add_revolve
- Energy Network Manager
- Network Map UI
- system_collections
- pill
- Landing Capsule Blender Build
- CablePlacer
- Package Lock A
- Drill
- R22 - Refonte de la génération du terrain
- Map Utility & Minimap
- Package Lock B
- Package Lock C
- Tunnel Blender Build
- ElectricCable
- Package Lock D
- Input Actions
- Transform
- ItemCollectionUI
- IInterractable.GetInfo
- cbox
- CraftingStation
- ChunkManager
- CameraMovement
- Airlock Animator
- Distribution Panel UI
- Scripts de référence
- TextMeshProFloatingText
- CraftingStationAnimator
- Airlock Blender Build
- TMP_TextEventHandler
- UI Manager
- Stat
- Scripts (`Assets/Scripts/`)
- cp_build.py
- Vector3
- PlayerInputActions
- Item
- SolarPanelAnimator
- ModelImportSetup
- 00 - Index Pipeline 3D.md
- st_build (2)
- DrillAnimator
- SaveableObject
- ChatController
- dependencies
- bmesh
- TMP_TextSelector_B
- TMP_FrameRateCounter
- Conventions de code
- DayNightCycle
- PlayerMovement
- Données des items.md
- BuildingPowerPanel
- PlayerMovement
- WeatherSystem
- 00 - Index Fonctionnalités.md
- ws_build.py
- Terrain procédural.md
- RelayAntennaAnimator
- PlayerManager
- EnergyDevice
- TerrainPoint
- packages-lock (2)
- Scripts de référence
- TMP_TextInfoDebugTool
- BatteryAnimator
- WindTurbineAnimator
- SubclassPicker.cs
- ControlPanelUI
- cs_anim
- dr_build
- StorageContainer
- PlayerInputActions (3)
- ItemSlot
- EnergyConsumer
- SkewTextExample
- 00 - Index Game Design.md
- packages-lock (3)
- packages-lock (4)
- packages-lock (5)
- TMP_UiFrameRateCounter
- ControlPanelAnimator
- SolarPanel
- GameObject
- StructureData
- Historique - Refontes
- TerrainCraters
- SubclassPicker
- PlayerInputActions (4)
- EnergyStorage
- com.unity.performance.profile-analyzer@1.3.3
- packages-lock (7)
- DunesSettings
- LandingCapsuleAnimator
- TerrainNoise
- TerrainMask
- PlainsSettings
- Technologie humaine
- wt_build.py
- TMPro
- VertexShakeB
- PlayerContext
- BenchmarkType
- Fonctionnalités implémentées
- WallScreenAnimator
- com.unity.modules.adaptiveperformance@1.0.0
- dependencies
- packages-lock (8)
- PlateausSettings
- Stat
- TerrainRegions
- ChargingStation
- dependencies
- packages-lock (11)
- zfight (2)
- CameraController
- Game design
- DeathScreenUI
- NotificationUI
- PauseMenuUI
- À faire
- StatBarUI
- ParentedUI
- ToolData.cs
- DistributionPanel
- com.unity.settings-manager
- TerrainShape.Sample
- PlayerInventory
- TMP_ExampleScript_01
- com.unity.modules.ai
- À éviter et check-list
- com.unity.modules.androidjni
- com.coplaydev.unity-mcp@https://github.com/CoplayDev/unity-mc...
- com.unity.collab-proxy@2.12.4
- EnergyNetworkManager
- PickupInfoGroupUI
- com.unity.modules.accessibility@1.0.0
- packages-lock (14)
- com.unity.modules.assetbundle@1.0.0
- Assembly-CSharp
- Direction
- com.unity.modules.imageconversion@1.0.0
- com.unity.modules.ui@1.0.0
- TerrainShapeSettings
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
- Menaces et lisibilité
- manifest (22)
- Fiche du jeu
- manifest (24)
- manifest (25)
- Progression de la partie
- manifest (27)
- manifest (28)
- BuildingInterior
- manifest (30)
- manifest (31)
- manifest (32)
- IStorageMachine
- Points ouverts du game design
- Mode
- TerrainPreview.Render3D
- DryRiversSettings
- Ressources vitales
- CLAUDE
- al_anim.py
- ItemGaugeUnit
- DryLakesSettings
- Couleur et lumière
- Prompts IA
- Créer une interface
- Bible de direction artistique : [NOM DU JEU]
- Intention et piliers du style
- 14 - Production Unity.md
- Outils scientifiques
- Instructions

## God Nodes (most connected - your core abstractions)
1. `Builder` - 90 edges
2. `DistributionPanel` - 78 edges
3. `EnergyDevice` - 76 edges
4. `SaveManager` - 69 edges
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
- `Nommage` --references--> `PlayerModeOptions`  [INFERRED]
  obsidian/Instructions/Code/02 - Conventions de code.md → Assets/Scripts/Manager/PlayerModeOptions.cs
- `ElectricCable` --references--> `Builder`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Câble électrique.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs
- `ChunkManager (singleton, scène Monde)` --references--> `PlayerManager`  [INFERRED]
  obsidian/Fonctionnalités/Monde/Terrain procédural.md → Assets/Scripts/Behaviours/Bootstrap/Manager/PlayerManager.cs

## Import Cycles
- None detected.

## Communities (216 total, 59 thin omitted)

### Community 0 - "00 - Index À faire.md"
Cohesion: 0.07
Nodes (20): B03 - Décollage en haut des pentes, B07 - Ressenti des déplacements, B21 - Curseur visible après Échap, B26 - Flag identified des objets ramassables, F03 - Accroupissement, F06 - Vrai glisser-déposer, F20 - Recettes à plusieurs résultats, F27 - Réseau d'oxygène (+12 more)

### Community 1 - "Pose et destruction.md"
Cohesion: 0.25
Nodes (3): R12 - Intégration des constructions au terrain, Base de dômes, Dans le jeu aujourd'hui

### Community 2 - "PlayerInventory"
Cohesion: 0.09
Nodes (28): SubclassPicker, AbstractModifier, EnduranceModifier, LifeModifier, StatModifier, ConsummableData, ApplyModifiersAction, HoldItemAction (+20 more)

### Community 3 - "CoalGeneratorAnimator"
Cohesion: 0.07
Nodes (20): CoalGeneratorAnimator, ChimneyHeight, CollarRotation, IsBusy, IsHatchOpen, IsRunning, MinChimneyHeight, SmokePoint (+12 more)

### Community 4 - "TMP_Text"
Cohesion: 0.05
Nodes (9): EnvMapAnimator, TeleType, TextConsoleSimulator, TMP_TextEventCheck, VertexColorCycler, VertexJitter, VertexAnim, VertexShakeA (+1 more)

### Community 5 - "Builder Placement"
Cohesion: 0.07
Nodes (7): Structure, Builder, CorridorStructure, Inventory, IsActive, Source, R13 - Snapping entre points de connexion

### Community 6 - "Material"
Cohesion: 0.09
Nodes (16): Socket, InteractionDistance, CorridorData, ExtensionLength, ExtensionPiece, MaxExtension, MaxLength, MinExtensionLength (+8 more)

### Community 7 - "ItemData"
Cohesion: 0.09
Nodes (11): Pickuper, Equipment, ItemCollection, MaxSize, ItemData, Description, Id, Model (+3 more)

### Community 8 - "Corridor"
Cohesion: 0.06
Nodes (24): Airlock, All, DoorsBusy, IsBuilt, SaveKey, BuiltStructure, CostMultiplier, Data (+16 more)

### Community 9 - "ConnectorPreview.Destroy"
Cohesion: 0.09
Nodes (8): ConnectorPreview, DomeConnector, MaskMatrix, Owner, MeshCutter, Connecteurs de dôme, Dômes, Plans de construction

### Community 10 - "Label"
Cohesion: 0.10
Nodes (9): ClockUI, DistributionPanelUI, Instance, ItemTooltip, MainMenuUI, SaveSlotInfo, F54 - Cycle jour nuit et météo, Horloge (+1 more)

### Community 11 - "Distribution Panel Core"
Cohesion: 0.04
Nodes (47): DistributionPanel, BatteryChargeSupplied, BatteryThreshold, Building, ChargeRequest, ChargeRoom, ChargeSupplied, ConsumedEnergy (+39 more)

### Community 12 - "InteractableOutline"
Cohesion: 0.09
Nodes (8): CameraManager, Camera, Instance, Target, CameraContext, CameraManager, InteractableOutline, Contour de l'objet pointé

### Community 13 - "Save System"
Cohesion: 0.09
Nodes (15): ComponentSaveData, ObjectSaveData, PlayerSaveData, SaveData, WorldTimeSaveData, SaveManager, HasSave, Instance (+7 more)

### Community 14 - "TerrainGenerator"
Cohesion: 0.16
Nodes (3): EnvironmentSpawner, TerrainGenerator, F31 - Structures générées

### Community 15 - "add_revolve"
Cohesion: 0.12
Nodes (29): build_airlock_body(), densify(), add_box(), add_cyl(), add_revolve(), add_screw(), finish(), zone_mats() (+21 more)

### Community 16 - "Energy Network Manager"
Cohesion: 0.12
Nodes (4): EnergyNetworkManager, HasInstance, Instance, Network

### Community 19 - "pill"
Cohesion: 0.14
Nodes (26): beam(), oct_cyl(), pill(), animate(), arm_k(), mast_k(), build_arm(), build_base() (+18 more)

### Community 20 - "Landing Capsule Blender Build"
Cohesion: 0.13
Nodes (24): animate(), build_body(), build_door(), build_door_frames(), build_exterior(), q(), build_interior(), q() (+16 more)

### Community 21 - "CablePlacer"
Cohesion: 0.14
Nodes (6): CablePlacer, Blocked, Inventory, Source, BuildModeUI, Instance

### Community 22 - "Package Lock A"
Cohesion: 0.08
Nodes (28): dependencies, depth, source, version, dependencies, depth, source, version (+20 more)

### Community 23 - "Drill"
Cohesion: 0.09
Nodes (13): ControlPanel, Consumer, DisplayName, IsPowered, Drill, CanSwitchMachine, Gauge, GaugeText (+5 more)

### Community 24 - "R22 - Refonte de la génération du terrain"
Cohesion: 0.21
Nodes (13): TerrainDryLakes, TerrainDryRivers, TerrainDunes, TerrainPlains, TerrainPlateaus, TerrainRegions, ITerrainFeature, TerrainShape (+5 more)

### Community 25 - "Map Utility & Minimap"
Cohesion: 0.18
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

### Community 32 - "Transform"
Cohesion: 0.18
Nodes (5): StorageAnimator, CanOpen, IsBusy, IsOpen, StackPoint

### Community 33 - "ItemCollectionUI"
Cohesion: 0.15
Nodes (6): ItemCollectionUI, SlotsUI, ItemStorageUI, PlayerInventoryContentUI, PlayerToolBarUI, Clics de l'inventaire

### Community 35 - "cbox"
Cohesion: 0.18
Nodes (16): animate(), build_body(), build_chimney_base(), build_collar(), build_fan(), build_hatch(), build_head(), build_lever() (+8 more)

### Community 36 - "CraftingStation"
Cohesion: 0.08
Nodes (22): Crafter, CraftingStation, Current, IsUsable, Output, Power, Progress, Recipes (+14 more)

### Community 37 - "ChunkManager"
Cohesion: 0.16
Nodes (4): ChunkManager, Instance, Seed, TerrainEdits

### Community 38 - "CameraMovement"
Cohesion: 0.18
Nodes (5): CameraMovement, Pitch, Target, Yaw, Caméra

### Community 39 - "Airlock Animator"
Cohesion: 0.19
Nodes (5): AirlockAnimator, InnerOpen, IsBusy, OuterOpen, Pressurized

### Community 41 - "Scripts de référence"
Cohesion: 0.06
Nodes (44): bake_positions(), bilinear(), blur(), build(), build_mesh(), build_textures(), fields(), lerp() (+36 more)

### Community 42 - "TextMeshProFloatingText"
Cohesion: 0.14
Nodes (8): Benchmark01, Benchmark02, Benchmark03, Benchmark04, SimpleScript, TextMeshProFloatingText, TextMeshSpawner, BlueprintMaterial

### Community 43 - "CraftingStationAnimator"
Cohesion: 0.21
Nodes (4): CraftingStationAnimator, HologramScreen, IsPowered, IsPrinting

### Community 44 - "Airlock Blender Build"
Cohesion: 0.17
Nodes (12): build_connector(), build_fan(), build_leaf(), dome_surface_y(), door_leaf_poly(), finish2(), fix_normals(), hollow_y() (+4 more)

### Community 46 - "TMP_TextEventHandler"
Cohesion: 0.11
Nodes (11): TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, onCharacterSelection, onLineSelection, onLinkSelection, onSpriteSelection (+3 more)

### Community 47 - "UI Manager"
Cohesion: 0.12
Nodes (14): UIManager, ev_updatePlayerContext, Instance, PlayerInventory, PlayerStats, UI, ParentUI, CraftingStationUI (+6 more)

### Community 48 - "Stat"
Cohesion: 0.19
Nodes (6): Stat, MaxValue, Value, StatMarker, Duration, Modifier

### Community 49 - "Scripts (`Assets/Scripts/`)"
Cohesion: 0.06
Nodes (27): Bootstrapper, Root, SceneContext, InterfaceManager, HasOpenInterface, ModeManager, Instance, Mode (+19 more)

### Community 50 - "cp_build.py"
Cohesion: 0.14
Nodes (15): animate(), build_body(), build_hologram(), sbox(), sp(), build_projector(), cham(), clear() (+7 more)

### Community 51 - "Vector3"
Cohesion: 0.09
Nodes (7): Dome, ConnectorPrefab, Connectors, InnerWallRadius, Model, Radius, DomeConnection

### Community 52 - "PlayerInputActions"
Cohesion: 0.14
Nodes (11): @Actions, ActionsActions, @Build, @ChangeHandedItem, @CloseInterfaces, @Drop, enabled, @Interract (+3 more)

### Community 53 - "Item"
Cohesion: 0.15
Nodes (10): HeldItemUI, HeldItem, Item, Data, Gauge, GaugeRatio, GaugeSettings, HasGauge (+2 more)

### Community 54 - "SolarPanelAnimator"
Cohesion: 0.13
Nodes (7): SolarPanelAnimator, DeployProgress, HeadingAngle, IsDeployed, IsRotating, PanelFront, PanelUp

### Community 55 - "ModelImportSetup"
Cohesion: 0.15
Nodes (5): ModelImportSetup, Fichiers et import, Méthode qui marche (gisements, 2026-11-23), Textures, Export FBX

### Community 56 - "00 - Index Pipeline 3D.md"
Cohesion: 0.13
Nodes (12): Construction des modèles, Méthode technique, Pipeline 3D (Blender → Unity), Sommaire, Façon de travailler, Palette des matériaux, Dimensions de référence, Objets muraux (+4 more)

### Community 57 - "st_build (2)"
Cohesion: 0.23
Nodes (12): build(), cabinet_body(), cabinet_doors(), cham(), clear(), collisions(), crate_body(), crate_lids() (+4 more)

### Community 58 - "DrillAnimator"
Cohesion: 0.19
Nodes (5): DrillAnimator, DustPoint, IsBusy, IsDrilling, Powered

### Community 59 - "SaveableObject"
Cohesion: 0.14
Nodes (6): SaveId, SaveableObject, Id, IsRegistered, IsRuntime, PrefabKey

### Community 61 - "dependencies"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, url (+18 more)

### Community 62 - "bmesh"
Cohesion: 0.36
Nodes (6): check(), check2(), faces_of(), islands_of(), overlap_area(), Erreurs à ne pas refaire

### Community 64 - "TMP_FrameRateCounter"
Cohesion: 0.13
Nodes (12): TMP_FrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight, TMPro_InstructionOverlay, FpsCounterAnchorPositions (+4 more)

### Community 65 - "Conventions de code"
Cohesion: 0.33
Nodes (6): Commentaires, Composants, Conventions de code, Gameplay et modèles, Nommage, Robustesse

### Community 66 - "DayNightCycle"
Cohesion: 0.06
Nodes (21): ObjectSpin, MotionType, Rotation, SearchLight, Translation, FlashlightBeam, DayNightCycle, Day (+13 more)

### Community 67 - "PlayerMovement"
Cohesion: 0.13
Nodes (5): PlayerMovement, forwardDirection, IsSprinting, Stats, Déplacement du joueur

### Community 68 - "Données des items.md"
Cohesion: 0.19
Nodes (3): F17 - Contour masqué par les obstacles, F32 - Ressources générées, Objets posés

### Community 69 - "BuildingPowerPanel"
Cohesion: 0.10
Nodes (16): BuildingOutlet, Building, OptionalDemand, SaveKey, BuildingPanelTerminal, Panel, BuildingPowerPanel, Building (+8 more)

### Community 71 - "WeatherSystem"
Cohesion: 0.07
Nodes (19): WeatherSystem, AmbientFactor, Instance, IsStorm, IsStormComing, Seed, SkyFactor, SolarFactor (+11 more)

### Community 72 - "00 - Index Fonctionnalités.md"
Cohesion: 0.09
Nodes (9): F11 - Données propres à chaque exemplaire, F37 - Oxygène hors des zones respirables, R01 - Refonte du chargement de scènes, R02 - Persistance entre scènes, R15 - Ids uniques des objets de scène, Inventaire du joueur, Code Unity, Point ouvert (+1 more)

### Community 73 - "ws_build.py"
Cohesion: 0.20
Nodes (11): arc_pt(), build_body(), build_screen(), clear(), rect(), rho(), screen_uv(), setup() (+3 more)

### Community 74 - "Terrain procédural.md"
Cohesion: 0.20
Nodes (5): B10 - Pivot de ReplaceObject trop haut, F30 - Biomes, F67 - Éléments naturels 3D, O03 - Monde infini, Objets du décor

### Community 75 - "RelayAntennaAnimator"
Cohesion: 0.20
Nodes (4): RelayAntennaAnimator, IsAntennaExtended, IsDishDeployed, IsRotating

### Community 76 - "PlayerManager"
Cohesion: 0.10
Nodes (14): PlayerManager, Instance, Inventory, IsDead, Player, SpawnedFromSave, Stats, ViewSource (+6 more)

### Community 77 - "EnergyDevice"
Cohesion: 0.13
Nodes (12): DisplayName, EnergyDevice, Cables, CanSwitch, DeviceState, DisplayName, ForcedOff, IsOn (+4 more)

### Community 79 - "packages-lock (2)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 80 - "Scripts de référence"
Cohesion: 0.11
Nodes (24): prism_y(), aim(), bank_body(), belt(), boss(), build(), cabinet_body(), clear() (+16 more)

### Community 82 - "BatteryAnimator"
Cohesion: 0.20
Nodes (6): BatteryAnimator, IsCharging, IsPowered, Level, BatteryIndicator, Batteries

### Community 83 - "WindTurbineAnimator"
Cohesion: 0.18
Nodes (4): WindTurbineAnimator, IsDeployed, WindStrength, Scripts d'animation C# (runtime)

### Community 86 - "cs_anim"
Cohesion: 0.33
Nodes (13): build_anim(), clear(), head_clearance(), ik(), k_arm(), k_head(), k_laser(), k_plate() (+5 more)

### Community 87 - "dr_build"
Cohesion: 0.19
Nodes (14): animate(), build_body(), build_carriage(), build_head(), build_hologram(), sbox(), sp(), collisions() (+6 more)

### Community 88 - "StorageContainer"
Cohesion: 0.09
Nodes (15): State, StorageContainer, Above, Below, SaveKey, Stackable, StackPoint, StackSize (+7 more)

### Community 89 - "PlayerInputActions (3)"
Cohesion: 0.22
Nodes (6): @Movement, MovementActions, enabled, @Jump, @Move, @Sprint

### Community 90 - "ItemSlot"
Cohesion: 0.14
Nodes (7): ItemSlotUI, Element, Slots, ItemSlot, ev_update, Item, Quantity

### Community 91 - "EnergyConsumer"
Cohesion: 0.09
Nodes (13): EnergyConsumer, Consumption, Demand, IsPowered, State, TestMachine, Items du réseau, Objets de test (+5 more)

### Community 95 - "00 - Index Game Design.md"
Cohesion: 0.07
Nodes (36): F47 - Relais radio, F66 - Chutes de météorites, Formes et silhouettes, Matériaux et textures, Environnement, Traces de vie ancienne, Colons, Direction artistique (+28 more)

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
Cohesion: 0.14
Nodes (8): Benchmark01_UGUI, DropdownSample, TMP_UiFrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

### Community 100 - "ControlPanelAnimator"
Cohesion: 0.22
Nodes (5): ControlPanelAnimator, HologramScreen, IsBusy, IsPowered, Modèle

### Community 101 - "SolarPanel"
Cohesion: 0.12
Nodes (10): EnergyDeviceInfo, SolarPanel, Generation, WindTurbine, Generation, Protecting, Wind, F49 - Génération d'énergie (+2 more)

### Community 102 - "GameObject"
Cohesion: 0.07
Nodes (23): AirlockDoor, Airlock, Outer, BuildUtility, PlacedObject, Data, Searcher, CurrentSearchedStorage (+15 more)

### Community 103 - "StructureData"
Cohesion: 0.09
Nodes (21): BuildWheelUI, Instance, IsOpen, AirlockData, SnapDistance, StructureCatalog, Structures, StructureData (+13 more)

### Community 104 - "Historique - Refontes"
Cohesion: 0.07
Nodes (27): Pickupable, SaveKey, SaveState, EquipmentData, Slot, EquipmentSlot, Back, Body (+19 more)

### Community 107 - "TerrainCraters"
Cohesion: 0.20
Nodes (7): CraterScale, CellSize, Chance, CratersSettings, MaxRadius, Scales, TerrainCraters

### Community 109 - "PlayerInputActions (4)"
Cohesion: 0.26
Nodes (4): @Camera, CameraActions, enabled, @Rotation

### Community 111 - "EnergyStorage"
Cohesion: 0.11
Nodes (18): EnergyGenerator, Generation, Output, Production, EnergyStorage, Capacity, Charge, Discharge (+10 more)

### Community 113 - "com.unity.performance.profile-analyzer@1.3.3"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.performance.profile-analyzer@1.3.3, com.unity.performance.profile-analyzer

### Community 114 - "packages-lock (7)"
Cohesion: 0.14
Nodes (14): dependencies, depth, source, version, dependencies, depth, source, url (+6 more)

### Community 115 - "DunesSettings"
Cohesion: 0.33
Nodes (6): DunesSettings, BasinDepth, BasinThreshold, Height, Wavelength, WindAngle

### Community 116 - "LandingCapsuleAnimator"
Cohesion: 0.08
Nodes (10): LandingCapsuleAnimator, IsBusy, IsOpen, LandingCapsule, IsOpen, SaveKey, SaveState, LandingCapsuleDoor (+2 more)

### Community 118 - "TerrainMask"
Cohesion: 0.15
Nodes (9): TerrainMask, Matrix, Shape, Box, Cylinder, TerrainEditSaveData, TerrainFlattening, F65 - Intégration des modèles 3D (+1 more)

### Community 119 - "PlainsSettings"
Cohesion: 0.29
Nodes (7): PlainsSettings, BaseHeight, HillAreaSize, HillHeight, HillSize, Relief, ReliefSize

### Community 120 - "Technologie humaine"
Cohesion: 0.20
Nodes (10): Dômes de verre et tunnels, La capsule, Outils du joueur, Principes, Production, Rations, Robots et IA, Serres (+2 more)

### Community 121 - "wt_build.py"
Cohesion: 0.32
Nodes (11): animate(), fold(), mast(), build_base(), build_blade(), build_mast(), build_nacelle(), build_rotor() (+3 more)

### Community 122 - "TMPro"
Cohesion: 0.18
Nodes (3): TMP_DigitValidator, TMP_PhoneNumberValidator, TMPro

### Community 124 - "PlayerContext"
Cohesion: 0.11
Nodes (10): PlayerContext, PlayerContext, Interractable, PlayerManager, PointedObject, PlayerInterractor, PlayerPointer, Source (+2 more)

### Community 125 - "BenchmarkType"
Cohesion: 0.33
Nodes (6): BenchmarkType, TEXTMESH_BITMAP, TMP_BITMAP_MOBILE, TMP_SDF, TMP_SDF_MOBILE, TMP_SDF__MOBILE_SSD

### Community 126 - "Fonctionnalités implémentées"
Cohesion: 0.20
Nodes (10): Bootstrap, Construction, Crafting, Fonctionnalités implémentées, Interaction, Interfaces, Inventaire, Joueur (+2 more)

### Community 127 - "WallScreenAnimator"
Cohesion: 0.19
Nodes (5): ShaderPropAnimator, WallScreenAnimator, IsPowered, ScreenSurface, ConnectionPoint

### Community 129 - "dependencies"
Cohesion: 0.17
Nodes (12): dependencies, com.unity.modules.ai@1.0.0, com.unity.modules.cloth@1.0.0, com.unity.modules.tilemap@1.0.0, com.unity.modules.umbra@1.0.0, com.unity.modules.unitywebrequestassetbundle@1.0.0, com.unity.modules.vectorgraphics@1.0.0, com.unity.modules.unitywebrequestassetbundle (+4 more)

### Community 130 - "packages-lock (8)"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, version, dependencies, depth, source, version (+10 more)

### Community 132 - "PlateausSettings"
Cohesion: 0.29
Nodes (7): PlateausSettings, CanyonSize, CanyonWidth, EdgeWidth, Height, Strata, Threshold

### Community 133 - "Stat"
Cohesion: 0.13
Nodes (11): PlayerStats, StatName, Endurance, Health, Hunger, Oxygen, Thirst, StatSaveData (+3 more)

### Community 135 - "TerrainRegions"
Cohesion: 0.50
Nodes (4): RegionsSettings, Size, WarpSize, WarpStrength

### Community 136 - "ChargingStation"
Cohesion: 0.10
Nodes (14): ChargingStation, Consumption, Storage, DeathBag, Searchable, ItemSlotSaveData, ItemStorage, Content (+6 more)

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

### Community 142 - "DeathScreenUI"
Cohesion: 0.11
Nodes (5): SceneLoadingManager, DeathScreenUI, Instance, IsOpen, Écran de mort

### Community 144 - "NotificationUI"
Cohesion: 0.20
Nodes (7): NotificationType, Info, Warning, NotificationUI, Instance, Message, Messages au joueur

### Community 145 - "PauseMenuUI"
Cohesion: 0.23
Nodes (4): PauseMenuUI, Instance, IsOpen, Menu pause

### Community 146 - "À faire"
Cohesion: 0.25
Nodes (8): Bugs, Features, Historique, Identifiants, Optimisations, Prochains numéros libres, Refontes, À faire

### Community 147 - "StatBarUI"
Cohesion: 0.23
Nodes (5): PlayerStatsUI, StatBarUI, StatUIData, R05 - Prefab asset modifié au runtime, Stats du joueur

### Community 149 - "ParentedUI"
Cohesion: 0.12
Nodes (3): PointedInterractorInformationUI, HandedCursorUI, ParentedUI

### Community 151 - "ToolData.cs"
Cohesion: 0.50
Nodes (3): ToolConsumptionMode, Continuous, PerUse

### Community 153 - "DistributionPanel"
Cohesion: 0.25
Nodes (8): PanelState, Fused, Loop, NoInput, Off, On, Shed, Starting

### Community 154 - "com.unity.settings-manager"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.settings-manager, com.unity.settings-manager

### Community 157 - "PlayerInventory"
Cohesion: 0.11
Nodes (8): PlayerInventory, HandedIndex, HandedSlot, HasPlayer, HeldAction, IsOpen, ToolBar, Historique - Bugs

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

### Community 165 - "PickupInfoGroupUI"
Cohesion: 0.31
Nodes (3): PickupInfoGroupUI, Group, PickupInfoUI

### Community 167 - "packages-lock (14)"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

### Community 170 - "Direction"
Cohesion: 0.50
Nodes (3): Direction, In, Out

### Community 173 - "TerrainShapeSettings"
Cohesion: 0.12
Nodes (8): TerrainShapeSettings, Craters, DryLakes, DryRivers, Dunes, Plains, Plateaus, Regions

### Community 180 - "manifest.json"
Cohesion: 0.17
Nodes (11): com.unity.modules.androidjni@1.0.0, com.unity.modules.audio@1.0.0, com.unity.modules.particlesystem@1.0.0, com.unity.modules.wind@1.0.0, com.unity.visualscripting@1.9.11, com.unity.modules.audio, com.unity.modules.particlesystem, com.unity.modules.androidjni (+3 more)

### Community 192 - "Space Survival (nom provisoire)"
Cohesion: 0.50
Nodes (4): Décisions en attente, Les parties du coffre, Pour commencer, Space Survival (nom provisoire)

### Community 194 - "Menaces et lisibilité"
Cohesion: 0.67
Nodes (3): Les menaces, Menaces et lisibilité, Règles de lisibilité

### Community 196 - "Fiche du jeu"
Cohesion: 0.40
Nodes (5): En bref, Fiche du jeu, Pitch, Production, Ton

### Community 199 - "Progression de la partie"
Cohesion: 0.40
Nodes (5): Au départ, Développement, Fin de partie, Progression de la partie, Ressenti

### Community 202 - "BuildingInterior"
Cohesion: 0.17
Nodes (6): BuildingInterior, Radius, Zone, Roof, Intérieur des bâtiments, Météo

### Community 206 - "IStorageMachine"
Cohesion: 0.18
Nodes (8): IStorageMachine, CanSwitchMachine, Gauge, GaugeText, MachineName, MachineOn, MachineState, Foreuse

### Community 207 - "Points ouverts du game design"
Cohesion: 0.50
Nodes (4): Décisions en attente, Points ouverts du game design, Questions relevées en documentant le concept *(proposition)*, Écarts entre le concept et le jeu actuel

### Community 208 - "Mode"
Cohesion: 0.29
Nodes (7): Mode, Airlock, Building, Corridor, Destroying, None, Placing

### Community 211 - "DryRiversSettings"
Cohesion: 0.40
Nodes (5): DryRiversSettings, AreaSize, Depth, Size, Width

### Community 212 - "Ressources vitales"
Cohesion: 0.40
Nodes (5): Eau, Nourriture, Oxygène, Ressources vitales, Électricité

### Community 214 - "al_anim.py"
Cohesion: 0.33
Nodes (7): build(), doors(), fans(), key(), state(), build_all(), build_size()

### Community 216 - "ItemGaugeUnit"
Cohesion: 0.50
Nodes (4): ItemGaugeUnit, Energy, Oxygen, Wear

### Community 217 - "DryLakesSettings"
Cohesion: 0.50
Nodes (4): DryLakesSettings, Depth, Size, Threshold

### Community 218 - "Couleur et lumière"
Cohesion: 0.50
Nodes (4): Couleur et lumière, Extérieur, par zone, Intérieur des dômes, Le cycle jour / nuit est un outil émotionnel

### Community 219 - "Prompts IA"
Cohesion: 0.50
Nodes (3): Conseils, Prompt de base (image), Prompts IA

### Community 220 - "Créer une interface"
Cohesion: 0.29
Nodes (6): Modes de jeu et interfaces, Créer une interface, Interface du jeu (fenêtre qu'on ouvre), Mise en page, Textes, Éléments du HUD

### Community 221 - "Bible de direction artistique : [NOM DU JEU]"
Cohesion: 0.67
Nodes (3): Bible de direction artistique : [NOM DU JEU], Légende, Sommaire

### Community 222 - "Intention et piliers du style"
Cohesion: 0.67
Nodes (3): Intention, Intention et piliers du style, Les piliers du style

### Community 224 - "Outils scientifiques"
Cohesion: 0.67
Nodes (3): Outils scientifiques, Scanner, Table d'observation

### Community 225 - "Instructions"
Cohesion: 0.67
Nodes (3): Domaines, Instructions, Règles communes

## Knowledge Gaps
- **490 isolated node(s):** `ConnectorPrefab`, `InnerWallRadius`, `Model`, `Radius`, `HologramScreen` (+485 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1120 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **59 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Airlock` connect `Corridor` to `Builder Placement`, `Material`, `Airlock Animator`, `GameObject`, `ConnectorPreview.Destroy`, `TextMeshProFloatingText`, `ChargingStation`, `StructureData`, `system`, `add_revolve`, `Scripts (`Assets/Scripts/`)`, `Vector3`, `LandingCapsuleAnimator`, `TerrainMask`, `StorageContainer`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.135) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `Builder` (e.g. with `Historique - Bugs` and `Historique - Optimisations`) actually correct?**
  _`Builder` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `ConnectorPrefab`, `InnerWallRadius`, `Model` to the rest of the system?**
  _490 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `00 - Index À faire.md` be split into smaller, more focused modules?**
  _Cohesion score 0.0666049953746531 - nodes in this community are weakly interconnected._
- **Why does `Modélisation Blender` connect `add_revolve` to `SolarPanel`, `Corridor`, `Airlock Blender Build`, `Scripts de référence`, `pill`, `00 - Index Pipeline 3D.md`, `Tunnel Blender Build`?**
  _High betweenness centrality (0.133) - this node is a cross-community bridge._
- **Should `PlayerInventory` be split into smaller, more focused modules?**
  _Cohesion score 0.09191919191919191 - nodes in this community are weakly interconnected._
- **Why does `add_revolve()` connect `add_revolve` to `wt_build.py`, `cbox`, `Scripts de référence`, `pill`, `Landing Capsule Blender Build`, `dr_build`, `st_build (2)`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._