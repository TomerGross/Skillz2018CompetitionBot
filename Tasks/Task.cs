using Pirates;

namespace Punctuation {

	
	//This is an interface class, it builds the super type to all the tasks
	
	public interface Task {
		
		int Bias(); //The task's bias

		int GetWeight(Pirate pirate); //The per task cost of chunks

		string Preform(Pirate pirate); //Preform the task
		
	}
}
