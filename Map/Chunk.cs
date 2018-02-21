using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Hydra {

    public class Chunk {

        public static int n = 20; //Number of chunks
        public static int size = Main.game.Cols / n;

        public static Chunk[,] chunks = new Chunk[n, n]; //Keeps track of all the chunks


        public static Chunk GetChunk(Location loc) { //Returns a chunkk

            int y = loc.Row / size;
            int x = loc.Col / size;

            if (x >= n || y >= n || y < 0 || x < 0) {
                return chunks[0, 0];
            }

            if (chunks[y, x] != null) {
                return chunks[y, x];
            }

            return new Chunk(x, y);
        }


        public static Chunk GetChunk(int x, int y) { //Returns a chunk from an index

            if (chunks[y, x] != null) {

                return chunks[y, x];
            }

            return new Chunk(x, y);
        }

        //---------------[ End of static functions and vars ]--------

        readonly int X, Y;
        readonly Dictionary<int, List<Chunk>> neighbors; //Keeps track of neighbors and removes the need to recalculate


        protected Chunk(int X, int Y) { //This should only be used by GetChunk methods, never by us.

            this.Y = Y;
            this.X = X;

            neighbors = new Dictionary<int, List<Chunk>>();

            chunks[Y, X] = this; //Registers the chunk
        }


        public Location GetLocation() => new Location((Y * size) + (size / 2), (X * size) + (size / 2));


        public override string ToString() => "[Y: " + Y + ", X:" + X + "]";


        public int Distance(GameObject to) => GetLocation().Distance(to);


        public int Distance(Location to) => GetLocation().Distance(to);


        public int Distance(Chunk to) => GetLocation().Distance(to.GetLocation());


        public Tuple<int, Location> GetOptimalSailLocation(Location goal, List<Trait> traits) {

            if (GetChunk(goal) == this)
                return new Tuple<int, Location>(0, goal);

            var wormsInChunk = Main.game.GetAllWormholes().Where(w => w.InRange(GetLocation(), w.WormholeRange + size * 2)).OrderBy(GetLocation().Distance);

            if (wormsInChunk.Any()) {

                var worm = wormsInChunk.First();
                int teleportDistance = worm.Partner.Distance(goal) + Distance(worm);

                if (teleportDistance < Distance(goal))
                    return new Tuple<int, Location>(teleportDistance, worm.Location);
            }

            return new Tuple<int, Location>(Distance(goal), GetLocation());
        }


        public List<Pirate> GetEnemyPirates() {
            // TODO Gets every living pirate that is on the chunk, needs to be reworked

            var list = new List<Pirate>();

            foreach (Pirate enemy in Main.game.GetEnemyLivingPirates()) {
                if (enemy.Distance(GetLocation()) < size / 2) {

                    list.Add(enemy);
                }
            }

            return list;
        }


        public List<Chunk> GetNeighbors(int level) { //The level is the distance of neighbors it should get
                                                     //0 will return only adjacent chunks
            if (neighbors.ContainsKey(level)) {
                return neighbors[level];
            }

            var list = new List<Chunk>();

            for (int rx = (X - (level + 1)); rx <= (X + (level + 1)); rx++) {
                for (int ry = (Y - (level + 1)); ry <= (Y + (level + 1)); ry++) {
                    if (rx >= 0 && ry >= 0 && rx < n && ry < n) {

                        list.Add(GetChunk(rx, ry));
                    }
                }
            }

            neighbors[level] = list;
            return list;
        }

    }

}

