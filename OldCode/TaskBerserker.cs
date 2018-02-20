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

            if (Utils.EnemyHoldersByDistance(pirate.Location).Count > 0) {

                Pirate enemyHolder = Utils.EnemyHoldersByDistance(pirate.Location).First();

                //Turn 107 against  2 there is a problem in pushing
                if (pirate.CanPush(enemyHolder)) {

                    if (Predict.enemiesPossiblyPushed.Contains(enemyHolder.Id) && !Main.piratesPushed.Contains(enemyHolder)) {

                        pirate.Push(enemyHolder, Utils.OppositeLocation(enemyHolder.Location));
                        Main.piratesPushed.Add(enemyHolder);
                        return "Pushed the boosted enemy holder";
                    }

                    var cloestEdge = Utils.CloestEdge(enemyHolder.Location);
                    double killCost = ((double)cloestEdge.Item1) / game.PushDistance;
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
                    }

                    if (killCost <= 1.26) /*add the movement of the enemy pirate to kill cost*/{

                        pirate.Push(enemyHolder, cloestEdge.Item2);
                        return Utils.GetPirateStatus(pirate, "Attacked holder");
                    }

                }

                Utils.SafeSail(pirate, enemyHolder.GetLocation());
                return Utils.GetPirateStatus(pirate, "Moving towards enemy holder");
            }

            if (Main.enemyMines.Count > 0 && game.GetEnemyMotherships().Count() > 0) {

                var cloestEnemyMine = Utils.OrderByDistance(Main.enemyMines, pirate.Location).First();
                var cloestEnemyShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), pirate.Location).First();
                var sailLocation = cloestEnemyShip.Towards(cloestEnemyMine, game.PirateMaxSpeed + game.PushDistance);

                Utils.SafeSail(pirate, sailLocation);
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

            return 20;
        }




    }


}
