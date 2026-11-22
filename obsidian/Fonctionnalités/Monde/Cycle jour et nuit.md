---
tags: [jeu-colonie, fonctionnalite, monde]
---

# Cycle jour et nuit

`DayNightCycle`, sur la `Directional Light` de `0_World`, qui est le soleil ; dossier `Scripts/Behaviours/Environment`.

- **Horloge** : le soleil se lève à 6:00 et se couche à 18:00. Le jour dure `dayDuration` (900 s, 15 min réelles), la nuit `nightDuration` (600 s, 10 min) : l'horloge va plus vite la nuit. Une nouvelle partie commence le jour 1 à `startHour` (8:00). Le temps s'arrête pendant la pause (`timeScale` 0). L'heure est sauvegardée.
- **Test** : `speed` accélère l'horloge, et `day` / `hour` se modifient dans l'Inspector en jeu.
- **Soleil** : il tourne à vitesse constante par heure de jeu, de l'est (`sunYaw`) à l'ouest, à `sunTilt` (30°) du zénith à midi. Sa lumière s'éteint en passant sous l'horizon et devient orangée près de lui (`horizonColor`).
- **Lune** : une seconde lumière directionnelle (enfant `Moon`, créée en jeu, ombres douces), à l'opposé du soleil, bleutée (`moonColor`). Une seule des deux est allumée à la fois.
- **Nuits** : chaque nuit a sa luminosité, tirée de la seed du monde et du numéro de la nuit (une nuit commence au coucher du soleil). La plupart sont très sombres (la [[Lampe torche]] est nécessaire), certaines sont claires : luminosité = tirage ^ `brightNightRarity` (4), soit une nuit sur six environ à plus de 50 %. Elle règle la lune (`darkMoonIntensity` 0,01 à `brightMoonIntensity` 0,3), la lumière ambiante et les reflets (`darkAmbient` 2 % à `brightAmbient` 20 % de ceux du jour) et le ciel (`darkSky` 1 % à `brightSky` 12 % de son exposition).
- **Crépuscule** : le ciel, la lumière ambiante et les reflets passent du jour à la nuit entre un peu au-dessus et un peu sous l'horizon. Le ciel est une copie du skybox de la scène, dont l'exposition est baissée ; les valeurs du jour sont celles de la scène.
- **Pour les autres systèmes** : `Day`, `Hour`, `IsDay`, `Sunlight` (lumière du soleil au sol, 0 à 1, pour les plantes), `Sun` (la lumière), `SunDirection` (direction vers le soleil) et `NoonSunHeight` (hauteur du soleil à midi), utilisés par le [[Panneau solaire]], et `NightBrightness`.

Voir aussi : [[Horloge]] · [[Sauvegarde]]
