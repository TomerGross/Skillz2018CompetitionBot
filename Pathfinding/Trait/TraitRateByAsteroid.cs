using Pirates;

namespace Hydra {

    public class TraitRateByAsteroid : Trait {

        readonly int range;


        public TraitRateByAsteroid(int range) {

            this.range = range;
        }


        override public int Cost(Chunk chunk) {

            PirateGame game = Main.game;
            int cost = 0;

            foreach (Asteroid asteroid in game.GetAllAsteroids()) {
                if (asteroid.Distance(chunk.GetLocation()) < range * 320) {
                    return 100000;
                }
            }

            return cost;
        }


    }

}