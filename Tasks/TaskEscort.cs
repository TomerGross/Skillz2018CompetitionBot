using System.Linq;
using Pirates;

namespace Hydra {

    public class TaskEscort : Task {


        int radius = 500; // Maximum range
        Pirate pirate;

        public TaskEscort(Pirate pirate) {

            this.pirate = pirate;
        }


        override public string Preform() {

            PirateGame game = Main.game;

            if (Main.game.GetMyCapsule().Holder != null) {

                Pirate holder = game.GetMyCapsule().Holder;

                if (!pirate.CanPush(holder) && holder.Distance(game.GetMyMothership()) <= (game.PushDistance + holder.MaxSpeed)) {

                    pirate.Push(holder, game.GetMyMothership());
                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                }

                if (holder.Distance(pirate) >= radius) {

                    pirate.Sail(Main.game.GetMyCapsule().Holder.GetLocation());
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder";
                }
            }

            foreach (Pirate enemy in Main.game.GetEnemyLivingPirates().Where(enemy => pirate.CanPush(enemy))) {

                pirate.Push(enemy, enemy.Location.Towards(Main.game.GetEnemyMothership(), -5000));
                return Utils.GetPirateStatus(pirate, "Pushed enemy pirate " + enemy.Id);
            }

            pirate.Sail(Main.mine.GetLocation().Towards(game.GetMyMothership(), 500));
            return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
        }


        override public int GetWeight() {

            if (Main.game.GetMyCapsule().Holder != null && Main.game.GetMyCapsule().Holder != pirate) {



                var sortedlist1 = Utils.SoloClosestPair(Main.game.GetEnemyLivingPirates(), Main.game.GetMyMothership());
                Pirate myholder = Main.game.GetMyCapsule().Holder;
                if (sortedlist1[0].Distance(Main.game.GetMyMothership()) > myholder.Distance(Main.game.GetMyMothership()) + myholder.PushRange)
                    return 0;

                var sortedlist = Utils.SoloClosestPair(Main.game.GetMyLivingPirates(), Main.game.GetMyCapsule().Holder);
                int numofpirates = Main.game.GetAllMyPirates().Length;

                return (numofpirates - sortedlist.IndexOf(pirate)) * (100 / numofpirates);
            }

            return 0;
        }


        override public int Bias() {

            if (Main.game.GetMyCapsule().Holder == null) {
                return 0;
            }

            return 50;
        }


    }
}
