---
tags: [jeu-colonie, a-faire, feature, energie]
id: F50
categorie: Feature
feature: Énergie
statut: en cours
---

# F50 - Réseau électrique complet

**Réseau électrique** : batteries, et câbles posés à la main. Unité : le **watt** (production et consommation en W).
- **Pose des câbles et des tuyaux** ([[F51 - Système d'oxygène|F51]]) : un lien automatique entre deux objets, qui peut passer par plusieurs points intermédiaires. Le tracé est arrondi entre deux points, pour l'esthétique.
- **Réseaux** :
  - Tout ce qui est relié forme un réseau : par câble, dans un même bâtiment, ou dans des bâtiments reliés ([[Historique - Features#^F26|F26]]). Un bâtiment alimenté alimente tout ce qu'il contient, sans câble.
  - Calcul par paliers dans un gestionnaire persistant, et sauvegardé.
- **Les générateurs et les batteries ne relient pas les réseaux** : un générateur ou une batterie relié de deux côtés appartient à deux réseaux, sans les fusionner.
  - Exemple : `G1 - A1 - G2 - A2` donne deux réseaux, {G1, A1, G2} et {G2, A2}. A1 profite de G1 et de G2, A2 ne profite que de G2.
  - Un générateur ou une batterie posé dans un bâtiment fait partie du réseau du bâtiment. S'il est aussi relié par câble à une machine dehors, il est dans deux réseaux. Le joueur pourra peut-être l'exclure du réseau du bâtiment (à voir).
  - Revu par [[Historique - Refontes#^R19|R19]] et [[Historique - Features#^F64|F64]] : générateurs et batteries relient maintenant les réseaux (plus de réseau partagé). Posé dans un bâtiment, il est dans le circuit du bâtiment ; câblé aussi à une machine dehors, cette machine rejoint ce circuit.
- **Générateurs** :
  - Ils tournent toujours à fond, dans la limite de l'oxygène reçu pour ceux qui en ont besoin ([[F52 - Machines consommatrices d'oxygène|F52]]).
  - Un générateur ne donne à chaque réseau que ce dont il a besoin, batteries comprises. Le reste est perdu, rejeté sous forme de chaleur (surchauffe de certains générateurs peut-être plus tard).
- **Répartition d'un générateur partagé** :
  1. Les générateurs reliés à un seul réseau servent d'abord leur réseau.
  2. Les générateurs partagés couvrent ensuite ce qui manque, au prorata des manques de chaque réseau.
  3. Si un réseau disjoncte, sa part revient aux autres réseaux du générateur.
- **Batteries** :
  - Elles absorbent les surplus et fournissent ce qui manque. Une batterie partagée fait passer de l'énergie d'un réseau à l'autre sans les relier.
  - Exemple : `G1 - A1 - B1 - A2 - G2`. G1 ne fournit que A1 + la charge de B1. G2 ne fournit que A2 + ce que B1 peut encore recevoir. Si G1 ou G2 ne produit pas assez, B1 fournit ce qui manque.
  - **Pas de débit maximum** de charge ou de décharge. Plus tard, peut-être certaines batteries avec un débit, à côté de batteries à débit infini.
- **Manque d'énergie** : si la production et les batteries ne suffisent pas, le réseau **disjoncte** et tout ce qu'il alimente s'arrête. Il se **réarme automatiquement** (après quelques secondes) quand la production et les batteries couvrent la demande de tout ce qui est allumé.
- **Contrôle des bâtiments** : on peut couper l'alimentation d'un bâtiment et programmer ses heures d'alimentation ([[F54 - Cycle jour nuit et météo|F54]]). Ces réglages sont ceux du tableau de distribution intégré au bâtiment ([[F56 - Horaires du tableau de distribution|F56]]). Un bâtiment coupé arrête ses machines, qui ne consomment plus, mais le courant le traverse toujours vers ce qui est relié derrière lui.
  - Fait ([[Historique - Features#^F64|F64]]) : coupure et réglages du tableau intégré. Le passage du courant vers un bâtiment relié attend les couloirs ([[Historique - Features#^F26|F26]]). Horaires : attend [[F54 - Cycle jour nuit et météo|F54]].

Voir aussi : [[Réseau électrique]]
