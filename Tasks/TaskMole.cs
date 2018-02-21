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

        ////////////////////////////////////////////////////////
        ////////////////TODO: FIX THE POSITIONS!!!!/////////////
        ////////////////////////////////////////////////////////
        //TAKE NumPushesForCapsuleLoss MOLES TO THE CLOSEST ENEMY AND KILL HIM
        //CHECK CASES OF DELAYING

        override public string Preform() {

            if (game.GetEnemyCapsules().Count() == 0 || game.GetEnemyMotherships().Count() == 0) {
                return Utils.GetPirateStatus(pirate, "No enemy capsules or ships");
            }

            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First();
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule.Location).First();

            if (Main.didTurn.Contains(pirate.Id)) {
                return Utils.GetPirateStatus(pirate, "Already did turn");
            }



            var sortedEnemyHolders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Where(enemy => !Main.piratesPushed.Contains(enemy) && pirate.CanPush(enemy));
            var enemyHldrs = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Where(eprt => !Main.piratesPushed.Contains(eprt));

            foreach (Wormhole hole in game.GetAllWormholes().Where(h => h.Distance(nearestShip) < pirate.MaxSpeed * 6 && !Main.wormsPushed.Contains(h))) {

                if (enemyHldrs.Any()) {
                    var closestholder = enemyHldrs.First();

                    if (hole.Distance(nearestShip) < closestholder.Distance(nearestShip) - 5 * closestholder.MaxSpeed) {

                        if (pirate.CanPush(hole)) {
                            pirate.Push(hole, Utils.OppositeLocation(nearestShip.GetLocation()));
                            Main.wormsPushed.Add(hole);
                            return "pirate pushed hole away";
                        }

                        return "sailing to hole,    " + Sailing.SafeSail(pirate, hole.GetLocation());
                    }

                } else {

                    if (pirate.CanPush(hole)) {
                        pirate.Push(hole, Utils.OppositeLocation(nearestShip.GetLocation()));
                        Main.wormsPushed.Add(hole);
                        return "pirate pushed hole away";
                    }

                    return "sailing to hole, " + Sailing.SafeSail(pirate, hole.GetLocation().Towards(nearestShip.GetLocation(), pirate.PushRange));

                }
            }

            if (!sortedEnemyHolders.Any()) {

                int radius = (game.PushRange + game.PushDistance) / 2;

                if (Utils.PiratesWithTask(TaskType.MOLE).IndexOf(pirate) > 1) {
                    radius += game.PushDistance;
                }

                var loc = nearestShip.GetLocation().Towards(nearestCapsule, radius);

                //sailing out enemy holder
                var enemyHolders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Where(enemy => !Main.piratesPushed.Contains(enemy));
                foreach (Pirate enemyHolder in enemyHolders) {
                    var caseI = enemyHolder.Distance(nearestShip) < 1.5 * pirate.PushRange + pirate.Distance(nearestShip);
                    var caseII = pirate.PushReloadTurns <= (enemyHolder.Distance(nearestShip) - pirate.Distance(nearestShip)) / (2 * enemyHolder.MaxSpeed);
                    var caseIII = pirate.Distance(loc) < 4 * game.PushDistance;

                    if (caseI && caseII && caseIII) {
                        //Sailing.SafeSail(pirate, enemyHolder.GetLocation());
                        // return Utils.GetPirateStatus(pirate, "Sailing out enemy holder");
                    }

                }

                return Utils.GetPirateStatus(pirate, "Is sailing to position, " + Sailing.SafeSail(pirate, loc));
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

                    pushers.ForEach(m => m.Push(enemyHolder, pushLocation));
                    Main.didTurn.AddRange(from p in pushers select p.Id);

                    Main.piratesPushed.Add(enemyHolder);
                    return Utils.GetPirateStatus(pirate, enemyHolder.NumPushesForCapsuleLoss + " pirates droped the enemy capsule");
                }

                // Boost enemy to closest dropers couple
                var myMoles = Utils.PiratesWithTask(TaskType.MOLE).ToList().Where(p => p.Id != pirate.Id && p.PushReloadTurns <= 1).OrderBy(p => p.Distance(nearestShip)).ToList();
                var regularEnemyPirates = game.GetEnemyLivingPirates().Where(prt => !prt.HasCapsule()).ToList();
                bool shouldUseBuddies = myMoles.Any() && pirate.PushRange + pirate.MaxSpeed / 2 < myMoles.OrderBy(pirate.Distance).First().Distance(pirate);

                if (regularEnemyPirates.Any() && myMoles.Count() >= 2 && shouldUseBuddies) {
                    foreach (Pirate A in myMoles) {
                        foreach (Pirate B in myMoles.Where(m => m.Id != A.Id)) {

                            if (A.Distance(pirate) < A.PushRange * 1.5) {
                                continue;
                            }

                            var centerLoc = Utils.Center(A.Location, B.Location);
                            var pushLocation = centerLoc.Towards(pirate, enemyHolder.MaxSpeed / 2);

                            if (Utils.Center(A.Location, B.Location).Distance(enemyHolder) <= pirate.PushRange) {
                                pushLocation = centerLoc;
                            }

                            bool checkI = pushLocation.Distance(A) <= A.PushDistance && pushLocation.Distance(B) <= B.PushDistance;
                            bool checkII = enemyHolder.StateName == "normal";

                            // TODO add check if there is a booster close to the enemy pirate
                            if (checkI && checkII) {
                                pirate.Push(enemyHolder, pushLocation);
                                Main.piratesPushed.Add(enemyHolder);
                                return Utils.GetPirateStatus(pirate, "Pushed pirates towards buddies!");
                            }
                        }
                    }

                }


                /*
                //delay enemy holder, wait for couple pushing
                var molesCantPush = Utils.PiratesWithTask(TaskType.MOLE).Where(p => p.Id != pirate.Id && p.PushReloadTurns > 1).OrderBy(p => p.Distance(nearestShip)).ToList();
                if (molesCantPush.Count() > 1){
                    var turnsToArrive = enemyHolder.GetLocation().Towards(nearestShip, -pirate.PushDistance + enemyHolder.MaxSpeed / 2).Distance(nearestShip) / enemyHolder.MaxSpeed;
                    var checkI = turnsToArrive > molesCantPush[0].PushReloadTurns && turnsToArrive > molesCantPush[1].PushReloadTurns;
                    
                    if(checkI){
                        var pushLoc = enemyHolder.GetLocation().Towards(nearestShip, -5*game.HeavyPushDistance);
                        pirate.Push(enemyHolder, pushLoc);
                        Main.piratesPushed.Add(enemyHolder);
                        return Utils.GetPirateStatus(pirate, "Pirate delay enemy holder");
                    }
                }
                */

            }

            return Utils.GetPirateStatus(pirate, "Is idle");
        }


        override public double HeavyWeight() {

            if (game.GetEnemyCapsules().Count() == 0 || game.GetEnemyMotherships().Count() == 0) {
                return 0;
            }

            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First();
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule.Location).First();


            if (game.GetMyMotherships().Any() && pirate.Distance(nearestShip) > 4 * game.PushDistance) {
                return -101;
            }

            var cloestEdge = Utils.CloestEdge(pirate.GetLocation());

            if (pirate.Distance(cloestEdge.Item2) <= game.HeavyPushDistance) {
                return 60;
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

            if (game.GetEnemyMotherships().Count() == 0 || Utils.PiratesWithTask(TaskType.MOLE).Count() >= number) {
                return 0;
            }


            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), pirate.Location).First().Location;
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule).First();

            if (Utils.EnemyHoldersByDistance(pirate.Location).Count() > 0) {
                nearestCapsule = Utils.EnemyHoldersByDistance(pirate.Location).First().Location;
            }

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(nearestShip));
            double weight = ((double)(maxDis - pirate.Distance(nearestShip)) / maxDis) * 100;

            if (double.IsNaN(weight)) {
                return 0;
            }

            return weight;
        }


        override public int Bias() {

            var bonus = 0;
            if (Utils.EnemyHoldersByDistance(pirate.GetLocation()).Count() > 0 && Utils.EnemyHoldersByDistance(pirate.GetLocation()).First().InRange(pirate, pirate.PushRange)) {
                bonus = 30;
            }
            if (Utils.PiratesWithTask(TaskType.MOLE).Count >= 2 * game.GetMyCapsules().Count() - Utils.FreeCapsulesByDistance(pirate.Location).Count()) {
                return 1;
            }

            return 70 + bonus;
        }

    }
}

