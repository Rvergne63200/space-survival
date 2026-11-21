# Space Survival : règles de production 3D (Blender → Unity)

Ce document est la référence pour toute session qui modélise, anime ou intègre un modèle dans le projet **Space Survival**.
Ces règles ne valent **que pour ce projet** : les autres projets ont leur propre DA.

- Projet Unity : `F:\Users\Richard\Documents\Richard\Dev\Jeux\Projets Unity\Space Survival`.
  - Unity 6000.4, **Built-in Render Pipeline**, shader **Standard** (pas URP ni HDRP).
- Blender 5.3 en **interface française**, piloté par le serveur MCP Blender.
  - Pour le lancer : Édition > Préférences > Add-ons > MCP > « Start MCP Bridge Server ».
- Scripts Blender de référence : `ArtPipeline/Blender/` (voir §9). **Toujours repartir de ces helpers** au lieu de réinventer la modélisation.
- Script d'import Unity : `Assets/Scripts/Editor/ModelImportSetup.cs`.
- Scripts d'animation runtime : `Assets/Scripts/Behaviours/Animations/<Model>Animator.cs`.

---

## 0. Façon de travailler avec l'utilisateur

- L'utilisateur écrit en français : réponds en français.
- Quand il dit « si tu as des questions, demande avant de commencer », pose les questions **avant** de modéliser, en proposant des choix concrets (taille, type de mécanisme, animations voulues…).
- Il itère visuellement. Après chaque modèle :
  - envoie un ou plusieurs **rendus** (vue 3/4, et poses d'animation si le modèle est animé) ;
  - exporte **directement** dans le projet Unity.
- Signale honnêtement ce qui n'a pas été vérifié dans Unity : tu ne peux pas voir l'éditeur Unity.
- Un modèle validé ne se « redessine » pas sans demande. On corrige le point signalé, et rien d'autre.

---

## 1. Direction artistique (DA)

### 1.1 Esprit général
- **Inspiration Subnautica** (bases sous-marines / spatiales modulaires), mais avec une DA propre : ne jamais copier un asset existant.
- Industriel propre, high-tech et lisible.
- Silhouettes simples, volumes francs, détails concentrés sur les bandes, les joints et les panneaux.

### 1.2 Palette (matériaux unis, **pas de textures** pour l'instant)
Chaque modèle a ses propres matériaux nommés **`M_<Model>_<Suffixe>`**. Le suffixe pilote la création automatique du `.mat` Unity.

| Suffixe | Rôle | Blender Base Color (linéaire) | Émission Blender |
|---|---|---|---|
| `_White` | Coque principale, carénages (couleur dominante) | (0.82, 0.85, 0.88) | — |
| `_Dark` | Graphite : cadres, brides, bandes, socles, joints, intérieurs de passage | (0.035, 0.045, 0.06) | — |
| `_Metal` | Métal gris foncé : sols, vis, axes, grilles, charnières | (0.16, 0.18, 0.22) | — |
| `_Neon` | Lignes lumineuses **bleues** (signature de la DA) | (0.55, 0.92, 1.0) | (0.05, 0.65, 1.0) × 3 |
| `_Orange` | **Quelques** petites lumières orange : balises, voyants | (1.0, 0.38, 0.03) | (1.0, 0.25, 0.0) × 3 |
| `_Glow` | Écrans / lueur intérieure bleu foncé | (0.1, 0.4, 0.85) | (0.01, 0.2, 0.55) × 3 |
| `_Plate` | Plaques métal clair | (0.55, 0.65, 0.75) | — |
| `_Hazard` | Bandes de danger jaune-orange (seuils, poignées) | (0.95, 0.6, 0.04) | — |
| `_Status` | Voyant dont la couleur est pilotée par script (orange ↔ bleu) | (1.0, 0.45, 0.05) | (1.0, 0.3, 0.0) × 3 |
| `_Glass` | Vitres (transparent) | (0.25, 0.6, 0.8), Alpha 0.35, Roughness 0.08 | — |
| `_Cell` | Cellules solaires bleu nuit | (0.03, 0.08, 0.22), Roughness 0.15, Metallic 0.3 | — |
| `_Fire` | Foyer visible derrière un hublot (générateur à charbon) ; intensité pilotée par script | (1.0, 0.30, 0.04) | (1.0, 0.22, 0.02) × 4 |
| `_Hologram`, `_Laser` | Effets spécifiques (station de craft) | voir `cs_v5.py` | — |

**Proportions et règles d'usage :**
- **Le blanc domine** (environ 60 à 70 % de la surface visible) et le graphite structure (environ 20 à 30 %).
- Le **néon bleu** est fin : des lignes ou filets de 2 à 5 cm. On en met une ou deux bandes par module, pas plus.
- L'**orange** reste rare : 1 à 4 petits voyants ou balises par modèle. Il ne devient jamais une couleur de surface.
- Le **`_Fire`** est réservé à un disque de foyer derrière une vitre (prisme `_Glass` 5 mm devant). Ce n'est pas une couleur de surface non plus.
- Le **hazard** est réservé aux seuils de porte, aux poignées et aux petits marquages.
- Roughness 0.5 partout dans Blender, sauf pour les vitres et les cellules.
- Mets toujours `m.diffuse_color` : c'est la couleur de prévisualisation dans le viewport.
- Dans Blender, cherche le nœud Principled **par type** (`n.type == 'BSDF_PRINCIPLED'`), jamais par nom : l'interface est en français.

La palette Unity équivalente est dans `ModelImportSetup.cs` → `FlatPalette`. Si tu inventes un nouveau suffixe, **ajoute-le à `FlatPalette`**, sinon Unity ne crée pas le matériau.

### 1.3 Formes
- **Pas d'arête vive à 90°.** Toutes les arêtes ont :
  - soit un **large chanfrein** (à 45°, de 2 à 80 cm selon la taille de la pièce) ;
  - soit un petit **edge break** de 6 mm (`EB = 0.006`) sur toutes les arêtes restantes.
- **Facettes plutôt que formes lisses.** Les cylindres sont octogonaux (`oct_cyl`, 8 côtés, ou 12 pour les grands). Les dômes sont géodésiques à triangles visibles, jamais des sphères lisses.
- **Section standard des modules habitables :** profil à 6 côtés (`prof6`), avec fond plat, flancs verticaux et deux grands chanfreins à 45° en haut.
- Le détail se fait par **petits éléments rapportés**, posés 1 à 6 mm au-dessus de la surface :
  - panneaux inset graphite, grilles d'aération (lamelles `_Metal`) ;
  - vis (`add_screw`) sur les brides ;
  - filets néon (`pill`), bandes graphite autour des caissons ;
  - balises orange sur le toit, petites consoles (écran `_Glow` et boutons).
- Les **socles** sont graphite, avec des pieds ou une jupe qui s'enfonce légèrement dans le sol (z < 0) pour ne pas « flotter ».
- **Shading :** `shade_smooth()` puis `set_sharp_from_angle(35°)`, ce qui donne un rendu facetté avec des chanfreins nets. Les vitres utilisent 5°.

### 1.4 Dimensions de référence
- Le joueur mesure environ 1,80 m. Le sol de marche intérieur est à **0.215 m** : sol du dôme à 0.20, plus 1,5 cm de seuil.
- **Murs pleins :** 1.20 m de haut maximum, soit à peu près à mi-hauteur du personnage. Au-dessus, c'est **vitré**.
- **Bride standard**, la même pour le connecteur de dôme, le sas et tous les tunnels. Profils `(x, z)` :
  - Passage libre : `PASS = prof6(0.90, 0.215, 2.60, 0.40)`, soit 1,80 m de large et 2,60 m de haut.
  - Coque : `SHELL = prof6(1.30, 0.00, 2.95, 0.55)`.
  - Bride : `offset_poly(SHELL, 0.15)`, épaisseur 0.12, avec des vis tous les 55 cm.
- Tout nouveau module qui se raccorde au réseau **doit réutiliser cette bride**. Il se chaîne par des empties enfants nommés **`Socket`** (axe de sortie = -Y Blender).

- **Passages praticables :** le collider du joueur fait environ **0,85 m de diamètre et 1,70 m de haut** (CapsuleCollider de rayon 0,5 et de hauteur 2, à l'échelle 0,85 dans `Player.prefab`). Une porte de module autonome doit faire au moins 1,1 m de large et 1,9 m de haut.

### 1.4 bis Modèles habitables autonomes (capsule)
- **Coque creuse à facettes** : une bande de facettes (anneau ouvert, extérieur et intérieur `_White`, tranches `_Dark`), plus la facette de la porte construite en 4 pièces (allège, linteau, deux jambages) avec `panel()` / `hexa()` de `lc_build.py`. Ces helpers placent n'importe quel élément dans le plan d'une facette inclinée (`fpt(angle, u, z, profondeur)`).
- **Porte affleurante qui se rabat en rampe** :
  - charnière sur l'axe X, **4,5 cm à l'extérieur de la face de l'allège** (le bas du vantail ne frotte jamais) ;
  - angle d'ouverture **calculé** (`open_angle()`) pour que le point le plus bas du vantail touche le sol ;
  - le haut d'un vantail qui pivote vers l'extérieur **se soulève** de quelques centimètres en passant par la verticale. Il faut donc arrêter le vantail sous le linteau (7,5 cm ici) et masquer la fente par un rebord intérieur, côté cabine, que le vantail ne balaie pas ;
  - la face intérieure du vantail devient la surface de marche : y mettre les marches anti-dérapantes, les rails et la bande hazard au bout.
- **Intérieur vide** quand l'utilisateur l'aménage lui-même dans Unity : sol `_Metal` avec bordure graphite, nervures graphite aux angles, plafond blanc, un anneau néon. Pour les rendus, une lumière temporaire `_InteriorLight` (préfixe `_`, jamais exportée).

### 1.4 ter Stockage et objets empilables
- **Couvercle couvrant tout le dessus.** Les poteaux d'angle s'arrêtent sous le couvercle (8 mm sous le rebord, sans plan partagé). Un couvercle découpé entre les poteaux laissait voir l'intérieur par des fentes.
- **Empilage** : plots de centrage graphite sur le couvercle, au-dessus des poteaux ; empty **`StackPoint`** à `hauteur du couvercle - bas du socle` (le socle descend à -0,005). La racine du caisson du dessus se pose sur ce point. Un caisson chargé ne s'ouvre pas : `StorageAnimator.CanOpen`.
- **Charnières** : l'axe passe **2 cm à l'extérieur du bord du couvercle** (`HINGE_OFF`). Les charnières fixes sont posées sur une équerre en L (un pied sous le rebord, un montant derrière le bord du couvercle), alternées avec celles du couvercle, qui s'y rattachent par une patte. Sinon, le couvercle « flotte » à l'ouverture, ou recoupe les charnières fixes.
- **Portes battantes** : décoller la porte de 6 mm de la façade, et vérifier que la charnière (rayon ≤ 1,5 cm) ne recoupe pas le chanfrein d'angle du corps en tournant.
- **Corps creux** : `hollow_z()` (bac ouvert en haut) et `solid()` (polygone en U extrudé pour une armoire ouverte en façade) dans `st_build.py`. L'intérieur est graphite.

### 1.5 Vitres
- Ce sont des **prismes fermés minces** (deux faces et une tranche), jamais des plans simples : les plans simples donnaient des normales incohérentes et des vitres manquantes.
- Le matériau `_Glass` est en Blended dans Blender, avec `use_backface_culling = False`. Dans Unity, c'est un Standard Transparent créé automatiquement.

### 1.6 Textures
- **En pause.** Tout est en matériaux unis.
- `ModelImportSetup` gère déjà les textures (`<Model>_BaseColor`, `_Normal`, `_MetallicSmoothness`, `_Occlusion`, `_Emission` en `.png`) si on les réactive un jour. Ne pas en produire sans demande.

---

## 2. Méthode de modélisation (Blender, procédural)

- Tout est **généré par script Python (bmesh)**, jamais modélisé à la main.
  - Un script `xx_build.py` par modèle contient la géométrie, la hiérarchie et l'animation.
  - Il s'appuie sur la chaîne de helpers `cs_helpers.py` → `cs_v5.py` → `al_build.py`, chargée par `exec(open(SP + r"\al_build.py").read())`.
- **Helpers disponibles** (voir les fichiers) :
  - **Volumes simples :**
    - `cbox(bm, size, center, mat, ch, eb, rot)` : boîte chanfreinée ;
    - `pill` : bouton ou filet arrondi ;
    - `prism_x` / `prism_y` : extrusion d'un profil 2D, avec chanfrein des bouts et edge break ;
    - `oct_cyl` : cylindre facetté ;
    - `add_revolve` : révolution d'un profil.
  - **Éléments linéaires :**
    - `beam(p0, p1, w, h)` : poutre entre deux points ;
    - `tube` : tuyau ;
    - `add_screw` : vis.
  - **Sections et passages :**
    - `prof6`, `offset_poly`, `densify` ;
    - `hollow_y(bm, outer, inner, y0, y1, mo, mi, mc)` : cadre creux extrudé. Matériaux par arête possibles, par exemple sol `_Metal` et murs `_White`.
  - **Finalisation :**
    - `fix_normals(bm)` : normales sortantes par îlot fermé, selon le signe du volume ;
    - `finish2(name, bm, col, parent, mats, origin)` : crée l'objet, retire les slots inutilisés et **réapplique les indices de matériaux**.
  - **Contrôle :** `zfight.py` → `check2(objs, skip)` détecte les faces coplanaires qui se chevauchent.
  - **Spécifiques :** `geodesic.py` (dômes) ; `tn_build.py` (`section`, `path_frame`, `sweep` pour les coudes et jonctions).
- **Convention d'axes :**
  - z = 0 est le sol ;
  - l'**avant / l'extérieur / le sens de passage** est **-Y Blender**, ce qui donne **+Z Unity** ;
  - X reste X.
- **Hiérarchie :**
  - un **empty racine** au nom exact du modèle (`SolarPanel`, `Airlock`…) ;
  - des enfants mesh nommés en PascalCase avec underscores (`Base`, `Panel_Yaw`, `Door_Outer_L`, `Blade_1`) ;
  - chaque pièce mobile est un objet séparé, avec son **origine sur son pivot** ;
  - les pièces fixes sont fusionnées dans `Body` / `Base`.
- **Collection** `Models`. Les objets dont le nom commence par `_` (caméra, lumières de preview) ne sont ni supprimés ni exportés.
- **Un `.blend` par modèle ou par kit**, sauvegardé dans **`ArtPipeline/Blends/<Model>.blend`** (les blends existants y sont). Ne jamais le mettre sous `Assets/`.
- **Budget indicatif :** de quelques milliers à environ 20 000 triangles par module.

---

## 3. Export (Blender → Unity)

- Dossier : **`Assets/Resources/Models/<Model>/`**, un dossier par modèle, contenant **uniquement ce qu'il faut à Unity** : le `.fbx`, puis les `.mat` et `.controller` générés automatiquement.
  - Pas de `.blend`, de rendu ni de script dans ce dossier.
  - Un **kit** partage un dossier : `Tunnel/` contient `Tunnel_Straight`, `Tunnel_Extension`, `Tunnel_Elbow45`, `Tunnel_Elbow90`, `Tunnel_T` et `Tunnel_Cross`.
  - Ne **rien** poser à la racine `Models/`.
- **Nom du FBX = nom de l'empty racine = nom du dossier** (sauf pour un kit). Ce nom sert de clé dans `ModelImportSetup.cs`.
- Paramètres d'export FBX (obligatoires) :
  ```python
  bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True,   # (ou use_active_collection=True)
      object_types={'EMPTY', 'MESH'},
      apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=False,
      axis_forward='-Z', axis_up='Y',
      mesh_smooth_type='FACE', add_leaf_bones=False, path_mode='RELATIVE',
      # modèles animés seulement :
      bake_anim=True, bake_anim_use_nla_strips=False, bake_anim_use_all_actions=False,
      bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0)
  ```
  Pour un modèle statique : `bake_anim=False`.
- Avant l'export :
  - mets la racine à l'identité (`root.matrix_world = Matrix()`), puis restaure sa position ;
  - sélectionne uniquement la racine et ses descendants.
- **Noms uniques :** Blender renomme les doublons (`Socket.001`). Pour un kit, renomme temporairement les empties au moment de l'export pour que chaque FBX contienne `Socket`, et non `Socket.001`.
- **Vérifie après export :**
  - réimporte le FBX dans une scène temporaire ;
  - contrôle les clips et les courbes animées ;
  - supprime la scène temporaire.
- **Côté Unity**, pour un nouveau modèle :
  - **animé :** ajoute sa table de clips dans `ModelClips`, et si besoin `ModelLayers` / `DefaultState` ;
  - **statique :** rien à faire, il est importé sans animation ;
  - **réglages d'import modifiés :** incrémente `GetVersion()`.
- **Nommage imposé par l'utilisateur :**
  - `Dome_8m`, `Dome_12m`, `Dome_16m` ;
  - `DomeConnector_8m`, `DomeConnector_12m`, `DomeConnector_16m`.

---

## 4. Import Unity : `ModelImportSetup.cs`

Cet `AssetPostprocessor` s'applique automatiquement à tout ce qui est sous `Assets/Resources/Models/`.

- **TOUS les modèles** sont importés avec `bakeAxisConversion = false` et `preserveHierarchy = true`.
  - Sinon, le modèle **bascule de 90°** dès qu'un clip joue : la conversion d'axes n'est pas appliquée aux courbes d'animation.
  - Conséquence : sous la racine du prefab, **les enfants gardent leurs axes locaux Blender** (vertical = Z local, avant = -Y local). Les scripts C# doivent en tenir compte (voir §6).
- Les modèles **sans table de clips** sont importés avec `animationType = None` et `importAnimation = false`.
- **Matériaux :**
  - pour chaque matériau FBX nommé `M_*_<Suffixe>` connu, un `.mat` Standard est créé dans **le même dossier** ;
  - le remap se fait uniquement sur ce dossier, car `FindAssets` est récursif et doit être filtré ;
  - `_Glass` passe en Standard Transparent, `_Hologram` utilise `Custom/Hologram` ou un fallback en mode Fade.
- **Clips :**
  - le FBX contient **une seule timeline** ;
  - la table `ModelClips[model]` donne, pour chaque clip, `(nom, première frame, dernière frame, loop)` ;
  - pour les modèles qui ont une table, celle-ci écrase toujours les réglages manuels.
- **Animator Controller** : créé automatiquement **seulement s'il n'existe pas** (`<Model>.controller`). Pour le régénérer après un changement de couches, **supprime-le** : il est recréé.
- Les modèles `Dome_*` sont importés avec `isReadable = true`, pour la découpe du mur au runtime.
- Après toute modification du script, **vérifie qu'il compile** : cherche `error CS` dans `%LOCALAPPDATA%\Unity\Editor\Editor.log`. Une erreur de compilation empêche toute la pipeline de tourner, avec des clips périmés et sans controller.

---

## 5. Animations (Blender)

- **Une seule timeline globale par modèle**, à 24 fps. Les clips se suivent avec **10 à 18 frames vides** entre eux, et le découpage se fait dans Unity (§4).
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

---

## 6. Animator Unity

- Le controller est **généré automatiquement** par `CreateController` : ne pas le construire à la main.
- **Modèle simple** (CraftStation, Airlock) :
  - une couche, un état par clip ;
  - état par défaut dans `DefaultState` (CraftStation → Power_Off, Airlock → Inner_Close, LandingCapsule → Door_Close, Crate_Small / Crate_Large → Lid_Close, Cabinet → Doors_Close) ;
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

---

## 7. Scripts d'animation C# (runtime)

- Emplacement : **`Assets/Scripts/Behaviours/Animations/<Model>Animator.cs`**, avec un composant par modèle.
  - Existants : `CraftingStationAnimator`, `AirlockAnimator`, `RelayAntennaAnimator`, `SolarPanelAnimator`, `WindTurbineAnimator`, `CoalGeneratorAnimator`, `LandingCapsuleAnimator`, `DrillAnimator`, `StorageAnimator` (commun à Crate_Small, Crate_Large et Cabinet).
- **Un script par modèle**, sauf si la logique est strictement identique : un script commun détecte alors ses clips dans le controller (`*_Open` / `*_Close` pour `StorageAnimator`), au lieu de coder les noms en dur (demande de l'utilisateur).
- **L'Animator d'une instance doit recevoir son `.controller` à la main** (une instance posée depuis le FBX n'en a pas). Sinon, aucun clip ne joue et `Editor.log` affiche « Animator does not have an AnimatorController ». L'utilisateur ne veut pas que les scripts le chargent automatiquement.
- **État d'une translation interruptible** : mémoriser la **cible** dès le lancement du clip (`lowered = true` au début de `Drill_Lower`), pas à la fin. Sinon, un arrêt demandé en pleine descente croit le chariot encore en haut et ne le remonte pas.
- **Clips aller / retour symétriques** (même durée, même courbe) : si une commande inverse arrive en plein mouvement, repartir de la position atteinte avec `animator.Play(clipInverse, layer, 1 - normalizedTime)` (voir `LandingCapsuleAnimator`).
- Commentaires et `<summary>` **en français**, `[RequireComponent(typeof(Animator))]`. Le composant se place sur la **racine de l'instance**, à côté de l'Animator.
- **API publique claire**, avec des verbes : `Deploy()`, `Fold()`, `Toggle()`, `StartRotation()`, `StopRotation()`, `OpenOuter()`, `RequestEnterFromOutside()`… L'état se lit par des propriétés (`IsDeployed`, `IsBusy`, `Pressurized`…). Le gameplay n'appelle jamais les clips directement.
- **Lancement des clips :**
  - `animator.CrossFadeInFixedTime("Clip", 0.1f, layer)` pour lancer ;
  - `animator.Play("Clip", layer, 1f)` dans `Awake` pour poser l'état initial sans animation ;
  - attendre la fin avec `WaitForSeconds(Length("Clip"))`, où la durée est lue dans `runtimeAnimatorController.animationClips` ;
  - **séquences** en coroutines, avec une seule coroutine active (`Run()` arrête la précédente) ;
  - les commandes peuvent être **mises en file**, comme dans l'Airlock.
- **Boucles :** vitesse via le paramètre `<Couche>Speed`, avec une montée et une descente progressives (`MoveTowards`) dans `Update`.
- **⚠ Piège de l'Animator :** un Animator réécrit **à chaque frame** tous les transforms que ses clips animent. Avec Write Defaults désactivé, il garde la dernière valeur jouée. Donc :
  - un transform piloté **par script** (suivi du soleil, orientation au vent, alignement du rotor) doit garder **son propre angle** dans un champ (`yawAngle`, `nacelleAngle`, `rotorAngle`) ;
  - le script l'**impose de façon absolue dans `LateUpdate`** (`t.localRotation = rest * Quaternion.AngleAxis(angle, axeLocal)`) ;
  - ne jamais faire `t.Rotate(...)` en incrémental sur un transform que l'Animator touche ;
  - pour passer d'un clip de boucle au script, mesurer l'angle atteint (`MeasureYaw()` / `MeasureRotor()`) ;
  - pour passer du script au clip, `Play("Rotate_Loop", layer, Mathf.Repeat(angle/360, 1))` repart de l'angle courant.
- **Axes dans les scripts** : les enfants gardent les axes Blender (§4).
  - Axe vertical d'une pièce = `Vector3.forward` (Z local) ; avant = `Vector3.down` (-Y local) ; rotor = `Vector3.up` (Y local).
  - Calcule les caps avec `Vector3.SignedAngle` après `ProjectOnPlane` sur l'axe vertical monde du parent (voir `HeadingFor` / `Heading`).
- **Trouver les pièces** par nom dans `GetComponentsInChildren<Transform>(true)`, mémoriser la `localRotation` de repos dans `Awake`, et logger un warning si une pièce manque.
- **Voyants pilotés** (matériau `_Status`) : instancier le matériau du renderer et changer `_Color` / `_EmissionColor`. Orange = dépressurisé / alerte, bleu = OK, clignotant = en cours.
- **Sas** : la règle « jamais les deux portes ouvertes » et la pression requise pour chaque porte sont gérées dans `AirlockAnimator` (ouverture extérieure si dépressurisé, intérieure si pressurisé, cycle portes fermées).
- **Pièces à longueur variable** (cheminée du générateur, qui doit traverser un dôme) :
  - découpage : un socle fixe dans `Body`, un **fût** séparé (`Chimney_Shaft`, origine en bas, mesh de longueur connue le long de +Z, sans détail rond qui se déformerait à l'étirement), une **tête** séparée (`Chimney_Head`) **enfant de la racine, pas du fût** (sinon elle hérite de l'échelle), et un **collier** séparé (`Chimney_Collar`) placé ou masqué par script ;
  - le script étire `localScale.z` du fût (Z local = vertical) et place la tête et le collier avec `upPerMeter = (tête.localPosition - fût.localPosition) / longueurParDéfaut`, mesuré dans `Awake` : aucune hypothèse sur les axes ni les unités ;
  - ces transforms ne doivent être animés par **aucun** clip, sinon l'Animator les réécrirait.
- **Collier de traversée orientable** (cheminée qui sort d'un dôme) :
  - c'est un **disque graphite plein** (16 facettes, rayon = rayon du fût + 10 cm, chanfrein simple), **sans vis ni manchon**. Le fût le traverse visuellement, donc il n'y a pas de trou quand il pivote, et rien ne se déforme à l'étirement ;
  - le script de placement calcule la rotation (normale de la coque du dôme) et la passe à `SetCollarRotation(Quaternion)`, avec normale = `rotation * Vector3.up` ;
  - l'animator aligne le Z local du disque sur cette normale et son X local sur la ligne de plus grande pente, puis l'étire en ellipse : rayon `r / cos θ + débord` dans le sens de la pente, `r + débord` en travers. Il n'y a pas de limite d'inclinaison, seulement un garde-fou à 89° contre la division par zéro.
- **Points d'ancrage d'effets** : un empty enfant (ex. `Smoke` en haut de la cheminée, `DrillDust` au point de forage), Z local = haut, pour y accrocher un ParticleSystem dans Unity.
- **Écran holographique** (préféré à un écran `_Glow` quand l'utilisateur veut un rendu futuriste) : un projecteur métal et néon, un objet `Hologram` (cadre et faisceau en matériau `_Hologram`), et un quad enfant `Hologram_Screen` aux UV 0..1 pour une future interface. Recette : `build_hologram()` dans `dr_build.py` ou `build_holo()` dans `cs_v5.py`. Le script le masque quand le modèle n'est pas alimenté.
- **Vérins hydrauliques** : tube fixe **ouvert** (anneau de révolution, alésage 5 cm) et tige mobile de rayon inférieur, attachée à la pièce qui se déplace. La tige coulisse sans jamais toucher le tube, ce qui garde le test BVH propre. Les colliers du tube ont leur alésage dans l'épaisseur de la paroi (6,2 cm), jamais dans le plan de l'alésage du tube.
- **Le soleil** : `SolarPanelAnimator.sun` = la Directional Light. Le panneau suit `-sun.transform.forward`, **seulement une fois déployé**.
- Tu ne peux pas tester dans Unity : dis-le, et vérifie au moins la compilation dans `Editor.log`.

---

## 8. Erreurs déjà commises (à NE PAS refaire)

1. **Normales inversées** : la face devient invisible ou transparente, car Unity fait du backface culling (cause du « plan de travail transparent » de la station). → `fix_normals(bm)` par îlot fermé, selon le signe du volume, **toujours** dans `finish2`.
2. **Vitres en plans simples** : normales incohérentes et triangles sans vitre. → prismes fermés minces.
3. **Faces coplanaires qui se chevauchent** (z-fighting visible : bordure noire qui scintille sur le mur, plan de travail qui « glitch » avec la base, mât et collier). →
   - les pièces rapportées dépassent d'au moins 1 à 6 mm, ou s'arrêtent avant le cadre ;
   - un cadre ou une bande dépasse le mur de 1 cm et ne partage aucun plan avec lui ;
   - le corps d'un tunnel s'arrête **avant** les brides ;
   - lance `zfight.check2` avant chaque export.
4. **`mesh.materials.clear()` remet tous les indices de matériaux à 0.** → réappliquer les indices avec `foreach_set` (déjà fait dans `finish2`).
5. **Révolution fermée** avec une liste de matériaux trop courte. → une entrée par segment, y compris le segment de fermeture.
6. **Modèle basculé de 90° pendant les clips.** → `bakeAxisConversion = false` et `preserveHierarchy = true` pour TOUS les modèles. Le sas n'avait pas ce bug à l'origine, mais il suit la même règle.
7. **Erreur de compilation dans `ModelImportSetup.cs`** (variable locale `def` en conflit, CS0136) : la pipeline ne tournait plus, les clips de l'antenne restaient périmés (« toutes les animations font la même chose »). → vérifier `Editor.log` après chaque modification.
8. **Meta FBX périmé** (anciennes prises, `externalObjects` pollués) : réécrire le `.meta` en **gardant le GUID**, après en avoir fait une sauvegarde. Ne jamais corrompre un meta avec un remplacement PowerShell approximatif.
9. **Matériaux créés à la racine `Models/`** (`FindAssets` récursif, `SetupFolder` sur la racine). → filtres `IsModelFolder` et « même dossier ». Ne rien poser à la racine.
10. **Script qui fait tourner un transform animé par l'Animator** (le suivi du soleil ne marchait pas, le repliage de l'éolienne pouvait bloquer). → angle détenu par le script et appliqué dans `LateUpdate` (§7).
11. **Controller généré avant l'ajout des couches** : le panneau solaire n'avait qu'une couche. → supprimer le `.controller` pour qu'il soit recréé.
12. **Translation appliquée deux fois** (nœuds d'angle du T qui flottaient), **entretoises flottantes**, **prisme extrudé à l'envers** (nacelle), **capot coplanaire** : toujours faire un rendu de contrôle et vérifier les contacts.
13. **Collisions pendant un pliage** (pile repliée du panneau solaire contre les charnières). → test BVH sur toutes les frames, ajuster la hauteur et la position des charnières.
14. **Dôme géodésique** : des faces plates restaient couchées sur l'anneau, côté équateur. → supprimer les faces dont tous les sommets sont sur la limite.
15. **Empties renommés `Socket.001` dans le FBX** → renommage temporaire à l'export.
16. **Pose manuelle ignorée au rendu** → détacher les actions avant de rendre.
17. **T de tunnel raté** (simple tube avec une branche collée). → une vraie jonction, avec un moyeu et des branches qui partagent la même section et les mêmes brides.
18. Python n'est pas installé localement : exécuter le Python dans Blender (MCP), et éditer les fichiers avec les outils d'édition.
19. **Charnière qui traverse la pièce mobile** (couvercle du générateur posé sur les charnières fixes) : la plaque mobile doit commencer à distance de l'axe (rayon > rayon des charnières fixes), reliée à sa charnière par une patte plus étroite que l'écart entre les charnières fixes.
20. **Pièce optionnelle dans la zone de balayage d'une pièce mobile** (collier de cheminée contre la trappe ouverte) : inclure aussi les pièces optionnelles ou repositionnables dans le test BVH, dans leur position la plus défavorable.
21. **Vantail qui heurte son linteau** (porte de la capsule) : un panneau qui pivote autour d'un axe bas, vers l'extérieur, monte jusqu'à `axe + rayon` en passant par la verticale. Vérifier cette hauteur et laisser du jeu sous le linteau et sous le cadre extérieur.
22. **Bandeau décoratif sous une charnière** : un bandeau saillant en bas de coque était balayé par la charnière de la porte. Garder les bandeaux hors du cercle de rotation des pièces mobiles.

---

## 9. Scripts de référence (`ArtPipeline/Blender/`)

Ces scripts sont exécutés dans Blender via MCP. La variable `SP` en tête de chaque script pointe sur ce dossier.

| Fichier | Contenu |
|---|---|
| `cs_helpers.py` | Primitives de base : `add_box`, `add_revolve`, `add_cyl`, `add_screw`, `_copy_into`, `finish` ; constantes d'index matériaux `WHITE, DARK, METAL, NEON, ORANGE, INNER, PLATE, HAZARD` |
| `cs_v5.py` | `EB`, `cbox`, `pill`, `prism_x`, `oct_cyl`, `beam`, `tube`, plus le modèle de la station de craft (exemple de référence de la DA) |
| `al_build.py` | **Base commune** : `MAT_DEF` / `make_mats(prefix)`, bride standard (`prof6`, `PASS`, `SHELL`, `offset_poly`), `hollow_y`, `prism_y`, `fix_normals`, `finish2`, plus DomeConnector et Airlock |
| `cs_anim.py`, `al_anim.py` | Animations station / sas |
| `dome_build.py`, `geodesic.py`, `dome_sizes.py` | Dômes géodésiques paramétriques (8 / 12 / 16 m, même taille de triangles, mur 1.20 m) |
| `tn_build.py` | Kit tunnel : section vitrée, chemins droits ou en arc, jonctions T et + |
| `ra_build.py` | Antenne relais et ses animations |
| `sp_build.py` | Panneau solaire, avec pliage accordéon, inclinaison et rotation |
| `wt_build.py` | Éolienne, avec mât télescopique, pales repliables et rotor |
| `cg_build.py` | Générateur à charbon : trappe de trémie, levier, ventilateur, cheminée à hauteur variable (fût étirable, tête, collier disque plein orientable, empty `Smoke`), plus `collisions()` (test BVH réutilisable) et `tilt_collar(deg)` (aperçu de l'étirement elliptique fait par le script) |
| `lc_build.py` | Capsule d'atterrissage : coque conique à 8 facettes creuse et habitable, porte affleurante qui se rabat en rampe ; helpers `fpt` / `panel` / `hexa` pour poser des éléments sur des facettes inclinées, `open_angle()` (angle d'ouverture qui pose la rampe au sol) |
| `dr_build.py` | Foreuse : derrick à trépied, chariot poussé par deux vérins, tête à 3 molettes qui tourne, bloc latéral avec pupitre et écran holographique (`build_hologram`), empty `DrillDust`, plus `collisions()` (descente, puis rotation en position basse) |
| `st_build.py` | Stockage famille A : `build("Crate_Small" / "Crate_Large" / "Cabinet")`, un modèle et un `.blend` chacun ; helpers `hollow_z`, `solid` (polygones non convexes), charnières en équerre, `StackPoint` ; `collisions(model)` teste toutes les paires de pièces mobiles |
| `zfight.py` | Détecteur de z-fighting (`check2`) |

Pour un nouveau modèle :
1. Crée `xx_build.py` qui fait `exec(open(SP + r"\al_build.py", encoding="utf8").read())`.
2. Utilise `make_mats("<Model>")`, plus les matériaux spécifiques si besoin.
3. Écris `setup()` (hiérarchie), puis `animate()` (timeline et `CLIPS`).
4. Contrôle avec `fix_normals`, `zfight` et le test BVH, puis fais les rendus.
5. Exporte (§3), puis complète `ModelImportSetup.cs` (§4 et §6).
6. Écris `<Model>Animator.cs` (§7) si le modèle est animé.
7. Mets à jour ce document : tables de clips, couches et nouvelles erreurs rencontrées.
