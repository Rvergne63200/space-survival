---
tags: [jeu-colonie, pipeline-3d, blender, code]
---

# Scripts de référence

Dossier : `ArtPipeline/Blender/`.

Ces scripts sont exécutés dans Blender via MCP. La variable `SP` en tête de chaque script pointe sur ce dossier.

| Fichier | Contenu |
|---|---|
| `cs_helpers.py` | Primitives de base : `add_box`, `add_revolve`, `add_cyl`, `add_screw`, `_copy_into`, `finish` ; constantes d'index matériaux `WHITE, DARK, METAL, NEON, ORANGE, INNER, PLATE, HAZARD` |
| `cs_v5.py` | `EB`, `cbox`, `pill`, `prism_x`, `oct_cyl`, `beam`, `tube`, plus le modèle de la station de craft (exemple de référence pour la méthode ; ses cylindres à 8 pans sont plus facettés que la règle actuelle, 12 à 16 côtés) |
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
| `cp_build.py` | Panneau de contrôle (pupitre holographique) : bureau en `prism_x` à profil concave, logement ouvert (`hollow_z`) d'où sort la barre `Projector` (clips Power_On / Power_Off), hologramme vertical (`build_hologram`), `collisions()`, `tri_ngons()` |
| `ws_build.py` | Écran mural (TV incurvée lisse) : `sweep()` (profil balayé sur un arc), `screen_uv()` (UV 0..1 sur la face avant courbe), `wall_clearance()` (marge contre les murs des dômes 8 / 12 / 16 m), `tri_ngons()` |
| `bt_build.py` | Batteries statiques : `build("Battery_Wall" / "Battery_Cabinet" / "Battery_Bank")`, un `.blend` chacun ; jauge en objets séparés `Gauge_1..5` (néon), `belt()` (bande décalée autour d'une coque chanfreinée), `louvers()`, `boss()` (prise de câble), `wall_clearance()`, `preview_rig()` / `aim()` / `set_gauge()` pour les rendus, `export()` |
| `dp_build.py` | Gisements, base : `ORES` (couleurs par minerai), fissures (`make_cracks`, progression `T` du centre vers les bouts), champs du relief (`fields`), modèle du jeu (`build_mesh` : décimation pondérée, Delaunay 2D, UV dépliées, normales du relief lissé), `bilinear`, `blur`, `save_png`, `make_material` |
| `dp_hd.py` | Gisements, cuisson : `grid_mesh` (modèle détaillé avec attributs), `hp_material` (nœuds Cycles : rampes, AO, Bevel, profondeur au pixel), `bake`, `new_img`, `pixels` |
| `dp_v3.py` | Gisements, version retenue : `build_v3("Coal" / "Iron" / "Copper")` ; dalle en roche cuite, minerai en géométrie (`build_ore_geo` : couche coupée par les fissures, masses incrustées, éclats ; `chunk`, `box_uv`), texture raccordable du minerai (`ore_textures`) |
| `ground_rust.py` | Texture raccordable du sol orange pour le terrain (`TileNoise`, tuile de 8 m) |
| `zfight.py` | Détecteur de z-fighting (`check2`) |

Pour un nouveau modèle :
1. Crée `xx_build.py` qui fait `exec(open(SP + r"\al_build.py", encoding="utf8").read())`.
2. Utilise `make_mats("<Model>")`, plus les matériaux spécifiques si besoin.
3. Écris `setup()` (hiérarchie), puis `animate()` (timeline et `CLIPS`).
4. Contrôle avec `fix_normals`, `zfight` et le test BVH, puis fais les rendus.
5. Exporte ([[11 - Export FBX]]), puis complète `ModelImportSetup.cs` ([[12 - Import Unity]] et [[14 - Animator Unity]]).
6. Écris `<Model>Animator.cs` ([[15 - Scripts d'animation Unity]]) si le modèle est animé.
7. Mets à jour ces notes : tables de clips, couches et nouvelles erreurs rencontrées.

[[00 - Index Pipeline 3D|Index]] · ← [[16 - Erreurs à ne pas refaire]]
