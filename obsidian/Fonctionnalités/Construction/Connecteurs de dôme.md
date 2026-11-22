---
tags: [jeu-colonie, fonctionnalite, construction, batiments]
---

# Connecteurs de dôme

`DomeConnector`, sur le prefab du connecteur : relient un [[Sas]] ou un [[Couloir]] au mur d'un dôme. Origine sur la peau extérieure du mur, au sol, +Z vers l'extérieur ; l'enfant `Socket` est la bride où commence le sas ou le couloir (0,55 m dehors).

- **Masque** : une boîte (`maskCenter`, `maskSize`, espace local : 2,2 × 2,6 × 1,2 m, du seuil à 2,8 m de haut, à travers le mur) envoyée à tous les shaders masqués (`_StructureMasks`, 32 au plus). La coque du connecteur cache les bords.
- **DomeConnection** (statique, partagé par le sas et le couloir) : pose du connecteur et de la bride vers une direction depuis le centre du dôme, n'importe où sur le pourtour ; place libre (`IsFree` / `Problem` : 2,8 m au moins entre deux connecteurs le long du mur, 1,8 m au moins de la borne électrique du dôme) ; dôme le plus proche d'un point (`NearestDome`) ; création (enfant du dôme, appartient au sas ou au couloir, détruit avec lui) ; fantôme du connecteur pendant la pose (`ConnectorPreview`).

Voir aussi : [[Dômes]]
