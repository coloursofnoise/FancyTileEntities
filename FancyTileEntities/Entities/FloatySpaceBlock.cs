using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyFloatySpaceBlock")]
    [TrackedAs(typeof(FloatySpaceBlock))]
    public class FancyFloatySpaceBlock : FloatySpaceBlock {
        private static readonly MethodInfo m_AddToGroupAndFindChildren;
        private static readonly MethodInfo m_TryToInitPosition;
        private static readonly FieldInfo<char> f_tileType;
        private static readonly FieldInfo<HashSet<Actor>> f_Solid_riders;

        private VirtualMap<char> tileMap;
        private LightOcclude badLightOcclude;

        static FancyFloatySpaceBlock() {
            m_AddToGroupAndFindChildren = typeof(FloatySpaceBlock).GetMethod("AddToGroupAndFindChildren", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            m_TryToInitPosition = typeof(FloatySpaceBlock).GetMethod("TryToInitPosition", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            f_tileType = typeof(FloatySpaceBlock).GetField<char>("tileType", BindingFlags.NonPublic | BindingFlags.Instance);
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

        internal static void Awake(On.Celeste.FloatySpaceBlock.orig_Awake orig, FloatySpaceBlock self, Scene scene) {
            IntPtr ptr = typeof(Solid).GetMethod("Awake").MethodHandle.GetFunctionPointer();
            Action<Scene> awake_Solid = (Action<Scene>) Activator.CreateInstance(typeof(Action<Scene>), self, ptr);
            awake_Solid(scene);

            DynData<FloatySpaceBlock> baseData = new DynData<FloatySpaceBlock>(self);

            baseData["awake"] = true;
            if (!self.HasGroup) {
                baseData["MasterOfGroup"] = true;
                self.Moves = new Dictionary<Platform, Vector2>();
                self.Group = new List<FloatySpaceBlock>();
                self.Jumpthrus = new List<JumpThru>();
                self.GroupBoundsMin = new Point((int) self.X, (int) self.Y);
                self.GroupBoundsMax = new Point((int) self.Right, (int) self.Bottom);
                m_AddToGroupAndFindChildren.Invoke(self, new object[] { self });

                Rectangle rectangle = new Rectangle(self.GroupBoundsMin.X / 8, self.GroupBoundsMin.Y / 8, (self.GroupBoundsMax.X - self.GroupBoundsMin.X) / 8 + 1, (self.GroupBoundsMax.Y - self.GroupBoundsMin.Y) / 8 + 1);
                VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
                foreach (FloatySpaceBlock floatySpaceBlock in self.Group) {
                    if (floatySpaceBlock is FancyFloatySpaceBlock) {
                        Point offset = new Point((int) (floatySpaceBlock.X / 8f) - rectangle.X, (int) (floatySpaceBlock.Y / 8f) - rectangle.Y);
                        (floatySpaceBlock as FancyFloatySpaceBlock).tileMap.CopyInto(virtualMap, offset);
                    } else {
                        char tileType = f_tileType.GetValue(floatySpaceBlock);

                        int xOffset = (int) (floatySpaceBlock.X / 8f) - rectangle.X;
                        int yOffset = (int) (floatySpaceBlock.Y / 8f) - rectangle.Y;
                        int w = (int) (floatySpaceBlock.Width / 8f);
                        int h = (int) (floatySpaceBlock.Height / 8f);
                        for (int x = xOffset; x < xOffset + w; x++) {
                            for (int y = yOffset; y < yOffset + h; y++) {
                                virtualMap[x, y] = tileType;
                            }
                        }
                    }
                }

                TileGrid tiles = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour {
                    EdgesExtend = false,
                    EdgesIgnoreOutOfLevel = false,
                    PaddingIgnoreOutOfLevel = false
                }).TileGrid;

                tiles.Position = new Vector2(self.GroupBoundsMin.X - self.X, self.GroupBoundsMin.Y - self.Y);
                baseData["tiles"] = tiles;
                self.Add(baseData.Get<TileGrid>("tiles"));
            }
            m_TryToInitPosition.Invoke(self, default);
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
