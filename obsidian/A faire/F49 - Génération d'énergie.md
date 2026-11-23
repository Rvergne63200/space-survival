---
tags: [jeu-colonie, a-faire, feature, energie]
id: F49
categorie: Feature
feature: Énergie
statut: en cours
---

# F49 - Génération d'énergie

**Génération d'énergie** : panneaux solaires, éoliennes, générateur à charbon (consomme du `Coal` et de l'oxygène, voir [[F51 - Système d'oxygène|F51]]), etc.
- [x] Panneau solaire (`SolarPanel`, objet 29) : puissance selon la hauteur du soleil et l'orientation des cellules, 1000 W max à midi, 0 la nuit ; se déplie et suit le soleil allumé, se replie éteint. Se pose uniquement dehors (`PlaceableItemData.OutdoorOnly`).
- [x] Éolienne (`WindTurbine`, objet 33) : 800 W max à partir de 12 m/s de vent, rien sous 3 m/s ; se déploie et se tourne face au vent allumée, se replie éteinte ou quand le vent dépasse 20 m/s (tempête). Se pose uniquement dehors. Reste : une recette et son icône.
- [x] Générateur à charbon (`CoalGenerator`, objet 30) : 1000 W tant qu'un charbon brûle (60 s chacun, réserve de 99 par la trappe), une seule interface (réserve, état, jauge du charbon en cours, Allumer / Éteindre), charbon en cours perdu quand on l'éteint, fantôme de pose avec la cheminée réglée en direct, cheminée qui traverse la coupole d'un dôme avec son collier. Reste : sa consommation d'oxygène ([[F51 - Système d'oxygène|F51]]).

Voir aussi : [[Panneau solaire]] · [[Éolienne]] · [[Générateur à charbon]]
