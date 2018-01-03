using System.Collections.Generic;
using Pirates;

namespace Punctuation{

    public class Line {
        	
        readonly Point point1, point2;
			
	
        public Line (Point point1, Point point2){
           
            this.point1 = point1;
            this.point2 = point2;
        }
        
        
        public Line (Location loc1, Location loc2){
           
            this.point1 = new Point(loc1);
            this.point2 =  new Point(loc2);
        }
        
	
        public Point[] GetPointsBetween(int count){

            Point[] points = new Point[count];

            for (int i = 0; i < count; i++) {
                
                double distance = this.GetLength() / (i + 1); 
                
                int x = (int) System.Math.Round(this.point1.GetX() + distance);
                int y = (int) System.Math.Round((x * GetSlope()) - (this.point1.GetX() * GetSlope()) - this.point1.GetY());
                
                points[i] = new Point(x, y);
            }

            return points;
        }
        
       	
        public Point[] GetPoints(int range){

            Point[] points = new Point[range];
			double distance = Punctuation.game.PirateMaxSpeed;
			
            for (int i = 0; i < range; i++) {
                                
           		var x = (int) System.Math.Round(this.point1.GetX() + distance);
                var y = (int) System.Math.Round((x * GetSlope()) - (this.point1.GetX() * GetSlope()) - this.point1.GetY());
                
                points[i] = new Point(x, y);
            }
			
            return points;
        }


        public double GetLength(){
            return System.Math.Sqrt(System.Math.Pow(point1.GetY() - point2.GetY(), 2) + System.Math.Pow(point1.GetX() - point2.GetX(), 2));
        }


        public double GetSlope(){
            return (point1.GetY() - point2.GetY()) / (point1.GetX() - point2.GetX());
        }

    }

}