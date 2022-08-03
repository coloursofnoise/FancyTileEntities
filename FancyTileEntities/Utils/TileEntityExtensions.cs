using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.FancyTileEntities {
    public static partial class Extensions {

        #region SmoothMovement

        private static readonly FieldInfo<HashSet<Actor>> f_Solid_riders = typeof(Solid).GetField<HashSet<Actor>>("riders", BindingFlags.NonPublic | BindingFlags.Static);

        public static void MoveHExactSmooth(this Solid solid, int move) {
            solid.GetRiders();
            float right = solid.Right;
            float left = solid.Left;
            Player player = solid.Scene.Tracker.GetEntity<Player>();
            HashSet<Actor> riders = f_Solid_riders.GetValue(null);
            if (player != null && Input.MoveX.Value == Math.Sign(move) && Math.Sign(player.Speed.X) == Math.Sign(move) && !riders.Contains(player) && solid.CollideCheck(player, solid.Position + Vector2.UnitX * move - Vector2.UnitY)) {
                player.MoveV(1f);
            }

            solid.X += move;
            solid.MoveStaticMovers(Vector2.UnitX * move);

            if (solid.Collidable) {
                foreach (Actor actor in solid.Scene.Tracker.GetEntities<Actor>()) {
                    if (actor.AllowPushing) {
                        bool collidable = actor.Collidable;
                        actor.Collidable = true;
                        if (!actor.TreatNaive && solid.CollideCheck(actor, solid.Position)) {
                            solid.Collidable = false;
                            actor.MoveHExact(move, actor.SquishCallback, solid);
                            actor.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        } else if (riders.Contains(actor)) {
                            solid.Collidable = false;
                            if (actor.TreatNaive) {
                                actor.NaiveMove(Vector2.UnitX * move);
                            } else {
                                actor.MoveHExact(move);
                            }
                            actor.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        }
                        actor.Collidable = collidable;
                    }
                }
            }

            riders.Clear();
        }

        public static void MoveVExactSmooth(this Solid solid, int move) {
            ModInterop.GravityHelper.BeginOverride?.Invoke();
            solid.GetRiders();
            float bottom = solid.Bottom;
            float top = solid.Top;
            HashSet<Actor> riders = f_Solid_riders.GetValue(null);

            solid.Y += move;
            solid.MoveStaticMovers(Vector2.UnitY * move);

            if (solid.Collidable) {
                foreach (Actor actor in solid.Scene.Tracker.GetEntities<Actor>()) {
                    if (actor.AllowPushing) {
                        bool collidable = actor.Collidable;
                        actor.Collidable = true;
                        if (!actor.TreatNaive && solid.CollideCheck(actor, solid.Position)) {
                            solid.Collidable = false;
                            actor.MoveVExact(move, actor.SquishCallback, solid);
                            actor.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        } else if (riders.Contains(actor)) {
                            solid.Collidable = false;
                            if (actor.TreatNaive) {
                                actor.NaiveMove(Vector2.UnitY * move);
                            } else {
                                actor.MoveVExact(move);
                            }
                            actor.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        }
                        actor.Collidable = collidable;
                    }
                }
            }

            riders.Clear();
            ModInterop.GravityHelper.EndOverride?.Invoke();
        }

        #endregion

        #region SoundIndex

        public static int GetLandSoundIndex(this Solid solid, Entity entity, VirtualMap<char> tileMap) {
            float y = ModInterop.GravityHelper.IsActorInverted?.Invoke() == true ? entity.Top : entity.Bottom;

            int idx = solid.SurfaceSoundIndexAt(new Vector2(entity.CenterX, y) + Vector2.UnitY * 4f, tileMap);
            if (idx == -1) {
                idx = solid.SurfaceSoundIndexAt(new Vector2(entity.Left, y) + Vector2.UnitY * 4f, tileMap);
            }
            if (idx == -1) {
                idx = solid.SurfaceSoundIndexAt(new Vector2(entity.Right, y) + Vector2.UnitY * 4f, tileMap);
            }
            return idx;
        }

        public static int GetWallSoundIndex(this Solid solid, Player player, int side, VirtualMap<char> tileMap) {
            int idx = solid.SurfaceSoundIndexAt(player.Center + Vector2.UnitX * side * 8f, tileMap);
            if (idx < 0) {
                idx = solid.SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, -6f), tileMap);
            }
            if (idx < 0) {
                idx = solid.SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, 6f), tileMap);
            }
            return idx;
        }

        public static int GetStepSoundIndex(this Solid solid, Entity entity, VirtualMap<char> tileMap) {
            float y = ModInterop.GravityHelper.IsActorInverted?.Invoke() == true ? entity.Top : entity.Bottom;

            int idx = solid.SurfaceSoundIndexAt(new Vector2(entity.CenterX, y) + Vector2.UnitY * 4f, tileMap);
            if (idx == -1) {
                idx = solid.SurfaceSoundIndexAt(new Vector2(entity.Left, y) + Vector2.UnitY * 4f, tileMap);
            }
            if (idx == -1) {
                idx = solid.SurfaceSoundIndexAt(new Vector2(entity.Right, y) + Vector2.UnitY * 4f, tileMap);
            }
            return idx;
        }

        public static int SurfaceSoundIndexAt(this Solid solid, Vector2 readPosition, VirtualMap<char> tileMap) {
            int x = (int) ((readPosition.X - solid.X) / 8f);
            int y = (int) ((readPosition.Y - solid.Y) / 8f);
            if (x >= 0 && y >= 0 && x < tileMap.Columns && y < tileMap.Rows) {
                char c = tileMap[x, y];
                if (c == 'k') {
                    return solid.CoreTileSurfaceIndex();
                }
                if (c != '0' && SurfaceIndex.TileToIndex.TryGetValue(c, out int index)) {
                    return index;
                }
            }
            return -1;
        }

        public static int CoreTileSurfaceIndex(this Entity entity) {
            return (entity.Scene as Level).CoreMode switch {
                Session.CoreModes.Hot => SurfaceIndex.CoreMoltenRock,
                Session.CoreModes.Cold => SurfaceIndex.CoreIce,
                _ => SurfaceIndex.Dirt,
            };
        }

        #endregion

    }
}
