namespace Hydra {

    using Pirates;

	//This is an interface class, it builds the super type to all the tasks

	public enum TaskType {
		BERSERKER, ESCORT, MINER, MOLE, BOOSTER
	};


	public abstract class Task {


		public virtual int Bias() => 0;
		

        public virtual double GetWeight() => 0;


        public virtual double HeavyWeight() => 0;


        public virtual int Priority() => 0;


        public virtual string Preform() => "Nulp";


        public virtual void UpdatePirate(Pirate pirate) {}

	}

}
