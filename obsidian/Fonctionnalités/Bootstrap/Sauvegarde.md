---
tags: [jeu-colonie, fonctionnalite, bootstrap, sauvegarde]
---

# Sauvegarde

`SaveManager`, dans `Bootstrap`.

- Un fichier JSON par partie dans `Application.persistentDataPath/Saves`, choisi dans le menu principal (`NewGame`, `LoadGame`, `DeleteSave`, `ListSaves`). Sauvegarde à la fermeture du jeu, avant un changement de scène et toutes les `autosaveInterval` secondes (120) dans le monde. Restauration au chargement du monde.
  - Une nouvelle partie repart de l'inventaire et des stats de départ (capturés au lancement du jeu), même après une autre partie dans la même session.
  - Monde lancé directement dans l'éditeur, sans passer par le menu : une sauvegarde « Partie éditeur » est créée.
  - Version du format (`SaveData.CurrentVersion`, 2) : une sauvegarde d'une version plus ancienne est ignorée (absente du menu, avertissement dans la console). La version 2 est arrivée avec les ids d'items en texte et la renumérotation des items.
  - Menu contextuel du composant : « Refresh item list » et « Open saves folder ».
- **Contenu** : la seed du monde, l'heure du monde (`SaveData.time` : jour et heure, une sauvegarde sans heure repart à l'heure de départ), le joueur (position, rotation, vue de la caméra, stats, inventaire, barre d'outils, équipement, slot en main), les objets sauvegardables, les objets retirés et les aplanissements du terrain.
- **SaveableObject** : un objet sauvegardé (transform et composants `ISaveableComponent`, y compris ceux de ses enfants qui n'ont pas leur propre SaveableObject). Trois sortes d'objets :
  - **posé dans une scène** : id = `scène:chemin dans la hiérarchie` (voir [[R15 - Ids uniques des objets de scène|R15]]) ;
  - **généré** (`EnvironmentSpawner`) : id déterministe (`env_<x>_<z>_<index>`) ;
  - **créé en jeu** (construction, objet jeté) : id unique et clé de prefab (`build:<nom>`, `item:<id>`). Le SaveManager le recrée au chargement.
- Un objet posé ou généré qui est retiré en jeu (ramassé, détruit) est noté et n'est plus recréé. Les objets des chunks non visités gardent leur dernier état sauvegardé.
- **Composants sauvegardés** : `ItemStorage` (contenu des coffres : chaque coffre a automatiquement un SaveableObject, chaque capsule garde donc son propre intérieur) et `Pickupable` (item, quantité). Les items sont retrouvés par leur `Id` dans la liste `items` du SaveManager.
- **Terrain** : la seed sauvegardée remplace celle de la scène. Les aplanissements sous les constructions (`TerrainFlattening`, `TerrainEditSaveData` : disque ou rectangle) sont enregistrés et rejoués à la génération de chaque chunk concerné. Ceux des anciennes sauvegardes (ancien format) sont ignorés.
- **Joueur restauré** : sa position est conservée (sur le sol d'une capsule, par exemple), et son Rigidbody reste cinématique jusqu'à la génération du chunk sous lui (`ChunkManager.HoldUntilGenerated`).

Voir aussi : [[Menu principal]] · [[Structures et catalogue]] (sauvegarde des constructions) · [[Terrain sous les constructions]]
