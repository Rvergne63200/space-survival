---
tags: [jeu-colonie, instructions, code, sauvegarde]
---

# Rendre un objet sauvegardable

Le fonctionnement complet est décrit dans [[Sauvegarde]]. Cette note dit quoi faire pour un nouvel objet.

## L'objet
- Ajoute un `SaveableObject` sur sa racine. Il sauvegarde le transform et tous les `ISaveableComponent` de l'objet et de ses enfants (sauf ceux sous un autre `SaveableObject`, sauvegardés avec lui).
- Choisis la sorte d'id :
  - **posé dans une scène** : rien à faire, l'id vient du chemin dans la hiérarchie (fragile, voir [[R15 - Ids uniques des objets de scène|R15]]) ;
  - **généré** par le monde : `SetGeneratedId(id)` juste après `Instantiate`, avant `Start`, avec un id déterministe (exemple : `env_<x>_<z>_<index>` dans `EnvironmentSpawner`) ;
  - **créé en jeu** : `SetRuntime(clé de prefab)` juste après `Instantiate`, avant `Start`. Le SaveManager le recréera au chargement à partir de cette clé.
- Clés de prefab existantes (constantes de `SaveManager`) :
  - `item:<id d'item>` : objet jeté au sol (`Pickupable.Spawn`) ;
  - `build:<id>` / `blueprint:<id>` : structure finie / plan (`BuildUtility`) ;
  - `placed:<id d'item>` : objet posé (`BuildUtility.InstantiatePlaced`) ;
  - `cable:<id d'item>` : câble électrique ;
  - `prefab:<nom du prefab>` : autre objet runtime ; son prefab doit être dans la liste `runtimePrefabs` du SaveManager (exemple : `DeathBag`).
- Passe par `BuildUtility` pour instancier une structure, un plan ou un objet posé : il pose déjà le `SaveableObject` et sa clé.

## Son état
Implémente `ISaveableComponent` sur le composant qui porte l'état, sur le modèle de `StorageContainer` :

```csharp
[System.Serializable]
private class State { public string belowId; }

public string SaveKey => "storageContainer";   // unique parmi les composants sauvegardés de l'objet
public string CaptureState() => JsonUtility.ToJson(new State { belowId = belowId });
public void RestoreState(string json)
{
    State state = JsonUtility.FromJson<State>(json);
    if (state != null) belowId = state.belowId;
}
```

- `RestoreState` doit accepter un état absent ou incomplet (ancienne sauvegarde, champ ajouté depuis).
- Un lien vers un autre objet sauvegardé se garde par son **id** (`SaveableObject.Id`), jamais par une référence.
- Ce qui n'est pas sauvegardé doit se recalculer au chargement (exemple : le charbon en cours du générateur est perdu, c'est voulu et documenté).

## Format
- Si un changement rend les anciennes sauvegardes illisibles (renommage d'ids, changement de structure), augmente `SaveData.CurrentVersion` : les sauvegardes plus anciennes sont alors ignorées. Préviens l'utilisateur.

Voir aussi : [[05 - Ajouter un item]] · [[08 - Constructions et objets posés]]
