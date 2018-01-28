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

            int MoveDistance = Main.game.PirateMaxSpeed;               

            if (MoveDistance * range > chunk.Distance(goal)){
                return -5 * (MoveDistance * range - chunk.Distance(goal));
            }

            return 0; 
		}


	}
}