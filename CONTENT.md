# Space Survival : features implémentées

## 1. Bootstrap & chargement de scènes

### Scènes
- `Bootstrap` : contient les objets persistants (managers).
- `0_MainMenu` : menu principal, point d'entrée du joueur.
- `0_Capsule` : ancienne capsule, plus utilisée (servira plus tard pour l'intérieur de la capsule).
- `0_World` : monde extérieur, qui contient la nouvelle capsule.
- `1_TestScene` : scène de test.

Flux prévu : lancement → Menu → Nouvelle partie → Capsule ⇄ Monde extérieur.

### Fonctionnement actuel
- **Root** : singleton (flag static) marqué `DontDestroyOnLoad`. C'est la racine des objets persistants de la scène Bootstrap.
- **SceneContext** : à placer dans chaque scène de jeu. Si aucun `Root` n'existe, il charge `Bootstrap` en additif, ce qui permet de lancer le jeu depuis n'importe quelle scène dans l'éditeur.
- **Bootstrapper** : si seule la scène Bootstrap est chargée, il charge `firstScene` (en mode Single).
- **SceneLoadingManager** : appelle `DynamicGI.UpdateEnvironment()` à chaque chargement de scène.
- **PlayerManager** (singleton) : garde l'inventaire et les stats. À chaque `sceneLoaded`, il détruit le joueur puis le réinstancie depuis le prefab, à la position du manager lui-même. Il notifie ensuite l'UI et la caméra (`ev_spawnPlayer`, `ev_updatePlayerContext`).
- **CameraManager** / **UIManager** (singletons) : instancient leur prefab une seule fois, dans leur Awake. UIManager branche les events du joueur (Pickuper, Searcher, Crafter) sur l'UI à chaque spawn.
- **SceneLoader** : `LoadScene()` charge `sceneName` en mode Single. Le champ `position` n'est pas utilisé.
- **TriggerSceneLoader** : appelle `LoadScene()` quand un objet tagué `Player` entre dans le trigger.

## 2. Déplacement joueur & caméra

Vue à la première personne uniquement.

### PlayerMovement
- Joueur physique : Rigidbody de 75 kg avec rotations bloquées et **interpolation**, et collider capsule (rayon 0.5, hauteur 2, scale 0.85). Un matériau physique **sans friction** est créé dans `Awake` si aucun n'est assigné. Les entrées passent par le New Input System (`PlayerInputActions.Movement`).
- Physique dans `FixedUpdate`. Le déplacement est relatif à la caméra (plan horizontal). La vitesse tend vers la vitesse visée avec une accélération plafonnée (`acceleration`, en m/s²).
- **Pentes** : au sol, la vitesse est contrôlée dans le plan de la pente (même vitesse en montée et en descente). La composante de la gravité parallèle à la pente est annulée, donc le joueur reste immobile sur une pente sans input. En l'air, seule la vitesse horizontale est contrôlée.
- **Marche** (`baseSpeed`, 4.5 m/s) et **sprint** (`sprintSpeed`, 7 m/s), avec une accélération de 50 m/s². Le sprint consomme de l'endurance et s'arrête quand elle est vide.
- **Gravité renforcée** pour le joueur seulement (`gravityMultiplier`, ×2), pour un saut moins flottant.
- **En l'air** : contrôle réduit (`airControl`, 30 %), et l'élan est conservé sans input.
- **Saut** : vitesse verticale calculée pour atteindre `jumpHeight` (1.1 m, ~0.67 s en l'air). Possible si le joueur est au sol, s'il a assez d'endurance et après un délai (`jumpCooldown`). Il consomme `jumpEnduranceCost` (5) d'endurance, via un marker négatif réparti sur 0.25 s.
- **Sol** (`IsGrounded` / `TryGetGround`, qui renvoie aussi la normale du sol) : SphereCast sous la capsule (tolérance `groundCheckDistance`), qui ignore les triggers et le layer du joueur. Une surface plus pentue que `maxSlopeAngle` (45°) ne compte pas comme du sol.
- **Murs et pentes raides** : les normales de contact raides sont collectées (`OnCollisionEnter` et `OnCollisionStay`). La composante de la vitesse visée dirigée vers ces surfaces est retirée, donc le joueur glisse le long sans grimper.
- Le joueur s'oriente selon le forward de la caméra quand il bouge (`MoveRotation`).
- Peut être désactivé par l'option de mode `player_movement_disabled`.

### CameraMovement
- Rotation à la souris (`sensibility`, 0.15), pitch limité à ±90°. La rotation est faite dans `Update`. La caméra suit `Target.position + offset` dans `LateUpdate`, y compris quand la rotation est verrouillée.
- **FOV au sprint** : le FOV augmente de `sprintFovIncrease` (+10°) quand `PlayerMovement.IsSprinting` est vrai, c'est-à-dire en sprint et plus vite que la marche. La transition est lissée (`fovChangeSpeed`) et le FOV de base est celui de la caméra.
- Peut être bloquée par l'option de mode `camera_movement_locked`.

## 3. Stats du joueur

- **StatName** : `Health` et `Endurance`.
- **PlayerStats** : liste de paires (StatName → Stat), sérialisée sur le PlayerManager, donc persistante. Appelle `Actualize(deltaTime)` sur chaque stat à chaque frame.
- **Stat** : `Value` est bornée entre 0 et `MaxValue`. Events `ev_updateValue` et `ev_updateMaxValue`. `Consume(x)` retire une valeur fixe. À chaque frame, la stat gagne `baseRegeneration` plus la somme des markers actifs, multipliée par deltaTime.
- **StatMarker** : modificateur de régénération identifié par une clé. Il vaut un montant par seconde (positif ou négatif) pendant une durée (infinie par défaut), puis il est supprimé.
- **Modifiers (effets)** : `AbstractModifier.Modify(GameObject)`. `LifeModifier` et `EnduranceModifier` ajoutent un marker sur la stat correspondante (montant/s pendant `duration`).
- **Consommables** : `ConsummableData` (ScriptableObject, hérite d'ItemData) contient une liste de modifiers (`SerializeReference` + SubclassPicker), appliqués à la consommation depuis `PlayerInventory`.
- **UI** : `PlayerStatsUI`, `StatBarUI` et `StatUIData` affichent les stats sous forme de barres.

## 4. Inventaire

### Données
- **ItemData** (ScriptableObject `Item/Item`) : `Id` (int saisi à la main), `StackMax` (99 par défaut), `Name`, `Description`, `Sprite` (icône UI) et `Model` (prefab 3D utilisé au drop). Sous-classe `ConsummableData` (voir feature 3).
- **Item** : simple wrapper autour d'un ItemData (`Item.CreateFromData`). Pas encore de données d'instance (durabilité, etc.).
- **ItemSlot** : un item et une quantité. `Add` respecte `StackMax` et renvoie le reste, `Remove` renvoie ce qui manque. `Switch` échange le contenu avec un autre slot. `Drop` passe par `Pickupable.Spawn`, qui instancie le `Model` devant la source avec un `Pickupable` déjà identifié et un Rigidbody qui reçoit une petite impulsion. Le slot n'est vidé que si le spawn réussit (modèle valide avec un `Pickupable`).
- **ItemCollection** : liste de slots de taille fixe (`MaxSize`). `Add` complète d'abord les stacks non pleins puis remplit les slots vides, et renvoie le surplus. `RemoveItem` retire en partant du dernier slot. `Count` compte la quantité totale d'un item. Event `ev_Update`.
- **ItemStorage** (MonoBehaviour) : encapsule une ItemCollection. Sert de base au PlayerInventory et aux conteneurs.

### PlayerInventory (sur le PlayerManager, donc persistant)
- Inventaire de **32 slots** et barre d'outils de **6 slots**.
- `Add` remplit l'inventaire d'abord, puis la barre d'outils. `Remove` retire de l'inventaire, puis de la barre d'outils.
- Touche Inventaire : ouvre ou ferme l'inventaire (`ev_UpdateIsOpen`).
- Molette : change le slot en main (`HandedIndex`, avec bouclage).
- `Use2` : consomme l'item en main si c'est un `ConsummableData`. `Use1` : vide. Les deux sont ignorés quand une interface est ouverte (`OPTION_USE_DISABLED`, mode `Interface`).
- Touche Drop (inventaire ouvert) : jette devant le joueur tout le contenu du slot tenu au curseur. Le slot du curseur est aussi jeté automatiquement quand la dernière interface se ferme.

### Interactions UI
- Système par **clic**, sans vrai glisser-déposer. Les clics sont gérés par `ItemSlotUI.OnPointerClick`. Un clic gauche sur un slot (`TryMerge`) prend son contenu dans le `draggingSlot`, qui suit la souris (`UIFollowCursor`). Un nouveau clic le dépose.
  - Même item : fusion des stacks, le surplus reste au curseur.
  - Item différent : échange (Switch).
- **Clic droit** (`SplitOrPlaceOne`) : avec un slot en main, dépose une unité sur le slot visé (vide ou même item). Main vide : prend la moitié du slot visé (arrondie au supérieur).
- **Shift + clic gauche** (`QuickTransfer`) : transfère tout le slot.
  - Avec un conteneur fouillé : conteneur → barre d'outils (puis inventaire), et barre d'outils ou inventaire → conteneur.
  - Sans conteneur : inventaire ↔ barre d'outils.
  - Ce qui ne rentre pas reste dans le slot d'origine.
- Pas de séparation de stack, de clic droit ni de shift-clic.
- **ItemCollectionUI** génère la grille de slots à partir de la collection (`slotsByLine`) et la redimensionne. Les slots UI sont réutilisés à chaque mise à jour : il n'en crée ou n'en supprime que si la taille change. `PlayerInventoryContentUI`, `PlayerToolBarUI` et `ItemStorageUI` (conteneur fouillé) en dérivent.
- **InventorySlotUI** : affiche le sprite et la quantité (quantité masquée si `StackMax == 1`).
- **HandedCursorUI** : le curseur glisse vers le slot en main dans la barre d'outils (SmoothDamp, `smoothTime` 0.05 s).

## 5. Interaction

- **PlayerPointer** : à chaque frame, raycast de **10 m** depuis la caméra, qui ignore le joueur et les triggers. Il met à jour `PlayerContext.PointedObject` avec l'objet qui porte l'`IInterractable` (cherché dans les parents du collider touché), avec l'event `ev_updatePointedObject`.
- **PlayerContext.Interractable** : `IInterractable` trouvé sur l'objet pointé.
- **PlayerInterractor** : la touche Interagir appelle `Interract(player)` sur l'objet pointé.
- **IInterractable** : `Interract(GameObject)`, `GetInfo()` (texte affiché) et `GetIcon()`. Implémentations :
  - **Pickupable** : `item` et `count`. Interagir l'ajoute à l'inventaire via `Pickuper`. L'objet est détruit si tout est ramassé, sinon le reste reste au sol. Si `identified` vaut `true`, le texte affiche « Nx Nom », sinon « Pickup ».
  - **Searchable** : ouvre l'`ItemStorage` associé via `Searcher` dans l'UI de conteneur (`ItemStorageUI`). Interagir de nouveau avec le même conteneur le referme, et interagir avec un autre conteneur ouvre celui-ci à la place.
  - **CraftingStation** : voir la feature Crafting.
- **Pickuper** : ajoute à l'inventaire et émet `ev_pickup(quantité ramassée, item)`.
- **Searcher** : garde le conteneur ouvert et émet `ev_UpdateSearchStorage`. Le conteneur est fermé via `ClearCurrentSearchedStorage` quand l'UI se ferme.
- **UI**
  - `PointedInterractorInformationUI` : affiche l'icône, le texte et la touche de l'objet pointé, avec un fondu. Le texte est mis à jour en continu (changement d'objet, ramassage partiel).
  - `PickupInfoGroupUI` / `PickupInfoUI` : notification « +N Nom » avec icône, qui disparaît en fondu en 5 s.
- **Contour de l'objet pointé** (`InteractableOutline`, sur le prefab `PlayerCamera`) : un contour blanc en espace écran entoure l'objet interactable pointé.
  - Un `CommandBuffer` (`BeforeImageEffects`) dessine la silhouette des `MeshRenderer` et `SkinnedMeshRenderer` de l'objet, tous sous-meshes compris, dans une texture masque. Le shader `Hidden/InteractableOutline` (`Assets/Shaders`) trace ensuite le contour autour du masque.
  - Le buffer n'est reconstruit que quand l'objet pointé change. Réglages : `outlineColor` (blanc) et `outlineWidth` (3 px).
  - Le contour suit toute la silhouette, y compris les parties cachées derrière un autre objet.

## 6. Crafting

- **RecipeData** (ScriptableObject `Crafting/Recipe`) : listes d'`Ingredients` et de `Results`. Chaque entrée est un `ItemBlock` (ItemData + quantité).
- **CraftingStation** (`IInterractable`) : contient la liste des recettes disponibles. Interagir avec elle appelle `Crafter.Check(station)`, qui ouvre l'UI.
- **Crafter** (sur le joueur) :
  - `Craft(recipe)` vérifie que la station est valide, que la recette lui appartient et que les ingrédients sont présents (`CheckHasItems`, qui additionne les ingrédients en double et compte l'inventaire et la barre d'outils).
  - Ensuite, il retire les ingrédients de l'inventaire et ajoute les résultats. Ce qui ne rentre pas (inventaire plein) est jeté au sol devant le joueur. Le craft est instantané.
- **UI**
  - `CraftingStationUI` génère un `RecipePanelUI` par recette de la station.
  - `RecipePanelUI` affiche le premier résultat (icône et quantité) et les ingrédients. Le bouton appelle `OnCraft` → `Crafter.Craft`.

## 7. Construction

- **Builder** (sur le joueur), avec un seul prefab constructible (`build`, actuellement `BaseDom`) :
  - La touche Build fait apparaître un fantôme là où vise la caméra (raycast de 300 m sur le layer `Terrain`). Un nouvel appui l'annule.
  - Le fantôme suit le terrain. Sa hauteur est arrondie à un pas de **5 m**, plus **3 m** de décalage (`PutHeight`, en cours de modification). X et Z ne sont pas alignés sur une grille.
  - `Use1` valide la pose si le `BuildVerifier` l'autorise. Le terrain est alors **aplani** sous la construction (`ChunkManager.FlattenTerrainAt`, rayon basé sur le scale, hauteur = position + 0.2) puis `BuildObject.Put()` est appelé.
- **BuildVerifier** : la pose est valide s'il n'y a **aucun trigger en contact** (compteur Enter/Exit) **et** si le terrain est assez plat. La planéité est mesurée par un raycast 5×5 sur l'emprise, avec un écart de hauteur ≤ 2 m. Elle est recalculée quand le fantôme bouge de plus de 0.5 m. Event `ev_updateValidity`.
- **BuildObject** : `Put()` détruit le verifier, active les colliders des `childs` et émet `ev_put`.
- **BuildObjectTexture** : applique un matériau valide ou invalide sur le fantôme selon la validité, puis remet les matériaux d'origine à la pose.
- `ObjectPlacement` (gameObject, position 2D, hauteur) : classe utilitaire, non utilisée pour l'instant.

## 8. Terrain procédural

- **ChunkManager** (singleton, scène Monde) :
  - Grille de chunks de `chunkSize` (64 m). Les chunks situés à moins de `generationDistance` (2) de celui du joueur sont chargés, soit une grille de 5×5.
  - Les chunks éloignés sont **désactivés** (jamais détruits) et réactivés au retour.
  - Les chunks chargés sont reliés à leurs voisins (`Terrain.SetNeighbors`, vérifié à chaque frame dans `LateUpdate`, auto-connexion Unity désactivée), ce qui évite les fissures de LOD vues de loin.
  - Seed → offset Perlin global, ce qui donne un terrain continu et déterministe.
  - `ToReplace` : le joueur et les transforms configurés sont replacés sur la surface quand leur chunk a fini de se générer.
  - `FlattenTerrainAt(pos, rayon, hauteur)` aplanit tous les chunks concernés (utilisé par la construction, en cours de modification). Le plateau a une pente douce sur 1.3× le rayon, avec une tolérance verticale de 4 m. Seul le terrain au-dessus de « cible − tolérance » est modifié : les creux profonds ne sont pas remplis.
  - Gizmos de debug : chunks générés et chargés.
- **TerrainGenerator** (sur le prefab `Chunk`) : duplique le `TerrainData`. Le terrain utilise le shader `Terrain/World UV Standard` : les textures sont positionnées depuis la position monde, donc continues entre les chunks. Il calcule la heightmap (résolution `size+1`, hauteur max 1024) **en asynchrone** (`Task.Run`). Le bruit combine plusieurs couches de Perlin : variation locale, variation globale, montagnes et creux. Émet `ev_OnTerminateGeneration` à la fin.
- **EnvironmentSpawner** (sur le prefab `Chunk`) : pour chaque m² du chunk, a 0.1 % de chances de faire apparaître `prefab`, avec une rotation et une taille aléatoires.

## 9. Modes de jeu & interfaces

- **PlayingMode** (`[Flags]`) : `Default`, `Interface`, `Build`. Plusieurs modes peuvent être actifs en même temps.
- **ModeManager** (singleton C# statique) : `EnableMode` et `DisableMode` modifient le masque de modes et émettent `ev_OnUpdateOptions`. `IsEnabled(option)` renvoie vrai si un mode actif contient cette option.
- **PlayerModeOptions** : table statique mode → options. Le mode `Interface` active `player_movement_disabled`, `interface_manager_cursor_visible` et `camera_movement_locked`.
- **InterfaceManager** (singleton C# statique) : liste des interfaces ouvertes (`SetInterface(go, bool)`).
  - Dès qu'une interface est ouverte, il active le mode `Interface` : curseur visible et déverrouillé, mouvement et caméra bloqués. Il le désactive quand plus rien n'est ouvert.
  - `CloseInterfaces()`, appelé par la touche CloseInterfaces depuis `ParentUI`, ferme tout.
- **ParentUI** : racine de l'UI instanciée par UIManager. Elle référence les sous-UI (notifications de ramassage, conteneur, craft) et affiche le slot tenu au curseur.
- **ParentedUI** : classe de base des éléments d'UI. Elle reçoit `OnUpdateManager` et `OnUpdatePlayerContext` quand le manager ou le joueur change.
