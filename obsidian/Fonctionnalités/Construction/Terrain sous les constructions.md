---
tags: [jeu-colonie, fonctionnalite, construction, terrain]
---

# Terrain sous les constructions

- **Aplanissement** (`TerrainFlattening`, à la pose du plan : dôme, sas, couloir) : l'emprise est un disque (dôme, rayon du mur) ou un rectangle tourné et incliné le long de sa longueur (sas : sa boîte ; couloir : d'un dôme à l'autre, avec la pente entre les deux), agrandie de 0,75 m (le terrain a un point par mètre). Dedans, le terrain est mis 18 cm sous le sol (sol à 0,2 m au-dessus de l'origine), creusé ou remblayé : il ne traverse plus le sol et la construction ne flotte pas. Autour, il rejoint le terrain naturel sur 2,5 m (pente douce). Seuls les points à portée sont lus et écrits (`ChunkManager.AddTerrainEdit`, `ApplyTerrainEdit`).
- **Masque** (`TerrainMask`, visuel seulement) : des volumes au-dessus du sol et dans les murs où le shader du terrain (`Terrain/World UV Standard`, `ClipTerrainMasks`) ne le dessine pas : cylindre du dôme, boîtes du connecteur, du sas et du couloir (incliné avec lui), à partir de 10 cm au-dessus de l'origine. Ils cachent les quelques centimètres qui dépasseraient encore entre deux points du terrain, le long des murs. 64 au plus.
- Les aplanissements sont sauvegardés et rejoués à la génération des chunks (voir [[Sauvegarde]] et [[Terrain procédural]]).

Voir aussi : [[Pose et destruction]] · [[Dômes]] · [[Sas]] · [[Couloir]]
