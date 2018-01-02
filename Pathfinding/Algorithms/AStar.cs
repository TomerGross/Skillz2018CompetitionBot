using System.Collections.Generic;
using System.Collections;

namespace Punctuation {


	public class AStar{


		readonly Chunk origin, endgoal;
        readonly List<Trait> traits;
		readonly Stack pathStack;

		Dictionary<Chunk, int> fScore;		
		Dictionary<Chunk, int> gScore;
        Dictionary<Chunk, Chunk> cameFrom;
        List<Chunk> closedSet, openSet;


		public AStar(List<Trait> traits, Chunk origin, Chunk endgoal) {

			this.traits = traits;
			this.origin = origin;
			this.endgoal = endgoal;
		
			fScore = new Dictionary<Chunk, int>();
			gScore = new Dictionary<Chunk, int>();
			cameFrom = new Dictionary<Chunk, Chunk>();
			closedSet = new List<Chunk>();
			openSet = new List<Chunk>();
			
			pathStack = calculate();
		}

		
		protected Stack calculate() {

			for (int x = 0; x < Chunk.n; x++) {
				for (int y = 0; y < Chunk.n; y++) {

					Chunk chunk = Chunk.GetChunk(x,y);
					fScore[chunk] = int.MaxValue;
					gScore[chunk] = int.MaxValue;
				}
			}

			fScore[origin] = 0;
			gScore[origin] = 1;
			openSet.Add(origin);

			for (int count = 0; count < Chunk.n * Chunk.n; count++) {

				Chunk current = CheapestChunk();

				if (current == endgoal) {
					Punctuation.game.Debug("AStar took " + count + " steps to complete");
					break;
				}

				openSet.Remove(current);
				closedSet.Add(current);			
					
				foreach (Chunk neighbor in current.GetNeighbors(0)) {

					if (closedSet.Contains(neighbor)) {
						continue;
					}
					
					if (!openSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					}

					int g = gScore[current] + current.Distance(neighbor);

					foreach (Trait trait in traits) {
						g += trait.Cost(neighbor);
					}

					if (g >= gScore[neighbor]) {
						continue;
					}
					
					cameFrom[neighbor] = current;
					gScore[neighbor] = g;
					fScore[neighbor] = gScore[neighbor] + neighbor.Distance(endgoal);
					
				}
			}

			Chunk trace = endgoal;
            var p = new Stack();

            while(cameFrom.ContainsKey(trace)){
              
             	Punctuation.game.Debug("AStar Path = " + trace + gScore[trace] + " | " + fScore[trace]);

			 	p.Push(trace);
                trace = cameFrom[trace];
            }

			return p;
		}	
		
	
		protected Chunk CheapestChunk(){

			int cheapest_price = int.MaxValue;
			Chunk cheapest_chunk = null;
 
            foreach(Chunk chunk in openSet){   
                if(!closedSet.Contains(chunk)){
                    if(cheapest_price > fScore[chunk]){
                        cheapest_price = fScore[chunk];
                        cheapest_chunk = chunk;
                    }   
                }
            }
	
            return cheapest_chunk;
        }


		public Stack GetPathStack() {
			return pathStack;
		}

	}
}
