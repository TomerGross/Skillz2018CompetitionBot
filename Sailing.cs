using System.Linq;
using System.Collections.Generic;
using Pirates;

namespace Hydra {

    public class Sailing {

        public static PirateGame game = Main.game;


        /// <summary> Sails to goal safely </summary>
        /// <returns> A tuple with bool and the action string </returns>

        public static string SafeSail(Pirate pirate, Location to) {

            if (Main.didTurn.Contains(pirate.Id)) return "Already did turn";

            if (Utils.HasEnemyBomb(pirate)) {

                var bomb = game.__stickyBombs.Where(b => b.Owner == game.GetEnemy()).OrderBy(pirate.Distance).First();

                if (bomb.Countdown < 2) {
                    foreach (Pirate enemy in game.GetEnemyLivingPirates().Where(pirate.CanPush).SkipWhile(Main.piratesPushed.Contains).OrderBy(pirate.Distance)) {
                        pirate.Push(enemy, pirate);
                        Main.didTurn.Add(pirate.Id);
                        return "Hugged an enemy to death";
                    }
                }

                foreach (Pirate enemy in game.GetEnemyLivingPirates().OrderBy(pirate.Distance)) {

                    double turnsToArrive = pirate.Distance(enemy) / pirate.MaxSpeed;

                    var shouldSail = (game.GetMyLivingPirates().Count(p => p.InRange(pirate, bomb.ExplosionRange)) - game.GetEnemyLivingPirates().Count(e => e.InRange(pirate, bomb.ExplosionRange))) <= 0;

                    if (shouldSail) {
                        pirate.Sail(enemy);
                        Main.didTurn.Add(pirate.Id);
                        return "Sailing to bomb enemy";
                    }
                }

                Main.didTurn.Add(pirate.Id);

                return "Stop!";
            }

            var dangerPirates = game.GetMyLivingPirates().ToList();
            dangerPirates.AddRange(game.GetEnemyLivingPirates().ToList());
            dangerPirates = dangerPirates.Where(p => (Utils.HasEnemyBomb(p) || Utils.HasMyBomb(p)) && p.StickyBombs.First().Countdown <= 2 && pirate.CanPush(p)).ToList();

            foreach (Pirate prt in dangerPirates) {
                pirate.Push(prt, Utils.OptimalBomberPushLocation(pirate, prt));
                Main.didTurn.Add(pirate.Id);
                return "Pushed bomber away";
            }


            var interactWithAsteroid = InteractWithAsteroid(pirate, to);

            if (interactWithAsteroid.Item1) return interactWithAsteroid.Item2;


            if (!Utils.PiratesWithTask(TaskType.MOLE).Contains(pirate)) {

                var killEnemy = TryKill(pirate);
                if (killEnemy.Item1) {
                    return killEnemy.Item2;
                }
            }

            var enemys = game.GetEnemyLivingPirates().OrderBy(p => p.Distance(pirate)).ToList();

            if (enemys.Any() && pirate.InStickBombRange(enemys.First()) && game.GetMyself().TurnsToStickyBomb == 0) {
                //pirate.StickBomb(enemys.First());
                //Main.didTurn.Add(pirate.Id);
                //return "Stick bombed enemy holder";
            }

            var interactWithWormHole = InteractWithWormHole(pirate, to);

            if (interactWithWormHole.Item1) return interactWithWormHole.Item2;

            var objects = new List<MapObject>();
            objects.AddRange(Utils.AsteroidsByDistance(pirate.Location).Where(ass => ass.Direction.Add(ass.Location).Distance(pirate) <= 4 * pirate.MaxSpeed + ass.Size));
            objects.AddRange(game.GetActiveWormholes().Where(hole => hole.Distance(pirate.GetLocation()) <= 4 * Chunk.size));
            objects.AddRange(game.GetAllStickyBombs().Where(bomb => bomb.Distance(pirate) < 1.5 * bomb.ExplosionRange + pirate.MaxSpeed));
            objects = objects.OrderBy(obj => obj.Distance(pirate)).ToList();

            if (!objects.Any()) {
                pirate.Sail(to);
                Main.didTurn.Add(pirate.Id);
                return "Sailing safely directly to goal i.e. " + Chunk.GetChunk(to);
            }


            var traits = new List<Trait>() { new TraitRateByLazyAsteroid(game.HeavyPushDistance), new TraitRateByMovingAsteroid(game.HeavyPushDistance / 2 + game.PirateMaxSpeed * 3), new TraitWormhole(to, pirate), new TraitRateByStickyBomb() };
            Path path = new Path(pirate.Location, to, traits);

            if (path.GetSailLocations().Count > 1) {
                pirate.Sail(path.Pop());
                Main.didTurn.Add(pirate.Id);
                return Chunk.GetChunk(to).ToString();
            }

            pirate.Sail(to);
            Main.didTurn.Add(pirate.Id);
            return Chunk.GetChunk(to).ToString();
        }




        /// <summary> Tries to interact with a Wormhole. </summary>
        /// <returns> A tuple with a bool and the action string </returns>

        protected static Tuple<bool, string> InteractWithWormHole(Pirate pirate, Location goal) {

            if (!game.GetAllWormholes().Any() || !game.GetMyMotherships().Any() || !Main.mines.Any() || !game.GetEnemyMotherships().Any() || pirate.PushReloadTurns > 0) {
                return new Tuple<bool, string>(false, "Did not interact with a wormhole");
            }

            foreach (Wormhole wormhole in game.GetAllWormholes().Where(w => w.InRange(pirate, pirate.PushRange + pirate.MaxSpeed))) {

                if (Utils.DistanceWithWormhole(pirate.Location, goal, pirate.MaxSpeed) != pirate.Distance(goal)) {
                    return new Tuple<bool, string>(false, "Sailing into hole is faster");
                }

                var cloestFriendlyMum = game.GetMyMotherships().OrderBy(wormhole.Distance).First();
                var cloestFriendlyMine = Main.mines.OrderBy(wormhole.Distance).First();

                bool closerToMum = wormhole.Distance(cloestFriendlyMum) < wormhole.Partner.Distance(cloestFriendlyMum);
                bool closerToMine = wormhole.Distance(cloestFriendlyMine) < wormhole.Partner.Distance(Main.mines.OrderBy(wormhole.Partner.Distance).First());

                var pushLocation = cloestFriendlyMum.Location;

                if (closerToMine) {
                    pushLocation = cloestFriendlyMine;
                }

                if (closerToMum || closerToMine) {

                    if (pirate.CanPush(wormhole)) {

                        pirate.Push(wormhole, pushLocation);
                        Main.wormsPushed.Add(wormhole);

                    } else {

                        pirate.Sail(wormhole);
                    }

                    Main.didTurn.Add(pirate.Id);
                    return new Tuple<bool, string>(true, "Interacted with Wormhole");
                }
            }

            return new Tuple<bool, string>(false, "Did not interact with a wormhole");
        }




        /// <summary> Tries to kill an enemy, alone or with other pirates </summary>
        /// <returns> A tuple with bool and the action string </returns>

        protected static Tuple<bool, string> TryKill(Pirate pirate) {

            foreach (Pirate enemy in game.GetEnemyLivingPirates().OrderBy(pirate.Distance).Where(e => pirate.CanPush(e))) {

                var killLocation = Utils.NearestKillLocation(enemy.Location);

                if (((double)killLocation.Item1 + enemy.MaxSpeed / 2) / pirate.PushDistance <= 1) {

                    pirate.Push(enemy, killLocation.Item2);
                    Main.didTurn.Add(pirate.Id);
                    Main.piratesPushed.Add(enemy);
                    return new Tuple<bool, string>(true, "Killed enemy");
                }

                double killCost = killLocation.Item1 + ((double)enemy.MaxSpeed / 2);
                var helpers = game.GetMyLivingPirates().Where(h => h.CanPush(enemy) && h.Id != pirate.Id && !Main.didTurn.Contains(h.Id)).OrderBy(h => h.PushDistance);
                var killHelpers = helpers.Where(h => killCost / ((double)h.PushDistance + pirate.PushDistance) <= 1);

                if (killHelpers.Any()) {

                    var partner = killHelpers.OrderByDescending(h => killCost / ((double)h.PushDistance + pirate.PushDistance) <= 1).First();

                    pirate.Push(enemy, killLocation.Item2);
                    Main.didTurn.Add(pirate.Id);

                    partner.Push(enemy, killLocation.Item2);
                    Main.didTurn.Add(partner.Id);

                    Main.piratesPushed.Add(enemy);
                    return new Tuple<bool, string>(true, "Couple killed holder");
                }

                if (enemy.HasCapsule() && game.GetEnemyCapsules().ToList().Any() && game.GetEnemyMotherships().ToList().Any()) {

                    var nearestCapsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First();
                    var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule.Location).First();

                    // If they can make him drop his capsule but not kill him
                    if (helpers.Count() + 1 >= enemy.NumPushesForCapsuleLoss) {

                        var pushers = helpers.Take(enemy.NumPushesForCapsuleLoss - 1).ToList();
                        pushers.Add(pirate);

                        var pushLocation = Utils.NearestKillLocation(enemy.GetLocation()).Item2;

                        if (Utils.NearestKillLocation(enemy.GetLocation()).Item2.Distance(nearestCapsule) < nearestShip.Distance(nearestCapsule)) {
                            pushLocation = nearestShip.GetLocation();
                        }

                        pushers.ForEach(m => m.Push(enemy, pushLocation));

                        Main.didTurn.AddRange(from p in pushers select p.Id);
                        Main.piratesPushed.Add(enemy);
                        return new Tuple<bool, string>(true, " pirates droped the enemy capsule");

                    }
                }

            }

            return new Tuple<bool, string>(false, "Did not run");
        }


        /// <summary> Tries to either avoid or push the asteroid </summary>
        /// <returns> A tuple with bool and the action string </returns>

        protected static Tuple<bool, string> InteractWithAsteroid(Pirate pirate, Location to) {

            var asteroids = Utils.AsteroidsByDistance(pirate.Location).Where(ass => ass.Direction.Add(ass.Location).Distance(pirate) <= game.PushDistance + pirate.MaxSpeed + ass.Size).ToList();

            foreach (Asteroid asteroid in asteroids) {
                
                if (pirate.CanPush(asteroid) && !Main.asteroidsPushed.Contains(asteroid)) {

                    var pushTuple = Utils.OptimalAsteroidPushLocation(pirate.PushDistance, asteroid);

                    if (pirate.InRange(asteroid.Location.Add(asteroid.Direction), asteroid.Size + pirate.MaxSpeed)) {
                        pirate.Push(asteroid, pushTuple.Item1);
                        return new Tuple<bool, string>(true, "Pushed asteroid in an emergency");
                    }

                    if (pushTuple.Item2 > 0) {
                        pirate.Push(asteroid, pushTuple.Item1);
                        Main.asteroidsPushed.Add(asteroid);
                        Main.didTurn.Add(pirate.Id);
                        return new Tuple<bool, string>(true, "Pushed asteroid to best location");
                    }
                }

                bool shouldAvoid = pirate.Distance(asteroid) < game.PushDistance + asteroid.Size;
                shouldAvoid = shouldAvoid && game.GetEnemyLivingPirates().Any(p => p.InRange(asteroid, p.PushDistance + p.MaxSpeed));

                if (Utils.AsteroidIsMoving(asteroid) && (shouldAvoid || pirate.Location == to)) {
                    pirate.Sail(SafestCloestLocation(pirate.Location, to, pirate));
                    Main.didTurn.Add(pirate.Id);
                    return new Tuple<bool, string>(true, "Avoiding asteroid");
                }
            }

            return new Tuple<bool, string>(false, "Did not run");
        }



        /// <summary>  
        /// Searches for the safest adjcent chunk to the danger zone  with respect to the goal.
        /// </summary>
        /// <returns> A tuple with bool and the action string </returns>            

        public static Location SafestCloestLocation(Location danger, Location goal, int range, bool enemies, Pirate pirate) {

            var traits = new List<Trait>() { new TraitRateByLazyAsteroid(game.HeavyPushDistance / 2), new TraitRateByMovingAsteroid(game.HeavyPushDistance / 2 + game.PirateMaxSpeed * 3), new TraitWormhole(goal, pirate), new TraitRateByStickyBomb() };

            if (enemies) traits.Add(new TraitRateByEnemy(1000, 1, -1));

            var best = Chunk.GetChunk(danger).GetNeighbors(range).OrderBy(chunk => traits.Sum(trait => trait.Cost(chunk)) + chunk.GetLocation().Distance(goal)).First();

            if (best.Distance(goal) < Chunk.size) return goal;

            return best.GetLocation();
        }


        public static Location SafestCloestLocation(Location danger, Location goal, Pirate pirate) => SafestCloestLocation(danger, goal, 0, false, pirate);


    }
}