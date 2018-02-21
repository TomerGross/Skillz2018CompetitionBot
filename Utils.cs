using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public enum PirateState { NORMAL, HEAVY };

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


        public static List<Capsule> OrderByDistance(List<Capsule> list, Location l) => list.OrderBy(cap => DistanceWithWormhole(cap.GetLocation(), l, 0)).ToList();
        public static List<Capsule> FreeCapsulesByDistance(Location l) => game.GetMyCapsules().ToList().Where(cap => cap.Holder == null && !Main.capsulesTargetted.Values.Contains(cap)).OrderBy(cap => DistanceWithWormhole(cap.GetLocation(), l, 0)).ToList();


        public static List<Location> OrderByDistance(List<Location> list, Location l) => list.OrderBy(obj => DistanceWithWormhole(obj, l, 0)).ToList();
        public static List<MapObject> OrderByDistance(List<MapObject> list, Location l) => list.OrderBy(l.Distance).ToList();
        public static List<Mothership> OrderByDistance(List<Mothership> list, Location l) => list.OrderBy(l.Distance).ToList();


        public static List<Pirate> OrderByDistance(List<Pirate> list, Location l) => list.OrderBy(p => DistanceWithWormhole(p.GetLocation(), l, 0)).ToList();
        public static List<Pirate> EnemyHoldersByDistance(Location l) => game.GetEnemyLivingPirates().Where(e => e.HasCapsule()).OrderBy(l.Distance).ToList();
        public static List<Pirate> GetMyHolders() => game.GetMyLivingPirates().Where(p => p.Capsule != null).ToList();
        public static List<Pirate> PiratesWithTask(TaskType t) => (from tuple in Main.tasks.Where(pair => pair.Value.Item1 == t) select game.GetMyPirateById(tuple.Key)).ToList();
        public static List<Pirate> PiratesWithState(PirateState state) => game.GetAllMyPirates().Where(p => p.StateName == state.ToString()).ToList();


        public static List<Asteroid> AsteroidsByDistance(Location l) => game.GetLivingAsteroids().OrderBy(asteroid => asteroid.Distance(l)).ToList();


        public static List<Wormhole> OrderByDistance(List<Wormhole> list, Location l) => list.OrderBy(p => p.Distance(l)).ToList();
        public static List<Wormhole> WormHolesByDistance(Location l) => game.GetAllWormholes().ToList().OrderBy(w => w.Distance(l)).ToList();


        public static int ClosestEdgeDistance(Location l) => new List<int> { l.Col, l.Row, game.Cols - l.Col, game.Rows - l.Row }.OrderBy(dis => dis).First();


        public static Location OppositeLocation(Location loc) => new Location(game.Rows - loc.Row, game.Cols - loc.Col);

        public static Location Center(Location A, Location B) => new Location((A.Row + B.Row) / 2, (A.Col + B.Col) / 2);


        public static int DistanceWithWormhole(Location from, Location to, int speed) {

            if (game.GetAllWormholes().Any()) {
                
                var bestHole = game.GetAllWormholes().OrderBy(w => from.Distance(w) + w.Partner.Distance(to)).First();
                int teleportMin = from.Distance(bestHole) + bestHole.Partner.Distance(to) + bestHole.TurnsToReactivate * speed;

                return System.Math.Min(teleportMin, from.Distance(to));
            }

            return from.Distance(to);
        }


        public static Tuple<int, Location> NearestKillLocation(Location loc) => new List<Tuple<int, Location>> { ClosestAss(loc), CloestEdge(loc) }.OrderBy(tuple => tuple.Item1).First();


        public static Tuple<int, Location> ClosestAss(Location loc) {

            if (game.GetLivingAsteroids().Any()) {
                var cloestAss = game.GetLivingAsteroids().OrderBy(loc.Distance).First();
                return new Tuple<int, Location>(cloestAss.Distance(loc) - cloestAss.Size, cloestAss.Location);
            }

            return CloestEdge(loc);
        }


        public static Tuple<int, Location> CloestEdge(Location loc) {

            return new List<Tuple<int, Location>> {

                new Tuple<int, Location>(loc.Col, new Location(loc.Row, -1 * game.Rows)),
                new Tuple<int, Location>(game.Cols - loc.Col, new Location(loc.Row, game.Cols * 2)),
                new Tuple<int, Location>(loc.Row, new Location(-1 * game.Cols , loc.Col)),
                new Tuple<int, Location>(game.Rows - loc.Row, new Location(game.Rows * 2, loc.Col))
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

        public static int GetNumOfMyPiratesInRange(Location loc) {

            var mylivingpirates = game.GetMyLivingPirates();
            int count = 0;
            foreach (Pirate prt in mylivingpirates) {
                if (prt.GetLocation().InRange(loc, game.WormholeRange))
                    count += 1;

            }

            return count;
        }


        public static int GetNumOfEnemyPiratesOnPoint(Location loc) {

            var enemylivingpirates = game.GetEnemyLivingPirates();
            int count = 0;
            foreach (Pirate enemy in enemylivingpirates) {
                if (enemy.GetLocation().Row == loc.Row && enemy.GetLocation().Col == loc.Col)
                    count += 1;

            }

            return count;
        }


        public static bool AsteroidIsMoving(Asteroid ast) => ast.Location.Add(ast.Direction).Row == ast.Location.Row && ast.Location.Add(ast.Direction).Col == ast.Location.Col;


        public static bool InAsteroid(Location to, Asteroid ass) => to.InRange(ass.Location, ass.Size);


        public static string GetPirateStatus(Pirate pirate, string status) {

            string task = "NO TASK";
            if (Main.tasks.ContainsKey(pirate.Id)) {
                task = Main.tasks[pirate.Id].Item1.ToString();
            }

            bool canPush = game.GetEnemyLivingPirates().Any(pirate.CanPush);

            return task.Substring(0, 2)+ " | ID: " + pirate.Id + " | RT: " + pirate.PushReloadTurns + " | CP: " + canPush + " | " + status;
        }

    }
}