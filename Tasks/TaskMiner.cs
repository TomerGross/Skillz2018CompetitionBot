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
                Chunk endgoal = Chunk.GetChunk(game.GetMyMotherships()[0].Location);

                var traits = new List<Trait>() {
                        new TraitRateByEnemy(2, 1,-1),
                        new TraitAttractedToGoal(3, game.GetMyMotherships()[0]),
                        new TraitRateByEdges(5, 2),
                        new TraitRateByAsteroid(2)
                };

                Path path = new Path(origin, endgoal, traits, Path.Algorithm.ASTAR);

                if (path.GetChunks().Count > 0) {

                    Chunk nextChunk = path.Pop();
                    pirate.Sail(nextChunk.GetLocation());
                    return Utils.GetPirateStatus(pirate, "Sailing to: " + nextChunk.ToString());
                }

                pirate.Sail(game.GetMyMotherships()[0]);
                return Utils.GetPirateStatus(pirate, "Is idle");
            }

            if (Main.mines.Count > 0) {
                pirate.Sail(Utils.OrderByDistance(Main.mines, pirate.Location).First());
                return Utils.GetPirateStatus(pirate, "Sailing to mine...");
            }

            return Utils.GetPirateStatus(pirate, "Is idle....");
        }


        override public double GetWeight() {

            if (game.GetMyCapsules().Count() == 0 || Utils.PiratesWithTask(TaskType.MINER).Count >= Main.maxMiners) {
                return -100;
            }

            // If there is no free capsule a miner is not needed...
            if (Utils.FreeCapsulesByDistance().Count() == 0) {
                return 0;
            }

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(Utils.FreeCapsulesByDistance(pirate.Location).Last()));
            double distance = pirate.Distance(Utils.FreeCapsulesByDistance(pirate.Location).First());

            return ((maxDis - distance) / maxDis) * 100;
        }


        override public int Bias() {

            if (game.GetMyCapsules().Count() == 0) {
                return 0;
            }


            return 100;
        }


    }
}
