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

            var holesInChunk = Main.game.GetAllWormholes().Any(w => chunk.Distance(w) <= w.WormholeRange && pirate.Distance(w) / pirate.MaxSpeed + 2 <= w.TurnsToReactivate);

            if (holesInChunk) {
                if (pirate.HasCapsule() && Utils.PiratesWithTask(TaskType.BOOSTER).Any()) {
                    return 100000;
                }

                if (chunk.Distance(goal) == Utils.DistanceWithWormhole(chunk.GetLocation(), goal, pirate.MaxSpeed)) {
                    return 100000;
                }
            }

            return 0;
        }

    }
}