namespace Hydra {

    using System.Collections.Generic;
    using Pirates;

    public enum EmitterType { PASSABLE, DANGEROUS, IMPASSBLE };


	public abstract class Emitter {


        public virtual List<Tuple<int, EmitterType>> Radius() => new List<Tuple<int, EmitterType>>();


        public virtual Location Center() => new Location(0,0);

    }
}
