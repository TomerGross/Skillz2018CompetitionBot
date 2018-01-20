using System;
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
		public static Dictionary<Pirate,Task> tasks;
		public static List<Pirate> unemployedPirates;
		public static List<Task> todoTaks;
		//--------------------------------------------

		public void DoTurn(PirateGame game) {

			Main.game = game
			
			if (game.GetMyCapsule().Holder == null) {
				mine = game.GetMyCapsule().GetLocation();
			}

			if (game.GetEnemyCapsule().Holder == null) {
				mineEnemy = game.GetEnemyCapsule().GetLocation();
			}
			
			// Initiates all the tasks
			giveTaks();
			
		}

		public void giveTaks() {


			var costs = new Dictionary<int,Tuple<Pirate,Task>>();

			foreach (Pirate pirate in unemployedPirates) {
				foreach (Task task in todoTaks) {
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

	}
