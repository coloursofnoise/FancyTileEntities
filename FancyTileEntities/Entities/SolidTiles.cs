using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {
    [CustomEntity("FancyTileEntities/FancySolidTiles")]
    [TrackedAs(typeof(SolidTiles))]
    class FancySolidTiles : SolidTiles {
        private static readonly FieldInfo<VirtualMap<char>> f_SolidTiles_tileTypes;

        private bool blendEdges;
        private int seed;
        private EntityID id;
        private bool loadGlobally;

        static FancySolidTiles() {
            f_SolidTiles_tileTypes = typeof(SolidTiles).GetField<VirtualMap<char>>("tileTypes", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public FancySolidTiles(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, GenerateTileMap(data.Attr("tileData", ""))) {
            blendEdges = data.Bool("blendEdges");

            seed = data.Int("randomSeed");

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

            if (seed != 0)
                Calc.PushRandom(seed);
            else
                Calc.PushRandom(Calc.Random.Next());

            Autotiler.Generated generated;
            if (blendEdges) {
                Level level = scene as Level;
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) X / 8 - tileBounds.Left;
                int y = (int) Y / 8 - tileBounds.Top;
                generated = GFX.FGAutotiler.GenerateOverlay(f_SolidTiles_tileTypes[this], x, y, solidsData, default);
            } else {
                generated = GFX.FGAutotiler.GenerateMap(f_SolidTiles_tileTypes[this], default(Autotiler.Behaviour));
            }
            Calc.PopRandom();

            Tiles = generated.TileGrid;
            Tiles.VisualExtend = 1;

            Add(Tiles);
            Add(AnimatedTiles = generated.SpriteOverlay);
        }
    }
}
