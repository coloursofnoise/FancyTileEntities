using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {
    [CustomEntity("FancyTileEntities/FancySolidTiles")]
    [TrackedAs(typeof(SolidTiles))]
    public class FancySolidTiles : SolidTiles {
        private static readonly FieldInfo<VirtualMap<char>> f_SolidTiles_tileTypes;

        private bool blendEdges;
        private int seed;
        private EntityID id;
        private bool loadGlobally;
        private string bgTileString;

        static FancySolidTiles() {
            f_SolidTiles_tileTypes = typeof(SolidTiles).GetField<VirtualMap<char>>("tileTypes", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public FancySolidTiles(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, GenerateTileMap(data.Attr("tileData", ""))) {
            blendEdges = data.Bool("blendEdges");

            seed = data.Int("randomSeed");

            bgTileString = data.Attr("tileDataBG");

            if (data.Bool("loadGlobally"))
                loadGlobally = true;
            else
                RemoveTag(Tags.Global);
            this.id = id;

            Remove(Tiles);
            Remove(AnimatedTiles);

            VirtualMap<char> tileMap = f_SolidTiles_tileTypes[this];
            for (int x = 0; x < tileMap.Columns; x++) {
                for (int y = 0; y < tileMap.Rows; y++) {
                    if (tileMap.AnyInSegmentAtTile(x, y) && tileMap[x, y] != '0') {
                        Add(new LightOcclude(new Rectangle(x * 8, y * 8, 8, 8)));
                    }
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            int tileSeed = seed != 0 ? seed : Calc.Random.Next();
            Calc.PushRandom(tileSeed);

            Autotiler.Generated generated;
            if (blendEdges) {
                Level level = scene as Level;
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) X / 8 - tileBounds.Left;
                int y = (int) Y / 8 - tileBounds.Top;
                generated = GFX.FGAutotiler.GenerateOverlay(f_SolidTiles_tileTypes[this], x, y, solidsData, default);
                scene.Add(new BGTileEntity(Position, bgTileString, level.BgData, new Point(x, y), tileSeed));
            } else {
                generated = GFX.FGAutotiler.GenerateMap(f_SolidTiles_tileTypes[this], default(Autotiler.Behaviour));
                scene.Add(new BGTileEntity(Position, bgTileString, tileSeed));
            }
            Calc.PopRandom();

            Tiles = generated.TileGrid;
            Tiles.VisualExtend = 1;

            Add(Tiles);
            Add(AnimatedTiles = generated.SpriteOverlay);
        }
    }
}
