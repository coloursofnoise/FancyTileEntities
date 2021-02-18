using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.FancyTileEntities {
    [CustomEntity("FancyTileEntities/FancyFakeWall")]
    [TrackedAs(typeof(FakeWall))]
    public class FancyFakeWall : FakeWall {
        private DynData<FakeWall> baseData;
        private VirtualMap<char> tileMap;

        public FancyFakeWall(EntityData data, Vector2 offset, EntityID eid)
            : base(eid, data, offset, data.Enum("type", Modes.Wall)) {
            baseData = new DynData<FakeWall>(this);

            tileMap = Extensions.GenerateTileMap(data.Attr("tileData", ""));
            Collider = Extensions.GenerateBetterColliderGrid(tileMap, 8, 8);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(baseData.Get<TileGrid>("tiles"));
            Remove(Get<TileInterceptor>());

            TileGrid tiles;
            if (baseData.Get<Modes>("mode") == Modes.Wall) {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) X / 8 - tileBounds.Left;
                int y = (int) Y / 8 - tileBounds.Top;
                tiles = GFX.FGAutotiler.GenerateOverlay(tileMap, x, y, solidsData).TileGrid;
            } else {
                tiles = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid;
            }
            baseData["tiles"] = tiles;
            Add(baseData.Get<TileGrid>("tiles"));
            Add(new TileInterceptor(baseData.Get<TileGrid>("tiles"), false));
        }
    }
}
