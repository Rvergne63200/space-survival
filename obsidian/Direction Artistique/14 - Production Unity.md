---
tags: [jeu-colonie, direction-artistique, production, unity]
---

# Production (Unity, développeur solo)

- **Privilégier les kits modulaires :** dômes, tunnels, sas et machines conçus comme des pièces qui s'assemblent. C'est cohérent avec le gameplay de construction et beaucoup plus rapide à produire.
- **Le style stylisé est un allié :** des formes simples demandent moins de détail que le photoréalisme, et vieillissent mieux. Pour l'instant, les modèles sont en **matériaux unis, sans textures** (voir [[09 - Textures]]).
- **L'atmosphère fait la moitié du travail :** brume, lumière du jour et de la nuit, ciel et post-traitement donneront au jeu la majeure partie de son identité. Il faut y investir tôt.
- **Pipeline de rendu :** le projet utilise le **Built-in Render Pipeline** avec le shader **Standard** (Unity 6000.4). URP ou HDRP (brume volumétrique, éclairage plus riche) restent une option pour plus tard, selon la machine cible et les performances.
- **Méthode de production des modèles** (Blender → Unity) : voir [[00 - Index Pipeline 3D]].

## Livrables par asset
- Feuille de concept : silhouette, vues principales, échelle par rapport à un humain de 1,80 m en combinaison.
- Planche couleur de jour et de nuit.
- Fiche matériaux.
- Modèle avec une carte d'émission séparée pour les lumières.
