---
tags: [jeu-colonie, a-faire, feature, terrain]
id: F32
categorie: Feature
feature: Terrain
statut: à faire
---

# F32 - Ressources générées

**Ressources** générées (minerais, plantes, débris…), certaines réapparaissant avec le temps, d'autres non.

La [[Foreuse]] tire ses minerais au hasard, partout : avec des gisements, elle devra extraire celui sur lequel elle est posée.

- Les modèles de gisement existent (charbon, fer, cuivre, voir [[F65 - Intégration des modèles 3D|F65]]) : il reste les prefabs, leur collider, leur placement dans le monde et le lien avec la foreuse (empty `DrillPoint`, où caler la foreuse). Le terrain n'est pas creusé sous le gisement : le fond des fissures reste au-dessus du sol.

Voir aussi : [[Objets du décor]]
