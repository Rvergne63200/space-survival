---
tags: [jeu-colonie, instructions, code, items]
---

# Ajouter un item

Les classes d'items sont décrites dans [[Données des items]]. Étapes pour un nouvel item :

1. **Choisis la classe** (menu Create > Item) :
   - `ItemData` : ressource simple ;
   - `ConsummableData` : consommable (liste de modifiers) ;
   - `PlaceableItemData` : objet qui se pose (voir [[08 - Constructions et objets posés]]) ; `CableItemData` pour un câble ;
   - `ToolData` : outil en main (actions `ItemAction` au clic gauche / droit, consommation de sa barre) ;
   - `EquipmentData` (ou `JetpackData`) : équipement porté dans un emplacement.
2. **Id et nom de fichier** : `<id>_<Nom>.asset` dans `ScriptableObjects/Items`, avec l'id suivant (les items du jeu sont numérotés sans trou). Un item qui ne sert qu'aux tests va dans `ScriptableObjects/TestItems` avec un id `T` + 3 chiffres. L'id est du texte saisi à la main et doit être unique.
3. **Liste du SaveManager** : ajoute l'item à la liste `items` du `SaveManager` (scène `Bootstrap`, menu contextuel « Refresh item list »). Sinon il ne sera ni sauvegardé ni rechargé. Au lancement, le SaveManager signale un id vide ou en double.
4. **Modèle au sol** : le `Model` (prefab utilisé quand l'item est jeté) doit porter un `Pickupable`, sinon il ne peut pas être jeté.
5. **Barre** (outil ou équipement) : un objet avec une barre a `StackMax` = 1 et garde sa valeur propre (voir [[Barre des objets]]).
6. **Coffre de test** : ajoute un stack du nouvel item au coffre de test de `0_World` (`TestChest`), qui contient un exemplaire de chaque item.
7. **Nouvelle action d'outil** : une sous-classe d'`ItemAction`, `ToggleItemAction` ou `HoldItemAction` selon le mode (voir [[Utilisation des objets en main]]), choisie dans l'Inspector grâce au `SubclassPicker`.

Mets à jour la note [[Données des items]] (liste des objets) dans le même commit.

Voir aussi : [[04 - Rendre un objet sauvegardable]]
