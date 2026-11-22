---
tags: [jeu-colonie, instructions, code]
---

# Managers et cycle de vie

Le fonctionnement détaillé est décrit dans [[Scènes et chargement]] et [[Modes de jeu et interfaces]]. Cette note dit comment s'y brancher.

## Objets persistants
- Les managers vivent dans la scène `Bootstrap`, sous `Root` (`DontDestroyOnLoad`). Chaque scène de jeu a un `SceneContext`, qui charge `Bootstrap` si besoin : une scène se lance donc seule depuis l'éditeur.
- Un nouveau manager persistant va sous `Root` dans `Bootstrap`, avec le schéma singleton existant :
  - `Awake` : si `Instance` existe déjà et n'est pas `this`, `Destroy(gameObject)` et `return` ; sinon `Instance = this` ;
  - `OnDestroy` : remettre `Instance = null` si c'est lui.
- Un manager propre au monde (`ChunkManager`, `DayNightCycle`) est un singleton de la scène `0_World`, pas de `Bootstrap`.
- `ModeManager` et `InterfaceManager` sont des singletons **C# statiques** (`ModeManager.Instance`, `InterfaceManager.GetInstance()`), pas des MonoBehaviour.

## Le joueur
- `PlayerManager` garde l'inventaire et les stats (persistants) et recrée le joueur à chaque scène qui a un `PlayerSpawnPoint`.
- Le joueur peut être absent (menu) ou recréé (respawn, changement de scène) : ne garde pas de référence directe au joueur dans un objet persistant. Passe par `PlayerManager.Instance.Player` et écoute `ev_spawnPlayer` / `ev_updatePlayerContext` pour mettre à jour les références.
- Les composants du joueur se partagent le `PlayerContext` (objet pointé, interactable).

## Modes
- Un comportement qui doit se bloquer dans certains états (interface ouverte, construction) lit une option : `ModeManager.Instance.IsEnabled(OPTION_…)`, et écoute `ev_OnUpdateOptions` si besoin. Une nouvelle option s'ajoute aux modes concernés dans `PlayerModeOptions`.
- Une interface ouverte s'enregistre dans `InterfaceManager` (voir [[07 - Créer une interface]]) : le mode `Interface` libère le curseur et bloque mouvement et caméra.

Voir aussi : [[02 - Conventions de code]] · [[R01 - Refonte du chargement de scènes|R01]] · [[R02 - Persistance entre scènes|R02]]
