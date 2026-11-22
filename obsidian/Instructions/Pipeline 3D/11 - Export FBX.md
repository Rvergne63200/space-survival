---
tags: [jeu-colonie, pipeline-3d, blender, unity]
---

# Export FBX

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

[[00 - Index Pipeline 3D|Index]] · ← [[10 - Modélisation Blender]] · [[12 - Import Unity]] →
