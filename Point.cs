using Pirates;

namespace Punctuation{
    
    public class Point{

        private int x, y;

        public Point(int x, int y){
            
            this.x = x;
            this.y = y;
        }
        
        public Point(Location loc){
            
            this.x = loc.Col;
            this.y = loc.Row;
        }


        public int GetX(){
            return this.x;
        }

        public int GetY(){
            return this.y;
        }

        public Location GetLocation(){
          return new Location(x, y);  
        }

    }

}