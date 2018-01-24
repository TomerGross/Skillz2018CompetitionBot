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
		public static List<Task> todoTasks = new List<Task>();
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
			todoTasks = chooseTasks();

			// do the tasks
			giveTasks();
			

		}


		public void giveTasks() {

			var costs = new Dictionary<int, Tuple<Pirate,Task>>();

			foreach (Pirate pirate in unemployedPirates) {
				foreach (Task task in todoTasks) {
					costs[task.Bias() + task.GetWeight(pirate)] = new Tuple<Pirate,Task>(pirate,task);
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


		public List<Task> chooseTasks() {
			var remainTasks = game.GetMyLivingPirates().Length;
			
			
			List<Task> tasksTodo = new List<Task>();

			tasksTodo.Add(new TaskMiner(game.GetMyLivingPirates()[0]));
			remainTasks--;

			while (remainTasks > 0) {

				if (!tasksTodo.Contains(TaskEscort)) {
					tasksTodo.Add(new TaskEscort());
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
	}
}
