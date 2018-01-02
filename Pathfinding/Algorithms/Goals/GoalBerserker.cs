using System.Collections.Generic;
using Pirates;

namespace Punctuation {
	
	public class GoalBerserker : Goal  {

		public static Dictionary<int, Path> paths = new Dictionary<int, Path>();
		
		
		public string Preform(Pirate pirate) {

			
            return Utils.GetPirateStatus(pirate, "TODO");
		}
		
		
		public int GetWeight(Pirate pirate) {
		
			return 0;		
		}
		
		
	}
}
