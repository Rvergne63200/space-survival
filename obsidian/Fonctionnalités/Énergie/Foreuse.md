---
tags: [jeu-colonie, fonctionnalite, energie, ressources]
---

# Foreuse

`Drill` (`EnergyConsumer`, 500 W), objet `37_Drill` (Drill), prefab `Prefabs/Environment/Energy/Drill`, modèle `Resources/Models/Drill` (derrick à trépied, 4,5 × 3,3 m, 4,4 m de haut) en enfant `Model`, avec son `Animator` (`Drill.controller`) et `DrillAnimator`.

- **Extraction** : alimentée (allumée et réseau qui la fournit), elle fore un minerai toutes les 20 s (`extractionTime`), tiré au hasard selon des poids (`outputs`) : Rock 40, Iron Ore 20, Copper Ore 15, Coal 15, Ice 7, Titanium Ore 3. Le monde n'a pas encore de gisements ([[F32 - Ressources générées|F32]]) : elle donne la même chose partout.
- Le temps ne compte que tête descendue dans le sol : à la mise en route, la tête accélère puis le chariot descend (quelques secondes).
- **Réserve** : `ItemStorage` de 6 emplacements. Quand elle ne peut plus prendre le prochain minerai, la foreuse s'arrête (tête remontée) et repart quand on la vide. Le joueur peut aussi y déposer ses objets (pas de restriction).
- **Interface** : interagir n'importe où sur la machine ouvre l'interface des conteneurs (`IStorageMachine`, comme le [[Générateur à charbon]]) : son état (éteinte, éteinte par le tableau, pas alimentée, réserve pleine, mise en route, en forage), une jauge du minerai en cours (secondes restantes), le bouton Allumer / Éteindre et la réserve.
- **Modèle** : l'écran holographique du pupitre est visible quand elle est alimentée ; la tête tourne et descend pendant le forage, remonte puis s'arrête sinon. Voyant bleu en forage, orange à l'arrêt. Pas de poussière (le point `DrillDust` n'a pas de `ParticleSystem`).
- Collisions : un `MeshCollider` sur chaque mesh du modèle (`BuildUtility.AddMeshColliders`). Point d'accroche du câble : `Connector`, sur le flanc du pupitre, à 30 cm du sol.
- **Pose** : dehors seulement (`OutdoorOnly`). Sur la carte du [[Panneau de contrôle]] : icône de machine.
- **Sauvegarde** : la réserve et l'interrupteur, pas le minerai en cours (perdu au rechargement).
- Pas de recette : seulement dans le coffre de test. Icône provisoire : celle de l'éolienne.

Voir aussi : [[Appareils électriques]] · [[Objets du réseau électrique]] · [[Objets posés]]
