using Pirates;

namespace Hydra {

	//This is an interface class, it builds the super type to all the tasks

	public abstract class Task {

		//The task's bias
		public virtual int Bias() {
			return 0;
		}

		public virtual int GetWeight() {
			return 0;
		}
		
		public virtual string Preform() {
			return "Nulp";
		}

	}

}
