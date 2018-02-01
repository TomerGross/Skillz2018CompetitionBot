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

            if (game.GetMyCapsules()[0].Holder != null) {

                Pirate holder = game.GetMyCapsules()[0].Holder;

                game.Debug(Utils.GetPirateStatus(pirate, "PUSH TURNS: " + pirate.PushReloadTurns + " CANPUSH: " + pirate.CanPush(holder) + " DIS: " + holder.Distance(game.GetMyMotherships()[0])) + " DIS2: " + (game.PushDistance + holder.MaxSpeed));

                if (pirate.CanPush(holder) && Main.numofpushes == 0 && holder.Distance(game.GetMyMotherships()[0]) - 300 <= (game.PushDistance + holder.MaxSpeed)) {
                    game.Debug("numofpushes: " + Main.numofpushes);
                    Main.numofpushes++;
                    pirate.Push(holder, game.GetMyMotherships()[0]);
                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                }
                
                Location futureLocation = holder.GetLocation().Towards(game.GetMyMotherships()[0], game.PushDistance);
                var enemyDisMyMom = Utils.SoloClosestPair(game.GetEnemyLivingPirates(), game.GetMyMotherships()[0]);
                
                if (pirate.CanPush(holder) && enemyDisMyMom.Count > 0 && Main.numofpushes == 0 && futureLocation.Distance(game.GetMyMotherships()[0]) < enemyDisMyMom.First().Item2 - game.PushRange)
                {
                    game.Debug("numofpushes: " + Main.numofpushes);
                    Main.numofpushes++;
                    pirate.Push(holder, game.GetMyMotherships()[0]);
                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                }
                

                if (holder.Distance(pirate) >= radius) {

                    pirate.Sail(holder);
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

                pirate.Sail(holder);
                return Utils.GetPirateStatus(pirate, "Sailing towards holder");
            }

            Utils.OrderByDistance(Main.mines, pirate.Location)
                 pirate.Sail(Utils.OrderByDistance(Main.mines, pirate.Location).First().Towards(game.GetMyMotherships()[0], 500));
            return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
        }


        override public double GetWeight() {
            return new TaskEscort(pirate, radius).GetWeight();
        }


        override public int Bias() {
            
            if (game.GetMyCapsules().Count() == 0) {
                return 0;
            }


            if (Utils.PiratesWithTask(TaskType.BOOSTER).Count == 0) {
                return 100;
            }

            return 0;
        }


    }
}
