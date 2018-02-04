using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class TaskBerserker : Task {

        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------


        readonly Pirate pirate;


        public TaskBerserker(Pirate pirate) {
            this.pirate = pirate;
        }


        override public string Preform() {

            if (Main.didTurn.Contains(pirate.Id)) {
                return Utils.GetPirateStatus(pirate, "Already did turn");
            }

            if (Utils.PushAsteroid(pirate)) {
                return Utils.GetPirateStatus(pirate, "Pushed asteroid");
            }
            
            if (Utils.EnemyHoldersByDistance(pirate.Location).Count > 0) {

                Pirate enemyHolder = Utils.EnemyHoldersByDistance(pirate.Location).First();
                game.Debug("ID: " + enemyHolder.Id + " " + enemyHolder.Distance(pirate) + " -> " + game.PushRange);
                
                if (pirate.CanPush(enemyHolder)) {
                
                    var cloestEdge = Utils.CloestEdge(enemyHolder.Location);
                    double killCost = ((double) cloestEdge.Item1) / game.PushDistance;

                    game.Debug("KILLCOST: " + killCost);

                    var available = Utils.PiratesWithTask(TaskType.BERSERKER);
                    available.AddRange(Utils.PiratesWithTask(TaskType.MOLE));
                    available.RemoveAll(pirateAvailable => !pirateAvailable.CanPush(enemyHolder) || pirateAvailable.Id == pirate.Id || Main.didTurn.Contains(pirateAvailable.Id));
                    available.Insert(0, pirate);
              
                    if (available.Count >= 2) {
                        var pushLocation = new Location(game.Rows - enemyHolder.Location.Row, game.Cols - enemyHolder.Location.Col);

                        if (0.5 * killCost <= 1) {
                            pushLocation = cloestEdge.Item2;
                        }

                        foreach (Pirate berserker in available.Take(2)) {
                            Main.didTurn.Add(berserker.Id);
                            berserker.Push(enemyHolder, cloestEdge.Item2);
                        }

                        return Utils.GetPirateStatus(pirate, "Couple attacked holder");


                    } else if (killCost <= 1.26) /*add the movement of the enemy pirate to kill cost*/{

                        pirate.Push(enemyHolder, cloestEdge.Item2);
                        return Utils.GetPirateStatus(pirate, "Attacked holder");
                    }

                }

                pirate.Sail(Utils.SafeSail(pirate, enemyHolder.GetLocation()));
                return Utils.GetPirateStatus(pirate, "Moving towards enemy holder");
            }

            if (Main.enemyMines.Count > 0 && game.GetEnemyMotherships().Count() > 0) {

                var cloestEnemyMine = Utils.OrderByDistance(Main.enemyMines, pirate.Location).First();
                var cloestEnemyShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), pirate.Location).First();
                var sailLocation = cloestEnemyMine.Towards(cloestEnemyShip, game.PirateMaxSpeed + game.PushDistance);
                
                
                if (game.GetAllAsteroids().Count() > 0){
                    
                    var size = game.GetAllAsteroids()[0].Size;
                    List<Location> allastroidslocation = new List<Location>();
                    
                    foreach(Asteroid asteroid in game.GetAllAsteroids()){
                        allastroidslocation.Add(asteroid.InitialLocation);
                        if (asteroid.IsAlive())
                            allastroidslocation.Add(asteroid.GetLocation());
                    }
                    
                    foreach (Location loc in allastroidslocation)
                    {
                        if (sailLocation.InRange(loc, size))
                            sailLocation = loc.Towards(cloestEnemyShip, game.PirateMaxSpeed + game.PushDistance);
                    }
                    
                }
                
                
                pirate.Sail(Utils.SafeSail(pirate, sailLocation));
                return Utils.GetPirateStatus(pirate, "Moving towards enemy mine");
            }


            return Utils.GetPirateStatus(pirate, "Is idle.");
        }


        override public double GetWeight() {

            if (game.GetEnemyCapsules().Count() > 0) {

                var capsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First().GetLocation();

                if (Utils.EnemyHoldersByDistance(pirate.Location).Count > 0) {
                    capsule = Utils.EnemyHoldersByDistance(pirate.Location).First().Location;
                }

                double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(capsule));
                double weight = ((double)(maxDis - pirate.Distance(capsule)) / maxDis) * 100;

                if (double.IsNaN(weight)) {
                    return 0;
                }

                return weight;
            }

            return 0;
        }



        override public int Bias() {

            if (game.Cols < 3400) {
                return 10000;
            }

            if (Utils.PiratesWithTask(TaskType.BERSERKER).Count % 2 == 0) {
                return 45;
            }

            return 1;
        }




    }


}
