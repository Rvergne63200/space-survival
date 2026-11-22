---
tags: [jeu-colonie, pipeline-3d, blender, animation]
---

# Animations Blender

- **Une seule timeline globale par modèle**, à 24 fps. Les clips se suivent avec **10 à 18 frames vides** entre eux, et le découpage se fait dans Unity ([[12 - Import Unity]]).
- Les clips vont **par paires aller / retour** (`X_Deploy` / `X_Fold`, `Outer_Open` / `Outer_Close`, `Antenna_Extend` / `Antenna_Retract`) :
  - chaque clip commence et finit sur des **poses stables** ;
  - la fin de l'un égale le début de l'autre.
- Les **boucles** se nomment `*_Loop` ou `*_Spin` :
  - interpolation **LINEAR** ;
  - de 0 à ±360° exactement, première frame identique à la dernière ;
  - durée typique : 1 tour en 2 à 4 s.
- **Nommage des clips :** `<Partie>_<Action>` en PascalCase. Exemples : `Dish_Deploy`, `Rotate_Loop`, `Rotor_Spin`, `Inner_Close`, `Power_On`, `Print_Loop`.
- **Une pièce mobile = un objet = une seule propriété animée**, de préférence (rotation sur un axe, ou translation).
- **Accès aux courbes, Blender 5.3 (layered actions) :**
  ```python
  action.layers[0].strips[0].channelbag(ob.animation_data.action_slot).fcurves
  ```
- **Mouvements indépendants** (par exemple : déployer **et** tourner en même temps) :
  - chaque groupe anime des **objets distincts**, comme `Panel_Yaw` pour la rotation et `Panel_Tilt` / `Wing_*` pour le déploiement ;
  - ils deviendront des **couches** séparées dans l'Animator.
- **Vérifications obligatoires** avant export :
  1. **Collisions** entre pièces mobiles : test de chevauchement `BVHTree.overlap`, échantillonné sur toutes les frames des clips (au moins toutes les 2 frames). C'est surtout important pour les pliages (panneaux, pales, portes).
  2. **Z-fighting :** `zfight.check2(...)` sur la pose de repos et sur les poses clés.
  3. **Rendus des poses clés** envoyés à l'utilisateur. Pour rendre une pose manuelle, **détache d'abord les actions** : le rendu réévalue l'animation.

**Tables actuelles** (voir `ModelClips` dans `ModelImportSetup.cs`) :

| Modèle | Clips (frames) |
|---|---|
| CraftStation | Power_On 1–49, Power_Off 60–108, Print_Start 120–128, Print_Loop 130–322 (loop), Print_End 330–338 |
| Airlock | Outer_Open 1–30, Outer_Close 40–70, Inner_Open 80–110, Inner_Close 120–150, Cycle 160–280 |
| RelayAntenna | Dish_Deploy 1–37, Dish_Undeploy 50–86, Antenna_Extend 100–124, Antenna_Retract 130–154, Rotate_Loop 170–266 |
| SolarPanel | Panel_Deploy 1–72, Panel_Fold 90–162, Rotate_Loop 180–276 |
| WindTurbine | Turbine_Deploy 1–96, Turbine_Fold 110–200, Rotor_Spin 220–268 |
| CoalGenerator | Hatch_Open 1–24, Hatch_Close 36–60, Power_On 72–84, Power_Off 96–108, Fan_Spin 120–168 (loop) |
| LandingCapsule | Door_Open 1–48, Door_Close 60–108 (une couche, défaut Door_Close) |
| Drill | Drill_Lower 1–48, Drill_Raise 60–108, Drill_Spin 120–168 (loop) |
| Crate_Small | Lid_Open 1–24, Lid_Close 36–60 (défaut Lid_Close) |
| Crate_Large | Lid_Open 1–30, Lid_Close 42–72 (deux volets papillon, défaut Lid_Close) |
| Cabinet | Doors_Open 1–30, Doors_Close 42–72 (deux portes, défaut Doors_Close) |
| ControlPanel | Power_On 1–24 (barre 1–16, puis hologramme déployé 16–24), Power_Off 36–60 (hologramme replié 36–44, puis barre 44–60), défaut Power_Off |

[[00 - Index Pipeline 3D|Index]] · ← [[12 - Import Unity]] · [[14 - Animator Unity]] →
