---
tags: [jeu-colonie, a-faire, feature, oxygene]
id: F51
categorie: Feature
feature: Oxygène
statut: à faire
---

# F51 - Système d'oxygène

**Système d'oxygène** (modèle détaillé de [[F27 - Réseau d'oxygène|F27]]) :
- **Unités** :
  - L'oxygène se compte en **litres**. Les débits (production, consommation, régulateurs) sont en **L/s**.
  - Le volume d'un bâtiment se mesure en **m³**, converti en litres pour calculer la **concentration** (% d'O2 dans l'air).
  - Dehors, la concentration est nulle (peut-être 1 à 2 % plus tard).
- **Respiration** : sous un seuil de concentration, le joueur ne peut plus respirer et sa barre d'oxygène baisse. Plus la concentration est basse, plus la barre baisse vite.
- **Réseaux** :
  - Tout ce qui est relié par des tuyaux forme **un réseau**, qui partage son oxygène sans notion de sens de circulation.
  - Calcul par paliers dans un gestionnaire persistant, comme pour l'électricité, et sauvegardé.
- **Appareil sans tuyau** :
  - Il puise dans l'air autour de lui et y rejette son oxygène : dans le bâtiment s'il est à l'intérieur, sinon dehors, où l'oxygène est perdu.
  - Avec un tuyau en entrée, il puise dans le tuyau. Avec un tuyau en sortie, il envoie son oxygène dans le tuyau.
- **Bâtiments** :
  - Un bâtiment, ou un ensemble de bâtiments reliés ([[Historique - Features#^F26|F26]]), a un volume et une quantité d'O2.
  - Chaque bâtiment a une **valve unidirectionnelle intégrée** qui vise **toujours 21 %** (non configurable). Il reçoit l'oxygène du réseau jusqu'à 21 %, mais ne lui en rend jamais. Seul un récolteur posé dedans peut en sortir de l'oxygène.
  - **Plafond** : au-delà de 21 % (plantes, machines sans tuyau), le surplus s'échappe. Un récolteur peut le récupérer avant : l'échappement est le dernier recours.
- **Tuyau qui entre dans un bâtiment** :
  - **Sortie libre** : l'oxygène se diffuse dans le bâtiment, qui fait alors partie du réseau.
  - **Sortie reliée** à un autre tuyau : le tuyau traverse le mur et le bâtiment n'est pas dans le réseau. Exemple : un générateur d'O2 dehors dédié à un générateur à charbon dedans.
- **Éléments** :
  - **Générateur d'O2** : il produit X L/s. Il ralentit, voire s'arrête, quand le réseau ne peut plus rien recevoir.
  - **Réservoir** : il stocke l'oxygène du réseau et le rend selon les besoins, comme une batterie.
  - **Régulateur** :
    - Vanne à **sens unique** avec un **débit maximum** configurable (0 = fermée). Il sépare deux sous-réseaux.
    - Il sert à limiter le débit vers une machine, à fermer l'accès à un réservoir ou à favoriser un réservoir. Exemple : `BAT1 <- REGUL <- GEN -> REGUL -> BAT2`.
  - **Récolteur** :
    - Il aspire l'oxygène de l'air autour de lui, avec un **débit configurable**, et l'envoie dans son tuyau. Exemple : dans un bâtiment, devant un générateur à charbon dont on veut limiter la consommation.
    - Il consomme d'autant plus d'électricité que le débit est élevé et que la concentration de l'air est basse.
    - Il peut être posé dehors (inutile s'il n'y a pas d'O2).
- **Répartition dans un réseau** :
  1. Les consommateurs (machines, bâtiments sous 21 %) sont servis en premier.
  2. Le surplus va aux réservoirs, qui complètent en cas de manque.
  - Quand il n'y en a pas assez pour tout le monde : **parts égales plafonnées par les débits max**, la part qu'un consommateur ne peut pas prendre allant aux autres. Avec des plafonds de 3 et 10 L/s : 5 L/s produits donnent 2,5 et 2,5 ; 9 L/s donnent 3 et 6.
  - Ce système peut être biaisé volontairement, par exemple avec un récolteur dans un bâtiment relié à un réservoir.
- **Sas** : ouvrir une porte qui donne dehors fait perdre de l'O2, il faut donc un sas. Pas de bâtiments endommagés pour l'instant.

Voir aussi : [[Intérieur des bâtiments]] · [[Sas]]
