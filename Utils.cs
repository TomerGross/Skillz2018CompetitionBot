using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

	public class Utils {


        public static int GetClosestEdgeDistance(Location loc){
            return new List<int> { loc.Col, loc.Row, 6400 - loc.Col, 6400 - loc.Row }.OrderBy(dis => dis).First();
        }


        // ClosestPair returns a list of the closest pair of enemy and friendly bot
        public static List<MapObject> ClosestPair(MapObject[] group1, MapObject[] group2) {

            int min = group1.Min(obj => group2.Min(obj.Distance));

            var obj1 = group1.First(obj => min == group2.Min(obj.Distance));
            var obj2 = group1.First(obj => min == group1.Min(obj.Distance));

            return new List<MapObject>() { obj1, obj2 };
        }


        public static Tuple<int, Location> CloestEdge(Location loc) {

            return new List<Tuple<int, Location>> {
                new Tuple<int, Location>(loc.Col, new Location(loc.Row, -1)),
                new Tuple<int, Location>(6400 - loc.Col, new Location(loc.Row, 6401)),
                new Tuple<int, Location>(loc.Row, new Location(loc.Col, -1)),
                new Tuple<int, Location>(6400 - loc.Row, new Location(loc.Row, 6401))
            }.OrderBy(tuple => tuple.Item1).First();
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
            if (Main.tasks.ContainsKey(pirate.id)) {
                task = Main.tasks[pirate.Id].ToString();
            }

			return task + " | " + pirate.UniqueId.ToString() + " | " + status;
		}


		public static Pirate GetClosestCapsuleEnemyPirate(Pirate pirate,PirateGame game) {

			Pirate closestPirate = game.GetEnemyLivingPirates()[0];
			int closestDistace = pirate.Distance(closestPirate);

			foreach (Pirate enemy in game.GetEnemyLivingPirates()) {

				if (enemy.HasCapsule() && pirate.Distance(enemy) < closestDistace && (pirate.Distance(enemy)) < game.PushRange) {
					closestDistace = pirate.Distance(closestPirate) - 400;
					closestPirate = enemy;
				}
			}

			return closestPirate;
		}


		public static bool PushBeta(Pirate PusherPi,Pirate PushedPi,PirateGame game,bool way,Location location) {

			//True meens to the point 
			//False means away
			if (way) {
				//to the location
				PusherPi.Push(PushedPi,location);
				return true;
			} else {
				int PushRange = PusherPi.PushDistance;
				Location pushedLocation = PushedPi.GetLocation();
				int dist = location.Distance(pushedLocation);
				dist = dist + PushRange;
				PusherPi.Push(PushedPi,location.Towards(pushedLocation,dist));
				return true;
			}
		}



		public static bool TryPush(Pirate pirate,PirateGame game) {

			foreach (Pirate enemy in game.GetEnemyLivingPirates()) {
				if (pirate.CanPush(enemy)) {
					pirate.Push(enemy,Utils.CanPushOutBeta(enemy.GetLocation(),game));

					System.Console.WriteLine("pirate " + pirate + " pushes " + enemy + " towards " + enemy.InitialLocation);

					return true;
				}
			}

			return false;
		}

		
		public static Location[] GetFormation(Pirate Miner,Location oldLocation,Location newLocation) {

			if (Miner.HasCapsule()) {

				return new Location[2] { oldLocation,newLocation };

			} else {

				return null;
			}

		}


		//The function return a list of sorted map object from group1 by distance to object2	
		public static List<MapObject> SoloClosestPair(MapObject[] group1,MapObject object2) {

			var sortedmapobjects = new List<MapObject>();
			var listofmapobjects = new List<MapObject>();

			foreach (MapObject mapobject in group1) {
				listofmapobjects.Add(mapobject);
			}

			while (listofmapobjects.Count > 0) {
			
				MapObject mingroup1 = listofmapobjects[0];
				int mindistance = group1[0].Distance(object2);

				for (int i = 0; i < listofmapobjects.Count; i++) {
					if (listofmapobjects[i].Distance(object2) < mindistance) {
						mindistance = listofmapobjects[i].Distance(object2);
						mingroup1 = listofmapobjects[i];
					}

				}
				
				sortedmapobjects.Add(mingroup1);
				listofmapobjects.Remove(mingroup1);
			}

			return sortedmapobjects;
		}

	}
}