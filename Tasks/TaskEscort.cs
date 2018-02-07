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


        override public string Preform() {

            if (Utils.GetMyHolders().Count() > 0) {

                var holder = Utils.OrderByDistance(Utils.GetMyHolders(), pirate.Location).First();
                var ship = Utils.OrderByDistance(game.GetMyMotherships().ToList(), holder.Location).First();

                var threats = game.GetEnemyLivingPirates().ToList();
                threats.RemoveAll(threat => threat.PushReloadTurns > 2 || threat.Distance(ship) > game.PushRange * 2 || !pirate.CanPush(threat));
                threats = threats.OrderBy(treath => treath.Distance(holder)).ToList();


                if (holder.Distance(pirate) >= radius) {
                    Utils.SafeSail(pirate, holder.GetLocation());
                    return Utils.GetPirateStatus(pirate, "Sailing towards holder");
                }

                if (threats.Count() > 0) {
                    var cloestEdge = Utils.CloestEdge(threats.First().Location);
                    pirate.Push(threats.First(), cloestEdge.Item2);
                    return Utils.GetPirateStatus(pirate, "Pushed enemy mole " + threats.First().Id);
                }


                Utils.SafeSail(pirate, holder.GetLocation());
                return Utils.GetPirateStatus(pirate, "Sailing towards holder");
            }

            if (Main.mines.Count > 0 && game.GetMyMotherships().Count() > 0) {

                var nearestMine = Utils.OrderByDistance(Main.mines, pirate.Location).First();
                var nearestShipToMine = Utils.OrderByDistance(game.GetMyMotherships().ToList(), nearestMine).First();
                var locTowards = nearestMine.Towards(nearestShipToMine, game.PirateMaxSpeed * 2);

                Utils.SafeSail(pirate, locTowards);
                return Utils.GetPirateStatus(pirate, "Sailing to rendezvous point");
            }

            return Utils.GetPirateStatus(pirate, "Is idle.");
        }


        override public double GetWeight() {

            if (game.GetMyCapsules().Count() == 0) {
                return 0;
            }

            if (Utils.GetMyHolders().Count() > 0 && game.GetEnemyLivingPirates().Count() > 0) {
                var cloestMom = Utils.OrderByDistance(game.GetMyMotherships().ToList(), pirate.Location).First().GetLocation();
                var cloestEnemyToMom = Utils.OrderByDistance(game.GetEnemyLivingPirates().ToList(), cloestMom).First();
                var cloestHolder = Utils.GetMyHolders().OrderBy(holder => holder.Distance(pirate)).First();

                if (cloestEnemyToMom.Distance(cloestMom) > cloestHolder.Distance(cloestMom)) {
                    return -Bias();
                }
            }

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
