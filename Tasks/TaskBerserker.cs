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
                    double killCost = (cloestEdge.Item1 + pirate.MaxSpeed/2) / game.PushDistance;

                    var available = Utils.PiratesWithTask(TaskType.BERSERKER);
                    available.RemoveAll(escort => !escort.CanPush(enemyHolder) || escort.Id == pirate.Id);
                    available.Insert(0, pirate);

                    if (available.Count >= 2) {
                        var pushLocation = new Location(6400 - enemyHolder.Location.Row, 6400 - enemyHolder.Location.Col);

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

            var sailLocation = Main.mineEnemy.Towards(Main.game.GetEnemyMotherships()[0], 1000));

               Chunk origin = Chunk.GetChunk(pirate.Location);
                Chunk endgoal = Chunk.GetChunk(Main.mineEnemy.Towards(Main.game.GetEnemyMotherships()[0], 1000));

                var traits = new List<Trait>() {
                        new TraitRateByAsteroid(2)
                };

                Path path = new Path(origin, endgoal, traits, Path.Algorithm.ASTAR);

                if (path.GetChunks().Count > 0) {

                    Chunk nextChunk = path.Pop();
                    pirate.Sail(nextChunk.GetLocation());
                }
                
            return Utils.GetPirateStatus(pirate, "Moving towards enemy mine");
        }


        override public double GetWeight() {

            Location holder = game.GetEnemyCapsules()[0].Location;

            if (game.GetEnemyCapsules()[0].Holder != null) {
                holder = game.GetEnemyCapsules()[0].Holder.Location;
            }

            var pairs = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), holder);
            int index = pairs.IndexOf(pairs.First(tuple => tuple.Item1 == pirate));

            int numofpirates = Main.game.GetAllMyPirates().Length;

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(holder));
            double weight = ((double)(maxDis - pirate.Distance(holder)) / maxDis) * 100;

            return weight;
        }



        override public int Bias() {

            if (Utils.PiratesWithTask(TaskType.BERSERKER).Count % 2 == 0) {
                return 45;
            }

            return 1;
        }




    }


}
