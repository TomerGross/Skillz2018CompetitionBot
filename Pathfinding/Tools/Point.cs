using Pirates;

namespace Hydra {

	public class Point { //Suport class to Line, Will probably be removed in the near future

		readonly int x, y;

		public Point(int x,int y) {

			this.x = x;
			this.y = y;
		}

		public Point(Location loc) {

			x = loc.Col;
			y = loc.Row;
		}


		public int GetX() {
			return x;
		}

		public int GetY() {
			return y;
		}

		public Location GetLocation() {
			return new Location(x,y);
		}

	}

}