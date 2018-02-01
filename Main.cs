using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class Main : IPirateBot {


        //---------------[ Main variables ]-----------
        public static PirateGame game;
        public static List<int> didTurn = new List<int>();
        //public static List<Pirate> enemypirates = game.GetEnemyLivingPirates().ToList(); 
        //--------------------------------------------


        //---------------[ Mines ]--------------------
        public static List<Location> mines;
        public static List<Location> enemyMines;
        public static int maxMiners = 2;
        //--------------------------------------------


        //---------------[ Task mangment ]-------------
        public static Dictionary<int, TaskType> tasks = new Dictionary<int, TaskType>();
        public static List<Pirate> unemployedPirates = new List<Pirate>();
        public readonly List<TaskType> todoTasks = new List<TaskType>(new List<TaskType>
        { TaskType.MINER, TaskType.ESCORT, TaskType.BOOSTER, TaskType.BERSERKER, TaskType.MOLE});
        public static int alivePirateCount = 0;
        //--------------------------------------------


        public void DoTurn(PirateGame game) {

            Main.game = game;

            // Clearing objects
            tasks.Clear();
            didTurn.Clear();
            alivePirateCount = game.GetMyLivingPirates().Count();
            unemployedPirates = game.GetMyLivingPirates().ToList();

            // Gettings the mines
            game.GetMyCapsules().Where(cap => cap.Holder == null && !mines.Contains(cap.Location)).ToList().ForEach(cap => mines.Add(cap.Location));
            game.GetEnemyCapsules().Where(cap => cap.Holder == null && !enemyMines.Contains(cap.Location)).ToList().ForEach(cap => enemyMines.Add(cap.Location));

            GiveTasks();

            foreach (KeyValuePair<int, TaskType> pair in tasks) {
                game.Debug(taskTypeToTask(game.GetMyPirateById(pair.Key), pair.Value).Preform());
            }
        }


        public Dictionary<Tuple<Pirate, TaskType>, double> GetCurrentCosts() {

            var scores = new Dictionary<Tuple<Pirate, TaskType>, double>();

            foreach (Pirate pirate in unemployedPirates) {
                foreach (TaskType taskType in todoTasks) {

                    Task task = taskTypeToTask(pirate, taskType);
                    double score = task.Bias() + task.GetWeight();

                    scores[new Tuple<Pirate, TaskType>(pirate, taskType)] = score;
                }
            }

            game.Debug("SCORES LEN: " + scores.Count);

            
            foreach (Pirate pirate in unemployedPirates) {
                var ptasks = from tup in scores.Keys.ToList().Where(tup => tup.Item1.Id == pirate.Id) select tup.Item2.ToString() + " > " + scores[tup] + "  ||  ";
                string s = "";
                ptasks.ToList().ForEach(str => s += str);
                game.Debug(pirate.Id + " | " + s);
            }

            return scores;
        }


        public void GiveTasks() {

            for (int i = 0; i < alivePirateCount; i++) {

                var scores = GetCurrentCosts();
                var sorted = scores.Keys.OrderByDescending(key => scores[key]);

                Pirate pirate = sorted.First().Item1;
                TaskType taskType = sorted.First().Item2;

                tasks[pirate.Id] = taskType;
                unemployedPirates.Remove(pirate);

                game.Debug("Gave: " + pirate.Id + " | " + taskType + " at cost: " + scores[sorted.First()]);
            }
        }


        public Task taskTypeToTask(Pirate pirate, TaskType task) {

            switch (task) {
                case TaskType.BERSERKER:
                    return new TaskBerserker(pirate);

                case TaskType.ESCORT:
                    return new TaskEscort(pirate);
                    
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
