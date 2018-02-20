namespace Hydra {

    using Pirates;
    using System.Collections.Generic;

    public class EmitterPirate : Emitter {

        readonly Pirate pirate;
        readonly List<Tuple<int, EmitterType>> radius;


        public EmitterPirate(Pirate pirate) {

            this.pirate = pirate;

            radius = new List<Tuple<int, EmitterType>>(new List<Tuple<int, EmitterType>> {
                new Tuple<int, EmitterType>(pirate.PushRange * 2, EmitterType.DANGEROUS),
                new Tuple<int, EmitterType>(pirate.PushRange, EmitterType.IMPASSBLE)});
        }


        override public List<Tuple<int, EmitterType>> Radius() => radius;

        override public Location Center() => pirate.Location;

    }
}