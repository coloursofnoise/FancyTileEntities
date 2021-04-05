using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.FancyTileEntities {
    public class FancyTileEntitiesModule : EverestModule {

        private static ILHook hook_FallingBlock_Sequence;

        public override void Load() {
            //IL.Celeste.LightingRenderer.DrawLightOccluders += LightingRenderer_DrawLightOccluders;

            Extensions.Load();
            TileSeedController.Load();

            On.Celeste.CrumbleWallOnRumble.Break += FancyCrumbleWallOnRumble.Break;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool += FancyDashBlock.Break;
            On.Celeste.FallingBlock.LandParticles += FancyFallingBlock.LandParticles;
            hook_FallingBlock_Sequence = new ILHook(
                typeof(FallingBlock).GetMethod("Sequence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetStateMachineTarget(),
                FancyFallingBlock.Sequence
            );
            On.Celeste.FinalBossMovingBlock.StopParticles += FancyFinalBossMovingBlock.StopParticles;
            On.Celeste.FinalBossMovingBlock.ImpactParticles += FancyFinalBossMovingBlock.ImpactParticles;
            On.Celeste.FinalBossMovingBlock.Finish += FancyFinalBossMovingBlock.Finish; 
            On.Celeste.FloatySpaceBlock.Awake += FancyFloatySpaceBlock.Awake;
            On.Celeste.RidgeGate.EnterSequence += FancyRidgeGate.EnterSequence;
        }

        public override void Unload() {
            //IL.Celeste.LightingRenderer.DrawLightOccluders -= LightingRenderer_DrawLightOccluders;

            Extensions.Unload();
            TileSeedController.Unload();

            On.Celeste.CrumbleWallOnRumble.Break -= FancyCrumbleWallOnRumble.Break;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool -= FancyDashBlock.Break;
            On.Celeste.FallingBlock.LandParticles -= FancyFallingBlock.LandParticles;
            hook_FallingBlock_Sequence?.Dispose();
            On.Celeste.FinalBossMovingBlock.StopParticles -= FancyFinalBossMovingBlock.StopParticles;
            On.Celeste.FinalBossMovingBlock.ImpactParticles -= FancyFinalBossMovingBlock.ImpactParticles;
            On.Celeste.FinalBossMovingBlock.Finish -= FancyFinalBossMovingBlock.Finish;
            On.Celeste.FloatySpaceBlock.Awake -= FancyFloatySpaceBlock.Awake;
        }

        private void LightingRenderer_DrawLightOccluders(ILContext il) {
            MethodReference m_get_Tracker = il.Import(typeof(Scene).GetProperty("Tracker").GetGetMethod());
            MethodReference m_GetComponents = il.Import(typeof(Tracker).GetMethod("GetComponents").MakeGenericMethod(typeof(LightOccludeList)));
            MethodReference f_get_Tracker = il.Import(typeof(Tracker).GetProperty("Tracker").GetGetMethod());

            ILCursor c = new ILCursor(il);

            c.GotoNext(i => i.OpCode == OpCodes.Callvirt &&
                ((MethodReference) i.Operand).Name == "GetComponents")
                .GotoNext(MoveType.After);

            //c.Emit(OpCodes.Ldloc_1);

        }

    }
}
