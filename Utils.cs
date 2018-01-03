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

	}
}