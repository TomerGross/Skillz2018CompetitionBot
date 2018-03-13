# Skillz 2018 Competition Bot - עמי אסף דרום השרון 1

Along the last month (February - March, 2018) Me and my friend Matan Rak developed this bot. our bot picked the 8 place in Skillz 2018 - The israeli cyber competition.
Our major target was to make it as generic as we can so it would be easy to deal with the new feature that arrived every week.
The strategy is first of all spliting the map into chunks with the same size. To every chunk we calculate his cost, that set by our traits. Trait can be a good thing or a bad one. Our pirate force split into tasks, each turn a pirate is assigned the optiomal task for him.

The tasks:
  1. Miner - unload a capsule in our mothership.
  2. Moler - prevents from enemy pirates to unload there capsules in their mothership. 
  3. Booster - boost our miner to his goal to make points.
  
The traits:
  1. TraitAttractedToGoal - calculated by distance from goal.
  2. TraitRateByEdges - calculated by distance from closest edge.
  3. TraitRateByEnemy - calculated by enemys we need to avoid.
  4. TraitRateByLazyAsteroid - make us avoid from unmoving asteroids in way.
  5. TraitRateByMovingAsteroid - make us avoid from moving asteroids in way.
  6. TraitRateByStickyBomb - make us avoid from sticky bombs in explosion range.
  7. TraitWormhole - prevent get into wormholes.

  




