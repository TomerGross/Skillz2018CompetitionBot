using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class Utils {

        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------


        public static bool PushAsteroid(Pirate pirate) {

            foreach (Asteroid asteroid in game.GetAllAsteroids()) {

                var nextLocation = asteroid.Location.Add(asteroid.Direction);

                if (pirate.CanPush(asteroid) && !Main.asteroidsPushed.Contains(asteroid)) {
                    if (nextLocation.Distance(pirate) < asteroid.Size) {

                        pirate.Push(asteroid, new Location(asteroid.Location.Row - asteroid.Direction.Row, asteroid.Location.Col - asteroid.Direction.Col));
                        Main.asteroidsPushed.Add(asteroid);
                        return true;
                    }
                }
            }

            return false;
        }


        public static int GetNumOfEnemyPiratesOnPoint(Location loc) {
            var enemylivingpirates = game.GetEnemyLivingPirates();
            int count = 0;
            foreach (Pirate enemy in enemylivingpirates) {
                if (enemy.GetLocation().Row == loc.Row && enemy.GetLocation().Col == loc.Col) {
                    count += 1;
                }

            }

            return count;
        }



        public static bool SafeSail(Pirate pirate, Location to) {

            if (Main.didTurn.Contains(pirate.Id)) {
                return false;
            }

            var threaths = AsteroidsByDistance(pirate.Location);
            threaths.RemoveAll(asteroid => asteroid.Direction.Add(asteroid.Location).Distance(pirate) > 5 * asteroid.Size);

            if (threaths.Count() == 0) {
                pirate.Sail(to);
                Main.didTurn.Add(pirate.Id);
                return true;
            }

            if (InAsteroid(to, threaths.First())) {
                pirate.Sail(SafestCloest(pirate.Location, to));
                Main.didTurn.Add(pirate.Id);
                return true;
            }

            if (pirate.Distance(to) < pirate.MaxSpeed * 2.5) {

                if (pirate.CanPush(threaths.First()) && !Main.asteroidsPushed.Contains(threaths.First()) && pirate.Distance(threaths.First()) < threaths.First().Size * 1.5) {

                    var pushLocation = new Location(threaths.First().Location.Row - threaths.First().Direction.Row, threaths.First().Location.Col - threaths.First().Direction.Col);
                    if (threaths.Count() > 1 && threaths[1].Distance(threaths.First()) < 1000) {
                        pushLocation = threaths[1].Location;
                    }

                    pirate.Push(threaths.First(), pushLocation);
                    Main.asteroidsPushed.Add(threaths.First());
                    Main.didTurn.Add(pirate.Id);
                    return true;
                }

                pirate.Sail(SafestCloest(pirate.Location, to));
                Main.didTurn.Add(pirate.Id);
                return true;
            }

            var traits = new List<Trait>() { new TraitRateByLazyAsteroid(1), new TraitRateByMovingAsteroid(5) };
            Path path = new Path(Chunk.GetChunk(pirate.Location), Chunk.GetChunk(to), traits, Path.Algorithm.ASTAR);

            if (path.GetChunks().Count > 0) {

                Chunk nextChunk = path.Pop();
                pirate.Sail(nextChunk.GetLocation());
                Main.didTurn.Add(pirate.Id);
                return true;
            }

            pirate.Sail(to);
            Main.didTurn.Add(pirate.Id);
            return true;
        }


        public static bool AsteroidMove(Asteroid ast) {
            return ast.Location.Add(ast.Direction).Row == ast.Location.Row && ast.Location.Add(ast.Direction).Col == ast.Location.Col;
        }


        public static Location SafestCloest(Location danger, Location goal) {

            var traits = new List<Trait>() { new TraitRateByLazyAsteroid(1), new TraitRateByMovingAsteroid(5) };
            var best = Chunk.GetChunk(danger).GetNeighbors(0).OrderBy(chunk => traits.Sum(trait => trait.Cost(chunk)) + chunk.GetLocation().Distance(goal)).First();

            if (best.Distance(goal) < Chunk.size) {
                return goal;
            }

            return best.GetLocation();
        }


        public static bool InAsteroid(Location to, Asteroid ass) {

            if (to.Distance(ass.Location.Add(ass.Direction)) < ass.Size) {
                return true;
            }

            return false;
        }


        public static List<Pirate> OrderByDistance(List<Pirate> list, Location l) => list.OrderBy(p => p.Distance(l)).ToList();
        public static List<Capsule> OrderByDistance(List<Capsule> list, Location l) => list.OrderBy(a => a.Distance(l)).ToList();
        public static List<Location> OrderByDistance(List<Location> list, Location l) => list.OrderBy(obj => obj.Distance(l)).ToList();
        public static List<Location> OrderByDistance(List<MapObject> list, Location l) => OrderByDistance((from obj in list select obj.GetLocation()).ToList(), l);
        public static List<Location> OrderByDistance(List<Mothership> list, Location l) => OrderByDistance((from ship in list select ship.Location).ToList(), l);
        public static List<Capsule> FreeCapsulesByDistance(Location l) => game.GetMyCapsules().ToList().Where(cap => cap.Holder == null && !Main.capsulesTargetted.Values.Contains(cap)).OrderBy(cap => cap.Distance(l)).ToList();


        public static List<Pirate> GetMyHolders() => (from cap in game.GetMyCapsules().ToList().Where(cap => cap.Holder != null) select cap.Holder).ToList();
        public static List<Pirate> EnemyHoldersByDistance(Location l) => (from cap in game.GetEnemyCapsules().ToList().Where(cap => cap.Holder != null).OrderBy(cap => cap.Distance(l)).ToList() select cap.Holder).ToList();
        public static List<Pirate> PiratesWithTask(TaskType t) => (from tuple in Main.tasks.Where(pair => pair.Value == t) select game.GetMyPirateById(tuple.Key)).ToList();


        public static List<Asteroid> AsteroidsByDistance(Location l) => game.GetAllAsteroids().OrderBy(asteroid => asteroid.Distance(l)).ToList();


        public static int ClosestEdgeDistance(Location l) => new List<int> { l.Col, l.Row, game.Cols - l.Col, game.Rows - l.Row }.OrderBy(dis => dis).First();


        public static Location OppositeLocation(Location loc) {
            return new Location(game.Rows - loc.Row, game.Cols - loc.Col);
        }


        // Returns the distance from the cloest edge and the push location
        public static Tuple<int, Location> CloestEdge(Location loc) {

            return new List<Tuple<int, Location>> {
                new Tuple<int, Location>(loc.Col, new Location(loc.Row, -1)),
                new Tuple<int, Location>(game.Cols - loc.Col, new Location(loc.Row, game.Cols + 1)),
                new Tuple<int, Location>(loc.Row, new Location(-1, loc.Col)),
                new Tuple<int, Location>(game.Rows - loc.Row, new Location(game.Rows + 1, loc.Col))
            }.OrderBy(tuple => tuple.Item1).First();
        }


        // ClosestPair returns a list of the closest pair of enemy and friendly bot
        public static Tuple<MapObject, MapObject> ClosestPair(MapObject[] group1, MapObject[] group2) {

            int min = group1.Min(obj => group2.Min(obj.Distance));

            var obj1 = group1.First(obj => min == group2.Min(obj.Distance));
            var obj2 = group1.First(obj => min == group1.Min(obj.Distance));

            return new Tuple<MapObject, MapObject>(obj1, obj2);
        }


        //The function return a list of sorted map object from group1 by distance to object2    
        public static List<Tuple<MapObject, int>> SoloClosestPair(MapObject[] objects, MapObject to) {
            return (from obj in objects select new Tuple<MapObject, int>(obj, obj.Distance(to))).OrderBy(tuple => tuple.Item2).ToList();
        }


        //The function return a list of sorted map object from group1 by distance to object2    
        public static List<Tuple<MapObject, int>> ClosestThreaths(MapObject to, List<Pirate> enemies) {
            var pairs = SoloClosestPair(enemies.ToArray(), to);
            pairs.RemoveAll(tuple => ((Pirate)tuple.Item1).PushReloadTurns != 0);
            return pairs;
        }


        public static List<GameObject> GetObstacles(PirateGame game, Location origin, Location end) {

            var obstacles = new List<GameObject>();

            foreach (Point point in new Line(origin, end).GetPoints(10)) {
                foreach (Pirate pirate in game.GetEnemyLivingPirates()) {

                    if (pirate.Distance(point.GetLocation()) < game.PushDistance) {
                        obstacles.Add(pirate);
                    }
                }
            }

            return obstacles;
        }


        public static string GetPirateStatus(Pirate pirate, string status) {

            string task = "NO TASK";
            if (Main.tasks.ContainsKey(pirate.Id)) {
                task = Main.tasks[pirate.Id].ToString();
            }

            return task + " | " + pirate.Id + " | " + status;
        }


        public static Location[] GetFormation(Pirate Miner, Location oldLocation, Location newLocation) {

            if (Miner.HasCapsule()) {

                return new Location[2] { oldLocation, newLocation };

            } else {

                return null;
            }

        }



    }
}