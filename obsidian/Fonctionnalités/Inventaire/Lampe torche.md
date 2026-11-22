---
tags: [jeu-colonie, fonctionnalite, inventaire, outils]
---

# Lampe torche

`22_Flashlight`, `FlashlightAction`, `FlashlightBeam`. Créée comme enfant de la caméra au premier allumage. Réglages sur l'asset.

- **Utilisation** : clic gauche en main pour allumer ou éteindre (action `Toggle`). Elle s'éteint quand elle quitte la main. 10 Wh, 5 W en continu quand elle est allumée (voir [[Barre des objets]]).
- **Faisceau** : un spot (intensité 3, portée 45 m, angle 75°, ombres douces) tenu en main, un peu à droite et sous les yeux (`handOffset`) : les ombres des objets se voient. Il suit la vue sans retard.
- **Forme** : un cookie généré donne un centre lumineux (`hotspot`, 30 % du cône), un léger anneau du réflecteur et un halo large et faible autour (`spill`, 20 %), au lieu d'un disque uniforme.

Voir aussi : [[Utilisation des objets en main]] · [[Cycle jour et nuit]] (nuits sombres)
