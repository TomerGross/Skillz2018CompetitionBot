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

                foreach (Pirate enemy in game.GetEnemyLivingPirates().Where(enemy => pirate.CanPush(enemy))) {
                    /*if (Main.enemypirates.Contains(enemy)){
                        Main.enemypirates.Remove(enemy);
                        pirate.Push(enemy, enemy.Location.Towards(game.GetEnemyMothership(), -5000));
                        return Utils.GetPirateStatus(pirate, "Pushed enemy pirate " + enemy.Id);
                    }*/
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

            if (game.GetMyCapsule().Holder == null || Utils.PiratesWithTask(TaskType.MINER).Count == 0) {
                return 0;
            }

            Location holder = Utils.PiratesWithTask(TaskType.MINER).First().Location;

            if(game.GetMyCapsule().Holder != null){
                holder = game.GetMyCapsule().Holder.Location;
            }

            var enemyDisMyMom = Utils.SoloClosestPair(game.GetEnemyLivingPirates(), game.GetMyMothership());

            if (enemyDisMyMom.First().Item2 > holder.Distance(game.GetMyMothership()) + game.PushRange) {
                return 0;
            }

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(holder));

            double weight = ((double)(maxDis - pirate.Distance(holder)) / maxDis) * 100;
            game.Debug("ESCORT WEIGHT: " + weight);

            return weight;
        }


        override public int Bias() {

            return 30;
        }


    }
}
