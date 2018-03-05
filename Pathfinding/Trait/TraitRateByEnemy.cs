using Pirates;
using System.Linq;

namespace Hydra {

    public class TraitRateByEnemy : Trait {

        readonly int setRange, multiplier, pirateID;

        //if the bias is minus the cost will make the pirate attracted to enemies
        //if pirateID is -1 the trait will target all enemies, if it's an ID it will only effect the given pirate


        public TraitRateByEnemy(int setRange, int multiplier, int pirateID) {
            
            this.setRange = setRange;
            this.multiplier = multiplier;
            this.pirateID = pirateID;
        }


        override public int Cost(Chunk chunk) {

            PirateGame game = Main.game;
            int cost = 0;
            var pirates = game.GetAllEnemyPirates().ToList();

            if (pirateID != -1) {
                pirates.AddRange(game.GetAllMyPirates());
            }

            foreach (Pirate pirate in pirates.Where(e => chunk.Distance(e) < setRange + e.PushRange + e.MaxSpeed)) {
                if (pirateID == -1 || pirate.UniqueId == pirateID) {
                    int range = setRange + pirate.PushRange + pirate.MaxSpeed;
                    cost += (range - chunk.Distance(pirate)) * multiplier;
                }
            }

            return cost;
        }


    }
}