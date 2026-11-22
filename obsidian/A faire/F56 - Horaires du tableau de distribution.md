---
tags: [jeu-colonie, a-faire, feature, energie]
id: F56
categorie: Feature
feature: Énergie
statut: en cours
---

# F56 - Horaires du tableau de distribution

**Tableau de distribution** (`Distribution Panel`) : il crée un sous-réseau et permet de le gérer.
- **Fonctionnement** :
  - Il a une **entrée** (l'énergie qui arrive) et une **sortie**. Tout ce qui est relié après sa sortie forme son sous-réseau.
  - Il transmet au sous-réseau toute l'énergie qu'il reçoit. Vu de l'amont, le sous-réseau entier est **un seul consommateur**.
  - **Sens unique** : un générateur ou une batterie du sous-réseau ne renvoie jamais d'énergie vers l'amont.
- **Coupure** : couper le tableau arrête tout ce qui est derrière lui.
- **Tableau intégré aux bâtiments** : chaque bâtiment a un tableau de distribution intégré. Sa coupure et ses horaires ([[F50 - Réseau électrique complet|F50]]) sont ceux de ce tableau.
  - Fait ([[Historique - Features#^F64|F64]]), sauf les horaires.
- **Protection** :
  - **Limite de puissance** (W max) : si le sous-réseau demande plus, seul le tableau saute, l'amont continue de tourner.
  - **Délestage automatique** : il coupe son sous-réseau quand l'amont manque d'énergie, ou quand la charge des batteries de l'amont passe sous X %. Il réarme quand ça remonte. On protège ainsi l'essentiel en sacrifiant le secondaire.
  - **Priorité** : en cas de manque, les tableaux de priorité faible coupent en premier.
  - Sans délestage ni priorité configurés, un manque d'énergie fait disjoncter tout le réseau, sous-réseaux compris ([[F50 - Réseau électrique complet|F50]]).
- **Automatisation** :
  - **Horaires** : alimentation par plages horaires ([[F54 - Cycle jour nuit et météo|F54]]). **Reste à faire** (attend [[F54 - Cycle jour nuit et météo|F54]]) : tout le reste de F56 est fait ([[Historique - Features#^F59|F59]]).
  - **Démarrage différé** : après une coupure, il attend X secondes avant de réarmer, pour échelonner les redémarrages.
  - Les conditions (personne dans le bâtiment, jour ou nuit…) ne sont pas prévues pour l'instant.
- **Affichage** : consommation instantanée du sous-réseau, énergie consommée au total, état (en marche, coupé, sauté, et pourquoi).

Voir aussi : [[Tableau de distribution]] · [[Électricité des bâtiments]]
