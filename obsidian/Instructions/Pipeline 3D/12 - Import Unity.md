---
tags: [jeu-colonie, pipeline-3d, unity]
---

# Import Unity

Script : `Assets/Scripts/Editor/ModelImportSetup.cs`.

Cet `AssetPostprocessor` s'applique automatiquement à tout ce qui est sous `Assets/Resources/Models/`.

- **TOUS les modèles** sont importés avec `bakeAxisConversion = false` et `preserveHierarchy = true`.
  - Sinon, le modèle **bascule de 90°** dès qu'un clip joue : la conversion d'axes n'est pas appliquée aux courbes d'animation.
  - Conséquence : sous la racine du prefab, **les enfants gardent leurs axes locaux Blender** (vertical = Z local, avant = -Y local). Les scripts C# doivent en tenir compte (voir [[15 - Scripts d'animation Unity]]).
- Les modèles **sans table de clips** sont importés avec `animationType = None` et `importAnimation = false`.
- **Matériaux :**
  - pour chaque matériau FBX nommé `M_*_<Suffixe>` connu, un `.mat` Standard est créé dans **le même dossier** ;
  - pour chaque jeu de textures `<Prefix>_BaseColor.png` du dossier, un `M_<Prefix>.mat` texturé est créé (voir [[09 - Textures]]) ; si le jeu de textures arrive avant une modification du script, réimporte le dossier ;
  - le remap se fait uniquement sur ce dossier, car `FindAssets` est récursif et doit être filtré ;
  - `_Glass` passe en Standard Transparent, `_Hologram` utilise `Custom/Hologram` ou un fallback en mode Fade.
- **Clips :**
  - le FBX contient **une seule timeline** ;
  - la table `ModelClips[model]` donne, pour chaque clip, `(nom, première frame, dernière frame, loop)` ;
  - pour les modèles qui ont une table, celle-ci écrase toujours les réglages manuels.
- **Animator Controller** : créé automatiquement **seulement s'il n'existe pas** (`<Model>.controller`). Pour le régénérer après un changement de couches, **supprime-le** : il est recréé.
- Les modèles `Dome_*` sont importés avec `isReadable = true`, pour la découpe du mur au runtime.
- Après toute modification du script, **vérifie qu'il compile** : cherche `error CS` dans `%LOCALAPPDATA%\Unity\Editor\Editor.log`. Une erreur de compilation empêche toute la pipeline de tourner, avec des clips périmés et sans controller.

[[00 - Index Pipeline 3D|Index]] · ← [[11 - Export FBX]] · [[13 - Animations Blender]] →
