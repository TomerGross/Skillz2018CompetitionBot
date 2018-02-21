using System.Linq;
using System.Collections.Generic;
using Pirates;

namespace Hydra {

    public class TaskBooster : Task {

        public static PirateGame game = Main.game;


        readonly int radius; // Maximum distance from holder
        Pirate pirate;


        public TaskBooster(Pirate pirate, int radius) {

            this.pirate = pirate;
            this.radius = radius;
        }


        public TaskBooster(Pirate pirate) {

            this.pirate = pirate;
            radius = pirate.PushRange;
        }


        override public void UpdatePirate(Pirate pirate) => this.pirate = pirate;


        override public int Priority() => 2;


        override public string Preform() {

            if (!Utils.GetMyHolders().Any() || !game.GetMyCapsules().Any() || !game.GetAllMotherships().Any()) {

                if (Main.mines.Count > 0 && game.GetMyMotherships().Any()) {
                    var sailLocation = Utils.GetMyHolders().OrderBy(h => h.Distance(pirate)).First().Location;
                    return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point, " + Sailing.SafeSail(pirate, sailLocation));
                }

                return Utils.GetPirateStatus(pirate, "Is idle.");
            }

            var holders = Utils.GetMyHolders().OrderBy(h => h.Distance(pirate));
            var cloestHolder = holders.First();

            foreach (Pirate holder in Utils.GetMyHolders().OrderBy(h => h.Distance(pirate))) {

                var nearestShip = Utils.OrderByDistance(game.GetMyMotherships().ToList(), holder.Location).First();

                if (!pirate.CanPush(holder)) {

                    var sailLocation = Utils.GetMyHolders().OrderBy(h => h.Distance(pirate)).First().Location;
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder, " + Sailing.SafeSail(pirate, sailLocation));
                }

                var threats = game.GetEnemyLivingPirates().Where(t => t.PushReloadTurns > 2 && t.Distance(nearestShip) < pirate.Distance(nearestShip) * 1.5).OrderBy(nearestShip.Distance);

                // Checks if the holder can be pushed directly onto the ship
                bool caseI = holder.Distance(nearestShip) - game.MothershipUnloadRange <= pirate.PushDistance + holder.MaxSpeed;

                bool caseII = false;
                if (threats.Any() && threats.First().Distance(nearestShip) > holder.Distance(nearestShip))
                    caseII = holder.Distance(nearestShip) - threats.First().Distance(nearestShip) < pirate.PushRange;

                var holderLocAfterPush = holder.GetLocation().Towards(nearestShip, pirate.PushDistance + holder.MaxSpeed / 2);
                bool caseIII_PI = threats.Any() && threats.First().Distance(nearestShip) < holder.Distance(nearestShip);
                bool caseIII_PII = threats.Any() && threats.First().PushRange < holderLocAfterPush.Distance(threats.First());
                bool ImminentDeath = game.GetEnemyLivingPirates().Count(t => holder.InRange(t, holder.MaxSpeed + t.MaxSpeed + t.PushRange) && t.PushReloadTurns <= 1) >= holder.NumPushesForCapsuleLoss;

                if (ImminentDeath && !caseI && holder.MaxSpeed * 4 < holder.Distance(nearestShip)) {

                    var safest = Sailing.SafestCloestLocation(holder.Location, nearestShip.Location, 2, true, pirate);
                    pirate.Push(holder, safest);
                    Main.piratesPushed.Add(holder);
                    Main.didTurn.Add(pirate.Id);

                    if (!Main.didTurn.Contains(holder.Id)) {
                        holder.Push(pirate, safest);
                        Main.piratesPushed.Add(pirate);
                        Main.didTurn.Add(holder.Id);
                    }

                    return Utils.GetPirateStatus(pirate, "Moved away from danger zone");
                }

                bool caseIII = threats.Any() && caseIII_PI && caseIII_PII && ImminentDeath;

                game.Debug(caseI + " || " + caseII + " || " + caseIII + " +| IMD: " + ImminentDeath);

                if (caseI || caseIII) {
                    if (Utils.GetMyHolders().Count() > 1) {

                        var secondHolder = Utils.GetMyHolders().First(h => h.Id != holder.Id);

                        if (!Main.didTurn.Contains(holder.Id) && holder.CanPush(secondHolder) && !Main.piratesPushed.Contains(secondHolder) && secondHolder.Distance(nearestShip) - nearestShip.UnloadRange < pirate.PushDistance + secondHolder.MaxSpeed) {

                            holder.Push(secondHolder, nearestShip);
                            Main.piratesPushed.Add(secondHolder);

                            pirate.Push(holder, nearestShip);
                            Main.piratesPushed.Add(holder);

                            if (!Main.didTurn.Contains(secondHolder.Id) && holder.NumPushesForCapsuleLoss > 2 && secondHolder.CanPush(holder)) {

                                secondHolder.Push(holder, nearestShip);
                                Main.piratesPushed.Add(holder);
                            }

                            Main.didTurn.Add(holder.Id);
                            Main.didTurn.Add(pirate.Id);
                            return Utils.GetPirateStatus(pirate, "Pushed holder to the motherShip while the holder boosted another holder");
                        }
                    }

                    if (!Main.didTurn.Contains(holder.Id)) {
                        holder.Sail(nearestShip);
                        Main.didTurn.Add(holder.Id);
                    }

                    pirate.Push(holder, nearestShip);
                    Main.didTurn.Add(pirate.Id);
                    Main.piratesPushed.Add(holder);

                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                }

                Main.holdersPaired.Add(holder);
                return Utils.GetPirateStatus(pirate, "Sailing towards paired holder, " + Sailing.SafeSail(pirate, holder.Location));
            }

            return "Did nothing";
        }



        override public double HeavyWeight() {


            if (Utils.GetMyHolders().Any() && game.GetEnemyLivingPirates().Any()) {

                var cloestMom = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First().GetLocation();
                var cloestHolder = Utils.GetMyHolders().OrderBy(holder => holder.Distance(pirate)).First();


                bool checkI = cloestHolder.Distance(cloestMom) <= game.HeavyPushDistance + pirate.MaxSpeed * 5;
                bool checkII = cloestHolder.Distance(cloestMom) >= pirate.PushDistance + pirate.PushRange + pirate.MaxSpeed;


                if (checkII) {
                    return -99;
                }

                if (checkI) {
                    return 100;
                }


            }

            return 0;
        }


        override public double GetWeight() {

            if (game.GetMyCapsules().Count() == 0 || !Utils.PiratesWithTask(TaskType.MINER).Any() || Utils.PiratesWithTask(TaskType.BOOSTER).Any()) {
                return 0;
            }

            if (Utils.GetMyHolders().Any() && game.GetEnemyLivingPirates().Count() > 0) {

                var cloestMom = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First().GetLocation();
                var cloestEnemyToMom = Utils.OrderByDistance(game.GetEnemyLivingPirates().ToList(), cloestMom).First();
                var cloestHolder = Utils.GetMyHolders().OrderBy(holder => holder.Distance(pirate)).First();

                if (cloestEnemyToMom.Distance(cloestMom) > cloestHolder.Distance(cloestMom)) {
                    return -50 - System.Math.Abs(Bias());
                }
            }

            double weight = 0;

            if (game.GetMyCapsules().Count() > 0) {

                var capsule = Utils.OrderByDistance(game.GetMyCapsules().ToList(), pirate.Location).First().GetLocation();

                if (Utils.GetMyHolders().Count > 0) {
                    capsule = Utils.GetMyHolders().OrderBy(hold => hold.Distance(pirate)).First().Location;
                }

                double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(capsule));
                weight += ((maxDis - pirate.Distance(capsule)) / maxDis) * 100;
            }


            if (Utils.GetMyHolders().Count() > 0 && pirate.Capsule == null) {

                var cloestHolder = Utils.OrderByDistance(Utils.GetMyHolders(), pirate.Location).First().GetLocation();
                var cloestToHolder = game.GetAllMyPirates().OrderBy(p => p.Distance(cloestHolder));
                var cloestShip = game.GetMyMotherships().OrderBy(mom => mom.Distance(cloestHolder)).First().GetLocation();

                bool caseI = true;

                if (Utils.FreeCapsulesByDistance(pirate.Location).Any()) {
                    caseI = pirate.Distance(cloestShip) < pirate.Distance(Utils.FreeCapsulesByDistance(pirate.Location).First());
                }

                if (cloestToHolder.Any() && cloestToHolder.First().Id == pirate.Id && pirate.Distance(cloestHolder) <= Chunk.size * 3 && caseI) {
                    weight += 3500;
                }
            }

            return weight;
        }


        override public int Bias() {

            // TODO  game.GetMyCapsules().Where(cap => cap.Holder == null).Count() != 0  ---> return 0
            if (!game.GetMyCapsules().Any() || Utils.GetMyHolders().Count() <= Utils.PiratesWithTask(TaskType.BOOSTER).Count()) {
                return -100000;
            }

            if (Utils.PiratesWithTask(TaskType.BOOSTER).Count() == 0) {
                return 100;
            }

            return 0;
        }


    }
}
