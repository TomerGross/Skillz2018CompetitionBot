using System.Linq;
using System.Collections.Generic;
using Pirates;

namespace Hydra {

    public class Sailing {


        public static PirateGame game = Main.game;
        

        /// <summary> Sails to goal safely </summary>
        /// <returns> A tuple with bool and the action string </returns>

        public static string SafeSail(Pirate pirate, Location to) {

            if (Main.didTurn.Contains(pirate.Id)) 
                return "Already did turn";

            var objects = new List<MapObject>();
            objects.AddRange(Utils.AsteroidsByDistance(pirate.Location).Where(ass => ass.Direction.Add(ass.Location).Distance(pirate) <= 5 * ass.Size));
            objects.AddRange(game.GetActiveWormholes().Where(hole => hole.Distance(pirate.GetLocation()) <= 4 * Chunk.size));
            objects = objects.OrderBy(obj => obj.Distance(pirate)).ToList();

            if (!objects.Any()) {
                pirate.Sail(to);
                Main.didTurn.Add(pirate.Id);
                return "Sailing safely directly to goal i.e. " + Chunk.GetChunk(to);
            }

            var interactWithAsteroid = InteractWithAsteroid(pirate, to);
            if(interactWithAsteroid.Item1)
                return interactWithAsteroid.Item2; 

            if (!Utils.PiratesWithTask(TaskType.MOLE).Contains(pirate)) {

                var killEnemy = TryKill(pirate);
                if (killEnemy.Item1) 
                    return killEnemy.Item2; 
            }

            var interactWithWormHole = InteractWithWormHole(pirate);

            if (interactWithWormHole.Item1)
                return interactWithWormHole.Item2;
  
            var traits = new List<Trait>() { new TraitRateByLazyAsteroid(1), new TraitRateByMovingAsteroid(3), new TraitWormhole(to, pirate) };
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

        protected static Tuple<bool, string> InteractWithWormHole(Pirate pirate) {

            if (!game.GetAllWormholes().Any() || !game.GetMyMotherships().Any() || !Main.mines.Any() || !game.GetEnemyMotherships().Any() || pirate.PushReloadTurns > 0)
                return new Tuple<bool, string>(false, "");


            foreach (Wormhole wormhole in game.GetAllWormholes().Where(w => w.InRange(pirate, pirate.PushRange + pirate.MaxSpeed))) {

                var cloestFriendlyMum = game.GetMyMotherships().OrderBy(wormhole.Distance).First();
                var cloestFriendlyMine = Main.mines.OrderBy(wormhole.Distance).First();
                                                                                
                bool closerToMum = wormhole.Distance(cloestFriendlyMum) < wormhole.Partner.Distance(cloestFriendlyMum);
                bool closerToMine = wormhole.Distance(cloestFriendlyMine) < wormhole.Partner.Distance(Main.mines.OrderBy(wormhole.Partner.Distance).First());

                var pushLocation = cloestFriendlyMum.Location;

                if (closerToMine)
                    pushLocation = cloestFriendlyMine;
            
                if (closerToMum || closerToMine){

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

            return new Tuple<bool, string>(false, "");
        }



        /// <summary> Tries to kill an enemy, alone or with other pirates </summary>
        /// <returns> A tuple with bool and the action string </returns>

        protected static Tuple<bool, string> TryKill(Pirate pirate){

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
            }

            return new Tuple<bool, string>(false, "Did not run");
        }


        /// <summary>  Tries to either avoid or push the asteroid </summary>
        /// <returns> A tuple with bool and the action string </returns>

        protected static Tuple<bool, string> InteractWithAsteroid(Pirate pirate, Location to) {

            var asteroids = Utils.AsteroidsByDistance(pirate.Location).Where(ass => ass.Direction.Add(ass.Location).Distance(pirate) <= 5 * ass.Size).ToList();

            foreach (Asteroid asteroid in asteroids) {
                
                if (Utils.InAsteroid(to, asteroid)) {
                    pirate.Sail(SafestCloestLocation(pirate.Location, to, pirate));
                    Main.didTurn.Add(pirate.Id);
                    return new Tuple<bool, string>(true, "Sailing around asteroid");
                }

                var nextAssLocation = asteroid.Location.Add(asteroid.Direction);
                                                            
                if (pirate.CanPush(asteroid) && nextAssLocation.Distance(pirate) < pirate.Distance(asteroid) && nextAssLocation.Distance(pirate) < pirate.MaxSpeed * 2) {
                    var pushLocation = new Location(asteroid.Location.Row - asteroid.Direction.Row, asteroid.Location.Col - asteroid.Direction.Col);
                    pirate.Push(asteroid, pushLocation);
                    Main.asteroidsPushed.Add(asteroid);
                    Main.didTurn.Add(pirate.Id);
                    return new Tuple<bool, string>(true, "Pushed asteroid");
                }

                if (pirate.Distance(to) < pirate.MaxSpeed * 5) {
                    pirate.Sail(SafestCloestLocation(pirate.Location, to, pirate));
                    Main.didTurn.Add(pirate.Id);
                    return new Tuple<bool, string>(true, "Aoiding asteroid");
                }
            }

            return new Tuple<bool, string>(false, "Did not run");
        }



        /// <summary>  
        ///     Searches for the safest adjcent chunk to the danger zone
        ///     with respect to the goal.
        /// </summary>
        /// 
        /// <returns> A tuple with bool and the action string </returns>

        public static Location SafestCloestLocation(Location danger, Location goal, Pirate pirate) => SafestCloestLocation(danger, goal, 0, false, pirate);
            

        public static Location SafestCloestLocation(Location danger, Location goal, int range, bool enemies, Pirate pirate) {

            var traits = new List<Trait>() { new TraitRateByLazyAsteroid(0), new TraitRateByMovingAsteroid(3), new TraitWormhole(goal, pirate) };

            if (enemies)
                traits.Add(new TraitRateByEnemy(1000, 1, -1));
            
            var best = Chunk.GetChunk(danger).GetNeighbors(range).OrderBy(chunk => traits.Sum(trait => trait.Cost(chunk)) + chunk.GetLocation().Distance(goal)).First();

            if (best.Distance(goal) < Chunk.size) {
                return goal;
            }

            return best.GetLocation();
        }

    }
}
