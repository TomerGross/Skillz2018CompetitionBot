using System;
using System.Collections.Generic;
using System.Text;
using Pirates;

namespace Skillz
{
    class TaskEscort : Task
    {
        public static Dictionary<int, Path> paths = new Dictionary<int, Path>();
        int radius = 1000; //Max of distance
        public string Preform(Pirate pirate)
        {

            if ((!paths.ContainsKey(pirate.UniqueId) || paths[pirate.UniqueId] == null))
            { //Checks if path exists

                Chunk origin = Chunk.GetChunk(pirate.GetLocation()); //His starting position

                if (Skillz.game.GetMyCapsule().Holder().Distance(pirate) >= radius)
                    Chunk endgoal = Chunk.GetChunk(Skillz.game.GetMyCapsule().Holder().GetLocation()); //His final goal
                else
                    Chunk endgoal = Chunk.GetChunk(Skillz.game.GetMyMotherShip().GetLocation()); //His final goal

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

            if (!Skillz.game.GetMyCapsule().Holder() == pirate)
            {
                List<MapObject> sortedlist = new List<MapObject>();
                sortedlist = Utils.SoloClosestPair(Skillz.game.GetMyLivingPirates(), Skillz.game.GetMyCapsule().Holder());
                return (sortedlist.IndexOf(pirate) + 1) * (100 / Skillz.game.GetAllMyPirates().Length);
            }

            return 0;
        }


        public int Bias()
        {
            if (!Skillz.game.GetMyCapsule().Holder())
                return 0;
            else
                return 5;
        }

    }




}
}
