/*using Pirates;
using System.Collections.Generic;

namespace Hydra {
	
	public class Predict {


		static readonly Dictionary<int, List<Location>> log = new Dictionary<int, List<Location>>();
		
		static readonly Dictionary<int, Dictionary<int, int>> possibleTargets = new Dictionary<int, Dictionary<int, int>>();
			
		
		public void Log() {	//This SHOULD be ran EVERY TURN

			foreach (Pirate pirate in Main.game.GetEnemyLivingPirates()) {

				if (log.ContainsKey(pirate.UniqueId)) {

					log.Add(pirate.UniqueId, new List<Location>(new[] { pirate.GetLocation() }));
					continue;
				}

				log[pirate.UniqueId].Add(pirate.GetLocation());
			}

		}
		
		
		public void AppendPossibleTargets(Pirate pirate, Pirate target, int a) {

			if (!possibleTargets.ContainsKey(pirate.UniqueId)) {
				
				possibleTargets.Add(pirate.UniqueId, new Dictionary<int, int>());
			}

			if (!possibleTargets[pirate.UniqueId].ContainsKey(target.UniqueId)) {

				possibleTargets[pirate.UniqueId].Add(target.UniqueId, 1);
				
				return;
			}
			
			possibleTargets[pirate.UniqueId][target.UniqueId] += a;
		}
		
		
		public int GetNextChunks(Pirate pirate) {
			
			List<Location> list = log[pirate.UniqueId];
			
			if (list.Count < 2) {
			
				return -999;
			}		
			
			double m = (list[list.Count - 1].Row - list[list.Count - 2].Row) / (list[list.Count - 1].Col - list[list.Count - 2].Col);
			
			return 0;
		}
		
		
	}

}
*/