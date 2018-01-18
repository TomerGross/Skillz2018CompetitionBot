﻿using System.Collections.Generic;
using Pirates;
using Punctuation;

namespace Hydra {

	class TaskEscort : Task {

		public static Dictionary<int, Path> paths = new Dictionary<int, Path>();
		int radius = 1000; // Maximum range
	
		
		public string Preform(Pirate pirate) {

			if ((!paths.ContainsKey(pirate.UniqueId) || paths[pirate.UniqueId] == null)) { //Checks if path exists

				Chunk origin = Chunk.GetChunk(pirate.GetLocation()); //His starting position
				Chunk endgoal = new Chunk();

				if (Main.game.GetMyCapsule().Holder.Distance(pirate) >= radius) {
					endgoal = Chunk.GetChunk(Main.game.GetMyCapsule().Holder.GetLocation()); //His final goal
				} else {
					endgoal = Chunk.GetChunk(Main.game.GetMyMothership().GetLocation()); //His final goal
				}
				
				paths[pirate.UniqueId] = new Path(origin, endgoal, new List<Trait>(), Path.Algorithm.ASTAR); //Generate a path using AStar
			}

			int ID = pirate.UniqueId;
			Path path = paths[ID];

			if (path.GetChunks().Count > 0) {

				Chunk next = path.GetNext();

				if (next != null) {

					if (next.GetLocation().Distance(pirate) < (Chunk.divider / 2)) {
						path.GetChunks().Pop();
					}

					pirate.Sail(next.GetLocation());
					return Utils.GetPirateStatus(pirate,"Sailing to: " + next.ToString());
				}
			}
			return Utils.GetPirateStatus(pirate,"Next is null");
		}


		public int GetWeight(Pirate pirate) {

			if (!Main.game.GetMyCapsule().Holder == pirate) {
				List<MapObject> sortedlist = new List<MapObject>();
				sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetMyCapsule().Holder);
				int numofpirates = Main.game.GetAllMyPirates().Length;
				return (numofpirates - sortedlist.IndexOf(pirate)) * (100 / numofpirates);
			}

			return 0;
		}


		public int Bias() {
			if (!Main.game.GetMyCapsule().Holder())
				return 0;
			else
				return 5;
		}

	}

}
