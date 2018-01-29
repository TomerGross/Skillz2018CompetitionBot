using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

	public class Utils {


        public static int ClosestEdgeDistance(Location loc) {
            return new List<int> { loc.Col, loc.Row, 6400 - loc.Col, 6400 - loc.Row }.OrderBy(dis => dis).First();
        }


        // Returns the distance from the cloest edge and the push location
        public static Tuple<int, Location> CloestEdge(Location loc) {

            return new List<Tuple<int, Location>> {
                new Tuple<int, Location>(loc.Col, new Location(loc.Row, -1)),
                new Tuple<int, Location>(6400 - loc.Col, new Location(loc.Row, 6401)),
                new Tuple<int, Location>(loc.Row, new Location(loc.Col, -1)),
                new Tuple<int, Location>(6400 - loc.Row, new Location(loc.Row, 6401))
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

            var sortedmapobjects = new List<MapObject>();
            var listofmapobjects = new List<MapObject>();

            return (from obj in objects select new Tuple<MapObject, int>(obj, obj.Distance(to))
                   ).OrderBy(tuple => tuple.Item1.Distance(to)).ToList();
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