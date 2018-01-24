using Pirates;

namespace Hydra {

	//This is an interface class, it builds the super type to all the tasks

	public interface Task {

		public static int Bias(); //The task's bias


		static int GetWeight(Pirate pirate); //How much the task want a pirate

		public static string Preform(Pirate pirate); //Preform the task

	}

}
