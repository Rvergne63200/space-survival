---
tags: [jeu-colonie, pipeline-3d, unity, animation]
---

# Animator Unity

- Le controller est **généré automatiquement** par `CreateController` : ne pas le construire à la main.
- **Modèle simple** (CraftStation, Airlock) :
  - une couche, un état par clip ;
  - état par défaut dans `DefaultState` (CraftStation → Power_Off, Airlock → Inner_Close, LandingCapsule → Door_Close, Crate_Small / Crate_Large → Lid_Close, Cabinet → Doors_Close, ControlPanel → Power_Off) ;
  - transitions automatiques de fin de clip dans `AutoTransitions` (Print_Start → Print_Loop).
- **Modèle à mouvements indépendants** (`ModelLayers`) :
  - **une couche par groupe**, chacune avec ses objets cibles, ses clips et son état par défaut. Exemple pour SolarPanel :
    - couche `Panel` : Panel_Tilt, Wing_L, Wing_R ;
    - couche `Rotation` : Panel_Yaw ;
  - `OnPostprocessAnimation` **retire de chaque clip les courbes des objets d'une autre couche**, sinon une couche écraserait l'autre ;
  - **Write Defaults désactivé** sur tous les états, et toutes les couches à weight 1 ;
  - une couche dont `defaultClip == null` démarre sur un état **`Idle` vide** ;
  - chaque clip qui boucle reçoit un paramètre float **`<Couche>Speed`** (défaut 1). Exemples : `RotationSpeed`, `RotorSpeed`.
- Couches actuelles :
  - **RelayAntenna :**
    - `Dish` {Arm} ;
    - `Mast` {Mast_Mid, Mast_Top} ;
    - `Rotation` {Turret}.
  - **SolarPanel :**
    - `Panel` {Panel_Tilt, Wing_L, Wing_R} ;
    - `Rotation` {Panel_Yaw}.
  - **WindTurbine :**
    - `Deploy` {Mast_Mid, Mast_Top, Blade_1..3} ;
    - `Rotor` {Rotor}.
  - **CoalGenerator :**
    - `Hatch` {Hatch}, défaut Hatch_Close ;
    - `Power` {Lever}, défaut Power_Off ;
    - `Fan` {Fan}, Idle, paramètre `FanSpeed`.
  - **Drill :**
    - `Feed` {Carriage}, défaut Drill_Raise ;
    - `Spin` {Drill_Head}, Idle, paramètre `SpinSpeed`. `Drill_Head` est l'**enfant** de `Carriage` : la descente et la rotation se combinent sans se gêner.

[[00 - Index Pipeline 3D|Index]] · ← [[13 - Animations Blender]] · [[15 - Scripts d'animation Unity]] →
