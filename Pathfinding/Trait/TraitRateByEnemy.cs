using Pirates;

namespace Hydra {

	public class TraitRateByEnemy : Trait {

		readonly int range, bias, pirateID;
		
		 //if the bias is minus the cost will make the pirate attracted to enemies
		 //if pirateID is -1 the trait will target all enemies, if it's an ID it will only effect the given pirate
		  
		  
		public TraitRateByEnemy(int range, int bias, int pirateID) {

			this.range = range;
			this.pirateID = pirateID;
		}
		

		override public int Cost(Chunk chunk) {

			PirateGame game = Main.game;
			int cost = 0, PDistance = game.PushDistance, PRange = game.PushRange;

			foreach (Pirate enemy in game.GetEnemyLivingPirates()) {
				if (pirateID == -1 || enemy.UniqueId == pirateID) {
					if (enemy.Distance(chunk.GetLocation()) < PRange * range) {

						double price = (int) System.Math.Round(PDistance / 100.0) + 1;
						price -= enemy.Distance(chunk.GetLocation()) / 100.0;
						price *= 100;

						if (0 <= price) {
							cost += bias * ((int) System.Math.Round(price));
						}
					}
				}
			}
			
			return cost;
		}


	}

}