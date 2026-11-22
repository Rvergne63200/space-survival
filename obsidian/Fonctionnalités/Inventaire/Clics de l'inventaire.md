---
tags: [jeu-colonie, fonctionnalite, inventaire, interfaces]
---

# Clics de l'inventaire

- Système par **clic**, sans vrai glisser-déposer (voir [[F06 - Vrai glisser-déposer|F06]]). Les clics sont gérés par `ItemSlotUI` (`PointerDownEvent` sur l'élément `slot`). Un clic gauche sur un slot (`TryMerge`) prend son contenu dans le `draggingSlot`, qui suit la souris (`HeldItemUI`). Un nouveau clic le dépose.
  - Même item : fusion des stacks, le surplus reste au curseur.
  - Item différent : échange (Switch).
- **Infobulle** (`ItemTooltip`) : au survol d'un slot, le nom, la description et la valeur de la barre de l'objet (« Charge : 3,2 / 10 Wh », mise à jour en direct). Un élément par UIDocument, créé au premier survol.
- **Double-clic** (`GatherToCursor`) : deux clics gauches sur le même slot à moins de 0,35 s, détectés par `ItemSlotUI` et non par `clickCount`, qui compte les clics de tout le panel. Le premier clic prend la pile au curseur, le second y rassemble les items du même type de l'inventaire, de la barre d'outils et du conteneur ouvert, jusqu'à une pile complète (`StackMax`). Les plus petites piles sont prises en premier.
- **Clic droit** (`SplitOrPlaceOne`) : avec un slot en main, dépose une unité sur le slot visé (vide ou même item). Main vide : prend la moitié du slot visé (arrondie au supérieur).
- **Shift + clic gauche** (`QuickTransfer`) : transfère tout le slot.
  - Avec un conteneur fouillé : conteneur → barre d'outils (puis inventaire), et barre d'outils ou inventaire → conteneur.
  - Sans conteneur : inventaire ↔ barre d'outils.
  - Ce qui ne rentre pas reste dans le slot d'origine.
- **ItemCollectionUI** ajoute un `ItemSlot.uxml` par slot de la collection dans un conteneur du UIDocument. Les slots UI sont réutilisés à chaque mise à jour : il n'en crée ou n'en supprime que si la taille change. La largeur de la grille (8 slots par ligne) est fixée dans `Game.uss` (`.item-grid`). `PlayerInventoryContentUI` (`Inventory.uxml`, à gauche), `PlayerToolBarUI` (HUD, en bas au centre) et `ItemStorageUI` (conteneur fouillé, `Storage.uxml`, à droite) en dérivent.
- **ItemSlotUI** : affiche le sprite et la quantité (quantité masquée si `StackMax == 1`).
- **HandedCursorUI** : le cadre (`handed-cursor`) glisse vers le slot en main dans la barre d'outils (SmoothDamp, `smoothTime` 0.05 s).
- **HeldItemUI** (`HeldItem.uxml`) : l'item tenu au curseur, affiché au-dessus des interfaces et qui ne capte pas les clics. `PickingMode.Ignore` est forcé en code sur l'élément, ses enfants et ses parents.

Voir aussi : [[Inventaire du joueur]] · [[Interfaces UI Toolkit]]
