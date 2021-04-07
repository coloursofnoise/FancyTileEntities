using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyFloatySpaceBlock")]
    [TrackedAs(typeof(FloatySpaceBlock))]
    public class FancyFloatySpaceBlock : FloatySpaceBlock {
        private static readonly FieldInfo<HashSet<Actor>> f_Solid_riders;

        private VirtualMap<char> tileMap;
        private LightOcclude badLightOcclude;

        static FancyFloatySpaceBlock() {
            f_Solid_riders = typeof(Solid).GetField<HashSet<Actor>>("riders", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public FancyFloatySpaceBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Char("connectsTo", '3'), data.Bool("disableSpawnOffset", false)) {
            badLightOcclude = Get<LightOcclude>();

            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            ColliderList colliders = GenerateBetterColliderGrid(tileMap, 8, 8);
            Collider = colliders;
            AddLightOcclude(this, colliders);
        }

        internal static void Awake(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, instr => instr.Match(OpCodes.Endfinally), instr => instr.Match(OpCodes.Ldarg_0));
            // Can't use `cursor.MoveAfterLabels` because ????
            cursor.Emit(OpCodes.Ldloc_S, il.Body.Variables.First(v => v.VariableType.Name == "VirtualMap`1"));
            cursor.EmitDelegate<Action<FloatySpaceBlock, VirtualMap<char>>>((block, map) => {
                Rectangle rect = new Rectangle(block.GroupBoundsMin.X / 8, block.GroupBoundsMin.Y / 8,
                    (block.GroupBoundsMax.X - block.GroupBoundsMin.X) / 8 + 1, (block.GroupBoundsMax.Y - block.GroupBoundsMin.Y) / 8 + 1);
                foreach (FloatySpaceBlock child in block.Group) {
                    if (child is FancyFloatySpaceBlock fancyBlock) {
                        Point offset = new Point((int) (fancyBlock.X / 8f) - rect.X, (int) (fancyBlock.Y / 8f) - rect.Y);
                        fancyBlock.tileMap.CopyInto(map, offset);
                    }
                }
            });
            cursor.Emit(OpCodes.Ldarg_0);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(badLightOcclude);
        }

        #region SmoothMovement

        public override void MoveHExact(int move) {
            GetRiders();
            float right = Right;
            float left = Left;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && Input.MoveX.Value == Math.Sign(move) && Math.Sign(entity.Speed.X) == Math.Sign(move) && !f_Solid_riders.GetValue(null).Contains(entity) && CollideCheck(entity, Position + Vector2.UnitX * move - Vector2.UnitY)) {
                entity.MoveV(1f, null, null);
            }
            X += move;
            MoveStaticMovers(Vector2.UnitX * move);
            if (Collidable) {
                foreach (Entity entity2 in Scene.Tracker.GetEntities<Actor>()) {
                    Actor actor = (Actor) entity2;
                    if (actor.AllowPushing) {
                        bool collidable = actor.Collidable;
                        actor.Collidable = true;
                        if (!actor.TreatNaive && CollideCheck(actor, Position)) {
                            do {
                                Collidable = false;
                                actor.MoveHExact(move, actor.SquishCallback, this);
                                actor.LiftSpeed = LiftSpeed;
                                Collidable = true;
                            } while (CollideCheck(actor, Position));
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
                            do {
                                Collidable = false;
                                actor.MoveVExact(move, actor.SquishCallback, this);
                                actor.LiftSpeed = LiftSpeed;
                                Collidable = true;
                            } while (CollideCheck(actor, Position));
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
