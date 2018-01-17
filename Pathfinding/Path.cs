using System.Collections;
using System.Collections.Generic;


namespace Punctuation {

	public class Path {

		public enum Algorithm {
			ASTAR
		}
		
		readonly Chunk origin, endgoal;
		readonly List<Trait> traits;
		
		Stack chunks, chunks_previous;
		
		
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
		
		
		public Chunk Pop() {

			var popped = (Chunk) chunks_previous.Pop();
			chunks_previous.Push(popped);

			return popped;
		}


		public Chunk GetNext() {

			if (chunks.Count > 2) {

				var from = (Chunk) chunks.Pop();
				var skipped = Pop();
				var jumpto = Pop();

				var recalculated = new Path(from,jumpto,traits,Algorithm.ASTAR);

				if (recalculated.GetChunks().Count == 1 && recalculated.GetChunks().Peek() == skipped) {

					Punctuation.game.Debug("No better path could be found");

					chunks.Push(jumpto);
					chunks.Push(skipped);
					chunks.Push(from);
				}

				var reversed = new Stack();

				foreach (var c in recalculated.GetChunks()) {
					reversed.Push(c);
				}

				foreach (var rechunk in reversed) {
					chunks.Push(rechunk);
				}

				Punctuation.game.Debug("Path found and recalculted");
			}

			return (Chunk) chunks.Peek();
		}


		public Stack GetChunks() {
			return chunks;
		}


		public Stack GetPreviousChunks() {
			return chunks_previous;
		}


		public Chunk GetEndGoal() {
			return endgoal;
		}


		public override string ToString() {

			string build = "";

			foreach (Chunk chunk in chunks.ToArray()) {
				build += chunk + " -> ";
			}

			return build;
		}


		public void SetChunks(Stack chunks) {
			this.chunks = chunks;
		}


		public void AddChunk(Chunk chunk) {
			chunks.Push(chunk);
		}

	}


}