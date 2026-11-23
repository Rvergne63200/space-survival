---
tags: [jeu-colonie, a-faire, feature, modeles]
id: F65
categorie: Feature
feature: Modèles
statut: en cours
---

# F65 - Intégration des modèles 3D

**Intégration des modèles 3D** (`Resources/Models`, avec leurs scripts d'animation dans `Scripts/Behaviours/Animations`) :
- [x] Capsule de départ : modèle `LandingCapsule` dans le prefab `SpaceCapsule` (ancien modèle, coffres de test et station de craft retirés, intérieur vide), porte qui s'ouvre et se ferme en interagissant avec elle, état de la porte sauvegardé. Coffre de test à côté de la capsule avec tous les items (`TestChest`).
- [x] Dômes 8, 12 et 16 m (prefabs `Dome_<taille>`, ancien `BaseDom` supprimé), connecteurs (`DomeConnector`, masque qui cache le mur du dôme dans le connecteur et découpe ses colliders), sas posé librement et accroché au dôme le plus proche (`Airlock`), connecteur à chaque bout des couloirs. Placement des connecteurs partagé par le sas et le couloir (`DomeConnection`).
- [x] Sas : portes ouvertes et fermées par interaction (cycle du sas entre les deux).
- [x] Couloirs droits : modèles `Tunnel_Straight` répétés et centrés, `Tunnel_Extension` étiré à chaque bout jusqu'au connecteur (`CorridorPieces`). Extensions de 0,5 à 1,5 m (0,25 à 0,75 morceau droit), pour que le morceau incliné ne rentre pas dans le connecteur en bas d'une pente. Pose : le connecteur suit le point visé sur le premier dôme, le couloir part en rouge vers le point visé, puis se fixe sur le second dôme (recalé sur la ligne des centres). Les plans du sas et du couloir montrent leurs connecteurs. En pente, le couloir reste à plat et ses morceaux sont cisaillés verticalement : sections verticales sur les brides, lignes droites d'un dôme à l'autre.
- [x] Terrain sous les constructions : aplanissement refait (disque ou rectangle incliné, 18 cm sous le sol, creusé ou remblayé, pente douce autour, `TerrainFlattening`), y compris sous les couloirs, et masque du terrain dans le shader au-dessus des sols (`TerrainMask`). Ancien aplanissement retiré.
- [x] Station de craft : modèle `CraftStation` (animé par `CraftingStationAnimator`), 20 W (allumée et alimentée : écran et hologrammes, sinon inutilisable), 2 s par exemplaire, résultat récupéré automatiquement à proximité ou laissé sur la station (F), allumage / extinction et annulation dans son interface.
- [x] Rangements : grande caisse (`8_Crate`, modèle `Crate_Large`, 20 slots), petite caisse (`31_SmallCrate`, 10) et armoire (`32_Cabinet`, 30), ouverts et fermés avec leur interface (`StorageAnimator`), caisses empilables (`StorageContainer`).
- [ ] Panneau de contrôle : modèles livrés, à intégrer aux prefabs (collider, point `Connector` du câble, branchement de l'animateur sur l'alimentation de `ControlPanel`).
  - `ControlPanel` : pupitre holographique (0,84 × 0,54 m, hologramme jusqu'à 1,78 m), barre de projecteur qui sort à l'allumage puis écran holographique qui se déploie depuis la lentille, et l'inverse à l'extinction (clips `Power_On` / `Power_Off`, écran invisible éteint) ; écran holographique vertical `Hologram_Screen` (UV 0..1, pour la carte du réseau), voyant `_Status`. Script `ControlPanelAnimator` (`PowerOn`, `PowerOff`, `SetPowered`, `HologramScreen`).
  - `WallScreen` : télévision incurvée (1,10 × 0,62 m) à accrocher au mur plein des dômes, statique. Origine au contact du mur, au milieu de l'écran. Dalle `Screen` (UV 0..1) et voyant pilotés par `WallScreenAnimator` (`PowerOn`, `PowerOff`, `SetPowered`, `ScreenSurface`). Son rôle en jeu reste à définir (panneau de contrôle mural ?).
- [x] Éolienne : modèle `WindTurbine` intégré au prefab `WindTurbine` (voir [[Éolienne]]). Il manque une icône d'item, et une pose déployée sans animation dans `WindTurbineAnimator` (pour le chargement d'une sauvegarde, comme `SolarPanelAnimator.SetDeployedImmediate`).
- Reste : coudes, T et croix des tunnels (`Tunnel_Elbow45`, `Tunnel_Elbow90`, `Tunnel_T`, `Tunnel_Cross`), les autres modèles (générateurs, foreuse, antenne, coffres, station de craft…).

Voir aussi : [[Capsule de départ]] · [[Dômes]] · [[Couloir]] · [[Panneau de contrôle]]
