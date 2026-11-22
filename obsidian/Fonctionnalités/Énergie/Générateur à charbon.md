---
tags: [jeu-colonie, fonctionnalite, energie, generateurs]
---

# Générateur à charbon

`CoalGenerator` (`EnergyGenerator`), objet `30_CoalGenerator`, prefab `Prefabs/Environment/Energy/CoalGenerator`, modèle `Resources/Models/CoalGenerator` avec son `Animator` et `CoalGeneratorAnimator`.

- **Combustible** : une réserve d'un emplacement qui n'accepte que le charbon (`ItemStorage`, 99, restriction remise à chaque frame car non sauvegardée). Allumé, il brûle un charbon après l'autre, `burnTime` (60 s) chacun, et donne toute sa puissance (`generation`, 1000 W) tant qu'un charbon brûle, quoi que prenne le réseau. Plus de charbon : il s'arrête (allumé, mais sans production). Éteint (ou éteint par un tableau), le charbon en cours est perdu : rallumé, il passe au suivant.
- **Interface** : interagir n'importe où sur la machine ouvre son interface, celle des conteneurs (`ItemStorageUI`, `IStorageMachine`) : son nom en titre, son état (en marche avec sa production, éteint, éteint par le tableau, plus de charbon), une jauge du charbon en cours (secondes restantes), le bouton Allumer / Éteindre et la réserve, à côté de l'inventaire. La trappe de la trémie s'ouvre avec l'interface (`OpenHatch`) et se referme avec elle (`CloseHatch`). Collisions : un `MeshCollider` sur chaque mesh du modèle.
- **Modèle** : en marche tant qu'un charbon brûle (`TurnOn` / `TurnOff`).
- **Cheminée** : dehors, la plus courte possible (`MinChimneyHeight`) et sans collier (`HideCollar`). Dans un dôme, elle traverse la coupole (1,5 m au-dessus, `chimneyAboveRoof`, `SetChimneyTopWorldHeight`), avec le collier à la traversée (`SetCollarWorldHeight`), orienté selon la normale du dôme depuis le centre de sa sphère (1,2 m au-dessus de son origine, `SetCollarRotation`).
  - Le collier cache la traversée de la structure du dôme : la structure est mesurée sous la cheminée (face extérieure par un rayon venant d'en haut, face intérieure par un rayon partant de la cheminée vers le haut, faces vues de dos comprises pour ce rayon ; 6 cm au moins), le collier est centré sur son épaisseur (`SetCollarWorldHeight`) et plus épais qu'elle de 3 cm de chaque côté selon sa normale (`SetCollarThickness`, ajouté à `CoalGeneratorAnimator`, qui étire le disque du modèle). Débord de 30 cm autour du fût (`collarOverhang`, sur le prefab).
  - Le toit est cherché par un rayon venant d'en haut (la peau extérieure du dôme) toutes les 2 s.
  - Le fantôme de pose montre la cheminée et le collier tels qu'ils seront, mis à jour en direct (`IPlacementPreview`).
- **Pose** : dehors ou dans un dôme, pas dans un couloir ni un sas (`PlaceableItemData.NotInCorridors`).
- **Sauvegarde** : la réserve et l'interrupteur, pas le charbon en cours (perdu au rechargement).
- Reste : sa consommation d'oxygène ([[F51 - Système d'oxygène|F51]], [[F52 - Machines consommatrices d'oxygène|F52]]).

Voir aussi : [[Appareils électriques]] · [[Dômes]] · [[Pose et destruction]]
