using Pirates;

namespace Hydra {

    public class TaskMole : Task {


        readonly Pirate pirate;

        public TaskMole(Pirate pirate) {
            this.pirate = pirate;
        }


        override public string Preform() {

            PirateGame game = Main.game;

            int radius = game.PirateMaxSpeed * 2;

            if (game.GetEnemyCapsule().Holder != null) {

                Pirate enemyHolder = game.GetEnemyCapsule().Holder;
                Location towardsEnemy = game.GetEnemyMothership().GetLocation().Towards(game.GetEnemyCapsule(), radius);

                // If the pirate is in position and can attack
                if (pirate.Distance(towardsEnemy) < (radius / 8) && pirate.CanPush(enemyHolder)) {

                    var cloestEdge = Utils.CloestEdge(enemyHolder.GetLocation());

                    if (cloestEdge.Item1 <= game.PushDistance) {
                        pirate.Push(enemyHolder, cloestEdge.Item2);
                    } else {
                        pirate.Push(enemyHolder, Main.mineEnemy);
                    }

                    return Utils.GetPirateStatus(pirate, "Pushed enemy holder");


                } else {

                    // Sail to a position
                    pirate.Sail(towardsEnemy);
                    return "Pirate is sailing to position";
                }
            }

            pirate.Sail(game.GetEnemyMothership().GetLocation().Towards(Main.mineEnemy, radius));
            return Utils.GetPirateStatus(pirate, "Is sailing to position");
        }



        override public double GetWeight() {

            return 60;
        }


        override public int Bias() {

            return 50;
        }

    }
}
