using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Hydra {

	public class MTDSlite{
		
		//Moving Target - DStar Light by Xiaoxun Sun, William Yeoh & Sven Koenig
		//http://idm-lab.org/bib/abstracts/papers/aamas10a.pdf
		
		readonly Chunk origin, end; 
        readonly List<Trait> traits; 
		readonly Stack pathStack;

		Dictionary<Chunk, int> fScore;		
		Dictionary<Chunk, int> gScore;
        Dictionary<Chunk, Chunk> cameFrom;
        Stack closedSet, openSet;
		double Km;
		
				
		public MTDSlite(List<Trait> traits, Chunk origin, Chunk end) {

			this.traits = traits;
			this.origin = origin;
			this.end = end;

			Km = 0;
			
			//Initiating everything 
			fScore = new Dictionary<Chunk, int>();
			gScore = new Dictionary<Chunk, int>();
			cameFrom = new Dictionary<Chunk, Chunk>();
			closedSet = new Stack();
			openSet = new Stack();
			
			//pathStack = calculate();
		}

	
		public int TraitCost(Chunk s, Chunk st) {

			int c = 0;
			
			foreach (Trait trait in traits){
				c += trait.Cost(st);
			}

			return c;
		}
		
		
		public int rhs(Chunk s){
			return s.GetNeighbors(0).Min(st => (gScore[st] + TraitCost(s, st))); //Picks the cheapest neighbor chunk
		}
		
		
		public double[] CalculateKeys(Chunk s) {
			
			var keys = new double[2];
            keys[0] = System.Math.Min(gScore[s], rhs(s)) + end.Distance(s) + Km;
            keys[1] = System.Math.Min(gScore[s], rhs(s));
           
			return keys;
		}

		public bool KeyIsSmaller(Chunk u, Chunk s) {

			var Ku = CalculateKeys(u);
			var Ks = CalculateKeys(s);

			if (Ku[0] < Ks[0] || Ku[1] < Ks[1]{
				return true;
			}

			return false;
		}
		
		public void ComputeCostMinimalPath() {

			while (KeyIsSmaller((Chunk) openSet.Peek(), origin) || rhs(origin) > gScore[origin]) {
				
				
			}
		}
	
	}
}
