---
tags: [jeu-colonie, fonctionnalite, inventaire, items]
---

# Données des items

- **ItemData** (ScriptableObject `Item/Item`) : `Id` (texte saisi à la main, unique : un doublon ou un id vide est signalé par le SaveManager au lancement), `StackMax` (99 par défaut), `Name`, `Description`, `Sprite` (icône UI) et `Model` (prefab 3D utilisé au drop). Sous-classes :
  - `ConsummableData` (consommé au clic droit, voir [[Modificateurs et consommables]]), `PlaceableItemData` (voir [[Objets posés]]) et `CableItemData` (voir [[Câble électrique]]).
  - `UsableItemData` (abstraite) : un item qui peut avoir une barre (`Gauge`, voir [[Barre des objets]]). Deux sous-classes :
    - `ToolData` (`Item/Tool`) : un outil utilisé en main, avec ses actions au clic gauche (`PrimaryActions`) et au clic droit (`SecondaryActions`), et la consommation de sa barre : mode `Continuous` (par seconde tant que l'outil est actif, en W pour une charge, en L/s ou unités/s sinon) ou `PerUse` (montant fixe à chaque utilisation), quelle que soit l'unité. Item actuel : `22_Flashlight`.
    - `EquipmentData` (`Item/Equipment`) : un item qui se porte dans un emplacement d'équipement (`Slot` : `Head`, `Body`, `Back`, `Feet`, `Hands`, `OxygenTank`). Les bouteilles d'oxygène ont leurs propres emplacements, à part du dos, pour porter un jetpack et de l'oxygène à la fois. Items actuels : `18_Jetpack` (dos) et `17_OxygenTank` (bouteille).
    - `JetpackData` (`Item/Jetpack`, hérite d'`EquipmentData`) : poussée (`Thrust`, m/s² en plus de ce qui annule la gravité du joueur), vitesse de montée max (`MaxRiseSpeed`) et consommation pendant la poussée (`Consumption`, W pour une charge). Voir [[Équipement et jetpack]].
- **Ids et fichiers** : un item est nommé `<id>_<Nom>`. Les items du jeu (`ScriptableObjects/Items`) sont numérotés à la suite, sans trou, de `0` à `32`. Les items qui ne servent qu'aux tests (`ScriptableObjects/TestItems`) ont un id `T` suivi de 3 chiffres (`T001` à `T004` : générateur infini, machine et batterie de test, voir [[Objets du réseau électrique]], et `Test Potion`, consommable qui retire de la vie).
- **Objets** (`ScriptableObjects/Items` ; 25 à 30 : câble, tableau de distribution, panneau de contrôle, station de recharge, panneau solaire et générateur à charbon, voir [[Objets du réseau électrique]] ; 31 et 32 : petite caisse et armoire, voir [[Rangements]]) : ressources `Rock`, `Wood Log`, `Branch`, `Plant Fibers`, `Iron Ore`, `Iron`, `Coal`, `Copper Ore`, `Titanium Ore`, `Titanium`, `Ice`, `String`, `Circuit Board`, `Microchip`, `Glass`, `Glue` ; consommables `Ration`, `Water`, `First Aid Kit` ; plaçables `Crate`, `Radio Relay` (prefab `RadioRelais`, sans effet pour l'instant) et `Crafter` (station de craft, prefab `Crafter`, voir [[Station de craft]]) ; `Oxygen Tank` et `Jetpack` sans effet pour l'instant, `Flashlight` (pile de 1). Chaque objet doit être dans la liste `items` du SaveManager (Bootstrap).
- **Item** : un ItemData (`Item.CreateFromData`) et la valeur de sa barre (`Gauge`), propre à chaque objet.
- **ItemSlot** : un item et une quantité. `Add` respecte `StackMax` et renvoie le reste, `Remove` renvoie ce qui manque. `Switch` échange le contenu avec un autre slot. `Drop` passe par `Pickupable.Spawn`, qui instancie le `Model` devant la source avec un `Pickupable` déjà identifié et un Rigidbody qui reçoit une petite impulsion. Le slot n'est vidé que si le spawn réussit (modèle valide avec un `Pickupable`).
- **ItemCollection** : liste de slots de taille fixe (`MaxSize`). `Add` complète d'abord les stacks non pleins puis remplit les slots vides, et renvoie le surplus. `RemoveItem` retire en partant du dernier slot. `Count` compte la quantité totale d'un item. Event `ev_Update`.
- **ItemStorage** (MonoBehaviour) : encapsule une ItemCollection. Sert de base au PlayerInventory et aux conteneurs.

Voir aussi : [[Inventaire du joueur]] · [[Barre des objets]]
