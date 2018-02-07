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

            int cost = 0;

            foreach (Asteroid asteroid in Main.game.GetAllAsteroids().ToList()){

                if (IsMoving(asteroid)) {
                    foreach (Chunk next in GetNextChunks(asteroid).Where(n => n.Distance(chunk) < Chunk.size * range)) {
                        cost += 10000 - next.Distance(chunk);
                    }
                    continue;
                }

                cost += new TraitRateByLazyAsteroid(0).Cost(chunk);
            }
         
            return cost;
        }


        public bool IsMoving(Asteroid ast) {
            return ast.Direction.Col != 0 && ast.Direction.Row != 0;
        }


        public List<Chunk> GetNextChunks(Asteroid asteroid) {

            var nextChunks = new List<Chunk>();
            var loc = asteroid.Location.Add(asteroid.Direction);

            for (int i = 1; i <= 5; i++) {
                if (loc.InMap()) {
                    
                    Chunk chunk = Chunk.GetChunk(loc);
                    if (!nextChunks.Contains(chunk)) {
                        nextChunks.Add(chunk);
                    }

                    loc = loc.Add(asteroid.Direction);
                }
            }


            return nextChunks;
        }


    }

}