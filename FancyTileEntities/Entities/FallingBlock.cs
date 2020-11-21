using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyFallingBlock")]
    [TrackedAs(typeof(FallingBlock))]
    public class FancyFallingBlock : FallingBlock {
        private static readonly FieldInfo<HashSet<Actor>> f_Solid_solidRiders;
        private static readonly FieldInfo f_Sequence_this;

        private DynData<FallingBlock> baseData;

        private VirtualMap<char> tileMap;
        private AnimatedTiles animatedTiles;
        private LightOcclude badLightOcclude;

        static FancyFallingBlock() {
            f_Solid_solidRiders = typeof(Solid).GetField<HashSet<Actor>>("riders", BindingFlags.NonPublic | BindingFlags.Static);

            // For whatever reason you can't get this from the ILContext object
            f_Sequence_this = typeof(FallingBlock).GetNestedType("<Sequence>d__21", BindingFlags.NonPublic).GetField("<>4__this", BindingFlags.Public | BindingFlags.Instance);
        }

        internal static void Sequence(ILContext il) {
            FieldReference fieldRef = il.Import(f_Sequence_this);
            TypeDefinition typeRef = ModuleDefinition.ReadModule(typeof(FancyFallingBlock).Assembly.Location).GetType("Celeste.Mod.FancyTileEntities.FancyFallingBlock");

            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt(typeof(Platform), "StopShaking"));
            Instruction next = c.Next;

            Instruction after = new ILCursor(c).GotoNext(instr => instr.Match(OpCodes.Ldarg_0) && instr.Next.OpCode == OpCodes.Ldc_R4 && ((float) instr.Next.Operand) == 0f).Next;

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, fieldRef);
            c.Emit(OpCodes.Isinst, typeRef);
            c.Emit(OpCodes.Brfalse_S, next);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, fieldRef);
            c.Emit(OpCodes.Call, typeRef.FindMethod("System.Void FallParticles()"));
            c.Emit(OpCodes.Br, after);
        }

        public FancyFallingBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, '3', data.Width, data.Height, data.Bool("finalBoss", false), data.Bool("behind", false), data.Bool("climbFall", true)) {
            baseData = new DynData<FallingBlock>(this);
            Remove(baseData.Get<TileGrid>("tiles"));
            Remove(Get<TileInterceptor>());
            badLightOcclude = Get<LightOcclude>();

            int newSeed = Calc.Random.Next();

            Calc.PushRandom(newSeed);
            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Autotiler.Generated generated = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour));
            baseData["tiles"] = generated.TileGrid;
            Add(baseData.Get<TileGrid>("tiles"));
            Add(animatedTiles = generated.SpriteOverlay);
            Calc.PopRandom();

            if (data.Bool("finalBoss", false)) {
                VirtualMap<char> tileMapHighlighted = GenerateTileMap(data.Attr("tileDataHighlight", ""));
                Calc.PushRandom(newSeed);
                TileGrid highlight = GFX.FGAutotiler.GenerateMap(tileMapHighlighted, default(Autotiler.Behaviour)).TileGrid;
                highlight.Alpha = 0f;
                baseData["highlight"] = highlight;
                Add(baseData.Get<TileGrid>("highlight"));
                Calc.PopRandom();
            }

            ColliderList colliders = new ColliderList();
            for (int x = 0; x < tileMap.Columns; x++) {
                for (int y = 0; y < tileMap.Rows; y++) {
                    if (tileMap.AnyInSegmentAtTile(x, y) && tileMap[x, y] != '0') {
                        colliders.Add(new Hitbox(8, 8, x * 8, y * 8));
                        Add(new LightOcclude(new Rectangle(x * 8, y * 8, 8, 8)));
                    }
                }
            }
            Collider = colliders;
            Add(new TileInterceptor(baseData.Get<TileGrid>("tiles"), false));
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(badLightOcclude);
        }

        public override void OnShake(Vector2 amount) {
            base.OnShake(amount);
            animatedTiles.Position += amount;
        }

        private void FallParticles() {
            for (int x = 2; x <= Width; x += 4) {
                //Move top to bottom
                for (int y = 2; y <= Height; y += 4) {
                    if (CollidePoint(new Vector2(X + x, Y + y))) {
                        if (Scene.CollideCheck<Solid>(new Vector2(X + x, Y + y - 3))) {
                            SceneAs<Level>().Particles.Emit(P_FallDustA, 2, new Vector2(X + x, Y + y), Vector2.One * 4f, (float) Math.PI / 2f);
                        }
                        SceneAs<Level>().Particles.Emit(P_FallDustB, 2, new Vector2(X + x, Y + y), Vector2.One * 4f);
                        break;
                    }
                }
            }
        }

        internal static void LandParticles(On.Celeste.FallingBlock.orig_LandParticles orig, FallingBlock self) {
            if (self is FancyFallingBlock) {
                for (int x = 2; x <= self.Width; x += 4) {
                    //Move bottom to top
                    for (int y = (int) self.Height - 2; y >= 0; y -= 4) {
                        if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                            if (self.Scene.CollideCheck<Solid>(new Vector2(self.X + x, self.Y + y + 3))) {
                                self.SceneAs<Level>().ParticlesFG.Emit(P_FallDustA, 1, new Vector2(self.X + x, self.Y + y), Vector2.One * 4f, -(float) Math.PI / 2f);
                                float direction = (!(x < self.Width / 2f)) ? 0f : ((float) Math.PI);
                                self.SceneAs<Level>().ParticlesFG.Emit(P_LandDust, 1, new Vector2(self.X + x, self.Y + y), Vector2.One * 4f, direction);
                            }
                            break;
                        }
                    }
                }
            } else
                orig(self);
        }

        #region SmoothVMovement

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
                        } else if (f_Solid_solidRiders.GetValue(null).Contains(actor)) {
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
            f_Solid_solidRiders.GetValue(null).Clear();
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
