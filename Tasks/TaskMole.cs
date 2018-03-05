using Pirates;
using System.Linq;
using System.Collections.Generic;

namespace Hydra {

    public class TaskMole : Task {

        public static PirateGame game = Main.game;

        Pirate pirate;

        public TaskMole(Pirate pirate) {
            this.pirate = pirate;
        }


        override public void UpdatePirate(Pirate pirate) => this.pirate = pirate;


        override public string Preform() {

            if (game.GetEnemyCapsules().Count() == 0 || game.GetEnemyMotherships().Count() == 0) {
                return Utils.GetPirateStatus(pirate, "No enemy capsules or ships");
            }

            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First();
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule.Location).First();

            if (Main.didTurn.Contains(pirate.Id)) return Utils.GetPirateStatus(pirate, "Already did turn");

            if (Utils.HasEnemyBomb(pirate)) return Utils.GetPirateStatus(pirate, Sailing.SafeSail(pirate, new Location(0, 0)));

            var sortedEnemyHolders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Where(enemy => !Main.piratesPushed.Contains(enemy) && pirate.CanPush(enemy));

            bool shouldGo = !sortedEnemyHolders.Any() || pirate.MaxSpeed * 4 + pirate.Distance(nearestShip) < sortedEnemyHolders.First().Distance(nearestShip);

            if (shouldGo) {
                foreach (Wormhole hole in game.GetAllWormholes().Where(h => h.Distance(nearestShip) < pirate.MaxSpeed * 10 && !Main.wormsPushed.Contains(h))) {

                    var molesByDistance = Utils.PiratesWithTask(TaskType.MOLE).OrderBy(hole.Distance);
                    bool closest = molesByDistance.First().Id == pirate.Id || (molesByDistance.Count() > 1 && molesByDistance.Take(2).Contains(pirate));
                    var eholdersbydistance = Utils.EnemyHoldersByDistance(nearestShip.GetLocation());

                    if (!pirate.CanPush(hole) && pirate.PushRange < pirate.Distance(hole) && closest) {

                        var wormLoc = pirate.Location.Towards(hole, pirate.Distance(hole) - hole.WormholeRange);
                        var assDanger = game.__livingAsteroids.Any(a => a.Location.Add(a.Direction).Distance(pirate) <= a.Size + pirate.MaxSpeed * 2);
                        var bombDanger = game.__stickyBombs.Any(b => b.Distance(pirate) < b.ExplosionRange + pirate.MaxSpeed * 2);

                        var wormPushLocation = pirate.Location.Towards(hole, pirate.Distance(hole) - pirate.PushRange);
                        var caseI = pirate.Distance(wormPushLocation) / pirate.MaxSpeed >= pirate.PushReloadTurns;
                        var caseII = true;

                        if (eholdersbydistance.Any()) caseII = hole.Distance(nearestShip) + pirate.MaxSpeed * 4 < eholdersbydistance.First().Distance(nearestShip);
                        
                        if (!assDanger && !bombDanger && caseI && caseII) {
                            
                            pirate.Sail(wormLoc);
                            Main.didTurn.Add(pirate.Id);
                            return Utils.GetPirateStatus(pirate, "Sailing out to worm hole ");
                        }

                        if (caseI && caseII) return Utils.GetPirateStatus(pirate, "Safely sailing out to worm hole " + Sailing.SafeSail(pirate, wormLoc));
                    }

                    var enemyHolders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).SkipWhile(Main.piratesPushed.Contains).OrderBy(hole.Distance);

                    if (pirate.CanPush(hole) && hole.IsActive && enemyHolders.Any() && hole.Distance(nearestShip) < hole.Partner.Distance(nearestShip)) {
                        foreach (Pirate enemyHolder in enemyHolders) {

                            int cost = enemyHolder.Distance(hole) + enemyHolder.MaxSpeed / 2;

                            if (cost < pirate.PushDistance) {
                                pirate.Push(hole, pirate.Location.Towards(enemyHolder.Location, cost));
                                Main.didTurn.Add(pirate.Id);
                                Main.piratesPushed.Add(enemyHolder);
                                Main.wormsPushed.Add(hole);
                                return Utils.GetPirateStatus(pirate, "Pushed hole on enemy");
                            }
                        }
                    }

                    if (pirate.CanPush(hole) && Main.mines.Any()) {
                        pirate.Push(hole, Main.mines.OrderBy(nearestShip.Distance).First());
                        Main.didTurn.Add(pirate.Id);
                        Main.wormsPushed.Add(hole);
                        return Utils.GetPirateStatus(pirate, "Pushed hole away");
                    }
                }
            }

            foreach (Pirate enemyHolder in sortedEnemyHolders) {

                game.Debug("pirate can push holder:  " + pirate.CanPush(enemyHolder));
                var killLocation = Utils.NearestKillLocation(enemyHolder.Location);
                double maxDistance = ((double)killLocation.Item1 + enemyHolder.MaxSpeed / 2);
                var canKillAlone = maxDistance / pirate.PushDistance <= 1;

                if (canKillAlone) {
                    pirate.Push(enemyHolder, killLocation.Item2);
                    Main.didTurn.Add(pirate.Id);
                    Main.piratesPushed.Add(enemyHolder);
                    return Utils.GetPirateStatus(pirate, "Killed enemy holder");
                }

                // Initialize variables
                var pushHelpers = game.GetMyLivingPirates().Where(h => h.CanPush(enemyHolder) && !Main.didTurn.Contains(h.Id)).OrderBy(h => h.PushDistance);
                var killHelpers = pushHelpers.Where(h => h.Id != pirate.Id && ((double)killLocation.Item1 + enemyHolder.MaxSpeed / 2) / ((double)h.PushDistance + pirate.PushDistance) <= 1);

                // If they can kill him
                if (killHelpers.Any()) {
                    var partner = killHelpers.OrderByDescending(h => maxDistance / ((double)h.PushDistance + pirate.PushDistance) <= 1).First();
                    pirate.Push(enemyHolder, killLocation.Item2);
                    partner.Push(enemyHolder, killLocation.Item2);
                    Main.didTurn.AddRange(new List<int> { pirate.Id, partner.Id });
                    Main.piratesPushed.Add(enemyHolder);
                    return Utils.GetPirateStatus(pirate, "Couple killed enemy holder");
                }

                // If they can make him drop his capsule but not kill him
                if (pushHelpers.Count() >= enemyHolder.NumPushesForCapsuleLoss) {

                    var pushers = pushHelpers.Take(enemyHolder.NumPushesForCapsuleLoss).ToList();

                    var pushLocation = Utils.NearestKillLocation(enemyHolder.GetLocation()).Item2;

                    if (Utils.NearestKillLocation(enemyHolder.GetLocation()).Item2.Distance(nearestCapsule) < nearestShip.Distance(nearestCapsule)) {
                        pushLocation = nearestShip.GetLocation();
                    }

                    pushers.ForEach(m => m.Push(enemyHolder, pushLocation));
                    Main.didTurn.AddRange(from p in pushers select p.Id);

                    Main.piratesPushed.Add(enemyHolder);
                    return Utils.GetPirateStatus(pirate, enemyHolder.NumPushesForCapsuleLoss + " pirates droped the enemy capsule");
                }

                // Boost enemy to closest dropers couple
                var myMoles = Utils.PiratesWithTask(TaskType.MOLE).ToList().Where(p => p.Id != pirate.Id && p.PushReloadTurns <= 1).OrderBy(p => p.Distance(nearestShip)).ToList();
                var regularEnemyPirates = game.GetEnemyLivingPirates().Where(prt => !prt.HasCapsule()).ToList();
                bool shouldUseBuddies = myMoles.Any() && pirate.PushRange + pirate.MaxSpeed / 2 < myMoles.OrderBy(pirate.Distance).First().Distance(pirate);
                bool enemyIsTerr = Utils.HasMyBomb(enemyHolder);

                if (regularEnemyPirates.Any() && myMoles.Count() >= 2 && shouldUseBuddies && !enemyIsTerr) {
                    foreach (Pirate A in myMoles) {
                        foreach (Pirate B in myMoles.Where(m => m.Id != A.Id)) {

                            if (A.Distance(pirate) < A.PushRange * 1.5) continue;

                            var centerLoc = Utils.Center(A.Location, B.Location);
                            var pushLocation = pirate.GetLocation().Towards(centerLoc, pirate.PushDistance - enemyHolder.MaxSpeed / 2);

                            bool checkI = pushLocation.Distance(A) <= A.PushRange && pushLocation.Distance(B) <= B.PushRange;
                            bool checkII = enemyHolder.StateName == "normal";

                            // TODO add check if there is a booster close to the enemy pirate
                            if (checkI && checkII) {
                                pirate.Push(enemyHolder, centerLoc);
                                Main.didTurn.Add(pirate.Id);
                                Main.piratesPushed.Add(enemyHolder);
                                return Utils.GetPirateStatus(pirate, "Pushed pirates towards buddies!");
                            }
                        }
                    }
                }
            }

            int radius = (game.PushRange + game.HeavyPushDistance) / 3;
            int coupleIndex = Utils.PiratesWithTask(TaskType.MOLE).OrderBy(nearestShip.Distance).ToList().IndexOf(pirate) / 2;

            if (coupleIndex > 0) {
                radius += game.HeavyPushDistance;
            }

            var loc = nearestShip.GetLocation().Towards(nearestCapsule, radius);


            foreach (Pirate enemyHolder in sortedEnemyHolders) {

                var CheckI = enemyHolder.Distance(nearestShip) < 2 * pirate.PushRange + pirate.Distance(nearestShip);
                var CheckII = pirate.PushReloadTurns <= (enemyHolder.Distance(nearestShip) - pirate.Distance(nearestShip)) / (2 * enemyHolder.MaxSpeed);
                var CheckIII = pirate.Distance(loc) < 2 * pirate.MaxSpeed;
                var CheckIV = game.GetMyLivingPirates().Count(p => p.Id != pirate.Id && p.GetLocation().Col == pirate.Location.Col && p.GetLocation().Row == pirate.Location.Row) >= 1;
                //var CheckV = Utils.PiratesWithTask(TaskType.MOLE).OrderBy(enemyHolder.Distance).First().Id == pirate.Id;

                //game.Debug(CheckI + "   ||  " + CheckII + "   ||  " + CheckIII + "   ||  " + CheckIV + "   ||  "/*+ CheckV + "   ||  "*/);
                if (CheckI && CheckII && CheckIII && CheckIV/* && CheckV*/) {
                    return Utils.GetPirateStatus(pirate, "Sailing out to enemy holder " + Sailing.SafeSail(pirate, enemyHolder.GetLocation()));
                }
            }

            return Utils.GetPirateStatus(pirate, "Is sailing to position, " + Sailing.SafeSail(pirate, loc));

        }


        override public double HeavyWeight() {

            if (game.GetEnemyCapsules().Any() && game.GetEnemyMotherships().Any()) {

                var nearestCapsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First();
                var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule.Location).First();

                if (pirate.Distance(nearestShip) > 4 * game.PushDistance) return -2;

                var maxDis = game.HeavyPushDistance;
                var cloestkillLoc = Utils.NearestKillLocation(pirate.GetLocation());
                var numOfHelpers = Utils.GetNumOfMyPiratesOnPoint(pirate.GetLocation()) - 1;

                if (numOfHelpers > 0) maxDis += numOfHelpers * game.PushDistance;

                if (pirate.Distance(cloestkillLoc.Item2) <= maxDis) return 2;
            }

            return 0;
        }


        override public double GetWeight() {


            var number = 0;
            var numofholders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Count();

            if (numofholders == 1) {
                number = 3;
            } else {
                number = 2 * numofholders;
            }

            if (game.GetEnemyMotherships().Count() == 0 || Utils.PiratesWithTask(TaskType.MOLE).Count() >= number) return 0;

            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), pirate.Location).First().Location;
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule).First();

            if (Utils.EnemyHoldersByDistance(pirate.Location).Any()) {

                if (Utils.EnemyHoldersByDistance(pirate.Location).First().Distance(pirate) <= pirate.MaxSpeed * 4 && !pirate.HasCapsule()) {
                    return 1000000;
                }

                nearestCapsule = Utils.EnemyHoldersByDistance(pirate.Location).First().Location;
            }

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(nearestShip));
            double weight = ((double)(maxDis - pirate.Distance(nearestShip)) / maxDis) * 100;

            if (double.IsNaN(weight)) return 0;

            return weight;
        }


        override public int Bias() {

            var bonus = 0;

            if (Utils.EnemyHoldersByDistance(pirate.GetLocation()).Any() && Utils.EnemyHoldersByDistance(pirate.GetLocation()).First().InRange(pirate, pirate.PushRange)) bonus = 30;

            if (Utils.PiratesWithTask(TaskType.MOLE).Count >= 2 * game.GetMyCapsules().Count() - Utils.FreeCapsulesByDistance(pirate.Location).Count()) return 1;

            return 70 + bonus;
        }

    }
}

