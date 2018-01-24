using Pirates;
using System.Collections.Generic;

namespace Hydra {

	public class TaskMiner : Task {

		Pirate pirate;
		Path path;
		
		public TaskMiner(Pirate pirate) {
			this.pirate = pirate;
		}
		
		
		override public string Preform() {

			if (pirate.HasCapsule()) {
				
				// If there is no path
				if (this.path == null) { 

					Chunk origin = Chunk.GetChunk(pirate.GetLocation()); // Staring position
					Chunk endgoal = Chunk.GetChunk(Main.game.GetMyMothership().Location);

					var traits = new List<Trait>() { new TraitRateByEnemy(1,1,-1) };
					this.path = new Path(origin, endgoal, traits, Path.Algorithm.ASTAR);
				}
				
				if(path.GetChunks().Count > 0){
				
					Chunk nextChunk = path.Pop();
					pirate.Sail(nextChunk.GetLocation());
					return Utils.GetPirateStatus(pirate,"Sailing to: " + nextChunk.ToString());
				} 
					
				return Utils.GetPirateStatus(pirate, "Is idle");

			} else {

				pirate.Sail(Main.mine);
			
				return Utils.GetPirateStatus(pirate, "Sailing to mine...");
			}
		}


		override public int GetWeight() {

			if (Main.game.GetMyCapsule().Holder == null) {
			
				var sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetMyCapsule());
				int numofpirates = Main.game.GetAllMyPirates().Length;
				
				return (numofpirates - sortedlist.IndexOf(pirate)) * (100 / numofpirates);
			}  

			if (Main.game.GetMyCapsule().Holder == pirate) {
				return 1000; //he is already in his task
			}
				
			return 50; // only one miner for this lvl of competition		
		}


		override public int Bias() {
			return 100;
		}

	}
}
