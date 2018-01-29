using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class TaskBerserker : Task {

        //-------------------Globals---------------------------------------------
        public static Dictionary<int, Path> paths = new Dictionary<int, Path>();
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------


        readonly Pirate pirate;


        public TaskBerserker(Pirate pirate) {
            this.pirate = pirate;
        }


        override public string Preform() {

            if (game.GetEnemyCapsule().Holder != null) {

                Pirate enemyHolder = game.GetEnemyCapsule().Holder;

                if (pirate.CanPush(enemyHolder)) {

                    var cloestEdge = Utils.CloestEdge(enemyHolder.Location);

                    if (cloestEdge.Item1 <= game.PushDistance) {
                        pirate.Push(enemyHolder, cloestEdge.Item2);
                    } else {
                        pirate.Push(enemyHolder, Main.mineEnemy);
                    }

                    return Utils.GetPirateStatus(pirate, "Pushed enemy holder");
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
