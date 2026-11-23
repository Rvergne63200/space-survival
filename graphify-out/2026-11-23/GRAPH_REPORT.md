# Graph Report - Space Survival  (2026-11-23)

## Corpus Check
- 391 files · ~179,225 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 1563 file(s) not represented in the graph (top: .meta 963, .mat 205, .asset 93)
- WARNING: 1563 of 2036 files seen (77%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 3649 nodes · 9029 edges · 214 communities (151 shown, 63 thin omitted)
- Extraction: 86% EXTRACTED · 14% INFERRED · 0% AMBIGUOUS · INFERRED: 1303 edges (avg confidence: 0.89)
- Token cost: 497,038 input · 0 output

## Graph Freshness
- Built from commit: `dc8d0325`
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
- Label
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
- Sauvegarde.md
- Interfaces UI Toolkit.md
- Map Utility & Minimap
- Package Lock B
- Package Lock C
- Tunnel Blender Build
- ElectricCable
- Package Lock D
- Input Actions
- ModelImportSetup
- IInterractable.GetInfo
- ItemData
- Terrain Chunks
- CallbackContext
- Airlock Animator
- Distribution Panel UI
- st_build
- TextMeshProFloatingText
- Contour de l'objet pointé.md
- Airlock Blender Build
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
- Dome
- dependencies
- bmesh
- TMP_UiFrameRateCounter
- TMP_FrameRateCounter
- PlayerContext
- DayNightCycle
- InteractableOutline
- Données des items.md
- BuildingPowerPanel
- PlayerMovement
- WeatherSystem
- FlashlightBeam
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
- ControlPanelUI
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
- IInterractable
- Technologie humaine
- wt_build
- TMPro
- TerrainGenerator
- StorageAnimator
- WeatherSystem.Update
- Fonctionnalités implémentées
- WallScreenAnimator
- com.unity.modules.adaptiveperformance@1.0.0
- dependencies
- packages-lock (8)
- al_anim
- EnergyGenerator
- PlayerStats
- ObjectSpin
- IStorageMachine
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
- VertexShakeA
- Couleur et lumière
- Objets persistants
- À éviter et check-list
- Prompts IA
- Bible de direction artistique : [NOM DU JEU]
- DistributionPanel
- com.unity.settings-manager
- Menaces et lisibilité
- 14 - Production Unity.md
- PlayerInventory
- ToolData.cs
- com.unity.modules.ai
- VertexZoom
- com.unity.modules.androidjni
- com.coplaydev.unity-mcp@https://github.com/CoplayDev/unity-mc...
- com.unity.collab-proxy@2.12.4
- EnergyNetworkManager
- Searcher
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
- SaveManager.UnloadObjects
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
- WeatherState
- Points ouverts du game design
- Space Survival (nom provisoire)
- MonoBehaviour
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
10. `WeatherSystem` - 48 edges

## Surprising Connections (you probably didn't know these)
- `Inventaire du joueur` --references--> `PlayerInventory`  [INFERRED]
  obsidian/Fonctionnalités/Inventaire/Inventaire du joueur.md → Assets/Scripts/Behaviours/Entities/Player/PlayerInventory.cs
- `DistributionPanelUI` --references--> `DistributionPanelSettingsView`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Tableau de distribution.md → Assets/Scripts/Behaviours/UI/Energy/DistributionPanelSettingsView.cs
- `ElectricCable` --references--> `Builder`  [INFERRED]
  obsidian/Fonctionnalités/Énergie/Câble électrique.md → Assets/Scripts/Behaviours/Entities/Player/Builder.cs
- `R05 - Prefab asset modifié au runtime` --references--> `ItemCollectionUI`  [INFERRED]
  obsidian/A faire/R05 - Prefab asset modifié au runtime.md → Assets/Scripts/Behaviours/UI/Inventory/ItemCollectionUI.cs
- `Historique - Bugs` --references--> `ItemSlotUI`  [INFERRED]
  obsidian/A faire/Historique/Historique - Bugs.md → Assets/Scripts/Behaviours/UI/Inventory/ItemSlotUI.cs

## Import Cycles
- None detected.

## Communities (214 total, 63 thin omitted)

### Community 0 - "00 - Index À faire.md"
Cohesion: 0.09
Nodes (15): B03 - Décollage en haut des pentes, B07 - Ressenti des déplacements, B26 - Flag identified des objets ramassables, F06 - Vrai glisser-déposer, F20 - Recettes à plusieurs résultats, F27 - Réseau d'oxygène, F50 - Réseau électrique complet, F51 - Système d'oxygène (+7 more)

### Community 2 - "PlayerInventory"
Cohesion: 0.10
Nodes (30): SubclassPicker, HeldAction, AbstractModifier, EnduranceModifier, LifeModifier, StatModifier, ConsummableData, ApplyModifiersAction (+22 more)

### Community 3 - "Coal Generator Animator"
Cohesion: 0.07
Nodes (21): CoalGeneratorAnimator, ChimneyHeight, CollarRotation, IsBusy, IsHatchOpen, IsRunning, MinChimneyHeight, SmokePoint (+13 more)

### Community 4 - "TMP_Text"
Cohesion: 0.10
Nodes (5): EnvMapAnimator, TeleType, TextConsoleSimulator, TMP_TextEventCheck, VertexColorCycler

### Community 5 - "Builder Placement"
Cohesion: 0.07
Nodes (7): Structure, Builder, CorridorStructure, Inventory, IsActive, Source, R13 - Snapping entre points de connexion

### Community 6 - "Transform"
Cohesion: 0.12
Nodes (9): ShaderPropAnimator, BuildUtility, BlueprintMaterial, CorridorPieces, Piece, OwnedMeshes, Historique - Refontes, Couloir (+1 more)

### Community 7 - "ItemSlot"
Cohesion: 0.13
Nodes (8): Equipment, ItemCollection, MaxSize, Slots, ItemSlot, ev_update, Item, Quantity

### Community 8 - "ConnectorPreview.Destroy"
Cohesion: 0.11
Nodes (8): ConnectorPreview, DomeConnector, MaskMatrix, Owner, Socket, MeshCutter, Connecteurs de dôme, Dômes

### Community 9 - "Corridor"
Cohesion: 0.05
Nodes (34): Airlock, All, DoorsBusy, IsBuilt, SaveKey, State, BuiltStructure, CostMultiplier (+26 more)

### Community 10 - "Label"
Cohesion: 0.08
Nodes (14): ClockUI, HeldItemUI, HeldItem, ItemCollectionUI, SlotsUI, ItemSlotUI, Element, ItemStorageUI (+6 more)

### Community 11 - "Distribution Panel Core"
Cohesion: 0.04
Nodes (47): DistributionPanel, BatteryChargeSupplied, BatteryThreshold, Building, ChargeRequest, ChargeRoom, ChargeSupplied, ConsumedEnergy (+39 more)

### Community 12 - "CameraMovement"
Cohesion: 0.10
Nodes (11): CameraManager, Camera, Instance, Target, CameraContext, CameraManager, CameraMovement, Pitch (+3 more)

### Community 13 - "Save System"
Cohesion: 0.09
Nodes (16): ComponentSaveData, ObjectSaveData, PlayerSaveData, SaveData, StatSaveData, WorldTimeSaveData, SaveManager, HasSave (+8 more)

### Community 14 - "ISaveableComponent"
Cohesion: 0.24
Nodes (3): EnvironmentSpawner, F31 - Structures générées, Objets du décor

### Community 15 - "add_revolve"
Cohesion: 0.12
Nodes (29): build_airlock_body(), densify(), add_box(), add_cyl(), add_revolve(), add_screw(), finish(), zone_mats() (+21 more)

### Community 16 - "Energy Network Manager"
Cohesion: 0.15
Nodes (4): EnergyNetworkManager, HasInstance, Instance, Network

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

### Community 23 - "Sauvegarde.md"
Cohesion: 0.18
Nodes (4): B10 - Pivot de ReplaceObject trop haut, F32 - Ressources générées, O03 - Monde infini, R15 - Ids uniques des objets de scène

### Community 24 - "Interfaces UI Toolkit.md"
Cohesion: 0.15
Nodes (6): R01 - Refonte du chargement de scènes, R02 - Persistance entre scènes, R09 - Système d'interaction extensible, Code Unity, Point ouvert, Sommaire

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

### Community 33 - "ModelImportSetup"
Cohesion: 0.17
Nodes (3): ModelImportSetup, Textures, Export FBX

### Community 36 - "ItemData"
Cohesion: 0.10
Nodes (8): Pickuper, ItemData, Description, Id, Model, Name, Sprite, StackMax

### Community 37 - "Terrain Chunks"
Cohesion: 0.19
Nodes (4): ChunkManager, Instance, Seed, TerrainEdits

### Community 39 - "Airlock Animator"
Cohesion: 0.19
Nodes (5): AirlockAnimator, InnerOpen, IsBusy, OuterOpen, Pressurized

### Community 40 - "Distribution Panel UI"
Cohesion: 0.15
Nodes (4): DistributionPanelSettingsView, Target, DistributionPanelUI, Instance

### Community 41 - "st_build"
Cohesion: 0.18
Nodes (16): animate(), build_body(), build_chimney_base(), build_collar(), build_fan(), build_hatch(), build_head(), build_lever() (+8 more)

### Community 42 - "TextMeshProFloatingText"
Cohesion: 0.11
Nodes (12): Benchmark01, Benchmark02, Benchmark03, BenchmarkType, TEXTMESH_BITMAP, TMP_BITMAP_MOBILE, TMP_SDF, TMP_SDF_MOBILE (+4 more)

### Community 44 - "Airlock Blender Build"
Cohesion: 0.17
Nodes (12): build_connector(), build_fan(), build_leaf(), dome_surface_y(), door_leaf_poly(), finish2(), fix_normals(), hollow_y() (+4 more)

### Community 46 - "TMP Text Event Handler"
Cohesion: 0.12
Nodes (11): TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, onCharacterSelection, onLineSelection, onLinkSelection, onSpriteSelection (+3 more)

### Community 47 - "UI Manager"
Cohesion: 0.12
Nodes (15): UIManager, ev_updatePlayerContext, Instance, PlayerContext, PlayerInventory, PlayerStats, UI, ParentUI (+7 more)

### Community 48 - "EnergyDevice"
Cohesion: 0.09
Nodes (13): DisplayName, EnergyDevice, Cables, CanSwitch, ConnectionPoint, DeviceState, DisplayName, ForcedOff (+5 more)

### Community 49 - "PlayingMode"
Cohesion: 0.15
Nodes (9): ModeManager, Instance, Mode, PlayingMode, Build, Default, Interface, Menu (+1 more)

### Community 50 - "cp_build.py"
Cohesion: 0.14
Nodes (15): animate(), build_body(), build_hologram(), sbox(), sp(), build_projector(), cham(), clear() (+7 more)

### Community 51 - "Vector3"
Cohesion: 0.14
Nodes (3): DomeConnection, InteractionDistance, Structures et catalogue

### Community 52 - "PlayerInputActions"
Cohesion: 0.14
Nodes (11): @Actions, ActionsActions, @Build, @ChangeHandedItem, @CloseInterfaces, @Drop, enabled, @Interract (+3 more)

### Community 54 - "SolarPanelAnimator"
Cohesion: 0.12
Nodes (8): SolarPanelAnimator, DeployProgress, HeadingAngle, IsDeployed, IsRotating, PanelFront, PanelUp, F65 - Intégration des modèles 3D

### Community 55 - "Main Menu UI"
Cohesion: 0.17
Nodes (3): MainMenuUI, SaveSlotInfo, Menu principal

### Community 56 - "00 - Index Pipeline 3D.md"
Cohesion: 0.10
Nodes (14): Formes et silhouettes, Matériaux et textures, Construction des modèles, Méthode technique, Pipeline 3D (Blender → Unity), Sommaire, Façon de travailler, Palette des matériaux (+6 more)

### Community 57 - "st_build (2)"
Cohesion: 0.23
Nodes (12): build(), cabinet_body(), cabinet_doors(), cham(), clear(), collisions(), crate_body(), crate_lids() (+4 more)

### Community 58 - "DrillAnimator"
Cohesion: 0.19
Nodes (5): DrillAnimator, DustPoint, IsBusy, IsDrilling, Powered

### Community 59 - "SaveableObject"
Cohesion: 0.15
Nodes (6): SaveId, SaveableObject, Id, IsRegistered, IsRuntime, PrefabKey

### Community 60 - "Dome"
Cohesion: 0.16
Nodes (6): Dome, ConnectorPrefab, Connectors, InnerWallRadius, Model, Radius

### Community 61 - "dependencies"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, url (+18 more)

### Community 62 - "bmesh"
Cohesion: 0.36
Nodes (6): check(), check2(), faces_of(), islands_of(), overlap_area(), Erreurs à ne pas refaire

### Community 63 - "TMP_UiFrameRateCounter"
Cohesion: 0.15
Nodes (7): Benchmark01_UGUI, TMP_UiFrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight

### Community 64 - "TMP_FrameRateCounter"
Cohesion: 0.13
Nodes (12): TMP_FrameRateCounter, FpsCounterAnchorPositions, BottomLeft, BottomRight, TopLeft, TopRight, TMPro_InstructionOverlay, FpsCounterAnchorPositions (+4 more)

### Community 65 - "PlayerContext"
Cohesion: 0.12
Nodes (7): PlayerContext, PlayerManager, PointedObject, PlayerInterractor, PlayerPointer, Source, Historique - Optimisations

### Community 66 - "DayNightCycle"
Cohesion: 0.11
Nodes (13): DayNightCycle, Day, Hour, Instance, IsDay, LightLevel, NightBrightness, NoonSunHeight (+5 more)

### Community 68 - "Données des items.md"
Cohesion: 0.16
Nodes (4): F56 - Horaires du tableau de distribution, Objets posés, Base de dômes, Dans le jeu aujourd'hui

### Community 69 - "BuildingPowerPanel"
Cohesion: 0.09
Nodes (16): BuildingOutlet, Building, OptionalDemand, SaveKey, BuildingPanelTerminal, Panel, BuildingPowerPanel, Building (+8 more)

### Community 70 - "PlayerMovement"
Cohesion: 0.12
Nodes (5): PlayerMovement, forwardDirection, IsSprinting, Stats, Déplacement du joueur

### Community 71 - "WeatherSystem"
Cohesion: 0.08
Nodes (15): WeatherSystem, AmbientFactor, Instance, IsStorm, IsStormComing, Seed, SkyFactor, SolarFactor (+7 more)

### Community 73 - "ws_build.py"
Cohesion: 0.20
Nodes (11): arc_pt(), build_body(), build_screen(), clear(), rect(), rho(), screen_uv(), setup() (+3 more)

### Community 74 - "PlayerModeOptions"
Cohesion: 0.17
Nodes (11): PlayerModeOptions, Options, Commentaires, Composants, Conventions de code, Gameplay et modèles, Nommage, Robustesse (+3 more)

### Community 75 - "RelayAntennaAnimator"
Cohesion: 0.18
Nodes (4): RelayAntennaAnimator, IsAntennaExtended, IsDishDeployed, IsRotating

### Community 76 - "PlayerManager"
Cohesion: 0.14
Nodes (10): PlayerManager, Instance, Inventory, IsDead, Player, SpawnedFromSave, Stats, ViewSource (+2 more)

### Community 77 - "ControlPanel"
Cohesion: 0.13
Nodes (10): ControlPanel, Consumer, DisplayName, IsPowered, Queue, Carte, ControlPanelUI, Panneau de contrôle (+2 more)

### Community 78 - "ParentedUI"
Cohesion: 0.12
Nodes (3): PointedInterractorInformationUI, HandedCursorUI, ParentedUI

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
Cohesion: 0.17
Nodes (4): WindTurbineAnimator, IsDeployed, WindStrength, Scripts d'animation C# (runtime)

### Community 85 - "CraftingStation"
Cohesion: 0.06
Nodes (26): CraftingStationAnimator, HologramScreen, IsPowered, IsPrinting, Crafter, CraftingStation, Current, IsUsable (+18 more)

### Community 86 - "cs_anim"
Cohesion: 0.33
Nodes (13): build_anim(), clear(), head_clearance(), ik(), k_arm(), k_head(), k_laser(), k_plate() (+5 more)

### Community 87 - "dr_build"
Cohesion: 0.19
Nodes (14): animate(), build_body(), build_carriage(), build_head(), build_hologram(), sbox(), sp(), collisions() (+6 more)

### Community 88 - "VertexJitter"
Cohesion: 0.12
Nodes (3): VertexJitter, VertexAnim, VertexShakeB

### Community 89 - "PlayerInputActions (3)"
Cohesion: 0.22
Nodes (6): @Movement, MovementActions, enabled, @Jump, @Move, @Sprint

### Community 91 - "EnergyConsumer"
Cohesion: 0.11
Nodes (9): EnergyConsumer, Consumption, Demand, IsPowered, State, TestMachine, Items du réseau, Objets de test (+1 more)

### Community 92 - "StorageContainer"
Cohesion: 0.16
Nodes (9): StorageContainer, Above, Below, SaveKey, Stackable, StackPoint, StackSize, State (+1 more)

### Community 93 - "ItemStorage"
Cohesion: 0.08
Nodes (18): ChargingStation, Consumption, Storage, DeathBag, ItemSlotSaveData, Item, Data, Gauge (+10 more)

### Community 94 - "Stat (2)"
Cohesion: 0.16
Nodes (6): Stat, MaxValue, Value, StatMarker, Duration, Modifier

### Community 95 - "00 - Index Game Design.md"
Cohesion: 0.07
Nodes (34): F30 - Biomes, F47 - Relais radio, Environnement, Traces de vie ancienne, Colons, Direction artistique, La chaleur qui s'installe, Points ouverts (+26 more)

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
Cohesion: 0.13
Nodes (5): SceneLoadingManager, DeathScreenUI, Instance, IsOpen, Écran de mort

### Community 102 - "PlaceableItemData"
Cohesion: 0.10
Nodes (15): PlacedObject, Data, IPlacementPreview, PreviewModel, PlaceableItemData, NotInCorridors, OutdoorOnly, PlacedPrefab (+7 more)

### Community 103 - "StructureData"
Cohesion: 0.05
Nodes (32): AirlockDoor, Airlock, Outer, BuildWheelUI, Instance, IsOpen, AirlockData, SnapDistance (+24 more)

### Community 104 - "Historique - Refontes"
Cohesion: 0.07
Nodes (27): Pickupable, SaveKey, SaveState, EquipmentData, Slot, EquipmentSlot, Back, Body (+19 more)

### Community 105 - "TerrainEditSaveData"
Cohesion: 0.38
Nodes (3): TerrainEditSaveData, TerrainFlattening, Terrain sous les constructions

### Community 107 - "00 - Index Fonctionnalités.md"
Cohesion: 0.10
Nodes (8): B21 - Curseur visible après Échap, F03 - Accroupissement, F11 - Données propres à chaque exemplaire, F37 - Oxygène hors des zones respirables, F38 - Position du sac de mort, R07 - CharacterController, R08 - Refonte du HUD des stats, Inventaire du joueur

### Community 109 - "PlayerInputActions (4)"
Cohesion: 0.26
Nodes (4): @Camera, CameraActions, enabled, @Rotation

### Community 111 - "EnergyStorage"
Cohesion: 0.09
Nodes (18): EnergyGenerator, Generation, Output, Production, EnergyStorage, Capacity, Charge, Discharge (+10 more)

### Community 112 - "StatBarUI"
Cohesion: 0.23
Nodes (5): PlayerStatsUI, StatBarUI, StatUIData, R05 - Prefab asset modifié au runtime, Stats du joueur

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
Cohesion: 0.15
Nodes (9): LandingCapsuleAnimator, IsBusy, IsOpen, LandingCapsule, IsOpen, SaveKey, LandingCapsuleDoor, Capsule (+1 more)

### Community 117 - "Intention et piliers du style"
Cohesion: 0.67
Nodes (3): Intention, Intention et piliers du style, Les piliers du style

### Community 118 - "TerrainMask"
Cohesion: 0.29
Nodes (5): TerrainMask, Matrix, Shape, Box, Cylinder

### Community 119 - "IInterractable"
Cohesion: 0.24
Nodes (8): EnergyDeviceInfo, Interractable, IInterractable, Searchable, Rangements, Panneau solaire, Éolienne, Interactions

### Community 120 - "Technologie humaine"
Cohesion: 0.20
Nodes (10): Dômes de verre et tunnels, La capsule, Outils du joueur, Principes, Production, Rations, Robots et IA, Serres (+2 more)

### Community 121 - "wt_build"
Cohesion: 0.32
Nodes (11): animate(), fold(), mast(), build_base(), build_blade(), build_mast(), build_nacelle(), build_rotor() (+3 more)

### Community 122 - "TMPro"
Cohesion: 0.18
Nodes (3): TMP_DigitValidator, TMP_PhoneNumberValidator, TMPro

### Community 124 - "StorageAnimator"
Cohesion: 0.16
Nodes (5): StorageAnimator, CanOpen, IsBusy, IsOpen, StackPoint

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

### Community 132 - "EnergyGenerator"
Cohesion: 0.14
Nodes (7): SolarPanel, Generation, WindTurbine, Generation, Protecting, Wind, F49 - Génération d'énergie

### Community 133 - "PlayerStats"
Cohesion: 0.16
Nodes (10): PlayerStats, StatName, Endurance, Health, Hunger, Oxygen, Thirst, SerializableKeyValuePair (+2 more)

### Community 134 - "ObjectSpin"
Cohesion: 0.25
Nodes (5): ObjectSpin, MotionType, Rotation, SearchLight, Translation

### Community 135 - "IStorageMachine"
Cohesion: 0.13
Nodes (11): PickupInfoGroupUI, Group, PickupInfoUI, IStorageMachine, CanSwitchMachine, Gauge, GaugeText, MachineName (+3 more)

### Community 136 - "BuildingInterior"
Cohesion: 0.29
Nodes (4): BuildingInterior, Radius, Zone, Intérieur des bâtiments

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
Cohesion: 0.50
Nodes (4): ItemGaugeUnit, Energy, Oxygen, Wear

### Community 144 - "NotificationUI"
Cohesion: 0.19
Nodes (7): NotificationType, Info, Warning, NotificationUI, Instance, Message, Messages au joueur

### Community 145 - "PauseMenuUI"
Cohesion: 0.17
Nodes (5): Blocked, PauseMenuUI, Instance, IsOpen, Menu pause

### Community 146 - "À faire"
Cohesion: 0.25
Nodes (8): Bugs, Features, Historique, Identifiants, Optimisations, Prochains numéros libres, Refontes, À faire

### Community 148 - "Couleur et lumière"
Cohesion: 0.50
Nodes (4): Couleur et lumière, Extérieur, par zone, Intérieur des dômes, Le cycle jour / nuit est un outil émotionnel

### Community 149 - "Objets persistants"
Cohesion: 0.13
Nodes (11): Root, SceneContext, SceneLoader, Fonctionnement actuel, Scènes, Scènes et chargement, Objets persistants, Ce que l'IA vérifie (+3 more)

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

### Community 158 - "ToolData.cs"
Cohesion: 0.50
Nodes (3): ToolConsumptionMode, Continuous, PerUse

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

### Community 192 - "SaveManager.UnloadObjects"
Cohesion: 0.40
Nodes (3): ChunkManager (singleton, scène Monde), Terrain procédural, TerrainGenerator (sur le prefab `Chunk`)

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

### Community 206 - "WeatherState"
Cohesion: 0.50
Nodes (4): WeatherState, Clear, Storm, StormComing

### Community 207 - "Points ouverts du game design"
Cohesion: 0.50
Nodes (4): Décisions en attente, Points ouverts du game design, Questions relevées en documentant le concept *(proposition)*, Écarts entre le concept et le jeu actuel

### Community 208 - "Space Survival (nom provisoire)"
Cohesion: 0.50
Nodes (4): Décisions en attente, Les parties du coffre, Pour commencer, Space Survival (nom provisoire)

### Community 210 - "MonoBehaviour"
Cohesion: 0.10
Nodes (8): Benchmark04, ChatController, DropdownSample, TMP_ExampleScript_01, objectType, TextMeshPro, TextMeshProUGUI, Bootstrapper

### Community 211 - "Outils scientifiques"
Cohesion: 0.67
Nodes (3): Outils scientifiques, Scanner, Table d'observation

### Community 212 - "Instructions"
Cohesion: 0.67
Nodes (3): Domaines, Instructions, Règles communes

## Knowledge Gaps
- **446 isolated node(s):** `SlotsUI`, `Element`, `Data`, `Gauge`, `GaugeRatio` (+441 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1030 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **63 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Airlock` connect `Corridor` to `Transform`, `StructureData`, `Airlock Animator`, `ConnectorPreview.Destroy`, `system`, `add_revolve`, `MonoBehaviour`, `Vector3`, `CraftingStation`, `SolarPanelAnimator`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.149) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `Builder` (e.g. with `Historique - Bugs` and `Historique - Optimisations`) actually correct?**
  _`Builder` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `SlotsUI`, `Element`, `Data` to the rest of the system?**
  _446 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `00 - Index À faire.md` be split into smaller, more focused modules?**
  _Cohesion score 0.08902439024390243 - nodes in this community are weakly interconnected._
- **Why does `Modélisation Blender` connect `add_revolve` to `EnergyGenerator`, `Corridor`, `Airlock Blender Build`, `Scripts de référence`, `oct_cyl`, `00 - Index Pipeline 3D.md`, `Tunnel Blender Build`?**
  _High betweenness centrality (0.141) - this node is a cross-community bridge._
- **Should `system_collections_generic` be split into smaller, more focused modules?**
  _Cohesion score 0.06439393939393939 - nodes in this community are weakly interconnected._
- **Why does `@PlayerInputActions` connect `Input Actions` to `CallbackContext`, `PlaceableItemData`, `PlayerInputActions (4)`, `unityengine_inputsystem`, `PlayerInputActions`, `PlayerInputActions (3)`, `Map Utility & Minimap`?**
  _High betweenness centrality (0.063) - this node is a cross-community bridge._