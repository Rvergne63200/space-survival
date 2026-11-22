---
tags: [jeu-colonie, fonctionnalite, joueur, camera]
---

# Caméra

Composant `CameraMovement`.

- Rotation à la souris (`sensibility`, 0.15), pitch limité à ±90°. La rotation est faite dans `Update`. La caméra suit `Target.position + offset` dans `LateUpdate`, y compris quand la rotation est verrouillée.
- **FOV au sprint** : le FOV augmente de `sprintFovIncrease` (+10°) quand `PlayerMovement.IsSprinting` est vrai, c'est-à-dire en sprint et plus vite que la marche. La transition est lissée (`fovChangeSpeed`) et le FOV de base est celui de la caméra.
- Peut être bloquée par l'option de mode `camera_movement_locked`.

Voir aussi : [[Déplacement du joueur]] · [[Modes de jeu et interfaces]]
