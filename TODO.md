# TODO

Chaque tâche est préfixée par la feature concernée (voir `CONTENT.md`).

## Bug

- [x] **[Mouvement]** Le joueur s'accroche aux murs et peut les escalader. Causes corrigées :
  - [x] Aucun Physic Material sur le collider du joueur. Un matériau sans friction est maintenant créé dans `Awake` si aucun n'est assigné.
  - [x] `IsGrounded()` : test sur la normale (`maxSlopeAngle`, 45°), tolérance réduite (`groundCheckDistance`, 10 cm), triggers et layer du joueur ignorés.
  - [x] Physique déplacée dans `FixedUpdate` (vitesse visée et accélération plafonnée en m/s²).
  - [x] Téléportations manuelles (dépénétration et `KeepDistanceWithObstacles`) supprimées. La composante de vitesse dirigée vers un mur ou une pente raide est retirée à partir des contacts.
  - [x] Contrôle en l'air limité (`airControl`) et conservation de l'élan sans input.
  - [x] Délai entre deux sauts (`jumpCooldown`), qui empêche le double saut juste après une impulsion.
- [x] **[Mouvement]** Le joueur glisse sur les pentes douces (effet du matériau sans friction). Au sol, la partie de la gravité parallèle à la pente est annulée. La vitesse est contrôlée dans le plan de la pente : arrêt complet sans input, et vitesse pleine en montée comme en descente. Pendant `jumpCooldown` après un saut, le joueur est traité comme en l'air pour ne pas freiner le saut.
- [ ] **[Mouvement]** (À tester) En haut d'une montée, la vitesse le long de la pente peut faire décoller légèrement le joueur. Si c'est gênant, ajouter un « ground snapping ».
- [x] **[Mouvement]** Saccades : interpolation du Rigidbody activée dans `Player.prefab`.
- [x] **[Mouvement]** Suivi de la caméra (`CameraMovement`) déplacé dans `LateUpdate`. La rotation reste dans `Update` pour que les raycasts (`PlayerPointer`, `Builder`) visent avec l'orientation courante. La caméra suit aussi le joueur quand elle est verrouillée (interfaces ouvertes).
- [x] **[Mouvement]** Réglage des valeurs pour un déplacement plus dynamique, sur le modèle des FPS courants : marche 4.5 m/s, sprint 7 m/s, accélération 50 m/s², contrôle en l'air 30 %, saut de 1.1 m, gravité du joueur ×2 (~0.67 s en l'air).
- [ ] **[Mouvement]** (À tester) Ajuster finement le ressenti en jeu : `baseSpeed`, `sprintSpeed`, `acceleration`, `jumpHeight`, `gravityMultiplier`. Pistes : FOV de base (60° vertical actuellement), head bob au sprint.
- [x] **[Mouvement]** Sensibilité de la souris baissée de 0.2 à 0.15 (`CameraMovement.sensibility`, prefab `PlayerCamera`).
- [x] **[Mouvement]** Augmentation du FOV au sprint (`CameraMovement.sprintFovIncrease`, +10°, transition lissée par `fovChangeSpeed`).
- [ ] **[Terrain]** `ReplaceObject` place le pivot à +0.7 m du sol. Les pieds du joueur (demi-hauteur 0.85 m) arrivent donc 15 cm sous la surface. Calculer le décalage à partir du collider.
- [ ] **[Mouvement]** Le nom du marker de saut utilise `Time.fixedDeltaTime`, qui est constant : la clé est toujours la même. Utiliser `Time.time` ou un compteur.
- [ ] **[Stats]** À vérifier : le saut ajoute un marker **positif** sur l'endurance (`jumpEnduranceCost * 4` pendant 0.25 s). Il régénère donc de l'endurance au lieu d'en consommer.
- [ ] **[Stats]** `Stat.Actualize` : quand un marker expire, `RemoveMarker` puis `break` arrêtent la boucle. Les markers suivants ne sont pas appliqués pendant cette frame. Collecter les clés expirées et les supprimer après la boucle.
- [ ] **[Bootstrap]** Le joueur spawne à la position du PlayerManager, et aussi dans le menu (à chaque `sceneLoaded`).
- [ ] **[Bootstrap]** Build Settings : ajouter `0_MainMenu`, qui manque, et vérifier le `firstScene` du Bootstrapper.
- [ ] **[Inventaire]** `DropDraggingSlot` appelle `Drop(..., gameObject)` avec le GameObject du PlayerInventory, qui est sur le PlayerManager. L'objet tombe donc à la position du manager, pas devant le joueur. Utiliser la position et la direction du joueur ou de la caméra.
- [ ] **[Inventaire]** `ItemSlot.Drop` : crash si `Model` est null ou n'a pas de `Pickupable`. Ajouter des vérifications.
- [ ] **[Inventaire]** `ChangeHandedItem` : division par zéro si `scroll.y == 0` (`scroll.y / Mathf.Abs(scroll.y)`). Utiliser `Mathf.Sign`.
- [ ] **[Inventaire]** `ItemCollection.RemoveFromSlot` : la borne `index > MaxSize` devrait être `>=`.
- [ ] **[Inventaire]** `HandedCursorUI` : la vitesse de SmoothDamp est réinitialisée à chaque frame et le smoothTime vaut 0.007, donc le lissage est inefficace.
- [ ] **[Interfaces]** (À vérifier dans un build) Le curseur reste visible après la fermeture d'une interface avec Échap. C'est probablement dû à l'éditeur Unity, qui libère le curseur sur Échap. Un réverrouillage à la frame suivante a été essayé sans succès en éditeur, puis abandonné.
- [ ] **[Interfaces]** À vérifier : l'objet tenu au curseur (`draggingSlot`) à la fermeture de l'inventaire. Il n'est pas remis dans l'inventaire.
- [ ] **[Interaction]** `PointedInterractorInformationUI` ne se met à jour qu'en passant de « rien » à « un objet » (ou l'inverse). Quand on passe directement d'un interactable à un autre, l'ancien texte reste. Le texte n'est pas non plus rafraîchi après un ramassage partiel.
- [ ] **[Interaction]** `Searcher.Search` bascule ouvert/fermé. Interagir avec un autre conteneur pendant qu'un conteneur est ouvert le ferme au lieu d'ouvrir le nouveau.
- [ ] **[Interaction]** Raycast du pointeur : pas de layer mask, et il touche aussi les triggers. `hit.transform` renvoie le Rigidbody parent ou le collider, donc un `IInterractable` posé sur un parent n'est pas toujours trouvé. Utiliser `GetComponentInParent`.
- [ ] **[Interaction]** Flag `Pickupable.identified` : il n'est mis à `true` que pour les objets jetés. Les objets placés dans la scène affichent « Pickup » au lieu de leur nom. À clarifier ou supprimer.
- [x] **[Crafting]** `CheckHasItems` compte maintenant les ingrédients de l'inventaire **et** de la barre d'outils, via `ItemStorage.Count`, surchargé par `PlayerInventory` comme `Add` et `Remove`.
- [ ] **[Crafting]** Si l'inventaire est plein, les résultats du craft qui ne rentrent pas sont perdus (le `rest` est ignoré). Les jeter au sol, ou bloquer le craft s'il n'y a pas assez de place.
- [ ] **[Crafting]** `Crafter.ClearStation()` n'est jamais appelé : la station reste « active » après la fermeture de l'UI. La brancher sur la fermeture de l'interface, comme pour le Searcher.
- [ ] **[Construction]** `BuildVerifier` : le compteur de triggers reste bloqué (invalide) si un collider en contact est détruit ou désactivé, car `OnTriggerExit` n'est pas appelé. Il compte aussi n'importe quel collider ou trigger, sans filtre de layer.
- [ ] **[Construction]** Conflit sur `Use1` entre `Builder` (valider la pose) et `PlayerInventory.Use1` (outils et armes). Utiliser le mode `PlayingMode.Build`, qui existe mais n'est pas utilisé.
- [x] **[Terrain]** `ReplaceObject` calcule l'index de heightmap avec `chunkPosition - objectPosition`, qui est inversé et négatif : la hauteur lue est fausse. Utiliser `objectPosition - chunkPosition` (ou `terrain.SampleHeight`).
- [ ] **[Terrain]** `EnvironmentSpawner.Start` lit le `TerrainData` avant la fin de la génération asynchrone. Les objets sont placés à la hauteur du terrain « modèle ». Le brancher sur `ev_OnTerminateGeneration`.
- [ ] **[Terrain]** `EnvironmentSpawner` utilise le `Random` global : le spawn n'est pas déterministe (il dépend de l'ordre de chargement). Utiliser un `System.Random` seedé par les coordonnées du chunk. `Random.Range(1.80f, 1.20f)` a aussi ses bornes inversées.
- [ ] **[Terrain]** `ChunkManager` récupère le joueur par `FindGameObjectWithTag` dans `Start` : la référence est perdue si le joueur est réinstancié. Passer par `PlayerManager.ev_spawnPlayer`.
- [ ] **[Terrain]** Coutures de LOD entre chunks : appeler `Terrain.SetNeighbors` entre chunks voisins.

## Feature

- [ ] **[Bootstrap]** Système de sauvegarde : reprendre une partie sur la scène sauvegardée (capsule ou monde extérieur).
- [ ] **[Bootstrap]** Réutiliser `0_Capsule` pour l'intérieur de la capsule.
- [ ] **[Mouvement]** Accroupissement.
- [ ] **[Stats]** Ajouter les stats Faim, Soif et Oxygène, avec leurs modifiers associés.
- [ ] **[Stats]** Gérer les conséquences d'une stat à 0 (mort du joueur, dégâts quand faim, soif ou oxygène sont à 0).
- [ ] **[Inventaire]** Vrai glisser-déposer (drag & drop) entre les slots.
- [ ] **[Inventaire]** Séparer un stack (clic droit : prendre la moitié ou une seule unité).
- [ ] **[Inventaire]** Transfert rapide en shift-clic entre l'inventaire, la barre d'outils et un conteneur.
- [ ] **[Inventaire]** Implémenter `Use1` (action principale) : outils, armes, validation d'une construction.
- [ ] **[Inventaire]** (À voir) Revoir l'ordre de remplissage. Actuellement : inventaire d'abord, puis barre d'outils.
- [ ] **[Inventaire]** (À décider) Données propres à chaque exemplaire d'item (durabilité, charges…).
- [ ] **[Interaction]** Réduire la portée d'interaction à **3 m** (10 m actuellement).
- [ ] **[Interaction]** Fermer le conteneur automatiquement quand le joueur s'éloigne.
- [ ] **[Interaction]** Afficher un retour « inventaire plein » quand le ramassage est partiel ou impossible.
- [ ] **[Interaction]** `PickupInfoGroupUI` : regrouper les notifications successives d'un même item. La branche « - » n'est jamais utilisée.
- [x] **[Interaction]** Contour blanc autour de l'objet interactable pointé (`InteractableOutline` + shader `Hidden/InteractableOutline`).
- [ ] **[Interaction]** (À voir) Contour : masquer les parties cachées par un autre objet (test de profondeur dans le masque), ou ajouter une légère animation d'apparition.
- [ ] **[Crafting]** Fermer l'UI de craft quand le joueur s'éloigne de la station.
- [ ] **[Crafting]** UI : indiquer si une recette est réalisable (ingrédients possédés / requis, recette grisée sinon).
- [ ] **[Crafting]** UI : seul `Results[0]` est affiché. Gérer les recettes à plusieurs résultats.
- [ ] **[Crafting]** Remplacer les `Debug.Log` d'échec par des retours visibles par le joueur.
- [ ] **[Construction]** **Catalogue de constructions** : bâtiments, machines, plantes (plantations), objets, meubles, coffres… Prévoir une donnée par construction (ScriptableObject `BuildData` : prefab, catégorie, coûts…) et un menu de sélection.
- [ ] **[Construction]** **Coût en ressources** prélevé dans l'inventaire. Deux options à trancher :
  - coût direct à la pose ;
  - pose d'un **plan**, puis apport progressif des ressources, et construction une fois tout réuni.
- [ ] **[Construction]** **Déconstruction** : chaque construction définit ses ressources de coût **et** les ressources rendues à la destruction (remboursement partiel).
- [ ] **[Construction]** **Rotation** des pièces pendant le placement.
- [ ] **[Construction]** **Liaison des bâtiments** : relier deux dômes proches par un tunnel, avec des points de connexion sur les modules.
- [ ] **[Construction]** **Réseau d'oxygène** : un générateur produit une quantité d'O2. La concentration dépend de la puissance totale des générateurs et du volume de l'ensemble des bâtiments reliés. Lien avec la stat Oxygène du joueur.
- [ ] **[Construction]** **Sauvegarde** des constructions posées et rechargement avec le monde (et les chunks).
- [ ] **[Construction]** Réduire la portée de placement (300 m actuellement).
- [ ] **[Terrain]** **Biomes** : zones avec des matériaux, des textures et un relief différents.
- [ ] **[Terrain]** **Structures** générées dans le monde (épaves, ruines…).
- [ ] **[Terrain]** **Ressources** générées (minerais, plantes, débris…), certaines réapparaissant avec le temps, d'autres non.
- [ ] **[Terrain]** Sauvegarder les modifications du terrain (aplanissements) et ce qu'il y a sur les chunks (constructions, objets ramassés).
- [ ] **[Interfaces]** Menu pause (nouveau mode ou nouvelle interface).

## Optimisation

- [ ] **[Terrain]** Les chunks ne sont jamais détruits, donc la mémoire augmente sans limite en explorant. **Stocker les chunks sur disque** (via le système de persistance) : détruire les chunks éloignés et les recharger depuis le disque au retour dans la zone, modifications comprises.
- [ ] **[Terrain]** `Update` recalcule les chunks nécessaires à chaque frame : ne le faire qu'au changement de chunk du joueur.
- [ ] **[Terrain]** Monde **infini** (à confirmer) : vérifier la précision des flottants loin de l'origine (floating origin).
- [x] **[Inventaire]** `ItemCollectionUI.UpdateItemCollectionUI` réutilise les slots UI existants au lieu de tous les détruire et recréer à chaque `ev_Update`. Il n'en crée ou n'en supprime que pour suivre la taille de la collection.
  - [x] `InventorySlotUI.setSlot` ne se réabonne à `ItemSlot.ev_update` que si le slot change (sinon, doublons de listeners à chaque mise à jour).
  - [x] Un slot `null` garde son UI (affichée vide), donc l'index UI correspond toujours à l'index du slot. C'est ce qu'utilise `HandedCursorUI`.
- [ ] **[Interaction]** `PlayerPointer`, `PlayerInterractor` et `Builder` font un `GetComponent<PlayerContext>()` à chaque accès : le mettre en cache.

## Refonte

- [ ] **[Bootstrap]** Refondre le chargement de scènes pour un code propre, maintenable et évolutif (rôles clairs pour Root, SceneContext, Bootstrapper et les managers).
- [ ] **[Bootstrap]** Revoir ce qui persiste entre deux scènes et comment.
- [ ] **[Bootstrap]** Spawn du joueur : utiliser un point de spawn propre à la scène ou à l'entrée empruntée (`SceneLoader.position` n'est pas utilisé).
- [x] **[Global]** Gestion des abonnements aux events :
  - `ModeManager` et `InterfaceManager` sont des singletons C# statiques. Les MonoBehaviour (PlayerMovement, CameraMovement) s'y abonnent dans leur **constructeur** et ne se désabonnent jamais, donc les listeners d'objets détruits s'accumulent.
  - Les lambdas des inputs (PlayerMovement, CameraMovement, PlayerInventory, ParentUI) ne sont jamais désabonnées.
  - PlayerManager ne se désabonne pas de `SceneManager.sceneLoaded`.
  - `InventorySlotUI` et `ParentedUI`, recréés à chaque mise à jour de l'inventaire, ne retirent pas leurs listeners (`ItemSlot.ev_update`, `ParentUI.ev_updateManager`, `UIManager.ev_updatePlayerContext`).
- [ ] **[UI]** `ItemCollectionUI` et `PlayerStatsUI` modifient le **prefab asset** au runtime (`slotPrefab.GetComponent<...>().parentUI = parentUI`). En éditeur, la modification persiste dans l'asset, avec une référence vers un objet de scène. Assigner `parentUI` sur l'instance après l'`Instantiate`.
- [ ] **[Global]** Les `PlayerInputActions` créés par chaque composant ne sont jamais `Dispose()` (un asset d'input est perdu à chaque respawn du joueur).- [ ] **[Mouvement]** Alternative à envisager : passer sur un `CharacterController` (ou un kinematic controller) plutôt qu'un Rigidbody.
- [ ] **[Stats]** (Futur chantier) Refonte complète du HUD des stats, plus moderne. Exemple : afficher l'endurance seulement pendant qu'elle est consommée, avec autre chose qu'une barre.
- [ ] **[Interaction]** Rendre le système d'interaction extensible à de nouveaux types (véhicules, récolte de plantes, portes, machines…). Par exemple : interaction maintenue ou instantanée, conditions (outil requis), plusieurs actions par objet. Ne plus dépendre de `GetComponent<Xxx>()` côté joueur pour chaque type.
- [ ] **[Inventaire]** Les `Id` d'ItemData sont saisis à la main (risque de doublons). Envisager un identifiant généré ou unique.
- [ ] **[Inventaire]** `TryMerge` n'est appelé qu'avec `Direction.Out` : vérifier si `Direction.In` est utile.
- [ ] **[Construction]** Améliorer l'intégration des constructions dans l'environnement. L'aplanissement actuel n'est pas satisfaisant (creux non remplis, rendu). Solution à trouver.
- [ ] **[Construction]** (À voir) Arrondi de hauteur par pas de 5 m (`PutHeight`) pour aligner les modules. À garder ou à remplacer par un vrai système de snapping entre points de connexion.
- [ ] **[Construction]** Nettoyage : retirer le `Debug.DrawRay` de `Builder.Update`, et utiliser ou supprimer `ObjectPlacement`, qui n'est utilisé nulle part.
