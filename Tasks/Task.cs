using Pirates;

namespace Hydra {

	//This is an interface class, it builds the super type to all the tasks

	public interface Task {
		
		int Bias(); //The task's bias

		int GetWeight(); //How much the task want a pirate

		string Preform(); //Preform the task

	}

}
