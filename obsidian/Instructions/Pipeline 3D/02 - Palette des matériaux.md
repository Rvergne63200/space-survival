---
tags: [jeu-colonie, pipeline-3d, materiaux]
---

# Palette des matériaux

> Le choix des couleurs, leurs proportions et leurs usages relèvent de la DA, qui fait foi : voir [[08 - Technologie humaine]] (couleurs et voyants). Cette note donne seulement leur mise en œuvre dans Blender et Unity.

Chaque modèle a ses propres matériaux nommés **`M_<Model>_<Suffixe>`**. Le suffixe pilote la création automatique du `.mat` Unity.

| Suffixe | Rôle | Blender Base Color (linéaire) | Émission Blender |
|---|---|---|---|
| `_White` | Coque principale, carénages | (0.82, 0.85, 0.88) | — |
| `_Dark` | Graphite : cadres, brides, bandes, socles, joints, intérieurs de passage | (0.035, 0.045, 0.06) | — |
| `_Metal` | Métal gris foncé : sols, vis, axes, grilles, charnières | (0.16, 0.18, 0.22) | — |
| `_Neon` | Lignes lumineuses bleues | (0.55, 0.92, 1.0) | (0.05, 0.65, 1.0) × 3 |
| `_Orange` | Petites lumières orange : balises, voyants | (1.0, 0.38, 0.03) | (1.0, 0.25, 0.0) × 3 |
| `_Glow` | Écrans / lueur intérieure bleu foncé | (0.1, 0.4, 0.85) | (0.01, 0.2, 0.55) × 3 |
| `_Plate` | Plaques métal clair | (0.55, 0.65, 0.75) | — |
| `_Hazard` | Bandes de danger jaune-orange | (0.95, 0.6, 0.04) | — |
| `_Status` | Voyant dont la couleur est pilotée par script | (1.0, 0.45, 0.05) | (1.0, 0.3, 0.0) × 3 |
| `_Glass` | Vitres (transparent) | (0.25, 0.6, 0.8), Alpha 0.35, Roughness 0.08 | — |
| `_Cell` | Cellules solaires bleu nuit | (0.03, 0.08, 0.22), Roughness 0.15, Metallic 0.3 | — |
| `_Fire` | Foyer visible derrière un hublot (générateur à charbon) ; intensité pilotée par script | (1.0, 0.30, 0.04) | (1.0, 0.22, 0.02) × 4 |
| `_Hologram`, `_Laser` | Effets spécifiques (station de craft) | voir `cs_v5.py` | — |

**Mise en œuvre :**
- Le `_Fire` est un disque de foyer derrière une vitre (prisme `_Glass` de 5 mm devant), pas une couleur de surface.
- Roughness 0.5 partout dans Blender, sauf pour les vitres et les cellules.
- Mets toujours `m.diffuse_color` : c'est la couleur de prévisualisation dans le viewport.
- Dans Blender, cherche le nœud Principled **par type** (`n.type == 'BSDF_PRINCIPLED'`), jamais par nom : l'interface est en français.

La palette Unity équivalente est dans `ModelImportSetup.cs` → `FlatPalette`. Si tu inventes un nouveau suffixe, **ajoute-le à `FlatPalette`**, sinon Unity ne crée pas le matériau.

[[00 - Index Pipeline 3D|Index]] · ← [[01 - Façon de travailler]] · [[03 - Géométrie des modèles]] →
