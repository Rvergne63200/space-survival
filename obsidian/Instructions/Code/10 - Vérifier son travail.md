---
tags: [jeu-colonie, instructions, code]
---

# Vérifier son travail

L'IA interroge l'éditeur par le MCP Unity mais ne joue pas : elle vérifie ce qu'elle peut, puis dit précisément à l'utilisateur ce qu'il doit tester en jeu. L'utilisation du MCP Unity (connexion, compilation, ce qu'on a le droit de modifier) est décrite dans `CLAUDE.md`.

## Ce que l'IA vérifie
- **Compilation** : par le MCP Unity (`refresh_unity` puis `read_console`), sinon `error CS` dans `%LOCALAPPDATA%\Unity\Editor\Editor.log`. Si ni l'un ni l'autre n'a pu être vérifié, dis-le.
- **Démarrage** : pour un changement qui touche l'initialisation (managers, sauvegarde, scène), lancer le Play mode quelques secondes par le MCP et lire la console, après avoir prévenu l'utilisateur.
- **Prefabs et scènes** : contrôler par le MCP qu'un composant ajouté est bien présent et réglé sur le prefab ou dans la scène (`find_gameobjects`, `manage_components`, `manage_prefabs`).
- **Import de modèles** : dans la console Unity ou dans `Editor.log`, « self-intersecting » et les erreurs d'import (voir [[16 - Erreurs à ne pas refaire]]).
- **Impact** : avant de modifier une classe centrale (`Builder`, `DistributionPanel`, `EnergyDevice`, `SaveManager`, `PlayerInventory`…), `graphify affected "<Classe>"` pour voir ce qui en dépend.
- **Sauvegarde** : tout nouvel état persistant a son `CaptureState` / `RestoreState`, et un chargement d'une sauvegarde existante ne doit pas planter (champ absent).

## Ce que l'utilisateur teste
Donne une liste courte et concrète des étapes à faire en jeu (« pose une batterie dans le dôme, coupe le dôme depuis la borne, vérifie que… »). Signale les cas limites qu'il vaut la peine d'essayer.

## Outils de test
- Coffre de test à côté de la capsule de départ (`TestChest`, scène `0_World`) : un exemplaire de chaque item.
- Objets de test électriques : `T001` générateur infini, `T002` machine de test, `T003` batterie de test. `T004` : potion qui retire de la vie.
- `DayNightCycle` : `speed`, `day` et `hour` modifiables dans l'Inspector en jeu.
- Dans l'éditeur, la touche **²** remplace Échap (fermer une interface, menu pause).
- Une scène se lance seule depuis l'éditeur (le `SceneContext` charge `Bootstrap`) ; lancée directement, `0_World` crée la sauvegarde « Partie éditeur ».

Voir aussi : [[02 - Conventions de code]]
