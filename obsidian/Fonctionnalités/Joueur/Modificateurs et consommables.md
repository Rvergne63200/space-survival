---
tags: [jeu-colonie, fonctionnalite, joueur, stats, inventaire]
---

# Modificateurs et consommables

- **Modifiers (effets)** : `AbstractModifier.Modify(GameObject)`.
  - `StatModifier` (générique) : modifie une stat de `amount`, d'un coup (`duration` = 0) ou réparti sur `duration` secondes.
  - `LifeModifier` ajoute un marker sur la vie (montant/s pendant `duration`). `EnduranceModifier` ne fait qu'un `Debug.Log`.
- **Consommables** : `ConsummableData` (ScriptableObject, hérite d'ItemData) contient une liste de modifiers (`SerializeReference` + SubclassPicker), appliqués à la consommation depuis `PlayerInventory` (clic droit, voir [[Utilisation des objets en main]]).
- **Consommables de test** : `6_Ration` (faim +40) et `7_Water` (soif +50), dans le coffre de test (voir [[Scènes et chargement]]).
- **First Aid Kit** (`23_FirstAidKit`) : consommable, vie +50 d'un coup.

Voir aussi : [[Stats du joueur]] · [[Données des items]]
