using Pirates;
using System.Linq;

namespace Hydra {

    public class TraitWormhole : Trait {

        readonly Location goal;
        readonly Pirate pirate;

        public TraitWormhole(Location goal, Pirate pirate) {

            this.goal = goal;
            this.pirate = pirate;
        }


        override public int Cost(Chunk chunk) {

            var holesInChunk = Main.game.GetAllWormholes().Any(w => chunk.Distance(w) <= w.WormholeRange + Chunk.size);

            if(holesInChunk)
                if (chunk.Distance(goal) == Utils.DistanceWithWormhole(chunk.GetLocation(), goal, pirate.MaxSpeed))
                    return 10000;
           
            return 0;
        }

    }
}