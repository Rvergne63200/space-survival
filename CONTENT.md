# Space Survival : features implémentées

## 1. Bootstrap & chargement de scènes

### Scènes
- `Bootstrap` : contient les objets persistants (managers).
- `0_MainMenu` : menu principal, première scène chargée par le Bootstrapper (`MainMenuUI`).
- `0_World` : le monde. Il contient les capsules (prefab `SpaceCapsule`), avec leur intérieur, et le `PlayerSpawnPoint`.
  - **Coffres de test** de la capsule de spawn (surcharges de l'instance dans la scène, le prefab reste vide) : `Box 1` (29 slots) contient un stack complet de chaque item du jeu (`0` à `28`), `Box 2` un stack complet de chaque item de test (`T001` à `T004`). `Box 3` est vide. Le joueur commence avec un inventaire et une barre d'outils vides.
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
  - **HUD** (`Hud.uxml`, ordre 0) : réticule, texte de l'objet pointé, barres de stats, ramassages, barre d'outils et cadre du slot en main. La **minimap** est en haut à droite (voir plus bas). Seuls les slots de la barre d'outils captent la souris.
  - **Inventaire** (`Inventory.uxml`), **Conteneur** (`Storage.uxml`) et **Fabrication** (`CraftingStation.uxml`), ordre 2 : chacun est un GameObject `Panel` activé à l'ouverture et enregistré comme interface (`InterfaceManager`). Fermé par Échap ou à la mort, il prévient son propriétaire (inventaire fermé, `Searcher.ClearCurrentSearchedStorage`, `Crafter.ClearStation`).
  - **Item au curseur** (`HeldItem.uxml`, ordre 4) : au-dessus des interfaces, ne capte pas la souris.
- Modèles instanciés par le code : `ItemSlot.uxml`, `StatBar.uxml`, `PickupInfo.uxml`, `RecipeSlot.uxml`, `RecipeIngredient.uxml`. Les scripts retrouvent les éléments par leur nom.
- Sans joueur (menu principal), le HUD et l'item au curseur sont masqués (`ParentUI.SetHudVisible`).
- **Minimap** (`MinimapUI`, sur le GameObject HUD, éléments `minimap` de `Hud.uxml`, dossier `Scripts/Behaviours/UI/Map`) :
  - Ronde, en haut à droite. Elle tourne avec la vue : ce qui est devant le joueur est en haut, et un « N » rouge suit le bord pour indiquer le nord.
  - **Rendu** : stylisé comme la carte du panneau de contrôle (`MapUtility`, partagé avec `NetworkMapElement`), sans le réseau électrique. Relief en teintes avec ombrage et courbes de niveau tous les 4 m (lignes vectorielles lissées, `MapUtility.DrawContours`), grille tous les 25 m, contours des bâtiments et couloirs (bleu) et des objets posés (orange), vus de dessus.
  - **Tuile** : le relief est calculé depuis la seed sur un carré de `tileFactor` (3) × `radius` de côté, en `topographyResolution` (256) pixels. Il est recalculé quand le joueur s'approche de son bord. L'élément `minimap-image` est déplacé et tourné autour du joueur. Les contours sont recollectés toutes les `refreshInterval` (1 s).
  - Une flèche jaune au centre représente le joueur. Le rayon affiché (`radius`) est fixe et réglable dans l'Inspector (50 m, sur le prefab `UI`). Légèrement transparente (`opacity` 0,8 dans `Game.uss`). Fond plus clair et plus transparent que la carte du panneau de contrôle (`lowColor`, `highColor`, `backgroundAlpha` 0,5, et fond du cercle à 25 %), lignes et contours inchangés.
  - Elle est masquée sans joueur ou à la mort.


### Sauvegarde (`SaveManager`, dans `Bootstrap`)
- Un fichier JSON par partie dans `Application.persistentDataPath/Saves`, choisi dans le menu principal (`NewGame`, `LoadGame`, `DeleteSave`, `ListSaves`). Sauvegarde à la fermeture du jeu, avant un changement de scène et toutes les `autosaveInterval` secondes (120) dans le monde. Restauration au chargement du monde.
  - Une nouvelle partie repart de l'inventaire et des stats de départ (capturés au lancement du jeu), même après une autre partie dans la même session.
  - Monde lancé directement dans l'éditeur, sans passer par le menu : une sauvegarde « Partie éditeur » est créée.
  - Version du format (`SaveData.CurrentVersion`, 2) : une sauvegarde d'une version plus ancienne est ignorée (absente du menu, avertissement dans la console). La version 2 est arrivée avec les ids d'items en texte et la renumérotation des items.
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
- **Saut** : vitesse verticale calculée pour atteindre `jumpHeight` (1.1 m, ~0.67 s en l'air). Possible si le joueur est au sol et après un délai (`jumpCooldown`). Il ne consomme pas d'endurance.
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
- **Consommables de test** : `6_Ration` (faim +40) et `7_Water` (soif +50), dans les coffres de test de la capsule (section 1).
- **First Aid Kit** (`23_FirstAidKit`) : consommable, vie +50 d'un coup.
- **Consommables** : `ConsummableData` (ScriptableObject, hérite d'ItemData) contient une liste de modifiers (`SerializeReference` + SubclassPicker), appliqués à la consommation depuis `PlayerInventory`.
- **UI** : `PlayerStatsUI` crée une barre (`StatBarUI`, `StatBar.uxml`) par stat de `statUIDataCollection` (`StatUIData` : stat, icône, couleur), en bas à gauche du HUD. Chaque barre affiche « valeur/max » et un remplissage proportionnel.

## 4. Inventaire

### Données
- **ItemData** (ScriptableObject `Item/Item`) : `Id` (texte saisi à la main, unique : un doublon ou un id vide est signalé par le SaveManager au lancement), `StackMax` (99 par défaut), `Name`, `Description`, `Sprite` (icône UI) et `Model` (prefab 3D utilisé au drop). Sous-classes :
  - `ConsummableData` (consommé au clic droit, voir feature 3), `PlaceableItemData` (et `CableItemData`, section 10).
  - `UsableItemData` (abstraite) : un item qui peut avoir une barre (`Gauge`, voir plus bas). Deux sous-classes :
    - `ToolData` (`Item/Tool`) : un outil utilisé en main, avec ses actions au clic gauche (`PrimaryActions`) et au clic droit (`SecondaryActions`), et la consommation de sa barre : mode `Continuous` (par seconde tant que l'outil est actif, en W pour une charge, en L/s ou unités/s sinon) ou `PerUse` (montant fixe à chaque utilisation), quelle que soit l'unité. Item actuel : `22_Flashlight`.
    - `EquipmentData` (`Item/Equipment`) : un item qui se porte dans un emplacement d'équipement (`Slot` : `Head`, `Body`, `Back`, `Feet`, `Hands`, `OxygenTank`). Les bouteilles d'oxygène ont leurs propres emplacements, à part du dos, pour porter un jetpack et de l'oxygène à la fois. Items actuels : `18_Jetpack` (dos) et `17_OxygenTank` (bouteille).
    - `JetpackData` (`Item/Jetpack`, hérite d'`EquipmentData`) : poussée (`Thrust`, m/s² en plus de ce qui annule la gravité du joueur), vitesse de montée max (`MaxRiseSpeed`) et consommation pendant la poussée (`Consumption`, W pour une charge). Voir « Équipement » plus bas.
- **Ids et fichiers** : un item est nommé `<id>_<Nom>`. Les items du jeu (`ScriptableObjects/Items`) sont numérotés à la suite, sans trou, de `0` à `28`. Les items qui ne servent qu'aux tests (`ScriptableObjects/TestItems`) ont un id `T` suivi de 3 chiffres (`T001` à `T004` : générateur infini, machine et batterie de test, voir section 10, et `Test Potion`, consommable qui retire de la vie).
- **Objets** (`ScriptableObjects/Items` ; 25 à 28 : câble, tableau de distribution, panneau de contrôle et station de recharge, voir section 10) : ressources `Rock`, `Wood Log`, `Branch`, `Plant Fibers`, `Iron Ore`, `Iron`, `Coal`, `Copper Ore`, `Titanium Ore`, `Titanium`, `Ice`, `String`, `Circuit Board`, `Microchip`, `Glass`, `Glue` ; consommables `Ration`, `Water`, `First Aid Kit` ; plaçables `Crate`, `Radio Relay` (prefab `RadioRelais`, sans effet pour l'instant) et `Crafter` (prefab `Crafter`, `CraftingStation` sans recette pour l'instant) ; `Oxygen Tank` et `Jetpack` sans effet pour l'instant, `Flashlight` (pile de 1). Chaque objet doit être dans la liste `items` du SaveManager (Bootstrap).
- **Item** : un ItemData (`Item.CreateFromData`) et la valeur de sa barre (`Gauge`), propre à chaque objet.
- **Barre** (`UsableItemData.Gauge`, `ItemGauge`) des outils et équipements : case `enabled` (décochée : pas de barre, quelle que soit la consommation), unité (`unit`) et capacité. L'unité ne change que l'affichage et la façon de remplir la barre :
  - **Électricité** (`Energy`, Wh) : l'objet neuf est vide. À 0, il est inutilisable (ses actions s'éteignent, « déchargé ») jusqu'à sa recharge à la station de recharge (section 10).
  - **Oxygène** (`Oxygen`, L) : l'objet neuf est vide. À 0, il est inutilisable (« vide »). Il se remplira dans un appareil à oxygène, qui reste à faire (rechargeur de bouteilles, F52).
  - **Usure** (`Wear`, unités) : l'objet neuf est plein et ne se recharge pas. À 0, il est détruit (« cassé »).
  - **Consommation** : pour l'instant, seuls les outils vident leur barre (`ToolData`). Un outil est **actif** quand une action `Toggle` est allumée ou qu'une action `Hold` est en cours (`PlayerInventory.UpdateGauge`). Une utilisation est un clic dont l'action a été faite.
  - Un objet avec une barre ne s'empile pas (`StackMax` vaut 1) : chaque slot en garde une copie (`Item.Copy` dans `ItemSlot.Add`), et la valeur suit l'objet quand il change de slot ou est jeté. Elle est sauvegardée avec les slots (`ItemSlotSaveData.gauge`) et les objets au sol (`Pickupable`).
  - **Affichage** : une barre en bas du slot, bleue pour une charge, cyan clair pour l'oxygène, verte puis orange (sous 50 %) puis rouge (sous 20 %) pour une usure. L'infobulle affiche « Charge », « Oxygène » ou « Usure » avec la valeur dans son unité.
  - Objets actuels : `22_Flashlight` (outil, 10 Wh, 5 W en continu quand elle est allumée, soit 2 h) et `18_Jetpack` (équipement, 50 Wh, 2000 W en vol, soit 90 s). La barre de `17_OxygenTank` est en oxygène mais désactivée, ses valeurs restent à définir.
- **ItemSlot** : un item et une quantité. `Add` respecte `StackMax` et renvoie le reste, `Remove` renvoie ce qui manque. `Switch` échange le contenu avec un autre slot. `Drop` passe par `Pickupable.Spawn`, qui instancie le `Model` devant la source avec un `Pickupable` déjà identifié et un Rigidbody qui reçoit une petite impulsion. Le slot n'est vidé que si le spawn réussit (modèle valide avec un `Pickupable`).
- **ItemCollection** : liste de slots de taille fixe (`MaxSize`). `Add` complète d'abord les stacks non pleins puis remplit les slots vides, et renvoie le surplus. `RemoveItem` retire en partant du dernier slot. `Count` compte la quantité totale d'un item. Event `ev_Update`.
- **ItemStorage** (MonoBehaviour) : encapsule une ItemCollection. Sert de base au PlayerInventory et aux conteneurs.

### PlayerInventory (sur le PlayerManager, donc persistant)
- Inventaire de **32 slots** et barre d'outils de **6 slots**.
- `Add` : un slot qui contient déjà le même item est prioritaire (barre d'outils, puis inventaire), puis les slots vides de la barre d'outils, puis ceux de l'inventaire. `Remove` retire de l'inventaire, puis de la barre d'outils.
- Touche Inventaire (I ou Tab) : ouvre ou ferme l'inventaire, avec l'équipement sous la grille (`ev_UpdateIsOpen`).
- **Équipement** (`Equipment`, 7 slots, sauvegardé avec le joueur et lâché dans le sac à la mort) : tête, corps, dos, mains, pieds et deux bouteilles d'oxygène (`EquipmentLayout`). Chaque slot n'accepte que les `EquipmentData` de son emplacement, un seul à la fois (`ItemSlot.SetRestriction` : `Accepts`, `Limit`). Affiché sous l'inventaire avec le nom de chaque emplacement (`PlayerInventoryContentUI`), les mêmes clics que les autres slots. Shift + clic : un équipement de l'inventaire ou de la barre d'outils va dans son emplacement libre, un équipement porté revient dans l'inventaire (puis la barre d'outils). `WornSlot(emplacement)` donne le slot porté.
  - **Jetpack** (`PlayerMovement.ApplyJetpack`) : porté sur le dos, la touche Saut maintenue en l'air annule la gravité du joueur et pousse vers le haut jusqu'à `MaxRiseSpeed`, en vidant sa barre (`Consumption`). Vide, il ne pousse plus (notification) jusqu'à la recharge à la station.
  - La consommation d'oxygène des bouteilles attend le réseau d'oxygène (F27, F51).
- Molette : change le slot en main (`HandedIndex`, avec bouclage). Sans effet quand une interface (inventaire, panneau de contrôle…) ou un menu est ouvert.
- **Usage de l'item en main** : pour un outil, `Use1` (clic gauche) exécute ses actions principales et `Use2` (clic droit) ses actions secondaires (`ToolData.PrimaryActions` / `SecondaryActions`, des `ItemAction` comme `ApplyModifiersAction`). Sans action secondaire, un `ConsummableData` est consommé au clic droit. Les deux sont ignorés quand une interface est ouverte ou en mode construction (`OPTION_USE_DISABLED`).
  - **Modes des actions** (`ItemAction.Mode`) : les outils s'utilisent au clic gauche.
    - `Press` (`ItemAction`) : `Execute` une fois par clic (arme qui tire, consommable).
    - `Toggle` (`ToggleItemAction`) : chaque clic alterne allumé / éteint (`SetOn`). L'action s'éteint quand l'item quitte la main.
    - `Hold` (`HoldItemAction`) : `Execute` démarre, `Hold(context, heldTime)` est appelé à chaque frame jusqu'à ce qu'il renvoie false, puis `End(context, completed)`. Le relâchement du bouton, l'ouverture d'une interface ou le changement d'item arrêtent l'action (`completed` = false).
  - **Item en main** : `PlayerInventory` vérifie à chaque frame si l'item en main change (autre slot, slot vidé, joueur absent). Il appelle alors `OnUnequip` sur les actions de l'ancien outil (`ToolData.Unequip`).
  - `ItemUseContext.View` : le transform de la caméra du joueur.
  - L'état d'une action (allumée, maintenue) est porté par l'asset ToolData : il est partagé par tous les items de ce type.
- **Lampe torche** (`22_Flashlight`, `FlashlightAction`) : un spot (couleur, intensité 1,5, portée 30 m, angle 60°, ombres douces) créé comme enfant de la caméra au premier allumage.
- Touche Drop (inventaire ouvert) : jette devant le joueur tout le contenu du slot tenu au curseur. Le slot du curseur est aussi jeté automatiquement quand la dernière interface se ferme.

### Interactions UI
- Système par **clic**, sans vrai glisser-déposer. Les clics sont gérés par `ItemSlotUI` (`PointerDownEvent` sur l'élément `slot`). Un clic gauche sur un slot (`TryMerge`) prend son contenu dans le `draggingSlot`, qui suit la souris (`HeldItemUI`). Un nouveau clic le dépose.
  - Même item : fusion des stacks, le surplus reste au curseur.
  - Item différent : échange (Switch).
- **Infobulle** (`ItemTooltip`) : au survol d'un slot, le nom, la description et la valeur de la barre de l'objet (« Charge : 3,2 / 10 Wh », mise à jour en direct). Un élément par UIDocument, créé au premier survol.
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
- **StructureData** (ScriptableObject, Construction/Structure) : id (sauvegardes), nom, icône, prefab, coût (`Cost`), ressources rendues à la destruction (`Refund`, n'importe quels items), aplanissement, pente max. **StructureCatalog** : la liste des structures (roue et sauvegarde). Structures actuelles : `BaseDom`, dôme de 8 m de diamètre, base de 8,8 m (20 fer, 30 bois ; rend 10 fer), et le couloir (voir plus bas).
- **Layer `Building`** : à l'instanciation d'une structure terminée (`BuildUtility.InstantiateStructure`), ses parties sur le layer `Default` passent sur `Building`. La minimap ne s'en sert plus (elle dessine les contours des structures). Les requêtes physiques ne filtrent pas ce layer : rien ne change pour les collisions et la visée.
- **BuildingInterior** (enfant `Interior` du prefab `BaseDom`) : l'intérieur du bâtiment, un collider trigger (sphère de 4 m de rayon, comme le dôme) sur le layer `Ignore Raycast` pour ne gêner ni la visée ni la pose. `Contains(point)` dit si un point est dedans. Sert à l'électricité du bâtiment (section 10) et à la pose des couloirs (bord du bâtiment), plus tard à l'oxygène et au joueur à l'intérieur (F51, F55). Le dôme lui-même est désactivé pour pouvoir entrer, en attendant un vrai modèle.
- **Couloir** (`CorridorData` : hérite de `StructureData`, longueurs min et max ; `Corridor` ; prefab `Prefabs/Build/Corridor`, asset `ScriptableObjects/Structures/Corridor`, dans le catalogue) :
  - **Pose** (`Builder`, mode couloir) : choisi dans la roue, on clique un premier bâtiment (structure finie avec un `BuildingInterior`, visé jusqu'à 30 m), puis on vise le second. Un couloir droit est tracé entre leurs bords, face à face (`Corridor.ComputePlacement` : bord à `BuildingInterior.Radius` du centre, à la hauteur du sol des bâtiments). Le bandeau donne la longueur, le coût et ce qui bloque. Clic gauche : pose le plan.
  - **Validité** : bâtiments pas déjà reliés, longueur entre 2 et 20 m, dénivelé entre les deux bâtiments ≤ `MaxSlope` (2 m), rien sur le passage (le terrain et les deux bâtiments exceptés). Le terrain n'est pas aplani.
  - **Modèle provisoire** : sol (à la hauteur de la base du dôme), deux murs et un toit, 3 m de large et 3,2 m de haut. Le prefab fait 1 m de long, il est étiré selon Z.
  - **Coût et remboursement par mètre** (arrondi au-dessus) : 2 fer et 1 bois par mètre, rend 1 fer par mètre (`CostMultiplier` sur `StructureBlueprint` et `BuiltStructure`).
  - **Sauvegarde** : comme les autres structures (`build:Corridor`, `blueprint:Corridor`), avec les ids de ses deux bâtiments et sa longueur (composant `Corridor`).
  - **Destruction** : directement, ou avec un de ses deux bâtiments (plan compris) : il rend ses ressources.
- **StructureBlueprint** (plan) : **hologramme bleu** (shader `Custom/Hologram` : lueur additive, bords lumineux, lignes de balayage animées, léger scintillement ; matériau `Resources/Materials/Blueprint`). Il a un collider pour être visé, sur le layer `Blueprint`, qui n'entre pas en collision avec le joueur : on le traverse (matrice de collision, et `Physics.IgnoreLayerCollision` par sécurité). La détection du sol du joueur l'ignore aussi. Pour interagir avec le plan, il faut le viser depuis l'extérieur. Interagir dépose tout ce qui manque et qu'on a dans l'inventaire. Le texte affiche l'avancement (« Dôme (plan) : Iron 12/20, WoodLog 4/30 »). Une fois tout apporté, il devient la structure réelle (`BuiltStructure`). Détruit, il rend ce qui lui a été apporté.
- **PlaceableItemData** (ItemData, Item/Placeable) : `placedPrefab` et `Refund`, rendu à la destruction (vide : l'item lui-même). Le contenu des conteneurs d'un objet détruit va aussi dans l'inventaire. Item actuel : `8_Crate` (caisse de 20 slots).
- **Matériaux de test** : les coffres de la capsule de spawn (section 1) contiennent un stack complet de chaque item, de quoi construire.
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
- **TerrainGenerator** (sur le prefab `Chunk`) : duplique le `TerrainData`. Le terrain utilise le shader `Terrain/World UV Standard` : les textures sont positionnées depuis la position monde, donc continues entre les chunks. Il calcule la heightmap (résolution `size+1`, hauteur max 1024) **en asynchrone** (`Task.Run`). Le bruit combine plusieurs couches de Perlin : variation locale, variation globale, montagnes et creux. Émet `ev_OnTerminateGeneration` à la fin. `HeightAt` donne la hauteur générée en un point du bruit, et `ChunkManager.GeneratedHeightAt(x, z)` en une position du monde, chargée ou non (sans les aplanissements). La carte du panneau de contrôle s'en sert pour le relief.
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
  - **EnergyConsumer** : consommation en W. Il est alimenté (`IsPowered`, `ev_OnPoweredChanged`) quand son réseau n'est pas disjoncté.
  - **EnergyGenerator** : production en W. Il tourne toujours à fond. `Output` est ce qu'il a réellement donné.
  - **EnergyStorage** : capacité, stock (sauvegardé) et débits entrant et sortant.
  - **On / off** (`SwitchedOn`, `SetSwitchedOn`, sauvegardé) : éteint, un consommateur ne consomme plus (état « éteinte »), un générateur ne produit plus, une batterie ne charge ni ne décharge. Le câble passe toujours : un appareil éteint relie toujours son réseau. Les ports d'un tableau de distribution ne s'éteignent pas (`CanSwitch`), le tableau a sa propre coupure.
    - **Tableau éteint** (règle 6) : son circuit est éteint de force (`ForcedOff`, recalculé à chaque mise à jour par `EnergyNetworkManager.UpdateForcedOff`). Un appareil ne marche que si son réglage est sur on **et** si aucun tableau au-dessus de lui n'est éteint (`IsOn`). Son réglage est gardé, et bloqué tant qu'il est éteint de force (« éteint par le tableau »).
- **EnergyNetworkManager** (créé au premier appareil, persistant) : il reconstruit les réseaux quand un appareil ou un câble change, et les calcule toutes les 0,2 s. **Règles** (les mêmes dans l'en-tête du script) :
  1. **Réseau** : tout ce qui est relié (câbles, intérieur d'un bâtiment, couloirs) forme un réseau. Seuls les tableaux séparent les réseaux : leur entrée est un consommateur du réseau du dessus, leur sortie un générateur de leur circuit, servie après les générateurs du circuit. Sens unique.
  2. **Demande d'un tableau** : obligatoire (les machines de son circuit) et facultative (la recharge de ses batteries, les prises de sortie de bâtiments derrière lui), dans sa limite de puissance. Le facultatif n'est servi que par le surplus, avant les batteries du dessus, et ne fait jamais disjoncter.
  3. **Bâtiment** : un tableau dont le circuit est son intérieur (`BuildingPowerPanel`). Sa prise de sortie est un tableau intérieur dont toute la demande est facultative (`BuildingOutlet`). Un couloir relie les entrées de deux bâtiments.
  4. **Chaque réseau** : ses générateurs donnent ce qu'il demande, leur surplus charge ses batteries (parts égales, dans la limite du débit entrant et de la place, réseau disjoncté compris), les batteries donnent ce qui manque encore (dans la limite du débit sortant). Le reste est perdu.
  5. **Manque** : une fois les flux stabilisés, un réseau qui manque encore d'énergie déleste ses tableaux réglés sur « manque d'énergie » (priorité la plus faible en premier), puis disjoncte : ses consommateurs s'arrêtent. Il se réarme après `rearmDelay` (3 s) dès qu'il pourrait être alimenté.
  6. **Coupure** : un tableau éteint éteint son circuit et les circuits des tableaux qui y sont, sauf les tableaux intégrés à un bâtiment, qui ne sont plus qu'alimentés.
  7. **Redémarrage** : un tableau sans entrée ou délesté repart quand l'amont pourrait de nouveau le servir. Un tableau intégré à un bâtiment compte aussi sur ce que les tableaux du dessus pourraient obtenir plus haut.
- **ElectricCable** : objet « runtime » sauvegardé (`cable:<id de l'item>`), avec les ids de ses deux appareils, ses points de passage et le nombre de câbles utilisés.
  - **Tracé** : une courbe de Catmull-Rom qui pend légèrement entre deux points (`LineRenderer`).
  - **Ciblage** : des colliders triggers le long du câble, visés par le mode destruction (`Builder`). Ils ne gênent pas le joueur.
  - **Destruction** : il rend ses câbles au joueur.
- **CablePlacer** (sur le joueur), actif avec un `CableItemData` en main (`Item/Cable` : mètres par item, largeur, matériau) :
  - **Clic gauche** : sur un appareil, il commence le câble. Ailleurs, il ajoute un point de passage. Sur un 2e appareil, il termine le câble.
  - **Clic droit** : il retire le dernier point, ou annule le câble.
  - **Aperçu** : vert si le câble peut être terminé sur l'appareil visé, rouge s'il manque des câbles. Le bandeau indique le nombre de câbles nécessaires et possédés.
  - Deux appareils ne peuvent être reliés qu'une fois.
- **EnergyDeviceInfo** (interactable) : en visant l'objet, affiche l'état de ses appareils. La touche d'interaction les allume ou les éteint.
  - Consommateur : alimenté, réseau disjoncté, non raccordé ou éteinte, avec sa consommation.
  - Générateur : donné / production (« éteint » s'il l'est).
  - Batterie : stock / capacité et flux (« éteinte » si elle l'est).
- **Tableau de distribution** (`DistributionPanel`, sur la racine du prefab `Prefabs/Environment/Energy/DistributionPanel`) :
  - **Ports** : deux ports enfants. `DistributionPanelInput` (`EnergyConsumer`, cube vert) est un consommateur du réseau amont. `DistributionPanelOutput` (`EnergyGenerator`, cube orange) est un générateur de son circuit.
  - **Branchement** : un câble se branche sur le port le plus proche du point visé (`EnergyDevice.FromHit`). Les câbles sauvegardent le port (`PortId`). L'entrée et la sortie d'un même tableau ne peuvent pas être reliées.
  - **Sens unique** : rien ne remonte vers l'amont. La sortie est servie **après** les générateurs du circuit, et son surplus charge les batteries du circuit après le leur.
  - **Calcul** (`EnergyNetworkManager.Simulate`) :
    1. **Demande** : les sorties donnent sans limite, et ce qu'elles donnent est la demande du circuit (`Request`). Ce qu'elles donnent aux consommateurs est **obligatoire** (`Request`), ce qu'elles donnent aux batteries est **facultatif** (`ChargeRequest`). Pour un tableau sous un autre, la demande de ses machines compte dans la demande du tableau du dessus, et sa recharge (plafonnée par sa puissance max) dans la recharge du tableau du dessus. Le calcul est répété pour les tableaux imbriqués. Les réseaux disjonctés comptent aussi, pour pouvoir se réarmer. Les demandes sont recalculées après les décisions des tableaux quand l'un d'eux démarre ou s'arrête : un circuit qui démarre sous un autre ne le fait plus disjoncter (`ComputeDemands`).
    2. **Décisions** du tableau (`BeginUpdate`) : on/off, fusible (demande au-dessus de la limite), délestage sur batteries amont (sous le seuil, réarmé 5 % au-dessus), démarrage différé.
    3. **Distribution** : l'entrée demande la part obligatoire à l'amont. La part facultative (la recharge, dans la limite de puissance) est servie comme une consommation, avant la recharge des batteries amont, mais ne fait pas disjoncter si elle n'est pas couverte entièrement. Elle est aussi servie par les batteries du réseau d'entrée, une fois leurs manques couverts (dans la limite de leur débit sortant, stock compris). La sortie donne au circuit la part obligatoire plus ce qui a été reçu pour la recharge. Un réseau en manque déleste d'abord ses tableaux réglés sur « manque d'énergie » (priorité la plus faible en premier), puis disjoncte. Un tableau dont le réseau d'entrée disjoncte ne donne plus rien. Les réseaux ne disjonctent (ou ne délestent) qu'une fois la part facultative arrivée partout : elle descend d'un tableau par passe.
    4. **Après la distribution** (`EndUpdate`) :
       - un tableau délesté ou sans alimentation repart quand l'amont pourrait de nouveau le servir ;
       - pour un tableau délesté, après 3 s ;
       - puis il attend son démarrage différé.
  - **États** : en marche, éteint (ou éteint par le tableau du dessus), démarrage, pas d'alimentation en entrée, sauté, délesté, boucle (la sortie alimente le réseau de sa propre entrée, par exemple une batterie ou un générateur câblé à la fois sur l'amont et sur le circuit).
  - **Sauvegarde** : les réglages et l'énergie transmise au total.
  - **Limite** : un tableau de distribution délesté ne se réarme que sur les sources de son réseau d'entrée (pas sur ce que l'amont d'un autre tableau pourrait donner en plus). Les tableaux intégrés aux bâtiments n'ont pas cette limite (règle 7).
- **DistributionPanelUI** (`UI.prefab`, `DistributionPanel.uxml`, style dans `Game.uss`) : ouverte en interagissant avec le tableau. C'est une interface, fermée par Échap.
  - L'état et les réglages sont dans `DistributionPanelSettings.uxml` (`DistributionPanelSettingsView`), repris par le panneau de contrôle.
  - **Affichage** : état, demande du circuit (ses consommateurs seulement), puissance transmise, puissance réellement envoyée en recharge des batteries du circuit (`ChargeSupplied`), énergie transmise. Le texte au pointeur affiche aussi la part en recharge. La part envoyée aux prises de sortie de bâtiments en aval est affichée à part (« Sortie », `OutletSupplied`), ni dans la demande du circuit ni dans la recharge.
  - **Réglages** : bouton Éteindre / Allumer (voir « Tableau éteint » plus haut ; bloqué si un tableau au-dessus est éteint), limite de puissance (à 0 W, elle coupe le circuit) (ou illimitée), délestage (aucun, manque d'énergie en amont, batteries en amont sous le seuil), seuil des batteries, priorité, démarrage différé.
- **Électricité du bâtiment** (`BuildingPowerPanel`, hérite de `DistributionPanel`, sur l'enfant `PowerSocket` du prefab `BaseDom`) : le tableau intégré au bâtiment, dont le circuit est l'intérieur (règle 3). Mêmes réglages et même interface qu'un tableau de distribution (`DistributionPanelUI`, touche d'interaction sur la prise).
  - **Prise** : un poteau gris de 1,5 m au bord de la base (côté +Z), avec deux ports câblables : l'entrée (cube vert, en haut) et la prise de sortie (cube orange, en dessous). Le câble se branche sur le port le plus proche de la visée.
  - **Intérieur** : la sortie du tableau (port caché `interior`) est reliée sans câble à tout ce qui est posé dans le bâtiment (`BuildingInterior.Contains` sur la position de l'appareil), sauf les sorties d'autres tableaux et les ports du bâtiment (`EnergyNetworkManager.BuildBuildingLinks`). `EnergyNetworkManager.Links` donne les voisins d'un appareil : câbles, intérieur des bâtiments et couloirs.
  - **Prise de sortie** (`BuildingOutlet`, port orange `output`) : un tableau caché dans le bâtiment, entrée (`outletInput`) reliée à l'intérieur, sortie câblée dehors. Toute sa demande est facultative (`OptionalDemand`, règle 2) : elle ne renvoie que le surplus, avant les batteries du bâtiment, et ce qui est derrière manque de courant avant le bâtiment.
  - **Tableau intégré** (`DistributionPanel.Building` : le bâtiment et sa prise de sortie) : jamais éteint par un tableau au-dessus (règle 6), pas d'icône sur la carte, et pour repartir il compte aussi sur l'amont plus lointain (règle 7, `EnergyNetworkManager.Potential`).
  - **Câbles** (`CablePlacer`) : deux ports d'un même bâtiment ne se relient pas, ni les entrées de deux bâtiments (on passe par la prise de sortie ou un couloir).
  - **Couloir** (`Corridor`, voir section 7) : il relie sans câble les entrées de ses deux bâtiments (règle 1). Chaque bâtiment garde son tableau : couper B1 ne coupe pas B2, la limite de B1 ne concerne que B1. Une machine posée dans le couloir n'est reliée à rien (il faut la câbler).
  - Exemples : `B1 ---> B2` (prise de sortie de B1) : B2 reçoit le surplus de B1, un générateur de B2 n'alimente pas B1 ; couper B1 prive B2 d'alimentation sans l'éteindre. `B1 ---> B2 ---- G1` : couper B1 n'éteint pas G1, qui alimente B2. Un tableau de distribution posé dans un bâtiment est alimenté par le bâtiment ; une machine posée dedans est toujours dans le circuit du bâtiment.
- **Panneau de contrôle** (`ControlPanel` et un `EnergyConsumer` de 20 W, sur le prefab `Prefabs/Environment/Energy/ControlPanel`) :
  - Il se branche par câble sur un réseau. Il ne s'ouvre (touche d'interaction) que s'il est alimenté, et se ferme s'il ne l'est plus. Éteint depuis sa propre carte, il se rallume en interagissant avec lui.
  - **Portée** (`CollectNetwork`), quatre règles :
    1. Il voit son réseau, et tout ce qui est en aval : derrière l'entrée de chaque tableau rencontré, bâtiments compris.
    2. Il ne voit jamais ce qui est derrière l'entrée d'un tableau qui alimente ce qu'il voit (l'amont).
    3. Dans un bâtiment, son réseau est celui de son groupe de couloirs (`Corridor.Group`), avec le réseau qui alimente ce groupe.
    4. Sur ce réseau d'alimentation, les autres bâtiments sont vus de l'extérieur (contour, câble, réglages de leur tableau), pas leur intérieur.
    - Exemples : `PD1 ---> G1 ---> B1`, panneau dans B1 : G1, PD1 et ce que PD1 alimente d'autre, pas ce qui alimente PD1 (3, 2). `G1 ---> B1 ---> B2`, panneau dans B2 : B1 de l'extérieur, pas G1 (4, 2). `G1 ---- B1 ==== B2 ---> B3`, panneau dans B1 : G1, B1, B2 et B3 avec leur contenu (3, 1). `PD1 ---> B1` et `PD1 ---> B5`, panneau dans B1 : B5 de l'extérieur (4).
  - **ControlPanelUI** (`UI.prefab`, `ControlPanel.uxml`) : une interface, fermée par Échap. À gauche la carte, à droite l'appareil sélectionné. Bouton « Recentrer ».
  - **Carte** (`NetworkMapElement`), nord en haut :
    - Relief stylisé : teinte selon la hauteur, ombrage des pentes et courbes de niveau tous les 4 m (calculées en marching squares sur la grille des hauteurs et tracées en vectoriel, non pixélisées) (une sur 5 plus marquée), calculés depuis la seed (`ChunkManager.GeneratedHeightAt`) pour la zone affichée. Une grille par-dessus.
    - Bâtiments (`BuiltStructure`) : leur forme vue de dessus (enveloppe convexe des sommets de leurs meshes : dôme rond, couloir orienté).
    - Câbles : lignes lumineuses, cyan si leur réseau est alimenté, cyan pâle si leur réseau n'a aucun consommateur, rouges s'il est disjoncté, grises sinon (`EnergyNetworkManager.CableState`).
    - Icônes : carré pour une machine, rond pour un générateur, rectangle plat pour une batterie (la partie sombre, à droite, est la place libre), triangle pour un panneau de contrôle, losange pour un tableau de distribution. Pas d'icône pour le tableau intégré à un bâtiment : c'est le bâtiment qui se sélectionne. Le panneau utilisé est entouré d'un anneau qui pulse.
    - Trois couleurs seulement : bleu en marche, rouge disjoncté (tableau sauté, délesté, sans alimentation en entrée ou en boucle), gris éteint ou non raccordé (tableau en démarrage). Générateurs et batteries : selon leur réseau.
    - Bâtiments : surlignés au survol (et quand leur tableau est sélectionné), leur nom en infobulle. Un clic sélectionne leur tableau intégré, s'il est dans le réseau affiché.
    - Molette : zoom autour du curseur. Glisser : déplacer. Survol : nom de l'appareil ou du bâtiment. Clic : sélection.
  - **Sélection** : pour une machine, un générateur ou une batterie, ses informations et un bouton Allumer / Éteindre. Pour un tableau, tous ses réglages, comme dans son interface.
- **Station de recharge** (`ChargingStation`, un `EnergyConsumer`, et un `ItemStorage` d'un slot, prefab `Prefabs/Environment/Energy/ChargingStation`, item `28_ChargingStation`) :
  - Interagir avec elle ouvre son slot, comme un conteneur. Le texte au pointeur indique la charge de l'objet déposé.
  - Alimentée, elle recharge l'objet électrique (barre en Wh) déposé avec `chargePower` (100 W). Elle ne demande cette puissance à son réseau que tant que l'objet n'est pas plein, et rien sinon.
- **Items du réseau** (`ScriptableObjects/Items`) :
  - `25_ElectricCable` : le câble, 1 par mètre.
  - `26_DistributionPanel` : le tableau de distribution.
  - `27_ControlPanel` : le panneau de contrôle.
  - `28_ChargingStation` : la station de recharge.
- **Objets de test** (`ScriptableObjects/TestItems` ; prefabs dans `Prefabs/Environment/Energy`, matériau `Materials/EnergyTest`) :
  - `T001_InfiniteGenerator` : générateur, 1000 W par défaut.
  - `T002_TestMachine` : consommateur de 100 W, avec `TestMachine`. Elle est rouge quand elle est alimentée, blanche sinon.
  - `T003_TestBattery` : batterie de 1000 Wh, avec des débits de 500 W.
