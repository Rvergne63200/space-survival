# Space Survival : instructions pour Claude

Jeu de survie et de colonisation spatiale en vue subjective, développé seul par l'utilisateur avec l'aide de Claude.
Unity 6000.4, **Built-in Render Pipeline**, shader Standard. Blender 5.3 (interface française) piloté par le serveur MCP Blender pour les modèles.

- Réponds en **français**. L'utilisateur écrit en français et itère visuellement.
- Quand il dit « si tu as des questions, pose-les avant de commencer », pose-les **avant** de toucher à quoi que ce soit, avec des choix concrets.
- Tu peux interroger l'éditeur Unity par le MCP (voir plus bas), mais tu ne joues pas comme l'utilisateur : dis clairement ce qui n'a pas été vérifié en jeu (rendu, ressenti, comportement).

## Où est l'information

| Besoin | Où chercher |
|---|---|
| Design du jeu, style, ce qui existe, ce qui reste à faire | Le vault Obsidian `obsidian/` (voir plus bas) |
| Code : où est quoi, qui appelle qui | `graphify` (voir plus bas), puis lecture des lignes trouvées |
| État de l'éditeur, console, scènes, prefabs | Le MCP Unity `UnityMCP` (voir plus bas) |
| Erreurs de compilation et d'import, si Unity n'est pas connecté | `%LOCALAPPDATA%\Unity\Editor\Editor.log` (chercher `error CS`, « self-intersecting ») |

Le design ne se documente **que** dans Obsidian. Ce fichier ne contient que les règles valables pour **toutes** les sessions ; les règles détaillées par domaine sont dans `obsidian/Instructions/`.

## Avant de commencer une tâche

| Tâche | À lire d'abord |
|---|---|
| Modéliser, animer ou intégrer un modèle | La DA (`03 - Formes et silhouettes`, `05 - Matériaux et textures`, `08 - Technologie humaine`) **et** les notes utiles de `Instructions/Pipeline 3D` |
| Écrire ou modifier du code | `Instructions/Code` (au moins `02 - Conventions de code` et la note du sujet : sauvegarde, item, interaction, interface, construction, énergie), la note `Fonctionnalités` de la feature, la tâche `A faire` concernée et, pour une nouvelle mécanique, la note `Game Design` du thème |
| Toute autre tâche sur le jeu | L'index du dossier Obsidian concerné |

## Obsidian (`obsidian/`)

Le vault rassemble toutes les informations sur le jeu, en français, **une note par point**.

| Dossier | Contenu | Règle |
|---|---|---|
| `Projet/` | `00 - Index du projet`, le sommaire général du vault | À mettre à jour quand un dossier apparaît. |
| `Game Design/<Thème>/` | Le concept du jeu : pitch, boucle, monde, survie, colonie, exploration, et `Points ouverts du game design` | **Source de vérité du gameplay voulu.** Ne la modifie que sur demande ou après validation explicite de l'utilisateur ; une idée de Claude y est marquée *(proposition)*. |
| `Direction Artistique/` | Bible de DA : intention, formes, couleurs, matériaux, technologie, ambiance, à éviter | **Source de vérité du style.** Ne la modifie que sur demande ou après validation explicite de l'utilisateur. |
| `Fonctionnalités/<Thème>/` | Ce qui est implémenté, une note par feature (comportement et détails de code) | À tenir à jour à chaque changement de comportement. |
| `A faire/` | Une note par tâche ouverte, et `Historique/` pour les tâches terminées | À tenir à jour à chaque changement de code. |
| `Instructions/` | Règles que l'IA respecte pour produire le jeu, par domaine : `Pipeline 3D/` (modélisation Blender → Unity), `Code/` (organisation et conception du code) | Jamais de règle de style ici : elle va dans la DA. Jamais de règle déjà dans ce fichier. |

Chaque dossier a une note `00 - Index …` qui sert de sommaire : commence par elle.

### Lire
- Pour trouver une info, `graphify query "<question>"` ou `graphify section "<titre>"` fonctionnent aussi sur les notes (leurs titres sont des nœuds du graphe). Sinon, l'index du dossier, puis la note.
- En cas de contradiction entre la DA et une note `Instructions`, la DA l'emporte : signale-la à l'utilisateur au lieu de trancher seul.

### Écrire
- Édite les fichiers `.md` directement (Write / Edit). Le serveur MCP `obsidian` sert seulement si tu as besoin de ses outils (tags, recherche).
- **Conventions** :
  - frontmatter avec `tags: [jeu-colonie, <dossier>, …]`, puis `# Titre`, puis le contenu en listes ; une ligne `Voir aussi : [[…]] · [[…]]` en fin de note ;
  - les liens visent le **nom exact** de la note (`[[Tableau de distribution]]`), donc chaque nom doit être **unique dans tout le vault** ;
  - pas de `#`, `|`, `^`, `[` ou `]` dans un nom de note (ils cassent les liens) ;
  - jamais de note à la racine du vault. Obsidian crée une note vide à la racine quand on clique sur un lien vers une note inexistante : supprime ces fichiers vides si tu en trouves.
- **Tâches (`A faire/`)** :
  - nom `<ID> - <titre court>.md`, frontmatter `id`, `categorie`, `feature`, `statut` (`à faire` / `en cours`) ;
  - ids **stables** : `B` bug, `F` feature, `O` optimisation, `R` refonte. Une nouvelle tâche prend le prochain numéro libre de sa catégorie (indiqué dans `00 - Index À faire`, à mettre à jour). Un numéro n'est jamais réutilisé. L'utilisateur désigne les tâches par leur id ;
  - lien vers une tâche ouverte : `[[F51 - Système d'oxygène|F51]]` ;
  - tâche terminée : son texte rejoint `Historique/Historique - <Bugs|Features|Optimisations|Refontes>.md` sous la forme `- [x] **F45** **[Inventaire]** … ^F45`, sa note est supprimée, l'index et les liens vers elle sont mis à jour (`[[Historique - Features#^F45|F45]]`).
- **À chaque changement de code**, dans le même commit : coche ou mets à jour les tâches concernées (et ajoute celles découvertes en route), et mets à jour la note `Fonctionnalités` dont le comportement change.
- **Après une session de modélisation** : complète `Instructions/Pipeline 3D` (tables de clips, couches, nouvelles erreurs dans `16 - Erreurs à ne pas refaire`).
- **Après toute modification du vault** : vérifie qu'aucun `[[lien]]` n'est cassé (chaque cible doit correspondre à un nom de fichier `.md`), puis lance `graphify update .`.

## graphify

Le projet a un graphe de connaissances dans `graphify-out/` : le code C# (classes, méthodes, appels) et les fichiers Markdown, notes Obsidian comprises (un nœud par titre). Il répond en bien moins de tokens qu'un grep ou qu'une lecture de fichiers.

Règles :
- Quand `graphify-out/graph.json` existe, lance `graphify query "<question>"` **avant** ton premier grep, ta première recherche ou ta première lecture de fichier source pour une question sur le code. Mets cette règle dans le prompt de chaque sous-agent qui explore le code.
- Ne lis les fichiers bruts que pour les lignes que le graphe ou une lecture précédente a déjà localisées.
- Commandes :
  - `graphify query "<question>"` : sous-graphe lié à la question (`--budget N` pour plus de résultats, 2000 tokens par défaut ; `--depth N` de 1 à 6) ;
  - `graphify explain "<concept>"` : un nœud et ses voisins (`--all` pour tout voir) ;
  - `graphify path "<A>" "<B>"` : chemin le plus court entre deux nœuds ;
  - `graphify affected "<X>"` : ce qui dépend de X, avant de le modifier ;
  - `graphify section "<titre>"` : texte d'une section Markdown, sans ouvrir le fichier (`"chemin.md#titre"` si plusieurs notes ont ce titre) ;
  - `graphify god-nodes` : les nœuds les plus connectés, pour une vue d'ensemble.
- Codes de sortie : 3 = nom ambigu (les candidats sont listés, précise), 4 = aucun nœud (reformule, ou passe au grep).
- Les outils MCP `mcp__graphify__*` (`query_graph`, `get_node`, `get_neighbors`, `shortest_path`, `get_section`…) font la même chose sans passer par le shell.
- Lis `graphify-out/GRAPH_REPORT.md` seulement pour une revue globale de l'architecture, ou si query / path / explain ne suffisent pas.
- Si un message indique que le graphe est périmé pour un fichier, fie-toi au fichier lu, puis mets le graphe à jour.
- **Après chaque modification du code ou des notes**, lance `graphify update .` (extraction locale, sans coût d'API). Une extraction sémantique des documents (`/graphify --update`) coûte des appels d'API : seulement si l'utilisateur la demande.
- `graphify-out/graph.json` est réécrit en entier à chaque build : un gros diff est normal. Ne le signale pas comme du bruit et ne propose pas de sauter la reconstruction.

## MCP Unity (`UnityMCP`)

Le serveur MCP for Unity est déclaré dans la configuration de Claude Desktop (`%LOCALAPPDATA%\Claude-Data\…\claude_desktop_config.json`, via `npx mcp-remote http://127.0.0.1:8080/mcp`). Ses outils s'appellent `mcp__UnityMCP__*`. Il ne fonctionne que si Unity est ouvert avec le bridge MCP for Unity démarré en **HTTP** (port 8080).

- **Ne le déclare nulle part ailleurs** (ni `~/.claude.json`, ni `.mcp.json`) : un second serveur ferait doublon. Un serveur `unityMCP` en minuscules est un ancien doublon cassé (mode stdio) : ne l'utilise pas.
- **Connexion** : un premier appel simple (`read_console`) dit si l'éditeur est joignable. Si aucune instance n'est trouvée, dis-le à l'utilisateur et reviens à `Editor.log`. (La lecture des ressources `mcpforunity://…` n'est pas disponible à travers `mcp-remote` : passe par les outils.)
- **Après une modification de code** : `refresh_unity` (`scope: "scripts"`, `compile: "request"`, `wait_for_ready: true`) attend la fin de la compilation, puis `read_console` (erreurs et avertissements). C'est la vérification de compilation par défaut.
- **Lire plutôt qu'écrire** : utilise-le d'abord pour vérifier (console, hiérarchie d'une scène ou d'un prefab, composants, valeurs d'un asset). Modifier une scène, un prefab, un asset ou les réglages du projet par le MCP est une modification des fichiers de l'utilisateur : fais-le seulement quand la tâche le demande, dis ce que tu as changé, et ne sauvegarde pas une scène qu'il a ouverte sans le lui dire.
- **Play mode** : tu peux le lancer pour vérifier qu'aucune erreur n'apparaît au démarrage, puis l'arrêter. Préviens l'utilisateur avant, il est peut-être en train de travailler dans l'éditeur.
- **API Unity** : en cas de doute sur une API, vérifie-la avec `unity_reflect` / `unity_docs` plutôt que de te fier à ta mémoire.

## Code Unity

- **Pièces des modèles** : le code de gameplay ne fait jamais tourner ni bouger une pièce de modèle (porte, ventilateur, tête…). Il appelle le script d'animation livré avec le modèle (`Assets/Scripts/Behaviours/Animations/<Model>Animator.cs`). S'il manque une méthode, dis-le à l'utilisateur et demande s'il l'ajoute dans le pipeline ou si tu dois l'ajouter au script, au lieu de coder le mouvement toi-même.
- **Assets de l'utilisateur** (TerrainLayer, textures, matériaux faits à la main, modèles validés) : n'y touche pas sans demande.
- **Fichiers `.meta`** : un asset se déplace ou se renomme avec son `.meta` ; ne change jamais un GUID. Ne supprime pas un `.mat` généré tant qu'un `.meta` de FBX le référence (matériaux roses sinon).
- **Langue** : commentaires du code de gameplay en anglais (comme l'existant) ; scripts `*Animator.cs` des modèles commentés en français (règle du pipeline) ; textes affichés au joueur en français.
- **Vérification** : après une modification de code, contrôle la compilation par le MCP Unity (`read_console`), ou dans `Editor.log` (`error CS`) si Unity n'est pas connecté, et dis ce qui reste à tester en jeu.
- Dans l'éditeur, la touche **²** remplace Échap pour fermer les interfaces et ouvrir le menu pause (Échap y libère le curseur).

## Bugs signalés par l'utilisateur

Pars de ce qu'il a observé. S'il manque une information (quel objet, où exactement), pose **une** question avant de changer quoi que ce soit. Une hypothèse à la fois, liée à ce qu'il a dit ; pas de chaîne de corrections spéculatives.

## Fichiers et outils

- **Racine du projet** : rien en vrac. Les fichiers Blender vont dans `ArtPipeline/Blends/<Model>.blend`, les rendus dans `ArtPipeline/Renders/<Model>_<vue>.png`, les scripts Blender dans `ArtPipeline/Blender/`. Les modèles exportés vont dans `Assets/Resources/Models/<Model>/` (voir `Instructions/Pipeline 3D/11 - Export FBX`).
- **Python n'est pas installé** : pour un script local, utilise Node.js (`node`). Le Python des modèles s'exécute dans Blender via le MCP.
- Shell Windows : les chemins contiennent des espaces et des accents, mets-les entre guillemets.
- Ne touche pas à `obsidian/.obsidian/` (configuration de l'application Obsidian).

## Git

- Quand l'utilisateur dit « commit », inclus **tout** ce qui est en attente (`git add -A`), y compris les fichiers modifiés par Unity (ProjectSettings…), sauf s'il limite lui-même le périmètre. Ne pousse que s'il le demande.
- Messages de commit en anglais, une phrase qui décrit le changement (« Add the coal generator, share the insides of buildings linked by corridors… »).
- Le commit contient aussi les mises à jour Obsidian liées au changement (tâches et fonctionnalités).
