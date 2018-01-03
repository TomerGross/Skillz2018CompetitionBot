using System.Collections.Generic;
using System.Collections;

namespace Punctuation {


	public class Dikjstra {

		//OLD CODE, NOT USED
		
    	private Dictionary<Chunk, int> dist;
        private Dictionary<Chunk, Chunk> prev;
        private List<Chunk> finished;
        private Chunk origin, endgoal;
        private	Stack pathStack;

		public Dikjstra(List<Trait> traits, Chunk origin, Chunk endgoal) {

			this.origin = origin;
			this.endgoal = endgoal;

			this.dist = new Dictionary<Chunk,int>();
			this.prev = new Dictionary<Chunk,Chunk>();
			this.finished = new List<Chunk>();

			this.pathStack = this.dijkstra();
		}

		private Chunk CheapestChunk() {

			int level = 0, possible = 0, cheapest_price = int.MaxValue;
			Chunk cheapest_chunk = null;

			while (possible == 0) {
				foreach (Chunk neighbor in this.origin.GetNeighbors(level)) {
					if (!this.finished.Contains(neighbor)) {
						possible++;
					}
				}

				if (possible == 0) {
					level++;
				}
			}

			List<Chunk> neighbors = this.origin.GetNeighbors(level);

			if (level > 0) {
				neighbors.RemoveAll(item => this.origin.GetNeighbors(level - 1).Contains(item));
			}

			foreach (Chunk neighbor in this.origin.GetNeighbors(level)) {
				if (!finished.Contains(neighbor)) {
					if (this.dist.ContainsKey(neighbor) && cheapest_price > this.dist[neighbor]) {
						cheapest_price = this.dist[neighbor];
						cheapest_chunk = neighbor;
					}
				}
			}

			if (this.finished.Count == 0) {
				return this.origin;
			}

			return cheapest_chunk;
		}


		private Stack dijkstra() {

			int chunk_count = Chunk.chunks.GetLength(0);
			Punctuation.game.Debug("COUNT: " + chunk_count);

			for (int x = 0; x < chunk_count; x++) {
				for (int y = 0; y < chunk_count; y++) {

					this.dist[Chunk.GetChunk(x,y)] = int.MaxValue;
				}
			}

			this.dist[this.origin] = 0;

			for (int count = 0; count < ((Chunk.chunks.GetLength(0) * Chunk.chunks.GetLength(1)) - 1); count++) {

				Chunk position = CheapestChunk();
				finished.Add(position);

				if (position == this.endgoal) {
					Punctuation.game.Debug("Path has been found! " + position.ToString());
					break;
				}

				foreach (Chunk neighbor in position.GetNeighbors(0)) {

					int cost = this.dist[position]; // + NEIGHBORS COST

					if (cost < this.dist[neighbor]) {

						this.dist[neighbor] = cost;
						this.prev[neighbor] = position;
					} else {

					}
				}
			}

			foreach (KeyValuePair<Chunk,int> kvp in this.dist) {
				Punctuation.game.Debug("Key = " + kvp.Key + " Value =  " + kvp.Value + " " + this.finished.Contains(kvp.Key));
			}

			Chunk t = this.endgoal;
			Stack p = new Stack();

			while (this.prev.ContainsKey(t)) {
				p.Push(t);
				t = this.prev[t];
				if (t != null) {
					Punctuation.game.Debug("Path = " + t.ToString() + this.dist[t]);
				}
			}

			return p;
		}


		public Stack GetPathStack() {
			return this.pathStack;
		}

	}
}
