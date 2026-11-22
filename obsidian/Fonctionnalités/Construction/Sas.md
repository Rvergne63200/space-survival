---
tags: [jeu-colonie, fonctionnalite, construction, batiments]
---

# Sas

`AirlockData` : hérite de `StructureData`, distance d'accroche `snapDistance` ; `Airlock` ; prefab `Prefabs/Build/Airlock` avec le modèle, son `Animator` et `AirlockAnimator` ; asset `ScriptableObjects/Structures/Airlock`, dans le catalogue. Coût 10 fer + 5 bois, rend 5 fer.

- **Pose** (`Builder`, mode sas) : posé librement là où l'on vise. Quand le mur d'un dôme fini est à moins de 3 m du point visé, le fantôme se colle au mur, sur la bride, tourné vers l'extérieur, avec le fantôme du connecteur. Sinon il est rouge et ne peut pas être posé. Aussi invalide : connecteur trop proche d'un autre ou de la borne électrique, obstacle (le terrain et le dôme exceptés). La pose aplanit le terrain sous le sas au niveau du dôme.
- **Fini** : colliders sur son modèle, et le connecteur du dôme (qui ouvre le mur). On ne peut rien placer à l'intérieur (boîte englobante de son modèle).
- **Portes** (`AirlockDoor`, ajouté par `Airlock` sur les battants `Door_Inner_*` et `Door_Outer_*`) : on interagit avec un battant pour ouvrir ou fermer sa porte, à travers les séquences d'`AirlockAnimator` : ouvrir une porte ferme d'abord l'autre et fait le cycle du sas (pressurisé côté dôme, dépressurisé côté extérieur). Texte « Ouvrir la porte » / « Fermer la porte », ou « Sas en cours de cycle » : pendant une séquence, l'interaction est ignorée. Le cycle est joué 2 fois plus vite que le clip (`cycleSpeed` d'`AirlockAnimator`, sur le prefab `Airlock`). Le sas n'utilise pas d'électricité.
- **Sauvegarde** : l'id de son dôme (composant `Airlock`).
- **Destruction** : directement, ou avec son dôme (plan compris) : il rend ses ressources.

Voir aussi : [[Dômes]] · [[Connecteurs de dôme]] · [[Terrain sous les constructions]]
