using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class TaskMiner : Task {

        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------

        readonly Pirate pirate;


        public TaskMiner(Pirate pirate) {
            this.pirate = pirate;
        }


        override public string Preform() {

            if (pirate.HasCapsule()) {

                Chunk origin = Chunk.GetChunk(pirate.GetLocation());
                Chunk endgoal = Chunk.GetChunk(game.GetMyMothership().Location);

                var traits = new List<Trait>() {
                        new TraitRateByEnemy(2, 1,-1),
                        new TraitAttractedToGoal(3, game.GetMyMothership()),
                        new TraitRateByEdges(5, 2)
                };

                Path path = new Path(origin, endgoal, traits, Path.Algorithm.ASTAR);

                if (path.GetChunks().Count > 0) {

                    Chunk nextChunk = path.Pop();
                    pirate.Sail(nextChunk.GetLocation());
                    return Utils.GetPirateStatus(pirate, "Sailing to: " + nextChunk.ToString());
                }

                pirate.Sail(game.GetMyMothership());
                return Utils.GetPirateStatus(pirate, "Is idle");
            }

            pirate.Sail(Main.mine);
            return Utils.GetPirateStatus(pirate, "Sailing to mine...");
        }


        override public double GetWeight() {

            if (Utils.PiratesWithTask(TaskType.MINER).Count >= Main.maxMiners) {
                return -100;
            }

            if (game.GetMyCapsule().Holder == null) {

                var pairs = Utils.SoloClosestPair(game.GetMyLivingPirates(), game.GetMyCapsule());

                int index = pairs.IndexOf(pairs.First(tuple => tuple.Item1 == pirate));
                int numofpirates = game.GetAllMyPirates().Length;

                return (numofpirates - index) * (100 / numofpirates);
            }

            if (game.GetMyCapsule().Holder == pirate) {
                return 1000; //he is already in his task
            }

            return 0; // only one miner for this lvl of competition     
        }


        override public int Bias() {

            //if (Utils.PiratesWithTask(TaskType.MINER).Count >= Main.maxMiners) {
            //  return 0;
            //}

            return 100;
        }


    }
}
