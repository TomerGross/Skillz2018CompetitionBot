using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Pirates;

namespace Hydra {

    public class AStar {

        //The algorithm: 
        //https://en.wikipedia.org/wiki/A*_search_algorithm

        readonly Location origin, endgoal;
        readonly List<Trait> traits;
        readonly Stack pathStack;

        List<Chunk> closedSet, openSet;
        Dictionary<Chunk, int> fScore, gScore;
        Dictionary<Chunk, Location> optimalLocation;
        Dictionary<Chunk, Chunk> cameFrom;


        public AStar(Location origin, Location endgoal, List<Trait> traits) {

            this.traits = traits;
            this.origin = origin;
            this.endgoal = endgoal;

            fScore = new Dictionary<Chunk, int>();
            gScore = new Dictionary<Chunk, int>();
            cameFrom = new Dictionary<Chunk, Chunk>();
            optimalLocation = new Dictionary<Chunk, Location>();
            closedSet = new List<Chunk>();
            openSet = new List<Chunk>();

            pathStack = Calculate();
        }


        public Stack GetPathStack() => pathStack;


        protected Stack Calculate() {

            //Sets every chunk cost to INFINITY
            for (int x = 0; x < Chunk.n; x++) { 
                for (int y = 0; y < Chunk.n; y++) {
                    Chunk chunk = Chunk.GetChunk(x, y);
                    fScore[chunk] = int.MaxValue;
                    gScore[chunk] = int.MaxValue;
                }
            }

            fScore[Chunk.GetChunk(origin)] = 0; //Sets the first chunks scores so it will be picked
            gScore[Chunk.GetChunk(origin)] = 1;
            openSet.Add(Chunk.GetChunk(origin));

            for (int count = 0; count < Chunk.n * Chunk.n; count++) {

                Chunk current = openSet.OrderBy(chunk => fScore[chunk]).First();
                Location optiomalCurrent = current.GetOptimalSailLocation(endgoal).Item2;

                openSet.Remove(current);
                closedSet.Add(current);

                if (current == Chunk.GetChunk(endgoal)) 
                    break;
                
                int range = 0;
                if (Main.game.GetActiveWormholes().Any(wh => current.Distance(wh) <= Chunk.size * 4)) 
                    range = current.Distance(Main.game.GetActiveWormholes().OrderBy(current.Distance).First()) / Chunk.size;
                

                foreach (Chunk neighbor in current.GetNeighbors(range)) {

                    if (closedSet.Contains(neighbor)) 
                        continue;
                    
                    if (!openSet.Contains(neighbor)) 
                        openSet.Add(neighbor);
                    

                    var optiomal = neighbor.GetOptimalSailLocation(endgoal);
                    int g = gScore[current] - (Main.game.Cols - optiomal.Item1);

                    foreach (Trait trait in traits) 
                        g += trait.Cost(neighbor);
                    
                    if (g >= gScore[neighbor]) 
                        continue;
                    
                    optimalLocation[neighbor] = optiomal.Item2;
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = g;
                    fScore[neighbor] = gScore[neighbor] + optiomal.Item1;
                }
            }

            Chunk trace = Chunk.GetChunk(endgoal);
            var p = new Stack();

            while (cameFrom.ContainsKey(trace)) {

                p.Push(optimalLocation[trace]);
                trace = cameFrom[trace];
            }

            return p;
        }
    }
}
