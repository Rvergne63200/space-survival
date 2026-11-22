---
tags: [jeu-colonie, fonctionnalite, crafting]
---

# Station de craft

- **RecipeData** (ScriptableObject `Crafting/Recipe`) : listes d'`Ingredients` et de `Results`. Chaque entrée est un `ItemBlock` (ItemData + quantité).
- **CraftingStation** (`IInterractable`, sur le prefab `Crafter`, objet `24_Crafter`, modèle `Resources/Models/CraftStation` avec son `Animator` et `CraftingStationAnimator`) : la liste des recettes disponibles.
  - **Électricité** : un `EnergyConsumer` (20 W) à brancher par câble (point `Connector` à l'arrière). Allumée et alimentée (`IsUsable`), l'animateur l'allume (`PowerOn` : écran et hologrammes) ; éteinte ou sans courant, il l'éteint (`PowerOff`) et rien ne peut être lancé (la file attend). On l'allume ou l'éteint depuis son interface et depuis le [[Panneau de contrôle]].
  - **File** : chaque exemplaire prend `craftTime` (2 s), l'un après l'autre, pendant que l'animateur imprime (`StartPrint` / `StopPrint`). Les ingrédients sont pris au lancement ; « Annuler » rend au joueur ceux des fabrications pas finies.
  - **Sortie** : un exemplaire fini va dans l'inventaire du joueur s'il est à moins de `pickupDistance` (4,5 m) et qu'il a la place (notification de ramassage), sinon il attend sur la station (empilé avec le même objet). Une fabrication dont le résultat ne rentre pas dans la sortie attend. Interagir (F) récupère ce qui attend (ce qui rentre), sinon ouvre l'interface.
  - **Sauvegarde** : la file (index des recettes), la progression et la sortie.
- **Crafter** (sur le joueur) :
  - `Craft(recipe)` vérifie que la station est valide, que la recette lui appartient et que les ingrédients sont présents (`CheckHasItems`, qui additionne les ingrédients en double et compte l'inventaire et la barre d'outils).
  - Ensuite, il retire les ingrédients de l'inventaire et ajoute une fabrication à la file de la station (`Enqueue`). Refusé si la station n'est pas utilisable.
- **UI**
  - `CraftingStationUI` (`CraftingStation.uxml`, à droite) : liste des recettes de la station à gauche (une icône `RecipeSlot.uxml` par recette, `RecipeSlotUI`), détail de la recette sélectionnée à droite : icône, nom et description du premier résultat, quantité obtenue, ingrédients (`RecipeIngredient.uxml` : icône, nom, possédé/requis, en rouge s'il manque). En haut, l'état de la station (alimentée, éteinte, non alimentée, réseau disjoncté), la fabrication en cours (progression, nombre en attente, ce qui attend sur la station) et les boutons Annuler et Allumer / Éteindre ; « Fabriquer » est désactivé tant qu'elle n'est pas utilisable.
  - Quantité à fabriquer : boutons −, + et Max (limitée par `Crafter.MaxCraftable`, le nombre de crafts possibles avec l'inventaire). « Fabriquer » enchaîne les crafts. Les besoins affichés sont multipliés par la quantité.
  - Les recettes irréalisables sont grisées (classe `recipe-unavailable`). La case « Fabricables uniquement » les masque (choix conservé d'une ouverture à l'autre) ; si la recette sélectionnée est masquée, la première visible est sélectionnée.
  - Rafraîchi toutes les 0,25 s. L'inventaire du joueur s'ouvre avec la station (comme pour un conteneur). À la fermeture de l'interface, la station du `Crafter` est libérée (`ClearStation`). Les échecs (ressources manquantes…) et un résultat posé au sol affichent un message ([[Messages au joueur]]). L'UI se ferme quand le joueur s'éloigne de la station de plus de `Crafter.maxDistance` (4,5 m).

Voir aussi : [[Appareils électriques]] · [[Objets posés]]
