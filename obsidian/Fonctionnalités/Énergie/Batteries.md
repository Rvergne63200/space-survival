---
tags: [jeu-colonie, fonctionnalite, energie, batteries]
---

# Batteries

Trois batteries (`EnergyStorage`, débits entrant et sortant infinis), prefabs dans `Prefabs/Environment/Energy`, modèles statiques dans `Resources/Models` (enfant `Model`, avec `BatteryAnimator`) :

| Item | Prefab | Modèle | Capacité | Pose |
|---|---|---|---|---|
| `34_WallBattery` (Wall Battery) | `WallBattery` | `Battery_Wall` (0,60 × 0,14 × 0,46 m) | 2 kWh | sur le mur plein d'un dôme (`PlaceableItemData.WallMounted`) |
| `35_BatteryCabinet` (Battery Cabinet) | `BatteryCabinet` | `Battery_Cabinet` (0,70 × 0,50 × 1,10 m) | 5 kWh | au sol, dedans ou dehors |
| `36_BatteryBank` (Battery Bank) | `BatteryBank` | `Battery_Bank` (1,60 × 0,90 × 1,20 m) | 20 kWh | dehors seulement (`OutdoorOnly`) |

- **Comportement** : celui de toute batterie du réseau (voir [[Réseau électrique]], règle 4). Posée dans un dôme, elle fait partie du circuit du bâtiment sans câble. Interagir l'allume ou l'éteint (`EnergyDeviceInfo`), et le texte au pointeur donne le stock, la capacité et le flux.
- **Jauge** (`BatteryIndicator` sur la racine → `BatteryAnimator` du modèle) : 5 segments néon `Gauge_1` (en bas) à `Gauge_5`, allumés selon le niveau (une batterie qui n'est pas vide en allume au moins un). En charge, le segment qui se remplit clignote. Éteinte, la jauge est éteinte.
- **Voyant** (`_Status`) : bleu quand elle marche et contient de l'énergie, orange quand elle est vide, éteint quand elle est éteinte.
- **Pose murale** : le fantôme se colle au mur plein du dôme visé, vu de l'intérieur (pas le sol, le verre ni l'extérieur), dos au mur, face au centre, centre à 0,58 m au-dessus de l'origine du dôme (`Builder.wallMountHeight`). Ailleurs, il reste rouge (« Se pose sur le mur plein d'un dôme »). R ne le tourne pas. Le mur du dôme ne compte pas dans le test d'encombrement, les connecteurs et le tableau intégré si.
- Colliders : une boîte autour du modèle. Point d'accroche du câble : `Connector` (dessous du pack mural, dessus de l'armoire à l'arrière, flanc du banc).
- Pas de recette : seulement dans le coffre de test. Icône provisoire : celle de la batterie de test.

Voir aussi : [[Appareils électriques]] · [[Objets du réseau électrique]] · [[Objets posés]]
