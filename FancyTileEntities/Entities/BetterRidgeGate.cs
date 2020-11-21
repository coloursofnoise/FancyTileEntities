using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FancyTileEntities {
    [CustomEntity("FancyTileEntities/BetterRidgeGate")]
    class BetterRidgeGate : RidgeGate {

        internal string flag;
        private char tileType;
        public BetterRidgeGate(EntityData data, Vector2 offset) 
            : base(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), "objects/ridgeGate") {
            flag = data.Attr("flag", "");

            Remove(Get<Image>());
            tileType = data.Char("tiletype");
            Add(GFX.FGAutotiler.GenerateBox(tileType, data.Width / 8, data.Height / 8).TileGrid);
        }

        // flag handled in FancyRidgeGate
    }
}
