using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Linq;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyFloatySpaceBlock")]
    [TrackedAs(typeof(FloatySpaceBlock))]
    public class FancyFloatySpaceBlock : FloatySpaceBlock {

        private VirtualMap<char> tileMap;
        private LightOcclude badLightOcclude;

        private int seed;

        public FancyFloatySpaceBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Char("connectsTo", '3'), data.Bool("disableSpawnOffset", false)) {
            seed = data.Int("randomSeed");
            badLightOcclude = Get<LightOcclude>();
            Calc.PushRandom(seed != 0 ? seed : Calc.Random.Next());
            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Calc.PopRandom();

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

                if (block is FancyFloatySpaceBlock fancy && fancy.seed != 0 && block.Group.Count == 1) {
                    Calc.PushRandom(fancy.seed);
                }
            });
            cursor.Emit(OpCodes.Ldarg_0);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Autotiler>("GenerateMap"));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<FloatySpaceBlock>>(block => {
                if (block is FancyFloatySpaceBlock fancy && fancy.seed != 0 && block.Group.Count == 1) {
                    Calc.PopRandom();
                }
            });
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(badLightOcclude);
        }

        public override void MoveHExact(int move) => this.MoveHExactSmooth(move);
        public override void MoveVExact(int move) => this.MoveVExactSmooth(move);

    }
}
