using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

	public class Main : IPirateBot {


		//---------------[ Main variables ]-----------
		public static PirateGame game;
		//--------------------------------------------


		//---------------[ Mines ]--------------------
		public static Location mine;
		public static Location mineEnemy;
		//--------------------------------------------


		//---------------[ Task mangment ]-------------
		public static Dictionary<int, TaskType> tasks = new Dictionary<int, TaskType>();
		public static List<Pirate> unemployedPirates = new List<Pirate>();
		public static List<TaskType> todoTasks = new List<TaskType>(new List<TaskType> 
		{ TaskType.MINER, TaskType.ESCORT/*, TaskType.MOLE*/, TaskType.BERSERKER, TaskType.DEFAULT});
		public static int alivePirateCount = 0;
		//--------------------------------------------


		public void DoTurn(PirateGame game) {


			Main.game = game;
			alivePirateCount = game.GetMyLivingPirates().Count();
			
			if (game.GetMyCapsule().Holder == null) {
				mine = game.GetMyCapsule().GetLocation();
			}

			if (game.GetEnemyCapsule().Holder == null) {
				mineEnemy = game.GetEnemyCapsule().GetLocation();
			}

			
			// do the tasks
			unemployedPirates = game.GetMyLivingPirates().ToList();
			tasks.Clear();
			giveTasks();

			foreach (KeyValuePair<int, TaskType> pair in tasks){
				game.Debug(taskTypeToTask(game.GetMyPirateById(pair.Key), pair.Value).Preform());
				
				
			}
		}


		public int piratesWithTask(TaskType type) {

			int sum = 0;
			
			foreach (KeyValuePair<int, TaskType> pair in tasks) {
				if (pair.Value == type) {
					sum += 1;
				} 	
			}

			return 0;
		}


		public Dictionary<double, Tuple<Pirate, TaskType>> getCurrentCosts() {
			
			var costs = new Dictionary<double, Tuple<Pirate, TaskType>>();

			foreach (Pirate pirate in unemployedPirates) {

				foreach (TaskType taskType in todoTasks) {
					Task task = taskTypeToTask(pirate, taskType);
					
					double cost = task.Bias() + task.GetWeight();
					cost *= (1 - ((double) piratesWithTask(taskType) / game.GetMyLivingPirates().Count()));

					if (piratesWithTask(taskType) > 4){
						cost *= 50;
					}
					
					costs[cost] = new Tuple<Pirate, TaskType>(pirate, taskType);
					//game.Debug(pirate.Id + " | " + taskType + " | " + cost);
				}
			}

			return costs;
		}
		

		public void giveTasks() {

			for (int i = 0; i < alivePirateCount; i++) {

				var costs = getCurrentCosts();

				var sorted = costs.Keys.ToList();
				sorted.Sort();
				sorted.Reverse();
				
				foreach (var key in sorted) {

					Pirate pirate = costs[key].Item1;
					TaskType taskType = costs[key].Item2;
				
					if (!tasks.ContainsKey(pirate.Id)) {
					    
						game.Debug("Gave: " + pirate.Id + " | " + taskType + " at cost: " + key);
						
						tasks[pirate.Id] = taskType;
						unemployedPirates.Remove(pirate);
						break;
					}
				}
			}
		}
		

		public Task taskTypeToTask(Pirate pirate, TaskType task) {

			switch (task){
				case TaskType.BERSERKER:
					return new TaskBerserker(pirate);
					
				case TaskType.ESCORT:
					return new TaskEscort(pirate);
					
				case TaskType.MOLE:
					return new TaskMole(pirate);
				
				case TaskType.DEFAULT:
				    return new TaskDefault(pirate);
					
				default:
					return new TaskMiner(pirate);
			}
		}
		
	}

    
	public class Tuple<T1, T2> {

		readonly T1 m_Item1;
		readonly T2 m_Item2;

		public T1 Item1 {
			get {
				return m_Item1;
			}
		}
		public T2 Item2 {
			get {
				return m_Item2;
			}
		}

		public Tuple(T1 item1,T2 item2) {
			m_Item1 = item1;
			m_Item2 = item2;
		}

		
		object this[int index] {
			get {
				switch (index) {
					case 0:
						return Item1;
					case 1:
						return Item2;
					default:
						return null;
				}
			}
		}
	}
	
	
	
}
