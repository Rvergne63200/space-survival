---
tags: [jeu-colonie, pipeline-3d, direction-artistique]
---

# Stockage et empilage

- **Couvercle couvrant tout le dessus.** Les poteaux d'angle s'arrêtent sous le couvercle (8 mm sous le rebord, sans plan partagé). Un couvercle découpé entre les poteaux laissait voir l'intérieur par des fentes.
- **Empilage** : plots de centrage graphite sur le couvercle, au-dessus des poteaux ; empty **`StackPoint`** à `hauteur du couvercle - bas du socle` (le socle descend à -0,005). La racine du caisson du dessus se pose sur ce point. Un caisson chargé ne s'ouvre pas : `StorageAnimator.CanOpen`.
- **Charnières** : l'axe passe **2 cm à l'extérieur du bord du couvercle** (`HINGE_OFF`). Les charnières fixes sont posées sur une équerre en L (un pied sous le rebord, un montant derrière le bord du couvercle), alternées avec celles du couvercle, qui s'y rattachent par une patte. Sinon, le couvercle « flotte » à l'ouverture, ou recoupe les charnières fixes.
- **Portes battantes** : décoller la porte de 6 mm de la façade, et vérifier que la charnière (rayon ≤ 1,5 cm) ne recoupe pas le chanfrein d'angle du corps en tournant.
- **Corps creux** : `hollow_z()` (bac ouvert en haut) et `solid()` (polygone en U extrudé pour une armoire ouverte en façade) dans `st_build.py`. L'intérieur est graphite.

[[00 - Index Pipeline 3D|Index]] · ← [[05 - Modèles habitables]] · [[07 - Objets muraux]] →
