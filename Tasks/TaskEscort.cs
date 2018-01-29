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


        override public string Preform() {

            Pirate holder = game.GetMyCapsule().Holder;

            if (game.GetMyCapsule().Holder != null) {

                if (!pirate.CanPush(holder) && holder.Distance(game.GetMyMothership()) <= (game.PushDistance + holder.MaxSpeed)) {

                    pirate.Push(holder, game.GetMyMothership());
                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                }

                if (holder.Distance(pirate) >= radius) {

                    pirate.Sail(holder);
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

            }

            foreach (Pirate enemy in game.GetEnemyLivingPirates().Where(enemy => pirate.CanPush(enemy))) {

                pirate.Push(enemy, enemy.Location.Towards(game.GetEnemyMothership(), -5000));
                return Utils.GetPirateStatus(pirate, "Pushed enemy pirate " + enemy.Id);
            }

            if (game.GetMyCapsule().Holder != null) {

                var pairs = Utils.SoloClosestPair(game.GetMyLivingPirates(), game.GetEnemyCapsule().Holder);
                pirate.Sail(pairs[0].Item1);

                return Utils.GetPirateStatus(pirate, "Sailing closest pair");
            }

            pirate.Sail(Main.mine.GetLocation().Towards(game.GetMyMothership(), 500));
            return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
        }


        override public int GetWeight() {

            if (game.GetMyCapsule().Holder != null && game.GetMyCapsule().Holder != pirate) {
                
                var pairs = Utils.SoloClosestPair(game.GetMyLivingPirates(), game.GetEnemyCapsule().Holder);
                int index = pairs.IndexOf(pairs.First(tuple => tuple.Item1 == pirate));

                Pirate myholder = game.GetMyCapsule().Holder;

                if (pairs[0].Item1.Distance(game.GetMyMothership()) > myholder.Distance(game.GetMyMothership()) + myholder.PushRange) {
                    return 0;
                }

                var sortedlist = Utils.SoloClosestPair(game.GetMyLivingPirates(), game.GetMyCapsule().Holder);
                int numofpirates = game.GetAllMyPirates().Length;

                return (numofpirates - index) * (100 / numofpirates);
            }

            return 0;
        }


        override public int Bias() {

            if (game.GetMyCapsule().Holder == null) {
                return 0;
            }

            return 50;
        }


    }
}
