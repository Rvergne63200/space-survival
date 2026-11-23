---
tags: [jeu-colonie, fonctionnalite, energie]
---

# Appareils électriques

- **EnergyDevice** (base) : un appareil du réseau, relié par des câbles. Les câbles s'accrochent à `connector`, ou à défaut à l'enfant `Connector`, ou au haut de l'objet. Détruit avec son objet, il détruit et rend ses câbles.
  - **EnergyConsumer** : consommation en W. Il est alimenté (`IsPowered`, `ev_OnPoweredChanged`) quand son réseau n'est pas disjoncté.
  - **EnergyGenerator** : production en W. Il tourne toujours à fond. `Output` est ce qu'il a réellement donné. Générateurs actuels : [[Panneau solaire]], [[Éolienne]], [[Générateur à charbon]].
  - **EnergyStorage** : capacité, stock (sauvegardé) et débits entrant et sortant.
- **On / off** (`SwitchedOn`, `SetSwitchedOn`, sauvegardé) : éteint, un consommateur ne consomme plus (état « éteinte »), un générateur ne produit plus, une batterie ne charge ni ne décharge. Le câble passe toujours : un appareil éteint relie toujours son réseau. Les ports d'un tableau de distribution ne s'éteignent pas (`CanSwitch`), le tableau a sa propre coupure.
  - **Tableau éteint** (règle 6 du [[Réseau électrique]]) : son circuit est éteint de force (`ForcedOff`, recalculé à chaque mise à jour par `EnergyNetworkManager.UpdateForcedOff`). Un appareil ne marche que si son réglage est sur on **et** si aucun tableau au-dessus de lui n'est éteint (`IsOn`). Son réglage est gardé, et bloqué tant qu'il est éteint de force (« éteint par le tableau »).
- **EnergyDeviceInfo** (interactable) : en visant l'objet, affiche l'état de ses appareils. La touche d'interaction les allume ou les éteint.
  - Consommateur : alimenté, réseau disjoncté, non raccordé ou éteinte, avec sa consommation.
  - Générateur : donné / production (« éteint » s'il l'est).
  - Batterie : stock / capacité et flux (« éteinte » si elle l'est).

Voir aussi : [[Réseau électrique]] · [[Câble électrique]] · [[Objets du réseau électrique]]
