using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyDashBlock")]
    [TrackedAs(typeof(DashBlock))]
    public class FancyDashBlock : DashBlock {
        private static readonly FieldInfo<bool> f_DashBlock_blendIn;
        private static readonly FieldInfo<bool> f_DashBlock_permanent;
        private VirtualMap<char> tileMap;

        static FancyDashBlock() {
            f_DashBlock_blendIn = typeof(DashBlock).GetField<bool>("blendIn", BindingFlags.NonPublic | BindingFlags.Instance);
            f_DashBlock_permanent = typeof(DashBlock).GetField<bool>("permanent", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public FancyDashBlock(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, '3', data.Width, data.Height, data.Bool("blendin", false), data.Bool("permanent", true), data.Bool("canDash", true), id) {
            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Collider = GenerateInefficientColliderGrid(tileMap, 8, 8);
        }

        public override void Awake(Scene scene) {
            IntPtr ptr = typeof(Solid).GetMethod("Awake").MethodHandle.GetFunctionPointer();
            Action<Scene> awake_Solid = (Action<Scene>) Activator.CreateInstance(typeof(Action<Scene>), this, ptr);
            awake_Solid(scene);

            TileGrid tileGrid;
            if (!f_DashBlock_blendIn[this]) {
                tileGrid = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid;
                for (int x = 0; x < tileMap.Columns; x++) {
                    for (int y = 0; y < tileMap.Rows; y++) {
                        if (tileMap.AnyInSegmentAtTile(x, y) && tileMap[x, y] != '0') {
                            Add(new LightOcclude(new Rectangle(x * 8, y * 8, 8, 8)));
                        }
                    }
                }
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (X / 8f) - tileBounds.Left;
                int y = (int) (Y / 8f) - tileBounds.Top;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileMap, x, y, solidsData).TileGrid;
                Add(new EffectCutout());
                Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, true));
            if (CollideCheck<Player>()) {
                RemoveSelf();
            }
        }

        internal static void Break(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool_bool orig, DashBlock self, Vector2 from, Vector2 direction, bool playSound, bool playDebrisSound) {
            if (self is FancyDashBlock) {
                FancyDashBlock block = (self as FancyDashBlock);
                if (playSound)
                    Audio.Play(SFX.game_gen_wallbreak_stone, block.Position);

                for (int x = 0; x < block.Width / 8f; x++) {
                    for (int y = 0; y < block.Height / 8f; y++) {
                        if (block.tileMap.AnyInSegmentAtTile(x, y) && block.tileMap[x, y] != '0') {
                            block.Scene.Add(Engine.Pooler.Create<Debris>().Init(block.Position + new Vector2(4 + x * 8, 4 + y * 8), block.tileMap[x, y], playDebrisSound).BlastFrom(from));
                        }
                    }
                }
                block.Collidable = false;
                if (f_DashBlock_permanent[block]) {
                    block.RemoveAndFlagAsGone();
                } else {
                    block.RemoveSelf();
                }
            } else
                orig(self, from, direction, playSound, playDebrisSound);
        }

        #region SoundIndex

        public override int GetLandSoundIndex(Entity entity) {
            int idx = SurfaceSoundIndexAt(entity.BottomCenter + Vector2.UnitY * 4f);
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomLeft + Vector2.UnitY * 4f);
            }
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomRight + Vector2.UnitY * 4f);
            }
            return idx;
        }

        public override int GetWallSoundIndex(Player player, int side) {
            int idx = SurfaceSoundIndexAt(player.Center + Vector2.UnitX * side * 8f);
            if (idx < 0) {
                idx = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, -6f));
            }
            if (idx < 0) {
                idx = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, 6f));
            }
            return idx;
        }

        public override int GetStepSoundIndex(Entity entity) {
            int idx = SurfaceSoundIndexAt(entity.BottomCenter + Vector2.UnitY * 4f);
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomLeft + Vector2.UnitY * 4f);
            }
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomRight + Vector2.UnitY * 4f);
            }
            return idx;
        }

        private int SurfaceSoundIndexAt(Vector2 readPosition) {
            int x = (int) ((readPosition.X - X) / 8f);
            int y = (int) ((readPosition.Y - Y) / 8f);
            if (x >= 0 && y >= 0 && x < tileMap.Columns && y < tileMap.Rows) {
                char c = tileMap[x, y];
                if (c == 'k') {
                    return CoreTileSurfaceIndex();
                }
                if (c != '0' && SurfaceIndex.TileToIndex.ContainsKey(c)) {
                    return SurfaceIndex.TileToIndex[c];
                }
            }
            return -1;
        }

        private int CoreTileSurfaceIndex() {
            Level level = SceneAs<Level>();
            if (level.CoreMode == Session.CoreModes.Hot) {
                return 37;
            }
            if (level.CoreMode == Session.CoreModes.Cold) {
                return 36;
            }
            return 3;
        }

        #endregion

    }
}
