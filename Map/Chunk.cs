using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class Chunk {

        /// <summary> Number of chunks </summary>
        public static int n = 20; 
        public static int size = Main.game.Cols / n;

        /// <summary> Keeps track of all the chunks </summary>
        public static Chunk[,] chunks = new Chunk[n, n]; 


        /// <summary> Gets a chunk from a given location</summary>
        /// <returns> Either a cached or a new chunk </returns>
        public static Chunk GetChunk(Location loc) { 

            int y = loc.Row / size;
            int x = loc.Col / size;

            if (x >= n || y >= n || y < 0 || x < 0) 
                return chunks[0, 0];
        
            if (chunks[y, x] != null) 
                return chunks[y, x];
            
            return new Chunk(x, y);
        }


        /// <summary> Gets a chunk by it's index </summary>
        /// <returns> Either a cached or a new chunk </returns>
        public static Chunk GetChunk(int x, int y) { 

            if (chunks[y, x] != null) 
                return chunks[y, x];

            return new Chunk(x, y);
        }


        readonly int X, Y;
        readonly Dictionary<int, List<Chunk>> cahchedNeighbors; 


        protected Chunk(int X, int Y) { 

            this.Y = Y;
            this.X = X;

            cahchedNeighbors = new Dictionary<int, List<Chunk>>();
            chunks[Y, X] = this; 
        }


        public override string ToString() => "[Y: " + Y + ", X:" + X + "]";


        public Location GetLocation() => new Location((Y * size) + (size / 2), (X * size) + (size / 2));


        public int Distance(GameObject to) => GetLocation().Distance(to);


        public int Distance(Location to) => GetLocation().Distance(to);


        public int Distance(Chunk to) => GetLocation().Distance(to.GetLocation());


        public List<Pirate> GetPirates(List<Pirate> pirates) => pirates.Where(p => GetChunk(p.GetLocation()) == this).ToList();


        public List<Pirate> GetMyPirates() => GetPirates(Main.game.GetMyLivingPirates().ToList());


        public List<Pirate> GetEnemyPirates()=> GetPirates(Main.game.GetEnemyLivingPirates().ToList());  


        /// <summary> Gets the optimal sail location </summary>
        /// <returns> Either chunk center or wormhole</returns>
        /// <param name="goal"> Where do you want to go </param>
        public Tuple<int, Location> GetOptimalSailLocation(Location goal) {

            if (GetChunk(goal) == this) {
                return new Tuple<int, Location>(0, goal);
            }

            var wormsInChunk = Main.game.GetAllWormholes().Where(w => w.InRange(GetLocation(), w.WormholeRange + size * 2)).OrderBy(GetLocation().Distance);

            if (wormsInChunk.Any()) {

                var worm = wormsInChunk.First();
                int teleportDistance = worm.Partner.Distance(goal) + Distance(worm) + worm.TurnsToReactivate * Main.game.PirateMaxSpeed;

                if (teleportDistance < Distance(goal))
                    return new Tuple<int, Location>(teleportDistance, worm.Location);
            }

            return new Tuple<int, Location>(Distance(goal), GetLocation());
        }


        /// <summary> Gets the neighbor chunks within a range </summary>
        /// <param name="range"> If the range is 0 it will return adjecent chunks only</param>
        public List<Chunk> GetNeighbors(int range) {

            if (cahchedNeighbors.ContainsKey(range)) return cahchedNeighbors[range];

            var list = new List<Chunk>();

            for (int rx = (X - (range + 1)); rx <= (X + (range + 1)); rx++) {
                for (int ry = (Y - (range + 1)); ry <= (Y + (range + 1)); ry++) {
                    if (rx >= 0 && ry >= 0 && rx < n && ry < n) {

                        list.Add(GetChunk(rx, ry));
                    }
                }
            }

            cahchedNeighbors[range] = list;
            return list;
        }

    }

}

