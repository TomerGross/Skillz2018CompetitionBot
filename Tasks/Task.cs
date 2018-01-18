using Pirates;

namespace Hydra {

	//This is an interface class, it builds the super type to all the tasks

	public interface Task {

		int Bias(); //The task's bias

		int GetWeight(Pirate pirate); //How much the task want a pirate

		string Preform(Pirate pirate); //Preform the task

	}

}
