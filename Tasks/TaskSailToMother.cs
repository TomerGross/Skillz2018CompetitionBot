using System.Collections.Generic;
using Pirates;

namespace Hydra {
	
	public class TaskSailToMother : Task  {
	
		
		public static Dictionary<int, Path> paths = new Dictionary<int, Path>();

		Pirate pirate;

		public TaskSailToMother(Pirate pirate) {
			this.pirate = pirate;
		}
		
		
		public string Preform() {

			if (!paths.ContainsKey(pirate.UniqueId) ||paths[pirate.UniqueId] == null) { //Checks if path exists
				
				Chunk origin = Chunk.GetChunk(pirate.GetLocation()); //His starting position
				Chunk endgoal = Chunk.GetChunk(Main.game.GetMyMothership().GetLocation()); //His final goal
				
				paths[pirate.UniqueId] = new Path(origin, endgoal, Path.Algorithm.ASTAR); //Generate a path using AStar
			}
			
			int ID = pirate.UniqueId;
			Path path = paths[ID];
			
			if(path.GetChunks().Count > 0){

				Chunk next = path.GetNext();

				if (next != null) {
	
					if (next.GetLocation().Distance(pirate) < (Chunk.divider / 2)) {
						path.GetChunks().Pop();
					}
	
					pirate.Sail(next.GetLocation());
					return Utils.GetPirateStatus(pirate,"Sailing to: " + next.ToString());
				}
            }
            
            return Utils.GetPirateStatus(pirate, "Next is null");
		}
		
		
		public int GetWeight() {

			if (Punctuation.game.GetMyCapsule().Holder == pirate) {
				return 100;
			}

			return 0;		
		}
		
				
		public int Bias() {
			return 1;
		}
		
	}
}
