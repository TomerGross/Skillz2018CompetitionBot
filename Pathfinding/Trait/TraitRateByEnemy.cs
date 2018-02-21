using Pirates;
using System.Linq;

namespace Hydra {

    public class TraitRateByEnemy : Trait {

        readonly int range, multiplier, pirateID;

        //if the bias is minus the cost will make the pirate attracted to enemies
        //if pirateID is -1 the trait will target all enemies, if it's an ID it will only effect the given pirate


        public TraitRateByEnemy(int range, int multiplier, int pirateID) {

            this.range = range;
            this.multiplier = multiplier;
            this.pirateID = pirateID;
        }


        override public int Cost(Chunk chunk) {

            PirateGame game = Main.game;
            int cost = 0, PDistance = game.PushDistance, PRange = game.PushRange;
            var pirates = game.GetAllEnemyPirates().ToList();

            if (pirateID != -1) {
                pirates.AddRange(game.GetAllMyPirates());
            }

            int maxDistance = Chunk.size * range;

            foreach (Pirate pirate in pirates.Where(p => chunk.Distance(p) < maxDistance)) {
                if (pirateID == -1 || pirate.UniqueId == pirateID) {                             
                    if (pirate.Distance(chunk.GetLocation()) < Chunk.size * range) {
                        cost += (maxDistance - pirate.Distance(chunk.GetLocation())) * multiplier;
                    }
                }
            }

            return cost;
        }


    }

}