using Pirates;
using System.Linq;
using System.Collections.Generic;

namespace Hydra {
	

    public class Status{

        public readonly Location loc;
        public readonly bool alive;
        public readonly int pushReloadTurns;
        public readonly int turn;
        public readonly int id;

        public Status(Pirate pirate){

            loc = pirate.Location;
            alive = pirate.IsAlive();
            pushReloadTurns = pirate.PushReloadTurns;
            turn = Main.game.Turn;
            id = pirate.Id;
        }
    }


	public class Predict {


        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        public static readonly List<int> enemiesPossiblyPushed = new List<int>();
        static readonly Dictionary<int, Dictionary<int, Status>> myLog = new Dictionary<int, Dictionary<int, Status>>();
        static readonly Dictionary<int, Dictionary<int, Status>> enemyLog = new Dictionary<int, Dictionary<int, Status>>();
        //-----------------------------------------------------------------------
        		

        public static void Log() {

            enemiesPossiblyPushed.Clear();

            game.GetAllMyPirates().ToList().ForEach(Register);
            game.GetAllEnemyPirates().ToList().ForEach(Register);
        }
		

        public static void Register(Pirate pirate){

            var log = myLog;

            if(pirate.Owner != game.GetMyself()){
                log = enemyLog;
            }
            
            if(!log.ContainsKey(game.Turn)){
                log.Add(game.Turn, new Dictionary<int, Status>());
            }

            log[game.Turn].Add(pirate.Id, new Status(pirate));

            if (log.ContainsKey(game.Turn - 1) && log[game.Turn - 1].ContainsKey(pirate.Id)){

                var pstatus = log[game.Turn - 1][pirate.Id];

                if (pirate.Owner == game.GetEnemy()) {
                    if (pstatus.loc.Distance(pirate) > pirate.MaxSpeed) {

                        var possiblePushers = (from KeyValuePair<int, Status> pair in log[game.Turn - 1] select pair.Value).ToList();
                        possiblePushers.RemoveAll(status => status.id == pirate.Id);
                        possiblePushers.RemoveAll(status => status.loc.Distance(pstatus.loc) > game.PushRange);
                        possiblePushers.RemoveAll(status => EnemyPirateById(status.id).PushReloadTurns == 0);

                        if(possiblePushers.Count() > 0){
                            game.Debug("Pirate " + pirate.Id + " might have been pushed!");
                            enemiesPossiblyPushed.Add(pirate.Id);
                        }
                    }

                }

            }
        }

           
        public static Pirate EnemyPirateById(int Id){
            return game.GetAllEnemyPirates().ToList().Find(pirate => pirate.Id == Id);
        }
        
        /*
		public void AppendPossibleTargets(Pirate pirate, Pirate target, int a) {

			if (!possibleTargets.ContainsKey(pirate.UniqueId)) {
				
				possibleTargets.Add(pirate.UniqueId, new Dictionary<int, int>());
			}

			if (!possibleTargets[pirate.UniqueId].ContainsKey(target.UniqueId)) {

				possibleTargets[pirate.UniqueId].Add(target.UniqueId, 1);
				
				return;
			}
			
			possibleTargets[pirate.UniqueId][target.UniqueId] += a;
		}
		
	

		public int GetNextChunks(Pirate pirate) {
			
			List<Location> list = log[pirate.UniqueId];
			
			if (list.Count < 2) {
			
				return -999;
			}		
			
			double m = (list[list.Count - 1].Row - list[list.Count - 2].Row) / (list[list.Count - 1].Col - list[list.Count - 2].Col);
			
			return 0;
		}
		
		  */
	}

}
