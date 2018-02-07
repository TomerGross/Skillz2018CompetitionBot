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

            if (Main.didTurn.Contains(pirate.Id)) {
                return Utils.GetPirateStatus(pirate, "Already did turn");
            }

            if (pirate.HasCapsule()) {

                var nearestShip = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First();
                var threats = game.GetEnemyLivingPirates().ToList().Where(e => e.Distance(nearestShip) < pirate.Distance(nearestShip));
                threats = threats.OrderBy(treath => treath.Distance(nearestShip)).ToList();

                Chunk endgoal = Chunk.GetChunk(nearestShip);
                /*
                if (Utils.PiratesWithTask(TaskType.BOOSTER).Count > 0) {

                    var closestBooster = Utils.OrderByDistance(Utils.PiratesWithTask(TaskType.BOOSTER), pirate.Location).First();
                    var cloestHolderToBooster = Utils.OrderByDistance(Utils.GetMyHolders(), closestBooster.Location).First();

                    if(cloestHolderToBooster == pirate){
                        if(closestBooster.Distance(pirate) > game.PushRange){
                            pirate.Sail(closestBooster);
                            return Utils.GetPirateStatus(pirate, "Sailing closer to booster");
                        } 
                    }
                }
*/
                var closestenemy = game.GetEnemyLivingPirates().ToList().First();
                if (Utils.GetMyHolders().Count() > 1) {

                    var secondHolder = Utils.GetMyHolders().Where(h => h.Id != pirate.Id).First();
                    var nearestShipToSecondHolder = Utils.OrderByDistance(game.GetMyMotherships().ToList(), secondHolder.Location).First();

                    if (threats.Count() > 0 && Utils.GetNumOfEnemyPiratesOnPoint(closestenemy.GetLocation()) >= 4) {

                        bool caseI = nearestShip.Equals(nearestShipToSecondHolder);
                        bool caseII = pirate.Distance(nearestShip) < secondHolder.Distance(nearestShip);
                        bool caseIII = pirate.Distance(secondHolder) < pirate.Distance(nearestShip);
                        bool caseIV = game.PushRange * 3 < pirate.Distance(secondHolder);
                        bool caseV = game.PushRange < pirate.Distance(nearestShip);

                        if (caseI && caseII && caseIII && caseIV && caseV) {
                            // pirate.Sail(secondHolder);
                            //    return Utils.GetPirateStatus(pirate, "Sailing closer to second holder");
                        }
                    }

                    var secondThreats = game.GetEnemyLivingPirates().ToList().Where(e => e.CanPush(secondHolder) && e.Distance(nearestShip) < secondHolder.Distance(nearestShip));
                    secondThreats = secondThreats.OrderBy(treath => treath.Distance(secondHolder)).ToList();

                    bool caseVI = secondHolder.Distance(nearestShip) < pirate.Distance(nearestShip);
                    bool caseVII = secondHolder.Distance(nearestShip) < game.PushDistance + secondHolder.MaxSpeed * 1.5;

                    if (pirate.CanPush(secondHolder) && caseVI && caseVII && !Main.piratesPushed.Contains(secondHolder)) {
                        pirate.Push(secondHolder, nearestShipToSecondHolder);
                        Main.piratesPushed.Add(secondHolder);
                        return Utils.GetPirateStatus(pirate, "Boosted second holder");
                    }

                }

                var traits = new List<Trait>() {
                        new TraitRateByEnemy(1, 1, -1),
                        new TraitRateByEdges(2, 1),
                        new TraitRateByLazyAsteroid(1),
                        new TraitRateByMovingAsteroid(4)
                };

                if (Utils.GetMyHolders().Count() > 1) {
                    var secondHolder = Utils.GetMyHolders().Where(h => h.Id != pirate.Id).First();

                    if(Utils.GetNumOfEnemyPiratesOnPoint(closestenemy.GetLocation()) < 4){
                        traits.Add(new TraitRateByEnemy(5, 100, secondHolder.UniqueId));
                    }else{
                        traits.Add(new TraitRateByEnemy(5, -100, secondHolder.UniqueId));
                    }
                }

                if (pirate.Distance(nearestShip) > 300) {

                    Path path = new Path(Chunk.GetChunk(pirate.GetLocation()), endgoal, traits, Path.Algorithm.ASTAR);

                    if (path.GetChunks().Count > 0 && nearestShip.Distance(pirate.Location) > Chunk.size) {

                        Chunk nextChunk = path.Pop();

                        if (Utils.PushAsteroid(pirate)) {
                            return Utils.GetPirateStatus(pirate, "Pushed asteroid");
                        }

                        pirate.Sail(nextChunk.GetLocation());
                        return Utils.GetPirateStatus(pirate, "Sailing to: " + nextChunk.ToString());
                    }
                }

                Utils.SafeSail(pirate, nearestShip);
                return Utils.GetPirateStatus(pirate, "Sailing to ship");
            }


            if (Utils.PushAsteroid(pirate)) {
                return Utils.GetPirateStatus(pirate, "Pushed asteroid");
            }

            if (Main.mines.Count() > 0) {

                var targetCapsule = Utils.OrderByDistance(Main.mines, pirate.Location).First();

                if (Main.capsulesTargetted.ContainsKey(pirate)) {
                    targetCapsule = Main.capsulesTargetted[pirate].Location;
                }

                Utils.SafeSail(pirate, targetCapsule);
                return Utils.GetPirateStatus(pirate, "Sailing to target capsule... ");
            }

            return Utils.GetPirateStatus(pirate, "Is idle... ");
        }



        override public double GetWeight() {

            if (pirate.HasCapsule()) {
                return 1000;
            }

            if (game.GetMyCapsules().Count() == 0 || Utils.FreeCapsulesByDistance(pirate.Location).Count() == 0 || Utils.PiratesWithTask(TaskType.MINER).Count >= game.GetMyCapsules().Count() || Utils.FreeCapsulesByDistance(pirate.Location).Count() == 0) {
                return -Bias() - 50;
            }

            var cloestCapsule = Utils.FreeCapsulesByDistance(pirate.Location).First();
            double maxDis = Main.unemployedPirates.Max(unemployed => unemployed.Distance(Utils.FreeCapsulesByDistance(unemployed.GetLocation()).Last()));
            double weight = ((double)(maxDis - pirate.Distance(cloestCapsule) / maxDis)) * 100;

            if (pirate.PushReloadTurns > 2) {
                weight += 20;
            }

            return weight;
        }


        override public int Bias() {

            if (game.GetMyCapsules().Count() == 0 || Utils.FreeCapsulesByDistance(pirate.Location).Count() == 0) {
                return 0;
            }

            return 100;
        }


    }
}
