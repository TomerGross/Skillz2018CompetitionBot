
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace Punctuation {

	public class Punctuation : IPirateBot {

		public static PirateGame game;//Saving game

		public enum Role { MINER, MOLE, GUARDIAN, BERSERKER, GOALER }; //Creating "Role" Enum containing all pirate States



		



		











		//---------------Dictionarys------------------
		public static Dictionary<int, Role> roles;
		public static Dictionary<int, Goal> goals;
		//--------------------------------------------

		//---------------Globals to hold Mines--------
		public static Location myMine;
		public static Location enemyMine;
		//--------------------------------------------

		//---------------Globals To Avoid Caos--------------------------
		public List<Pirate> berserkerDidTurn = new List<Pirate>();
		public List<Pirate> boosted = new List<Pirate>();
		public Location oldLocation = new Location(0,0);
		//--------------------------------------------------------------

		//---------------constants-------------------
		public int beginingMiners = 2;
		public bool reachedPy = false;
		public int guardianDistanceFromMine = 1200;
		//-------------------------------------------

		public void DoTurn(PirateGame game) {

			Punctuation.game = game;
			berserkerDidTurn.Clear();
			boosted.Clear();

			if (game.GetMyCapsule().Holder == null) {

				reachedPy = false;
				myMine = game.GetMyCapsule().GetLocation();
			}

			if (game.GetEnemyCapsule().Holder == null) {

				enemyMine = game.GetEnemyCapsule().GetLocation();
			}


			if (game.Turn == 1) {

				goals = new Dictionary<int, Goal>();

				//TODO MAKE GENERIC SO DOESNT CRASH IF LESS THEN 2 PIRATES 
				roles = new Dictionary<int,Role>(){
				 {game.GetAllMyPirates()[0].UniqueId, Role.MOLE},
				 {game.GetAllMyPirates()[1].UniqueId, Role.MOLE},
				 {game.GetAllMyPirates()[2].UniqueId, Role.GUARDIAN},
				 {game.GetAllMyPirates()[3].UniqueId, Role.GUARDIAN},
				 {game.GetAllMyPirates()[4].UniqueId, Role.MINER},
				 {game.GetAllMyPirates()[5].UniqueId, Role.MINER},
				 {game.GetAllMyPirates()[6].UniqueId, Role.BERSERKER},
				 {game.GetAllMyPirates()[7].UniqueId, Role.BERSERKER}
				};
			}

            goals[game.GetAllMyPirates()[7].UniqueId] = new GoalOLD(game.GetAllMyPirates()[7].GetLocation(), new Location(2000, 200));
			foreach (Pirate pirate in game.GetMyLivingPirates()) {

				if (roles.ContainsKey(pirate.UniqueId)) {
					if (roles[pirate.UniqueId] == Role.MOLE) {
				        game.Debug(RoleMole(pirate));
					} else if (roles[pirate.UniqueId] == Role.BERSERKER) {
					    game.Debug(RoleWar(pirate,game));
					} else if (roles[pirate.UniqueId] == Role.GUARDIAN) {
					    game.Debug(RoleGuardian(pirate,game));
					} else {
					    game.Debug(RoleMine(pirate,game));
					}
				}
			}

			foreach (Pirate pirate in game.GetMyLivingPirates()) {

				if (pirate.HasCapsule()) {

					oldLocation = pirate.GetLocation();
				}
			}
		}

		public string RoleGetCapsuleToBase(Pirate pirate,PirateGame game) {

			Location loc = new Location(game.GetMyMothership().GetLocation().Row,myMine.Col);

			if (reachedPy) {
				pirate.Sail(game.GetMyMothership());
				return Utils.GetPirateStatus(pirate,"Sailing from PY to mothership...");
			} else {
				if (pirate.GetLocation().Distance(loc) < 50) {
					reachedPy = true;
					return Utils.GetPirateStatus(pirate,"Reached PY.");
				}

				pirate.Sail(loc);
				return Utils.GetPirateStatus(pirate,"Sailing to PY...");
			}
		}


		public string RoleWar(Pirate pirate,PirateGame game) {

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


		public string RoleMole(Pirate pirate) {

			Location location = game.GetEnemyMothership().GetLocation().Towards(enemyMine,650);

			if (game.GetEnemyCapsule().Holder != null) {
				location = game.GetEnemyMothership().GetLocation().Towards(game.GetEnemyCapsule(),650);
			}

			if (pirate.Distance(location) < 10) {

				Pirate closest = Utils.GetClosestCapsuleEnemyPirate(pirate,game);

				if (closest.HasCapsule() && pirate.Distance(closest) <= 300) {
					if (pirate.CanPush(closest)) {
						pirate.Push(closest,Utils.CanPushOutBeta(closest.GetLocation(),game));
						return Utils.GetPirateStatus(pirate,"pushed enemy");
					}
				}

			} else {

				pirate.Sail(location);
				return Utils.GetPirateStatus(pirate,"sailing towards objective");
			}

			return Utils.GetPirateStatus(pirate,"isn't doing anything");
		}


		public string RoleMine(Pirate pirate,PirateGame game) {// return to comment later

			if (Utils.GetMinersAlive(game) > beginingMiners) {    //Turns "MINER" into "GUARDIAN" if there are more miners than
				roles[pirate.UniqueId] = Role.GUARDIAN;         // the "beginingMiners" Constant
				game.Debug(RoleGuardian(pirate,game));
				return Utils.GetPirateStatus(pirate," role switched to GUARDIAN");
			}

			if (pirate.HasCapsule()) {
			
				pirate.Sail(game.GetMyMothership());
				
				return Utils.GetPirateStatus(pirate,"Working to reach base");

			} else {

				if (pirate.Distance(myMine) < 200 && Utils.TryPush(pirate,game)) {

					return "Pushed";

				} else {

					pirate.Sail(myMine);
					return "Sailing towards mine";
				}
			}
		}


		public string RoleGuardian(Pirate pirate,PirateGame game) {     //The goal of the Guardian is to protect and help the miners by using the push to either boost or kill as body guards 

			if (Utils.GetMinersAlive(game) == 0) {        //If there are no "MINER" a "GUARDIAN" becomes a "MINER"

				roles[pirate.UniqueId] = Role.MINER;
				game.Debug(RoleMine(pirate,game));
				return Utils.GetPirateStatus(pirate,"Role switched to MINER");
			}

			if (game.GetMyCapsule().Holder == null) {     //If there is no "MINER" holding a Capsule the "GUARDIAN" will go the space between or mine and an enemy mine Of guardianDistanceFromMine

				pirate.Sail(myMine.Towards(enemyMine,guardianDistanceFromMine));
				return Utils.GetPirateStatus(pirate,"Sailing towards mine");
			}

			Pirate holder = game.GetMyCapsule().Holder;

			game.Debug("DISTANCE: " + holder.Distance(game.GetMyMothership()).ToString() + "   PUSH " + game.PushDistance.ToString());
			if (!boosted.Contains(holder) && pirate.CanPush(holder) && holder.Distance(game.GetMyMothership()) <= (game.PushDistance + holder.MaxSpeed)) {

				pirate.Push(holder,game.GetMyMothership());
				boosted.Add(holder);

				return Utils.GetPirateStatus(pirate,"Boosted holder directly to base");

			} else {

				if (pirate.Distance(holder) > game.PushDistance * 2.5 && Utils.TryPush(pirate,game)) {

					return Utils.GetPirateStatus(pirate,"Protected holder");

				} else {

					pirate.Sail(holder);
					return Utils.GetPirateStatus(pirate,"Sailing towards holder");
				}
			}
		}

	}

	}

