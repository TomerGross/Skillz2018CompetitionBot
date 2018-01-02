using System.Collections.Generic;
using Pirates;


namespace Punctuation{

    public class Chunk{
        
        const int divider = 320;
        const int n = 6400 / divider;
        
        public static Chunk[,] chunks = new Chunk[n, n];
		
		
        public static Chunk GetChunk(Location loc){
            
            int y = loc.Row / divider;
            int x = loc.Col / divider;

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

    
        private int x, y;
        
        private Chunk(int x , int y){
          
            this.y = y;          
            this.x = x;

            chunks[y ,x] = this; 
        }   


        public int GetX(){
            return this.x;
        }

        public int GetY(){
            return this.y;
        }

        public int GetNumber(){
            return (this.y * n) + (this.x + 1);
        }

         public Location GetLocation(){
            return new Location((this.y*divider)+(divider/2),(this.x*divider)+(divider/2));
        }	

        public override string ToString(){
             return "[Y: " + this.GetY().ToString() + ", X:"  + this.GetX().ToString() + "]";
        }

        public int Distance(Chunk chunk){
            return System.Math.Abs(this.x - chunk.GetX()) + System.Math.Abs(this.y - chunk.GetY());
        }

        public List<Chunk> GetNeighbors(int level){

            List<Chunk> neighbors = new List<Chunk>();

            for(int rx = (this.x - (level + 1)); rx <= (this.x + (level + 1)); rx++){
                
                for(int ry = (this.y - (level + 1)); ry <= (this.y + (level + 1)); ry++){
                    
                    if(rx >= 0 && ry >= 0 && rx < n && ry < n){

                        neighbors.Add(Chunk.GetChunk(rx, ry));
                    }
                }
            }

            return neighbors;
        }

    }

}

