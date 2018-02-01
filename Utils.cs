using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

	public class Utils {

        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------


        public static bool pushAsteroid(Pirate pirate) {

            foreach (Asteroid asteroid in game.GetAllAsteroids()) {

            }

            return false;
        }


        public static Location SafeSail(Pirate pirate, Location to) {


            var asteroids = AsteroidsByDistance(pirate.Location);

            if (asteroids.Count > 0 && asteroids.First().Distance(pirate.Location) > Chunk.size * 2){
                return to;
            }

            if (game.GetAllAsteroids().Count() == 0 || pirate.Distance(to) < Chunk.size) {
                return to;
            }

            var traits = new List<Trait>() { new TraitRateByAsteroid(2) };
            Path path = new Path(Chunk.GetChunk(pirate.Location), Chunk.GetChunk(to), traits, Path.Algorithm.ASTAR);

            if (path.GetChunks().Count > 0) {

                Chunk nextChunk = path.Pop();
                return nextChunk.GetLocation();
            }

            return to;
        }


        public static List<Location> OrderByDistance(List<Location> list, Location l) => list.OrderBy(obj => obj.Distance(l)).ToList();

        public static List<Location> OrderByDistance(List<Pirate> list, Location l) => OrderByDistance((from pirate in list select pirate.Location).ToList(), l);
        public static List<Location> OrderByDistance(List<Capsule> list, Location l) => OrderByDistance((from capsule in list select capsule.Location).ToList(), l);
        public static List<Location> OrderByDistance(List<MapObject> list, Location l) => OrderByDistance((from obj in list select obj.GetLocation()).ToList(), l);
        public static List<Location> OrderByDistance(List<Mothership> list, Location l) => OrderByDistance((from ship in list select ship.Location).ToList(), l);


        public static List<Pirate> GetMyHolders() => (from cap in game.GetMyCapsules().ToList().Where(cap => cap.Holder != null) select cap.Holder).ToList();


        public static List<Pirate> EnemyHoldersByDistance(Location l) => (from cap in game.GetEnemyCapsules().ToList().Where(cap => cap.Holder != null).OrderBy(cap => cap.Distance(l)).ToList() select cap.Holder).ToList();


        public static List<Location> FreeCapsulesByDistance(Location l) => (from cap in game.GetMyCapsules().ToList().Where(cap => cap.Holder == null).OrderBy(cap => cap.Distance(l)).ToList() select cap.GetLocation()).ToList();


        public static List<Asteroid> AsteroidsByDistance(Location l) => game.GetAllAsteroids().OrderBy(asteroid => asteroid.Distance(l)).ToList();


        public static List<Pirate> PiratesWithTask(TaskType t) => (from tuple in Main.tasks.Where(pair => pair.Value == t) select game.GetMyPirateById(tuple.Key)).ToList(); 
  

        public static int ClosestEdgeDistance(Location l) => new List<int> { l.Col, l.Row, game.Cols - l.Col, game.Rows - l.Row }.OrderBy(dis => dis).First();
 

        public static Location OppositeLocation(Location loc){
            return new Location(game.Rows - loc.Row, game.Cols - loc.Col);
        }


        // Returns the distance from the cloest edge and the push location
        public static Tuple<int, Location> CloestEdge(Location loc) {

            return new List<Tuple<int, Location>> {
                new Tuple<int, Location>(loc.Col, new Location(loc.Row, -1)),
                new Tuple<int, Location>(6400 - loc.Col, new Location(loc.Row, 6401)),
                new Tuple<int, Location>(loc.Row, new Location(-1, loc.Col)),
                new Tuple<int, Location>(6400 - loc.Row, new Location(6401, loc.Col))
            }.OrderBy(tuple => tuple.Item1).First();
        }


        // ClosestPair returns a list of the closest pair of enemy and friendly bot
        public static Tuple<MapObject, MapObject> ClosestPair(MapObject[] group1, MapObject[] group2) {

            int min = group1.Min(obj => group2.Min(obj.Distance));

            var obj1 = group1.First(obj => min == group2.Min(obj.Distance));
            var obj2 = group1.First(obj => min == group1.Min(obj.Distance));

            return new Tuple<MapObject, MapObject> (obj1, obj2);
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

			return task + " | " + pirate.UniqueId.ToString() + " | " + status;
		}


		public static Location[] GetFormation(Pirate Miner,Location oldLocation,Location newLocation) {

			if (Miner.HasCapsule()) {

				return new Location[2] { oldLocation,newLocation };

			} else {

				return null;
			}

		}



	}
}