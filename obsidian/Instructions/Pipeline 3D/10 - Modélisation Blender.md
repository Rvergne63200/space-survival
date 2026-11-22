---
tags: [jeu-colonie, pipeline-3d, blender]
---

# Modélisation Blender

- Tout est **généré par script Python (bmesh)**, jamais modélisé à la main.
  - Un script `xx_build.py` par modèle contient la géométrie, la hiérarchie et l'animation.
  - Il s'appuie sur la chaîne de helpers `cs_helpers.py` → `cs_v5.py` → `al_build.py`, chargée par `exec(open(SP + r"\al_build.py").read())`.
- **Helpers disponibles** (voir les fichiers) :
  - **Volumes simples :**
    - `cbox(bm, size, center, mat, ch, eb, rot)` : boîte chanfreinée ;
    - `pill` : bouton ou filet arrondi ;
    - `prism_x` / `prism_y` : extrusion d'un profil 2D, avec chanfrein des bouts et edge break ;
    - `oct_cyl` : cylindre à pans chanfreinés, nombre de côtés réglable (`sides`, 8 par défaut : 12 à 16 pour une machine ou un bâtiment, voir [[03 - Géométrie des modèles]]) ;
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
- **Un `.blend` par modèle ou par kit**, sauvegardé dans **`ArtPipeline/Blends/<Model>.blend`** (les blends existants y sont). Ne jamais le mettre sous `Assets/`, ni à la racine du projet ou dans un dossier par modèle à la racine.
- **Rendus de preview** dans **`ArtPipeline/Renders/<Model>_<vue>.png`** (ex. `ControlPanel_on.png`). Rien à la racine du projet.
- **Budget indicatif :** de quelques milliers à environ 20 000 triangles par module.

[[00 - Index Pipeline 3D|Index]] · ← [[09 - Textures]] · [[11 - Export FBX]] →
