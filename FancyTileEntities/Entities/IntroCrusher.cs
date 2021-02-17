using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyIntroCrusher")]
    class FancyIntroCrusher : IntroCrusher {
        private static readonly FieldInfo<HashSet<Actor>> f_Solid_riders;

        private DynData<IntroCrusher> baseData;
        private VirtualMap<char> tileMap;

        static FancyIntroCrusher() {
            f_Solid_riders = typeof(Solid).GetField<HashSet<Actor>>("riders", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public FancyIntroCrusher(EntityData data, Vector2 offset)
            : base(data, offset) {
            baseData = new DynData<IntroCrusher>(this);
            Remove(baseData.Get<TileGrid>("tilegrid"));

            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Collider = GenerateBetterColliderGrid(tileMap, 8, 8);

            baseData["tilegrid"] = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid;
            Add(baseData.Get<TileGrid>("tilegrid"));
        }

        #region SmoothMovement

        public override void MoveHExact(int move) {
            GetRiders();
            float right = Right;
            float left = Left;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Input.MoveX.Value == Math.Sign(move) && Math.Sign(player.Speed.X) == Math.Sign(move) && !f_Solid_riders.GetValue(null).Contains(player) && CollideCheck(player, Position + Vector2.UnitX * move - Vector2.UnitY)) {
                player.MoveV(1f, null, null);
            }
            X += move;
            MoveStaticMovers(Vector2.UnitX * move);
            if (Collidable) {
                foreach (Entity entity in Scene.Tracker.GetEntities<Actor>()) {
                    Actor actor = (Actor) entity;
                    if (actor.AllowPushing) {
                        bool collidable = actor.Collidable;
                        actor.Collidable = true;
                        if (!actor.TreatNaive && CollideCheck(actor, Position)) {
                            Collidable = false;
                            actor.MoveHExact(move, actor.SquishCallback, this);
                            actor.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        } else if (f_Solid_riders.GetValue(null).Contains(actor)) {
                            Collidable = false;
                            if (actor.TreatNaive) {
                                actor.NaiveMove(Vector2.UnitX * move);
                            } else {
                                actor.MoveHExact(move, null, null);
                            }
                            actor.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                        actor.Collidable = collidable;
                    }
                }
            }
            f_Solid_riders.GetValue(null).Clear();
        }

        public override void MoveVExact(int move) {
            GetRiders();
            float bottom = Bottom;
            float top = Top;
            Y += move;
            MoveStaticMovers(Vector2.UnitY * move);
            if (Collidable) {
                foreach (Entity entity in Scene.Tracker.GetEntities<Actor>()) {
                    Actor actor = (Actor) entity;
                    if (actor.AllowPushing) {
                        bool collidable = actor.Collidable;
                        actor.Collidable = true;
                        if (!actor.TreatNaive && CollideCheck(actor, Position)) {
                            Collidable = false;
                            actor.MoveVExact(move, actor.SquishCallback, this);
                            actor.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        } else if (f_Solid_riders.GetValue(null).Contains(actor)) {
                            Collidable = false;
                            if (actor.TreatNaive) {
                                actor.NaiveMove(Vector2.UnitY * move);
                            } else {
                                actor.MoveVExact(move, null, null);
                            }
                            actor.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                        actor.Collidable = collidable;
                    }
                }
            }
            f_Solid_riders.GetValue(null).Clear();
        }

        #endregion

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
