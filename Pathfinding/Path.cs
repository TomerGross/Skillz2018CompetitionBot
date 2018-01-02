using System.Collections;
using Pirates;


namespace Punctuation{
        
    public class Path{


		public enum Algorithm {
			ASTAR
		}


		private Stack chunks, chunks_previous;
		private Chunk origin, endgoal;


		public Path(Chunk origin, Chunk endgoal, Algorithm algorithm){

			this.origin = origin;
			this.endgoal = endgoal;
			
			this.chunks_previous = new Stack();

			if (algorithm == Algorithm.ASTAR){
				this.chunks = new AStar(null,this.origin,this.endgoal).GetPathStack();
			} else {
				this.chunks = new Stack();
			}
        }


		public Chunk Pop() {

			var popped = (Chunk) this.chunks_previous.Pop();
			chunks_previous.Push(popped);

			return popped;
		}
		
		
		public Chunk GetNext(){

			if (chunks.Count > 2) {

				var from = (Chunk) chunks.Pop();
				var skipped = this.Pop();
				var jumpto = this.Pop();

				var recalculated = new Path(from, jumpto, Algorithm.ASTAR);

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


        public Stack GetChunks(){
            return this.chunks;
        }


		public Stack GetPreviousChunks() {
			return this.chunks_previous;
		}
		

		public Chunk GetEndGoal() {
			return this.endgoal;
		}
		
		
        public override string ToString(){

            string build = "";
            
            foreach(Chunk chunk in chunks.ToArray()){
                build += chunk.ToString() + " -> ";
            }  

            return build;
        }
        
		
        public void SetChunks(Stack chunks){
            this.chunks = chunks;
        }


		public void AddChunk(Chunk chunk) {
			chunks.Push(chunk);
		}
        
    }
    
	 
}