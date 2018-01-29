using System.Linq;
using System.Collections.Generic;
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


        public Pirate getHolder(){
            var holder = Utils.PiratesWithTask(TaskType.MINER)[0];

            if (game.GetMyCapsule().Holder != null) {
                holder = game.GetMyCapsule().Holder;
            }

            return holder;
        }

        override public string Preform() {

            var holder = getHolder();

            if (game.GetMyCapsule().Holder != null) {
                
                if (!pirate.CanPush(holder) && holder.Distance(game.GetMyMothership()) <= (game.PushDistance  + holder.MaxSpeed)) {
                    
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

            if (holder == null) {

                var pairs = Utils.SoloClosestPair(game.GetMyLivingPirates(), game.GetMyCapsule().Holder);
                pirate.Sail(pairs[0].Item1);

                return Utils.GetPirateStatus(pirate, "Sailing closest pair");
            }

            pirate.Sail(Main.mine.GetLocation().Towards(game.GetMyMothership(), 500));
            return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
        }


        override public int GetWeight() {

            if (Utils.PiratesWithTask(TaskType.MINER).Count == 0) {
                return 0;
            }

            if (Utils.PiratesWithTask(TaskType.ESCORT).Count >= Main.maxMiners * 2) {
                return 0;
            }

            var enemyDisMyMom = Utils.SoloClosestPair(game.GetEnemyLivingPirates(), game.GetMyMothership());

            if(enemyDisMyMom.First().Item2 > getHolder().Distance(game.GetMyMothership()) + game.PushRange){
                return 0;
            }

            int maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(getHolder()));

            return (pirate.Distance(getHolder()) - maxDis) / -10;
        }


        override public int Bias() {

            if (Utils.PiratesWithTask(TaskType.MINER).Count == 0) {
                return 0;
            }

            return 0;
        }


    }
}
