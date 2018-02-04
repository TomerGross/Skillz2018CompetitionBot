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

            if (Utils.PushAsteroid(pirate)) {
                return Utils.GetPirateStatus(pirate, "Pushed asteroid");
            }
            
            if (Utils.GetMyHolders().Count() > 0) {
                
                var holder = Utils.OrderByDistance(Utils.GetMyHolders(), pirate.Location).First();
                
                if (holder.Distance(pirate) >= radius) {
                    pirate.Sail(Utils.SafeSail(pirate, holder));
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

                foreach (Pirate enemy in game.GetEnemyLivingPirates().Where(enemy => pirate.CanPush(enemy))) {
                    pirate.Push(enemy, enemy.Location.Towards(game.GetEnemyMotherships()[0], -5000));
                    return Utils.GetPirateStatus(pirate, "Pushed enemy pirate " + enemy.Id);
                }


                pirate.Sail(Utils.SafeSail(pirate, holder));
                return Utils.GetPirateStatus(pirate, "Sailing towards holder");
            }

            if(Main.mines.Count > 0 && game.GetMyMotherships().Count() > 0){

                var nearestMine = Utils.OrderByDistance(Main.mines, pirate.Location).First();
                var nearestShipToMine = Utils.OrderByDistance(game.GetMyMotherships().ToList(), nearestMine).First();
                var locTowards = nearestMine.Towards(nearestShipToMine, game.PirateMaxSpeed * 2);
                    
                pirate.Sail(Utils.SafeSail(pirate, locTowards));
                return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
            }

            return Utils.GetPirateStatus(pirate, "Is idle.");
        }


        override public double GetWeight() {

            if (game.GetMyCapsules().Count() == 0) {
                return 0;
            }

            /*
             * TODO ADD BACK THIS: 
             * 
            var enemyDisMyMom = Utils.SoloClosestPair(game.GetEnemyLivingPirates(), game.GetMyMotherships()[0]);

            if (game.Turn > 1 && enemyDisMyMom.Count > 0 && enemyDisMyMom.First().Item2 > holder.Distance(game.GetMyMotherships()[0]) + game.PushRange) {
                return 0;
            }
            */

            if (game.GetMyCapsules().Count() > 0) {

                var capsule = Utils.OrderByDistance(game.GetMyCapsules().ToList(), pirate.Location).First().GetLocation();

                if (Utils.GetMyHolders().Count > 0) {
                    capsule = Utils.GetMyHolders().OrderBy(hold => hold.Distance(pirate)).First().Location;
                }

                double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(capsule));
                return ((maxDis - pirate.Distance(capsule)) / maxDis) * 100;
            }

            return 0;

        }


        override public int Bias() {

            if (Utils.GetMyHolders().Count > 0) {
                return 20;
            }

            return 0;
        }


    }
}
