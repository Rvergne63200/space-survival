---
tags: [jeu-colonie, fonctionnalite, construction]
---

# Structures et catalogue

- **StructureData** (ScriptableObject, Construction/Structure) : id (sauvegardes), nom, icône, prefab, coût (`Cost`), ressources rendues à la destruction (`Refund`, n'importe quels items), aplanissement, pente max.
- **StructureCatalog** : la liste des structures (roue et sauvegarde). Structures actuelles : les [[Dômes]] de 8, 12 et 16 m, le [[Sas]] et le [[Couloir]].
- **Layer `Building`** : à l'instanciation d'une structure terminée (`BuildUtility.InstantiateStructure`), ses parties sur le layer `Default` passent sur `Building`. La minimap ne s'en sert plus (elle dessine les contours des structures). Les requêtes physiques ne filtrent pas ce layer : rien ne change pour les collisions et la visée.
- **Sauvegarde** : les structures (`build:<id>`), les plans (`blueprint:<id>`, avec les ressources apportées) et les objets posés (`placed:<itemId>`, avec le contenu de leurs conteneurs) sont des objets « runtime », recréés par `BuildUtility` au chargement.

Voir aussi : [[Roue de construction]] · [[Plans de construction]] · [[Sauvegarde]]
