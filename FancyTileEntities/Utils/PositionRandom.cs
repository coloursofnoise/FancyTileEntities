using System;

namespace Celeste.Mod.FancyTileEntities {
    public class PositionRandom : Random {

        public enum Modes {
            /// <summary>
            /// Does not use position at all (<see cref="Random.Sample"/>).
            /// </summary>
            None,
            /// <summary>
            /// Uses the Witchmann-Hill algorithm (<see cref="PositionRandom.WichmannHill(int, int, int)"/>).
            /// </summary>
            WichmannHill,
            /// <summary>
            /// Uses a port of the Cogwheel tile prng (<see cref="PositionRandom.Cogwheel(int, int)"/>).
            /// </summary>
            Cogwheel,
        }

        private int x = 1;
        private int y = 1;

        private Modes mode;
        private int seed;

        public PositionRandom(Modes mode, int seed)
            : base(seed) {
            this.mode = mode;
            this.seed = seed;
        }

        public void SetPosition(int x, int y) {
            this.x = x;
            this.y = y;
        }

        protected override double Sample() {
            return mode switch {
                Modes.WichmannHill => WichmannHill(x, y, seed),
                Modes.Cogwheel => Cogwheel(x, y),
                _ => base.Sample(),
            };
        }

        public static double WichmannHill(int x, int y, int seed) {
            // Bounds checking, changes seed for different values
            int mod = x / 30000 + y / 30000;
            mod += (x < 1 ? 1 : 0) + (y < 1 ? 1 : 0) + (seed < 1 ? 1 : 0);
            seed = (Math.Abs(seed) + mod) % 30000 + 1;
            x = Math.Abs(x);
            y = Math.Abs(y);
            x %= 30000 + 1;
            y %= 30000 + 1;

            long a = (x * 171) % 30269;
            long b = (y * 172) % 30307;
            long c = (seed * 170) % 30323;

            return (a / 30269.0 + b / 30307.0 + c / 30323.0) % 1;
        }

        // Derived by jade from https://gitlab.com/0x0ade/everest.cogwheel/-/blob/master/js/components/utils.js#L122
        public static double Cogwheel(int x, int y) {
            int mod = 1;
            int a = ((x * 71317 + mod) << 16) | (y * 51713 + mod);
            a = (48271 * a) % int.MaxValue;
            int b = (48271 * a) % int.MaxValue;
            int c = (48271 * b) % int.MaxValue;

            double d =
                ((a & int.MaxValue) / (int.MaxValue + 1f) * mod) +
                ((b & int.MaxValue) / (int.MaxValue + 1f) * mod) +
                ((c & int.MaxValue) / (int.MaxValue + 1f) * mod);
            return ((d % mod) + 2 * mod) % mod;
        }

    }
}
