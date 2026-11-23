---
tags: [jeu-colonie, fonctionnalite, energie, interfaces]
---

# Panneau de contrôle

`ControlPanel` et un `EnergyConsumer` de 20 W, sur le prefab `Prefabs/Environment/Energy/ControlPanel`.

- Il se branche par câble sur un réseau. Il ne s'ouvre (touche d'interaction) que s'il est alimenté, et se ferme s'il ne l'est plus. Éteint depuis sa propre carte, il se rallume en interagissant avec lui.

## Portée
`CollectNetwork`, quatre règles :
1. Il voit son réseau, et tout ce qui est en aval : derrière l'entrée de chaque tableau rencontré, bâtiments compris.
2. Il ne voit jamais ce qui est derrière l'entrée d'un tableau qui alimente ce qu'il voit (l'amont).
3. Dans un bâtiment, son réseau est celui de son groupe de couloirs (`Corridor.Group`), avec le réseau qui alimente ce groupe.
4. Sur ce réseau d'alimentation, les autres bâtiments sont vus de l'extérieur (contour, câble, réglages de leur tableau), pas leur intérieur.

Exemples : `PD1 ---> G1 ---> B1`, panneau dans B1 : G1, PD1 et ce que PD1 alimente d'autre, pas ce qui alimente PD1 (3, 2). `G1 ---> B1 ---> B2`, panneau dans B2 : B1 de l'extérieur, pas G1 (4, 2). `G1 ---- B1 ==== B2 ---> B3`, panneau dans B1 : G1, B1, B2 et B3 avec leur contenu (3, 1). `PD1 ---> B1` et `PD1 ---> B5`, panneau dans B1 : B5 de l'extérieur (4).

## ControlPanelUI
`UI.prefab`, `ControlPanel.uxml` : une interface, fermée par Échap. À gauche la carte, à droite l'appareil sélectionné. Bouton « Recentrer ».

## Carte
`NetworkMapElement`, nord en haut :
- Relief stylisé : teinte selon la hauteur, ombrage des pentes et courbes de niveau tous les 4 m (calculées en marching squares sur la grille des hauteurs et tracées en vectoriel, non pixélisées) (une sur 5 plus marquée), calculés depuis la seed (`ChunkManager.GeneratedHeightAt`) pour la zone affichée. Une grille par-dessus. Rendu partagé avec la [[Minimap]] (`MapUtility`).
- Bâtiments (`BuiltStructure`) : leur forme vue de dessus (enveloppe convexe des sommets de leurs meshes : dôme rond, couloir orienté).
- Câbles : lignes lumineuses, cyan si leur réseau est alimenté, cyan pâle si leur réseau n'a aucun consommateur, rouges s'il est disjoncté, grises sinon (`EnergyNetworkManager.CableState`).
- Icônes : carré pour une machine, rond pour un générateur, rectangle plat pour une batterie (la partie sombre, à droite, est la place libre), triangle pour un panneau de contrôle, losange pour un tableau de distribution. Pas d'icône pour le tableau intégré à un bâtiment : c'est le bâtiment qui se sélectionne. Le panneau utilisé est entouré d'un anneau qui pulse.
- Trois couleurs seulement : bleu en marche, rouge disjoncté (tableau sauté, délesté, sans alimentation en entrée ou en boucle), gris éteint ou non raccordé (tableau en démarrage). Générateurs et batteries : selon leur réseau.
- Bâtiments : surlignés au survol (et quand leur tableau est sélectionné), leur nom en infobulle. Un clic sélectionne leur tableau intégré, s'il est dans le réseau affiché.
- Molette : zoom autour du curseur. Glisser : déplacer. Survol : nom de l'appareil ou du bâtiment. Clic : sélection.

## Sélection
Pour une machine, un générateur ou une batterie, ses informations et un bouton Allumer / Éteindre. Pour un tableau, tous ses réglages, comme dans son interface ([[Tableau de distribution]]).

## Modèle
- Prefab : modèle `ControlPanel` (pupitre holographique, 0,84 × 0,63 × 1,13 m) en enfant `Model`, avec son `Animator` (`ControlPanel.controller`) et `ControlPanelAnimator`.
- Alimenté (allumé et réseau qui le fournit), la barre du projecteur sort puis l'écran holographique se déploie ; sinon il se replie (`ControlPanel.Update` → `ControlPanelAnimator.SetPowered`). Voyant bleu alimenté, orange sinon, clignotant pendant le mouvement.
- L'écran holographique (`Hologram_Screen`) reste vierge : la carte ne s'y affiche pas.
- Collider : une boîte autour du pupitre (l'hologramme n'en a pas). Point d'accroche du câble : `Connector`, en bas de la face arrière.
- Chargé avec une sauvegarde alors qu'il est alimenté, il rejoue le déploiement (l'animateur n'a pas de pose déployée sans animation).
- Le modèle `WallScreen` est livré mais n'a pas encore de rôle : voir [[F65 - Intégration des modèles 3D|F65]].

Voir aussi : [[Réseau électrique]] · [[Électricité des bâtiments]]
