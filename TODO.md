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
- [x] **B11** **[Mouvement]** Le nom du marker de saut utilisait `Time.fixedDeltaTime`, qui est constant, donc un nouveau saut remplaçait le marker du précédent. La clé utilise maintenant un compteur de sauts (`jumpCount`).
- [x] **B12** **[Stats]** Le saut ajoutait un marker **positif** sur l'endurance, qui en régénérait au lieu d'en consommer. Le marker est maintenant négatif (`-jumpEnduranceCost * 4` pendant 0.25 s), ce qui consomme `jumpEnduranceCost` (5) par saut.
- [x] **B13** **[Stats]** `Stat.Actualize` : quand un marker expirait, `RemoveMarker` puis `break` arrêtaient la boucle, et les markers suivants n'étaient pas appliqués pendant cette frame. Les clés expirées sont maintenant collectées (`expiredMarkers`) et supprimées après la boucle.
- [ ] **B14** **[Bootstrap]** Le joueur spawne à la position du PlayerManager, et aussi dans le menu (à chaque `sceneLoaded`).
- [ ] **B15** **[Bootstrap]** Build Settings : ajouter `0_MainMenu`, qui manque, et vérifier le `firstScene` du Bootstrapper.
- [x] **B16** **[Inventaire]** `DropDraggingSlot` jette maintenant l'objet devant le joueur (`PlayerManager.Instance.Player`), et non plus à la position du PlayerManager, qui porte le PlayerInventory.
- [x] **B17** **[Inventaire]** `ItemSlot.Drop` ne plante plus si `Model` est null ou n'a pas de `Pickupable`. Le spawn est centralisé dans `Pickupable.Spawn`, qui vérifie le modèle, et le slot n'est vidé que si l'objet a bien été créé. Un Rigidbody déjà présent sur le modèle est réutilisé.
- [x] **B18** **[Inventaire]** `ChangeHandedItem` : division par zéro si `scroll.y == 0`. Un scroll sans composante verticale est maintenant ignoré, sinon le slot change de `Mathf.Sign(scroll.y)`.
- [x] **B19** **[Inventaire]** `ItemCollection.RemoveFromSlot` : la borne est maintenant `index >= MaxSize`.
- [x] **B20** **[Inventaire]** `HandedCursorUI` : la vitesse de SmoothDamp est maintenant conservée entre les frames, et le temps de lissage est réglable (`smoothTime`, 0.05 s au lieu de 0.007). Le calcul utilise le temps non mis à l'échelle, pour fonctionner aussi en pause. Pas de `GetChild` hors limites tant que les slots ne sont pas créés.
- [ ] **B21** **[Interfaces]** (À vérifier dans un build) Le curseur reste visible après la fermeture d'une interface avec Échap. C'est probablement dû à l'éditeur Unity, qui libère le curseur sur Échap. Un réverrouillage à la frame suivante a été essayé sans succès en éditeur, puis abandonné.
- [x] **B22** **[Interfaces]** L'objet tenu au curseur (`draggingSlot`) restait affiché après la fermeture de l'inventaire. Il est maintenant jeté au sol quand la dernière interface se ferme (fin du mode `Interface`, écoutée via `ModeManager.ev_OnUpdateOptions`).
- [x] **B23** **[Interaction]** `PointedInterractorInformationUI` ne se mettait à jour qu'en passant de « rien » à « un objet » (ou l'inverse). Le texte et l'icône sont maintenant rafraîchis à chaque changement d'objet pointé, y compris d'un interactable à un autre, et à chaque frame tant qu'un objet est pointé (après un ramassage partiel, par exemple).
- [x] **B24** **[Interaction]** `Searcher.Search` basculait ouvert/fermé, quel que soit le conteneur. Fouiller le conteneur déjà ouvert le ferme toujours, mais fouiller un autre conteneur l'ouvre à la place.
- [x] **B25** **[Interaction]** Raycast du pointeur (`PlayerPointer`) : il ignore maintenant le layer du joueur et les triggers. L'objet pointé est celui qui porte l'`IInterractable`, trouvé avec `GetComponentInParent` depuis le collider touché (collider sur un enfant de l'interactable).
- [ ] **B26** **[Interaction]** Flag `Pickupable.identified` : il n'est mis à `true` que pour les objets jetés. Les objets placés dans la scène affichent « Pickup » au lieu de leur nom. À clarifier ou supprimer.
- [x] **B27** **[Crafting]** `CheckHasItems` compte maintenant les ingrédients de l'inventaire **et** de la barre d'outils, via `ItemStorage.Count`, surchargé par `PlayerInventory` comme `Add` et `Remove`.
- [x] **B28** **[Crafting]** Les résultats du craft qui ne rentrent pas dans l'inventaire plein sont jetés au sol devant le joueur (`Pickupable.Spawn`), au lieu d'être perdus.
- [x] **B29** **[Crafting]** `Crafter.ClearStation()` n'était jamais appelé, donc la station restait « active » après la fermeture de l'UI. `CraftingStationUI.OnDisable` l'appelle maintenant à la fermeture de l'interface (Échap compris), comme pour le Searcher.
- [ ] **B30** **[Construction]** `BuildVerifier` : le compteur de triggers reste bloqué (invalide) si un collider en contact est détruit ou désactivé, car `OnTriggerExit` n'est pas appelé. Il compte aussi n'importe quel collider ou trigger, sans filtre de layer.
- [ ] **B31** **[Construction]** Conflit sur `Use1` entre `Builder` (valider la pose) et `PlayerInventory.Use1` (outils et armes). Utiliser le mode `PlayingMode.Build`, qui existe mais n'est pas utilisé.
- [x] **B32** **[Terrain]** `ReplaceObject` calcule l'index de heightmap avec `chunkPosition - objectPosition`, qui est inversé et négatif : la hauteur lue est fausse. Utiliser `objectPosition - chunkPosition` (ou `terrain.SampleHeight`).
- [x] **B33** **[Terrain]** `EnvironmentSpawner.Start` spawnait des objets avant la fin de la génération asynchrone, à la hauteur du terrain « modèle ». `Spawn` est déjà appelé par `ev_OnTerminateGeneration` (prefab `Chunk`), donc le spawn avait lieu deux fois. Le `Start` est supprimé.
- [x] **B34** **[Terrain]** `EnvironmentSpawner` utilise maintenant un `System.Random` seedé par la seed du monde (`ChunkManager.Seed`) et la position du chunk : un chunk a toujours les mêmes objets, quel que soit l'ordre de chargement. Rotation aléatoire uniforme, et échelle entre 1.2 et 1.8 (× 0.1).
- [x] **B35** **[Terrain]** `ChunkManager` récupère le joueur via `PlayerManager` (`Player` au démarrage, puis `ev_spawnPlayer` à chaque respawn) au lieu de `FindGameObjectWithTag`, et met à jour `ToReplace`. Le tag reste utilisé seulement si la scène est lancée sans le Bootstrap.
- [x] **B36** **[Terrain]** Traits noirs entre chunks vus de loin (fissures de LOD). `ChunkManager.LateUpdate` vérifie à chaque frame les voisins de chaque chunk chargé et n'appelle `Terrain.SetNeighbors` que s'ils sont faux. L'auto-connexion Unity est désactivée (prefab `Chunk` et code), et « Draw Instanced » est activé sur le prefab. C'est la combinaison validée en jeu lors des essais précédents.
- [x] **B37** **[Inventaire]** Slots vides « bloqués » : la sérialisation Unity crée un `Item` avec un `Data` null dans les slots vides (`draggingSlot`, slots des collections), et `ItemSlot.Add` refusait alors tout item. `IsEmpty` tient maintenant compte du `Data` null, et `Add` et `IsFull` s'appuient dessus. C'est ce qui bloquait le dépôt au clic droit et faisait perdre la moitié au split.
- [x] **B38** **[Inventaire]** Un clic droit dans une interface déclenchait aussi `Use2` (bouton droit), qui consommait l'item en main. `Use1` et `Use2` sont maintenant ignorés en mode `Interface` (option `PlayerInventory.OPTION_USE_DISABLED`).
- [x] **B39** **[Terrain]** Textures décalées entre deux chunks : Unity positionne les textures depuis le coin de chaque terrain. Le prefab `Chunk` utilise maintenant le matériau `Materials/Terrain/TerrainWorldUV` (shader `Terrain/World UV Standard`, copie du shader de terrain Standard d'Unity) : les coordonnées de texture viennent de la position monde du pixel, donc les textures restent fixes dans le monde, quelle que soit la taille des calques ou la position des chunks. `TerrainGenerator` transmet la taille des chunks au shader (`_WorldUVTerrainSize`, global).

## Feature

- [ ] **F01** **[Bootstrap]** Système de sauvegarde : reprendre une partie sur la scène sauvegardée (capsule ou monde extérieur).
- [ ] **F02** **[Bootstrap]** Réutiliser `0_Capsule` pour l'intérieur de la capsule.
- [ ] **F03** **[Mouvement]** Accroupissement.
- [ ] **F04** **[Stats]** Ajouter les stats Faim, Soif et Oxygène, avec leurs modifiers associés.
- [ ] **F05** **[Stats]** Gérer les conséquences d'une stat à 0 (mort du joueur, dégâts quand faim, soif ou oxygène sont à 0).
- [ ] **F06** **[Inventaire]** Vrai glisser-déposer (drag & drop) entre les slots.
- [x] **F07** **[Inventaire]** Clic droit (`PlayerInventory.SplitOrPlaceOne`) : avec un slot en main, dépose une unité sur le slot visé (vide ou même item). Main vide : prend la moitié du slot visé (arrondie au supérieur).
- [x] **F08** **[Inventaire]** Shift-clic (`PlayerInventory.QuickTransfer`) : transfert du slot entier. Avec un conteneur fouillé : conteneur → barre d'outils (puis inventaire si elle est pleine), barre d'outils → conteneur, inventaire → conteneur. Sans conteneur : inventaire ↔ barre d'outils. Ce qui ne rentre pas reste dans le slot d'origine.
  - Le split ne retire du slot que ce que le curseur a réellement pris.
  - Les clics sont gérés par `ItemSlotUI.OnPointerClick` (`IPointerClickHandler`). L'appel `onClick` du Button a été retiré du prefab `InventorySlot`, mais le Button reste pour les couleurs de survol.
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
