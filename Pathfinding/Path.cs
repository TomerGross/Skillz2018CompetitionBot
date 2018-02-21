using System.Collections;
using System.Collections.Generic;
using Pirates;

namespace Hydra {

	public class Path {

		public enum Algorithm {
			ASTAR
		}
		
        readonly Location origin, goal;
		readonly List<Trait> traits;
        readonly Stack locations;
		

        public Path(Location origin, Location goal, List<Trait> traits) {
        
            this.origin = origin;
            this.goal = goal;
            this.traits = traits;
                
            locations = new AStar(origin, goal, traits).GetPathStack();
        }

        /*
		public Path(Chunk origin, Chunk endgoal, List<Trait> traits, Algorithm algorithm) {
		
			this.origin = origin;
			this.endgoal = endgoal;
			this.traits = traits;
		
			chunks_previous = new Stack();
		
			if (algorithm == Algorithm.ASTAR) {
				chunks = new AStar(traits,this.origin,this.endgoal).GetPathStack();
			} else {
				chunks = new Stack();
			}
		}
		*/

        public Location Pop() => (Location)locations.Pop();


		public Stack GetSailLocations() => locations;
		

		public override string ToString() {

			string build = "";

            foreach (Chunk chunk in locations.ToArray()) {
				build += chunk + " -> ";
			}

			return build;
		}

	}


}