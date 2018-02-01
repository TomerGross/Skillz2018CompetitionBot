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

                if (pirate.CanPush(enemyHolder)) {

                    var cloestEdge = Utils.CloestEdge(enemyHolder.Location);
                    double killCost = (cloestEdge.Item1 + pirate.MaxSpeed / 2) / game.PushDistance;

                    var available = Utils.PiratesWithTask(TaskType.BERSERKER);
                    available.AddRange(Utils.PiratesWithTask(TaskType.MOLE));
                    available.RemoveAll(pirateAvailable => !pirateAvailable.CanPush(enemyHolder) || pirateAvailable.Id == pirate.Id);
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

                }

                var sailLocation = Utils.SafeSail(pirate, enemyHolder.Location);

                Chunk origin2 = Chunk.GetChunk(pirate.Location);
                Chunk endgoal2 = Chunk.GetChunk(enemyHolder.Location);

                var traits2 = new List<Trait>() {
                        new TraitRateByAsteroid(2)
                };

                Path path2 = new Path(origin2, endgoal2, traits2, Path.Algorithm.ASTAR);

                if (path2.GetChunks().Count > 0) {

                    Chunk nextChunk = path2.Pop();
                    pirate.Sail(nextChunk.GetLocation());
                    return Utils.GetPirateStatus(pirate, "Moving towards enemy holder");
                }

            }

            if (Main.enemyMines.Count > 0 && game.GetEnemyMotherships().Count() > 0) {

                var cloestEnemyMine = Utils.OrderByDistance(Main.enemyMines, pirate.Location).First();
                var cloestEnemyShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), pirate.Location).First();
                var sailLocation = cloestEnemyMine.Towards(cloestEnemyShip, game.PirateMaxSpeed + game.PushDistance);
                   
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
                return ((maxDis - pirate.Distance(capsule)) / maxDis) * 100;
            }

            return 0;
        }



        override public int Bias() {

            if (Utils.PiratesWithTask(TaskType.BERSERKER).Count % 2 == 0) {
                return 45;
            }

            return 1;
        }




    }


}
