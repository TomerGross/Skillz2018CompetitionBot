using Pirates;

namespace Hydra {

    public class TraitAttractedToGoal : Trait {

        readonly int range;
        readonly GameObject goal;

        public TraitAttractedToGoal(int range, GameObject goal) {

			this.range = range;
            this.goal = goal;
		}
		

		override public int Cost(Chunk chunk) {

			PirateGame game = Main.game;
			int cost = 0, PDistance = game.PushDistance, PRange = game.PushRange;

            if (Chunk.di) {

						double price = (int) System.Math.Round(PDistance / 100.0) + 1;
						price -= enemy.Distance(chunk.GetLocation()) / 100.0;
						price *= 100;

						if (0 <= price) {
							cost += bias * ((int) System.Math.Round(price));
						}
					}
				}
			}

			Main.game.Debug("COST: " + cost);
			return cost;
		}


	}

}