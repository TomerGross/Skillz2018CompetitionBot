using System.Collections.Generic;
using Pirates;

namespace Hydra {

    public class Node {

        public static PirateGame game = Main.game;
        public static List<Node> nodes;
        public static List<Emitter> emitters;

        static List<Node> toAdd;

        public static void GenerateMap() {

            nodes = new List<Node>();
            toAdd = new List<Node>();
            emitters = new List<Emitter>();

            foreach (Pirate pirate in game.GetEnemyLivingPirates()) {
                emitters.Add(new EmitterPirate(pirate));
            }

            nodes.Add(new Node(0, 0, game.Cols, game.Rows));
            nodes[0].Split();
            nodes.AddRange(toAdd);
            nodes.RemoveAll(n => n.children.Count != 0);

            game.Debug("NODE COUNT: " + nodes.Count);
            game.Debug("NODE EMITTER: " + emitters.Count);
        }


        readonly int row, col, rows, cols;
        List<Node> children;


        protected Node(int row, int col, int rows, int cols) {

            this.row = row;
            this.col = col;
            this.rows = rows;
            this.cols = cols;
            children = new List<Node>();
        }


        protected void Split() {

            if (cols <= game.Cols / 20 && rows <= game.Rows / 20) {
                // game.Debug(cols);
                return;
            }

            if (children.Count == 0 && emitters.Exists(Colliding)) {

                int hr = rows / 2;
                int hc = cols / 2;

                children.Add(new Node(row, col, hr, hc));
                children.Add(new Node(row, col + hc, hr, hc));
                children.Add(new Node(row + hr, col + hc, hr, hc));
                children.Add(new Node(row + hr, col, hr, hc));

                toAdd.AddRange(children);

                children.ForEach(child => child.Split());
            }
        }


        // RECT COLLISION https://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection
        public bool Colliding(Emitter emitter) {

            foreach (Tuple<int, EmitterType> tuple in emitter.Radius()) {

                int distanceX = System.Math.Abs(emitter.Center().Col - Center().Col);
                int distanceY = System.Math.Abs(emitter.Center().Row - Center().Row);

                // if (distanceX > cols / 2 + tuple.Item1) { return false; }
                // if (distanceY > rows / 2 + tuple.Item1) { return false; }

                if (distanceX <= cols / 2) { return true; }
                if (distanceY <= rows / 2) { return true; }

                int cornerDistanceSquared = (int)(System.Math.Pow(distanceX - cols / 2, 2) + System.Math.Pow(distanceY - rows / 2, 2));
                if (cornerDistanceSquared <= System.Math.Pow(tuple.Item1, 2)) {
                    return true;
                }
            }

            return false;
        }


        public Location Center() { //Gets the center of the node
            return new Location(row + (rows / 2), col + (cols / 2));
        }

    }




}