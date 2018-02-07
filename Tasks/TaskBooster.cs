using System.Linq;
using System.Collections.Generic;
using Pirates;

namespace Hydra {

    public class TaskBooster : Task {


        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------


        readonly int radius; // Maximum distance from holder
        readonly Pirate pirate;


        public TaskBooster(Pirate pirate, int radius) {

            this.pirate = pirate;
            this.radius = radius;
        }


        public TaskBooster(Pirate pirate) {

            this.pirate = pirate;
            radius = 500;
        }


        override public string Preform() {

            if (Utils.GetMyHolders().Count() > 0 && game.GetAllMotherships().Count() != 0) {

                var ship = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First();
                var holders = Utils.GetMyHolders().Where(h => !Main.holdersPaired.Contains(h)).OrderBy(h => h.Distance(pirate));
                var cloestHolder = Utils.GetMyHolders()[0];

                if (holders.Count() > 0) {
                    cloestHolder = holders.First();
                }

                if (pirate.Distance(cloestHolder) >= radius) {
                    Main.holdersPaired.Add(cloestHolder);
                    Utils.SafeSail(pirate, cloestHolder.Location);
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

                foreach (Pirate holder in Utils.GetMyHolders().OrderBy(h => h.Distance(pirate))) {

                    if (pirate.CanPush(holder) && !Main.piratesPushed.Contains(holder)) {

                        var threats = new List<MapObject>();
                        threats.AddRange(game.GetEnemyLivingPirates().ToList());
                        threats.RemoveAll(threat => ((Pirate)threat).PushReloadTurns > 2);
                        threats.AddRange(game.GetAllAsteroids().ToList());
                        threats.RemoveAll(threat => threat.Distance(ship) > holder.Distance(ship) + holder.PushRange);
                        threats = threats.OrderBy(treath => treath.Distance(ship)).ToList();

                        bool caseI = false;
                        if (threats.Count() > 0 && threats.First().Distance(ship) > holder.Distance(ship)) {
                            caseI = threats.First().Distance(ship) - holder.Distance(ship) > pirate.PushRange;
                        }

                        if (threats.Count() == 0 && holder.Distance(ship) < Chunk.size * 3) {
                            caseI = true;
                        }

                        bool caseII = holder.Distance(ship) <= game.PushDistance + holder.MaxSpeed;
                        /// bool caseIII = holder.Distance(threats.First()) < game.PushRange;

                        // game.Debug("case1: " + caseI + "   case2:  " + caseII);

                        if (caseI || caseII) {

                            if (Utils.GetMyHolders().Count() > 1) {

                                var secondHolder = Utils.GetMyHolders().Where(h => h.Id != holder.Id).First();

                                if (!Main.didTurn.Contains(holder.Id) && holder.CanPush(secondHolder) && !Main.piratesPushed.Contains(secondHolder) && secondHolder.Distance(ship) < game.PushDistance + secondHolder.MaxSpeed * 1.5) {

                                    holder.Push(secondHolder, ship);
                                    Main.piratesPushed.Add(secondHolder);

                                    pirate.Push(holder, ship);
                                    Main.piratesPushed.Add(holder);

                                    if (!Main.didTurn.Contains(secondHolder.Id)) {

                                        var edge = Utils.CloestEdge(threats.First().GetLocation());
                                        secondHolder.Push((Pirate)threats.Where(p => p is Pirate).First(), edge.Item2);
                                        Main.didTurn.Add(secondHolder.Id);
                                    }

                                    Main.didTurn.Add(holder.Id);
                                    Main.didTurn.Add(pirate.Id);
                                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship while the holder boosted another");
                                }
                            }

                            if (!Main.didTurn.Contains(holder.Id)) {
                                holder.Sail(ship);
                                Main.didTurn.Add(holder.Id);
                            }

                            pirate.Push(holder, ship);
                            Main.piratesPushed.Add(holder);
                            return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                        }
                    }
                }

                Utils.SafeSail(pirate, Utils.GetMyHolders().OrderBy(h => h.Distance(pirate)).First().Location);
                return Utils.GetPirateStatus(pirate, "Sailing towards holder");
            }

            if (Main.mines.Count > 0 && game.GetMyMotherships().Count() > 0) {

                var nearestMine = Utils.OrderByDistance(Main.mines, pirate.Location).First();
                var nearestShipToMine = Utils.OrderByDistance(game.GetMyMotherships().ToList(), nearestMine).First();
                var locTowards = nearestMine.Towards(nearestShipToMine, game.PirateMaxSpeed * 2);

                Utils.SafeSail(pirate, locTowards);
                return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
            }

            return Utils.GetPirateStatus(pirate, "Is idle.");
        }


        override public double GetWeight() {

            if (game.GetMyCapsules().Count() == 0) {
                return 0;
            }

            if (Utils.GetMyHolders().Count() > 0 && game.GetEnemyLivingPirates().Count() > 0) {
                var cloestMom = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First().GetLocation();
                var cloestEnemyToMom = Utils.OrderByDistance(game.GetEnemyLivingPirates().ToList(), cloestMom).First();
                var cloestHolder = Utils.GetMyHolders().OrderBy(holder => holder.Distance(pirate)).First();

                if (cloestEnemyToMom.Distance(cloestMom) > cloestHolder.Distance(cloestMom)) {
                    return -50;
                }
            }

            if (game.GetMyCapsules().Count() > 0) {

                var capsule = Utils.OrderByDistance(game.GetMyCapsules().ToList(), pirate.Location).First().GetLocation();

                if (Utils.GetMyHolders().Count > 0) {
                    capsule = Utils.GetMyHolders().OrderBy(hold => hold.Distance(pirate)).First().Location;
                }

                double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(capsule));
                return ((maxDis - pirate.Distance(capsule)) / maxDis) * 100;
            }

            return 0;
        }


        override public int Bias() {

            // TODO  game.GetMyCapsules().Where(cap => cap.Holder == null).Count() != 0  ---> return 0
            if (game.GetMyCapsules().Count() == 0 || Utils.GetMyHolders().Count() <= Utils.PiratesWithTask(TaskType.BOOSTER).Count()) {
                return -100000;
            }

            if (Utils.PiratesWithTask(TaskType.BOOSTER).Count() == 0) {
                return 100;
            }

            return 0;
        }


    }
}
