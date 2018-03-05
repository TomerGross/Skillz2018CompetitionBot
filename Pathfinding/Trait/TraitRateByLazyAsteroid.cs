using Pirates;
using System.Linq;

namespace Hydra {

    public class TraitRateByLazyAsteroid : Trait {

        readonly int range;


        public TraitRateByLazyAsteroid(int range) {
            this.range = range;
        }


        override public int Cost(Chunk chunk) {

            foreach (Asteroid asteroid in Main.game.GetAllAsteroids().Where(a => a.IsAlive() || a.TurnsToRevive <= 2)) {
                if (chunk.Distance(asteroid) <= range + asteroid.Size) {
                    return 100000;
                }
            }
   
            return 0;
        }


    }

}