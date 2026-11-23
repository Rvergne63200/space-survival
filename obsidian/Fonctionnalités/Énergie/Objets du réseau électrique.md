---
tags: [jeu-colonie, fonctionnalite, energie, items]
---

# Objets du réseau électrique

## Items du réseau
`ScriptableObjects/Items` :
- `25_ElectricCable` : le câble, 1 par mètre (voir [[Câble électrique]]).
- `26_DistributionPanel` : le [[Tableau de distribution]].
- `27_ControlPanel` : le [[Panneau de contrôle]].
- `28_ChargingStation` : la [[Station de recharge]].
- `29_SolarPanel` : le [[Panneau solaire]].
- `30_CoalGenerator` : le [[Générateur à charbon]].
- `33_WindTurbine` : l'[[Éolienne]].

## Objets de test
`ScriptableObjects/TestItems` ; prefabs dans `Prefabs/Environment/Energy`, matériau `Materials/EnergyTest` :
- `T001_InfiniteGenerator` : générateur, 1000 W par défaut.
- `T002_TestMachine` : consommateur de 100 W, avec `TestMachine`. Elle est rouge quand elle est alimentée, blanche sinon.
- `T003_TestBattery` : batterie de 1000 Wh, avec des débits de 500 W.

Voir aussi : [[Données des items]] · [[Appareils électriques]]
