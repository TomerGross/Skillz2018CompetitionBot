using System.Collections.Generic;
using Pirates;

namespace Hydra {

	public class TaskDefault : Task {

		//-------------------Globals---------------------------------------------
		public static Dictionary<int,Path> paths = new Dictionary<int,Path>();
		public PirateGame game = Main.game;
		//-----------------------------------------------------------------------


		private Pirate pirate;
		
		public TaskDefault(Pirate pirate) {
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
			if (game.GetEnemyCapsule().Holder != null) {

				Pirate enemyHolder = game.GetEnemyCapsule().Holder;

				if (pirate.CanPush(enemyHolder)) {
					pirate.Push(enemyHolder,enemyHolder.Location.Towards(game.GetEnemyMothership(), -5000));
					return "Berserker pushed enemy holder away.";
					
				}
			    else if (pirate.Distance(enemyHolder) <= 3 * pirate.PushRange) {
					pirate.Sail(enemyHolder);
					return "Berserker moving towards enemy holder...";
				}
				else
				{
				    pirate.Sail(Main.mineEnemy.Towards(Main.game.GetEnemyMothership(),650));
				    return "Berserker moved towards enemy mine.";
				    
				}
				

			} 
			else {
                pirate.Sail(Main.mineEnemy.Towards(Main.game.GetEnemyMothership(),650));
				return "Berserker moved towards enemy mine.";
				
			}
		}
		
		
		
		override public int GetWeight() {
            
			if (game.GetMyCapsule().Holder == null){
			   
                var sortedlist = new List<MapObject>();
				sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetEnemyCapsule());
				
				int numofpirates = Main.game.GetAllMyPirates().Length;
				
			    if (sortedlist.IndexOf(pirate)+1 <= numofpirates / 2)
				    return 1000;
				return 0;    
			} 
			
			return 0;		
		}
		
					
		override public int Bias() {

			return 0;	
		}

		
		
		
	}
	
	
}
