---
tags: [jeu-colonie, fonctionnalite, energie, generateurs]
---

# Éolienne

`WindTurbine` (`EnergyGenerator`), objet `33_WindTurbine`, prefab `Prefabs/Environment/Energy/WindTurbine`, modèle `Resources/Models/WindTurbine` avec son `Animator` et `WindTurbineAnimator` (enfant `Model`). Générateur dont la puissance suit le vent de la [[Météo]].

- **Puissance** : `generation` (le max, 800 W sur la prefab) × une courbe du vent : rien sous `cutInSpeed` (3 m/s), le max à partir de `ratedSpeed` (12 m/s), progression douce entre les deux. Rien tant qu'elle n'est pas complètement déployée.
- **Allumée** (par défaut, à la pose) : le mât sort, les pales se déplient, la nacelle se tourne face au vent et le rotor tourne avec lui (`rotorSpeedAtRated` 1,5 à 12 m/s, proportionnel au vent, 4 au plus). **Éteinte** (ou éteinte par un tableau) : le rotor ralentit, les pales se replient et le mât rentre. Interagir l'allume ou l'éteint (`EnergyDeviceInfo`).
- **Vent trop fort** (tempête de sable) : au-dessus de `cutOutSpeed` (20 m/s), elle se replie pour se protéger (« repliée : vent trop fort ») et se redéploie quand le vent repasse sous `cutOutSpeed` − `resumeMargin` (17 m/s). Pas sauvegardé : recalculé au chargement.
- **Chargement** : rechargée allumée, elle rejoue son déploiement (le `WindTurbineAnimator` n'a pas de pose immédiate, contrairement au panneau solaire).
- Se pose uniquement dehors (`PlaceableItemData.OutdoorOnly`, voir [[Objets posés]]). Pas de recette : seulement dans le coffre de test. Icône provisoire : celle du panneau solaire.
- Collider : une boîte autour du modèle replié (1,2 × 2,08 × 1,2 m). Point d'accroche du câble : `Connector`, à 0,4 m du sol.

Voir aussi : [[Météo]] · [[Appareils électriques]] · [[Panneau solaire]]
