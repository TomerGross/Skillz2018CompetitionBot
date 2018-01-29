namespace Hydra {

    public class TraitRateByEdges : Trait {

        readonly int range, multiplier;


        public TraitRateByEdges(int range, int multiplier) {

			this.range = range;
            this.multiplier = multiplier;
		}


        override public int Cost(Chunk chunk) {

            int radius = range * Main.game.PirateMaxSpeed;
            int distanceFromWall = Utils.ClosestEdgeDistance(chunk.GetLocation());

            if(distanceFromWall < radius){
                return distanceFromWall * multiplier;
            }

            return 0;
        }


	}
}