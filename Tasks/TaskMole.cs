using Pirates;
using System.Linq;

namespace Hydra {

	public class TaskMole : Task{


		readonly Pirate pirate;


		public TaskMole(Pirate pirate) {
			this.pirate = pirate;
		}
		
		
		override public string Preform() {

			PirateGame game = Main.game;

			int radius = pirate.MaxSpeed + game.PushDistance;
			
			if (game.GetEnemyCapsules()[0].Holder != null) {

				Pirate enemyHolder = game.GetEnemyCapsules()[0].Holder;
				//Location towardsEnemy = game.GetEnemyMothership().GetLocation().Towards(game.GetEnemyCapsule(),radius);

                // If the pirate is in position and can attack
                if (pirate.CanPush(enemyHolder)) {

                    var cloestEdge = Utils.CloestEdge(enemyHolder.GetLocation());

                    if (cloestEdge.Item1 <= game.PushDistance) {
                        pirate.Push(enemyHolder, cloestEdge.Item2);
                    } else {
                        pirate.Push(enemyHolder, Main.enemyMines[0]);
                    }

                    return Utils.GetPirateStatus(pirate, "Pushed enemy holder");


                } else {
                    pirate.Sail(game.GetEnemyMotherships()[0].GetLocation().Towards(game.GetEnemyCapsules()[0].Holder, radius));
					
					return Utils.GetPirateStatus(pirate, "Is sailing to position");
					/*if(pirate.Distance(game.GetEnemyMothership().GetLocation().Towards(Main.mineEnemy, radius)) < 500){
					// Sail to a position
					pirate.Sail(towardsEnemy);
					return "Pirate is sailing to position";
					}*/
				}
			}

            pirate.Sail(game.GetEnemyMotherships()[0].GetLocation().Towards(Main.enemyMines[0], radius));
			return Utils.GetPirateStatus(pirate, "Is sailing to position");
		}


	
        override public double GetWeight(){
            
            if(Utils.PiratesWithTask(TaskType.MOLE).Count >= 1){
                return 0;
            }
            
            Location holder = Main.game.GetEnemyMotherships()[0].Location;
            
            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(holder));

            double weight = ((double)(maxDis - pirate.Distance(holder)) / maxDis) * 100;
            Main.game.Debug("ESCORT WEIGHT: " + weight);

            return weight;
        }


		override public int Bias() {
		    
		    if(Utils.PiratesWithTask(TaskType.MOLE).Count >= 1){
                return 0;
            }
            
			return 80;
		}
		
	}
}
