using Pirates;

namespace Hydra {

	public class TaskMole : Task{


		Pirate pirate;

		public TaskMole(Pirate pirate) {
			this.pirate = pirate;
		}
		
		override public string Preform() {

			PirateGame game = Main.game;

			int radius = game.PirateMaxSpeed * 2;
			
			if (game.GetEnemyCapsule().Holder != null) {

				Pirate enemyCapuleHolder = game.GetEnemyCapsule().Holder;
				Location towardsEnemy = game.GetEnemyMothership().GetLocation().Towards(game.GetEnemyCapsule(),radius);

				// If the pirate is in position and can attack
				if (pirate.Distance(towardsEnemy) < (radius / 8)) {
					if (enemyCapuleHolder.HasCapsule() && pirate.CanPush(enemyCapuleHolder)) {

						pirate.Push(enemyCapuleHolder,Utils.CanPushOutBeta(enemyCapuleHolder.GetLocation(),game));
						return Utils.GetPirateStatus(pirate,"pushed enemy");
					}
					
				} else {
					
					// Sail to a position
					pirate.Sail(towardsEnemy);
					return "Pirate is sailing to position";
				}
			}

			pirate.Sail(game.GetEnemyMothership().GetLocation().Towards(Main.mineEnemy, radius));
			return Utils.GetPirateStatus(pirate, "Is sailing to position");
		}


	
        override public int GetWeight(){

			return 60;    
        }


		override public int Bias() {
			
			return 50;
		}
		
	}
}
