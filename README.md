LudumDare
=========

##Ludum Dare #27

Theme: 10 seconds.


###concept: 
A disaster happens every ten seconds. Can you keep up? 

In actuality, monsters spawn from portals, and you have to do clean-up.

###Todo:
 - Assert different tile sizes work.
 - Better attack 
 - Game conditions
  - Win condition
  - Final battle
  - Threat level
   - more concrete display? 
 - Upgrades
  - AoE attack, 
  - Flight, 
  - Closing portal
  - badassery
 - Graphics
  - draw-scan-paint-etc.
 - Refactor

###Done:
- Threat Level display
- Engine Upgrades
- Attack and damage
  - Remove health-less creatures from board
 - Walkable areas
 -- check if tile = 1 at some layer / contains property walkable.
 - Portals
  - Spawn creatures if active at count of 10. 
 - Creatures
  - Move around
