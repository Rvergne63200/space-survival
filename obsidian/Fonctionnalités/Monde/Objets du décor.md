---
tags: [jeu-colonie, fonctionnalite, monde, terrain]
---

# Objets du décor

`EnvironmentSpawner`, sur le prefab `Chunk`.

- Appelé une fois les hauteurs générées (`ev_OnTerminateGeneration`).
- Pour chaque m² du chunk, il a 0.1 % de chances de faire apparaître `prefab`, avec une rotation et une taille aléatoires.
- Le tirage est seedé par la seed du monde et la position du chunk : un chunk a toujours les mêmes objets.
- Les objets générés ont un id déterministe (`env_<x>_<z>_<index>`, voir [[Sauvegarde]]).

Voir aussi : [[Terrain procédural]]
