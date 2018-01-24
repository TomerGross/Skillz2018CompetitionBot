using Pirates;
using System.Collections.Generic;

namespace Hydra {

	public class TaskMiner : Task {

		public static Dictionary<int,Path> paths = new Dictionary<int,Path>();


		Pirate pirate;

		public TaskMiner(Pirate pirate) {
			this.pirate = pirate;
		}
		

		override public string Preform() {

			if (pirate.HasCapsule()) {
				
				// If there is no path
				if (!paths.ContainsKey(pirate.UniqueId) || paths[pirate.UniqueId] == null) { 

					Chunk origin = Chunk.GetChunk(pirate.GetLocation()); // Staring position
					Chunk endgoal = Chunk.GetChunk(Main.mine);

					var traits = new List<Trait>() { new TraitRateByEnemy(1,1,-1) };
					paths[pirate.UniqueId] = new Path(origin,endgoal,traits,Path.Algorithm.ASTAR);;

				}   

				Path path = paths[pirate.UniqueId];
				Chunk nextChunk = path.GetNext();

				if (nextChunk != null) {

					if (nextChunk.GetLocation().Distance(pirate) < Main.game.PirateMaxSpeed / 2) {
						path.GetChunks().Pop();
					}

					pirate.Sail(nextChunk.GetLocation());
					return Utils.GetPirateStatus(pirate,"Sailing to: " + nextChunk.ToString());
	
				} else {
					
					return Utils.GetPirateStatus(pirate, "Is idle");
				}

			} else {

				pirate.Sail(Main.mine);
			
				return Utils.GetPirateStatus(pirate, "Sailing to mine...");
			}
		}


		override public int GetWeight() {

			if (Main.game.GetMyCapsule().Holder == null) {
			
				var sortedlist = new List<MapObject>();
				sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetMyCapsule());
				int numofpirates = Main.game.GetAllMyPirates().Length;
				return (numofpirates - sortedlist.IndexOf(pirate)) * (100 / numofpirates);
				
			} 
			else {

				if (Main.game.GetMyCapsule().Holder == pirate) {
					return 100; //he is already in his task
				}
				
				return 0; // only one miner for this lvl of competition
			}
		}


		override public int Bias() {
			return 100;
		}

	}
}
