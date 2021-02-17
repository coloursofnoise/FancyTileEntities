using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyCrumbleWallOnRumble")]
    [TrackedAs(typeof(CrumbleWallOnRumble))]
    public class FancyCrumbleWallOnRumble : CrumbleWallOnRumble {
        private static readonly FieldInfo<bool> f_CrumbleWallOnRumble_blendIn;
        private VirtualMap<char> tileMap;

        static FancyCrumbleWallOnRumble() {
            f_CrumbleWallOnRumble_blendIn = typeof(CrumbleWallOnRumble).GetField<bool>("blendIn", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public FancyCrumbleWallOnRumble(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, data.Char("tiletype", 'm'), data.Width, data.Height, data.Bool("blendin", false), data.Bool("persistent", false), id) {
            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            ColliderList colliders = GenerateBetterColliderGrid(tileMap, 8, 8);
            AddLightOcclude(this, colliders);
            Collider = colliders;
        }

        public override void Awake(Scene scene) {
            IntPtr ptr = typeof(Solid).GetMethod("Awake").MethodHandle.GetFunctionPointer();
            Action<Scene> awake_Solid = (Action<Scene>) Activator.CreateInstance(typeof(Action<Scene>), this, ptr);
            awake_Solid(scene);

            TileGrid tileGrid;
            if (!f_CrumbleWallOnRumble_blendIn[this]) {
                tileGrid = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid;
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (X / 8f) - tileBounds.Left;
                int y = (int) (Y / 8f) - tileBounds.Top;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileMap, x, y, solidsData).TileGrid;
                Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, true));
            if (CollideCheck<Player>()) {
                RemoveSelf();
            }
        }

        internal static void Break(On.Celeste.CrumbleWallOnRumble.orig_Break orig, CrumbleWallOnRumble self) {
            if (self is FancyCrumbleWallOnRumble) {
                if (self.Collidable && self.Scene != null) {
                    FancyCrumbleWallOnRumble block = (self as FancyCrumbleWallOnRumble);
                    Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", block.Position);
                    block.Collidable = false;
                    for (int x = 0; x < block.Width / 8f; x++) {
                        for (int y = 0; y < block.Height / 8f; y++) {
                            if (!IsEmpty(block.tileMap[x, y]) && !block.Scene.CollideCheck<Solid>(new Rectangle((int) block.X + x * 8, (int) block.Y + y * 8, 8, 8))) {
                                block.Scene.Add(Engine.Pooler.Create<Debris>().Init(block.Position + new Vector2(4 + x * 8, 4 + y * 8), block.tileMap[x, y], true).BlastFrom(block.TopCenter));
                            }
                        }
                    }
                    DynData<CrumbleWallOnRumble> blockData = new DynData<CrumbleWallOnRumble>(block);
                    if (blockData.Get<bool>("permanent")) {
                        block.SceneAs<Level>().Session.DoNotLoad.Add(blockData.Get<EntityID>("id"));
                    }
                    block.RemoveSelf();
                }
            } else
                orig(self);
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
            Level level = Scene as Level;
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
