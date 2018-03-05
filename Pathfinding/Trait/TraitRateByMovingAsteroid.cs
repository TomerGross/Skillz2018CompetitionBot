using Pirates;
using System.Linq;
using System.Collections.Generic;

namespace Hydra {

    public class TraitRateByMovingAsteroid : Trait {

        readonly int range;


        public TraitRateByMovingAsteroid(int range) {

            this.range = range;
        }


        override public int Cost(Chunk chunk) {

            PirateGame game = Main.game;
            int cost = 0;

            foreach (Asteroid asteroid in Main.game.__livingAsteroids.Where(a => Utils.AsteroidIsMoving(a))) {
                foreach (Location next in GetNextLocations(asteroid).Where(loc => chunk.Distance(loc) < range)) {
                    cost += (range - chunk.Distance(asteroid)) * 10;
                }
            }

            return cost;
        }


        public List<Location> GetNextLocations(Asteroid asteroid) {

            var nextLocations = new List<Location>();
            var loc = asteroid.Location.Add(asteroid.Direction);

            for (int i = 1; i <= 5; i++) {

                loc = asteroid.Location.Add(asteroid.Direction);

                if (loc.InMap()) {

                    if (!nextLocations.Contains(loc)) {
                        nextLocations.Add(loc);
                    }

                    loc = loc.Add(asteroid.Direction);
                }
            }

            return nextLocations;
        }
    }

}