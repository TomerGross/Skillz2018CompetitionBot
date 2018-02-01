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


        override public string Preform() {

            if (Utils.GetMyHolders().Count() > 0 && game.GetAllMotherships().Count() != 0) {

                var holder = Utils.GetMyHolders().OrderBy(h => h.Distance(pirate)).First();
                var ship = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First();

                if (holder.Distance(pirate) >= radius) {
                    pirate.Sail(Utils.SafeSail(pirate, holder.Location));
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

                if (pirate.CanPush(holder) && Main.numofpushes == 0 && holder.Distance(ship) - 300 <= (game.PushDistance + holder.MaxSpeed)) {
                    game.Debug("numofpushes: " + Main.numofpushes);
                    Main.numofpushes++;
                    pirate.Push(holder, ship);
                    return Utils.GetPirateStatus(pirate, "Pushed holder directly to ship");
                }

                pirate.Sail(Utils.SafeSail(pirate, holder.Location));
                return Utils.GetPirateStatus(pirate, "Sailing towards holder");
            }

            if (Main.mines.Count > 0 && game.GetMyMotherships().Count() > 0) {

                var nearestMine = Utils.OrderByDistance(Main.mines, pirate.Location).First();
                var nearestShipToMine = Utils.OrderByDistance(game.GetMyMotherships().ToList(), nearestMine).First();
                var locTowards = nearestMine.Towards(nearestShipToMine, game.PirateMaxSpeed * 2);

                pirate.Sail(Utils.SafeSail(pirate, locTowards));
                return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
            }

            return Utils.GetPirateStatus(pirate, "Is idle.");
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
