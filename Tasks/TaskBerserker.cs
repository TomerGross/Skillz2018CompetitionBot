using System.Collections.Generic;
using Pirates;

namespace Punctuation {
    //-------------------Globals---------------------------------------------
	    public List<Pirate> berserkerDidTurn = new List<Pirate>();
	//-----------------------------------------------------------------------
	public class TaskBerserker : Task  {

		public static Dictionary<int, Path> paths = new Dictionary<int, Path>();
		
		
		public string Preform(Pirate pirate) {
            if (berserkerDidTurn.Contains(pirate)){
                return ("|Pirate: "string.Parse(pirate.UniqueId())+"| Has already did his turn")
		}
			if (berserkerDidTurn.Contains(pirate)) {
				return "Berserker already did thier turn";
			}

			if (game.GetEnemyCapsule().Holder != null) {

				Pirate enemyHolder = game.GetEnemyCapsule().Holder;

				if (pirate.CanPush(enemyHolder)) {

					int c = 0;
					foreach (Pirate pir in game.GetMyLivingPirates()) {

						if (roles[pir.UniqueId] == Role.BERSERKER) {
							if (pir.CanPush(enemyHolder)) {
								c++;
							}
						}
					}

					if (c > 1) {
						foreach (Pirate pir in game.GetMyLivingPirates()) {

							if (roles[pir.UniqueId] == Role.BERSERKER) {
								if (pir.CanPush(enemyHolder)) {
									pir.Push(enemyHolder,enemyHolder.Location.Towards(game.GetEnemyMothership(),-5000));
									berserkerDidTurn.Add(pir);
								}
							}
						}
					}

					return "Berserker pushed enemy holder away.";

				} else {

					berserkerDidTurn.Add(pirate);
					pirate.Sail(enemyHolder);

					return "Berserker moving towards enemy holder...";
				}

			} else {

				berserkerDidTurn.Add(pirate);
				pirate.Sail(enemyMine.Towards(myMine,650));
				return "Berserker moved towards enemy mine.";
			}
		
		
	}
}
