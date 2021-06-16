using System;

namespace Celeste.Mod.FancyTileEntities {
    public class PositionRandom : Random {

        private int x = 1;
        private int y = 1;

        private long seed;

        public PositionRandom(long seed) {
            this.seed = seed;
        }

        public void SetPosition(int x, int y) {
            this.x = x;
            this.y = y;
        }

        protected override double Sample() {
            long a = x + 1;
            long b = y + 1;

            a = (a * 171) % 30269;
            b = (b * 172) % 30307;
            long c = (seed * 170) % 30323;

            return (a / 30269.0 + b / 30307.0 + c / 30323.0) % 1;
        }
    }
}
