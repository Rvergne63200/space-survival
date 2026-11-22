---
tags: [jeu-colonie, pipeline-3d, direction-artistique]
---

# Modèles habitables

- **Coque creuse à facettes** : une bande de facettes (anneau ouvert, extérieur et intérieur `_White`, tranches `_Dark`), plus la facette de la porte construite en 4 pièces (allège, linteau, deux jambages) avec `panel()` / `hexa()` de `lc_build.py`. Ces helpers placent n'importe quel élément dans le plan d'une facette inclinée (`fpt(angle, u, z, profondeur)`).
- **Porte affleurante qui se rabat en rampe** :
  - charnière sur l'axe X, **4,5 cm à l'extérieur de la face de l'allège** (le bas du vantail ne frotte jamais) ;
  - angle d'ouverture **calculé** (`open_angle()`) pour que le point le plus bas du vantail touche le sol ;
  - le haut d'un vantail qui pivote vers l'extérieur **se soulève** de quelques centimètres en passant par la verticale. Il faut donc arrêter le vantail sous le linteau (7,5 cm ici) et masquer la fente par un rebord intérieur, côté cabine, que le vantail ne balaie pas ;
  - la face intérieure du vantail devient la surface de marche : y mettre les marches anti-dérapantes, les rails et la bande hazard au bout.
- **Intérieur vide** quand l'utilisateur l'aménage lui-même dans Unity : sol `_Metal` avec bordure graphite, nervures graphite aux angles, plafond blanc, un anneau néon. Pour les rendus, une lumière temporaire `_InteriorLight` (préfixe `_`, jamais exportée).

[[00 - Index Pipeline 3D|Index]] · ← [[04 - Dimensions de référence]] · [[06 - Stockage et empilage]] →
