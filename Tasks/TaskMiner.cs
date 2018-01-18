public class TaskMiner : Task
    {

        public static Dictionary<int, Path> paths = new Dictionary<int, Path>();


        public string Preform(Pirate pirate)
        {

            if ((!paths.ContainsKey(pirate.UniqueId) || paths[pirate.UniqueId] == null))
            { //Checks if path exists

                Chunk origin = Chunk.GetChunk(pirate.GetLocation()); //His starting position
                Chunk endgoal = new Chunk();
                
                if (!pirate.HasCapsule())
                    endgoal = Chunk.GetChunk(Punctuation.game.GetMyCapsule().GetLocation()); //His final goal
                else
                    endgoal = Chunk.GetChunk(Punctuation.game.GetMyMotherShip().GetLocation()); //His final goal
                
                paths[pirate.UniqueId] = new Path(origin, endgoal, Path.Algorithm.ASTAR); //Generate a path using AStar
            }

            int ID = pirate.UniqueId;
            Path path = paths[ID];

            if (path.GetChunks().Count > 0)
            {

                Chunk next = path.GetNext();

                if (next != null)
                {

                    if (next.GetLocation().Distance(pirate) < (Chunk.divider / 2))
                    {
                        path.GetChunks().Pop();
                    }

                    pirate.Sail(next.GetLocation());
                    return Utils.GetPirateStatus(pirate, "Sailing to: " + next.ToString());
                }
            }
            return Utils.GetPirateStatus(pirate, "Next is null");
        }


        public int GetWeight(Pirate pirate)
        {

            if (!Punctuation.game.GetMyCapsule().Holder())
            {
                List<MapObject> sortedlist = new List<MapObject>();
                sortedlist = Utils.SoloClosestPair(Skillz.game.GetMyLivingPirates(), Skillz.game.GetMyCapsule());
                numofpirates = Skillz.game.GetAllMyPirates().Length;
                return (numofpirates - sortedlist.IndexOf(pirate)) * (100 / numofpirates);
        }
            else
            {
                if (Punctuation.game.GetMyCapsule().Holder() == pirate)
                    return 100; //he is already in his task
                return 0; // only one miner for this lvl of competition
            }
                
        }

        public int Bias()
        {
            return 10;
        }

    }
