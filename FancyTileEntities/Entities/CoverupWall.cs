using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {
    [CustomEntity("FancyTileEntities/FancyCoverupWall")]
    [TrackedAs(typeof(CoverupWall))]
    public class FancyCoverupWall : CoverupWall {
        private static readonly FieldInfo<TileGrid> f_CoverupWall_tiles;

        private bool blendIn;

        private VirtualMap<char> tileMap;

        static FancyCoverupWall() {
            f_CoverupWall_tiles = typeof(CoverupWall).GetField<TileGrid>("tiles", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public FancyCoverupWall(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height) {
            blendIn = data.Bool("blendIn", true);
            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Collider = GenerateInefficientColliderGrid(tileMap, 8, 8);
        }

        public override void Added(Scene scene) {
            IntPtr ptr = typeof(Entity).GetMethod("Added").MethodHandle.GetFunctionPointer();
            Action<Scene> m_Entity_added = (Action<Scene>) Activator.CreateInstance(typeof(Action<Scene>), this, ptr);
            m_Entity_added(scene);

            if (blendIn) {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (X / 8f) - tileBounds.Left;
                int y = (int) (Y / 8f) - tileBounds.Top;

                f_CoverupWall_tiles[this] = GFX.FGAutotiler.GenerateOverlay(tileMap, x, y, solidsData).TileGrid;
            } else {
                f_CoverupWall_tiles[this] = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid;
            }

            Add(f_CoverupWall_tiles[this]);
            Add(new TileInterceptor(f_CoverupWall_tiles[this], false));
        }
    }
}
