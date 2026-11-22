---
tags: [jeu-colonie, fonctionnalite, energie]
---

# Réseau électrique

Code dans `Assets/Scripts/Behaviours/Energy`.

- **EnergyValue** : une puissance (W) ou une énergie (Wh) qui peut être infinie (`float.PositiveInfinity`). Dans l'Inspector, c'est une valeur suivie d'une case « ∞ ».
- Les appareils sont décrits dans [[Appareils électriques]], les câbles dans [[Câble électrique]].

## EnergyNetworkManager
Créé au premier appareil, persistant. Il reconstruit les réseaux quand un appareil ou un câble change, et les calcule toutes les 0,2 s.

### Règles
Les mêmes que dans l'en-tête du script :

1. **Réseau** : tout ce qui est relié (câbles, intérieur d'un bâtiment, couloirs) forme un réseau. Seuls les tableaux séparent les réseaux : leur entrée est un consommateur du réseau du dessus, leur sortie un générateur de leur circuit, servie après les générateurs du circuit. Sens unique.
2. **Demande d'un tableau** : obligatoire (les machines de son circuit) et facultative (la recharge de ses batteries, les prises de sortie de bâtiments derrière lui), dans sa limite de puissance. Le facultatif n'est servi que par le surplus, avant les batteries du dessus, et ne fait jamais disjoncter.
3. **Bâtiment** : un tableau dont le circuit est son intérieur (`BuildingPowerPanel`). Sa prise de sortie est un tableau intérieur dont toute la demande est facultative (`BuildingOutlet`). Un couloir relie les entrées de deux bâtiments, et leurs intérieurs : les bâtiments reliés par couloirs partagent leurs générateurs et leurs batteries.
4. **Chaque réseau** : ses générateurs donnent ce qu'il demande, leur surplus charge ses batteries (parts égales, dans la limite du débit entrant et de la place, réseau disjoncté compris), les batteries donnent ce qui manque encore (dans la limite du débit sortant). Le reste est perdu.
5. **Manque** : une fois les flux stabilisés, un réseau qui manque encore d'énergie déleste ses tableaux réglés sur « manque d'énergie » (priorité la plus faible en premier), puis disjoncte : ses consommateurs s'arrêtent. Il se réarme après `rearmDelay` (3 s) dès qu'il pourrait être alimenté.
6. **Coupure** : un tableau éteint éteint son circuit (pour un bâtiment : seulement ce qui est à l'intérieur, son intérieur étant partagé avec les bâtiments reliés par couloirs) et les circuits des tableaux qui y sont, sauf les tableaux intégrés à un bâtiment, qui ne sont plus qu'alimentés.
7. **Redémarrage** : un tableau sans entrée ou délesté repart quand l'amont pourrait de nouveau le servir. Un tableau intégré à un bâtiment compte aussi sur ce que les tableaux du dessus pourraient obtenir plus haut.

Voir aussi : [[Tableau de distribution]] · [[Électricité des bâtiments]] · [[Panneau de contrôle]]
