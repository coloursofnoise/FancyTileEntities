using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
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

        [MonoModLinkTo("Celeste.Solid", "System.Void Awake(Monocle.Scene)")]
        public void base_Awake(Scene scene) {
            base.Awake(scene);
        }

        public override void Awake(Scene scene) {
            base_Awake(scene);

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
                    Audio.Play(SFX.game_10_quake_rockbreak, block.Position);
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

        public override int GetLandSoundIndex(Entity entity) => this.GetLandSoundIndex(entity, tileMap);
        public override int GetWallSoundIndex(Player player, int side) => this.GetWallSoundIndex(player, side, tileMap);
        public override int GetStepSoundIndex(Entity entity) => this.GetStepSoundIndex(entity, tileMap);

    }
}
