namespace Hydra {
	
	public abstract class Trait {
		
		
		//Traits are cost calculations the pathfinding algorithm will use.
		//When calculating the path, a List of Traits will be given and the algorithm will add each one 
		//to the chunk's cost, the smaller the cost is the more the algorithm will tend to prefer it.
		//Because this class is abstract, each class that is inherited from it can be made with diffrent constructor 
		//methods and MUST (To make sense) ovverride Cost()
		
		
		public virtual int Cost(Chunk chunk) {
				
			return 0;
		}
		
	}
}
