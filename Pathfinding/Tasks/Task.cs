using Pirates;

namespace Punctuation {

	
	//This is an interface class, it builds the super type to all the goals
	
	public interface Task {
		
		int GetWeight(Pirate pirate); //The per goal cost of chunks

		string Preform(Pirate pirate); //Preform the goal
			
	}
}
