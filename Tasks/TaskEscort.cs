using System.Collections.Generic;
using Pirates;

namespace Hydra {

	public class TaskEscort : Task {
		
		
		int radius = 1000; // Maximum range
		Pirate pirate;
		
		public TaskEscort(Pirate pirate) {
			
			this.pirate = pirate;
		}
		
		
		override public string Preform() {

			if (Main.game.GetMyCapsule().Holder != null) { //Checks if path exists
                Location endgoal;

				if (Main.game.GetMyCapsule().Holder.Distance(pirate) >= radius) {
					endgoal = Main.game.GetMyCapsule().Holder.GetLocation(); //His final goal
				} else {
					endgoal = Main.game.GetMyMothership().GetLocation(); //His final goal
				}
				
			    pirate.Sail(endgoal);	
			
			}
			
			return "TODO: Create escort";
		}
	
	
		override public int GetWeight() {

			if ( Main.game.GetMyCapsule().Holder != null && Main.game.GetMyCapsule().Holder != pirate) {

				var sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetMyCapsule().Holder);
				int numofpirates = Main.game.GetAllMyPirates().Length;
				
				return (numofpirates - sortedlist.IndexOf(pirate)) * (100 / numofpirates);
			}

			return 0;
		}


		override public int Bias() {
			if (Main.game.GetMyCapsule().Holder == null) {
				return 0;
			} else {
				return 50;
			}
		}
	
	
	}
}
