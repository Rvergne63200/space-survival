---
tags: [jeu-colonie, fonctionnalite, energie]
---

# Tableau de distribution

`DistributionPanel`, sur la racine du prefab `Prefabs/Environment/Energy/DistributionPanel`.

- **Ports** : deux ports enfants. `DistributionPanelInput` (`EnergyConsumer`, cube vert) est un consommateur du réseau amont. `DistributionPanelOutput` (`EnergyGenerator`, cube orange) est un générateur de son circuit.
- **Branchement** : un câble se branche sur le port le plus proche du point visé (`EnergyDevice.FromHit`). Les câbles sauvegardent le port (`PortId`). L'entrée et la sortie d'un même tableau ne peuvent pas être reliées.
- **Sens unique** : rien ne remonte vers l'amont. La sortie est servie **après** les générateurs du circuit, et son surplus charge les batteries du circuit après le leur.
- **Calcul** (`EnergyNetworkManager.Simulate`) :
  1. **Demande** : les sorties donnent sans limite, et ce qu'elles donnent est la demande du circuit (`Request`). Ce qu'elles donnent aux consommateurs est **obligatoire** (`Request`), ce qu'elles donnent aux batteries est **facultatif** (`ChargeRequest`). Pour un tableau sous un autre, la demande de ses machines compte dans la demande du tableau du dessus, et sa recharge (plafonnée par sa puissance max) dans la recharge du tableau du dessus. Le calcul est répété pour les tableaux imbriqués. Les réseaux disjonctés comptent aussi, pour pouvoir se réarmer. Les demandes sont recalculées après les décisions des tableaux quand l'un d'eux démarre ou s'arrête : un circuit qui démarre sous un autre ne le fait plus disjoncter (`ComputeDemands`).
  2. **Décisions** du tableau (`BeginUpdate`) : on/off, fusible (demande au-dessus de la limite), délestage sur batteries amont (sous le seuil, réarmé 5 % au-dessus), démarrage différé.
  3. **Distribution** : l'entrée demande la part obligatoire à l'amont. La part facultative (la recharge, dans la limite de puissance) est servie comme une consommation, avant la recharge des batteries amont, mais ne fait pas disjoncter si elle n'est pas couverte entièrement. Elle est aussi servie par les batteries du réseau d'entrée, une fois leurs manques couverts (dans la limite de leur débit sortant, stock compris). La sortie donne au circuit la part obligatoire plus ce qui a été reçu pour la recharge. Un réseau en manque déleste d'abord ses tableaux réglés sur « manque d'énergie » (priorité la plus faible en premier), puis disjoncte. Un tableau dont le réseau d'entrée disjoncte ne donne plus rien. Les réseaux ne disjonctent (ou ne délestent) qu'une fois la part facultative arrivée partout : elle descend d'un tableau par passe.
  4. **Après la distribution** (`EndUpdate`) :
     - un tableau délesté ou sans alimentation repart quand l'amont pourrait de nouveau le servir ;
     - pour un tableau délesté, après 3 s ;
     - puis il attend son démarrage différé.
- **États** : en marche, éteint (ou éteint par le tableau du dessus), démarrage, pas d'alimentation en entrée, sauté, délesté, boucle (la sortie alimente le réseau de sa propre entrée, par exemple une batterie ou un générateur câblé à la fois sur l'amont et sur le circuit).
- **Sauvegarde** : les réglages et l'énergie transmise au total.
- **Limite** : un tableau de distribution délesté ne se réarme que sur les sources de son réseau d'entrée (pas sur ce que l'amont d'un autre tableau pourrait donner en plus). Les tableaux intégrés aux bâtiments n'ont pas cette limite (règle 7 du [[Réseau électrique]]).

## DistributionPanelUI
`UI.prefab`, `DistributionPanel.uxml`, style dans `Game.uss`. Ouverte en interagissant avec le tableau. C'est une interface, fermée par Échap.
- L'état et les réglages sont dans `DistributionPanelSettings.uxml` (`DistributionPanelSettingsView`), repris par le [[Panneau de contrôle]].
- **Affichage** : état, demande du circuit (ses consommateurs seulement), puissance transmise, puissance réellement envoyée en recharge des batteries du circuit (`ChargeSupplied`), énergie transmise. Le texte au pointeur affiche aussi la part en recharge. La part envoyée aux prises de sortie de bâtiments en aval est affichée à part (« Sortie », `OutletSupplied`), ni dans la demande du circuit ni dans la recharge.
- **Réglages** : bouton Éteindre / Allumer (voir « Tableau éteint » dans [[Appareils électriques]] ; bloqué si un tableau au-dessus est éteint), limite de puissance (à 0 W, elle coupe le circuit) (ou illimitée), délestage (aucun, manque d'énergie en amont, batteries en amont sous le seuil), seuil des batteries, priorité, démarrage différé.

Voir aussi : [[Réseau électrique]] · [[Électricité des bâtiments]]
