---
tags: [jeu-colonie, pipeline-3d, unity, code]
---

# Scripts d'animation C# (runtime)

- Emplacement : **`Assets/Scripts/Behaviours/Animations/<Model>Animator.cs`**, avec un composant par modèle.
  - Existants : `CraftingStationAnimator`, `AirlockAnimator`, `RelayAntennaAnimator`, `SolarPanelAnimator`, `WindTurbineAnimator`, `CoalGeneratorAnimator`, `LandingCapsuleAnimator`, `DrillAnimator`, `StorageAnimator` (commun à Crate_Small, Crate_Large et Cabinet), `ControlPanelAnimator`, `WallScreenAnimator` (modèle statique : pas d'Animator, il pilote seulement la dalle `_Glow` et le voyant).
- **Un script par modèle**, sauf si la logique est strictement identique : un script commun détecte alors ses clips dans le controller (`*_Open` / `*_Close` pour `StorageAnimator`), au lieu de coder les noms en dur (demande de l'utilisateur).
- **L'Animator d'une instance doit recevoir son `.controller` à la main** (une instance posée depuis le FBX n'en a pas). Sinon, aucun clip ne joue et `Editor.log` affiche « Animator does not have an AnimatorController ». L'utilisateur ne veut pas que les scripts le chargent automatiquement.
- **État d'une translation interruptible** : mémoriser la **cible** dès le lancement du clip (`lowered = true` au début de `Drill_Lower`), pas à la fin. Sinon, un arrêt demandé en pleine descente croit le chariot encore en haut et ne le remonte pas.
- **Clips aller / retour symétriques** (même durée, même courbe) : si une commande inverse arrive en plein mouvement, repartir de la position atteinte avec `animator.Play(clipInverse, layer, 1 - normalizedTime)` (voir `LandingCapsuleAnimator`).
- Commentaires et `<summary>` **en français**, `[RequireComponent(typeof(Animator))]`. Le composant se place sur la **racine de l'instance**, à côté de l'Animator.
- **API publique claire**, avec des verbes : `Deploy()`, `Fold()`, `Toggle()`, `StartRotation()`, `StopRotation()`, `OpenOuter()`, `RequestEnterFromOutside()`… L'état se lit par des propriétés (`IsDeployed`, `IsBusy`, `Pressurized`…). Le gameplay n'appelle jamais les clips directement.
- **Lancement des clips :**
  - `animator.CrossFadeInFixedTime("Clip", 0.1f, layer)` pour lancer ;
  - `animator.Play("Clip", layer, 1f)` dans `Awake` pour poser l'état initial sans animation ;
  - attendre la fin avec `WaitForSeconds(Length("Clip"))`, où la durée est lue dans `runtimeAnimatorController.animationClips` ;
  - **séquences** en coroutines, avec une seule coroutine active (`Run()` arrête la précédente) ;
  - les commandes peuvent être **mises en file**, comme dans l'Airlock.
- **Boucles :** vitesse via le paramètre `<Couche>Speed`, avec une montée et une descente progressives (`MoveTowards`) dans `Update`.
- **⚠ Piège de l'Animator :** un Animator réécrit **à chaque frame** tous les transforms que ses clips animent. Avec Write Defaults désactivé, il garde la dernière valeur jouée. Donc :
  - un transform piloté **par script** (suivi du soleil, orientation au vent, alignement du rotor) doit garder **son propre angle** dans un champ (`yawAngle`, `nacelleAngle`, `rotorAngle`) ;
  - le script l'**impose de façon absolue dans `LateUpdate`** (`t.localRotation = rest * Quaternion.AngleAxis(angle, axeLocal)`) ;
  - ne jamais faire `t.Rotate(...)` en incrémental sur un transform que l'Animator touche ;
  - pour passer d'un clip de boucle au script, mesurer l'angle atteint (`MeasureYaw()` / `MeasureRotor()`) ;
  - pour passer du script au clip, `Play("Rotate_Loop", layer, Mathf.Repeat(angle/360, 1))` repart de l'angle courant.
- **Axes dans les scripts** : les enfants gardent les axes Blender ([[12 - Import Unity]]).
  - Axe vertical d'une pièce = `Vector3.forward` (Z local) ; avant = `Vector3.down` (-Y local) ; rotor = `Vector3.up` (Y local).
  - Calcule les caps avec `Vector3.SignedAngle` après `ProjectOnPlane` sur l'axe vertical monde du parent (voir `HeadingFor` / `Heading`).
- **Trouver les pièces** par nom dans `GetComponentsInChildren<Transform>(true)`, mémoriser la `localRotation` de repos dans `Awake`, et logger un warning si une pièce manque.
- **Voyants pilotés** (matériau `_Status`) : instancier le matériau du renderer et changer `_Color` / `_EmissionColor`. Le code couleur des voyants est fixé par la DA : voir [[08 - Technologie humaine]].
- **Sas** : la règle « jamais les deux portes ouvertes » et la pression requise pour chaque porte sont gérées dans `AirlockAnimator` (ouverture extérieure si dépressurisé, intérieure si pressurisé, cycle portes fermées).
- **Pièces à longueur variable** (cheminée du générateur, qui doit traverser un dôme) :
  - découpage : un socle fixe dans `Body`, un **fût** séparé (`Chimney_Shaft`, origine en bas, mesh de longueur connue le long de +Z, sans détail rond qui se déformerait à l'étirement), une **tête** séparée (`Chimney_Head`) **enfant de la racine, pas du fût** (sinon elle hérite de l'échelle), et un **collier** séparé (`Chimney_Collar`) placé ou masqué par script ;
  - le script étire `localScale.z` du fût (Z local = vertical) et place la tête et le collier avec `upPerMeter = (tête.localPosition - fût.localPosition) / longueurParDéfaut`, mesuré dans `Awake` : aucune hypothèse sur les axes ni les unités ;
  - ces transforms ne doivent être animés par **aucun** clip, sinon l'Animator les réécrirait.
- **Collier de traversée orientable** (cheminée qui sort d'un dôme) :
  - c'est un **disque graphite plein** (16 facettes, rayon = rayon du fût + 10 cm, chanfrein simple), **sans vis ni manchon**. Le fût le traverse visuellement, donc il n'y a pas de trou quand il pivote, et rien ne se déforme à l'étirement ;
  - le script de placement calcule la rotation (normale de la coque du dôme) et la passe à `SetCollarRotation(Quaternion)`, avec normale = `rotation * Vector3.up` ;
  - l'animator aligne le Z local du disque sur cette normale et son X local sur la ligne de plus grande pente, puis l'étire en ellipse : rayon `r / cos θ + débord` dans le sens de la pente, `r + débord` en travers. Il n'y a pas de limite d'inclinaison, seulement un garde-fou à 89° contre la division par zéro.
- **Points d'ancrage d'effets** : un empty enfant (ex. `Smoke` en haut de la cheminée, `DrillDust` au point de forage), Z local = haut, pour y accrocher un ParticleSystem dans Unity.
- **Écran holographique** (préféré à un écran `_Glow` quand l'utilisateur veut un rendu futuriste) : un projecteur métal et néon, un objet `Hologram` (cadre et faisceau en matériau `_Hologram`), et un quad enfant `Hologram_Screen` aux UV 0..1 pour une future interface. Recette : `build_hologram()` dans `dr_build.py` ou `build_holo()` dans `cs_v5.py`. Le script le masque quand le modèle n'est pas alimenté, ou bien l'échelle de `Hologram` est animée dans les clips d'allumage (0,001 éteint, 1 allumé, origine sur la lentille : il se déploie depuis le projecteur ; voir `cp_build.py`).
- **Vérins hydrauliques** : tube fixe **ouvert** (anneau de révolution, alésage 5 cm) et tige mobile de rayon inférieur, attachée à la pièce qui se déplace. La tige coulisse sans jamais toucher le tube, ce qui garde le test BVH propre. Les colliers du tube ont leur alésage dans l'épaisseur de la paroi (6,2 cm), jamais dans le plan de l'alésage du tube.
- **Le soleil** : `SolarPanelAnimator.sun` = la Directional Light. Le panneau suit `-sun.transform.forward`, **seulement une fois déployé**.
- Tu ne peux pas tester dans Unity : dis-le, et vérifie au moins la compilation dans `Editor.log`.

[[00 - Index Pipeline 3D|Index]] · ← [[14 - Animator Unity]] · [[16 - Erreurs à ne pas refaire]] →
