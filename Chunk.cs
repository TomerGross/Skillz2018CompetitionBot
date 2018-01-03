using System.Collections.Generic;
using Pirates;


namespace Punctuation{

    public class Chunk{
        
        public const int size = 320;
        public const int n = 6400 / size;
        
        public static Chunk[,] chunks = new Chunk[n, n];
		
		
        public static Chunk GetChunk(Location loc){
            
            int y = loc.Row / size;
            int x = loc.Col / size;

            if(chunks[y, x] != null){
                
                return chunks[y, x];
            }
            
            return new Chunk(x, y);
        }


        public static Chunk GetChunk(int x, int y){

            if(chunks[y, x] != null){
                
                return chunks[y, x];
            }
            
            return new Chunk(x, y);
        }
    	 	
    	//---------------[ End of static functions and vars ]--------
    
        readonly int X, Y;
        
       	
        protected Chunk(int X , int Y){
          
            this.Y = Y;          
            this.X = X;

            chunks[Y, X] = this; 
        }   


        public int GetX(){
        
            return X;
        }


        public int GetY(){

            return Y;
        }
        

         public Location GetLocation(){
         
            return new Location((Y * size) + (size / 2), (X * size) + (size / 2));
        }	


        public override string ToString(){
        
             return "[Y: " + this.GetY() + ", X:"  + this.GetX() + "]";
        }


        public int Distance(Chunk chunk){
        
            return System.Math.Abs(X - chunk.GetX()) + System.Math.Abs(Y - chunk.GetY());
        }
        

		public List<Pirate> GetEnemyPirates() {

			var list = new List<Pirate>();
			
			foreach (Pirate enemy in Punctuation.game.GetEnemyLivingPirates()) {	
				if (enemy.Distance(GetLocation()) < size / 2) {

					list.Add(enemy);
				}
			}
			
			return list;
		}
		
		
        public List<Chunk> GetNeighbors(int level){

            var neighbors = new List<Chunk>();

            for(int rx = (X - (level + 1)); rx <= (X + (level + 1)); rx++){
                
                for(int ry = (Y - (level + 1)); ry <= (Y + (level + 1)); ry++){
                    
                    if(rx >= 0 && ry >= 0 && rx < n && ry < n){
						
                        neighbors.Add(Chunk.GetChunk(rx, ry));
                    }
                }
            }

            return neighbors;
        }

    }

}

