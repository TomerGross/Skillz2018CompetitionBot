using Pirates;

namespace Hydra {

    public class TraitRateByLazyAsteroid : Trait {
        
        readonly int range;


        public TraitRateByLazyAsteroid(int range) {

            this.range = range;
        }


        override public int Cost(Chunk chunk) {
            
            PirateGame game = Main.game;

            foreach (Asteroid asteroid in game.GetAllAsteroids()) {
                if (asteroid.Distance(chunk.GetLocation()) < range * Chunk.size + asteroid.Size) {
                    return 100000;
                }
            }

            return 0;
        }


    }

}