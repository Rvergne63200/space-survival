# TODO

Chaque tâche a un identifiant, suivi de la feature concernée (voir `CONTENT.md`).

Identifiants : `B` = Bug, `F` = Feature, `O` = Optimisation, `R` = Refonte. Ils sont **stables** : une nouvelle tâche prend le numéro suivant de sa catégorie (à la fin de la section), et un numéro n'est jamais réattribué, même si la tâche est supprimée ou cochée.

## Bug

- [x] **B01** **[Mouvement]** Le joueur s'accroche aux murs et peut les escalader. Causes corrigées :
  - [x] Aucun Physic Material sur le collider du joueur. Un matériau sans friction est maintenant créé dans `Awake` si aucun n'est assigné.
  - [x] `IsGrounded()` : test sur la normale (`maxSlopeAngle`, 45°), tolérance réduite (`groundCheckDistance`, 10 cm), triggers et layer du joueur ignorés.
  - [x] Physique déplacée dans `FixedUpdate` (vitesse visée et accélération plafonnée en m/s²).
  - [x] Téléportations manuelles (dépénétration et `KeepDistanceWithObstacles`) supprimées. La composante de vitesse dirigée vers un mur ou une pente raide est retirée à partir des contacts.
  - [x] Contrôle en l'air limité (`airControl`) et conservation de l'élan sans input.
  - [x] Délai entre deux sauts (`jumpCooldown`), qui empêche le double saut juste après une impulsion.
- [x] **B02** **[Mouvement]** Le joueur glisse sur les pentes douces (effet du matériau sans friction). Au sol, la partie de la gravité parallèle à la pente est annulée. La vitesse est contrôlée dans le plan de la pente : arrêt complet sans input, et vitesse pleine en montée comme en descente. Pendant `jumpCooldown` après un saut, le joueur est traité comme en l'air pour ne pas freiner le saut.
- [ ] **B03** **[Mouvement]** (À tester) En haut d'une montée, la vitesse le long de la pente peut faire décoller légèrement le joueur. Si c'est gênant, ajouter un « ground snapping ».
- [x] **B04** **[Mouvement]** Saccades : interpolation du Rigidbody activée dans `Player.prefab`.
- [x] **B05** **[Mouvement]** Suivi de la caméra (`CameraMovement`) déplacé dans `LateUpdate`. La rotation reste dans `Update` pour que les raycasts (`PlayerPointer`, `Builder`) visent avec l'orientation courante. La caméra suit aussi le joueur quand elle est verrouillée (interfaces ouvertes).
- [x] **B06** **[Mouvement]** Réglage des valeurs pour un déplacement plus dynamique, sur le modèle des FPS courants : marche 4.5 m/s, sprint 7 m/s, accélération 50 m/s², contrôle en l'air 30 %, saut de 1.1 m, gravité du joueur ×2 (~0.67 s en l'air).
- [ ] **B07** **[Mouvement]** (À tester) Ajuster finement le ressenti en jeu : `baseSpeed`, `sprintSpeed`, `acceleration`, `jumpHeight`, `gravityMultiplier`. Pistes : FOV de base (60° vertical actuellement), head bob au sprint.
- [x] **B08** **[Mouvement]** Sensibilité de la souris baissée de 0.2 à 0.15 (`CameraMovement.sensibility`, prefab `PlayerCamera`).
- [x] **B09** **[Mouvement]** Augmentation du FOV au sprint (`CameraMovement.sprintFovIncrease`, +10°, transition lissée par `fovChangeSpeed`).
- [ ] **B10** **[Terrain]** `ReplaceObject` place le pivot à +0.7 m du sol. Les pieds du joueur (demi-hauteur 0.85 m) arrivent donc 15 cm sous la surface. Calculer le décalage à partir du collider.
- [ ] **B11** **[Mouvement]** Le nom du marker de saut utilise `Time.fixedDeltaTime`, qui est constant : la clé est toujours la même. Utiliser `Time.time` ou un compteur.
- [ ] **B12** **[Stats]** À vérifier : le saut ajoute un marker **positif** sur l'endurance (`jumpEnduranceCost * 4` pendant 0.25 s). Il régénère donc de l'endurance au lieu d'en consommer.
- [ ] **B13** **[Stats]** `Stat.Actualize` : quand un marker expire, `RemoveMarker` puis `break` arrêtent la boucle. Les markers suivants ne sont pas appliqués pendant cette frame. Collecter les clés expirées et les supprimer après la boucle.
- [ ] **B14** **[Bootstrap]** Le joueur spawne à la position du PlayerManager, et aussi dans le menu (à chaque `sceneLoaded`).
- [ ] **B15** **[Bootstrap]** Build Settings : ajouter `0_MainMenu`, qui manque, et vérifier le `firstScene` du Bootstrapper.
- [ ] **B16** **[Inventaire]** `DropDraggingSlot` appelle `Drop(..., gameObject)` avec le GameObject du PlayerInventory, qui est sur le PlayerManager. L'objet tombe donc à la position du manager, pas devant le joueur. Utiliser la position et la direction du joueur ou de la caméra.
- [ ] **B17** **[Inventaire]** `ItemSlot.Drop` : crash si `Model` est null ou n'a pas de `Pickupable`. Ajouter des vérifications.
- [ ] **B18** **[Inventaire]** `ChangeHandedItem` : division par zéro si `scroll.y == 0` (`scroll.y / Mathf.Abs(scroll.y)`). Utiliser `Mathf.Sign`.
- [ ] **B19** **[Inventaire]** `ItemCollection.RemoveFromSlot` : la borne `index > MaxSize` devrait être `>=`.
- [ ] **B20** **[Inventaire]** `HandedCursorUI` : la vitesse de SmoothDamp est réinitialisée à chaque frame et le smoothTime vaut 0.007, donc le lissage est inefficace.
- [ ] **B21** **[Interfaces]** (À vérifier dans un build) Le curseur reste visible après la fermeture d'une interface avec Échap. C'est probablement dû à l'éditeur Unity, qui libère le curseur sur Échap. Un réverrouillage à la frame suivante a été essayé sans succès en éditeur, puis abandonné.
- [ ] **B22** **[Interfaces]** À vérifier : l'objet tenu au curseur (`draggingSlot`) à la fermeture de l'inventaire. Il n'est pas remis dans l'inventaire.
- [ ] **B23** **[Interaction]** `PointedInterractorInformationUI` ne se met à jour qu'en passant de « rien » à « un objet » (ou l'inverse). Quand on passe directement d'un interactable à un autre, l'ancien texte reste. Le texte n'est pas non plus rafraîchi après un ramassage partiel.
- [ ] **B24** **[Interaction]** `Searcher.Search` bascule ouvert/fermé. Interagir avec un autre conteneur pendant qu'un conteneur est ouvert le ferme au lieu d'ouvrir le nouveau.
- [ ] **B25** **[Interaction]** Raycast du pointeur : pas de layer mask, et il touche aussi les triggers. `hit.transform` renvoie le Rigidbody parent ou le collider, donc un `IInterractable` posé sur un parent n'est pas toujours trouvé. Utiliser `GetComponentInParent`.
- [ ] **B26** **[Interaction]** Flag `Pickupable.identified` : il n'est mis à `true` que pour les objets jetés. Les objets placés dans la scène affichent « Pickup » au lieu de leur nom. À clarifier ou supprimer.
- [x] **B27** **[Crafting]** `CheckHasItems` compte maintenant les ingrédients de l'inventaire **et** de la barre d'outils, via `ItemStorage.Count`, surchargé par `PlayerInventory` comme `Add` et `Remove`.
- [ ] **B28** **[Crafting]** Si l'inventaire est plein, les résultats du craft qui ne rentrent pas sont perdus (le `rest` est ignoré). Les jeter au sol, ou bloquer le craft s'il n'y a pas assez de place.
- [ ] **B29** **[Crafting]** `Crafter.ClearStation()` n'est jamais appelé : la station reste « active » après la fermeture de l'UI. La brancher sur la fermeture de l'interface, comme pour le Searcher.
- [ ] **B30** **[Construction]** `BuildVerifier` : le compteur de triggers reste bloqué (invalide) si un collider en contact est détruit ou désactivé, car `OnTriggerExit` n'est pas appelé. Il compte aussi n'importe quel collider ou trigger, sans filtre de layer.
- [ ] **B31** **[Construction]** Conflit sur `Use1` entre `Builder` (valider la pose) et `PlayerInventory.Use1` (outils et armes). Utiliser le mode `PlayingMode.Build`, qui existe mais n'est pas utilisé.
- [x] **B32** **[Terrain]** `ReplaceObject` calcule l'index de heightmap avec `chunkPosition - objectPosition`, qui est inversé et négatif : la hauteur lue est fausse. Utiliser `objectPosition - chunkPosition` (ou `terrain.SampleHeight`).
- [ ] **B33** **[Terrain]** `EnvironmentSpawner.Start` lit le `TerrainData` avant la fin de la génération asynchrone. Les objets sont placés à la hauteur du terrain « modèle ». Le brancher sur `ev_OnTerminateGeneration`.
- [ ] **B34** **[Terrain]** `EnvironmentSpawner` utilise le `Random` global : le spawn n'est pas déterministe (il dépend de l'ordre de chargement). Utiliser un `System.Random` seedé par les coordonnées du chunk. `Random.Range(1.80f, 1.20f)` a aussi ses bornes inversées.
- [ ] **B35** **[Terrain]** `ChunkManager` récupère le joueur par `FindGameObjectWithTag` dans `Start` : la référence est perdue si le joueur est réinstancié. Passer par `PlayerManager.ev_spawnPlayer`.
- [ ] **B36** **[Terrain]** Coutures de LOD entre chunks : appeler `Terrain.SetNeighbors` entre chunks voisins.

## Feature

- [ ] **F01** **[Bootstrap]** Système de sauvegarde : reprendre une partie sur la scène sauvegardée (capsule ou monde extérieur).
- [ ] **F02** **[Bootstrap]** Réutiliser `0_Capsule` pour l'intérieur de la capsule.
- [ ] **F03** **[Mouvement]** Accroupissement.
- [ ] **F04** **[Stats]** Ajouter les stats Faim, Soif et Oxygène, avec leurs modifiers associés.
- [ ] **F05** **[Stats]** Gérer les conséquences d'une stat à 0 (mort du joueur, dégâts quand faim, soif ou oxygène sont à 0).
- [ ] **F06** **[Inventaire]** Vrai glisser-déposer (drag & drop) entre les slots.
- [ ] **F07** **[Inventaire]** Séparer un stack (clic droit : prendre la moitié ou une seule unité).
- [ ] **F08** **[Inventaire]** Transfert rapide en shift-clic entre l'inventaire, la barre d'outils et un conteneur.
- [ ] **F09** **[Inventaire]** Implémenter `Use1` (action principale) : outils, armes, validation d'une construction.
- [ ] **F10** **[Inventaire]** (À voir) Revoir l'ordre de remplissage. Actuellement : inventaire d'abord, puis barre d'outils.
- [ ] **F11** **[Inventaire]** (À décider) Données propres à chaque exemplaire d'item (durabilité, charges…).
- [ ] **F12** **[Interaction]** Réduire la portée d'interaction à **3 m** (10 m actuellement).
- [ ] **F13** **[Interaction]** Fermer le conteneur automatiquement quand le joueur s'éloigne.
- [ ] **F14** **[Interaction]** Afficher un retour « inventaire plein » quand le ramassage est partiel ou impossible.
- [ ] **F15** **[Interaction]** `PickupInfoGroupUI` : regrouper les notifications successives d'un même item. La branche « - » n'est jamais utilisée.
- [x] **F16** **[Interaction]** Contour blanc autour de l'objet interactable pointé (`InteractableOutline` + shader `Hidden/InteractableOutline`).
- [ ] **F17** **[Interaction]** (À voir) Contour : masquer les parties cachées par un autre objet (test de profondeur dans le masque), ou ajouter une légère animation d'apparition.
- [ ] **F18** **[Crafting]** Fermer l'UI de craft quand le joueur s'éloigne de la station.
- [ ] **F19** **[Crafting]** UI : indiquer si une recette est réalisable (ingrédients possédés / requis, recette grisée sinon).
- [ ] **F20** **[Crafting]** UI : seul `Results[0]` est affiché. Gérer les recettes à plusieurs résultats.
- [ ] **F21** **[Crafting]** Remplacer les `Debug.Log` d'échec par des retours visibles par le joueur.
- [ ] **F22** **[Construction]** **Catalogue de constructions** : bâtiments, machines, plantes (plantations), objets, meubles, coffres… Prévoir une donnée par construction (ScriptableObject `BuildData` : prefab, catégorie, coûts…) et un menu de sélection.
- [ ] **F23** **[Construction]** **Coût en ressources** prélevé dans l'inventaire. Deux options à trancher :
  - coût direct à la pose ;
  - pose d'un **plan**, puis apport progressif des ressources, et construction une fois tout réuni.
- [ ] **F24** **[Construction]** **Déconstruction** : chaque construction définit ses ressources de coût **et** les ressources rendues à la destruction (remboursement partiel).
- [ ] **F25** **[Construction]** **Rotation** des pièces pendant le placement.
- [ ] **F26** **[Construction]** **Liaison des bâtiments** : relier deux dômes proches par un tunnel, avec des points de connexion sur les modules.
- [ ] **F27** **[Construction]** **Réseau d'oxygène** : un générateur produit une quantité d'O2. La concentration dépend de la puissance totale des générateurs et du volume de l'ensemble des bâtiments reliés. Lien avec la stat Oxygène du joueur.
- [ ] **F28** **[Construction]** **Sauvegarde** des constructions posées et rechargement avec le monde (et les chunks).
- [ ] **F29** **[Construction]** Réduire la portée de placement (300 m actuellement).
- [ ] **F30** **[Terrain]** **Biomes** : zones avec des matériaux, des textures et un relief différents.
- [ ] **F31** **[Terrain]** **Structures** générées dans le monde (épaves, ruines…).
- [ ] **F32** **[Terrain]** **Ressources** générées (minerais, plantes, débris…), certaines réapparaissant avec le temps, d'autres non.
- [ ] **F33** **[Terrain]** Sauvegarder les modifications du terrain (aplanissements) et ce qu'il y a sur les chunks (constructions, objets ramassés).
- [ ] **F34** **[Interfaces]** Menu pause (nouveau mode ou nouvelle interface).

## Optimisation

- [ ] **O01** **[Terrain]** Les chunks ne sont jamais détruits, donc la mémoire augmente sans limite en explorant. **Stocker les chunks sur disque** (via le système de persistance) : détruire les chunks éloignés et les recharger depuis le disque au retour dans la zone, modifications comprises.
- [ ] **O02** **[Terrain]** `Update` recalcule les chunks nécessaires à chaque frame : ne le faire qu'au changement de chunk du joueur.
- [ ] **O03** **[Terrain]** Monde **infini** (à confirmer) : vérifier la précision des flottants loin de l'origine (floating origin).
- [x] **O04** **[Inventaire]** `ItemCollectionUI.UpdateItemCollectionUI` réutilise les slots UI existants au lieu de tous les détruire et recréer à chaque `ev_Update`. Il n'en crée ou n'en supprime que pour suivre la taille de la collection.
  - [x] `InventorySlotUI.setSlot` ne se réabonne à `ItemSlot.ev_update` que si le slot change (sinon, doublons de listeners à chaque mise à jour).
  - [x] Un slot `null` garde son UI (affichée vide), donc l'index UI correspond toujours à l'index du slot. C'est ce qu'utilise `HandedCursorUI`.
- [ ] **O05** **[Interaction]** `PlayerPointer`, `PlayerInterractor` et `Builder` font un `GetComponent<PlayerContext>()` à chaque accès : le mettre en cache.

## Refonte

- [ ] **R01** **[Bootstrap]** Refondre le chargement de scènes pour un code propre, maintenable et évolutif (rôles clairs pour Root, SceneContext, Bootstrapper et les managers).
- [ ] **R02** **[Bootstrap]** Revoir ce qui persiste entre deux scènes et comment.
- [ ] **R03** **[Bootstrap]** Spawn du joueur : utiliser un point de spawn propre à la scène ou à l'entrée empruntée (`SceneLoader.position` n'est pas utilisé).
- [x] **R04** **[Global]** Gestion des abonnements aux events :
  - `ModeManager` et `InterfaceManager` sont des singletons C# statiques. Les MonoBehaviour (PlayerMovement, CameraMovement) s'y abonnent dans leur **constructeur** et ne se désabonnent jamais, donc les listeners d'objets détruits s'accumulent.
  - Les lambdas des inputs (PlayerMovement, CameraMovement, PlayerInventory, ParentUI) ne sont jamais désabonnées.
  - PlayerManager ne se désabonne pas de `SceneManager.sceneLoaded`.
  - `InventorySlotUI` et `ParentedUI`, recréés à chaque mise à jour de l'inventaire, ne retirent pas leurs listeners (`ItemSlot.ev_update`, `ParentUI.ev_updateManager`, `UIManager.ev_updatePlayerContext`).
- [ ] **R05** **[UI]** `ItemCollectionUI` et `PlayerStatsUI` modifient le **prefab asset** au runtime (`slotPrefab.GetComponent<...>().parentUI = parentUI`). En éditeur, la modification persiste dans l'asset, avec une référence vers un objet de scène. Assigner `parentUI` sur l'instance après l'`Instantiate`.
- [ ] **R06** **[Global]** Les `PlayerInputActions` créés par chaque composant ne sont jamais `Dispose()` (un asset d'input est perdu à chaque respawn du joueur).
- [ ] **R07** **[Mouvement]** Alternative à envisager : passer sur un `CharacterController` (ou un kinematic controller) plutôt qu'un Rigidbody.
- [ ] **R08** **[Stats]** (Futur chantier) Refonte complète du HUD des stats, plus moderne. Exemple : afficher l'endurance seulement pendant qu'elle est consommée, avec autre chose qu'une barre.
- [ ] **R09** **[Interaction]** Rendre le système d'interaction extensible à de nouveaux types (véhicules, récolte de plantes, portes, machines…). Par exemple : interaction maintenue ou instantanée, conditions (outil requis), plusieurs actions par objet. Ne plus dépendre de `GetComponent<Xxx>()` côté joueur pour chaque type.
- [ ] **R10** **[Inventaire]** Les `Id` d'ItemData sont saisis à la main (risque de doublons). Envisager un identifiant généré ou unique.
- [ ] **R11** **[Inventaire]** `TryMerge` n'est appelé qu'avec `Direction.Out` : vérifier si `Direction.In` est utile.
- [ ] **R12** **[Construction]** Améliorer l'intégration des constructions dans l'environnement. L'aplanissement actuel n'est pas satisfaisant (creux non remplis, rendu). Solution à trouver.
- [ ] **R13** **[Construction]** (À voir) Arrondi de hauteur par pas de 5 m (`PutHeight`) pour aligner les modules. À garder ou à remplacer par un vrai système de snapping entre points de connexion.
- [ ] **R14** **[Construction]** Nettoyage : retirer le `Debug.DrawRay` de `Builder.Update`, et utiliser ou supprimer `ObjectPlacement`, qui n'est utilisé nulle part.
