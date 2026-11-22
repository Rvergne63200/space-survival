---
tags: [jeu-colonie, pipeline-3d, index]
---

# Pipeline 3D (Blender → Unity)

Ces notes sont les règles que l'IA doit respecter pour toute session qui modélise, anime ou intègre un modèle dans le projet **Space Survival**. Elles ne valent **que pour ce projet**. Retour à [[00 - Index Instructions]].

> **La Direction Artistique fait foi.** Tout ce qui relève du style (couleurs et leurs proportions, formes, matériaux, ambiance, ce qu'il faut éviter) est dans [[00 - Index DA]], en particulier [[03 - Formes et silhouettes]], [[05 - Matériaux et textures]] et [[08 - Technologie humaine]]. Ces notes-ci ne décrivent que la mise en œuvre technique. En cas de contradiction, c'est la DA qui l'emporte : signale-la à l'utilisateur.

- Projet Unity : `F:\Users\Richard\Documents\Richard\Dev\Jeux\Projets Unity\Space Survival`.
  - Unity 6000.4, **Built-in Render Pipeline**, shader **Standard** (pas URP ni HDRP).
- Blender 5.3 en **interface française**, piloté par le serveur MCP Blender.
  - Pour le lancer : Édition > Préférences > Add-ons > MCP > « Start MCP Bridge Server ».
- Scripts Blender de référence : `ArtPipeline/Blender/` (voir [[17 - Scripts de référence]]). **Toujours repartir de ces helpers** au lieu de réinventer la modélisation.
- Script d'import Unity : `Assets/Scripts/Editor/ModelImportSetup.cs`.
- Scripts d'animation runtime : `Assets/Scripts/Behaviours/Animations/<Model>Animator.cs`.
- Fichiers de travail : `ArtPipeline/Blends/` (un `.blend` par modèle), `ArtPipeline/Renders/` (rendus de preview), `ArtPipeline/Blender/` (scripts).

## Sommaire

1. [[01 - Façon de travailler]]

### Construction des modèles

2. [[02 - Palette des matériaux]]
3. [[03 - Géométrie des modèles]]
4. [[04 - Dimensions de référence]]
5. [[05 - Modèles habitables]]
6. [[06 - Stockage et empilage]]
7. [[07 - Objets muraux]]
8. [[08 - Vitres]]
9. [[09 - Textures]]

### Méthode technique

10. [[10 - Modélisation Blender]]
11. [[11 - Export FBX]]
12. [[12 - Import Unity]]
13. [[13 - Animations Blender]]
14. [[14 - Animator Unity]]
15. [[15 - Scripts d'animation Unity]]
16. [[16 - Erreurs à ne pas refaire]]
17. [[17 - Scripts de référence]]
