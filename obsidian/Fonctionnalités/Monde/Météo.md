---
tags: [jeu-colonie, fonctionnalite, monde, meteo]
---

# Météo

`WeatherSystem`, sur l'objet `Weather` de `0_World` ; dossier `Scripts/Behaviours/Environment`. Vent et tempêtes de sable.

- **Rien n'est sauvegardé** : la météo est tirée de la seed du monde et de l'heure du jeu ([[Cycle jour et nuit]]). La même heure d'un même monde a toujours la même météo, et une partie rechargée retrouve la météo qu'elle avait.
- **Vent** : vitesse (m/s) et direction d'où il vient, qui varient lentement (bruit de Perlin sur les heures de jeu : `windPeriod` 5 h pour la force, `directionPeriod` 16 h pour la direction), de `calmWind` (1 m/s) à `strongWind` (14 m/s). Il se lève pendant l'annonce d'une tempête (`warningWind`, 35 % du vent de tempête à la fin de l'annonce), atteint `stormWind` (26 m/s) pendant la tempête et vient alors du côté de la tempête. Sert à l'[[Éolienne]].
- **Tempêtes** : à partir du jour 2, chaque jour a `stormChance` (30 %) de chances d'avoir une tempête, à une heure au hasard, de `minStormHours` à `maxStormHours` (1,5 à 4 h de jeu ; elle peut déborder sur le lendemain). Elle monte en `rampHours` (0,5 h) et retombe de même.
- **Annonce** (`warningHours`, 1,5 h de jeu avant, soit environ 2 minutes de jour) : message « Tempête de sable en approche : mettez-vous à l'abri », la brume s'épaissit et le ciel s'assombrit un peu (`warningLook`, 15 %), « Tempête en approche » dans l'[[Horloge]]. Puis « La tempête de sable arrive » au début et « La tempête de sable s'est calmée » à la fin. Pas de message pour une tempête déjà en cours au chargement ou après un saut dans le temps.
- **Pendant la tempête** :
  - les panneaux solaires gardent `stormSolar` (10 %) de leur puissance ([[Panneau solaire]]) ;
  - le soleil et la lune (`stormSunlight`, 15 %), le ciel (`stormSky`, 35 %) et la lumière ambiante (`stormAmbient`, 60 %) baissent : c'est le [[Cycle jour et nuit]] qui applique ces facteurs ;
  - le ciel (skybox procédural) prend la couleur de la poussière (`_SkyTint`, `_AtmosphereThickness`) ;
  - brouillard (exponentiel carré) brun-orange (`stormColor`) jusqu'à `stormFogDensity` (0,07 : presque rien au-delà de 30 m), assombri la nuit (`DayNightCycle.LightLevel`) ;
  - du sable souffle autour de la caméra dans le sens du vent (système de particules créé en jeu, shader `dustShader`).
  - Dans un bâtiment (`BuildingInterior.AnyContains`) : pas de sable, et le brouillard garde `indoorFog` (20 %) de sa densité.
- **Temps clair** : une brume de poussière légère au loin (`clearFogDensity` 0,001, couleur `hazeColor`). Les réglages de brouillard de la scène sont rendus quand le monde est déchargé.
- **Test** : la case `testStorm` de l'Inspector (ou le menu contextuel « Start a storm now ») lance une tempête tout de suite, avec son annonce (non sauvegardée). `state`, `windSpeed` et `storm` montrent la météo en jeu.
- **Pour les autres systèmes** : `WindSpeed`, `WindFrom`, `WindVelocity`, `State`, `IsStorm`, `IsStormComing`, `StormIntensity` (0 à 1), `SolarFactor`, `SunFactor`, `SkyFactor`, `AmbientFactor`.

Voir aussi : [[Cycle jour et nuit]] · [[Éolienne]] · [[Panneau solaire]] · [[04 - Couleur et lumière]]
