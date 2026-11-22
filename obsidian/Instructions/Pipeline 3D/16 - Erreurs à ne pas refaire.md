---
tags: [jeu-colonie, pipeline-3d, methode]
---

# Erreurs à ne pas refaire

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
10. **Script qui fait tourner un transform animé par l'Animator** (le suivi du soleil ne marchait pas, le repliage de l'éolienne pouvait bloquer). → angle détenu par le script et appliqué dans `LateUpdate` ([[15 - Scripts d'animation Unity]]).
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
23. **N-gones concaves écartés par Unity** (« A polygon of Mesh 'Body' ... is self-intersecting and has been discarded », donc un trou dans le mesh) : les bouts d'un `prism_x` à profil concave, les flancs décalés, les bouts d'un `sweep`. → `tri_ngons(ob)` (triangulation BEAUTY des faces de plus de 4 côtés) après `finish2`, comme dans `cp_build.py` et `ws_build.py`. Après un import, chercher « self-intersecting » dans `Editor.log`.
24. **Caméra de rendu qui ne bouge pas** : `_Cam` est en `rotation_mode = 'QUATERNION'` dans certains `.blend`, et `rotation_euler` est alors ignoré. → passer en `'XYZ'` avant de la viser.

[[00 - Index Pipeline 3D|Index]] · ← [[15 - Scripts d'animation Unity]] · [[17 - Scripts de référence]] →
