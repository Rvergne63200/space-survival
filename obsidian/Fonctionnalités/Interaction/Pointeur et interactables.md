---
tags: [jeu-colonie, fonctionnalite, interaction]
---

# Pointeur et interactables

- **PlayerPointer** : à chaque frame, raycast de **3 m** (`range`) depuis la caméra, qui ignore le joueur et les triggers. Il met à jour `PlayerContext.PointedObject` avec l'objet qui porte l'`IInterractable` (cherché dans les parents du collider touché), avec l'event `ev_updatePointedObject`.
- **PlayerContext.Interractable** : `IInterractable` trouvé sur l'objet pointé.
- **PlayerInterractor** : la touche Interagir appelle `Interract(player)` sur l'objet pointé.
- **IInterractable** : `Interract(GameObject)`, et `GetInfo()` (texte affiché). Implémentations :
  - **Pickupable** : `item` et `count`. Interagir l'ajoute à l'inventaire via `Pickuper`. L'objet est détruit si tout est ramassé, sinon le reste reste au sol. Si `identified` vaut `true`, le texte affiche « Nx Nom », sinon « Pickup ».
  - **Searchable** : ouvre l'`ItemStorage` associé via `Searcher` dans l'UI de conteneur (`ItemStorageUI`). L'inventaire du joueur s'ouvre avec lui et se referme avec lui, sauf si le joueur l'avait ouvert lui-même (touche Inventaire) : `PlayerInventory.OpenWith` / `CloseWith`. Interagir de nouveau avec le même conteneur le referme, et interagir avec un autre conteneur ouvre celui-ci à la place. Le conteneur d'une machine (`IStorageMachine` sur le même objet : [[Générateur à charbon]]) est son interface : son nom en titre, et au-dessus des emplacements son état, une jauge et son bouton Allumer / Éteindre.
  - **CraftingStation** : voir [[Station de craft]].
  - **LandingCapsuleDoor** : voir [[Capsule de départ]].
  - Portes du sas (`AirlockDoor`) : voir [[Sas]]. Appareils électriques (`EnergyDeviceInfo`) : voir [[Appareils électriques]].
- **Pickuper** : ajoute à l'inventaire et émet `ev_pickup(quantité ramassée, item)`.
- **Searcher** : garde le conteneur ouvert et émet `ev_UpdateSearchStorage`. Le conteneur est fermé via `ClearCurrentSearchedStorage` quand l'UI se ferme, quand le joueur s'en éloigne de plus de `maxDistance` (4,5 m, jusqu'à ses colliders) ou quand il est détruit.
- **UI**
  - `PointedInterractorInformationUI` : affiche le texte et la touche de l'objet pointé, au-dessus du réticule (sans icône), avec un fondu. Le texte est mis à jour en continu (changement d'objet, ramassage partiel).
  - `PickupInfoGroupUI` / `PickupInfoUI` (`PickupInfo.uxml`, en bas à droite) : notification « +N Nom » avec icône, qui disparaît en fondu en 5 s. Les ramassages d'un même item sont cumulés (« +8 Rock ») tant qu'ils se suivent à moins de `groupDuration` (10 s), même si sa notification a disparu entre-temps. Un ramassage partiel ou impossible affiche « Inventaire plein » ([[Messages au joueur]]).

Voir aussi : [[Contour de l'objet pointé]] · [[Rangements]]
