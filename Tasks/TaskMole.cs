using Pirates;
using System.Linq;

namespace Hydra {

    public class TaskMole : Task {

        //-------------------Globals---------------------------------------------
        public static PirateGame game = Main.game;
        //-----------------------------------------------------------------------

        readonly Pirate pirate;


        public TaskMole(Pirate pirate) {
            this.pirate = pirate;
        }


        override public string Preform() {


            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First();
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule.Location).First();

            if (Main.didTurn.Contains(pirate.Id)) {
                return Utils.GetPirateStatus(pirate, "Already did turn");
            }

            if (Utils.EnemyHoldersByDistance(pirate.Location).Count() > 0) {

                foreach (Pirate enemyHolder in Utils.EnemyHoldersByDistance(pirate.Location)) {

                    if (!Main.piratesPushed.Contains(enemyHolder) && pirate.CanPush(enemyHolder)) {

                        var cloestEdge = Utils.CloestEdge(enemyHolder.GetLocation());
                        double killCost = (cloestEdge.Item1 + pirate.MaxSpeed / 2) / game.PushDistance;

                        if (killCost <= 1) {
                            Main.piratesPushed.Add(enemyHolder);
                            Main.didTurn.Add(pirate.Id);
                            pirate.Push(enemyHolder, Utils.OppositeLocation(enemyHolder.Location));
                            return Utils.GetPirateStatus(pirate, "Pushed enemy holder to edge");
                        }

                        var available = Utils.PiratesWithTask(TaskType.MOLE);
                        available.AddRange(Utils.PiratesWithTask(TaskType.BERSERKER));
                        available.RemoveAll(pirateAvailable => !pirateAvailable.CanPush(enemyHolder) || pirateAvailable.Id == pirate.Id || Main.didTurn.Contains(pirateAvailable.Id));
                        available.Insert(0, pirate);

                        if (available.Count >= 2) {
                            var pushLocation = new Location(game.Rows - enemyHolder.Location.Row, game.Cols - enemyHolder.Location.Col);

                            if (0.5 * killCost <= 1) {
                                pushLocation = cloestEdge.Item2;
                            }

                            foreach (Pirate mole in available.Take(2)) {

                                Main.didTurn.Add(mole.Id);
                                mole.Push(enemyHolder, cloestEdge.Item2);
                            }

                            Main.piratesPushed.Add(enemyHolder);
                            return Utils.GetPirateStatus(pirate, "Couple attacked holder");
                        }

                    }

                    var enemysnoholders = game.GetEnemyLivingPirates().ToList();
                    enemysnoholders.Remove(enemyHolder);

                    var case1 = true;

                    if (enemysnoholders.Count() > 0) {
                        case1 = (Utils.OrderByDistance(enemysnoholders, enemyHolder.GetLocation()).First().Distance(enemyHolder) > pirate.MaxSpeed + pirate.PushRange) && enemyHolder.Distance(nearestShip) < 4 * game.PushDistance;
                    }

                    var case2 = enemyHolder.Distance(nearestShip) < 2 * game.PushDistance + pirate.Distance(nearestShip);
                    game.Debug(/*"case1: " + case1 + */"    case2: " + case2);

                    var case3 = pirate.PushReloadTurns <= (enemyHolder.Distance(nearestShip) - pirate.Distance(nearestShip)) / (2 * enemyHolder.MaxSpeed);
                    if (/*case1 ||*/ case2 && case3) {
                        Utils.SafeSail(pirate, enemyHolder.GetLocation());
                        return Utils.GetPirateStatus(pirate, "Sailing to enemy holder");
                    }

                }
            }

            int radius = (game.PushRange + game.PushDistance) / 2;
            /*
            if (game.Cols > 7000) {
                radius = game.PushDistance;
                radius += (game.PushDistance / 2) * (Utils.PiratesWithTask(TaskType.MOLE).IndexOf(pirate));
            } 
            */
            if (Utils.PiratesWithTask(TaskType.MOLE).IndexOf(pirate) > 1)
                radius += game.PushDistance;

            var loc = nearestShip.GetLocation().Towards(nearestCapsule, radius);

            Utils.SafeSail(pirate, loc);
            return Utils.GetPirateStatus(pirate, "Is sailing to position");
        }



        override public double GetWeight() {

            var number = 0;
            var numofholders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Count();

            if (number == 1) {
                number = 3;
            } else
                number = 2 * numofholders;


            if (game.GetEnemyMotherships().Count() == 0 || Utils.PiratesWithTask(TaskType.MOLE).Count() >= number) {
                return 0;
            }


            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), pirate.Location).First();
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule).First();

            if (Utils.EnemyHoldersByDistance(pirate.Location).Count() > 0) {
                nearestCapsule = Utils.EnemyHoldersByDistance(pirate.Location).First().Location;
            }

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(nearestShip));
            double weight = ((double)(maxDis - pirate.Distance(nearestShip)) / maxDis) * 100;

            if (double.IsNaN(weight)) {
                return 0;
            }

            return weight;
        }


        override public int Bias() {

            if (Utils.PiratesWithTask(TaskType.MOLE).Count >= 2 * game.GetMyCapsules().Count() - Utils.FreeCapsulesByDistance(pirate.Location).Count()) {
                return 1;
            }

            return 70;
        }

    }
}
