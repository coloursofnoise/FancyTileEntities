using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {
    public class BGTileEntity : Entity {
        public BGTileEntity(Vector2 position, string tileData, int? seed = null)
            : base(position) {
            Depth = Depths.BGTerrain;

            Calc.PushRandom(seed ?? Calc.Random.Next());
            VirtualMap<char> tileMap = GenerateTileMap(tileData);
            Autotiler.Generated generated = GFX.BGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour));
            Add(generated.TileGrid);
            Add(generated.SpriteOverlay);
            Calc.PopRandom();
        }

        public BGTileEntity(Vector2 position, string tileData, VirtualMap<char> levelData, Point origin, int? seed = null)
            : base(position) {
            Depth = Depths.BGTerrain;

            Calc.PushRandom(seed ?? Calc.Random.Next());
            VirtualMap<char> tileMap = GenerateTileMap(tileData);
            Autotiler.Generated generated = GFX.BGAutotiler.GenerateOverlay(tileMap, origin.X, origin.Y, levelData, default(Autotiler.Behaviour));
            Add(generated.TileGrid);
            Add(generated.SpriteOverlay);
            Calc.PopRandom();
        }
    }
}