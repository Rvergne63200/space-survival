---
tags: [jeu-colonie, instructions, code, energie]
---

# Ajouter un appareil électrique

Les règles du réseau sont dans [[Réseau électrique]] (et identiques dans l'en-tête de `EnergyNetworkManager`) ; les appareils dans [[Appareils électriques]]. Elles ne sont pas répétées ici.

## Choisir la base
- **Consommateur** : hérite d'`EnergyConsumer` ou ajoute un `EnergyConsumer` à côté de ton composant. Une consommation variable se fait en surchargeant `Consumption` (exemple : la station de recharge ne demande sa puissance que pendant une charge). L'état d'alimentation se lit par `IsPowered` et l'événement `ev_OnPoweredChanged`.
- **Générateur** : hérite d'`EnergyGenerator` et surcharge `Generation` (exemple : `SolarPanel` selon le soleil, `CoalGenerator` seulement quand un charbon brûle).
- **Stockage** : `EnergyStorage` (capacité, stock sauvegardé, débits).
- Puissances en **W**, énergies en **Wh**, avec `EnergyValue` (case « ∞ » possible dans l'Inspector).

## À respecter
- Un appareil s'enregistre seul auprès d'`EnergyNetworkManager` dans `OnEnable` / `OnDisable` : si tu surcharges ces méthodes, appelle la version de base. Un changement qui modifie le réseau (câble, appareil) appelle `EnergyNetworkManager.Instance.MarkDirty()`.
- Point d'accroche des câbles : un enfant nommé `Connector` (ou le champ `connector`), sinon le haut de l'objet.
- **On / off** : utilise `IsOn` (réglage de l'appareil **et** tableaux au-dessus), jamais `SwitchedOn` seul, pour décider si l'appareil tourne.
- L'état est déjà sauvegardé par `EnergyDevice` (`ISaveableComponent`) : surcharge `SaveKey` / `CaptureState` / `RestoreState` en appelant la base si tu ajoutes des données.
- `GetInfo()` (abstrait) donne le texte d'état, en français. Pour l'interaction standard (état + on / off), ajoute `EnergyDeviceInfo` sur l'objet.
- Le modèle s'allume ou s'éteint par son `<Model>Animator` (`PowerOn` / `PowerOff` / `SetPowered`), selon `IsOn` et l'alimentation.
- Vérifie qu'il apparaît correctement sur la carte du panneau de contrôle (icône selon le type d'appareil).
- Pour tester : les objets `T001` (générateur infini), `T002` (machine de test) et `T003` (batterie de test).

Voir aussi : [[Objets du réseau électrique]] · [[06 - Interactions]]
