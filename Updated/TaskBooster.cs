using System.Linq;
using Pirates;

namespace Hydra {

    public class TaskBooster: Task {


        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------


        readonly int radius; // Maximum distance from holder
        readonly Pirate pirate;


        public TaskBooster(Pirate pirate, int radius) {

            this.pirate = pirate;
            this.radius = radius;
        }


        public TaskBooster(Pirate pirate) {

            this.pirate = pirate;
            radius = 500;
        }


        //BUG, TURN 25 VS BUNKERBOT
        override public string Preform() {

            if (game.GetMyCapsule().Holder != null) {

                Pirate holder = game.GetMyCapsule().Holder;

                game.Debug(Utils.GetPirateStatus(pirate, "PUSH TURNS: " + pirate.PushReloadTurns + " CANPUSH: " + pirate.CanPush(holder) + " DIS: " + holder.Distance(game.GetMyMothership())) + " DIS2: " + (game.PushDistance + holder.MaxSpeed));

                if (pirate.CanPush(holder) && Main.numofpushes == 0 && holder.Distance(game.GetMyMothership()) - 300 <= (game.PushDistance + holder.MaxSpeed)) {
                    game.Debug("numofpushes: " + Main.numofpushes);
                    Main.numofpushes++;
                    pirate.Push(holder, game.GetMyMothership());
                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                }

                if (holder.Distance(pirate) >= radius) {

                    pirate.Sail(holder);
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

                pirate.Sail(holder);
                return Utils.GetPirateStatus(pirate, "Sailing towards holder");
            }

            pirate.Sail(Main.mine.GetLocation().Towards(game.GetMyMothership(), 500));
            return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
        }


        override public double GetWeight() {
            return new TaskEscort(pirate, radius).GetWeight();
        }


        override public int Bias() {

            if (Utils.PiratesWithTask(TaskType.BOOSTER).Count == 0) {
                return 100;
            }

            return 0;
        }


    }
}
