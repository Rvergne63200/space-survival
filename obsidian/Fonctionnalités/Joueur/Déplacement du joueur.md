---
tags: [jeu-colonie, fonctionnalite, joueur, mouvement]
---

# Déplacement du joueur

Vue à la première personne uniquement. Composant `PlayerMovement`.

- Joueur physique : Rigidbody de 75 kg avec rotations bloquées et **interpolation**, et collider capsule (rayon 0.5, hauteur 2, scale 0.85). Un matériau physique **sans friction** est créé dans `Awake` si aucun n'est assigné. Les entrées passent par le New Input System (`PlayerInputActions.Movement`).
- Physique dans `FixedUpdate`. Le déplacement est relatif à la caméra (plan horizontal). La vitesse tend vers la vitesse visée avec une accélération plafonnée (`acceleration`, en m/s²).
- **Pentes** : au sol, la vitesse est contrôlée dans le plan de la pente (même vitesse en montée et en descente). La composante de la gravité parallèle à la pente est annulée, donc le joueur reste immobile sur une pente sans input. En l'air, seule la vitesse horizontale est contrôlée.
- **Marche** (`baseSpeed`, 4.5 m/s) et **sprint** (`sprintSpeed`, 7 m/s), avec une accélération de 50 m/s². Le sprint consomme de l'endurance et s'arrête quand elle est vide.
- **Gravité renforcée** pour le joueur seulement (`gravityMultiplier`, ×2), pour un saut moins flottant.
- **En l'air** : contrôle réduit (`airControl`, 30 %), et l'élan est conservé sans input.
- **Saut** : vitesse verticale calculée pour atteindre `jumpHeight` (1.1 m, ~0.67 s en l'air). Possible si le joueur est au sol et après un délai (`jumpCooldown`). Il ne consomme pas d'endurance.
- **Sol** (`IsGrounded` / `TryGetGround`, qui renvoie aussi la normale du sol) : SphereCast sous la capsule (tolérance `groundCheckDistance`), qui ignore les triggers et le layer du joueur. Une surface plus pentue que `maxSlopeAngle` (45°) ne compte pas comme du sol.
- **Murs et pentes raides** : les normales de contact raides sont collectées (`OnCollisionEnter` et `OnCollisionStay`). La composante de la vitesse visée dirigée vers ces surfaces est retirée, donc le joueur glisse le long sans grimper.
- Le joueur s'oriente selon le forward de la caméra quand il bouge (`MoveRotation`).
- Peut être désactivé par l'option de mode `player_movement_disabled`.
- **Jetpack** : voir [[Équipement et jetpack]] (`PlayerMovement.ApplyJetpack`).

Voir aussi : [[Caméra]] · [[Stats du joueur]]
