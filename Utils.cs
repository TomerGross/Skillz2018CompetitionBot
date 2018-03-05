using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public enum PirateState { NORMAL, HEAVY };

    public class Utils {

        public static PirateGame game = Main.game;


        public static List<Capsule> OrderByDistance(List<Capsule> list, Location l) => list.OrderBy(cap => DistanceWithWormhole(cap.GetLocation(), l, 0)).ToList();
        public static List<Capsule> FreeCapsulesByDistance(Location l) => game.GetMyCapsules().ToList().Where(cap => cap.Holder == null && !Main.capsulesTargetted.Values.Contains(cap)).OrderBy(cap => DistanceWithWormhole(cap.GetLocation(), l, 0)).ToList();


        public static List<Location> OrderByDistance(List<Location> list, Location l) => list.OrderBy(obj => DistanceWithWormhole(obj, l, 0)).ToList();
        public static List<MapObject> OrderByDistance(List<MapObject> list, Location l) => list.OrderBy(l.Distance).ToList();
        public static List<Mothership> OrderByDistance(List<Mothership> list, Location l) => list.OrderBy(l.Distance).ToList();


        public static List<Pirate> OrderByDistance(List<Pirate> list, Location l) => list.OrderBy(p => DistanceWithWormhole(p.GetLocation(), l, 0)).ToList();
        public static List<Pirate> EnemyHoldersByDistance(Location l) => game.GetEnemyLivingPirates().Where(e => e.HasCapsule()).OrderBy(l.Distance).ToList();
        public static List<Pirate> PiratesWithTask(TaskType t) => (from tuple in Main.tasks.Where(pair => pair.Value.Item1 == t) select game.GetMyPirateById(tuple.Key)).ToList();
        public static List<Pirate> PiratesWithState(PirateState state) => game.GetAllMyPirates().Where(p => p.StateName == state.ToString()).ToList();
        public static List<Pirate> GetMyHolders() => game.GetMyLivingPirates().Where(p => p.HasCapsule() && !HasEnemyBomb(p)).ToList();

        public static List<Asteroid> AsteroidsByDistance(Location l) => game.GetLivingAsteroids().OrderBy(asteroid => asteroid.Distance(l)).ToList();


        public static List<Wormhole> OrderByDistance(List<Wormhole> list, Location l) => list.OrderBy(p => p.Distance(l)).ToList();
        public static List<Wormhole> WormHolesByDistance(Location l) => game.GetAllWormholes().ToList().OrderBy(w => w.Distance(l)).ToList();


        /// <summary> Returns the location that opposite to the given one </summary>
        public static Location OppositeLocation(Location loc) => new Location(game.Rows - loc.Row, game.Cols - loc.Col);


        /// <summary> Finds the middle between two points </summary>
        public static Location Center(Location A, Location B) => new Location((A.Row + B.Row) / 2, (A.Col + B.Col) / 2);


        /// <summary> Checks if an enemy pirate is carrying a bomb </summary>
        public static bool HasEnemyBomb(Pirate pirate) => pirate.StickyBombs.ToList().Any(b => b.Owner == game.GetEnemy());


        /// <summary> Checks if a friendly pirate is carrying a bomb </summary>
        public static bool HasMyBomb(Pirate enemypirate) => enemypirate.StickyBombs.ToList().Any(b => b.Owner == game.GetMyself());


        /// <summary> Checks if an asteroid is  moving. </summary>
        public static bool AsteroidIsMoving(Asteroid ast) => !(ast.Location.Add(ast.Direction).Row == ast.Location.Row && ast.Location.Add(ast.Direction).Col == ast.Location.Col);


        /// <summary> Checks if a location is an asteroid </summary>
        public static bool InAsteroid(Location loc, Asteroid ass) => loc.InRange(ass.Location, ass.Size);


        /// <summary> Calculates locations on a cricle. </summary>
        /// <returns> The requested number of locations on a circle</returns>
        /// <param name="n"> The Desired number of points </param>
        public static List<Location> LocOnCricle(Location center, int n, int radius) {

            return (from k in Enumerable.Range(1, n + 1).ToList()
                    let x = (int)(System.Math.Round(center.Row + radius * System.Math.Sin(2 * System.Math.PI * k / n)))
                    let y = (int)(System.Math.Round(center.Col + radius * System.Math.Cos(2 * System.Math.PI * k / n)))
                    select new Location(x, y)).ToList();
        }


        /// <summary> Check if any friendly pirates will be killed by a pushed asteroid </summary>
        /// <returns> All the pirates that may be killed </returns>
        /// <param name="pushLocation"> The location the asteroid will be pushed to</param>
        public static List<Pirate> FriendlyPiratesInDanger(Asteroid ast, Location pushLocation) {

            var direction = new Location(pushLocation.Row - ast.Location.Row, pushLocation.Col - ast.Location.Col);
            var killed = new List<Pirate>();
            var location = ast.Location;

            while(location != pushLocation && location.Distance(ast) < game.Cols / 4 && location.InMap()){
                killed.AddRange(game.GetMyLivingPirates().Where(p => p.InRange(location, ast.Size)));
                location = location.Add(direction);
            }

            return killed;
        }


        /// <summary> Finds the best location to push the asteroid to </summary>
        /// <returns> A tuple with the optimal location and the number of enemy pirates in danger </returns>
        /// <param name="pushDistance"> The total potential push distance </param>
        public static Tuple<Location, int> OptimalAsteroidPushLocation(int pushDistance, Asteroid ast){
            
            var locations = LocOnCricle(ast.Location, 6, pushDistance / 2 + ast.Size);
            locations.AddRange(LocOnCricle(ast.Location, 10, pushDistance + ast.Size));
            locations = locations.Where(l => !game.GetMyLivingPirates().Any(p => p.InRange(l, ast.Size))).ToList();
            locations = locations.Where(l => !FriendlyPiratesInDanger(ast, l).Any()).ToList();
            locations = locations.Where(l => !l.Equals(ast.Location)).ToList();

            var possibleKills = locations.ToDictionary(l => l, l => game.GetEnemyLivingPirates().Count(e => e.InRange(l, ast.Size))).OrderByDescending(p => p.Value);

            if (possibleKills.Any()) return new Tuple<Location, int>(locations.First(), game.GetEnemyLivingPirates().Count(e => e.InRange(locations.First(), ast.Size)));

            return new Tuple<Location, int>(new Location(ast.Location.Row - ast.Direction.Row, ast.Location.Col - ast.Direction.Col), -1);
        }
        

        /// <summary> Finds the best location to push a bomber to /summary>
        /// <param name="pirate"> The friendly pushing pirate </param>
        public static Location OptimalBomberPushLocation(Pirate pirate, Pirate enemyBomber){

            int bombExplosion = enemyBomber.StickyBombs.ToList().First().ExplosionRange;             

            List<Location> listOfPoints = LocOnCricle(enemyBomber.Location, 8, pirate.PushDistance);
            Dictionary<Location, int> pointPotential = listOfPoints.ToDictionary(l => l, l => game.GetMyLivingPirates().Count(p => p.InRange(l, bombExplosion)) - game.GetEnemyLivingPirates().Count(e => e.InRange(l, bombExplosion)));

            if (pointPotential.Any()) return pointPotential.OrderBy(t => t.Value).First().Key;

            return pirate.Location.Towards(enemyBomber, pirate.PushDistance * 2);
        }
        

        /// <summary> Gets and returns the number of enemy pirates on point. </summary>
        public static int GetNumOfEnemyPiratesOnPoint(Location loc) => game.GetEnemyLivingPirates().Count(e => e.GetLocation().Row == loc.Row && e.GetLocation().Col == loc.Col);

        /// <summary> Gets and returns the number of friendly pirates on point. </summary>
        public static int GetNumOfMyPiratesOnPoint(Location loc) => game.GetMyLivingPirates().Count(e => e.GetLocation().Row == loc.Row && e.GetLocation().Col == loc.Col);

        /// <summary> Gets and returns the distance from the nearest edge </summary>
        public static int ClosestEdgeDistance(Location l) => new List<int> { l.Col, l.Row, game.Cols - l.Col, game.Rows - l.Row }.OrderBy(dis => dis).First();


        /// <summary> Finds the minimal distance between two locations </summary>
        /// <returns> The minimal distance, either direct or with a wormhole</returns>
        public static int DistanceWithWormhole(Location from, Location to, int speed) {

            if (!game.GetAllWormholes().Any()) return from.Distance(to);

            var bestHole = game.GetAllWormholes().OrderBy(w => from.Distance(w) + w.Partner.Distance(to)).First();
            int teleportMin = from.Distance(bestHole) + bestHole.Partner.Distance(to) + bestHole.TurnsToReactivate * speed;

            return System.Math.Min(teleportMin, from.Distance(to));
        }


        /// <summary> Finds the minimal distance needed for a push to kill </summary>
        /// <returns> The minimal kill distance and location </returns>
        public static Tuple<int, Location> NearestKillLocation(Location loc) => new List<Tuple<int, Location>> { ClosestAsteroid(loc), CloestEdge(loc), ClosestExplodingBomb(loc) }.OrderBy(tuple => tuple.Item1).First();


        /// <summary> Gets the cloest bomb</summary>
        /// <returns> A tuple with distance to the cloest bomb and the location of it </returns>
        public static Tuple<int, Location> ClosestExplodingBomb(Location loc) {

            var stickybombs = game.__stickyBombs.Where(b => b.Countdown <= 1).OrderBy(loc.Distance);

            if (!stickybombs.Any()) return new Tuple<int, Location>(game.Cols * 2, CloestEdge(loc).Item2);

            var bomb = stickybombs.First();

            return new Tuple<int, Location>(bomb.Distance(loc) - bomb.ExplosionRange, bomb.GetLocation());
        }


        /// <summary> Finds the cloest asteroid </summary>
        /// <returns> A tuple with distance to the cloest bomb and the location of it </returns>
        public static Tuple<int, Location> ClosestAsteroid(Location loc) {

            var asteroids = game.__livingAsteroids.OrderBy(loc.Distance).OrderBy(loc.Distance);

            if (!asteroids.Any()) return CloestEdge(loc);

            var asteroid = asteroids.First();

            return new Tuple<int, Location>(asteroid.Distance(loc) - asteroid.Size, asteroid.Location.Add(asteroid.Direction));
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


        public static string GetPirateStatus(Pirate pirate, string status) {

            string task = "NO TASK";
            if (Main.tasks.ContainsKey(pirate.Id)) {
                task = Main.tasks[pirate.Id].Item1.ToString();
            }

            bool canPush = game.GetEnemyLivingPirates().Any(pirate.CanPush);

            return task.Substring(0, 2) + " | ID: " + pirate.Id + " | RT: " + pirate.PushReloadTurns + " | CP: " + canPush + " | " + status;
        }

    }
}