using System.Collections.Generic;
using Pirates;

namespace Hydra {

	public class TaskEscort : Task {
		
		
		int radius = 500; // Maximum range
		Pirate pirate;
		
		public TaskEscort(Pirate pirate) {
			
			this.pirate = pirate;
		}
		
		
		override public string Preform() {
		    
		    foreach (Pirate epirate in Main.game.GetEnemyLivingPirates())
			    {
			        if (pirate.CanPush(epirate)){
			        
			            pirate.Push(epirate, epirate.Location.Towards(Main.game.GetEnemyMothership(),-5000));
			            return "pirate attacking!";
			        }
			    }

			if (Main.game.GetMyCapsule().Holder != null) { //Checks if path exists
                Location endgoal;

                if(Main.game.GetMyCapsule().Holder.Distance(Main.game.GetMyMothership()) == Main.game.PushRange + Main.game.PirateMaxSpeed){
                    pirate.Push(Main.game.GetMyCapsule().Holder, Main.game.GetMyMothership());
                    return "Pushed holder";
                }

				if (Main.game.GetMyCapsule().Holder.Distance(pirate) >= radius) {
					endgoal = Main.game.GetMyCapsule().Holder.GetLocation(); //His final goal
				} else {
				    var sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetMyCapsule().Holder);
					endgoal = Main.game.GetMyMothership().GetLocation(); //His final goal
				}
				
			    pirate.Sail(endgoal);	
			
			}
			
			return "TODO: Create escort";
		}
	
	
		override public int GetWeight() {
            
			if ( Main.game.GetMyCapsule().Holder != null && Main.game.GetMyCapsule().Holder != pirate) {


               
			    var sortedlist1 = Utils.SoloClosestPair(Main.game.GetEnemyLivingPirates(), Main.game.GetMyMothership());
			    Pirate myholder = Main.game.GetMyCapsule().Holder;
			    if (sortedlist1[0].Distance(Main.game.GetMyMothership()) > myholder.Distance(Main.game.GetMyMothership()) + myholder.PushRange)
			        return 0;
			        
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
