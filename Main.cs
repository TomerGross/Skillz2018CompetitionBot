using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class Main : IPirateBot {


        //---------------[ Main variables ]-----------
        public static PirateGame game;

        public static bool debug = true;
        public static bool fullDebug = true;

        public static List<int> didTurn = new List<int>();

        public static List<Pirate> piratesPushed = new List<Pirate>();
        public static List<Pirate> sailToworm = new List<Pirate>();
        public static List<Pirate> holdersPaired = new List<Pirate>();

        public static List<Wormhole> wormsPushed = new List<Wormhole>();
        public static List<Wormhole> wormHolesTargetted = new List<Wormhole>();

        public static List<Asteroid> asteroidsPushed = new List<Asteroid>();
        //--------------------------------------------


        //---------------[ Dirty A$$ ]--------------------
        public static Tuple<Asteroid, int> dirtyAss;

        public static bool didDirtyAss = false;
        //--------------------------------------------


        //---------------[ Mines ]--------------------
        public static List<Location> mines = new List<Location>();
        public static List<Location> enemyMines = new List<Location>();

        public static Dictionary<Pirate, Capsule> capsulesTargetted = new Dictionary<Pirate, Capsule>();

        public static int maxMiners = 2;
        public static int maxMolers = 2;
        //--------------------------------------------


        //---------------[ Task mangment ]-------------
        public static Dictionary<int, Tuple<TaskType, Task>> tasks = new Dictionary<int, Tuple<TaskType, Task>>();

        public static List<Pirate> unemployedPirates = new List<Pirate>();

        public readonly List<TaskType> todoTasks = new List<TaskType>(new List<TaskType>
        { TaskType.MINER, TaskType.MOLE, TaskType.BOOSTER });
        //--------------------------------------------


        public void DoTurn(PirateGame game) {

            Main.game = game;

            // Clearing objects

            didTurn.Clear();
            sailToworm.Clear();
            capsulesTargetted.Clear();
            asteroidsPushed.Clear();
            piratesPushed.Clear();
            wormsPushed.Clear();

            // Gettings the mines
            if (game.GetMyCapsules().Count() > 0 && game.Turn == 1) {
                game.GetMyCapsules().Where(cap => cap.Holder == null && !mines.Contains(cap.Location)).ToList().ForEach(cap => mines.Add(cap.Location));
            }

            if (game.GetEnemyCapsules().Count() > 0 && game.Turn == 1) {
                game.GetEnemyCapsules().Where(cap => cap.Holder == null && !enemyMines.Contains(cap.Location)).ToList().ForEach(cap => enemyMines.Add(cap.Location));
            }


            unemployedPirates = game.GetMyLivingPirates().ToList();
            HandTasks();

            if(didDirtyAss == false && dirtyAss == null && game.GetLivingAsteroids().Any() && game.GetEnemyMotherships().Any() && Utils.PiratesWithTask(TaskType.MOLE).Any()){

                var cloestEnemyMom = game.GetEnemyMotherships().OrderBy(Utils.PiratesWithTask(TaskType.MOLE).First().Distance).First();
                var cloestAssToMom = game.GetLivingAsteroids().OrderBy(cloestEnemyMom.Distance).First();
                var cloestMoleToAss = Utils.PiratesWithTask(TaskType.MOLE).OrderBy(cloestAssToMom.Distance).First();

                dirtyAss = new Tuple<Asteroid, int>(cloestAssToMom, cloestMoleToAss.Id);
            }

            foreach (Pirate pirate in game.GetMyLivingPirates().OrderByDescending(p => tasks[p.Id].Item2.HeavyWeight())) {

                var switchWith = game.GetMyLivingPirates().Where(p => p.Id != pirate.Id
                                                              && tasks[p.Id].Item2.HeavyWeight() < tasks[pirate.Id].Item2.HeavyWeight()
                                                              && p.StateName == game.STATE_NAME_HEAVY)
                                                                .OrderBy(p => tasks[p.Id].Item2.HeavyWeight());

                if (switchWith.Any() && pirate.StateName != game.STATE_NAME_HEAVY) {
                    switchWith.First().SwapStates(pirate);
                    didTurn.Add(switchWith.First().Id);
                    break;
                }
            }

  
            foreach (KeyValuePair<int, Tuple<TaskType, Task>> pair in tasks.OrderByDescending(pair => pair.Value.Item2.Priority())) {

                var preform = pair.Value.Item2.Preform();

                if (debug) {
                    game.Debug(preform);
                }
            }
        }


        public Tuple<Pirate, TaskType> BestTaskToAssign() {

            var scores = new Dictionary<Tuple<Pirate, TaskType>, double>();

            foreach (Pirate pirate in unemployedPirates) {
                foreach (TaskType taskType in todoTasks) {

                    Task task = TaskTypeToTask(pirate, taskType);
                    double score = task.Bias() + task.GetWeight();

                    scores[new Tuple<Pirate, TaskType>(pirate, taskType)] = score;
                }
            }

            foreach (Pirate pirate in unemployedPirates) {
                var ptasks = from tup in scores.Keys.ToList().Where(tup => tup.Item1.Id == pirate.Id) select tup.Item2.ToString() + " > " + scores[tup] + "  ||  ";
                string s = "";
                ptasks.ToList().ForEach(str => s += str);

                if (fullDebug) {
                    game.Debug(pirate.Id + " | " + s);
                }
            }

            if (scores.Count > 0) {
                if (fullDebug) {
                    var best = scores.OrderByDescending(pair => pair.Value).First();
                    game.Debug("Gave: " + best.Key.Item1.Id + " | " + best.Key.Item2 + " at: " + best.Value);
                }

                return scores.OrderByDescending(pair => pair.Value).First().Key;
            }

            return new Tuple<Pirate, TaskType>(game.GetMyLivingPirates()[0], TaskType.MINER);
        }


        public void HandTasks() {

            Dictionary<int, Tuple<TaskType, Task>> tempTasks = tasks.ToDictionary(p1 => p1.Key, p2 => p2.Value);
            tasks.Clear();

            for (int i = 0; i < game.GetMyLivingPirates().Count(); i++) {

                var bestTask = BestTaskToAssign();

                Pirate pirate = bestTask.Item1;
                TaskType taskType = bestTask.Item2;

                if (!tempTasks.ContainsKey(pirate.Id) || tempTasks[pirate.Id].Item1 != taskType) {
                    tasks[pirate.Id] = new Tuple<TaskType, Task>(taskType, TaskTypeToTask(pirate, taskType));
                } else {
                    tasks[pirate.Id] = tempTasks[pirate.Id];
                    tasks[pirate.Id].Item2.UpdatePirate(pirate);
                }

                if (taskType == TaskType.MINER && Utils.FreeCapsulesByDistance(pirate.Location).Count > 0) {
                    var cloestCapsule = Utils.FreeCapsulesByDistance(pirate.Location).First();
                    capsulesTargetted.Add(pirate, cloestCapsule);
                }

                unemployedPirates.Remove(pirate);
            }
        }


        public Task TaskTypeToTask(Pirate pirate, TaskType task) {

            switch (task) {

                case TaskType.BOOSTER:
                    return new TaskBooster(pirate);

                case TaskType.MOLE:
                    return new TaskMole(pirate);

                default:
                    return new TaskMiner(pirate);
            }
        }


    }

}
