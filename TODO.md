# TODO

Chaque tâche est préfixée par la feature concernée (voir `CONTENT.md`).

## Bug

- [ ] **[Mouvement]** Le joueur s'accroche aux murs et peut les escalader. Causes probables :
  - [ ] Aucun Physic Material sur le collider du joueur. La friction par défaut (0.6) le retient contre un mur pendant qu'il pousse dedans en l'air. Mettre un matériau à friction 0 (`Friction Combine = Minimum`).
  - [ ] `IsGrounded()` : le SphereCast n'a ni layer mask ni test sur la normale de la surface. Il détecte les bords et les murs, ce qui permet de ressauter contre une paroi. Tester la normale (pente maximale) et utiliser un layer mask.
  - [ ] La physique est faite dans `Update` (`AddForce` à chaque frame, dépendant du framerate) : la déplacer dans `FixedUpdate`.
  - [ ] Les téléportations manuelles (`transform.position +=` dans la dépénétration et dans `KeepDistanceWithObstacles`) se battent avec le solveur physique et provoquent les glitchs. Les supprimer une fois les points ci-dessus corrigés.
  - [ ] Limiter le contrôle en l'air (pas de vitesse horizontale forcée à 100 %).
- [ ] **[Mouvement]** Saccades : activer l'interpolation du Rigidbody et déplacer la caméra dans `LateUpdate`.
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
- [ ] **[Interfaces]** À vérifier : l'objet tenu au curseur (`draggingSlot`) à la fermeture de l'inventaire. Il n'est pas remis dans l'inventaire.
- [ ] **[Interaction]** `PointedInterractorInformationUI` ne se met à jour qu'en passant de « rien » à « un objet » (ou l'inverse). Quand on passe directement d'un interactable à un autre, l'ancien texte reste. Le texte n'est pas non plus rafraîchi après un ramassage partiel.
- [ ] **[Interaction]** `Searcher.Search` bascule ouvert/fermé. Interagir avec un autre conteneur pendant qu'un conteneur est ouvert le ferme au lieu d'ouvrir le nouveau.
- [ ] **[Interaction]** Raycast du pointeur : pas de layer mask, et il touche aussi les triggers. `hit.transform` renvoie le Rigidbody parent ou le collider, donc un `IInterractable` posé sur un parent n'est pas toujours trouvé. Utiliser `GetComponentInParent`.
- [ ] **[Interaction]** Flag `Pickupable.identified` : il n'est mis à `true` que pour les objets jetés. Les objets placés dans la scène affichent « Pickup » au lieu de leur nom. À clarifier ou supprimer.
- [ ] **[Crafting]** `CheckHasItems` ne compte que `inventory.Content`, pas la barre d'outils, alors que `Remove` retire des deux. On ne peut pas crafter si une partie des ingrédients est dans la barre d'outils.
- [ ] **[Crafting]** Si l'inventaire est plein, les résultats du craft qui ne rentrent pas sont perdus (le `rest` est ignoré). Les jeter au sol, ou bloquer le craft s'il n'y a pas assez de place.
- [ ] **[Crafting]** `Crafter.ClearStation()` n'est jamais appelé : la station reste « active » après la fermeture de l'UI. La brancher sur la fermeture de l'interface, comme pour le Searcher.
- [ ] **[Construction]** `BuildVerifier` : le compteur de triggers reste bloqué (invalide) si un collider en contact est détruit ou désactivé, car `OnTriggerExit` n'est pas appelé. Il compte aussi n'importe quel collider ou trigger, sans filtre de layer.
- [ ] **[Construction]** Conflit sur `Use1` entre `Builder` (valider la pose) et `PlayerInventory.Use1` (outils et armes). Utiliser le mode `PlayingMode.Build`, qui existe mais n'est pas utilisé.
- [ ] **[Terrain]** `ReplaceObject` calcule l'index de heightmap avec `chunkPosition - objectPosition`, qui est inversé et négatif : la hauteur lue est fausse. Utiliser `objectPosition - chunkPosition` (ou `terrain.SampleHeight`).
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
- [ ] **[Inventaire]** `ItemCollectionUI.UpdateItemCollectionUI` détruit et recrée tous les slots UI à chaque `ev_Update`. Réutiliser les slots existants.
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
