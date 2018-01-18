using System.Collections.Generic;
using Pirates;

namespace Punctuation {

	public class Utils {


		public static List<GameObject> GetObstacles(PirateGame game,Location origin,Location end) {

			var obstacles = new List<GameObject>();

			foreach (Point point in new Line(origin,end).GetPoints(10)) {
				foreach (Pirate pirate in game.GetEnemyLivingPirates()) {

					if (pirate.Distance(point.GetLocation()) < game.PushDistance) {
						obstacles.Add(pirate);
					}
				}
			}

			return obstacles;
		}


		public static int GetMinersAlive(PirateGame game) {

			int c = 0;
			
			foreach (Pirate pirate in game.GetMyLivingPirates()) {
				if (Punctuation.roles.ContainsKey(pirate.UniqueId) && Punctuation.roles[pirate.UniqueId] == Punctuation.Role.MINER) {
					c++;
				}
			}

			return c;
		}


		public static string GetPirateStatus(Pirate pirate,string status) {

			string role = Punctuation.roles[pirate.UniqueId].ToString();
			return role + " | " + pirate.UniqueId.ToString() + " | " + status;
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


		public static Location CanPushOutBeta(Location location,PirateGame game) {

			int side = 0;
			int distance = location.Col;

			if (distance > location.Row) {
				side = 1;
				distance = location.Row;
			}

			if (distance > (6401 - location.Row)) {
				side = 2;
				distance = 6401 - location.Row;
			}

			if (distance > (6401 - location.Col)) {
				side = 3;
				distance = 6401 - location.Col;
			}

			switch (side) {
				case 0:
					return new Location(location.Row,0);

				case 1:
					return new Location(0,location.Col);

				case 2:
					return new Location(6401,location.Col);

				case 3:
					return new Location(location.Row,6401);

				default:
					return new Location(6401,6401);
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
		
		public MapObject[] ClosestPair(MapObject[] group1, MapObject[] group2){
		    
		    /* return list of the closest pair from both groups*/
		    MapObject mingroup1, mingroup2;
		    int mindistance = group1[0].distance(group2[0]);
		    for(int i = 0 ; i < group1.GetLength(); i++){
		        for(int j = 0; j < group2.GetLength(); j++){
		            if (group1[i].distance(group2[j]) < mindistance){
		                    mindistance = group1[i].distance(group2[j]);
		                    mingroup1 = group1[i];
		                    mingroup2 = group2[j];
		                }
		        }
		    }
		    return [mingroup1, mingroup2];
		}
		
		public List<MapObject> SoloClosestPair(MapObject[] group1, MapObject object2){
		    
                //the function return a list of sorted map object from group1 by distance to object2
                List<MapObject> sortedmapobjects = new List<MapObject>();
                List<MapObject> listofmapobjects = new List<MapObject>();
    
                foreach (MapObject mapobject in group1)
                {
                    listofmapobjects.Add(mapobject);
                }
    
                while (listofmapobjects.Count > 0)
                {
                    MapObject mingroup1 = listofmapobjects[0];
    
                    int mindistance = group1[0].Distance(object2);
    
                    for (int i = 0; i < listofmapobjects.Count; i++)
                    {
                        if (listofmapobjects[i].Distance(object2) < mindistance)
                        {
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