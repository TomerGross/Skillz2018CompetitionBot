using System.Collections.Generic;
using System.Collections;

namespace Punctuation {


	public class AStar{

		private Chunk origin, endgoal;

		private Dictionary<Chunk, int> fScore;		
		private Dictionary<Chunk, int> gScore;
        private Dictionary<Chunk, Chunk> cameFrom;
        private List<Chunk> closedSet, openSet;

		private Stack pathStack;
		
		
		public AStar(Goal goal, Chunk origin, Chunk endgoal) {

			this.origin = origin;
			this.endgoal = endgoal;
		
			this.fScore = new Dictionary<Chunk, int>();
			this.gScore = new Dictionary<Chunk, int>();
			this.cameFrom = new Dictionary<Chunk, Chunk>();
			this.closedSet = new List<Chunk>();
			this.openSet = new List<Chunk>();

			this.pathStack = this.calculate();
		}

		
		private Stack calculate() {

			for (int x = 0; x < Chunk.n; x++) {
				for (int y = 0; y < Chunk.n; y++) {

					Chunk chunk = Chunk.GetChunk(x,y);
					this.fScore[chunk] = int.MaxValue;
					this.gScore[chunk] = int.MaxValue;
				}
			}

			this.fScore[this.origin] = 0;
			this.gScore[this.origin] = 1;
			this.openSet.Add(this.origin);

			for (int count = 0; count < Chunk.n * Chunk.n; count++) {

				Chunk current = CheapestChunk();

				if (current == endgoal) {
					Punctuation.game.Debug("AStar took " + count + " steps to complete");
					break;
				}

				openSet.Remove(current);
				closedSet.Add(current);			
					
				foreach (Chunk neighbor in current.GetNeighbors(0)) {

					if (this.closedSet.Contains(neighbor))
						continue;
					
					if (!this.openSet.Contains(neighbor))
						this.openSet.Add(neighbor);

					int g = this.gScore[current] + current.Distance(neighbor) + neighbor.GetCost();

					if (g >= gScore[neighbor])
						continue;

					cameFrom[neighbor] = current;
					gScore[neighbor] = g;
					fScore[neighbor] = gScore[neighbor] + neighbor.Distance(this.endgoal);
					
				}
			}

			Chunk trace = this.endgoal;
            Stack p = new Stack();

            while(this.cameFrom.ContainsKey(trace)){
              
             	Punctuation.game.Debug("AStar Path = " + trace.ToString() + this.gScore[trace] + " | " + this.fScore[trace]);

			 	p.Push(trace);
                trace = this.cameFrom[trace];
            }

			return p;
		}	
		
	
		private Chunk CheapestChunk(){

			int cheapest_price = int.MaxValue;
			Chunk cheapest_chunk = null;
 
            foreach(Chunk chunk in this.openSet){   
                if(!closedSet.Contains(chunk)){
                    if(cheapest_price > this.fScore[chunk]){
                        cheapest_price = this.fScore[chunk];
                        cheapest_chunk = chunk;
                    }   
                }
            }
	
            return cheapest_chunk;
        }


		public Stack GetPathStack() {
			return this.pathStack;
		}

	}
}
