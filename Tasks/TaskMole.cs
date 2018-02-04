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

            if (Main.didTurn.Contains(pirate.Id)) {
                return Utils.GetPirateStatus(pirate, "Already did turn");
            }

            int radius = game.PushDistance;

            if (Utils.PushAsteroid(pirate)) {
                return Utils.GetPirateStatus(pirate, "Pushed asteroid");
            }

            if (Utils.EnemyHoldersByDistance(pirate.Location).Count() > 0) {

                var enemyHolder = Utils.EnemyHoldersByDistance(pirate.Location).First();

                if (pirate.CanPush(enemyHolder)) {
                    var cloestEdge = Utils.CloestEdge(enemyHolder.GetLocation());
                    double killCost = (cloestEdge.Item1 + pirate.MaxSpeed / 2) / game.PushDistance;

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

                        return Utils.GetPirateStatus(pirate, "Couple attacked holder");
                    }


                    pirate.Push(enemyHolder, Utils.OppositeLocation(enemyHolder.Location));
                    return Utils.GetPirateStatus(pirate, "Pushed enemy holder");
                }
            }

            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyCapsules().ToList(), pirate.Location).First();
            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule).First();


            if (game.Cols > 7000) {

                radius += (game.PushDistance / 2) * (Utils.PiratesWithTask(TaskType.MOLE).IndexOf(pirate));
            }
            
            else if (Utils.PiratesWithTask(TaskType.MOLE).IndexOf(pirate) > 1)
                radius += (game.PushDistance / 2);
            
            var loc = nearestShip.GetLocation().Towards(nearestCapsule, radius);

            pirate.Sail(Utils.SafeSail(pirate, loc));
            return Utils.GetPirateStatus(pirate, "Is sailing to position");
        }

        

        override public double GetWeight() {
            
            double numofholders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Count();
            if (numofholders == 0)
                numofholders = 0.5;
            
            if (game.GetEnemyMotherships().Count() == 0 || Utils.PiratesWithTask(TaskType.MOLE).Count >= 2 * numofholders) {
                return -80;
            }

            var nearestCapsule = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), pirate.Location).First();

            if (Utils.EnemyHoldersByDistance(pirate.Location).Count() > 0) {
                nearestCapsule = Utils.EnemyHoldersByDistance(pirate.Location).First().Location;
            }

            var nearestShip = Utils.OrderByDistance(game.GetEnemyMotherships().ToList(), nearestCapsule).First();

            double maxDis = Main.unemployedPirates.Max(pirate => pirate.Distance(nearestShip));
            double weight = ((double)(maxDis - pirate.Distance(nearestShip)) / maxDis) * 100;

            if (double.IsNaN(weight)) {
                return 0;
            }

            return weight;
        }


        override public int Bias() {
            
            double numofholders = Utils.EnemyHoldersByDistance(pirate.GetLocation()).Count();
            if (numofholders == 0)
                numofholders = 1;
            
            if (Utils.PiratesWithTask(TaskType.MOLE).Count >= 2 * numofholders) {
                return 0;
            }

            return 80;
        }

    }
}
