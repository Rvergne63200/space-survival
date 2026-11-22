---
tags: [jeu-colonie, fonctionnalite, energie, generateurs]
---

# Panneau solaire

`SolarPanel` (`EnergyGenerator`), objet `29_SolarPanel`, prefab `Prefabs/Environment/Energy/SolarPanel`, modèle `Resources/Models/SolarPanel` avec son `Animator` et `SolarPanelAnimator`. Générateur dont la puissance suit le soleil.

- **Puissance** : `generation` (le max, 1000 W par défaut, réglable sur la prefab) × lumière du soleil × incidence.
  - Lumière : hauteur du soleil / sa hauteur à midi (`DayNightCycle.SunDirection`, `NoonSunHeight`), donc 0 la nuit et au lever / coucher.
  - Incidence : cosinus entre les cellules (inclinées de `panelTilt`, 30°, vers l'avant de la tête) et le soleil. Face au soleil à midi, les cellules lui sont perpendiculaires : puissance max.
  - Environ 180 W à 7:00, 420 W à 8:00, 850 W à 10:00. Rien tant qu'il n'est pas complètement déplié.
- **Allumé** (par défaut, à la pose) : il se déplie, puis sa tête se tourne vers le soleil (`SolarPanelAnimator`, 20°/s). **Éteint** (ou éteint par un tableau) : il se replie et sa tête revient à sa pose de repos, vers l'avant de l'objet. Interagir l'allume ou l'éteint (`EnergyDeviceInfo`). Rechargé avec le monde, il reprend directement sa pose (dépliée si allumé).
- Se pose uniquement dehors (`PlaceableItemData.OutdoorOnly`, voir [[Objets posés]]).
- Collider : une boîte autour du socle et du panneau replié. Pas encore de météo.

Voir aussi : [[Cycle jour et nuit]] · [[Appareils électriques]]
