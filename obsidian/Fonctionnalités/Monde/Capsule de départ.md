---
tags: [jeu-colonie, fonctionnalite, monde, modeles]
---

# Capsule de départ

Prefab `SpaceCapsule`, posé dans `0_World`.

- Le modèle `Resources/Models/LandingCapsule` (cabine octogonale sur 4 pieds, porte qui se rabat en rampe), avec son `Animator` (`LandingCapsule.controller`), `LandingCapsuleAnimator` et `LandingCapsule`.
- L'intérieur est vide pour l'instant (sol de la cabine à 0,72 m au-dessus de l'origine du modèle).
- La racine est posée sur le terrain à « hauteur + 0,7 m » (`ChunkManager.ToReplace`), le modèle est 0,75 m plus bas : les pieds touchent le sol.
- Le joueur commence avec un inventaire et une barre d'outils vides. Un coffre de test avec tous les items est posé à côté (voir [[Scènes et chargement]]).
- **Porte** (`LandingCapsuleDoor`) : « Ouvrir la porte » / « Fermer la porte ». Interagir avec le battant lance `Door_Open` ou `Door_Close` (`LandingCapsuleAnimator.Toggle`) ; pendant le mouvement, une nouvelle interaction repart vers l'autre position.
- `LandingCapsule` (racine du modèle) ajoute à l'Awake un `MeshCollider` sur chaque mesh du modèle (coque, sol, pieds, et battant qui devient la rampe), le `LandingCapsuleDoor` sur l'enfant `Door` et un `SaveableObject` : la porte reste ouverte ou fermée dans la sauvegarde (reposée sans animation, `SetOpenImmediate`).

Voir aussi : [[Pointeur et interactables]] · [[Sauvegarde]]
