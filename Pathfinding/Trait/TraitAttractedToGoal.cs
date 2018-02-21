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

            if (chunk.Distance(goal) <= range){
                return -(range - chunk.Distance(goal));
            }

            return 0; 
		}


	}
}