using Pirates;

namespace Hydra {

	public class TraitRateByAsteroid : Trait {

		readonly int range;
		
		 //if the bias is minus the cost will make the pirate attracted to enemies
		 //if pirateID is -1 the trait will target all enemies, if it's an ID it will only effect the given pirate
		  
		  
		public TraitRateByAsteroid(int range) {

			this.range = range;
		}
		

		override public int Cost(Chunk chunk) {

			PirateGame game = Main.game;
			int cost = 0;

			foreach (Asteroid asteroid in game.GetAllAsteroids()) {
                    if (asteroid.Distance(chunk.GetLocation()) < range * 320) {
                        return 100000;
					}
				}
			

			return cost;
		}


	}

}