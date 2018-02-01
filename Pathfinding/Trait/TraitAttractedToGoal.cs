using Pirates;

namespace Hydra {

    public class TraitAttractedToGoal : Trait {

        readonly int range;
        readonly Chunk goal;


        public TraitAttractedToGoal(int range, Location goal) {

			this.range = range;
            this.goal = Chunk.GetChunk(goal);
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