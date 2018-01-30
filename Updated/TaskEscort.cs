using System.Linq;
using Pirates;

namespace Hydra {

    public class TaskEscort : Task {


        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;

        //-----------------------------------------------------------------------


        readonly int radius; // Maximum distance from holder
        readonly Pirate pirate;


        public TaskEscort(Pirate pirate, int radius) {

            this.pirate = pirate;
            this.radius = radius;
        }


        public TaskEscort(Pirate pirate) {

            this.pirate = pirate;
            radius = 500;
        }

        //BUG, TURN 25 VS BUNKERBOT
        override public string Preform() {

            if (game.GetMyCapsule().Holder != null) {

                Pirate holder = game.GetMyCapsule().Holder;

                if (holder.Distance(pirate) >= radius) {
                    pirate.Sail(holder);
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

                foreach (Pirate enemy in game.GetEnemyLivingPirates().Where(enemy => pirate.CanPush(enemy))) {
                    pirate.Push(enemy, enemy.Location.Towards(game.GetEnemyMothership(), -5000));
                    return Utils.GetPirateStatus(pirate, "Pushed enemy pirate " + enemy.Id);
                }

                pirate.Sail(holder);
                return Utils.GetPirateStatus(pirate, "Sailing towards holder");
            }

            pirate.Sail(Main.mine.GetLocation().Towards(game.GetMyMothership(), 500));
            return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
        }


        override public double GetWeight() {

            Location holder = Main.mine;

            if (game.GetMyCapsule().Holder != null) {
                holder = game.GetMyCapsule().Holder.Location;
            }

            var enemyDisMyMom = Utils.SoloClosestPair(game.GetEnemyLivingPirates(), game.GetMyMothership());

            if (game.Turn > 1 && enemyDisMyMom.First().Item2 > holder.Distance(game.GetMyMothership()) + game.PushRange) {
                return 0;
            }

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(holder));

            double weight = ((double)(maxDis - pirate.Distance(holder)) / maxDis) * 100;
            game.Debug("ESCORT WEIGHT: " + weight);

            return weight;
        }


        override public int Bias() {
            return 0;
        }


    }
}
