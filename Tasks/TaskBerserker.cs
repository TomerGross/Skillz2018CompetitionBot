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

                    var available = Utils.PiratesWithTask(TaskType.BERSERKER);
                    available.RemoveAll(escort => !escort.CanPush(enemyHolder) || escort.Id == pirate.Id);
                    available.Insert(0, pirate);

                    if (killCost <= 1) {

                        pirate.Push(enemyHolder, cloestEdge.Item2);
                        return Utils.GetPirateStatus(pirate, "Killed enemy holder");
                    }

                    if (available.Count >= 1) {

                        var pushLocation = new Location(6400 - enemyHolder.Location.Row, 6400 - enemyHolder.Location.Col);

                        if (0.5 * killCost <= 1) {
                            pushLocation = cloestEdge.Item2;
                        }

                        foreach (Pirate escort in available.Take(2)) {
                            Main.didTurn.Add(escort.Id);
                            escort.Push(enemyHolder, cloestEdge.Item2);
                        }

                        return Utils.GetPirateStatus(pirate, "Couple attacked holder");
                    }

                }

                pirate.Sail(enemyHolder);
                return "Berserker moving towards enemy holder...";
            }

            pirate.Sail(Main.mineEnemy.Towards(Main.game.GetEnemyMothership(), 300));
            return "Berserker is sailing towards enemy mine.";
        }



        override public int GetWeight() {

            if (game.GetEnemyCapsule().Holder != null) {

                var pairs = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetEnemyCapsule().Holder);
                int index = pairs.IndexOf(pairs.First(tuple => tuple.Item1 == pirate));

                int numofpirates = Main.game.GetAllMyPirates().Length;

                return (numofpirates - index) * (100 / numofpirates);
            }

            return 0;
        }


        override public int Bias() {

            return 50;
        }




    }


}
