---
tags: [jeu-colonie, fonctionnalite, interaction, rendu]
---

# Contour de l'objet pointé

`InteractableOutline`, sur le prefab `PlayerCamera` : un contour blanc en espace écran entoure l'objet interactable pointé.

- Un `CommandBuffer` (`BeforeImageEffects`) dessine la silhouette des `MeshRenderer` et `SkinnedMeshRenderer` de l'objet, tous sous-meshes compris, dans une texture masque. Le shader `Hidden/InteractableOutline` (`Assets/Shaders`) trace ensuite le contour autour du masque.
- Le buffer n'est reconstruit que quand l'objet pointé change. Réglages : `outlineColor` (blanc) et `outlineWidth` (3 px).
- Le contour suit toute la silhouette, y compris les parties cachées derrière un autre objet (voir [[F17 - Contour masqué par les obstacles|F17]]).

Voir aussi : [[Pointeur et interactables]]
