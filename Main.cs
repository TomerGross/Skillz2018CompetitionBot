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
		public static Dictionary<Pirate,Task> tasks = new Dictionary<Pirate,Task>();
		public static List<Pirate> unemployedPirates = new List<Pirate>();
		public static List<TaskType> todoTasks = new List<TaskType>();
		//--------------------------------------------


		public void DoTurn(PirateGame game) {


			Main.game = game;

			unemployedPirates = game.GetMyLivingPirates().ToList();
			
			if (game.GetMyCapsule().Holder == null) {
				mine = game.GetMyCapsule().GetLocation();
			}

			if (game.GetEnemyCapsule().Holder == null) {
				mineEnemy = game.GetEnemyCapsule().GetLocation();
			}

			// choose which task to do
			//todoTasks = chooseTasks();

			// do the tasks
			giveTasks();
			

		}


		public void giveTasks() {

			var costs = new Dictionary<int, Tuple<Pirate,Task>>();

			foreach (Pirate pirate in unemployedPirates) {
				foreach (TaskType taskType in todoTasks) {
					Task task = taskTypeToTask(taskType, pirate);
					costs[task.Bias() + task.GetWeight()] = new Tuple<Pirate,Task>(pirate,task);
				}
			}

			var sorted = costs.Keys.ToList();
			sorted.Sort();

			foreach (var key in sorted) {

				if (!tasks.ContainsKey(costs[key].Item1)) {
					tasks[costs[key].Item1] = costs[key].Item2;
				}

				costs.Remove(key);
			}
		}


		public Task taskTypeToTask(TaskType task, Pirate pirate) {

			switch (task){
				case TaskType.BERSERKER:
					return new TaskBerserker(pirate);
					
				case TaskType.ESCORT:
					return new TaskEscort(pirate);
					
				case TaskType.MOLE:
					return new TaskMole(pirate);
					
				default:
					return new TaskMiner(pirate);
			}
		}
		
		/*
		public List<Task> chooseTasks() {
			var remainTasks = game.GetMyLivingPirates().Length;
			
			
			List<Task> tasksTodo = new List<Task>();

			tasksTodo.Add(new TaskMiner(game.GetMyLivingPirates()[0]));
			remainTasks--;

			while (remainTasks > 0) {

				if (!tasksTodo.Contains(TaskType.ESCORT)) {
					tasksTodo.Add(TaskType.ESCORT));
					remainTasks--;
					continue;
				}
				if (!tasksTodo.Contains(TaskMole)) {
					tasksTodo.Add(new TaskMole());
					remainTasks--;
					continue;
				}

				if (!tasksTodo.Contains(TaskBerserker)) {
					tasksTodo.Add(new TaskBerserker());
					remainTasks--;
					continue;
				}

				tasksTodo.Add(new TaskEscort());
				remainTasks--;

			}

			return tasksTodo;

		}
		*/
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
