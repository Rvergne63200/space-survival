---
tags: [jeu-colonie, instructions, code]
---

# Conventions de code

## Nommage
- Événements : `UnityEvent` publics préfixés **`ev_`** (`ev_OnPoweredChanged`, `ev_spawnPlayer`, `ev_updatePlayerContext`).
- Singletons MonoBehaviour : `public static X Instance { get; private set; }` (voir [[03 - Managers et cycle de vie]]).
- Options de mode : constantes `OPTION_…` (`PlayerInventory.OPTION_USE_DISABLED`, `InterfaceManager.OPTION_CURSOR_VISIBLE`), associées aux modes dans `PlayerModeOptions`.
- Données en ScriptableObject : champ `[SerializeField] private T _nom;` et propriété en lecture seule `public T Nom => _nom;`. Menu de création via `[CreateAssetMenu(menuName = "Domaine/Type")]`.

## Commentaires
- En anglais, comme le code existant. En tête de chaque classe, un commentaire qui dit ce qu'elle est et comment elle s'insère (« Interface of a distribution panel…, opened by interacting with it »). Les champs non évidents ont leur ligne de commentaire (unité, valeur vide = …).
- Exception : les scripts `*Animator.cs` des modèles, commentés en français (voir [[15 - Scripts d'animation Unity]]).

## Composants
- Mets les `GetComponent` en cache dans `Awake` au lieu de les appeler à chaque frame ou à chaque accès.
- Toute abonnement à un événement a son désabonnement symétrique : `OnEnable` / `OnDisable`, ou `Awake` / `OnDestroy`. Jamais d'abonnement dans un constructeur de MonoBehaviour. Les lambdas d'input doivent aussi être retirées. (Leçon de [[Historique - Refontes#^R04|R04]].)
- Ne modifie jamais un **prefab asset** au runtime (`prefab.GetComponent<…>().x = …`) : en éditeur, la modification persiste dans l'asset. Modifie l'instance après `Instantiate` (voir [[R05 - Prefab asset modifié au runtime|R05]]).
- Polymorphisme réglable dans l'Inspector : `[SerializeReference]` avec l'attribut `SubclassPicker` (actions d'items, modifiers).
- Valeur de puissance ou d'énergie qui peut être infinie : `EnergyValue`.

## Robustesse
- Une référence manquante (élément UXML, enfant de modèle, prefab) désactive seulement la fonction concernée, avec un `Debug.LogWarning` explicite, au lieu de lever une exception qui casserait tout l'objet.
- Les messages destinés au joueur passent par `NotificationUI.Show(texte, NotificationType.Warning)`, en français. `Debug.Log` est réservé au développeur.
- Temps qui doit continuer pendant la pause (UI) : `Time.unscaledDeltaTime`.

## Gameplay et modèles
- Le code de gameplay ne fait jamais bouger une pièce de modèle lui-même : il passe par le `<Model>Animator.cs` du modèle (règle détaillée dans `CLAUDE.md`).

Voir aussi : [[01 - Organisation des dossiers]] · [[10 - Vérifier son travail]]
