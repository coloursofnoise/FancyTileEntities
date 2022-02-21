using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyFallingBlock")]
    [TrackedAs(typeof(FallingBlock))]
    public class FancyFallingBlock : FallingBlock {
        private static readonly FieldInfo f_Sequence_this = typeof(FallingBlock).GetNestedType("<Sequence>d__21", BindingFlags.NonPublic).GetField("<>4__this", BindingFlags.Public | BindingFlags.Instance);

        private bool manualTrigger;

        private DynData<FallingBlock> baseData;

        private VirtualMap<char> tileMap;
        private BGTileEntity bgTiles;
        private AnimatedTiles animatedTiles;
        private LightOcclude badLightOcclude;

        internal static void Sequence(ILContext il) {
            FieldReference fieldRef = il.Import(f_Sequence_this);

            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt(typeof(Platform), "StopShaking"));
            Instruction next = c.Next;

            Instruction after = new ILCursor(c).GotoNext(instr => instr.Match(OpCodes.Ldarg_0) && instr.Next.OpCode == OpCodes.Ldc_R4 && ((float) instr.Next.Operand) == 0f).Next;

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, fieldRef);
            c.Emit(OpCodes.Isinst, typeof(FancyFallingBlock));
            c.Emit(OpCodes.Brfalse_S, next);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, fieldRef);
            c.Emit(OpCodes.Call, typeof(FancyFallingBlock).GetMethod(nameof(FallParticles), BindingFlags.NonPublic | BindingFlags.Instance));
            c.Emit(OpCodes.Br, after);
        }

        internal static bool FallingBlock_PlayerFallCheck(On.Celeste.FallingBlock.orig_PlayerFallCheck orig, FallingBlock self) =>
            ((self as FancyFallingBlock)?.manualTrigger == true) ? false : orig(self);


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

            bgTiles = new BGTileEntity(Position, data.Attr("tileDataBG", ""), newSeed);

            if (data.Bool("finalBoss", false)) {
                VirtualMap<char> tileMapHighlighted = GenerateTileMap(data.Attr("tileDataHighlight", ""));
                Calc.PushRandom(newSeed);
                TileGrid highlight = GFX.FGAutotiler.GenerateMap(tileMapHighlighted, default(Autotiler.Behaviour)).TileGrid;
                highlight.Alpha = 0f;
                baseData["highlight"] = highlight;
                Add(baseData.Get<TileGrid>("highlight"));
                Calc.PopRandom();
            }

            ColliderList colliders = GenerateBetterColliderGrid(tileMap, 8, 8);
            AddLightOcclude(this, colliders);
            Collider = colliders;
            Add(new TileInterceptor(baseData.Get<TileGrid>("tiles"), false));

            manualTrigger = data.Bool("manualTrigger");
            if (manualTrigger)
                Add(new EntityTriggerListener(() => Triggered = true, () => Triggered = true));
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(badLightOcclude);
            scene.Add(bgTiles);
        }

        public override void Update() {
            base.Update();
            bgTiles.Position = Position;
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

        public override void MoveVExact(int move) => this.MoveVExactSmooth(move);

        public override int GetLandSoundIndex(Entity entity) => this.GetLandSoundIndex(entity, tileMap);
        public override int GetWallSoundIndex(Player player, int side) => this.GetWallSoundIndex(player, side, tileMap);
        public override int GetStepSoundIndex(Entity entity) => this.GetStepSoundIndex(entity, tileMap);

    }
}
