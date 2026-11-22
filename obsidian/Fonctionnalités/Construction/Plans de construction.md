---
tags: [jeu-colonie, fonctionnalite, construction]
---

# Plans de construction

`StructureBlueprint` (plan).

- **Hologramme bleu** (shader `Custom/Hologram` : lueur additive, bords lumineux, lignes de balayage animées, léger scintillement ; matériau `Resources/Materials/Blueprint`).
- Il a un collider pour être visé, sur le layer `Blueprint`, qui n'entre pas en collision avec le joueur : on le traverse (matrice de collision, et `Physics.IgnoreLayerCollision` par sécurité). La détection du sol du joueur l'ignore aussi. Pour interagir avec le plan, il faut le viser depuis l'extérieur.
- Interagir dépose tout ce qui manque et qu'on a dans l'inventaire. Le texte affiche l'avancement (« Dôme (plan) : Iron 12/20, WoodLog 4/30 »).
- Une fois tout apporté, il devient la structure réelle (`BuiltStructure`).
- Détruit, il rend ce qui lui a été apporté.
- Le plan d'un sas ou d'un couloir montre aussi son ou ses connecteurs en plan (`ConnectorPreview` avec le matériau du plan), jusqu'à la construction.

Voir aussi : [[Pose et destruction]] · [[Structures et catalogue]]
