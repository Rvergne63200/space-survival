# Space Survival : features implémentées

## 1. Bootstrap & chargement de scènes

### Scènes
- `Bootstrap` : contient les objets persistants (managers).
- `0_MainMenu` : menu principal, première scène chargée par le Bootstrapper (`MainMenuUI`).
- `0_World` : le monde. Il contient les capsules (prefab `SpaceCapsule`), avec leur intérieur, et le `PlayerSpawnPoint`.
- `1_TestScene` : scène de test.

Flux : Menu ⇄ Monde. Les capsules ne sont pas des scènes séparées : chacune est un prefab posé dans le monde, avec son propre intérieur, et on voit l'extérieur depuis l'intérieur.

### Fonctionnement actuel
- **Root** : singleton (flag static) marqué `DontDestroyOnLoad`. C'est la racine des objets persistants de la scène Bootstrap.
- **SceneContext** : à placer dans chaque scène de jeu. Si aucun `Root` n'existe, il charge `Bootstrap` en additif, ce qui permet de lancer le jeu depuis n'importe quelle scène dans l'éditeur.
- **Bootstrapper** : si seule la scène Bootstrap est chargée, il charge `firstScene` (en mode Single).
- **SceneLoadingManager** : appelle `DynamicGI.UpdateEnvironment()` à chaque chargement de scène.
- **PlayerManager** (singleton) : garde l'inventaire et les stats. À chaque `sceneLoaded`, il cherche un `PlayerSpawnPoint` dans les scènes chargées.
  - S'il en trouve un, il réinstancie le joueur à sa position sauvegardée (`SpawnedFromSave`), ou sinon sur le point de spawn. Il notifie ensuite l'UI et la caméra (`ev_spawnPlayer`, `ev_updatePlayerContext`).
  - Sans point de spawn (menu), il n'y a pas de joueur (`DespawnPlayer`), et la caméra du joueur et le HUD du jeu sont masqués (`UIManager.SetHudVisible`).
- **CameraManager** / **UIManager** (singletons) : instancient leur prefab une seule fois, dans leur Awake. UIManager branche les events du joueur (Pickuper, Searcher, Crafter) sur l'UI à chaque spawn.
- **SceneLoader** : `LoadScene()` sauvegarde le monde (`SaveManager.BeforeSceneChange`), puis charge `sceneName` en mode Single.

### Menus (UI Toolkit, mise en page dans `Assets/UI/Menus`, modifiable dans le UI Builder)
- Les scripts retrouvent les éléments par leur nom (`name` dans l'UXML). Un élément absent désactive seulement sa fonction, avec un avertissement dans la console. Style commun : `Menus.uss`.
- **Menu principal** (`MainMenuUI`, scène `0_MainMenu`, `MainMenu.uxml`) : mode `PlayingMode.Menu`, avec un curseur libre et sans contrôle du joueur.
  - **Nouvelle partie** : nom (pré-rempli « Partie N », doit être unique) et seed. La seed est pré-remplie au hasard, avec un bouton Aléatoire ; un texte non numérique est converti en nombre (`SaveManager.ParseSeed`).
  - **Charger une partie** : liste des sauvegardes (`SaveSlot.uxml`), de la plus récente à la plus ancienne, avec le nom, la seed et la date. Chaque ligne a un bouton Charger et un bouton Supprimer (avec confirmation).
  - **Quitter**.
- **Menu pause** (`PauseMenuUI`, dans `Bootstrap`, `PauseMenu.uxml`) : Échap ferme les interfaces ouvertes ; s'il n'y en a aucune, il ouvre le menu pause (en jeu). Le menu pause est lui-même une interface, donc Échap le ferme. Pendant la pause, `timeScale` vaut 0. Boutons : Reprendre, Retour au menu principal (sauvegarde puis chargement du menu) et Quitter.
- **Écran de mort** (`DeathScreenUI`, dans `Bootstrap`, `DeathScreen.uxml`) : filtre rouge (classe `death-root`) et deux boutons. « Réapparaître » est grisé avec un compte à rebours pendant `respawnDelay` secondes (25). « Menu principal » fait réapparaître le joueur, sauvegarde, puis charge le menu. Ce n'est pas une interface : Échap ne le ferme pas et n'ouvre pas le menu pause. Le curseur est libre (mode `Menu`).
- **Messages au joueur** (`NotificationUI`, dans `Bootstrap`, `Notifications.uxml`, style `.notification` / `.notification-warning`) : messages courts en bas de l'écran, qui disparaissent en fondu après 2,5 s (« Inventaire plein », « Ressources manquantes »…). Un message identique remet son compteur à zéro au lieu de s'afficher deux fois. Il ne capte pas la souris. Usage : `NotificationUI.Show(texte, NotificationType.Warning)`.
### Interfaces du jeu (UI Toolkit, mise en page dans `Assets/UI/Game`, modifiable dans le UI Builder)
- Prefab `Prefabs/UI/UI` (instancié par le `UIManager`, racine `ParentUI`). Chaque interface est un UIDocument (PanelSettings commun, style `Game.uss`) :
  - **HUD** (`Hud.uxml`, ordre 0) : réticule, texte de l'objet pointé, barres de stats, ramassages, barre d'outils et cadre du slot en main. Seuls les slots de la barre d'outils captent la souris.
  - **Inventaire** (`Inventory.uxml`), **Conteneur** (`Storage.uxml`) et **Fabrication** (`CraftingStation.uxml`), ordre 2 : chacun est un GameObject `Panel` activé à l'ouverture et enregistré comme interface (`InterfaceManager`). Fermé par Échap ou à la mort, il prévient son propriétaire (inventaire fermé, `Searcher.ClearCurrentSearchedStorage`, `Crafter.ClearStation`).
  - **Item au curseur** (`HeldItem.uxml`, ordre 4) : au-dessus des interfaces, ne capte pas la souris.
- Modèles instanciés par le code : `ItemSlot.uxml`, `StatBar.uxml`, `PickupInfo.uxml`, `RecipeSlot.uxml`, `RecipeIngredient.uxml`. Les scripts retrouvent les éléments par leur nom.
- Sans joueur (menu principal), le HUD et l'item au curseur sont masqués (`ParentUI.SetHudVisible`).


### Sauvegarde (`SaveManager`, dans `Bootstrap`)
- Un fichier JSON par partie dans `Application.persistentDataPath/Saves`, choisi dans le menu principal (`NewGame`, `LoadGame`, `DeleteSave`, `ListSaves`). Sauvegarde à la fermeture du jeu, avant un changement de scène et toutes les `autosaveInterval` secondes (120) dans le monde. Restauration au chargement du monde.
  - Une nouvelle partie repart de l'inventaire et des stats de départ (capturés au lancement du jeu), même après une autre partie dans la même session.
  - Monde lancé directement dans l'éditeur, sans passer par le menu : une sauvegarde « Partie éditeur » est créée.
  - Menu contextuel du composant : « Refresh item list » et « Open saves folder ».
- **Contenu** : la seed du monde, le joueur (position, rotation, vue de la caméra, stats, inventaire, barre d'outils, slot en main), les objets sauvegardables, les objets retirés et les aplanissements du terrain.
- **SaveableObject** : un objet sauvegardé (transform et composants `ISaveableComponent`, y compris ceux de ses enfants qui n'ont pas leur propre SaveableObject). Trois sortes d'objets :
  - **posé dans une scène** : id = `scène:chemin dans la hiérarchie` (voir R15) ;
  - **généré** (`EnvironmentSpawner`) : id déterministe (`env_<x>_<z>_<index>`) ;
  - **créé en jeu** (construction, objet jeté) : id unique et clé de prefab (`build:<nom>`, `item:<id>`). Le SaveManager le recrée au chargement.
- Un objet posé ou généré qui est retiré en jeu (ramassé, détruit) est noté et n'est plus recréé. Les objets des chunks non visités gardent leur dernier état sauvegardé.
- **Composants sauvegardés** : `ItemStorage` (contenu des coffres : chaque coffre a automatiquement un SaveableObject, chaque capsule garde donc son propre intérieur) et `Pickupable` (item, quantité). Les items sont retrouvés par leur `Id` dans la liste `items` du SaveManager.
- **Terrain** : la seed sauvegardée remplace celle de la scène. Les aplanissements (`FlattenTerrainAt`) sont enregistrés et rejoués à la génération de chaque chunk concerné.
- **Joueur restauré** : sa position est conservée (sur le sol d'une capsule, par exemple), et son Rigidbody reste cinématique jusqu'à la génération du chunk sous lui (`ChunkManager.HoldUntilGenerated`).

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

- **StatName** : `Health`, `Endurance`, `Hunger`, `Thirst` et `Oxygen` (sérialisé par valeur : ajouter les nouvelles stats à la fin).
- **PlayerStats** : liste de paires (StatName → Stat), sérialisée sur le PlayerManager, donc persistante. À chaque frame en jeu (pas dans le menu), il appelle `Actualize(deltaTime)` sur chaque stat.
  - **Faim** et **soif** baissent en continu (`baseRegeneration` négatif) : une jauge pleine tient environ 40 min pour la faim et 25 min pour la soif.
  - **Oxygène** : il reste au maximum pour l'instant. Il dépendra plus tard du réseau d'oxygène et de bouteilles d'oxygène (système d'équipements).
  - **Stats vitales** (`vitalStats` : faim, soif, oxygène) : tant que l'une d'elles est à 0, la vie baisse de `emptyVitalDamage` (1) par seconde, et ces pertes s'additionnent.
- **Mort** (vie à 0, `PlayerManager.KillPlayer`) : l'inventaire, la barre d'outils et l'objet tenu au curseur sont placés dans un **sac** (prefab `DeathBag`, conteneur fouillable) posé au sol à l'endroit de la mort. Le sac est sauvegardé (`prefab:DeathBag`) et disparaît une fois vidé (vérifié en fin de frame, à chaque changement de l'un de ses slots).
  - Les interfaces se ferment et l'**écran de mort** s'affiche (`DeathScreenUI`, voir Menus). Les stats sont figées (`PlayerManager.IsDead`).
  - **Réapparaître** (`PlayerManager.Respawn`) : toutes les stats remontent au maximum, et le joueur réapparaît sur le `PlayerSpawnPoint`.
- **Stat** : `Value` est bornée entre 0 et `MaxValue`. Events `ev_updateValue` et `ev_updateMaxValue`. `Consume(x)` retire une valeur fixe. À chaque frame, la stat gagne `baseRegeneration` plus la somme des markers actifs, multipliée par deltaTime.
- **StatMarker** : modificateur de régénération identifié par une clé. Il vaut un montant par seconde (positif ou négatif) pendant une durée (infinie par défaut), puis il est supprimé.
- **Modifiers (effets)** : `AbstractModifier.Modify(GameObject)`.
  - `StatModifier` (générique) : modifie une stat de `amount`, d'un coup (`duration` = 0) ou réparti sur `duration` secondes.
  - `LifeModifier` ajoute un marker sur la vie (montant/s pendant `duration`). `EnduranceModifier` ne fait qu'un `Debug.Log`.
- **Consommables de test** : `6_Ration` (faim +40) et `7_Water` (soif +50), 5 de chaque dans l'inventaire de départ.
- **First Aid Kit** (`23_FirstAidKit`) : consommable, vie +50 d'un coup.
- **Consommables** : `ConsummableData` (ScriptableObject, hérite d'ItemData) contient une liste de modifiers (`SerializeReference` + SubclassPicker), appliqués à la consommation depuis `PlayerInventory`.
- **UI** : `PlayerStatsUI` crée une barre (`StatBarUI`, `StatBar.uxml`) par stat de `statUIDataCollection` (`StatUIData` : stat, icône, couleur), en bas à gauche du HUD. Chaque barre affiche « valeur/max » et un remplissage proportionnel.

## 4. Inventaire

### Données
- **ItemData** (ScriptableObject `Item/Item`) : `Id` (int saisi à la main), `StackMax` (99 par défaut), `Name`, `Description`, `Sprite` (icône UI) et `Model` (prefab 3D utilisé au drop). Sous-classe `ConsummableData` (voir feature 3).
- **Objets** (`ScriptableObjects/Items`, ids 0 à 28 ; 25 à 28 : câble et objets de test électriques, voir section 10) : ressources `Rock`, `Wood Log`, `Branch`, `Plant Fibers`, `Iron Ore`, `Iron`, `Coal`, `Copper Ore`, `Titanium Ore`, `Titanium`, `Ice`, `String`, `Circuit Board`, `Microchip`, `Glass`, `Glue` ; consommables `Ration`, `Water`, `First Aid Kit` ; plaçables `Crate`, `Radio Relay` (prefab `RadioRelais`, sans effet pour l'instant) et `Crafter` (prefab `Crafter`, `CraftingStation` sans recette pour l'instant) ; `Oxygen Tank` et `Jetpack` sans effet pour l'instant, `Flashlight` (pile de 1). Chaque objet doit être dans la liste `items` du SaveManager (Bootstrap).
- **Item** : simple wrapper autour d'un ItemData (`Item.CreateFromData`). Pas encore de données d'instance (durabilité, etc.).
- **ItemSlot** : un item et une quantité. `Add` respecte `StackMax` et renvoie le reste, `Remove` renvoie ce qui manque. `Switch` échange le contenu avec un autre slot. `Drop` passe par `Pickupable.Spawn`, qui instancie le `Model` devant la source avec un `Pickupable` déjà identifié et un Rigidbody qui reçoit une petite impulsion. Le slot n'est vidé que si le spawn réussit (modèle valide avec un `Pickupable`).
- **ItemCollection** : liste de slots de taille fixe (`MaxSize`). `Add` complète d'abord les stacks non pleins puis remplit les slots vides, et renvoie le surplus. `RemoveItem` retire en partant du dernier slot. `Count` compte la quantité totale d'un item. Event `ev_Update`.
- **ItemStorage** (MonoBehaviour) : encapsule une ItemCollection. Sert de base au PlayerInventory et aux conteneurs.

### PlayerInventory (sur le PlayerManager, donc persistant)
- Inventaire de **32 slots** et barre d'outils de **6 slots**.
- `Add` : un slot qui contient déjà le même item est prioritaire (barre d'outils, puis inventaire), puis les slots vides de la barre d'outils, puis ceux de l'inventaire. `Remove` retire de l'inventaire, puis de la barre d'outils.
- Touche Inventaire : ouvre ou ferme l'inventaire (`ev_UpdateIsOpen`).
- Molette : change le slot en main (`HandedIndex`, avec bouclage).
- **Usage de l'item en main** : `Use1` (clic gauche) exécute ses actions principales et `Use2` (clic droit) ses actions secondaires (`ItemData.PrimaryActions` / `SecondaryActions`, des `ItemAction` comme `ApplyModifiersAction`). Sans action secondaire, un `ConsummableData` est consommé au clic droit. Les deux sont ignorés quand une interface est ouverte ou en mode construction (`OPTION_USE_DISABLED`).
  - **Modes des actions** (`ItemAction.Mode`) : les outils s'utilisent au clic gauche.
    - `Press` (`ItemAction`) : `Execute` une fois par clic (arme qui tire, consommable).
    - `Toggle` (`ToggleItemAction`) : chaque clic alterne allumé / éteint (`SetOn`). L'action s'éteint quand l'item quitte la main.
    - `Hold` (`HoldItemAction`) : `Execute` démarre, `Hold(context, heldTime)` est appelé à chaque frame jusqu'à ce qu'il renvoie false, puis `End(context, completed)`. Le relâchement du bouton, l'ouverture d'une interface ou le changement d'item arrêtent l'action (`completed` = false).
  - **Item en main** : `PlayerInventory` vérifie à chaque frame si l'item en main change (autre slot, slot vidé, joueur absent). Il appelle alors `OnUnequip` sur les actions de l'ancien item (`ItemData.Unequip`).
  - `ItemUseContext.View` : le transform de la caméra du joueur.
  - L'état d'une action (allumée, maintenue) est porté par l'asset ItemData : il est partagé par tous les items de ce type.
- **Lampe torche** (`22_Flashlight`, `FlashlightAction`) : un spot (couleur, intensité 1,5, portée 30 m, angle 60°, ombres douces) créé comme enfant de la caméra au premier allumage.
- Touche Drop (inventaire ouvert) : jette devant le joueur tout le contenu du slot tenu au curseur. Le slot du curseur est aussi jeté automatiquement quand la dernière interface se ferme.

### Interactions UI
- Système par **clic**, sans vrai glisser-déposer. Les clics sont gérés par `ItemSlotUI` (`PointerDownEvent` sur l'élément `slot`). Un clic gauche sur un slot (`TryMerge`) prend son contenu dans le `draggingSlot`, qui suit la souris (`HeldItemUI`). Un nouveau clic le dépose.
  - Même item : fusion des stacks, le surplus reste au curseur.
  - Item différent : échange (Switch).
- **Double-clic** (`GatherToCursor`) : deux clics gauches sur le même slot à moins de 0,35 s, détectés par `ItemSlotUI` et non par `clickCount`, qui compte les clics de tout le panel. Le premier clic prend la pile au curseur, le second y rassemble les items du même type de l'inventaire, de la barre d'outils et du conteneur ouvert, jusqu'à une pile complète (`StackMax`). Les plus petites piles sont prises en premier.
- **Clic droit** (`SplitOrPlaceOne`) : avec un slot en main, dépose une unité sur le slot visé (vide ou même item). Main vide : prend la moitié du slot visé (arrondie au supérieur).
- **Shift + clic gauche** (`QuickTransfer`) : transfère tout le slot.
  - Avec un conteneur fouillé : conteneur → barre d'outils (puis inventaire), et barre d'outils ou inventaire → conteneur.
  - Sans conteneur : inventaire ↔ barre d'outils.
  - Ce qui ne rentre pas reste dans le slot d'origine.
- **ItemCollectionUI** ajoute un `ItemSlot.uxml` par slot de la collection dans un conteneur du UIDocument. Les slots UI sont réutilisés à chaque mise à jour : il n'en crée ou n'en supprime que si la taille change. La largeur de la grille (8 slots par ligne) est fixée dans `Game.uss` (`.item-grid`). `PlayerInventoryContentUI` (`Inventory.uxml`, à gauche), `PlayerToolBarUI` (HUD, en bas au centre) et `ItemStorageUI` (conteneur fouillé, `Storage.uxml`, à droite) en dérivent.
- **ItemSlotUI** : affiche le sprite et la quantité (quantité masquée si `StackMax == 1`).
- **HandedCursorUI** : le cadre (`handed-cursor`) glisse vers le slot en main dans la barre d'outils (SmoothDamp, `smoothTime` 0.05 s).
- **HeldItemUI** (`HeldItem.uxml`) : l'item tenu au curseur, affiché au-dessus des interfaces et qui ne capte pas les clics. `PickingMode.Ignore` est forcé en code sur l'élément, ses enfants et ses parents.

## 5. Interaction

- **PlayerPointer** : à chaque frame, raycast de **3 m** (`range`) depuis la caméra, qui ignore le joueur et les triggers. Il met à jour `PlayerContext.PointedObject` avec l'objet qui porte l'`IInterractable` (cherché dans les parents du collider touché), avec l'event `ev_updatePointedObject`.
- **PlayerContext.Interractable** : `IInterractable` trouvé sur l'objet pointé.
- **PlayerInterractor** : la touche Interagir appelle `Interract(player)` sur l'objet pointé.
- **IInterractable** : `Interract(GameObject)`, et `GetInfo()` (texte affiché). Implémentations :
  - **Pickupable** : `item` et `count`. Interagir l'ajoute à l'inventaire via `Pickuper`. L'objet est détruit si tout est ramassé, sinon le reste reste au sol. Si `identified` vaut `true`, le texte affiche « Nx Nom », sinon « Pickup ».
  - **Searchable** : ouvre l'`ItemStorage` associé via `Searcher` dans l'UI de conteneur (`ItemStorageUI`). L'inventaire du joueur s'ouvre avec lui et se referme avec lui, sauf si le joueur l'avait ouvert lui-même (touche Inventaire) : `PlayerInventory.OpenWith` / `CloseWith`. Interagir de nouveau avec le même conteneur le referme, et interagir avec un autre conteneur ouvre celui-ci à la place.
  - **CraftingStation** : voir la feature Crafting.
- **Pickuper** : ajoute à l'inventaire et émet `ev_pickup(quantité ramassée, item)`.
- **Searcher** : garde le conteneur ouvert et émet `ev_UpdateSearchStorage`. Le conteneur est fermé via `ClearCurrentSearchedStorage` quand l'UI se ferme, quand le joueur s'en éloigne de plus de `maxDistance` (4,5 m, jusqu'à ses colliders) ou quand il est détruit.
- **UI**
  - `PointedInterractorInformationUI` : affiche le texte et la touche de l'objet pointé, au-dessus du réticule (sans icône), avec un fondu. Le texte est mis à jour en continu (changement d'objet, ramassage partiel).
  - `PickupInfoGroupUI` / `PickupInfoUI` (`PickupInfo.uxml`, en bas à droite) : notification « +N Nom » avec icône, qui disparaît en fondu en 5 s. Les ramassages d'un même item sont cumulés (« +8 Rock ») tant qu'ils se suivent à moins de `groupDuration` (10 s), même si sa notification a disparu entre-temps. Un ramassage partiel ou impossible affiche « Inventaire plein » (`NotificationUI`).
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
  - `CraftingStationUI` (`CraftingStation.uxml`, à droite) : liste des recettes de la station à gauche (une icône `RecipeSlot.uxml` par recette, `RecipeSlotUI`), détail de la recette sélectionnée à droite : icône, nom et description du premier résultat, quantité obtenue, ingrédients (`RecipeIngredient.uxml` : icône, nom, possédé/requis, en rouge s'il manque).
  - Quantité à fabriquer : boutons −, + et Max (limitée par `Crafter.MaxCraftable`, le nombre de crafts possibles avec l'inventaire). « Fabriquer » enchaîne les crafts. Les besoins affichés sont multipliés par la quantité.
  - Les recettes irréalisables sont grisées (classe `recipe-unavailable`). La case « Fabricables uniquement » les masque (choix conservé d'une ouverture à l'autre) ; si la recette sélectionnée est masquée, la première visible est sélectionnée.
  - Rafraîchi toutes les 0,25 s. L'inventaire du joueur s'ouvre avec la station (comme pour un conteneur). À la fermeture de l'interface, la station du `Crafter` est libérée (`ClearStation`). Les échecs (ressources manquantes…) et un résultat posé au sol affichent un message (`NotificationUI`). L'UI se ferme quand le joueur s'éloigne de la station de plus de `Crafter.maxDistance` (4,5 m).

## 7. Construction

Deux façons de poser quelque chose, avec le même fantôme vert (valide) ou rouge (invalide) :
- **Placement** : un item plaçable de l'inventaire (une caisse, par exemple) est posé tel quel.
- **Build** : une structure choisie dans la roue est posée sous forme de **plan**, auquel on apporte ensuite les ressources de son coût.

- **Roue de construction** (`BuildWheelUI`, dans `Bootstrap`, `BuildWheel.uxml` et `BuildWheelOption.uxml`, style dans `Menus.uss`) : on maintient la touche Build (B) pour l'afficher, avec le curseur libre (mode `Menu`). Un clic sur une option la choisit et ferme la roue ; relâcher B la ferme aussi.
  - Options : « Détruire », puis les structures du `StructureCatalog`. Elles sont disposées en cercle (`radius`), et le centre affiche le nom et le coût de l'option survolée.
- **Builder** (sur le joueur). Pendant le placement, le build ou la destruction, le mode `PlayingMode.Build` désactive l'usage de l'item en main (clics). Tout est masqué pendant qu'une interface est ouverte.
  - **Fantôme** : copie visuelle du prefab (meshes seulement, sans script ni collider : `BuildUtility.CreateVisual`), posée là où vise la caméra, à **8 m** maximum (`range`). Pour une structure, la portée s'ajoute au rayon de son emprise, et son centre reste à au moins « rayon + 1 m » du joueur, qui ne peut donc pas se trouver dedans.
  - **Commandes** : R le fait tourner (90°/s), Shift + R par pas de 45°. Clic gauche : poser. Clic droit ou Échap : annuler (Échap quitte d'abord le mode, puis ouvre le menu pause).
  - **Bandeau d'indication** (`BuildModeUI`, dans `Bootstrap`, `BuildModeHint.uxml`) : en bas de l'écran, le mode en cours (Placement, Construction ou Destruction, avec le nom de l'objet) et ses commandes. Il ne capte pas la souris.
  - **Validité** : surface d'appui assez horizontale, aucun objet dans la boîte englobante (le terrain excepté, avec une boîte réduite de 10 % pour ne pas compter le sol). Pour une structure, s'y ajoute un écart de hauteur du terrain sous les 4 coins ≤ `MaxSlope`.
  - **Placement** : démarre tout seul quand l'item en main est un `PlaceableItemData`. La pose instancie son `placedPrefab` (`PlacedObject`) et retire un item du slot en main. Après un clic droit, le fantôme ne revient qu'au prochain changement d'item en main.
  - **Build** : la pose aplanit le terrain sous la structure (si `FlattenTerrain`), puis crée son plan.
  - **Destruction** : la structure, le plan ou l'objet posé visé est surligné en rouge, et un clic gauche le détruit. Clic droit pour quitter le mode.
- **StructureData** (ScriptableObject, Construction/Structure) : id (sauvegardes), nom, icône, prefab, coût (`Cost`), ressources rendues à la destruction (`Refund`, n'importe quels items), aplanissement, pente max. **StructureCatalog** : la liste des structures (roue et sauvegarde). Structure actuelle : `BaseDom`, dôme de 8 m de diamètre, base de 8,8 m (20 fer, 30 bois ; rend 10 fer).
- **StructureBlueprint** (plan) : **hologramme bleu** (shader `Custom/Hologram` : lueur additive, bords lumineux, lignes de balayage animées, léger scintillement ; matériau `Resources/Materials/Blueprint`). Il a un collider pour être visé, sur le layer `Blueprint`, qui n'entre pas en collision avec le joueur : on le traverse (matrice de collision, et `Physics.IgnoreLayerCollision` par sécurité). La détection du sol du joueur l'ignore aussi. Pour interagir avec le plan, il faut le viser depuis l'extérieur. Interagir dépose tout ce qui manque et qu'on a dans l'inventaire. Le texte affiche l'avancement (« Dôme (plan) : Iron 12/20, WoodLog 4/30 »). Une fois tout apporté, il devient la structure réelle (`BuiltStructure`). Détruit, il rend ce qui lui a été apporté.
- **PlaceableItemData** (ItemData, Item/Placeable) : `placedPrefab` et `Refund`, rendu à la destruction (vide : l'item lui-même). Le contenu des conteneurs d'un objet détruit va aussi dans l'inventaire. Item actuel : `8_Crate` (caisse de 20 slots).
- **Inventaire de départ** (test) : 60 fer et 90 bois de plus (de quoi construire 3 dômes), et 2 caisses dans la barre d'outils, ainsi que 5 `Radio Relay` et 5 `Crafter`. L'inventaire contient aussi 50 de chaque nouvelle ressource, 5 `First Aid Kit`, un `Oxygen Tank`, un `Jetpack` et une `Flashlight`.
- **Sauvegarde** : les structures (`build:<id>`), les plans (`blueprint:<id>`, avec les ressources apportées) et les objets posés (`placed:<itemId>`, avec le contenu de leurs conteneurs) sont des objets « runtime », recréés par `BuildUtility` au chargement.
- Ancien système, encore présent sur le prefab `BaseDom` : `BuildObject` (`Put()` active les colliders, appelé à l'instanciation de la structure), `BuildVerifier` et `BuildObjectTexture` (retirés de l'instance, voir R14). `ObjectPlacement` n'est pas utilisé.

## 8. Terrain procédural

- **ChunkManager** (singleton, scène Monde) :
  - Grille de chunks de `chunkSize` (64 m). Les chunks situés à moins de `generationDistance` (5 dans `0_World`) de celui du joueur sont chargés, soit une grille de 11×11. Ce calcul n'est refait que quand le joueur change de chunk.
  - Les chunks éloignés sont **désactivés**, et réactivés au retour. Au-delà de `generationDistance + destroyMargin` (2 chunks de plus), ils sont **détruits** avec leur `TerrainData` pour libérer la mémoire. Au retour, ils sont régénérés à l'identique : relief tiré de la seed, aplanissements rejoués, et objets restaurés par le `SaveManager`, qui garde leur état (`UnloadObjects`).
  - Les chunks chargés sont reliés à leurs voisins (`Terrain.SetNeighbors`, vérifié à chaque frame dans `LateUpdate`, auto-connexion Unity désactivée), ce qui évite les fissures de LOD vues de loin.
  - Seed → offset Perlin global, ce qui donne un terrain continu et déterministe. La seed de la scène ne sert qu'aux nouvelles parties : une partie sauvegardée garde sa seed.
  - `ToReplace` : le joueur et les transforms configurés sont replacés sur la surface quand leur chunk a fini de se générer. Le joueur est fourni par `PlayerManager` (`ev_spawnPlayer`), et la référence est mise à jour à chaque respawn.
  - `FlattenTerrainAt(pos, rayon, hauteur)` aplanit tous les chunks concernés (utilisé par la construction, en cours de modification). Le plateau a une pente douce sur 1.3× le rayon, avec une tolérance verticale de 4 m. Seul le terrain au-dessus de « cible − tolérance » est modifié : les creux profonds ne sont pas remplis.
  - Gizmos de debug : chunks générés et chargés.
- **TerrainGenerator** (sur le prefab `Chunk`) : duplique le `TerrainData`. Le terrain utilise le shader `Terrain/World UV Standard` : les textures sont positionnées depuis la position monde, donc continues entre les chunks. Il calcule la heightmap (résolution `size+1`, hauteur max 1024) **en asynchrone** (`Task.Run`). Le bruit combine plusieurs couches de Perlin : variation locale, variation globale, montagnes et creux. Émet `ev_OnTerminateGeneration` à la fin.
- **EnvironmentSpawner** (sur le prefab `Chunk`) : appelé une fois les hauteurs générées (`ev_OnTerminateGeneration`). Pour chaque m² du chunk, il a 0.1 % de chances de faire apparaître `prefab`, avec une rotation et une taille aléatoires. Le tirage est seedé par la seed du monde et la position du chunk : un chunk a toujours les mêmes objets.

## 9. Modes de jeu & interfaces

- **PlayingMode** (`[Flags]`) : `Default`, `Interface`, `Build`. Plusieurs modes peuvent être actifs en même temps.
- **ModeManager** (singleton C# statique) : `EnableMode` et `DisableMode` modifient le masque de modes et émettent `ev_OnUpdateOptions`. `IsEnabled(option)` renvoie vrai si un mode actif contient cette option.
- **PlayerModeOptions** : table statique mode → options. Le mode `Interface` active `player_movement_disabled`, `interface_manager_cursor_visible` et `camera_movement_locked`.
- **InterfaceManager** (singleton C# statique) : liste des interfaces ouvertes (`SetInterface(go, bool)`).
  - Dès qu'une interface est ouverte, il active le mode `Interface` : curseur visible et déverrouillé, mouvement et caméra bloqués. Il le désactive quand plus rien n'est ouvert.
  - `CloseInterfaces()`, appelé par la touche CloseInterfaces depuis `ParentUI`, ferme tout.
- **ParentUI** : racine de l'UI instanciée par UIManager. Elle référence les sous-UI (notifications de ramassage, conteneur, craft) et affiche le slot tenu au curseur.
- **ParentedUI** : classe de base des éléments d'UI. Elle reçoit `OnUpdateManager` et `OnUpdatePlayerContext` quand le manager ou le joueur change.

## 10. Énergie (`Assets/Scripts/Behaviours/Energy`)

- **EnergyValue** : une puissance (W) ou une énergie (Wh) qui peut être infinie (`float.PositiveInfinity`). Dans l'Inspector, c'est une valeur suivie d'une case « ∞ ».
- **EnergyDevice** (base) : un appareil du réseau, relié par des câbles. Les câbles s'accrochent à `connector`, ou à défaut à l'enfant `Connector`, ou au haut de l'objet. Détruit avec son objet, il détruit et rend ses câbles.
  - **EnergyConsumer** : consommation en W. Il est alimenté (`IsPowered`, `ev_OnPoweredChanged`) quand son réseau n'est pas disjoncté. Les consommateurs **relient** les réseaux.
  - **EnergyGenerator** : production en W. Il tourne toujours à fond. `Output` est ce qu'il a réellement donné.
  - **EnergyStorage** : capacité, stock (sauvegardé) et débits entrant et sortant.
  - Générateurs et batteries **ne relient pas** les réseaux : reliés des deux côtés, ils appartiennent aux deux.
- **EnergyNetworkManager** (créé au premier appareil, persistant) : il reconstruit les réseaux quand un appareil ou un câble change, et les calcule toutes les 0,2 s.
  - **Réseaux** : un réseau est un groupe de consommateurs reliés par câbles, avec les générateurs et les batteries reliés à eux. Deux générateurs ou batteries reliés directement forment un réseau sans consommateur.
  - **Ordre de calcul** :
    1. Les générateurs reliés à un seul réseau donnent d'abord ce qu'il demande.
    2. Les générateurs partagés couvrent ensuite ce qui manque, au prorata du manque de chaque réseau.
    3. Leur surplus charge les batteries de leurs réseaux, réseaux disjonctés compris : parts égales, dans la limite du débit entrant et de la place.
    4. Les batteries donnent ensuite ce qui manque encore, y compris ce qu'elles viennent de recevoir, dans la limite du débit sortant.
    5. Le reste de la production est perdu.
  - **Disjonction** : un réseau qui manque encore d'énergie disjoncte, et ses consommateurs s'arrêtent. Le calcul est refait sans lui : sa part des générateurs partagés revient aux autres réseaux.
  - **Réarmement** : le réseau se réarme après `rearmDelay` (3 s) dès que ses générateurs et ses batteries peuvent couvrir sa demande.
- **ElectricCable** : objet « runtime » sauvegardé (`cable:<id de l'item>`), avec les ids de ses deux appareils, ses points de passage et le nombre de câbles utilisés.
  - **Tracé** : une courbe de Catmull-Rom qui pend légèrement entre deux points (`LineRenderer`).
  - **Ciblage** : des colliders triggers le long du câble, visés par le mode destruction (`Builder`). Ils ne gênent pas le joueur.
  - **Destruction** : il rend ses câbles au joueur.
- **CablePlacer** (sur le joueur), actif avec un `CableItemData` en main (`Item/Cable` : mètres par item, largeur, matériau) :
  - **Clic gauche** : sur un appareil, il commence le câble. Ailleurs, il ajoute un point de passage. Sur un 2e appareil, il termine le câble.
  - **Clic droit** : il retire le dernier point, ou annule le câble.
  - **Aperçu** : vert si le câble peut être terminé sur l'appareil visé, rouge s'il manque des câbles. Le bandeau indique le nombre de câbles nécessaires et possédés.
  - Deux appareils ne peuvent être reliés qu'une fois.
- **EnergyDeviceInfo** (interactable) : en visant l'objet, affiche l'état de ses appareils.
  - Consommateur : alimenté, réseau disjoncté ou non raccordé, avec sa consommation.
  - Générateur : donné / production.
  - Batterie : stock / capacité et flux.
- **Objets de test** (`Prefabs/Environment/Energy`, matériau `Materials/EnergyTest`) :
  - `26_InfiniteGenerator` : générateur, 1000 W par défaut.
  - `27_TestMachine` : consommateur de 100 W, avec `TestMachine`. Elle est rouge quand elle est alimentée, blanche sinon.
  - `28_TestBattery` : batterie de 1000 Wh, avec des débits de 500 W.
  - `25_ElectricCable` : le câble, 1 par mètre.
  - Inventaire de départ : 99 câbles et 2 générateurs dans la barre d'outils, 3 machines et 2 batteries dans l'inventaire.
