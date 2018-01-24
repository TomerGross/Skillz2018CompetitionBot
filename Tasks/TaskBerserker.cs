using System.Collections.Generic;
using Pirates;

namespace Hydra {

	public class TaskBerserker : Task {

		//-------------------Globals---------------------------------------------
		public static Dictionary<int,Path> paths = new Dictionary<int,Path>();
		public PirateGame game = Main.game;
		//-----------------------------------------------------------------------


		private Pirate pirate;
		
		public TaskBerserker(Pirate pirate) {
			this.pirate = pirate;
		}
		
		
		override public string Preform() {
			
			if (game.GetEnemyCapsule().Holder != null) {

				Pirate enemyHolder = game.GetEnemyCapsule().Holder;

				if (pirate.CanPush(enemyHolder)) {
					pirate.Push(enemyHolder,enemyHolder.Location.Towards(game.GetEnemyMothership(), -5000));
					return "Berserker pushed enemy holder away.";
					
				}
			    else {
					pirate.Sail(enemyHolder);
					return "Berserker moving towards enemy holder...";
				}

			} 
			else {

				pirate.Sail(Main.mineEnemy.Towards(Main.game.GetEnemyMothership(),650));
				return "Berserker moved towards enemy mine.";
			}
		}
		
		
		
		override public int GetWeight() {

			if (game.GetEnemyCapsule().Holder != null){
			
                var sortedlist = new List<MapObject>();
				sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetEnemyCapsule().Holder);
				
				int numofpirates = Main.game.GetAllMyPirates().Length;
				
				return (numofpirates - sortedlist.IndexOf(pirate)) * (100 / numofpirates);
			} 
			
			return 0;		
		}
		
					
		override public int Bias() {

			return 50;	
		}

		
		
		
	}
	
	
}
