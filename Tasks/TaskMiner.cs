using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class TaskMiner : Task {

        public static PirateGame game = Main.game;

        Pirate pirate;
        int turnCounter;
        double maxTurnsToArrive = 100;


        public TaskMiner(Pirate pirate) {

            this.pirate = pirate;
        }


        override public void UpdatePirate(Pirate pirate) => this.pirate = pirate;


        override public int Priority() => 1;


        override public string Preform() {

            if (Main.didTurn.Contains(pirate.Id) || !game.GetMyMotherships().Any()) return Utils.GetPirateStatus(pirate, "Already did turn");

            if (!pirate.HasCapsule()) {

                turnCounter = 0;

                if (Main.capsulesTargetted.ContainsKey(pirate)) {
                    var sailLocation = Main.capsulesTargetted[pirate].Location;
                    return Utils.GetPirateStatus(pirate, "Sailing to capsule " + Sailing.SafeSail(pirate, sailLocation));
                }

                return Utils.GetPirateStatus(pirate, "Is idle... ");
            }

            var nearestShip = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First();
            var threats = game.GetEnemyLivingPirates().ToList().Where(e => e.PushReloadTurns < 3 && e.Distance(nearestShip) < pirate.Distance(nearestShip)).OrderBy(nearestShip.Distance).ToList();

            if (Utils.HasEnemyBomb(pirate)) {
                var suicideObj = Utils.NearestKillLocation(pirate.GetLocation()).Item2;
                var bomb = game.__stickyBombs.OrderBy(pirate.Distance).First();
                bool canReachMum = pirate.Distance(nearestShip) / pirate.MaxSpeed + pirate.MaxSpeed < bomb.Countdown;

                if (!canReachMum) return Utils.GetPirateStatus(pirate, Sailing.SafeSail(pirate, new Location(0, 0)));
            }

            turnCounter++;

            if (turnCounter == 0) maxTurnsToArrive = ((double)pirate.Distance(nearestShip) / pirate.MaxSpeed) * 2.5;

            if (Utils.GetMyHolders().Count() > 1) {

                var secondHolder = Utils.GetMyHolders().Where(h => h.Id != pirate.Id).First();
                var nearestShipToSecondHolder = Utils.OrderByDistance(game.GetMyMotherships().ToList(), secondHolder.Location).First();

                var secondThreats = game.GetEnemyLivingPirates().ToList().Where(e => e.CanPush(secondHolder) && e.Distance(nearestShip) < secondHolder.Distance(nearestShip));
                secondThreats = secondThreats.OrderBy(treath => treath.Distance(secondHolder)).ToList();

                bool caseVI = secondHolder.Distance(nearestShip) < pirate.Distance(nearestShip);
                bool caseVII = secondHolder.Distance(nearestShip) < pirate.PushDistance + secondHolder.MaxSpeed + game.MothershipUnloadRange;

                if (pirate.CanPush(secondHolder) && caseVI && caseVII && !Main.piratesPushed.Contains(secondHolder)) {
                    pirate.Push(secondHolder, nearestShipToSecondHolder);
                    Main.didTurn.Add(pirate.Id);
                    Main.piratesPushed.Add(secondHolder);
                    return Utils.GetPirateStatus(pirate, "Boosted second holder");
                }

            }

            var traits = new List<Trait>() {
                        new TraitRateByLazyAsteroid(game.PushDistance + game.AsteroidSize),
                        new TraitRateByMovingAsteroid(game.HeavyPushDistance / 2 + game.PirateMaxSpeed * 3),
                        new TraitRateByEdges(750, 4),
                        new TraitRateByStickyBomb(),
                        new TraitRateByEnemy(150, 3, -1),
                        new TraitAttractedToGoal(500, nearestShip.Location),
                        new TraitWormhole(nearestShip.Location, pirate)
            };


            if (threats.Any() && maxTurnsToArrive < turnCounter && Utils.GetNumOfEnemyPiratesOnPoint(threats.First().Location) >= 4) {
                pirate.Sail(nearestShip);
                Main.didTurn.Add(pirate.Id);
                return Utils.GetPirateStatus(pirate, "Kamikazad on ship");
            }

            int abstainRadius = 8 * pirate.MaxSpeed;

            if (Utils.GetMyHolders().Count() > 1) {

                var secondHolder = Utils.GetMyHolders().First(h => h.Id != pirate.Id);
                abstainRadius = 3 * (pirate.MaxSpeed + secondHolder.MaxSpeed);

                if (abstainRadius / 2 < pirate.Distance(nearestShip)) {
                    traits.Add(new TraitRateByEnemy(abstainRadius, 100, secondHolder.UniqueId));
                }
            }

            var path = new Path(pirate.GetLocation(), nearestShip.Location, traits);

            bool nearWormHole = game.__wormholes.Any() && game.__wormholes.Any(w => w.TurnsToReactivate <= 2 && pirate.Distance(w) < w.WormholeRange + pirate.MaxSpeed);
            bool safe = (threats.Any() && game.PushDistance + game.AsteroidSize <= threats.OrderBy(pirate.Distance).First().Distance(pirate)) && !game.GetLivingAsteroids().Any(a => a.Distance(pirate) <= a.Size + pirate.MaxSpeed * 2);

            if (!nearWormHole && safe && (!threats.Any() && pirate.Distance(nearestShip) < abstainRadius || pirate.Distance(nearestShip) <= Chunk.size || path.GetSailLocations().Count <= 1)) {
                pirate.Sail(nearestShip);
                Main.didTurn.Add(pirate.Id);
                return Utils.GetPirateStatus(pirate, "Sailing directly to ship");
            }

            Location pathPopLocation = path.Pop();
            Location nextSailLocation = pathPopLocation;

            if (Utils.GetMyHolders().Count() > 1 && safe && threats.Count() >= pirate.NumPushesForCapsuleLoss) {

                var otherMiner = Utils.GetMyHolders().First(h => h.Id != pirate.Id);

                bool sameTargetShip = Utils.OrderByDistance(game.GetMyMotherships().ToList(), otherMiner.Location).First() == nearestShip;
                bool checkI = pirate.Distance(nearestShip) < otherMiner.Distance(nearestShip);
                bool checkII = Chunk.size * 4 < otherMiner.Distance(nearestShip) - pirate.Distance(nearestShip);

                if (sameTargetShip && checkI && checkII) {
                    nextSailLocation = pirate.Location.Towards(pathPopLocation, pirate.MaxSpeed / 2);
                }
            }

            if (Utils.PiratesWithTask(TaskType.BOOSTER).Any() && safe) {

                var closestBooster = Utils.OrderByDistance(Utils.PiratesWithTask(TaskType.BOOSTER), pirate.Location).First();
                bool cloestToBooster = Utils.OrderByDistance(Utils.GetMyHolders(), closestBooster.Location).First().Id == pirate.Id;
                bool checkI = threats.Any() && threats.First().MaxSpeed * 2 <= threats.First().Distance(pirate);

                if (cloestToBooster && game.PushRange < pirate.Distance(closestBooster) && checkI) {
                    nextSailLocation = pirate.Location.Towards(pathPopLocation, closestBooster.MaxSpeed / 2);
                }
            }

            return Utils.GetPirateStatus(pirate, "Sailing to goal, " + Sailing.SafeSail(pirate, nextSailLocation.GetLocation()));
        }


        override public double HeavyWeight() {

            if (!game.GetMyMotherships().Any()) return 0;
            
            if (Utils.HasMyBomb(pirate) || Utils.HasEnemyBomb(pirate)) return -6;

            var enemysInRange = game.GetEnemyLivingPirates().ToList().Where(e => e.InRange(pirate, e.PushRange + 2 * e.MaxSpeed));

            if (enemysInRange.Count() >= pirate.NumPushesForCapsuleLoss && pirate.HasCapsule()) return 4;

            return -4;
        }


        override public double GetWeight() {

            if (pirate.HasCapsule()) return 10000;

            if (game.GetMyCapsules().Count() == 0 || Utils.FreeCapsulesByDistance(pirate.Location).Count() == 0 || Utils.PiratesWithTask(TaskType.MINER).Count >= game.GetMyCapsules().Count() || Utils.FreeCapsulesByDistance(pirate.Location).Count() == 0) {
                return -Bias() - 50;
            }

            var cloestCapsule = Utils.FreeCapsulesByDistance(pirate.Location).First();

            double maxDis = Main.unemployedPirates.Max(unemployed => unemployed.Distance(Utils.FreeCapsulesByDistance(unemployed.GetLocation()).Last()));
            double weight = ((double) (maxDis - pirate.Distance(cloestCapsule) / maxDis)) * 100;

            if (pirate.PushReloadTurns > 2) weight += 20;

            return weight;
        }


        override public int Bias() {

            if (!game.GetMyCapsules().Any() || !Utils.FreeCapsulesByDistance(pirate.Location).Any()) return 0;

            if (Utils.PiratesWithTask(TaskType.MINER).Count == 0) return 1000;

            return 100;
        }


    }
}
