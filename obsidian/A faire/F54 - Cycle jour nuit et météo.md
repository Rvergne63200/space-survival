---
tags: [jeu-colonie, a-faire, feature, monde]
id: F54
categorie: Feature
feature: Monde
statut: en cours
---

# F54 - Cycle jour nuit et météo

**Cycle jour / nuit et météo** :
- **Cycle** : le soleil tourne, l'heure du jeu est sauvegardée et la durée d'une journée est réglable : **25 minutes** pour commencer (15 de jour, 10 de nuit), à ajuster si nécessaire. Le cycle sert aux panneaux solaires ([[F49 - Génération d'énergie|F49]]), à la lumière du soleil pour les plantes ([[F53 - Système de plantation|F53]]) et aux horaires d'alimentation ([[F50 - Réseau électrique complet|F50]]).
  - [x] Fait (`DayNightCycle`) : soleil de 6:00 à 18:00 (15 min de jour, 10 min de nuit), lune, nuits très sombres avec quelques nuits claires (tirées de la seed), crépuscule, heure sauvegardée, départ à 8:00, horloge dans le HUD sous la minimap (`ClockUI`).
  - Fait : panneaux solaires ([[F49 - Génération d'énergie|F49]]). Reste : les plantes ([[F53 - Système de plantation|F53]]) et les horaires d'alimentation ([[F50 - Réseau électrique complet|F50]]/[[F56 - Horaires du tableau de distribution|F56]]).
- **Météo** : vent variable pour les éoliennes, tempêtes de poussière qui réduisent le solaire, etc.

Voir aussi : [[Cycle jour et nuit]] · [[Panneau solaire]]
