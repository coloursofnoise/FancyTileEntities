using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.FancyTileEntities {
    [CustomEntity("FancyTileEntities/FancyRidgeGate")]
    class FancyRidgeGate : RidgeGate {

        protected string flag;
        protected TileGrid tiles;

        private VirtualMap<char> tileMap;
        
        public FancyRidgeGate(EntityData data, Vector2 offset) 
            : base(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), "objects/ridgeGate") {
            flag = data.Attr("flag", "");

            Remove(Get<Image>());
            tileMap = Extensions.GenerateTileMap(data.Attr("tileData", ""));
            Collider = Extensions.GenerateInefficientColliderGrid(tileMap, 8, 8);
            Add(tiles = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid);
        }

        internal static IEnumerator EnterSequence(On.Celeste.RidgeGate.orig_EnterSequence orig, RidgeGate self, Vector2 moveTo) {
            if (self is FancyRidgeGate gate) {
                Level level = self.Scene as Level;
                if (!string.IsNullOrEmpty(gate.flag) && !level.Session.GetFlag(gate.flag)) {
                    yield break;
                }
            }

            if (self is BetterRidgeGate betterGate) {
                Level level = self.Scene as Level;
                if (!string.IsNullOrEmpty(betterGate.flag) && !level.Session.GetFlag(betterGate.flag)) {
                    yield break;
                }
            }

            IEnumerator enumerator = orig(self, moveTo);
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }
}
