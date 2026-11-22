---
tags: [jeu-colonie, fonctionnalite, energie, batiments]
---

# Électricité des bâtiments

`BuildingPowerPanel`, hérite de `DistributionPanel`, sur l'enfant `PowerSocket` des prefabs de dôme : le tableau intégré au bâtiment, dont le circuit est l'intérieur (règle 3 du [[Réseau électrique]]). Mêmes réglages et même interface qu'un [[Tableau de distribution]] (`DistributionPanelUI`, touche d'interaction sur la prise).

- **Borne intérieure** (`BuildingPanelTerminal`, enfant `PanelTerminal` des prefabs de dôme, contre le mur côté prise) : ouvre l'interface du tableau du bâtiment depuis l'intérieur ; ce n'est pas un appareil électrique, on n'y câble rien.
- **Affichage** du tableau intégré : la consommation du bâtiment seul (ce qui est à l'intérieur, pas les bâtiments reliés), puis ce qui est transmis depuis l'entrée, la recharge et la sortie.
- **Prise** : un poteau gris de 1,5 m au bord de la base (côté +Z), avec deux ports câblables : l'entrée (cube vert, en haut) et la prise de sortie (cube orange, en dessous). Le câble se branche sur le port le plus proche de la visée.
- **Intérieur** : la sortie du tableau (port caché `interior`) est reliée sans câble à tout ce qui est posé dans le bâtiment (`BuildingInterior.Contains` sur la position de l'appareil, voir [[Intérieur des bâtiments]]), sauf les sorties d'autres tableaux et les ports du bâtiment (`EnergyNetworkManager.BuildBuildingLinks`). `EnergyNetworkManager.Links` donne les voisins d'un appareil : câbles, intérieur des bâtiments et couloirs.
- **Prise de sortie** (`BuildingOutlet`, port orange `output`) : un tableau caché dans le bâtiment, entrée (`outletInput`) reliée à l'intérieur, sortie câblée dehors. Toute sa demande est facultative (`OptionalDemand`, règle 2) : elle ne renvoie que le surplus, avant les batteries du bâtiment, et ce qui est derrière manque de courant avant le bâtiment.
- **Tableau intégré** (`DistributionPanel.Building` : le bâtiment et sa prise de sortie) : jamais éteint par un tableau au-dessus (règle 6), pas d'icône sur la carte, et pour repartir il compte aussi sur l'amont plus lointain (règle 7, `EnergyNetworkManager.Potential`).
- **Câbles** (`CablePlacer`) : deux ports d'un même bâtiment ne se relient pas, ni les entrées de deux bâtiments (on passe par la prise de sortie ou un couloir).
- **Couloir** (`Corridor`, voir [[Couloir]]) : il relie sans câble les entrées de ses deux bâtiments, et leurs intérieurs (sorties de leurs tableaux intégrés, règle 1) : un générateur ou une batterie posé dans B1 alimente aussi les machines de B2, et réciproquement. Chaque bâtiment garde son tableau : couper B1 n'éteint que ce qui est dans B1 (ses générateurs compris), B2 continue avec les siens ; la limite de B1 ne concerne que ce que B1 tire de son entrée. Une machine posée dans le couloir n'est reliée à rien (il faut la câbler).
- **Exemples** : `B1 ---> B2` (prise de sortie de B1) : B2 reçoit le surplus de B1, un générateur de B2 n'alimente pas B1 ; couper B1 prive B2 d'alimentation sans l'éteindre. `B1 ---> B2 ---- G1` : couper B1 n'éteint pas G1, qui alimente B2. Un tableau de distribution posé dans un bâtiment est alimenté par le bâtiment ; une machine posée dedans est toujours dans le circuit du bâtiment.

Voir aussi : [[Dômes]] · [[Panneau de contrôle]]
