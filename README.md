# Skillz 2018 Competition Bot - עמי אסף דרום השרון 1

Throughout the past month (February - March 2018), my friend Matan Rak and i have been developing our bot. 
It won 8th place in Skillz 2018 - The israeli cyber competition.
Our main goal was to make it as simple and adaptable as possible, so it would be easy to deal with the new feature that arrived each week.
Our strategy was to first of all, split the map into chunks of the same size. For each chunk, we calculated its cost, based on our traits. Traits might have had a positive impact on the cost but could have also had a negative impact. Then, our pirate force split into roles. Each turn, a pirate was assigned the optimal task for it.

The roles:
  1. Miner - unloads a capsule in our mothership.
  2. Moler - prevents from enemy pirates from unloading their capsules in their mothership. 
  3. Booster - pushes our miner to score points.
  
The traits:
  1. TraitAttractedToGoal - calculated by the distance from the goal.
  2. TraitRateByEdges - calculated by the distance from the closest edge.
  3. TraitRateByEnemy - calculated by the enemies we need to avoid.
  4. TraitRateByLazyAsteroid - attempt to avoid static asteroids in the way.
  5. TraitRateByMovingAsteroid - attempt to avoid moving asteroids in the way.
  6. TraitRateByStickyBomb - attempt to avoid the sticky bombs' blast radii.
  7. TraitWormhole - attempt to avoid falling into wormholed.
