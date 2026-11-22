---
tags: [jeu-colonie, fonctionnalite, bootstrap]
---

# Scènes et chargement

## Scènes
- `Bootstrap` : contient les objets persistants (managers).
- `0_MainMenu` : menu principal, première scène chargée par le Bootstrapper (`MainMenuUI`, voir [[Menu principal]]).
- `0_World` : le monde. Il contient la capsule de départ (prefab `SpaceCapsule`, voir [[Capsule de départ]]), le coffre de test et le `PlayerSpawnPoint`.
  - **Coffre de test** (`TestChest`, une grande caisse (`Crate`) posée à côté de la capsule, surcharges de l'instance dans la scène) : 37 slots, un stack de chaque item du jeu (`0` à `32`) et de chaque item de test (`T001` à `T004`). Posé sur le terrain comme la capsule (`ToReplace`, le coffre est 0,7 m sous sa racine).
- `1_TestScene` : scène de test.

Flux : Menu ⇄ Monde. Les capsules ne sont pas des scènes séparées : chacune est un prefab posé dans le monde, avec son propre intérieur, et on voit l'extérieur depuis l'intérieur.

## Fonctionnement actuel
- **Root** : singleton (flag static) marqué `DontDestroyOnLoad`. C'est la racine des objets persistants de la scène Bootstrap.
- **SceneContext** : à placer dans chaque scène de jeu. Si aucun `Root` n'existe, il charge `Bootstrap` en additif, ce qui permet de lancer le jeu depuis n'importe quelle scène dans l'éditeur.
- **Bootstrapper** : si seule la scène Bootstrap est chargée, il charge `firstScene` (en mode Single).
- **SceneLoadingManager** : appelle `DynamicGI.UpdateEnvironment()` à chaque chargement de scène.
- **PlayerManager** (singleton) : garde l'inventaire et les stats. À chaque `sceneLoaded`, il cherche un `PlayerSpawnPoint` dans les scènes chargées.
  - S'il en trouve un, il réinstancie le joueur à sa position sauvegardée (`SpawnedFromSave`), ou sinon sur le point de spawn. Il notifie ensuite l'UI et la caméra (`ev_spawnPlayer`, `ev_updatePlayerContext`).
  - Sans point de spawn (menu), il n'y a pas de joueur (`DespawnPlayer`), et la caméra du joueur et le HUD du jeu sont masqués (`UIManager.SetHudVisible`).
- **CameraManager** / **UIManager** (singletons) : instancient leur prefab une seule fois, dans leur Awake. UIManager branche les events du joueur (Pickuper, Searcher, Crafter) sur l'UI à chaque spawn.
- **SceneLoader** : `LoadScene()` sauvegarde le monde (`SaveManager.BeforeSceneChange`, voir [[Sauvegarde]]), puis charge `sceneName` en mode Single.

Voir aussi : [[Sauvegarde]] · [[Capsule de départ]] · [[Interfaces UI Toolkit]]
