using System.Collections.Generic;
using Pirates;

namespace Punctuation {
	
	public class TaskBerserker : Task  {

		public static Dictionary<int, Path> paths = new Dictionary<int, Path>();
		
		
		public string Preform(Pirate pirate) {

			
            return Utils.GetPirateStatus(pirate, "TODO");
		}
		
		
		public Pirate GetWeight(Pirate pirate) {
			
		}
		
		
	}
}
