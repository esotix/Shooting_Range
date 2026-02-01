# Configuration du Drone RB-3DCP

Ce document définit les contraintes de navigation et les paramètres de comportement du drone pour l'environnement VR.

## Paramètres de Navigation

| Paramètre | Valeur | Justification |
| :--- | :--- | :--- |
| **N (Directions)** | `32` | Équilibre optimal entre précision (couverture 360°) et performance CPU. |
| **coneAngle** | `60°` | Permet de chercher des chemins alternatifs sans dévier de la trajectoire cible. |
| **lookAheadBase** | `3.5 m` | Distance mini d'anticipation (doit être > 2x le clearanceRadius). |
| **lookAheadFactor**| `1.2` | Calcule l'anticipation selon la vitesse ($L = 3.5 + 1.2v$). |
| **clearanceRadius** | `0.4 m` | Rayon de sécurité (taille drone + marge) pour passages de >0.8m. |

## Spécifications Physiques

| Paramètre | Valeur | Justification |
| :--- | :--- | :--- |
| **maxSpeed** | `5 m/s` | Aligné sur la vitesse de déplacement d'un joueur standard. |
| **maxAccel** | `4 m/s²` | Freinage sur ~3.1m, garantissant l'arrêt avant l'obstacle (`lookAhead`). |
| **maxTurnRate** | `120°/s` | Rotation fluide permettant un demi-tour en 1.5s sans oscillations. |
| **stoppingDist** | `1.0 m` | Zone de confort autour du joueur pour éviter les micro-corrections. |

## Pondérations du Scoring (Behavior Tree / Utility AI)

| Poids | Valeur | Rôle |
| :--- | :--- | :--- |
| **wSafe** | `2.5` | **Priorité Critique** : Évitement des collisions sur layer *Industrial*. |
| **wFollow** | `1.5` | **Priorité Haute** : Suivi de l'offset 3D du joueur. |
| **wLoS** | `1.0` | **Maintien Visuel** : Évite de se perdre derrière des murs/poutres. |
| **wDyn** | `0.8` | **Lissage** : Pénalise les changements de direction brusques. |

## Interaction
En appuyant sur le bouton principal de la manette gauche, on lance l'interaction avec le drone.
