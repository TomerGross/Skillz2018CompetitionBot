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

            if (game.GetEnemyCapsule().Holder != null) {

                Pirate enemyHolder = game.GetEnemyCapsule().Holder;

                if (pirate.CanPush(enemyHolder)) {

                    var cloestEdge = Utils.CloestEdge(enemyHolder.Location);
                    double killCost = cloestEdge.Item1 / game.PushDistance;

                    if (killCost <= 1) {
                        pirate.Push(enemyHolder, cloestEdge.Item2);
                        return Utils.GetPirateStatus(pirate, "Pushed nemey holder to edge");
                    }

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

                pirate.Sail(enemyHolder);
                return Utils.GetPirateStatus(pirate, "Moving towards enemy holder");
            }

            pirate.Sail(Main.mineEnemy.Towards(Main.game.GetEnemyMothership(), 300));
            return Utils.GetPirateStatus(pirate, "Moving towards enemy mine");
        }



        override public double GetWeight() {

            if (game.GetEnemyCapsule().Holder != null) {

                var holderDisEnemyMom = game.GetEnemyCapsule().Holder.Distance(game.GetEnemyMothership());

                if (holderDisEnemyMom <= pirate.Distance(game.GetEnemyMothership()) - game.PushRange) {
                    return 0;
                }

                var pairs = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetEnemyCapsule().Holder);
                int index = pairs.IndexOf(pairs.First(tuple => tuple.Item1 == pirate));

                int numofpirates = Main.game.GetAllMyPirates().Length;

                double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(game.GetEnemyCapsule().Holder));
                double weight = ((double)(maxDis - pirate.Distance(game.GetEnemyCapsule().Holder)) / maxDis) * 100;

                return weight;
            }

            return 0;
        }


        override public int Bias() {

            if (Utils.PiratesWithTask(TaskType.BERSERKER).Count % 2 == 0) {
                return 45;
            }

            return 0;
        }




    }


}
