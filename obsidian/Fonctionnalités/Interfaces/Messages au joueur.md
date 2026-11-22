---
tags: [jeu-colonie, fonctionnalite, interfaces]
---

# Messages au joueur

`NotificationUI`, dans `Bootstrap`, `Notifications.uxml`, style `.notification` / `.notification-warning`.

- Messages courts en bas de l'écran, qui disparaissent en fondu après 2,5 s (« Inventaire plein », « Ressources manquantes »…).
- Un message identique remet son compteur à zéro au lieu de s'afficher deux fois.
- Il ne capte pas la souris.
- Usage : `NotificationUI.Show(texte, NotificationType.Warning)`.

Voir aussi : [[Pointeur et interactables]] (notifications de ramassage) · [[Interfaces UI Toolkit]]
