using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class Main : IPirateBot {


        //---------------[ Main variables ]-----------
        public static PirateGame game;

        public static bool debug = true;
        public static bool fullDebug = false;

        public static List<int> didTurn = new List<int>();

        public static List<Pirate> piratesPushed = new List<Pirate>();
        public static List<Pirate> sailToworm = new List<Pirate>();
        public static List<Pirate> holdersPaired = new List<Pirate>();

        public static List<Wormhole> wormsPushed = new List<Wormhole>();
        public static List<Wormhole> wormHolesTargetted = new List<Wormhole>();

        public static List<Asteroid> asteroidsPushed = new List<Asteroid>();

        public static List<Pirate> gotHeavy = new List<Pirate>();
        public static List<Pirate> goStick = new List<Pirate>();

        public static bool stopStick = false;
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

            ///*
            game.Debug("Kol od baleivav penimah");
            game.Debug("Nefesh Yehudi homiyah,");
            game.Debug("Ul(e)faatei mizrach kadimah,");
            game.Debug("Ayin leTziyon tzofiyah;");
            game.Debug("");
            game.Debug("Od lo avdah tikvateinu,");
            game.Debug("Hatikvah bat sh(e)not alpayim,");
            game.Debug("Lihyot am chofshi b(e)artzeinu,");
            game.Debug("Eretz-Tziyon virushalayim.");
            game.Debug("");
            //*/

            if (goStick.Any() && !game.GetMyPirateById(goStick.First().Id).IsAlive()) {
                goStick.Clear();
            }

            // Clearing objects
            didTurn.Clear();
            sailToworm.Clear();
            capsulesTargetted.Clear();
            asteroidsPushed.Clear();
            piratesPushed.Clear();
            wormsPushed.Clear();

            // Gettings the mines
            if (game.GetMyCapsules().Any() && game.Turn == 1) {
                game.GetMyCapsules().Where(cap => cap.Holder == null && !mines.Contains(cap.Location)).ToList().ForEach(cap => mines.Add(cap.Location));
            }

            if (game.GetEnemyCapsules().Any() && game.Turn == 1) {
                game.GetEnemyCapsules().Where(cap => cap.Holder == null && !enemyMines.Contains(cap.Location)).ToList().ForEach(cap => enemyMines.Add(cap.Location));
            }

            unemployedPirates = game.GetMyLivingPirates().ToList();
            HandTasks();

            foreach (Pirate pirate in game.GetMyLivingPirates().Where(p => p.StateName != game.STATE_NAME_HEAVY).OrderByDescending(p => tasks[p.Id].Item2.HeavyWeight())) {

                if (tasks[pirate.Id].Item2.HeavyWeight() <= 0) {
                    break;
                }

                var switchWith = game.GetMyLivingPirates().Where(p => p.Id != pirate.Id
                                                              && tasks[p.Id].Item2.HeavyWeight() < tasks[pirate.Id].Item2.HeavyWeight()
                                                              && p.StateName == game.STATE_NAME_HEAVY)
                                                              .OrderBy(p => tasks[p.Id].Item2.HeavyWeight());

                bool shouldNotSwitch = game.__livingAsteroids.Any(a => pirate.Distance(a) < pirate.MaxSpeed * 5) || game.GetEnemyLivingPirates().Any(e => pirate.Distance(e) < pirate.MaxSpeed * 3.5);

                if (shouldNotSwitch) break;

                if (switchWith.Any() && pirate.StateName != game.STATE_NAME_HEAVY) {
                    switchWith.First().SwapStates(pirate);
                    didTurn.Add(switchWith.First().Id);
                    break;
                }

            }

            foreach (KeyValuePair<int, Tuple<TaskType, Task>> pair in tasks.OrderByDescending(pair => pair.Value.Item2.Priority())) {

                try {
                    
                    var preform = pair.Value.Item2.Preform();

                    if (debug) game.Debug(preform);

                } catch (System.Exception e) {
                    game.Debug(e.Message);
                }

            }

            if (stopStick == true) Main.goStick.Clear();
        }


        /// <summary> Calculates what is the best task to asign </summary>
        /// <returns> Tuple with the pirate and the task-type </returns>
        public Tuple<Pirate, TaskType> BestTaskToAssign() {

            var scores = new Dictionary<Tuple<Pirate, TaskType>, double>();

            foreach (Pirate pirate in unemployedPirates) {
                foreach (TaskType taskType in todoTasks) {

                    Task task = TaskTypeToTask(pirate, taskType);
                    double score = task.Bias() + task.GetWeight();

                    scores[new Tuple<Pirate, TaskType>(pirate, taskType)] = score;
                }
            }

            if (scores.Count > 0) {

                var best = scores.OrderByDescending(pair => pair.Value).First();

                if (fullDebug) {
                    
                    (from pirate in unemployedPirates
                     let tasks = from tup in scores.Keys.Where(p => pirate.Equals(p)) select tup.Item2 + " > " + scores[tup]
                     let taskString = tasks.Aggregate((t1, t2) => t1 + "  ||  " + t2)
                     select taskString).ToList().ForEach(game.Debug);

                    game.Debug("Gave: " + best.Key.Item1.Id + " | " + best.Key.Item2 + " at: " + best.Value);
                }

                return best.Key;
            }

            return new Tuple<Pirate, TaskType>(game.GetMyLivingPirates()[0], TaskType.MINER);
        }


        /// <summary> Hands out the tasks according to the BestTaskToAssign() function </summary>
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

                case TaskType.BOOSTER: return new TaskBooster(pirate);
                
                case TaskType.MOLE: return new TaskMole(pirate);
                
                default: return new TaskMiner(pirate);
            }
        }

    }
}