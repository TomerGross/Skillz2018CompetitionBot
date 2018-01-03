using Pirates;

namespace Punctuation {

	public class TraitAvoidEnemies : Trait {

		readonly int range;


		public TraitAvoidEnemies(int range) {

			this.range = range;
		}


		override public int Cost(Chunk chunk) {

			PirateGame game = Punctuation.game;

			int cost = 0, PDistance = game.PushDistance, PRange = game.PushRange;

			foreach (Pirate enemy in game.GetEnemyLivingPirates()) {

				if (enemy.Distance(chunk.GetLocation()) < PRange * range) {


					double price = (int) System.Math.Round(PDistance / 100.0) + 1;
					price -= enemy.Distance(chunk.GetLocation()) / 100.0;
					price *= 100;

					if (0 <= price) {
						cost += (int) System.Math.Round(price);
					}
				}
			}

			return cost;
		}


	}

}