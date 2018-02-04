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

            if (Utils.PushAsteroid(pirate)) {
                return Utils.GetPirateStatus(pirate, "Pushed asteroid");
            }
            
            if (pirate.HasCapsule()) {

                var nearestShip = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First();

                Chunk origin = Chunk.GetChunk(pirate.GetLocation());
                Chunk endgoal = Chunk.GetChunk(nearestShip);

                var traits = new List<Trait>() {
                        new TraitRateByEnemy(2, 1,-1),
                        new TraitAttractedToGoal(2, nearestShip),
                        new TraitRateByEdges(5, 2),
                        new TraitRateByAsteroid(0)
                };

                Path path = new Path(origin, endgoal, traits, Path.Algorithm.ASTAR);

                if (path.GetChunks().Count > 0 && nearestShip.Distance(pirate.Location) > Chunk.size) {


                    Chunk nextChunk = path.Pop();

                    if (!Utils.PushAsteroid(pirate)) {

                        pirate.Sail(nextChunk.GetLocation());
                        return Utils.GetPirateStatus(pirate, "Sailing to: " + nextChunk.ToString());
                    }

                    return Utils.GetPirateStatus(pirate, "Pushed asteroid");
                }

                pirate.Sail(Utils.SafeSail(pirate, nearestShip));
                return Utils.GetPirateStatus(pirate, "Sailing to ship");
            }

            if (Utils.FreeCapsulesByDistance(pirate.Location).Count > 0) {
                if (!Utils.PushAsteroid(pirate)) {

                    pirate.Sail(Utils.SafeSail(pirate, Utils.FreeCapsulesByDistance(pirate.Location).First()));
                    return Utils.GetPirateStatus(pirate, "Sailing to mine... " + pirate.PushReloadTurns);
                }

                return Utils.GetPirateStatus(pirate, "Pushed asteroid");
            }


            //need to check where the miner goes
            return Utils.GetPirateStatus(pirate, "Is idle....");
        }


        override public double GetWeight() {

            if (pirate.HasCapsule()) {
                return 1000;
            }

            if (game.GetMyCapsules().Count() == 0 || Utils.PiratesWithTask(TaskType.MINER).Count >= game.GetMyCapsules().Count()) {
                return -100;
            }

            // If there is no free capsule a miner is not needed...
            if (Utils.FreeCapsulesByDistance(pirate.Location).Count() == 0) {
                return 0;
            }

            double maxDis = Main.unemployedPirates.Max(unemployed => unemployed.Distance(Utils.FreeCapsulesByDistance(unemployed.GetLocation()).Last()));
            double distance = pirate.Distance(Utils.FreeCapsulesByDistance(pirate.Location).First());
            double weight = ((maxDis - distance) / maxDis) * 100;

            if (pirate.PushReloadTurns > 2) {
                weight += 15;
            }

            return weight;
        }


        override public int Bias() {

            if (game.GetMyCapsules().Count() == 0) {
                return 0;
            }


            return 100;
        }


    }
}
