using System.Collections.Generic;
using Pirates;

namespace Punctuation{

    public class Chunk{

        
        public const int size = 320;
        public const int n = 6400 / size; //Number of chunks
        
        public static Chunk[,] chunks = new Chunk[n, n]; //Keeps track of all the chunks
		
		
        public static Chunk GetChunk(Location loc){ //Returns a chunkk
            
            int y = loc.Row / size;
            int x = loc.Col / size;

            if(chunks[y, x] != null){
                
                return chunks[y, x];
            }
            
            return new Chunk(x, y);
        }


        public static Chunk GetChunk(int x, int y){ //Returns a chunk from an index

            if(chunks[y, x] != null){
                
                return chunks[y, x];
            }
            
            return new Chunk(x, y);
        }
    	 	
    	//---------------[ End of static functions and vars ]--------
    
        readonly int X, Y;
		readonly Dictionary<int, List<Chunk>> neighbors; //Keeps track of neighbors and removes the need to recalculate
       	
        protected Chunk(int X , int Y){ //This should only be used by GetChunk methods, never by us.
          
            this.Y = Y;          
            this.X = X;
			
			neighbors = new Dictionary<int,List<Chunk>>();
			
            chunks[Y, X] = this; //Registers the chunk
        }


		public int GX { get; set; } = X;

		public int GetX(){
        
            return X;
        }


        public int GetY(){

            return Y;
        }
        

         public Location GetLocation(){ //Gets the center of a chunk
          
            return new Location((Y * size) + (size / 2), (X * size) + (size / 2));
        }	


        public override string ToString(){ //Because it override, this does NOT need to be called when building a string
        
             return "[Y: " + this.GetY() + ", X:"  + this.GetX() + "]";
        }


        public int Distance(Chunk chunk){ 
         
            return System.Math.Abs(X - chunk.GetX()) + System.Math.Abs(Y - chunk.GetY());
        }
        

		public List<Pirate> GetEnemyPirates() { //TODO Gets every living pirate that is on the chunk, needs to be reworked

			var list = new List<Pirate>();
			
			foreach (Pirate enemy in Punctuation.game.GetEnemyLivingPirates()) {	
				if (enemy.Distance(GetLocation()) < size / 2) {

					list.Add(enemy);
				}
			}
			
			return list;
		}
		
		
        public List<Chunk> GetNeighbors(int level){ //The level is the distance of neighbors it should get
													//0 will return only adjacent

			if (neighbors.ContainsKey(level)){
				return neighbors[level];
			}

			var list = new List<Chunk>();
			
            for(int rx = (X - (level + 1)); rx <= (X + (level + 1)); rx++){
                
                for(int ry = (Y - (level + 1)); ry <= (Y + (level + 1)); ry++){
                    
                    if(rx >= 0 && ry >= 0 && rx < n && ry < n){
						
                        list.Add(Chunk.GetChunk(rx, ry));
                    }
                }
            }

			neighbors[level] = list;
			
            return list;
        }

    }

}

